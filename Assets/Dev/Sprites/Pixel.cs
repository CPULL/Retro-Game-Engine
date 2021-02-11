using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pixel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public int pos = 0;
  Action<Pixel, bool> ClickCall;
  Action<Pixel> OverCall;
  [SerializeField] private RawImage img;
  [SerializeField] private Image border;
  static Color32 BorderNormal = new Color32(206, 224, 223, 120);
  static Color32 BorderOver = new Color32(150, 250, 150, 220);
  static Color32 BorderSelected = new Color32(150, 250, 150, 220);
  bool active = false;
  byte colindex = 255;


  /*
   Init with pos and color
   set and get color as Color32 and as byte
   call click actions and handle over
    handle border size and on/off
   */



  internal void InitBasic(int p, Action<Pixel, bool> cb) {
    pos = p;
    if (img == null) img = GetComponent<RawImage>();
    img.material = null;
    ClickCall = cb;
  }

  internal void InitBasic(int p, Color32 col, Action<Pixel, bool> cb) {
    pos = p;
    if (img == null) img = GetComponent<RawImage>();
    img.material = null;
    img.color = col;
    ClickCall = cb;
  }

  public void Init(int p, byte c, Action<Pixel, bool> cb, Action<Pixel> oc, Color32 borderN, Color32 borderO, Color32 borderS) {
    pos = p;
    ClickCall = cb;
    OverCall = oc;
    colindex = c;
    if (img == null) img = GetComponent<RawImage>();
    BorderNormal = borderN;
    BorderOver = borderO;
    BorderSelected = borderS;
    img.texture = Col.GetPaletteTexture(c);
    border.color = borderN;
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (eventData.button == PointerEventData.InputButton.Left) {
      ClickCall?.Invoke(this, true);
    }
    if (eventData.button == PointerEventData.InputButton.Right) {
      ClickCall?.Invoke(this, false);
    }
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (border == null) return;
    border.color = BorderOver;
    OverCall?.Invoke(this);
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (border == null) return;
    border.color = active ? BorderSelected : BorderNormal;
  }

  internal void SetBorderSprite(Sprite box) {
    border.sprite = box;
    border.color = BorderNormal;
  }

  internal void Select() {
    border.color = BorderSelected;
  }

  internal void Deselect() {
    border.color = BorderNormal;
  }

  internal Color32 Get32() {
    return Col.GetPaletteColor(colindex);
  }
  internal void Set32(Color32 c) {
    byte pos = Col.GetBestColor(c);
    img.texture = Col.GetPaletteTexture(pos);
    colindex = pos;
  }

  internal byte Get() {
    return colindex;
  }
  internal void Set(byte pos) {
    img.texture = Col.GetPaletteTexture(pos);
    colindex = pos;
  }

  internal void InRange(bool activate) {
    active = activate;
    border.color = active ? BorderSelected : BorderNormal;
  }
}
