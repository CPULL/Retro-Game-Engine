using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TilemapEditor : MonoBehaviour {
  public Transform map;
  public GameObject TileTemplate;

  private void Start() {
    AlterMapSize(false);
  }

  /*
   View map
  Change tiles in the map from palette
  flip and rotate tiles
  fill tiles
  scroll with simulated screen-view
  Edit a tile (switch to sprite editor?)
   
   
   
   */

  public TMP_InputField MapSizeField;
  public Slider MapSizeW;
  public Slider MapSizeH;
  Coroutine updateMapSize;
  public GridLayoutGroup mapGrid;
  int w = 24, h = 16;
  public void AlterMapSize(bool fromInputField) {
    if (fromInputField) {
      string val = MapSizeField.text.Trim().ToLowerInvariant();
      int pos1 = val.IndexOf(' ');
      int pos2 = val.IndexOf('x');
      if (pos1 == -1 && pos2 == -1) {
        MapSizeField.SetTextWithoutNotify(MapSizeW.value + "x" + MapSizeH.value);
        return;
      }
      int pos = pos1;
      if (pos2 != -1 && (pos2 < pos1 || pos1 == -1)) pos = pos2;
      if (!int.TryParse(val.Substring(0, pos).Trim(), out w)) {
        MapSizeField.SetTextWithoutNotify(MapSizeW.value + "x" + MapSizeH.value);
        return;
      }
      if (!int.TryParse(val.Substring(pos + 1).Trim(), out h)) {
        MapSizeField.SetTextWithoutNotify(MapSizeW.value + "x" + MapSizeH.value);
        return;
      }
      MapSizeW.SetValueWithoutNotify(w);
      MapSizeH.SetValueWithoutNotify(h);
    }
    else {
      MapSizeField.SetTextWithoutNotify(MapSizeW.value + "x" + MapSizeH.value);
    }
    if (updateMapSize == null) {
      updateMapSize = StartCoroutine(UpdateMapSize());
    }
  }


  IEnumerator UpdateMapSize() {
    yield return new WaitForSeconds(1);
    w = (int)MapSizeW.value;
    h = (int)MapSizeH.value;
    mapGrid.constraintCount = w;
    int size = w * h;
    for (int i = 0; i < map.childCount - size; i++) {
      Destroy(map.GetChild(size + i).gameObject);
    }
    for (int i = map.childCount; i < size; i++) {
      TileClickHandler t = Instantiate(TileTemplate, map).GetComponent<TileClickHandler>();
      t.id = 0;
      t.gameObject.SetActive(true);
    }

    for (int y = 0; y < MapSizeH.value; y++)
      for (int x = 0; x < MapSizeW.value; x++) {
        TileClickHandler t = map.GetChild(x + w * y).GetComponent<TileClickHandler>();
        t.x = (byte)x;
        t.y = (byte)y;
      }
    updateMapSize = null;
  }
}
