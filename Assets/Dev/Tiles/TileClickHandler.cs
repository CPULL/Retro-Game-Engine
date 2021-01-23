using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public byte id;
  public byte x;
  public byte y;
  public Image border;
  public Image img;
  Color32 Normal;
  Color32 Over = new Color32(255, 180, 25, 255);

  void Start() {
    Normal = border.color;
  }

  public void OnPointerClick(PointerEventData eventData) {
    throw new System.NotImplementedException();
  }

  public void OnPointerEnter(PointerEventData eventData) {
    border.color = Over;
  }

  public void OnPointerExit(PointerEventData eventData) {
    border.color = Normal;
  }
}
