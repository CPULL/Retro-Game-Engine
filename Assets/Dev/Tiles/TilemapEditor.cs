using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TilemapEditor : MonoBehaviour {
  public Transform map;
  public GameObject TileInMapTemplate;
  public GameObject TileInPaletteTemplate;
  public Texture2D emptyTexture;

  Dictionary<byte, TileInPalette> Palette = new Dictionary<byte, TileInPalette>();

  private void Start() {
    AlterMapSize(false);
  }

  /*
   View map
  Change tiles in the map from palette
  flip and rotate tiles
  fill tiles
  Edit a tile (switch to sprite editor?)
   
   
   */

  public TMP_InputField MapSizeField;
  public Slider MapSizeW;
  public Slider MapSizeH;
  Coroutine updateMapSize;
  public TMP_InputField TileSizeField;
  public Slider TileSizeW;
  public Slider TileSizeH;
  public GridLayoutGroup mapGrid;
  public GridLayoutGroup tilesGrid;
  int w = 24, h = 16;
  int tw = 16, th = 16;
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
      TileInMap t = Instantiate(TileInMapTemplate, map).GetComponent<TileInMap>();
      t.id = 0;
      t.CallBack = SelectTileInMap;
      t.img.texture = emptyTexture;
      t.gameObject.SetActive(true);
    }

    for (int y = 0; y < MapSizeH.value; y++)
      for (int x = 0; x < MapSizeW.value; x++) {
        TileInMap t = map.GetChild(x + w * y).GetComponent<TileInMap>();
        t.x = (byte)x;
        t.y = (byte)y;
      }
    updateMapSize = null;
  }

  public void AlterTileSize(bool fromInputField) {
    if (fromInputField) {
      string val = TileSizeField.text.Trim().ToLowerInvariant();
      int pos1 = val.IndexOf(' ');
      int pos2 = val.IndexOf('x');
      if (pos1 == -1 && pos2 == -1) {
        TileSizeField.SetTextWithoutNotify(TileSizeW.value + "x" + TileSizeH.value);
        return;
      }
      int pos = pos1;
      if (pos2 != -1 && (pos2 < pos1 || pos1 == -1)) pos = pos2;
      if (!int.TryParse(val.Substring(0, pos).Trim(), out w)) {
        TileSizeField.SetTextWithoutNotify(TileSizeW.value + "x" + TileSizeH.value);
        return;
      }
      if (!int.TryParse(val.Substring(pos + 1).Trim(), out h)) {
        TileSizeField.SetTextWithoutNotify(TileSizeW.value + "x" + TileSizeH.value);
        return;
      }
      TileSizeW.SetValueWithoutNotify(w);
      TileSizeH.SetValueWithoutNotify(h);
    }
    else {
      TileSizeField.SetTextWithoutNotify(TileSizeW.value + "x" + TileSizeH.value);
    }
  }

  public void UpdateTileSize() {
    if (tw == (int)TileSizeW.value && th == (int)TileSizeH.value) return;
    tw = (int)TileSizeW.value;
    th = (int)TileSizeH.value;
    mapGrid.cellSize = new Vector2(tw * 5, th * 5);

    // Warning, we have to alter all raw data of all tile palettes
    foreach(Transform tr in tilesGrid.transform) {
      TileInPalette tl = tr.GetComponent<TileInPalette>();
      tl.UpdateSize(tw, th);
    }
    foreach(Transform tr in mapGrid.transform) {
      TileInMap tl = tr.GetComponent<TileInMap>();
      tl.img.texture = Palette[tl.id].img.texture;
    }
  }


  public void CreateNewTile() {
    byte min = 0;
    foreach(Transform tr in tilesGrid.transform) {
      TileInPalette tl = tr.GetComponent<TileInPalette>();
      if (min < tl.id) min = tl.id;
    }
    TileInPalette t = Instantiate(TileInPaletteTemplate, tilesGrid.transform).GetComponent<TileInPalette>();
    t.Setup((byte)(min + 1), SelectTileInPalette, tw, th);
    t.gameObject.SetActive(true);
    Palette[t.id] = t;
  }

  public void EditTile() {
    if (currentPaletteTile == null) return;
  }

  public void DeleteTile() {
    if (currentPaletteTile == null) return;
    // FIXME show a warning
  }

  TileInPalette currentPaletteTile = null;
  TileInMap currentPaletteMap = null;
  public void SelectTileInPalette(TileInPalette tile) {
    if (currentPaletteTile != null) currentPaletteTile.Deselect();
    currentPaletteTile = tile;
  }


  public TMP_InputField Values;
  public Button LoadSubButton;

  public void Save() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = false;
    Values.text = "TODO";
  }

  public void PreLoad() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }
  readonly Regex rgComments = new Regex("([^\\n]*)(//[^\\n]*)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, System.TimeSpan.FromSeconds(1));
  readonly Regex rgHex1 = new Regex("[\\s]*0x([a-f0-9]+)[\\s]*", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));
  readonly Regex rgHex2 = new Regex("[\\s]*([a-f0-9]+)[\\s]*", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));

  public void PostLoad() {
    if (!gameObject.activeSelf) return;
    string data = Values.text.Trim();
    data = rgComments.Replace(data, " ").Replace('\n', ' ').Trim();
    while (data.IndexOf("  ") != -1) data = data.Replace("  ", " ");


    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
  }

  string ReadNextByte(string data, out byte res) {
    int pos1 = data.IndexOf(' ');
    int pos2 = data.IndexOf('\n');
    int pos3 = data.Length;
    if (pos1 == -1) pos1 = int.MaxValue;
    if (pos2 == -1) pos2 = int.MaxValue;
    if (pos3 == -1) pos3 = int.MaxValue;
    int pos = pos1;
    if (pos > pos2) pos = pos2;
    if (pos > pos3) pos = pos3;
    if (pos < 1) {
      res = 0;
      return "";
    }

    string part = data.Substring(0, pos);
    Match m = rgHex1.Match(part);
    if (m.Success) {
      res = (byte)System.Convert.ToInt32(m.Groups[1].Value, 16);
      return data.Substring(pos).Trim();
    }
    else {
      m = rgHex2.Match(part);
      if (m.Success) {
        res = (byte)System.Convert.ToInt32(m.Groups[1].Value, 16);
        return data.Substring(pos).Trim();
      }
    }

    res = 0;
    return data;
  }


  public Image[] SelectionButtons;

  DrawMode drawMode = DrawMode.None;
  enum DrawMode { None, Draw, Line, Box, Fill, Clear };
  bool lineStart = false;
  int x1, x2, y1, y2;

  public void Draw() {
    drawMode = DrawMode.Draw;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 0;
    lineStart = false;
  }

  public void Line() {
    drawMode = DrawMode.Line;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 1;
    lineStart = true;
  }

  public void Box() {
    drawMode = DrawMode.Box;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 2;
    lineStart = false;
  }

  public void Fill() {
    drawMode = DrawMode.Fill;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 3;
    lineStart = false;
  }

  public void Clear() {
    drawMode = DrawMode.Clear;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 4;
    lineStart = false;
  }


  public void SelectTileInMap(TileInMap tile) {
    if (currentPaletteMap != null) currentPaletteMap.Deselect();
    currentPaletteMap = tile;


    switch (drawMode) {
      case DrawMode.Draw:
        if (currentPaletteTile == null) return;
        tile.img.texture = currentPaletteTile.img.texture;
        tile.id = currentPaletteTile.id;
        break;

      case DrawMode.Clear:
        tile.img.texture = emptyTexture;
        tile.id = 0;
        break;


      case DrawMode.Line: {
        if (lineStart) {
          x1 = tile.x;
          y2 = tile.y;
          tile.border.color = Color.red;
          lineStart = false;
        }
        else {
          x2 = tile.x;
          y2 = tile.y;
          DrawLine(x1, y1, x2, y2, false); // Reset color here
          lineStart = true;
        }

      }
      break;

    }

  }

  void DrawLine(int x1, int y1, int x2, int y2, bool border) {
    if (!border) {
      //SetUndo(false);
    }
    int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
    dx = x2 - x1; dy = y2 - y1;
    if (dx == 0) { // Vertical
      if (y2 < y1) { int tmp = y1; y1 = y2; y2 = tmp; }
      for (y = y1; y <= y2; y++) {
        TileInMap t = map.GetChild(x1 * w * y).GetComponent<TileInMap>();
        if (border) t.Select();
        if (currentPaletteTile == null) return;
        t.img.texture = currentPaletteTile.img.texture;
        t.id = currentPaletteTile.id;
      }
      lineStart = true;
      return;
    }

    if (dy == 0) { // Horizontal
      if (x2 < x1) { int tmp = x1; x1 = x2; x2 = tmp; }
      for (x = x1; x <= x2; x++) {
        TileInMap t = map.GetChild(x * w * y1).GetComponent<TileInMap>();
        if (border) t.Select();
        if (currentPaletteTile == null) return;
        t.img.texture = currentPaletteTile.img.texture;
        t.id = currentPaletteTile.id;
      }
      lineStart = true;
      return;
    }

    // Diagonal
    dx1 = dx;
    if (dx1 < 0) dx1 = -dx1;
    dy1 = dy;
    if (dy1 < 0) dy1 = -dy1;
    px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
    if (dy1 <= dx1) {
      if (dx >= 0) {
        x = x1; y = y1; xe = x2;
      }
      else {
        x = x2; y = y2; xe = x1;
      }

      TileInMap t = map.GetChild(x * w * y).GetComponent<TileInMap>();
      if (border) t.Select();
      if (currentPaletteTile != null) {
        t.img.texture = currentPaletteTile.img.texture;
        t.id = currentPaletteTile.id;
      }

      for (i = 0; x < xe; i++) {
        x += 1;
        if (px < 0)
          px += 2 * dy1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y += 1; else y -= 1;
          px += 2 * (dy1 - dx1);
        }
        t = map.GetChild(x * w * y).GetComponent<TileInMap>();
        if (border) t.Select();
        if (currentPaletteTile != null) {
          t.img.texture = currentPaletteTile.img.texture;
          t.id = currentPaletteTile.id;
        }
      }
    }
    else {
      if (dy >= 0) {
        x = x1; y = y1; ye = y2;
      }
      else {
        x = x2; y = y2; ye = y1;
      }
      TileInMap t = map.GetChild(x * w * y).GetComponent<TileInMap>();
      if (border) t.Select();
      if (currentPaletteTile != null) {
        t.img.texture = currentPaletteTile.img.texture;
        t.id = currentPaletteTile.id;
      }
      for (i = 0; y < ye; i++) {
        y += 1;
        if (py <= 0)
          py += 2 * dx1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x += 1; else x -= 1;
          py += 2 * (dx1 - dy1);
        }
        t = map.GetChild(x * w * y).GetComponent<TileInMap>();
        if (border) t.Select();
        if (currentPaletteTile != null) {
          t.img.texture = currentPaletteTile.img.texture;
          t.id = currentPaletteTile.id;
        }
      }
    }
  }


}
