using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PianoKeyboard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {
  public Color32 normalColor;
  public string note;
  public Image image;
  Color32 pressedColor = new Color32(10, 200, 240, 255);
  Color32 overColor = new Color32(10, 200, 240, 255);
  public WaveformEditor we;

  void Start() {
    image = GetComponent<Image>();
    normalColor = image.color;
    overColor = normalColor;
    overColor.g = 200;
    overColor.b = 150;
    note = transform.GetChild(0).GetComponent<Text>().text.Trim();
  }

  public void OnPointerDown(PointerEventData eventData) {
    image.color = pressedColor;
    we.StartNote(note);
  }

  public void OnPointerUp(PointerEventData eventData) {
    image.color = normalColor;
    we.StopNote(note);
  }

  public void OnPointerEnter(PointerEventData eventData) {
    image.color = overColor;
  }

  public void OnPointerExit(PointerEventData eventData) {
    image.color = normalColor;
  }
}
