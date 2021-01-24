using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileInMap : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public byte id;
  public byte x;
  public byte y;
  public Image border;
  public RawImage img;
  Color32 Normal;
  Color32 Over = new Color32(255, 180, 25, 255);
  public System.Action<TileInMap> CallBack;
  Color32 Selected = new Color32(255, 255, 0, 255);
  bool selected = false;

  void Start() {
    Normal = border.color;
  }

  public void OnPointerClick(PointerEventData eventData) {
    CallBack?.Invoke(this);
    selected = true;
    border.color = Selected;
  }

  public void OnPointerEnter(PointerEventData eventData) {
    border.color = Over;
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (selected)
      border.color = Selected;
    else
      border.color = Normal;
  }

  public void Deselect() {
    selected = false;
    border.color = Normal;
  }
}
