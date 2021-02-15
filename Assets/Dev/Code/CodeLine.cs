using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeLine : MonoBehaviour {
  public int linenum;
  public TextMeshProUGUI Number;
  public TMP_InputField Line;
  public Image Breakpoint;
  public int indent;
  public bool breakpoint = false;
  public string line;

  public void SetLine(int num) {
    linenum = num;
    Number.text = "";
    Breakpoint.enabled = false;
    indent = 0;
    line = "";
    Line.SetTextWithoutNotify("");
  }

  internal void SetLine(int num, LineData data) {
    linenum = num;
    Number.text = num.ToString();
    Breakpoint.enabled = breakpoint;
    indent = data.indent;
    string indentation = "";
    for (int i = 0; i < data.indent; i++)
      indentation += "  ";
    if (line != data.line) {
      line = data.line;
      Line.SetTextWithoutNotify(indentation + line);
    }
  }

  internal void SetLine(int num, LineData data, string formatted) {
    linenum = num;
    Number.text = num.ToString();
    Breakpoint.enabled = breakpoint;
    indent = data.indent;
    string indentation = "";
    for (int i = 0; i < data.indent; i++)
      indentation += "  ";
    line = data.line;
    Line.SetTextWithoutNotify(indentation + formatted);
  }

  public void Clean() {
    linenum = -1;
    Number.text = "";
    Breakpoint.enabled = false;
    Line.SetTextWithoutNotify("");
    line = "";
  }

  internal void ToggleBreakpoint() {
    breakpoint = !breakpoint;
    Breakpoint.enabled = breakpoint;
  }

  internal void UpdateIndent(int ind) {
    if (indent == ind) return;
    string formatted = Line.text.Trim();
    string indentation = "";
    for (int i = 0; i < ind; i++)
      indentation += "  ";
    Line.SetTextWithoutNotify(indentation + formatted);

  }
}
