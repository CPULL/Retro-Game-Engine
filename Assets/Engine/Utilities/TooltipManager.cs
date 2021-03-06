using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour {
  static TooltipManager inst;
  public GameObject Container;
  public RectTransform RT;
  public TextMeshProUGUI Msg;
  string text = null;
  bool visible = false;
  Vector2 originalPos = Vector2.zero;

  void Awake() {
    inst = this;
  }

  public static void Show(string txt) {
    if (inst.text == txt) return;
    inst.Container.SetActive(true);
    inst.text = txt;
    Vector2 pos = Input.mousePosition;
    pos.y = -1080 + pos.y + 32;
    if (pos.x > 960) pos.x -= 32;
    else pos.x += 32;
    if (pos.x > 1650) pos.x = 1650;
    if (pos.y > 0) pos.y = 0;
    inst.RT.anchoredPosition = pos;
    inst.originalPos = pos;
    inst.Msg.text = txt;
    if (inst.hidingCoroutine != null) {
      inst.StopCoroutine(inst.hidingCoroutine);
      inst.hidingCoroutine = null;
    }
    inst.visible = true;
  }

  public static void Hide(string txt) {
    if (inst.text != txt) return;
    inst.timeForHiding = 5.5f; // FIXME
    if (inst.hidingCoroutine == null) inst.hidingCoroutine = inst.StartCoroutine(inst.HideDelayed());
  }

  float timeForHiding = 1;
  Coroutine hidingCoroutine = null;
  readonly WaitForSeconds delay = new WaitForSeconds(.25f);
  IEnumerator HideDelayed() {
    while (timeForHiding > 0) {
      yield return delay;
      timeForHiding -= .25f;
    }
    inst.Container.SetActive(false);
    hidingCoroutine = null;
    visible = false;
  }

  private void Update() {
    if (!visible) return;

    Vector2 pos = originalPos;
    float y = Input.mousePosition.y - pos.y - 1080 + 32;
    if (0 < y && pos.y < 80) {
      if (Input.mousePosition.x > 960) pos.x -= 180;
      else pos.x += 180;
      RT.anchoredPosition = pos;
    }
  }
}
