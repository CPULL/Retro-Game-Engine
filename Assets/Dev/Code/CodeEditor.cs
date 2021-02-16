using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodeEditor : MonoBehaviour {
  readonly List<LineData> lines = new List<LineData>();
  public CodeLine[] EditLines;
  public Scrollbar VerticalCodeBar;
  public RectTransform SelectionRT;
  public TextMeshProUGUI Result;
  public TextMeshProUGUI dbg;

  readonly CodeParser cp = new CodeParser();
  readonly Variables variables = new Variables();
  int currentLine = 0;
  int editLine = 0;
  float autorepeat = 0;
  int selectionS = -1;
  int selectionE = -1;
  string copied = "";

  private void Start() {
    foreach(CodeLine cl in EditLines) {
      cl.Number.text = "";
      cl.Line.SetTextWithoutNotify("");
      cl.Line.onFocusSelectAll = false;
      cl.Line.restoreOriginalTextOnEscape = true;
    }
    LineData l = new LineData(0);
    EditLines[0].SetLine(0);
    lines.Add(l);

    for (int i = 0; i < EditLines.Length; i++) {
      if (i < lines.Count) EditLines[i].SetLine(i);
      else EditLines[i].Clean();
    }

    Redraw();
    EventSystem.current.SetSelectedGameObject(EditLines[0].Line.gameObject);
  }

  void Redraw(bool doNotUpdate = false) {
    if (!doNotUpdate) {
      // Save if we need
      for (int i = 0; i < EditLines.Length; i++) {
        int pos = EditLines[i].linenum;
        if (pos >= 0 && pos < lines.Count) {
          if (lines[pos].line != EditLines[i].line) {
            lines[pos].line = EditLines[i].line;
          }
        }
      }
    }

    // What is the first visible line?
    int topLine = currentLine - editLine;
    if (topLine < 0) topLine = 0;
    for (int i = 0; i < EditLines.Length; i++) {
      int pos = i + topLine;
      if (pos < lines.Count) EditLines[i].SetLine(pos, lines[pos]);
      else EditLines[i].Clean();
    }
    if (EditLines[editLine].linenum == -1) {
      for (int i = editLine - 1; i >= 0; i--)
        if (EditLines[i].linenum != -1) {
          editLine = i;
          EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject);
          break;
        }
    }
    SetScroll();

    // Selection
    if (selectionS != -1 && selectionE != -1) {
      // Find the editrow with the startid, if less than 0 set it at 0, if more than 30 set it as 30
      // Find the editrow with the endid, if less than 0 set it at 0, if more than 30 set it as 30
      int rowStart = -1, rowEnd = -1;
      for (int i = 0; i < 31; i++) {
        if (EditLines[i].linenum == selectionS) rowStart = i;
        if (EditLines[i].linenum == selectionE) rowEnd = i;
      }

      if (rowStart == -1 && rowEnd == -1) SelectionRT.sizeDelta = new Vector2(1280, 0);
      else if (rowStart == -1 && rowEnd != -1) {
        SelectionRT.sizeDelta = new Vector2(1280, 33 * (rowEnd + 1));
        SelectionRT.anchoredPosition = new Vector2(0, 0);
      }
      else if (rowStart != -1 && rowEnd == -1) {
        SelectionRT.sizeDelta = new Vector2(1280, 33 * (30 - rowStart));
        SelectionRT.anchoredPosition = new Vector2(0, -33 * rowStart);
      }
      else {
        SelectionRT.sizeDelta = new Vector2(1280, 33 * (1 + rowEnd - rowStart));
        SelectionRT.anchoredPosition = new Vector2(0, -33 * rowStart);
      }

      foreach (CodeLine cl in EditLines)
        if (cl.Line.isFocused) {
          cl.Line.ReleaseSelection();
        }
    }
    else
      SelectionRT.sizeDelta = new Vector2(1280, 0);

  }

  bool settingScroll = false;
  void SetScroll() {
    settingScroll = true;
    float size = 30f / lines.Count;
    if (size > 1) size = 1;
    VerticalCodeBar.size = size;
    VerticalCodeBar.SetValueWithoutNotify((float)currentLine / lines.Count);
    int steps = lines.Count - 30;
    if (steps < 0) steps = 0;
    VerticalCodeBar.numberOfSteps = steps;
    settingScroll = false;
  }

  public void ScrollByBar() {
    if (settingScroll) return;
    currentLine = Mathf.RoundToInt(VerticalCodeBar.value * lines.Count);
    Redraw();
  }





  private void Update() {
    if (autorepeat > 0) autorepeat -= Time.deltaTime;
    bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    bool up = Input.GetKeyDown(KeyCode.UpArrow);
    bool down = Input.GetKeyDown(KeyCode.DownArrow);
    bool pup = Input.GetKey(KeyCode.UpArrow);
    bool pdown = Input.GetKey(KeyCode.DownArrow);
    bool enter = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

    if (ctrl) { // ******************************* Control *********************************************************************************************
      // Clear, insert, and duplicate
      if (Input.GetKeyDown(KeyCode.D)) {
        SaveLine();
        lines.Insert(currentLine, lines[currentLine].Duplicate());
        Redraw(true);
      }
      if (Input.GetKeyDown(KeyCode.Insert)) {
        SaveLine();
        LineData l = new LineData(lines[currentLine].indent);
        lines.Insert(currentLine, l);
        Redraw(true);
      }
      if (Input.GetKeyDown(KeyCode.Delete) && lines.Count > 1) {
        lines.RemoveAt(currentLine);
        if (currentLine >= lines.Count) currentLine = lines.Count - 1;
        Redraw(true);
      }

      // Ctrl+C
      if (Input.GetKeyDown(KeyCode.C) && selectionS != -1 && selectionE != -1) {
        copied = "";
        SaveLine();
        for (int line = selectionS; line <= selectionE; line++) {
          copied += lines[line];
          if (line != selectionE) copied += "\n";
        }
        selectionS = -1;
        selectionE = -1;
        SelectionRT.sizeDelta = new Vector2(1280, 0);
      }

      // Ctrl+X
      if (Input.GetKeyDown(KeyCode.X) && selectionS != -1 && selectionE != -1) {
        copied = "";
        SaveLine();
        for (int line = selectionS; line <= selectionE; line++) {
          copied += lines[line];
          if (line != selectionE) copied += "\n";
        }
        // Remove the copied lines
        for (int i = 0; i < selectionE - selectionS + 1; i++)
          lines.RemoveAt(selectionS);
        selectionS = -1;
        selectionE = -1;
        SelectionRT.sizeDelta = new Vector2(1280, 0);
        Redraw(true);
      }

      // Ctrl+V
      if (Input.GetKeyDown(KeyCode.V)) {
        // If we have copied lines, paste them as lines
        // If we have something in clipboard that has at least one newline, treat it as pasting lines (and do not save the currentEditLine because it will contain invalid data
        if (string.IsNullOrEmpty(copied)) {
          string clip = GUIUtility.systemCopyBuffer;
          if (clip.IndexOf('\n') != -1) copied = clip;
        }
        copied = copied.Trim(' ', '\t', '\n', '\r');
        if (!string.IsNullOrEmpty(copied)) {
          // How many lines?
          int num = 1;
          foreach(char c in copied) {
            if (c == '\n') num++;
          }
          string[] rows = copied.Split('\n');
          for (int i = rows.Length - 1; i >= 0; i--) {
            LineData l = new LineData(lines[currentLine].indent) {
              line = rows[i].Trim(' ', '\t', '\n', '\r')
            };
            lines.Insert(currentLine, l);
          }
          Redraw(true);
        }
      }


      
    }
    else if (shift) { // ******************************* Shift *********************************************************************************************
      // Selection
      if (down && (selectionS == -1 || selectionE == -1)) { // If nothing is selected, select current line and move up or down
        SaveLine();
        selectionS = currentLine;
        selectionE = currentLine;
        if (editLine < 28) {
          editLine++;
          EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject); // This will update currentline
        }
        else
          currentLine++;
        if (currentLine >= lines.Count) currentLine = lines.Count - 1;
        Redraw();

        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE + "\nCase 1";

      }
      else if (up && (selectionS == -1 || selectionE == -1)) { // If nothing is selected, select current line and move up or down
        SaveLine();
        selectionS = currentLine;
        selectionE = currentLine;
        if (currentLine > 0) currentLine--;
        if (editLine > 0) editLine--;
        Redraw();
        EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject);

        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE + "\nCase 1";

      }
      else if (currentLine <= selectionS && up) { // Extend
        selectionS--;
        if (selectionS < 0) selectionS = 0;
        if (currentLine > 0) currentLine--;
        if (editLine > 0) editLine--;
        Redraw();
        EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject);

        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE + "\nCase 2";

      }
      else if (currentLine >= selectionE && down) { // Extend
        selectionE++;
        if (selectionE >= lines.Count) selectionE = lines.Count - 1;
        if (editLine < 28) {
          editLine++;
          EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject);
        }
        else
          currentLine++;
        if (currentLine >= lines.Count) currentLine = lines.Count - 1;
        Redraw();

        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE + "\nCase 3";
      }
      else if (currentLine <= selectionS && down) { // Reduce
        selectionS++;
        if (selectionS > selectionE) {
          int tmp = selectionS;
          selectionS = selectionE;
          selectionE = tmp;
        }
        if (editLine < 28) {
          editLine++;
          EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject);
        }
        else
          currentLine++;
        if (currentLine >= lines.Count) currentLine = lines.Count - 1;
        Redraw();

        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE + "\nCase 4";

      }
      else if (currentLine >= selectionE && up) { // Reduce
        selectionE--;
        if (selectionS > selectionE) {
          int tmp = selectionS;
          selectionS = selectionE;
          selectionE = tmp;
        }
        if (currentLine > 0) currentLine--;
        if (editLine > 0) editLine--;
        EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject);
        Redraw();

        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE + "\nCase 5";

      }

    }
    else { // ******************************* Normal *********************************************************************************************
      if ((up || (pup && autorepeat <= 0)) && !shift && currentLine > 0) {
        SaveLine();
        currentLine--;
        if (editLine > 0) editLine--;
        EditLines[editLine].Line.Select();
        EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject);
        autorepeat = up ? .4f : .06f;
        if (currentLine < selectionS - 1 || currentLine > selectionE + 1) {
          selectionS = -1;
          selectionE = -1;
        }
        Redraw();
      }
      else if ((down || (pdown && autorepeat <= 0) || enter) && !shift) {
        SaveLine();
        if (editLine < 28) editLine++;
        currentLine++;
        if (currentLine >= lines.Count) {
          LineData l = new LineData(0);
          lines.Add(l);
          EditLines[editLine].SetLine(currentLine, l);
        }
        else if (enter) {
          LineData l = new LineData(0);
          lines.Add(l);
          EditLines[editLine].SetLine(currentLine, l);
          Redraw(true);
        }
        
        EventSystem.current.SetSelectedGameObject(EditLines[editLine].Line.gameObject);
        autorepeat = down ? .4f : .06f;
        if (currentLine < selectionS - 1 || currentLine > selectionE + 1) {
          selectionS = -1;
          selectionE = -1;
        }
        Redraw();
      }
      else if (Input.GetKeyDown(KeyCode.PageUp)) {
        SaveLine();
        currentLine -= 31;
        if (currentLine < 0) currentLine = 0;
        // We may need to recalculate the currentLine from the editLine
        if (EditLines[editLine].linenum != -1) currentLine = EditLines[editLine].linenum;
        if (currentLine < selectionS - 1 || currentLine > selectionE + 1) {
          selectionS = -1;
          selectionE = -1;
        }
        Redraw();
      }
      else if (Input.GetKeyDown(KeyCode.PageDown)) {
        SaveLine();
        currentLine += 31;
        if (currentLine >= lines.Count) currentLine = lines.Count - 1;
        if (EditLines[editLine].linenum != -1) currentLine = EditLines[editLine].linenum;
        if (currentLine < selectionS - 1 || currentLine > selectionE + 1) {
          selectionS = -1;
          selectionE = -1;
        }
        Redraw();
      }

//      if (Input.anyKeyDown) //FIXME debug
//        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE;
      
    }


  }
  
  
  readonly Regex rgSyntaxHighlight = new Regex("(\\<color=#[0-9a-f]{6}\\>)|(\\</color\\>)|(\\<mark=#[0-9a-f]{8}\\>)|(\\</mark\\>)|(\\<b\\>)|(\\</b\\>)|(\\<i\\>)|(\\</i\\>)", RegexOptions.IgnoreCase);
  readonly Regex rgCommentML = new Regex("/\\*(?:(?!\\*/)(?:.|[\r\n]+))*\\*/", RegexOptions.IgnoreCase | RegexOptions.Multiline, System.TimeSpan.FromSeconds(5));
  readonly Regex rgCommentSL = new Regex("(//.*)$", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));
  readonly Regex rgBlockOpen = new Regex("(?<!//.*?)\\{", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(5));
  readonly Regex rgBlockOpenAlone = new Regex("^[\\s]*\\{[\\s]*", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(5));
  readonly Regex rgBlockClose = new Regex("(?<!//.*?)\\}", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(5));

  void SaveLine() {
    // Save, parse, and do the syntax highlight
    if (currentLine < 0 || currentLine >= lines.Count || editLine < 0 || editLine >= EditLines.Length) return;
    string cleanline = rgSyntaxHighlight.Replace(EditLines[editLine].Line.text.Trim(), "");

    if (lines[currentLine].line != cleanline) { // Save the line, if needed
      lines[currentLine].line = cleanline;
    }
    string var = lines[currentLine].line;
    if (string.IsNullOrEmpty(var)) {
      Result.text = "";
      return;
    }

    try {
      // Handle first comments
      string comments = "";
      Match m = rgCommentSL.Match(cleanline);
      if (m.Success) {
        comments = m.Value;
        cleanline = rgCommentSL.Replace(cleanline, "").Trim();
      }
      if (string.IsNullOrEmpty(cleanline)) {
        if (!string.IsNullOrEmpty(comments))
          Result.text = "<color=#70e688><mark=#30061880>" + comments + "</mark></color>";
        else
          Result.text = "";

        lines[EditLines[editLine].linenum].line = rgSyntaxHighlight.Replace(Result.text, "");
        EditLines[editLine].SetLine(EditLines[editLine].linenum, lines[EditLines[editLine].linenum], Result.text);
        return;
      }
      if (rgBlockClose.IsMatch(cleanline)) {
        Result.text = "";
        lines[EditLines[editLine].linenum].line = "}";
        EditLines[editLine].SetLine(EditLines[editLine].linenum, lines[EditLines[editLine].linenum]);
        FixIndentation();
        return;
      }
      if (rgBlockOpenAlone.IsMatch(cleanline)) {
        Result.text = "";
        lines[EditLines[editLine].linenum].line = "{";
        EditLines[editLine].SetLine(EditLines[editLine].linenum, lines[EditLines[editLine].linenum]);
        return;
      }

      // Check if we need multiple lines, we do only if we have an IF, FOR, WHILE (and they are not single command)
      bool hadOpenBlock = cp.RequiresBlock(cleanline);

      CodeNode res = cp.ParseLine(cleanline, variables, currentLine - 1, out string except);
      if (except != null) {
        cleanline = res.CN1?.Format(variables) + (hadOpenBlock ? "{" : "") + (comments.Length > 0 ? " <color=#70e688><mark=#30061880>" + comments + "</mark></color>" : "");
        lines[EditLines[editLine].linenum].line = rgSyntaxHighlight.Replace(cleanline, ""); ;
        EditLines[editLine].SetLine(EditLines[editLine].linenum, lines[EditLines[editLine].linenum], cleanline);
        Result.text = "<color=#ff2e00>" + except + "</color>";
      }
      else {
        Result.text = res.CN1?.Format(variables) + (hadOpenBlock ? "{" : "") + (comments.Length > 0 ? " <color=#70e688><mark=#30061880>" + comments + "</mark></color>" : "");
        lines[EditLines[editLine].linenum].line = rgSyntaxHighlight.Replace(Result.text, "");
        EditLines[editLine].SetLine(EditLines[editLine].linenum, lines[EditLines[editLine].linenum], Result.text);
      }

      // Update the indent
      FixIndentation();

    } catch (System.Exception e) {
      Result.text = "ERROR:\n" + e.Message;
    }

  }

  void FixIndentation() {
    int indent = 0;
    int increaseone = 0;
    for (int i = 0; i < lines.Count; i++) {
      string line = rgCommentSL.Replace(lines[i].line.Trim(), "").Trim();
      if (indent < 0) indent = 0;
      lines[i].indent = indent;
      if (rgBlockOpen.IsMatch(line)) {
        if (increaseone > 0) {
          indent -= increaseone;
          increaseone = 0;
          lines[i].indent = indent;
        }
        indent++; // open { -> increase
      }
      if (rgBlockClose.IsMatch(line)) { // close } -< decrease (and set also current line)
        indent--;
        if (indent < 0) indent = 0;
        lines[i].indent = indent;
      }
      if (cp.RequiresBlockAfter(line)) { // if/for/while without statement->increase just one
        increaseone++;
        indent++;
      }
      else if (!string.IsNullOrWhiteSpace(line)) {
        indent -= increaseone;
        increaseone = 0;
        if (indent < 0) indent = 0;
      }
    }
    for (int i = 0; i < EditLines.Length; i++) {
      int num = EditLines[i].linenum;
      if (num > -1 && num < lines.Count) EditLines[i].UpdateIndent(lines[num].indent);
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


  }


  public void LineSelected(int num) {
    bool toredraw = false;
    if (Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) {
      Debug.Log("multiselect");
      int one = editLine;
      int two = num;
      if (one > two) { int tmp = one; one = two; two = tmp; }
      selectionS = EditLines[one].linenum;
      selectionE = EditLines[two].linenum;
      toredraw = true;
    }

    editLine = num;
    int line = EditLines[num].linenum;
    if (line == -1) { // Go up until we will find the first valid line
      int numlinestoadd = 0;
      int numback = num;
      while(line == -1 && numback > 0) {
        numlinestoadd++;
        numback--;
        line = EditLines[numback].linenum;
      }
      if (line == -1) Debug.LogError("Huston we have a problem");
      for (int i = 0; i < numlinestoadd; i++) {
        LineData l = new LineData(0);
        lines.Add(l);
        EditLines[numback + 1 + i].SetLine(line + i + 1, l);
      }
    }
    currentLine = EditLines[num].linenum;

    if (toredraw) Redraw();
  }

  public void AlterBreakPoint(int num) {
    EditLines[num].ToggleBreakpoint();
  }

}

public class LineData {
  public int indent;
  public bool breakpoint;
  public string line; // Clean text
  public CodeNode node;

  public LineData(int i) {
    indent = i;
    breakpoint = false;
    line = "";
    node = null;
  }

  internal LineData Duplicate() {
    return new LineData(indent) { breakpoint = this.breakpoint, line = this.line, node = this.node };
  }
}
