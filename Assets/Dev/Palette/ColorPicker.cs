using UnityEngine;
using UnityEngine.EventSystems;

public class ColorPicker : MonoBehaviour, IPointerClickHandler {
  public RectTransform ColorPickerH;
  public RectTransform ColorPickerV;
  public PaletteEditor editor;

  public void OnPointerClick(PointerEventData ed) {
    float s = (ed.position.x - 1251.8f) / 255f;
    float v = (ed.position.y - 271.8f) / -255f;
    int x = (int)(s * 255 -.5f);
    int y = (int)(v * -255 +.5f);
    ColorPickerV.anchoredPosition = new Vector2(x, 0);
    ColorPickerH.anchoredPosition = new Vector2(0, y);

    editor.SetColorSV(s, 1 - v);
  }
}
