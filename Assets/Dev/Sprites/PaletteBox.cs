using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class PaletteBox : MonoBehaviour, IPointerClickHandler {
  Vector2 mypos;
  public Slider hslider;
  public Slider vslider;

  void Start() {
    mypos = Camera.main.WorldToScreenPoint(transform.position);
    StartCoroutine(GetMyPos());
  }

  IEnumerator GetMyPos() {
    yield return new WaitForSeconds(.5f);
    mypos = Camera.main.WorldToScreenPoint(transform.position);
  }

  public void OnPointerClick(PointerEventData eventData) {
    int x = (int)(6 * (eventData.position.x - mypos.x) / 144);
    int y = 5 + (int)(6 * (eventData.position.y - mypos.y) / 144);
    hslider.value = x;
    vslider.value = y;
  }
}
