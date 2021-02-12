using TMPro;
using UnityEngine;

public class CodeLine : MonoBehaviour {
  public int linenum;
  public TextMeshProUGUI Number;
  public TMP_InputField Line;

  public void SetLine(int num, string txt) {
    linenum = num;
    string val = num.ToString();
    while (val.Length < 5) val = " " + val;
    Number.text = val;
    Line.SetTextWithoutNotify(txt);
  }

  public void Clean() {
    linenum = -1;
    Number.text = "";
    Line.SetTextWithoutNotify("");
  }
}
