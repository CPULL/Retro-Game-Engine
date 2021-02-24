using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour {
  static TooltipManager inst;
  public GameObject Container;
  public RectTransform RT;
  public TextMeshProUGUI Msg;
  string text = null;

  void Awake() {
    inst = this;
  }

  public static void Show(string txt) {
    inst.Container.SetActive(true);
    inst.text = txt;
    Vector2 pos = Input.mousePosition;
    pos.y = -1080 + pos.y + 32;
    pos.x += 32;
    if (pos.x > 1700) pos.x = 1700;
    if (pos.y > 0) pos.y = 0;
    inst.RT.anchoredPosition = pos;
    inst.Msg.text = txt;
  }

  public static void Hide(string txt) {
    if (inst.text != txt) return;
    inst.Container.SetActive(false);
  }
}
