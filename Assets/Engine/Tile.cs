using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour {
  public byte id;
  public byte rot;
  public RawImage sprite;
  public RectTransform rt;

  public void Rot() {
    switch (rot) {
      case 0:
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case 1:
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(-1, 1, 1);
        break;
      case 2:
        transform.rotation = Quaternion.Euler(0, 0, -90);
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case 3:
        transform.rotation = Quaternion.Euler(0, 0, -90);
        transform.localScale = new Vector3(-1, 1, 1);
        break;
      case 4:
        transform.rotation = Quaternion.Euler(0, 0, 180);
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case 5:
        transform.rotation = Quaternion.Euler(0, 0, 180);
        transform.localScale = new Vector3(-1, 1, 1);
        break;
      case 6:
        transform.rotation = Quaternion.Euler(0, 0, 90);
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case 7:
        transform.rotation = Quaternion.Euler(0, 0, 90);
        transform.localScale = new Vector3(-1, 1, 1);
        break;
    }

  }

}
