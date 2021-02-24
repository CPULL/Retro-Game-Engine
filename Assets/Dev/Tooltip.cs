using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  public string TooltipMsg;
  public void OnPointerEnter(PointerEventData eventData) {
    TooltipManager.Show(TooltipMsg);
  }

  public void OnPointerExit(PointerEventData eventData) {
    //TooltipManager.Hide(TooltipMsg);
  }
}
