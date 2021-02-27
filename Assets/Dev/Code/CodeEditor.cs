using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodeEditor : MonoBehaviour {
  public TMP_InputField edit;
  public Scrollbar Scroll;
  public TextMeshProUGUI EditText;
  public TextMeshProUGUI LineNumbers;
  public RectTransform LineNumbersRT;
  public TextMeshProUGUI Result;
  public TextMeshProUGUI CurrentLineText;
  public TMP_FontAsset MonoFont;
  readonly List<Comment> comments = new List<Comment>();
  readonly List<string> undos = new List<string> { "" };
  int undopos = 0;

  private void Start() {
    MonoFont.tabSize = 2;
    edit.resetOnDeActivation = false;
    edit.onFocusSelectAll = false;
    edit.restoreOriginalTextOnEscape = false;

    UpdateLinePos();
    EventSystem.current.SetSelectedGameObject(edit.gameObject);
    VariableEditVal.onEndEdit.AddListener(EditVariableValue);
    CompilationStatus.text = "<color=#80feef><i>No code</i></color>";
  }

  float delay = 1;
  int prevSize = 0;
  private void Update() {
    CurrentLineText.text = curline + "/" + numlines; // FIXME show it somewhere
    bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) {
      SetUndo();
      UpdateLinePos();
      CodeChange();
    }
    if (Input.GetKeyDown(KeyCode.RightCurlyBracket) || (Input.GetKeyDown(KeyCode.RightBracket) && shift)) {
      SetUndo();
      ParseBlock();
      CodeChange();
    }

    if (Input.GetKeyUp(KeyCode.Z) && ctrl) { Undo(); CodeChange(); }
    if (Input.GetKeyUp(KeyCode.Y) && ctrl) { Redo(); CodeChange(); }


    if ((Input.GetKeyUp(KeyCode.X) || Input.GetKeyUp(KeyCode.C)) && ctrl) {
      GUIUtility.systemCopyBuffer = rgSyntaxHighlight.Replace(GUIUtility.systemCopyBuffer, ""); // Remove the color coding
    }

    if (Input.GetKeyUp(KeyCode.F) && ctrl) {
      if (FindReplace.activeSelf) Find();
      else {
        FindReplace.SetActive(true);
        FindMsg.text = "";
        EventSystem.current.SetSelectedGameObject(TextToFind.gameObject);
      }
    }
    if (Input.GetKeyUp(KeyCode.Escape) && FindReplace.activeSelf) {
      FindReplace.SetActive(false);
      EventSystem.current.SetSelectedGameObject(edit.gameObject);
    }

    // FIXME ctrl+d
    // FIXME ctrl+del
    if (Input.GetKeyDown(KeyCode.D) && ctrl) DuplicateDelete(false);
    if (Input.GetKeyDown(KeyCode.K) && ctrl && shift) DuplicateDelete(true);

    if (Input.GetMouseButtonDown(0) && Input.mousePosition.x < 80 && overLine > 0 && overLine <= numlines) {
      if (breakPoints.Contains(overLine)) breakPoints.Remove(overLine);
      else breakPoints.Add(overLine);
      RedrawLineNumbersAndBreakPoints(numlines, 0);
      if (lastEdit <= lastDeploy && arcade.runStatus != Arcade.RunStatus.Stopped && arcade.runStatus != Arcade.RunStatus.Error) {
        arcade.SetBreakpoints(breakPoints);
      }
    }

    delay -= Time.deltaTime;
    if (delay < 0 && edit.text.Length != prevSize) {
      delay = 1;
      prevSize = edit.text.Length;
      FixFormatting();
    }
  }

  void SetUndo() {
    string value = rgSyntaxHighlight.Replace(edit.text, "").Trim();
    if (undos[undos.Count - 1] == value) return;
    undos.Add(value);
    undopos = undos.Count - 1;
  }
  void Undo() {
    undopos--;
    if (undopos < 0) undopos = 0;
    edit.SetTextWithoutNotify(undos[undopos]);
  }
  void Redo() {
    undopos++;
    if (undopos >= undos.Count) undopos = undos.Count - 1;
    edit.SetTextWithoutNotify(undos[undopos]);
  }

  readonly HashSet<int> breakPoints = new HashSet<int>();

  public GameObject[] LineBackgroundTemplate;
  public Transform Background;

  void UpdateLinePos() {
    int num = 1;
    int pos = 0;
    edit.SetTextWithoutNotify(edit.text.Replace("\r\n", "\n").Replace("\r", "\n"));
    foreach (char c in edit.text) {
      pos++;
      if (c == '\n') num++;
      if (pos == edit.stringPosition) curline = num;
    }

    if (numlines != num) {
      RedrawLineNumbersAndBreakPoints(num, 0);

      // Update the background lines
      Background.SetAsFirstSibling();
      if (Background.childCount > num) {
        for (int i = num; i < Background.childCount; i++) {
          GameObject line = Background.GetChild(Background.childCount - 1).gameObject;
          line.transform.SetParent(null);
          GameObject.Destroy(line);
        }
      }
      while (Background.childCount < num) {
        BackgroundLine line = Instantiate(LineBackgroundTemplate[Background.childCount & 1], Background).GetComponent<BackgroundLine>();
        line.lineNumber = Background.childCount;
        line.CallBack = OverLine;
      }
    }
    numlines = num;
  }
  void SetLinePos() {
    int num = 1;
    int pos = 0;
    edit.SetTextWithoutNotify(edit.text.Replace("\r\n", "\n").Replace("\r", "\n"));
    foreach (char c in edit.text) {
      pos++;
      if (c == '\n') {
        num++;
        if (num == curline + 1) {
          edit.stringPosition = pos - 1;
          return;
        }
      }
    }
    edit.stringPosition = int.MaxValue;
  }

  void DuplicateDelete(bool delete) {
    CodeChange();
    // Find the current line number
    curline = -1;
    int num = 1;
    int pos = 0;
    edit.SetTextWithoutNotify(edit.text.Replace("\r\n", "\n").Replace("\r", "\n"));
    string txt = edit.text;
    foreach (char c in txt) {
      pos++;
      if (c == '\n') num++;
      if (pos == edit.stringPosition) curline = num;
    }
    if (curline == -1) {
      curline = 0;
      return;
    }
    int posStart = txt.LastIndexOf('\n', edit.stringPosition - 1) + 1;
    int posEnd = txt.IndexOf('\n', edit.stringPosition);

    if (delete) { // true: Delete the whole line
      if (posStart == 0 && posEnd == -1) txt = "";
      else if (posStart == 0 && posEnd != -1) txt = txt.Substring(posEnd + 1);
      else if (posStart != 0 && posEnd == -1) { txt = txt.Substring(0, posStart); if (txt[txt.Length - 1] == '\n') txt = txt.Substring(0, txt.Length - 1); }
      else txt = txt.Substring(0, posStart) + txt.Substring(posEnd - 1);
      edit.SetTextWithoutNotify(txt);
    }
    else { // false: Copy the whole line
      if (posStart == 0 && posEnd == -1) txt += "\n" + txt;
      else if (posStart == 0 && posEnd != -1) txt = txt.Substring(0, posEnd + 1) + txt;
      else if (posStart != 0 && posEnd == -1) txt += (txt[txt.Length - 1] == '\n' ? "" : "\n") + txt.Substring(posStart);
      else txt = txt.Substring(0, posStart) + txt.Substring(posStart, posEnd - posStart + 1) + txt.Substring(posStart);
      edit.SetTextWithoutNotify(txt);
    }

    // Redraw the linenumbers
    RedrawLineNumbersAndBreakPoints(num);
  }

  private void RedrawLineNumbersAndBreakPoints(int num, int linenum = 0) {
    string nums = "";
    for (int i = 1; i <= num; i++) {
      if (breakPoints.Contains(i)) {
        char sp = linenum == i ? '4' : '1';
        if (i < 10) nums += "<nobr><sprite=" + sp + ">   " + i + "</nobr>\n";
        else if (i < 100) nums += "<nobr><sprite=" + sp + ">  " + i + "</nobr>\n";
        else if (i < 1000) nums += "<nobr><sprite=" + sp + "> " + i + "</nobr>\n";
        else nums += "<nobr><sprite=" + sp + ">" + i + "</nobr>\n";
      }
      else {
        if (linenum == i) {
          if (i < 10) nums += "<nobr><sprite=3>   " + i + "</nobr>\n";
          else if (i < 100) nums += "<nobr><sprite=3>  " + i + "</nobr>\n";
          else if (i < 1000) nums += "<nobr><sprite=3> " + i + "</nobr>\n";
          else nums += "<nobr><sprite=3>" + i + "</nobr>\n";
        }
        else
          nums += i + "\n";
      }
    }
    LineNumbers.text = nums.Substring(0, nums.Length - 1);
  }

  int overLine = 0;
  void OverLine(int num) {
    overLine = num;
  }

  void FixFormatting() {
    float s = Scroll.value;
    string text = edit.text;
    string tab = "";
    for (int i = 0; i < tabSize; i++) tab += " ";
    string clean = text.Replace("\r\n", "\n").Replace('\r', '\n');
    if (clean.Length == text.Length) return;
    edit.SetTextWithoutNotify(clean);
    Scroll.value = s;
    UpdateLinePos();
  }

  int numlines = 1;
  int curline = 1;

  int fontSize = 24;
  public TMP_Dropdown FontSizeDD;
  public void ChangeFontSize() {
    int.TryParse(FontSizeDD.options[FontSizeDD.value].text.Substring(11), out int newSize);
    if (newSize == 0) newSize = 28;
    edit.pointSize = newSize;
    LineNumbers.fontSize = EditText.fontSize;
    fontSize = newSize;

    foreach (Transform t in Background) {
      RectTransform rt = t.GetComponent<RectTransform>();
      rt.sizeDelta = new Vector2(1248, 1.1625f * fontSize);
    }

    // linenum RT width 78, lmargin -4
    //    text TMP left margin 80
    Debug.Log(EditText.margin);
    Vector2 lnsd = LineNumbersRT.sizeDelta;
    lnsd.x = (int)(3.44444444f * fontSize + 2.22222222222f);
    LineNumbersRT.sizeDelta = lnsd;
    EditText.margin = new Vector4((int)(3.333333333f * fontSize + 14.66666666666f), 0, 0, 0);
  }

  int tabSize = 2;
  public TMP_Dropdown TabSizeDD;
  public void ChangeTabSize() {
    int.TryParse(TabSizeDD.options[TabSizeDD.value].text.Substring(10), out int newSize);
    if (newSize == 0) newSize = 2;
    MonoFont.tabSize = (byte)newSize;
    edit.text = EditText.text;
    tabSize = newSize;
    EventSystem.current.SetSelectedGameObject(gameObject);
  }

  readonly CodeParser cp = new CodeParser();
  readonly Variables runVariables = new Variables();
  readonly Variables compVariables = new Variables();
  readonly Regex rgSyntaxHighlight = new Regex("(\\<color=#[0-9a-f]{6}\\>)|(\\</color\\>)|(\\<mark=#[0-9a-f]{8}\\>)|(\\</mark\\>)|(\\<b\\>)|(\\</b\\>)|(\\<i\\>)|(\\</i\\>)|(\\<color=red\\>)|", RegexOptions.IgnoreCase);
  readonly Regex rgCommentSL = new Regex("(//.*)$", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));
  readonly Regex rgCommentML = new Regex("/\\*(?:(?!\\*/)(?:.|[\r\n]+))*\\*/", RegexOptions.IgnoreCase | RegexOptions.Multiline, System.TimeSpan.FromSeconds(5));
  readonly Regex rgCommentMLs = new Regex("/\\*(?:(?!\\*/)(?:.|[\r\n]+))*", RegexOptions.IgnoreCase | RegexOptions.Multiline, System.TimeSpan.FromSeconds(5));
  readonly Regex rgCommentMLe = new Regex("(?:(?!\\*/)(?:.|[\r\n]+))*\\*/", RegexOptions.IgnoreCase | RegexOptions.Multiline, System.TimeSpan.FromSeconds(5));
  readonly Regex rgBlockOpen = new Regex("(?<!//.*?)\\{", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(5));
  readonly Regex rgBlockClose = new Regex("(?<!//.*?)\\}", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(5));
  readonly Regex rgString = new Regex("(\")([^\"]*)(\")", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));

  int blockCompile = 1; // 0 do not compile, 1 compile, 2 compile and optimize
  public TextMeshProUGUI BlockCompileText;
  readonly string[] blockCompileTxt = { "Don't Compile blocks", "Compile blocks", "Compile and Optimize" };
  public GameObject CompileContainer;

  public void BlockCompile() {
    blockCompile++;
    if (blockCompile >= 3) blockCompile = 0;
    BlockCompileText.text = blockCompileTxt[blockCompile];
  }

  public void Compile() {
    CompileContainer.SetActive(!CompileContainer.activeSelf);
  }


  public void CompileCode(bool optimize) {
    SetUndo();
    cp.SetOptimize(optimize);
    CompileContainer.SetActive(false);
    compiledCode = CompileCode(rgSyntaxHighlight.Replace(edit.text, "").Trim(), true);
    if (compiledCode == null) {
      codeHasErrors = true;
      UpdateCompilationStatus();
      return;
    }
    ParseBlock(compiledCode, 0, int.MaxValue);
    lastCompile = System.DateTime.Now;
    UpdateCompilationStatus();
  }

  void ParseBlock(CodeNode compiled, int start, int end) {
    // We need to find all nodes, one by one
    // For each grab the line number and get the Format (only if it is a command)
    // reconstruct the lines and update the input field
    string[] olines = edit.text.Split('\n');
    string code = rgSyntaxHighlight.Replace(edit.text, "").Trim();
    string[] clines = code.Split('\n');
    code = "";
    int indent = 0;
    int increaseone = 0;
    if (start < 0) start = 0;
    if (end > clines.Length) end = clines.Length;

    // For each line, be sure we have a corresponding LineData, add if required.
    while (comments.Count < clines.Length) comments.Add(new Comment());
    while (comments.Count > clines.Length) comments.RemoveRange(clines.Length, comments.Count - clines.Length);

    // If the line is not similar to the current line of code, update it (in all range)
    // If the line has to be parsed, replace the line value with the compiled one
    // Be sure to save the comments before stripping them out and parsing
    // When completed, recalculate indentation based on the LineData and reconstruct the final edit.text string

    for (int i = 0; i < start; i++) {
      code += olines[i] + "\n";
    }
    for (int i = start; i < end; i++) {
      CodeNode compiledLine = FindLine(compiled, i - start + 1);
      string line = clines[i].Trim(' ', '\t', '\r', '\n');
      comments[i].Zero();
      line = rgCommentSL.Replace(line, m => {
        comments[i].Set(m.Value, CodeNode.CommentType.SingleLine);
        return "";
      });
      line = rgCommentML.Replace(line, m => {
        comments[i].Set(m.Value, CodeNode.CommentType.MultiLineFull);
        return "";
      });
      line = rgCommentMLs.Replace(line, m => {
        comments[i].Set(m.Value, CodeNode.CommentType.MultiLineOpen);
        return "";
      });
      line = rgCommentMLe.Replace(line, m => {
        comments[i].Set(m.Value, CodeNode.CommentType.MultiLineClose);
        return "";
      });

      if (compiledLine == null) {
        if (rgBlockClose.IsMatch(line)) indent--;

        switch (comments[i].type) {
          case CodeNode.CommentType.MultiLineOpen:
            code += PrintLine(indent, line, false) + " <color=#70e688><mark=#30061880>" + comments[i].comment + (i < clines.Length - 1 ? "\n" : "");
            break;

          case CodeNode.CommentType.SingleLine:
          case CodeNode.CommentType.MultiLineFull:
            string cl = PrintLine(indent, line, false);
            code += cl + (string.IsNullOrEmpty(cl) ? "" : " ") + "<color=#70e688><mark=#30061880>" + comments[i].comment + "</mark></color>" + (i < clines.Length - 1 ? "\n" : "");
            break;

          case CodeNode.CommentType.MultiLineClose:
            code += comments[i].comment + "</mark></color>" + PrintLine(indent, line, i < clines.Length - 1);
            break;

          case CodeNode.CommentType.None:
          case CodeNode.CommentType.MultiLineInner:
          default:
            code += PrintLine(indent, line, i < clines.Length - 1);
            break;
        }

        if (rgBlockOpen.IsMatch(line)) indent++;
        continue;
      }

      clines[i] = compiledLine.Format(compVariables, true, comments[i].comment, comments[i].type);

      // Understand the required indent
      string l = rgCommentSL.Replace(compiledLine.Format(compVariables, false), "").Trim();
      if (indent < 0) indent = 0;
      if (rgBlockOpen.IsMatch(l)) {
        if (increaseone > 0) {
          indent -= increaseone;
          increaseone = 0;
        }
        code += PrintLine(indent, clines[i], i < clines.Length - 1);
        indent++;
      }
      else if (rgBlockClose.IsMatch(l)) { // close } -< decrease (and set also current line)
        indent--;
        if (indent < 0) indent = 0;
        code += PrintLine(indent, clines[i], i < clines.Length - 1);
      }
      else if ((compiledLine.type == BNF.WHILE || compiledLine.type == BNF.FOR || compiledLine.type == BNF.IF || compiledLine.type == BNF.ELSE) && compiledLine.iVal == 4) { // while with single statement on next line -> increase just one
        increaseone++;
        code += PrintLine(indent, clines[i], i < clines.Length - 1);
        indent++;
      }
      else if (!string.IsNullOrWhiteSpace(l)) {
        code += PrintLine(indent, clines[i], i < clines.Length - 1);
        indent -= increaseone;
        increaseone = 0;
        if (indent < 0) indent = 0;
      }
    }
    for (int i = end; i < clines.Length; i++) {
      code += olines[i];
      if (i < olines.Length - 1) code += "\n";
    }

    edit.SetTextWithoutNotify(code);
  }

  void ParseBlock() {
    if (blockCompile == 0) return;

    // Find the lines to be parsed
    string[] lines = rgSyntaxHighlight.Replace(edit.text, "").Trim().Split('\n');
    UpdateLinePos();
    int end = curline - 1;
    int start = curline - 1;
    int num = 0;
    for (int i = end; i >= 0; i--) {
      string line = rgCommentML.Replace(rgCommentSL.Replace(rgString.Replace(lines[i], ""), ""), "");
      if (rgBlockClose.IsMatch(line)) {
        num++;
      }
      if (rgBlockOpen.IsMatch(line)) {
        num--;
        if (num == 0) {
          start = i;
          break;
        }
      }
    }
    string code = "";
    for (int i = start; i <= end; i++) {
      code += lines[i] + "\n";
    }

    cp.SetOptimize(blockCompile == 2);
    CodeNode res = CompileCode(code, false, start);
    if (res == null) return;
    SetUndo();
    ParseBlock(res, start, end + 1);
    UpdateLineNumbers(res, start);
    SetLinePos();
  }

  string PrintLine(int tabs, string line, bool addNL) {
    string tab = "";
    for (int t = 0; t < tabs; t++) tab += '\t';
    string code = tab + line;
    if (addNL) return code + "\n";
    return code;
  }

  private CodeNode FindLine(CodeNode code, int line) {
    if (code == null) return null;
    if (code.origLineNum == line && code.type != BNF.BLOCK) return code;
    if (code.children == null) return null;
    foreach (CodeNode cn in code.children) {
      CodeNode res = FindLine(cn, line);
      if (res != null) return res;
    }
    return null;
  }

  void UpdateLineNumbers(CodeNode n, int incr) {
    if (n == null) return;
    n.origLineNum += incr;
    if (n.children == null) return;
    foreach (CodeNode c in n.children)
      UpdateLineNumbers(c, incr);
  }

  /* open { -> increase
   * close } -< decrease (and set also current line)
   * if/for/while without statement -> increase just one
   * 
   * if increased just one and normal statement -> collapse all increase by one
   * if increased just one and if/for/wile no statemeddnt -> increase by one again
   * 
   * 0   a++              | normal
   * 0   if (a) a++       | normal because statement here
   * 0   if (a)           | increases by just one line
   * 1    a++             | does the one line and goes back
   * 0   if (a) {         | indent++
   * 1    a++             | normal
   * 0   }                | indent--
   * 0   if (a)           | normal
   * 0   {                | indent++
   * 1    a++             | normal
   * 0   }                | indent--
   * 0   if (a)           | increases by just one line
   * 1    if (a)          | increases by just one line
   * 2      if (a)        | increases by just one line
   * 3        a++         | does the one line and goes back (3 times)
   * 0   if (a) {         | indent++
   * 1    if (a)          | increases by just one line
   * 2      if (a)        | increases by just one line
   * 3        a++         | does the one line and goes back (2 times)
   * 1    b++             | normal
   * 0   }                | indent--
   * 
   */

  public void CodeChange() {
    lastEdit = System.DateTime.Now;
    UpdateCompilationStatus();
  }

  public CodeNode CompileCode(string code, bool checkCompleteness, int startOffset = 0) {
    // Get all lines, produce an aggregated string, and do the full parsing.
    compVariables.Clear();
    codeHasErrors = false;
    CodeNode result = null;
    try {
      result = cp.Parse(code, compVariables, true, !checkCompleteness, startOffset);
      if (checkCompleteness && !result.HasNode(BNF.Config) && !result.HasNode(BNF.Data) && !result.HasNode(BNF.Start) && !result.HasNode(BNF.Update) && !result.HasNode(BNF.Functions)) {
        Result.text = "No executable code found (Start, Update, Functions, Config, or Data)";
        return null;
      }
      Result.text = "Parsing OK";

    } catch (ParsingException e) {
      Result.text = "<color=red>" + e.Message + "</color>\n" + e.Code + "\nLine: " + (e.LineNum);
      string[] olines = edit.text.Split('\n');
      int el = e.LineNum - 1;
      string tabs = "";
      if (el > 0) {
        string prev = rgSyntaxHighlight.Replace(olines[el - 1], "");
        foreach (char c in prev) {
          if (c == '\t') tabs += "\t";
          else break;
        }
      }
      if (el >= 0 && el < olines.Length) {
        olines[el] = tabs + "<color=red>" + rgSyntaxHighlight.Replace(olines[el], "").Trim() + " </color>";
        string coderes = "";
        for (int i = 0; i < olines.Length - 1; i++)
          coderes += olines[i] + "\n";
        coderes += olines[olines.Length - 1];
        edit.SetTextWithoutNotify(coderes);
      }
      curline = e.LineNum;
      SetLinePos();
    } catch (System.Exception e) {
      Result.text = "<color=red>" + e.Message + "</color>";
    }
    return result;
  }



  // ************************************************************************************************************************************************************************************************



  #region Run / Debug ***********************************************************************************************************************************************

  public Arcade arcade;
  ByteChunk rom = null;
  public Image[] ButtonsSelection;

  void ShowButton(Arcade.RunStatus m) {
    int b = (int)m;
    for (int i = 0; i < ButtonsSelection.Length; i++)
      ButtonsSelection[i].enabled = i == b;
  }

  public void AttachRom() {
    // Filebrowser with roms
    FileBrowser.Load(AttachRomPost, FileBrowser.FileType.Rom);
  }

  public void AttachRomPost(string path) {
    // Once selected read and generate the ByteChunk
    try {
      rom = new ByteChunk();
      ByteReader.ReadBinBlock(path, rom);
      Result.text = "Rom loaded";
    } catch (System.Exception e) {
      Result.text = "<color=#ff2e00>Error in loading rom:\n" + e.Message + "</color>";
      rom = null;
    }
  }

  public void Run(bool restart) {
    ShowButton(Arcade.RunStatus.Running);
    int num = 1;
    foreach (char c in edit.text)
      if (c == '\n')
        num++;
    RedrawLineNumbersAndBreakPoints(num, 0);

    if (lastCompile > lastDeploy && deployedCode != compiledCode && 
      (arcade.runStatus == Arcade.RunStatus.Running || arcade.runStatus == Arcade.RunStatus.Paused || arcade.runStatus == Arcade.RunStatus.RunAFrame || arcade.runStatus == Arcade.RunStatus.RunAStep)) {
      // Pause or run. Replace the code, using a specific function. Only the update and the variables will be replaced
      runVariables.CopyValuesFrom(compVariables);
      deployedCode = compiledCode;
      lastDeploy = System.DateTime.Now;
      arcade.UpdateCode(deployedCode, runVariables, breakPoints);
      UpdateCompilationStatus();
    }
    else if (arcade.runStatus != Arcade.RunStatus.Paused && !restart) {
      // Compile the code, if errors show them and stop
      compiledCode = CompileCode(rgSyntaxHighlight.Replace(edit.text, "").Trim(), true);
      if (compiledCode == null) {
        ShowButton(Arcade.RunStatus.Error);
        codeHasErrors = true;
        return;
      }
      runVariables.CopyValuesFrom(compVariables);
      deployedCode = compiledCode;
      lastCompile = System.DateTime.Now; ;
      lastDeploy = lastCompile;
      UpdateCompilationStatus();
      // Reset the Arcade, and pass the parsed parts
      arcade.LoadCode(deployedCode, runVariables, rom, UpdateVariables, CompletedExecutionStep, breakPoints);
    }
    arcade.runStatus = Arcade.RunStatus.Running;
  }

  public void Pause() {
    if (arcade.runStatus == Arcade.RunStatus.Paused) {
      ShowButton(Arcade.RunStatus.Running);
      arcade.runStatus = Arcade.RunStatus.Running;
      int num = 1;
      foreach (char c in edit.text)
        if (c == '\n')
          num++;
      RedrawLineNumbersAndBreakPoints(num, 0);
    }
    else if (arcade.runStatus == Arcade.RunStatus.Running || arcade.runStatus == Arcade.RunStatus.RunAStep || arcade.runStatus == Arcade.RunStatus.RunAFrame) {
      ShowButton(Arcade.RunStatus.Paused);
      arcade.runStatus = Arcade.RunStatus.GoPause;
    }
  }

  public void Stop() {
    ShowButton(Arcade.RunStatus.Stopped);
    arcade.runStatus = Arcade.RunStatus.Stopped;
    int num = 1;
    foreach (char c in edit.text)
      if (c == '\n')
        num++;
    RedrawLineNumbersAndBreakPoints(num, 0);
    InspectorVariablesTxt.SetTextWithoutNotify("");
    VariableEditName.text = "";
    VariableEditVal.SetTextWithoutNotify("");
    selectedVar = -1;
  }

  public void RunStep(bool frame) {
    if (arcade.runStatus != Arcade.RunStatus.Paused) return;
    if (frame) {
      ShowButton(Arcade.RunStatus.RunAFrame);
      arcade.runStatus = Arcade.RunStatus.RunAFrame;
    }
    else {
      ShowButton(Arcade.RunStatus.RunAStep);
      arcade.runStatus = Arcade.RunStatus.RunAStep;
    }
  }

  public GameObject InspectorVariables;
  public TMP_InputField InspectorVariablesTxt;
  public GameObject[] VariableLineTemplate;
  public Transform BackgroundVariables;
  int selectedVar = -1;
  public TextMeshProUGUI VariableEditName;
  public TMP_InputField VariableEditVal;
  public TextMeshProUGUI CompilationStatus;

  public void UpdateVariables(Variables vars) {
    if (!InspectorVariables.activeSelf) return;
    InspectorVariablesTxt.SetTextWithoutNotify(vars.GetFormattedValues(out int num));

    BackgroundVariables.SetAsFirstSibling();
    if (BackgroundVariables.childCount > num) {
      for (int i = num; i < BackgroundVariables.childCount; i++) {
        GameObject line = BackgroundVariables.GetChild(BackgroundVariables.childCount - 1).gameObject;
        line.transform.SetParent(null);
        GameObject.Destroy(line);
      }
    }
    while (BackgroundVariables.childCount < num) {
      BackgroundLine line = Instantiate(VariableLineTemplate[BackgroundVariables.childCount & 1], BackgroundVariables).GetComponent<BackgroundLine>();
      line.lineNumber = BackgroundVariables.childCount - 1;
      line.CallClick = SelectVariable;
      line.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 1.1625f * 26);
    }
    SelectVariable(selectedVar);
  }

  public void CompletedExecutionStep(int lineNumber) {
    if (arcade.runStatus == Arcade.RunStatus.Paused)
      ShowButton(Arcade.RunStatus.Paused);
    else if (arcade.runStatus == Arcade.RunStatus.Stopped || arcade.runStatus == Arcade.RunStatus.Error)
      ShowButton(Arcade.RunStatus.Stopped);
    int num = 1;
    foreach (char c in edit.text)
      if (c == '\n')
        num++;
    RedrawLineNumbersAndBreakPoints(num, lineNumber);
    if (arcade.LastErrorMessage != null) Result.text = "<color=red>" + arcade.LastErrorMessage + "</color>";
  }

  public void ShowHideVariables() {
    if (!arcade.running) {
      Result.text = "Engine is not running.";
      return;
    }
    InspectorVariables.SetActive(!InspectorVariables.activeSelf);
    if (InspectorVariables.activeSelf) arcade.ReadVariables();
  }
  public void CloseVariables() {
    InspectorVariables.SetActive(false);
  }

  public void SelectVariable(int selected) {
    if (selected != -1) selectedVar = selected;
    if (selectedVar == -1 || runVariables.Invalid(selectedVar)) {
      VariableEditName.text = "<size=19><i>Select variable to edit...</i></size>";
      VariableEditVal.SetTextWithoutNotify("");
    }
    else {
      Value val = runVariables.Get(selectedVar);
      switch (val.type) {
        case VT.None: VariableEditName.text = "<color=#20f350>NUL</color> " + runVariables.GetRegName(selectedVar); break;
        case VT.Int: VariableEditName.text = "<color=#20f350>INT</color> " + runVariables.GetRegName(selectedVar); break;
        case VT.Float: VariableEditName.text = "<color=#20f350>FLT</color> " + runVariables.GetRegName(selectedVar); break;
        case VT.String: VariableEditName.text = "<color=#20f350>STR</color> " + runVariables.GetRegName(selectedVar); break;
        case VT.Array: VariableEditName.text = "<color=#20f350>ARR</color> " + runVariables.GetRegName(selectedVar); break;
      }
      if (EventSystem.current.currentSelectedGameObject == VariableEditVal.gameObject) return;
      string v = val.ToStr();
      if (VariableEditVal.text != v) VariableEditVal.SetTextWithoutNotify(v);
    }
  }

  readonly Regex rgDec = new Regex("^[\\s]*(\\-)?[0-9]+[\\s]*$", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));
  readonly Regex rgFlt = new Regex("^[\\s]*(\\-)?[0-9]+\\.[0-9]+[\\s]*$", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));
  readonly Regex rgStr = new Regex("^[\\s]*(\")([^\"]*)(\")[\\s]*$", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));

  public void EditVariableValue(string val) {
    if (runVariables == null || runVariables.Invalid(selectedVar)) return;

    if (rgDec.IsMatch(val)) { // -> int
      int.TryParse(val, out int v);
      runVariables.Set(selectedVar, v);
    }
    else if (rgFlt.IsMatch(val)) { // -> float
      float.TryParse(val, out float v);
      runVariables.Set(selectedVar, v);
    }
    else if (rgStr.IsMatch(val)) { // -> string
      string v = rgStr.Match(val).Groups[2].Value.Replace("\\\"", "\"");
      runVariables.Set(selectedVar, v);
    }
    else if (!string.IsNullOrWhiteSpace(val)) { // Assume it is a string
      runVariables.Set(selectedVar, val);
    }
    else { // -> NUL
      runVariables.Set(selectedVar);
    }
    SelectVariable(selectedVar); // To update visuals
  }

  System.DateTime lastEdit;
  System.DateTime lastCompile;
  System.DateTime lastDeploy;
  CodeNode compiledCode = null;
  CodeNode deployedCode = null;
  bool codeHasErrors = false;

  /*
  No code -> No code
  Code but not compiled -> Compile
  Code compiled but different code running -> New code not deployed
  Code but not compiled and different code running -> Compile the new code
   
   */

  void UpdateCompilationStatus() {
    if (codeHasErrors) {
      CompilationStatus.text = "<color=#fe2f40><i>Fix code errors</i></color>";
    }
    else if (compiledCode == null) {
      if (string.IsNullOrWhiteSpace(edit.text)) {
        CompilationStatus.text = "<color=#80feef><i>No code</i></color>";
      }
      else {
        CompilationStatus.text = "<color=#feef80><i>You should compile</i></color>";
      }
    }
    else {
      if (lastEdit > lastCompile)
        CompilationStatus.text = "<color=#5eefc0><i>Compile the new code</i></color>";
      else if (lastCompile > lastDeploy)
        CompilationStatus.text = "<color=#8eefa0><i>New code not deployed</i></color>";
      else
        CompilationStatus.text = "<color=#3efe60><i>Code is deployed</i></color>";
    }
  }

  #endregion Run / Debug ***********************************************************************************************************************************************

  #region Load / Save ***********************************************************************************************************************************************
  public GameObject LoadSaveButtons;
  public Confirm Confirm;
  public TMP_InputField Values;
  public Button LoadSubButton;

  public void LoadSave() {
    LoadSaveButtons.SetActive(!LoadSaveButtons.activeSelf);
  }

  public void LoadTextPre() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }
  public void LoadTextPost() {
    if (!gameObject.activeSelf) return;
    Values.gameObject.SetActive(false);
    LoadSaveButtons.SetActive(false);

    edit.SetTextWithoutNotify(Values.text.Trim());
    curline = 0;
    UpdateLinePos();
  }

  public void SaveText() {
    string code = rgSyntaxHighlight.Replace(edit.text, "").Trim(' ', '\r', '\n');
    Values.gameObject.SetActive(true);
    Values.text = code;
    LoadSubButton.enabled = false;
    LoadSaveButtons.SetActive(false);
  }


  public void LoadTextFilePre() {
    FileBrowser.Load(LoadTextFilePost, FileBrowser.FileType.Code);
  }
  public void LoadTextFilePost(string path) {
    LoadSaveButtons.SetActive(false);
    try {
      string data = System.IO.File.ReadAllText(path);
      data = data.Replace("\r\n", "\n").Replace('\r', '\n').Trim(' ', '\r', '\n');
      edit.SetTextWithoutNotify(data);
      curline = 0;
      UpdateLinePos();
    } catch (System.Exception e) {
      Result.text = e.Message;
    }
  }

  public void SaveTextFilePre() {
    FileBrowser.Save(SaveTextFilePost, FileBrowser.FileType.Cartridges);
  }

  public void SaveTextFilePost(string path, string name) {
    LoadSaveButtons.SetActive(false);
    string code = rgSyntaxHighlight.Replace(edit.text, "").Trim(' ', '\r', '\n');
    path = System.IO.Path.Combine(path, name);
    try {
      if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
      using System.IO.StreamWriter outputFile = new System.IO.StreamWriter(path);
      outputFile.Write(code);
    } catch(System.Exception e) {
      Result.text = e.Message;
    }
  }


  #endregion Load / Save ***********************************************************************************************************************************************


  #region Find / Replace ***********************************************************************************************************************************************

  public TMP_InputField TextToFind;
  public TMP_InputField TextToReplace;
  public GameObject FindReplace;
  public TextMeshProUGUI FindMsg;
  int prevFinding = 0;
  string prevTextToFind = null;

  public void CloseFindReplace() {
    FindReplace.SetActive(false);
    FindMsg.text = "";
  }

  public void Find() {
    string toFind = TextToFind.text.ToLowerInvariant();
    string code = rgSyntaxHighlight.Replace(edit.text, "");
    int pos;
    if (toFind.Equals(prevTextToFind) && edit.selectionAnchorPosition != edit.selectionFocusPosition)
      pos = code.IndexOf(toFind, prevFinding, System.StringComparison.InvariantCultureIgnoreCase);
    else {
      pos = code.IndexOf(toFind, System.StringComparison.InvariantCultureIgnoreCase);
      prevTextToFind = toFind;
    }
    prevFinding = pos + 1;
    if (pos == -1) {
      FindMsg.text = "Text was not found";
      return;
    }
    FindMsg.text = "";
    edit.SetTextWithoutNotify(code);
    edit.caretPosition = pos + toFind.Length;
    edit.stringPosition = pos;
    edit.selectionAnchorPosition = pos;
    edit.selectionFocusPosition = pos + toFind.Length;
    edit.selectionStringAnchorPosition = pos;
    edit.selectionStringFocusPosition = pos + toFind.Length;
    edit.Select();
    EventSystem.current.SetSelectedGameObject(edit.gameObject);
  }

  public void Replace(bool all) {
    string toFind = TextToFind.text.ToLowerInvariant();
    string toReplace = TextToReplace.text;
    string code = rgSyntaxHighlight.Replace(edit.text, "");

    if (all) {
      int num = 0;
      Find();
      while (prevFinding > 0) {
        num++;
        code = code.Substring(0, prevFinding - 1) + toReplace + code.Substring(prevFinding + toFind.Length - 1);
        edit.SetTextWithoutNotify(code);
        Find();
      }
      if (num == 0)
        FindMsg.text = "Text not found";
      else
        FindMsg.text = "Replaced " + num + " occurrences";
    }
    else {
      if (prevFinding < 1 || code.IndexOf(toFind, prevFinding - 1, System.StringComparison.InvariantCultureIgnoreCase) != prevFinding - 1) {
        Find();
        if (prevFinding == 0) {
          FindMsg.text = "No more occurences";
          return; // Not found
        }
      }
      // Current selected part is exactly what we are finding: replace
      code = code.Substring(0, prevFinding - 1) + toReplace + code.Substring(prevFinding + toFind.Length - 1);
      edit.SetTextWithoutNotify(code);
      Find();
    }
  }

  #endregion Find / Replace ***********************************************************************************************************************************************
}



