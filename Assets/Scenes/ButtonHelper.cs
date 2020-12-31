using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

  public Loader loader;
  public int pos;

  public void OnPointerEnter(PointerEventData eventData) {
    loader.ShowHelp(pos);
  }

  public void OnPointerExit(PointerEventData eventData) {
    loader.ShowHelp(-1);
  }
}
