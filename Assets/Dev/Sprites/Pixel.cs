using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pixel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public int pos = 0;
  Action<int> ClickCall;
  Action<Pixel, bool> UseCall;
  Action<int> OverCall;
  [SerializeField] private Image img;
  [SerializeField] private Image border;
  Color32 Highlight = new Color32(255, 224, 223, 220);
  byte color;
  bool init = false;
  static Color32 BorderNormal = new Color32(206, 224, 223, 120);
  static Color32 BorderActive = new Color32(150, 250, 150, 220);
  bool active = false;

  public void Init(int p, Color32 c, Action<Pixel, bool> cb, Color32 defBorder) {
    pos = p;
    UseCall = cb;
    ClickCall = null;
    OverCall = null;
    if (img == null) img = GetComponent<Image>();
    img.color = c;
    BorderNormal = defBorder;
  }

  public void Init(int p, Color32 c, Action<int> cb, Action<int> oc) {
    pos = p;
    ClickCall = cb;
    OverCall = oc;
    if (img == null) img = GetComponent<Image>();
    img.color = c;
  }

  public void Init(int p, Action<int> cb, Action<int> oc) {
    pos = p;
    ClickCall = cb;
    OverCall = oc;
    img = GetComponent<Image>();
  }

  public void Set(byte c) {
    color = c;
    img.color = Col.GetColor(c);
    init = true;
  }
  public byte Get() {
    if (!init) return 255;
    return color;
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (eventData.button == PointerEventData.InputButton.Left) {
      ClickCall?.Invoke(pos);
      UseCall?.Invoke(this, true);
    }
    if (eventData.button == PointerEventData.InputButton.Right) {
      UseCall?.Invoke(this, false);
    }
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (border == null) return;
    border.color = Highlight;
    OverCall?.Invoke(pos);
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (border == null) return;
    border.color = active ? BorderActive : BorderNormal;
  }

  internal void SetBorderSprite(Sprite box) {
    border.sprite = box;
  }

  internal void Select() {
    border.color = Color.red;
  }

  internal void Deselect() {
    border.color = BorderNormal;
  }

  internal Color32 Get32() {
    return new Color32((byte)(255 * img.color.r), (byte)(255 * img.color.g), (byte)(255 * img.color.b), (byte)(255 * img.color.a));
  }
  internal void Set32(Color32 c) {
    img.color = c;
  }

  internal void InRange(bool activate) {
    active = activate;
    border.color = active ? BorderActive : BorderNormal;
  }
}
