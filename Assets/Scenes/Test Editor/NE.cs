using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NE : MonoBehaviour {

  public TMP_InputField edit;
  public Scrollbar Scroll;
  public TextMeshProUGUI EditText;
  public TextMeshProUGUI LineNumbers;
  public TextMeshProUGUI Result;
  public TextMeshProUGUI dbg;
  public TMP_FontAsset MonoFont;
  readonly List<Comment> comments = new List<Comment>();
  readonly List<string> undos = new List<string> { "" };
  int undopos = 0;

  private void Start() {
    MonoFont.tabSize = 2;
    edit.resetOnDeActivation = false;
    edit.onFocusSelectAll = false;
    edit.restoreOriginalTextOnEscape = false;

    numlines = 1;
    foreach (char c in edit.text)
      if (c == '\n') numlines++;
  }


  float delay = 1;
  int prevSize = 0;
  private void Update() {
    dbg.text = curline + "/" + numlines;

    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return)) {
      SetUndo();
      UpdateLinePos();
    }
    if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) UpdateLinePos();
    if (Input.GetKeyUp(KeyCode.RightCurlyBracket) || (Input.GetKeyUp(KeyCode.RightBracket) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))) {
      SetUndo();
      ParseBlock();
    }

    if (Input.GetKeyUp(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) Undo();
    if (Input.GetKeyUp(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) Redo();

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

  void UpdateLinePos() {
    int num = 1;
    int pos = 0;
    edit.SetTextWithoutNotify(edit.text.Replace("\r\n", "\n").Replace("\r", "\n"));
    foreach (char c in edit.text) {
      pos++;
      if (c == '\n') {
        num++;
      }
      if (pos == edit.stringPosition) {
        curline = num;
      }
    }

    if (numlines != num) {
      string nums = "";
      for (int i = 1; i <= num; i++)
        nums += i + "\n";
      LineNumbers.text = nums.Substring(0, nums.Length - 1);
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

  int fontSize = 28;
  public TMP_Dropdown FontSizeDD;
  public void ChangeFontSize() {
    int.TryParse(FontSizeDD.options[FontSizeDD.value].text.Substring(11), out int newSize);
    if (newSize == 0) newSize = 28;
    edit.pointSize = newSize;
    LineNumbers.fontSize = EditText.fontSize;
    fontSize = newSize;

    foreach (RectTransform rt in BackgroundLines) {
      rt.sizeDelta = new Vector2(1248, 1.1625f * fontSize);
    }
  }

  public RectTransform[] BackgroundLines;
  public float scalefs = 1f;

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
  readonly Regex rgSyntaxHighlight = new Regex("(\\<color=#[0-9a-f]{6}\\>)|(\\</color\\>)|(\\<mark=#[0-9a-f]{8}\\>)|(\\</mark\\>)|(\\<b\\>)|(\\</b\\>)|(\\<i\\>)|(\\</i\\>)", RegexOptions.IgnoreCase);
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
      string line = clines[i];
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
      code += clines[i] + "\n";
    }

    for (int i = start; i < end; i++) {
      CodeNode compiledLine = FindLine(compiled, i + 1);
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
            code += PrintLine(indent, line, false) + " <color=#70e688><mark=#30061880>" + comments[i].comment + "</mark></color>" + (i < clines.Length - 1 ? "\n" : "");
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
      string line = clines[i];
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
      code += clines[i];
      if (i < clines.Length - 1) code += "\n";
    }

    edit.SetTextWithoutNotify(code);
  }

  void ParseBlock() {
    if (blockCompile == 0) return;

    // Find the lines to be parsed
    string[] lines = rgSyntaxHighlight.Replace(edit.text, "").Trim().Split('\n');
    if (curline < 0 || curline >= lines.Length) UpdateLinePos();
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

    SetUndo();
    CodeNode res = CompileCode(code, false);
    UpdateLineNumbers(res, start);
    cp.SetOptimize(blockCompile == 2);
    ParseBlock(res, start, end + 1);
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




  public CodeNode CompileCode(string code, bool checkCompleteness) {
    // Get all lines, produce an aggregated string, and do the full parsing.
    variables.Clear();
    CodeNode result = null;
    try {
      result = cp.Parse(code, variables, true, !checkCompleteness);
      if (checkCompleteness && !result.HasNode(BNF.Config) && !result.HasNode(BNF.Data) && !result.HasNode(BNF.Start) && !result.HasNode(BNF.Update) && !result.HasNode(BNF.Functions)) {
        Result.text = "No executable code found (Start, Update, Functions, Config, or Data)";
        return null;
      }
      Result.text = "Parsing OK";

    } catch (ParsingException e) {
      Result.text = "<color=red>" + e.Message + "</color>\n" + e.Code + "\nLine: " + e.LineNum;
      // FIXME Scroll to line number
    } catch (System.Exception e) {
      Result.text = "<color=red>" + e.Message + "</color>";
    }
    return result;
  }

}

public class Comment {
  public string comment;
  public CodeNode.CommentType type;
  public int indent;
  public Comment() {
    comment = null;
    type = CodeNode.CommentType.None;
    indent = 0;
  }
  public Comment(string c, CodeNode.CommentType t) {
    comment = c;
    type = t;
    indent = 0;
  }

  internal void Set(string value, CodeNode.CommentType t) {
    comment = value;
    type = t;
  }
  public void Zero() {
    comment = null;
    type = CodeNode.CommentType.None;
    indent = 0;
  }
}


/*
add breakpoints on the left
add errors on the right
 try parsing

in parse have a falg for for,if,else,while to keep an open braket after

[low pri] Scrolling does not move the background lines

 */