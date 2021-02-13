using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeEditor : MonoBehaviour {
  readonly List<string> lines = new List<string>();
  public CodeLine[] EditLines;
  public Scrollbar VerticalCodeBar;
  public RectTransform SelectionRT;
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
    SetScroll();
  }

  float autorepeat = 0;
  int selectionS = -1;
  int selectionE = -1;
  string copied = "";
  private void Update() {
    if (autorepeat > 0) autorepeat -= Time.deltaTime;

    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
      bool down = Input.GetKeyDown(KeyCode.DownArrow);
      bool up = Input.GetKeyDown(KeyCode.UpArrow);
      // Nothing selected, select current line and move up or down
      if ((down || up) && (selectionS == -1 || selectionE == -1)) {
        selectionS = currentLine;
        selectionE = currentLine;
      }
      else if (currentLine <= selectionS && up) { // Extend
        selectionS--;
        if (selectionS < 0) selectionS = 0;
        ScrollLines(false);
      }
      else if (currentLine >= selectionE && down) { // Extend
        selectionE++;
        if (selectionE >= lines.Count) selectionE = lines.Count - 1;
        ScrollLines(true);
      }
      else if (currentLine <= selectionS && down) { // Reduce
        selectionS++;
        if (selectionS > selectionE) {
          selectionS = -1;
          selectionE = -1;
        }
        ScrollLines(true);
      }
      else if (currentLine >= selectionE && up) { // Reduce
        selectionE--;
        if (selectionS > selectionE) {
          selectionS = -1;
          selectionE = -1;
        }
        ScrollLines(false);
      }

      if (selectionS != -1 && selectionE != -1) {
        // Find the editrow with the startid, if less than 0 set it at 0, if more than 30 set it as 30
        // Find the editrow with the endid, if less than 0 set it at 0, if more than 30 set it as 30
        int rowStart = -1, rowEnd = -1;
        for (int i = 0; i < 31; i++) {
          if (EditLines[i].linenum == selectionS) rowStart = i;
          if (EditLines[i].linenum == selectionE) rowEnd = i;
        }

        if (rowStart == -1 && rowEnd == -1) SelectionRT.sizeDelta = new Vector2(0, 33);
        else if (rowStart == -1 && rowEnd != -1) {
          SelectionRT.sizeDelta = new Vector2(1280, 33 * (rowEnd + 1));
          SelectionRT.anchoredPosition = new Vector2(0, 0);
        }
        else if (rowStart != -1 && rowEnd == -1) {
          SelectionRT.sizeDelta = new Vector2(1280, 33 * (30 - rowStart));
          SelectionRT.anchoredPosition = new Vector2(0, -33 * rowStart);
        }
        else  {
          SelectionRT.sizeDelta = new Vector2(1280, 33 * (1 + rowEnd - rowStart));
          SelectionRT.anchoredPosition = new Vector2(0, -33 * rowStart);
        }

        foreach(CodeLine cl in EditLines)
          if (cl.Line.isFocused) {
            cl.Line.ReleaseSelection();
          }
      }
    }
    else {
      if (Input.GetKeyDown(KeyCode.DownArrow)) {
        ScrollLines(true);
        autorepeat = .12f;
      }
      else if (Input.GetKey(KeyCode.DownArrow) && autorepeat <= 0) {
        ScrollLines(true);
        autorepeat = .08f;
      }
      if (Input.GetKeyDown(KeyCode.UpArrow) && currentLine > 0) {
        ScrollLines(false);
        autorepeat = .12f;
      }
      else if (Input.GetKey(KeyCode.UpArrow) && currentLine > 0 && autorepeat <= 0) {
        ScrollLines(false);
        autorepeat = .08f;
      }
    }

    if (Input.GetKeyDown(KeyCode.PageDown)) {
      currentLine += 30;
      if (currentLine >= lines.Count) currentLine = lines.Count - 1;
      FullDraw();
    }
    if (Input.GetKeyDown(KeyCode.PageUp)) {
      currentLine -= 30;
      if (currentLine < 0) currentLine = 0;
      FullDraw();
    }

    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
      if (Input.GetKeyDown(KeyCode.D)) {
        string line = lines[currentLine];
        lines.Insert(currentLine, line);
        currentLine++;
        FullDraw();
      }
      if (Input.GetKeyDown(KeyCode.Delete) && lines.Count > 1) {
        lines.RemoveAt(currentLine);
        if (currentLine >= lines.Count) currentLine = lines.Count - 1;
        FullDraw();
      }
      if (Input.GetKeyDown(KeyCode.C) && selectionS != -1 && selectionE != -1) {
        copied = "";
        for (int line = selectionS; line <= selectionE; line++) {
          copied += lines[line];
          if (line != selectionE) copied += "\n";
        }
        dbg.text = copied;
        selectionS = -1;
        selectionE = -1;
        SelectionRT.sizeDelta = new Vector2(1280, 0);
      }
      if (Input.GetKeyDown(KeyCode.X) && selectionS != -1 && selectionE != -1) {
        copied = "";
        for (int line = selectionS; line <= selectionE; line++) {
          copied += lines[line];
          if (line != selectionE) copied += "\n";
        }
        dbg.text = copied;
        for (int line = selectionS; line <= selectionE; line++) {
          lines.RemoveAt(selectionS);
        }
        // We have to update all the contents of editLines that were visible
        for (int i = 0; i < EditLines.Length; i++) {
          if (EditLines[i].linenum >= selectionS && EditLines[i].linenum <= selectionE) {
            int num = EditLines[i].linenum - selectionS + 1;
            if (num < lines.Count) EditLines[i].SetLine(num, lines[num]);
            else EditLines[i].Clean();
          }
        }

        // We need to update all 

        if (currentLine >= lines.Count) currentLine = lines.Count - 1;
        editLine = 0;
        selectionS = -1;
        selectionE = -1;
        SelectionRT.sizeDelta = new Vector2(1280, 0);
        FullDraw();
      }
    }

  }

  public void LineSelected(int num) {
    if (lines[currentLine] != EditLines[editLine].Line.text) {
      lines[currentLine] = EditLines[editLine].Line.text;
    }
    currentLine = EditLines[num].linenum;
    editLine = num;
  }

  public void LineDeselected(int num) {
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
    if (editLine < 0) editLine = 0;
    if (editLine > 22) {
      if (lines.Count < 23)
        editLine = lines.Count - 1;
      else
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
        if (editLine < 0) editLine = 0;
        if (editLine > 30) editLine = 30;
        if (currentLine < 0) currentLine = 0;
        if (currentLine >= lines.Count) currentLine = lines.Count - 1;
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
      if (editLine < 0) editLine = 0;
      if (editLine > 30) editLine = 30;
      if (currentLine < 0) currentLine = 0;
      if (currentLine >= lines.Count) currentLine = lines.Count - 1;
      EditLines[editLine].SetLine(currentLine, lines[currentLine]);
      EditLines[editLine].Line.Select();
    }

    SetScroll();
  }

  void FullDraw() {
    if (editLine >= lines.Count) editLine = lines.Count - 1;
    if (currentLine < 8) {
      for (int line = 0; line < 31; line++) {
        if (line >= lines.Count) {
          EditLines[line].Clean();
        }
        else {
          EditLines[line].SetLine(line, lines[line]);
        }
      }
      editLine = currentLine;
      EditLines[editLine].Line.Select();
    }
    else {
      for (int line = editLine; line >= 0; line--) {
        int pos = currentLine - (editLine - line);
        if (pos >= 0 && pos < lines.Count)
          EditLines[line].SetLine(pos, lines[pos]);
        else
          EditLines[line].Clean();
      }

      for (int line = editLine + 1; line < 31; line++) {
        int pos = currentLine + (line - editLine);
        if (pos >= 0 && pos < lines.Count)
          EditLines[line].SetLine(pos, lines[pos]);
        else
          EditLines[line].Clean();
      }
    }
    SetScroll();
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
    Debug.Log("Scrolling " + currentLine);
    FullDraw();
  }

}

/*

Going up to the first line moves to the second line

Ctrl+V, +X
Ctrl+G jump
Ctrl+F find
Ctrl+H replace

Compile a line as soon it is completed -> show errors on the side
Save code, text and binary
Load code, text and binary
 */ 