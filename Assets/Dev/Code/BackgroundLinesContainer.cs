using UnityEngine;

public class BackgroundLinesContainer : MonoBehaviour {
  public RectTransform Me;
  public RectTransform Text;

  void Update() {
    Me.anchorMin = Text.anchorMin;
    Me.anchorMax = Text.anchorMax;
    Me.anchoredPosition = Text.anchoredPosition;
  }
}
