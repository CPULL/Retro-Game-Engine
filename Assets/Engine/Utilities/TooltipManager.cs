using System.Collections;
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
    if (inst.hidingCoroutine != null) {
      inst.StopCoroutine(inst.hidingCoroutine);
      inst.hidingCoroutine = null;
    }
  }

  public static void Hide(string txt) {
    if (inst.text != txt) return;
    inst.timeForHiding = .5f;
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
  }
}
