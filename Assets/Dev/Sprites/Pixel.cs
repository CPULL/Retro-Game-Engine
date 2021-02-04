﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pixel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public int pos = 0;
  Action<int> ClickCall;
  Action<int> OverCall;
  [SerializeField] private Image img;
  [SerializeField] private Image border;
  Color32 Highlight = new Color32(255, 224, 223, 220);
  Color32 Normal = new Color32(206, 224, 223, 120);
  byte color;
  bool init = false;
  static Color32 BorderNormal = new Color32(206, 224, 223, 120);

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
    if (eventData.button == 0) ClickCall(pos);
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

  internal void SetBorderSprite(Sprite box) {
    border.sprite = box;
  }

  internal void Select() {
    border.color = Color.red;
  }

  internal void Deselect() {
    border.color = BorderNormal;
  }
}
