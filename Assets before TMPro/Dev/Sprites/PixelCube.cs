using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PixelCube : MonoBehaviour, IPointerClickHandler {
  public int r;
  public int g;
  public int b;
  Action<int, int, int> ClickCall;
  public Image img;

  void Awake() {
    img = GetComponent<Image>();
    if (img == null) img = GetComponent<SkewedImageT>();
    if (img == null) img = GetComponent<SkewedImageR>();
  }

  public void Init(Action<int, int, int> cb) {
    ClickCall = cb;
  }


  public void OnPointerClick(PointerEventData eventData) {
    if (eventData.button == 0) ClickCall(r, g, b);
  }

}
