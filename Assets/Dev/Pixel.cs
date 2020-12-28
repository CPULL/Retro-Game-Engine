using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pixel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public int pos = 0;
  Action<int> ClickCall;
  public Image img;
  public Image border;
  Color32 Highlight = new Color32(255, 224, 223, 220);
  Color32 Normal = new Color32(206, 224, 223, 120);

  public void Init(int p, Color32 c, Action<int> cb) {
    pos = p;
    ClickCall = cb;
    img.color = c;
  }

  public void Init(int p, Action<int> cb) {
    pos = p;
    ClickCall = cb;
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
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (border == null) return;
    border.color = Normal;
  }
}
