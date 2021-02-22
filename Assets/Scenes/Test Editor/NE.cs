using System;
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
      UpdateLinePos();
    }
    if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) UpdateLinePos();

    delay -= Time.deltaTime;
    if (delay < 0 && edit.text.Length != prevSize) {
      delay = 1;
      prevSize = edit.text.Length;
      FixFormatting();
    }
  }

  void UpdateLinePos() {
    int num = 1;
    int pos = 0;
    foreach (char c in edit.text) {
      pos++;
      if (c == '\n') {
        num++;
      }
      if (pos == edit.caretPosition) {
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
  readonly Regex rgBlockOpen = new Regex("(?<!//.*?)\\{", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(5));
  readonly Regex rgBlockOpenAlone = new Regex("^[\\s]*\\{[\\s]*", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(5));
  readonly Regex rgBlockClose = new Regex("(?<!//.*?)\\}", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(5));

  public void Parse() {
    CodeNode compiled = CompileCode();
    if (compiled == null) return;

    // We need to find all nodes, one by one
    // For each grab the line number and get the Format (only if it is a command)
    // reconstruct the lines and update the input field
    string code = rgSyntaxHighlight.Replace(edit.text, "").Trim();
    string[] lines = code.Split('\n');
    code = "";
    int indent = 0;
    int increaseone = 0;
    for (int i = 0; i < lines.Length; i++) {
      CodeNode compiledLine = FindLine(compiled, i + 1);
      string line = lines[i].Trim(' ', '\t', '\r', '\n');
      if (compiledLine == null) {
        if (rgBlockClose.IsMatch(line)) indent--;
        code += PrintLine(indent, line, i < lines.Length - 1);
        if (rgBlockOpen.IsMatch(line)) indent++;
        continue;
      }
      
      lines[i] = compiledLine.Format(variables, true);

      // Understand the required indent
      string l = rgCommentSL.Replace(compiledLine.Format(variables, false), "").Trim();
      if (indent < 0) indent = 0;
      if (rgBlockOpen.IsMatch(l)) {
        if (increaseone > 0) {
          indent -= increaseone;
          increaseone = 0;
        }
        code += PrintLine(indent, lines[i], i < lines.Length - 1);
        indent++;
      }
      else if (rgBlockClose.IsMatch(l)) { // close } -< decrease (and set also current line)
        indent--;
        if (indent < 0) indent = 0;
        code += PrintLine(indent, lines[i], i < lines.Length - 1);
      }
      else if ((compiledLine.type == BNF.WHILE || compiledLine.type == BNF.FOR || compiledLine.type == BNF.IF || compiledLine.type == BNF.Else) && compiledLine.iVal == 4) { // while with single statement on next line -> increase just one
        increaseone++;
        code += PrintLine(indent, lines[i], i < lines.Length - 1);
        indent++;
      }
      // FIXME do the same for IF, ELSE
      else if (!string.IsNullOrWhiteSpace(l)) {
        code += PrintLine(indent, lines[i], i < lines.Length - 1);
        indent -= increaseone;
        increaseone = 0;
        if (indent < 0) indent = 0;
      }

    }
    edit.SetTextWithoutNotify(code);
  }

  string PrintLine(int tabs, string line, bool addNL) { // 
    string tab = "";
    for (int t = 0; t < tabs; t++) tab += '\t';
    string code = tab + line;
    if (addNL) return code + "\n";
    return code;
  }

  private CodeNode FindLine(CodeNode code, int line) {
    if (code.origLineNum == line && code.type != BNF.BLOCK) return code;
    if (code.children == null) return null;
    foreach (CodeNode cn in code.children) {
      CodeNode res = FindLine(cn, line);
      if (res != null) return res;
    }
    return null;
  }

 


    /* open { -> increase
     * close } -< decrease (and set also current line)
     * if/for/while without statement -> increase just one
     * 
     * if increased just one and normal statement -> collapse all increase by one
     * if increased just one and if/for/wile no statemeddnt -> increase by one again
     * 
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
     * 
     * 
     * 
     */


  

  public CodeNode CompileCode() {
    // Get all lines, produce an aggregated string, and do the full parsing.
    string code = rgSyntaxHighlight.Replace(edit.text, "").Trim();
    variables.Clear();
    CodeNode result = null;
    try {
      result = cp.Parse(code, variables, true);
      if (!result.HasNode(BNF.Config) && !result.HasNode(BNF.Data) && !result.HasNode(BNF.Start) && !result.HasNode(BNF.Update) && !result.HasNode(BNF.Functions)) {
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



/*
add breakpoints on the left
add errors on the right
 try parsing

in parse have a falg for for,if,else,while to keep an open braket after

[low pri] Scrolling does not move the background lines

 */