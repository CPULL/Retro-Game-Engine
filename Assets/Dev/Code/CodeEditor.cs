using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeEditor : MonoBehaviour {
  readonly List<string> lines = new List<string>();
  public CodeLine[] EditLines;
  public Scrollbar VerticalCodeBar;
  public TextMeshProUGUI dbg;

  int currentLine = 0;
  int editLine = 0;

  private void Start() {
    foreach(CodeLine cl in EditLines) {
      cl.Number.text = "";
      cl.Line.SetTextWithoutNotify("");
      cl.Line.onFocusSelectAll = false;
      cl.Line.restoreOriginalTextOnEscape = true;
    }
    EditLines[0].SetLine(0, "ZERO");
    lines.Add("ZERO");

    for (int i = 1; i < 40; i++) {
      lines.Add("[" + i + "] " + (i%2==0? (i * i * i * i + i * i + i).ToString("X4") : ""));
      if (i < EditLines.Length) {
        EditLines[i].SetLine(i, lines[i]);
      }
    }

  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.DownArrow)) {
      ScrollLines(true);
    }
    if (Input.GetKeyDown(KeyCode.UpArrow) && currentLine > 0) {
      ScrollLines(false);
    }

    if (Input.GetKeyDown(KeyCode.F1)) {

    }
    if (Input.GetKeyDown(KeyCode.F3)) {
      EditLines[editLine].Line.stringPosition = EditLines[editLine].Line.stringPosition - 1;
    }
    if (Input.GetKeyDown(KeyCode.F2)) {
      EditLines[editLine].Line.stringPosition = EditLines[editLine].Line.stringPosition + 1;
    }
  }

  public void LineSelected(int num) {
    currentLine = EditLines[num].linenum;
    editLine = num;
    dbg.text = "edit " + num.ToString() + " line " + currentLine;
  }

  public void LineDeselected(int num) {
    dbg.text = num.ToString() + " desel";
  }

  void ScrollLines(bool down) {
    // Save the editline
    if (lines[currentLine] != EditLines[editLine].Line.text) {
      lines[currentLine] = EditLines[editLine].Line.text;
    }
    if (down) {
      currentLine++;
      if (currentLine >= lines.Count) lines.Add("");
    }
    else {
      currentLine--;
    }

    // Change editline. If it is <8 then scroll up, if it is >22 then scroll down
    if (down) editLine++; else editLine--;

    if (editLine < 8) {
      // What is our line number?
      int linenum = EditLines[editLine].linenum;
      if (linenum > editLine) {
        editLine = 7;
        int pos = linenum - 7;
        for (int line = 0; line < 31; line++) {
          int lp = line + pos;
          if (lp >= 0 && lp < lines.Count) {
            EditLines[line].SetLine(lp, lines[lp]);
          }
          else {
            EditLines[line].Clean();
          }
        }
      }
      else {
        EditLines[editLine].SetLine(currentLine, lines[currentLine]);
        EditLines[editLine].Line.Select();
      }

    }
    else if (editLine > 22) {
      editLine = 22;
      for (int line = 0; line < 31; line++) {
        int pos = currentLine - editLine + line;
        if (pos >= 0 && pos < lines.Count) {
          EditLines[line].SetLine(pos, lines[pos]);
        }
        else {
          EditLines[line].Clean();
        }
      }
    }
    else { // Do not scroll, just select the line
      EditLines[editLine].SetLine(currentLine, lines[currentLine]);
      EditLines[editLine].Line.Select();
    }

    if (lines.Count < 31) {
      VerticalCodeBar.numberOfSteps = 0;
      VerticalCodeBar.SetValueWithoutNotify(0);
      VerticalCodeBar.size = 1;
    }
    else {
      VerticalCodeBar.numberOfSteps = lines.Count - 31;
      VerticalCodeBar.size = 1 / VerticalCodeBar.numberOfSteps;
      VerticalCodeBar.SetValueWithoutNotify(currentLine);
    }

  }

}

/*
Courier font
Ctrl+D to duplicate line
Ctrl+Del to remove line
Multi line Selection 
Ctrl+C, +V, +X
Ctrl+G jump
Ctrl+F find
Ctrl+H replace

 
 */ 