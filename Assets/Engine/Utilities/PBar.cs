using TMPro;
using UnityEngine;

public class PBar : MonoBehaviour {
  public GameObject[] Parts;
  public RectTransform Bar;
  public TextMeshProUGUI Text;
  static PBar pb;
  int max;
  string msg;

  private void Awake() {
    pb = this;
    for (int i = 0; i < 3; i++)
      Parts[i].SetActive(false);
  }

  public static object Show(string msg, int val, int max) {
    pb.max = max;
    pb.msg = msg;
    pb.Text.text = msg + ": " + (100 * val / max) + "%";
    pb.Bar.sizeDelta = new Vector2(632 * val / max, 42);
    for (int i = 0; i < 3; i++)
      pb.Parts[i].SetActive(true);
    return null;
  }

  public static void Hide() {
    for (int i = 0; i < 3; i++)
      pb.Parts[i].SetActive(false);
  }

  public static object Complete() {
    pb.Text.text = pb.msg + ": 100%";
    pb.Bar.sizeDelta = new Vector2(632, 42);
    return null;
  }

  public static object Progress(int val) {
    pb.Text.text = pb.msg + ": " + (100 * val / pb.max) + "%";
    pb.Bar.sizeDelta = new Vector2(632 * val / pb.max, 42);
    return null;
  }
}
