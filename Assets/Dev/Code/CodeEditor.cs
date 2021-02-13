﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodeEditor : MonoBehaviour {
  readonly List<string> lines = new List<string>();
  public CodeLine[] EditLines;
  public Scrollbar VerticalCodeBar;
  public RectTransform SelectionRT;
  public TextMeshProUGUI dbg;

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
    EditLines[0].SetLine(0, "");
    lines.Add("");

    for (int i = 1; i < -45; i++) {
      lines.Add("[" + i + "] " + (i%2==0? (i * i * i * i + i * i + i).ToString("X4") : ""));
    }
    for (int i = 0; i < EditLines.Length; i++) {
      if (i < lines.Count) EditLines[i].SetLine(i, lines[i]);
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
          if (lines[pos] != EditLines[i].Line.text) {
            lines[pos] = EditLines[i].Line.text;
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
      // Clear and duplicate
      if (Input.GetKeyDown(KeyCode.D)) {
        SaveLine();
        string line = lines[currentLine];
        lines.Insert(currentLine, line);
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
            lines.Insert(currentLine, rows[i].Trim(' ', '\t', '\n', '\r'));
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

        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE + "\nCase 0";

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
        if (currentLine >= lines.Count) lines.Add("");
        else if (enter) {
          lines.Insert(currentLine, "");
          Redraw(true);
        }
        
        EditLines[editLine].Line.Select();
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

      if (Input.anyKeyDown) //FIXME debug
        dbg.text = "CL: " + currentLine + " / EL: " + editLine + "\nSS:" + selectionS + " -> SE:" + selectionE;
      
    }


  }

  void SaveLine() {
    if (currentLine < 0 || currentLine >= lines.Count || editLine < 0 || editLine >= EditLines.Length) return;
    // Save the line if needed
    if (lines[currentLine] != EditLines[editLine].Line.text)
      lines[currentLine] = EditLines[editLine].Line.text;
  }

  public void LineSelected(int num) {
    editLine = num;
    int line = EditLines[num].linenum;
    if (line == -1) return;
    currentLine = line;
  }

  public void LineDeselected(int num) {
  }







}

/*

Enter should add an empty line where it is clicked (after current line)
Ctrl+F find
Ctrl+H replace

Compile a line as soon it is completed -> show errors on the side
Save code, text and binary
Load code, text and binary
 */ 