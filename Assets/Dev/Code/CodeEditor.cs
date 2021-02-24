using System;
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
  }

  float delay = 1;
  int prevSize = 0;
  private void Update() {
    CurrentLineText.text = curline + "/" + numlines; // FIXME show it somewhere

    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) {
      SetUndo();
      UpdateLinePos();
    }
    if (Input.GetKeyUp(KeyCode.RightCurlyBracket) || (Input.GetKeyUp(KeyCode.RightBracket) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))) {
      SetUndo();
      ParseBlock();
    }

    if (Input.GetKeyUp(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) Undo();
    if (Input.GetKeyUp(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) Redo();

    // FIXME ctrl+d
    // FIXME ctrl+del

    if (Input.GetMouseButtonDown(0) && Input.mousePosition.x < 80 && overLine > 0 && overLine <= numlines) {
      Debug.Log("Breakpoint on line: " + overLine);
      if (breakPoints.Contains(overLine)) breakPoints.Remove(overLine);
      else breakPoints.Add(overLine);
      RedrawLineNumbersAndBreakPoints(numlines);
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

  readonly List<int> breakPoints = new List<int>();

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
      RedrawLineNumbersAndBreakPoints(num);

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
        if (num == curline) {
          edit.stringPosition = pos;
          return;
        }
      }
    }
  }

  private void RedrawLineNumbersAndBreakPoints(int num) {
    string nums = "";
    for (int i = 1; i <= num; i++) {
      if (breakPoints.Contains(i)) {
        if (i < 10) nums += "<sprite=1>   " + i + "\n";
        else if (i < 100) nums += "<sprite=1>  " + i + "\n";
        else if (i < 1000) nums += "<sprite=1> " + i + "\n";
        else nums += "<sprite=1>" + i + "\n";
      }
      else
        nums += i + "\n";
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
  readonly Variables variables = new Variables();
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


  public void Parse(bool optimize) {
    SetUndo();
    cp.SetOptimize(optimize);
    CompileContainer.SetActive(false);
    CodeNode compiled = CompileCode(rgSyntaxHighlight.Replace(edit.text, "").Trim(), true);
    if (compiled == null) return;
    ParseBlock(compiled, 0, int.MaxValue);
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

      clines[i] = compiledLine.Format(variables, true, comments[i].comment, comments[i].type);

      // Understand the required indent
      string l = rgCommentSL.Replace(compiledLine.Format(variables, false), "").Trim();
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

  string PrintLine(int tabs, string line, bool addNL) { // 
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




  public CodeNode CompileCode(string code, bool checkCompleteness, int startOffset = 0) {
    // Get all lines, produce an aggregated string, and do the full parsing.
    variables.Clear();
    CodeNode result = null;
    try {
      result = cp.Parse(code, variables, true, !checkCompleteness, startOffset);
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
  enum RunMode { Stopped=0, Runnig=1, Paused=2, Error=99 };
  RunMode runMode = RunMode.Stopped;
  public Image[] ButtonsSelection;

  void ShowButton(RunMode m) {
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
    } catch (Exception e) {
      Result.text = "<color=#ff2e00>Error in loading rom:\n" + e.Message + "</color>";
      rom = null;
    }
  }

  public void Run() {
    ShowButton(RunMode.Runnig);
    // Compile the code, if errors show them and stop
    CodeNode code = CompileCode(rgSyntaxHighlight.Replace(edit.text, "").Trim(), true); 
    if (code == null) return;

    // Reset the Arcade, and pass the parsed parts
    arcade.LoadCode(code, variables, rom, UpdateVariables);
  }

  public GameObject InspectorVariables;
  public TMP_InputField InspectorVariablesTxt;
  public void UpdateVariables(Variables vars) {
    if (!InspectorVariables.activeSelf) return;
    InspectorVariablesTxt.SetTextWithoutNotify(vars.GetFormattedValues());
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



  public void AlterBreakPoint(int num) {
    Debug.Log(num);
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

  }

  #endregion Load / Save ***********************************************************************************************************************************************
}



