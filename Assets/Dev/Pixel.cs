using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pixel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public int pos = 0;
  Action<int> ClickCall;
  Action<int> OverCall;
  public Image img;
  public Image border;
  Color32 Highlight = new Color32(255, 224, 223, 220);
  Color32 Normal = new Color32(206, 224, 223, 120);

  public void Init(int p, Color32 c, Action<int> cb, Action<int> oc) {
    pos = p;
    ClickCall = cb;
    OverCall = oc;
    img.color = c;
  }

  public void Init(int p, Action<int> cb, Action<int> oc) {
    pos = p;
    ClickCall = cb;
    OverCall = oc;
    img = GetComponent<Image>();
  }

  public void Set(Color32 c) {
    img.color = c;
  }

  public void OnPointerClick(PointerEventData eventData) {
    ClickCall(pos);
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (border == null) return;
    border.color = Highlight;
    OverCall?.Invoke(pos);
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (border == null) return;
    border.color = Normal;
  }
}
