using UnityEngine;
using UnityEngine.EventSystems;

public class ColorPicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public RectTransform ColorPickerH;
  public RectTransform ColorPickerV;
  public PaletteEditor editor;
  bool inside = false;
  Vector2 pos;

  void Update() {
    if (!inside) return;
    if (Input.GetMouseButton(0)) {
      Vector2 newpos = Input.mousePosition;
      if (pos != newpos) {
        pos = newpos;
        SetColor(pos);
      }
    }
  }

  public void OnPointerClick(PointerEventData ed) {
    SetColor(ed.position);
  }

  void SetColor(Vector2 pos) {
    float s = (pos.x - 1251.8f) / 255f;
    float v = (pos.y - 271.8f) / -255f;
    int x = (int)(s * 255 - .5f);
    int y = (int)(v * -255 + .5f);
    ColorPickerV.anchoredPosition = new Vector2(x, 0);
    ColorPickerH.anchoredPosition = new Vector2(0, y);
    editor.SetColorSV(s, 1 - v);
  }

  public void OnPointerEnter(PointerEventData eventData) {
    inside = true;
  }

  public void OnPointerExit(PointerEventData eventData) {
    inside = false;
  }
}
