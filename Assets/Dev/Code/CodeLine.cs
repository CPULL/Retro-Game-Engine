using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeLine : MonoBehaviour {
  public int linenum;
  public TextMeshProUGUI Number;
  public TMP_InputField Line;
  public Image Breakpoint;

  public void SetLine(int num) {
    linenum = num;
    Number.text = "";
    Breakpoint.enabled = false;
    Line.SetTextWithoutNotify("");
  }
  public void CleanLine() {
    linenum = -1;
    Number.text = "";
    Breakpoint.enabled = false;
    Line.SetTextWithoutNotify("");
  }

  internal void SetLine(LineData data, bool opt) {
    Breakpoint.enabled = data.breakpoint;
    Line.SetTextWithoutNotify(data.LineCol(opt));
  }

  internal void SetLine(int num, LineData data, bool opt) {
    linenum = num;
    Number.text = (num + 1).ToString();
    Breakpoint.enabled = data.breakpoint;
    string formatted = data.LineCol(opt).Trim();
    string indentation = "";
    for (int i = 0; i < data.indent; i++)
      indentation += "  ";
    Line.SetTextWithoutNotify(indentation + formatted);
  }

  internal void SetLine(int num, LineData data, string formatted) {
    linenum = num;
    Number.text = (num + 1).ToString();
    Breakpoint.enabled = data.breakpoint;
    formatted = formatted.Trim();
    string indentation = "";
    for (int i = 0; i < data.indent; i++)
      indentation += "  ";
    Line.SetTextWithoutNotify(indentation + formatted);
    Line.SetTextWithoutNotify(formatted);
  }

  public void Clean() {
    linenum = -1;
    Number.text = "";
    Breakpoint.enabled = false;
    Line.SetTextWithoutNotify("");
  }

  internal void ToggleBreakpoint() {
    Breakpoint.enabled = !Breakpoint.enabled; // FIXME we need to alter it at line level
  }

  internal void UpdateIndent(int ind) {
    string formatted = Line.text.Trim();
    string indentation = "";
    for (int i = 0; i < ind; i++)
      indentation += "  ";
    Line.SetTextWithoutNotify(indentation + formatted);
  }
}

public class LineData {
  public int indent;
  public bool breakpoint;
  public bool insidecomment;
  private string lineNN; // original, clean text
  private string lineON; // optimized, clean text
  private string lineNC; // original, color coding
  private string lineOC; // optimized, color coding
  public CodeNode.CommentType commentT;
  public string comment;
  public bool toParse;

  public bool IsEmpty() {
    return string.IsNullOrWhiteSpace(lineNN) && string.IsNullOrWhiteSpace(lineON);
  }

  public string Line(bool opt) {
    return opt ? lineON : lineNN;
  }

  public string LineCol(bool opt) {
    if (string.IsNullOrWhiteSpace(lineNC)) return Line(opt);
    return opt ? lineOC : lineNC;
  }

  public LineData() {
    indent = 0;
    breakpoint = false;
    lineNN = "";
    lineON = "";
    lineNC = "";
    lineOC = "";
    toParse = false;
  }

  internal LineData Duplicate() {
    return new LineData() {
      breakpoint = breakpoint,
      lineNN = lineNN,
      lineON = lineON,
      lineNC = lineNC,
      lineOC = lineOC,
      toParse = toParse
    };
   }

  string RemoveDoubleSpaces(string s) {
    if (string.IsNullOrWhiteSpace(s)) return "";
    s = s.Trim(' ', '\t', '\r', '\n');
    if (s.Length == 1) return s;

    char p = s[0];
    string res = "";
    res += p;
    bool inQuotes = (p == '"');
    for (int i = 1; i < s.Length; i++) {
      char c = s[i];
      if (c == ' ' || c == '\t' || c == '\n' || c == '\r') c = ' ';

      if (p == ' ' && c == ' ' && !inQuotes) continue;
      if (c == '"') inQuotes = !inQuotes;
      p = c;
      res += c;
    }
    return res;
  }

  internal bool Same(string line) {
    string t = RemoveDoubleSpaces(line);
    return lineNN.Trim() == t || lineON.Trim() == t;
  }

  internal void Set(string line) {
    line = RemoveDoubleSpaces(line);
    toParse = (lineNN != line || lineON != line);
    lineNN = line;
    lineON = line;
    lineNC = line;
    lineOC = line;
  }

  internal void Set(string l, string c) {
    l = RemoveDoubleSpaces(l);
    toParse = true;
    lineNN = l;
    lineON = l;
    lineNC = c;
    lineOC = c;
  }

  internal void SetParsed(string ln, string lo, string lnc, string loc) {
    toParse = false;
    lineNN = RemoveDoubleSpaces(ln);
    lineON = RemoveDoubleSpaces(lo);
    lineNC = lnc;
    lineOC = loc;
  }

  internal void SetParsed(string l, string c) {
    SetParsed(l, l, c, c);
  }

  internal void SetComments(string c, CodeNode.CommentType ct) {
    comment = c;
    commentT = ct;
  }
}