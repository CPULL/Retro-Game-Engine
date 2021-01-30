using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TilemapEditor : MonoBehaviour {
  public GameObject TileInMapTemplate;
  public GameObject TileInPaletteTemplate;
  public Texture2D emptyTexture;
  readonly Dictionary<byte, TileInPalette> Palette = new Dictionary<byte, TileInPalette>();

  private void Start() {
    updateMapSize = StartCoroutine(UpdateMapSize(0, 0));
  }

  TileInMap[,] map;

  public TMP_InputField MapSizeField;
  public Slider MapSizeW;
  public Slider MapSizeH;
  Coroutine updateMapSize;
  public TMP_InputField TileSizeField;
  public Slider TileSizeW;
  public Slider TileSizeH;
  public GridLayoutGroup mapGrid;
  public GridLayoutGroup tilesGrid;

  public SpriteEditor editor;
  readonly int[] sizes = new int[] { 8, 16, 24, 32, 40, 48, 56, 64 };

  int w = 24, h = 16;
  int tw = 16, th = 16;
  public void AlterMapSize(bool fromInputField) {
    int pw = w;
    int ph = h;
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
      updateMapSize = StartCoroutine(UpdateMapSize(pw, ph));
    }
  }
  IEnumerator UpdateMapSize(int pw, int ph) {
    yield return new WaitForSeconds(.3f);
    w = (int)MapSizeW.value;
    h = (int)MapSizeH.value;
    mapGrid.constraintCount = w;
    int size = w * h;
    if (map == null) {
      pw = 0;
      ph = 0;
    }

    // Detach all the previous items from the grid, keep the array
    if (map != null)
      foreach (TileInMap t in map)
        t.transform.SetParent(null);

    // Create the new array and fill what is possible with the old array
    TileInMap[,] newmap = new TileInMap[w, h];
    for (int x = 0; x < w; x++)
      for (int y = 0; y < h; y++)
        if (x < pw && y < ph) {
          newmap[x, y] = map[x, y];
          map[x, y] = null;
        }

    // Destroy all items that are not moved
    if (map != null)
      for (int x = 0; x < pw; x++)
      for (int y = 0; y < ph; y++)
        if (map[x, y] != null)
          Destroy(map[x, y].gameObject);

    // Create the missing items
    for (int x = 0; x < w; x++)
      for (int y = 0; y < h; y++)
        if (newmap[x, y] == null) {
          TileInMap t = Instantiate(TileInMapTemplate, null).GetComponent<TileInMap>();
          t.Setup(SelectTileInMap, OverTileInMap, emptyTexture);
          newmap[x, y] = t;
        }

    // Repopulate the grid with the ordered siblings
    for (int y = 0; y < h; y++)
      for (int x = 0; x < w; x++) {
        newmap[x, y].x = (byte)x;
        newmap[x, y].y = (byte)y;
        newmap[x, y].transform.SetParent(mapGrid.transform);
        newmap[x, y].transform.SetSiblingIndex(x + w * y);
        newmap[x, y].transform.localScale = new Vector3(1, 1, 1);
      }

    map = newmap;
    updateMapSize = null;
  }
  
  public void AlterTileSize(bool fromInputField) {
    if (fromInputField) {
      string val = TileSizeField.text.Trim().ToLowerInvariant();
      TileSizeField.SetTextWithoutNotify(tw + "x" + th);
      int pos1 = val.IndexOf(' ');
      int pos2 = val.IndexOf('x');
      if (pos1 == -1 && pos2 == -1) return;
      int pos = pos1;
      if (pos2 != -1 && (pos2 < pos1 || pos1 == -1)) pos = pos2;
      if (!int.TryParse(val.Substring(0, pos).Trim(), out int ltw)) return;
      if (!int.TryParse(val.Substring(pos + 1).Trim(), out int lth)) return;
      for (int i = 0; i < sizes.Length; i++) {
        if (sizes[i] <= ltw) { tw = sizes[i]; TileSizeW.SetValueWithoutNotify(i); }
        if (sizes[i] <= lth) { th = sizes[i]; TileSizeH.SetValueWithoutNotify(i); }
      }
      TileSizeField.SetTextWithoutNotify(tw + "x" + th);
    }
    else {
      TileSizeField.SetTextWithoutNotify(sizes[(int)TileSizeW.value] + "x" + sizes[(int)TileSizeH.value]);
    }
  }
  public void UpdateTileSize() {
    if (tw == sizes[(int)TileSizeW.value] && th == sizes[(int)TileSizeH.value]) return;

    tw = sizes[(int)TileSizeW.value];
    th = sizes[(int)TileSizeH.value];
    mapGrid.cellSize = new Vector2(tw * 5, th * 5);

    // Warning, we have to alter all raw data of all tile palettes
    foreach(Transform tr in tilesGrid.transform) {
      TileInPalette tl = tr.GetComponent<TileInPalette>();
      tl.UpdateSize(tw, th);
    }
    foreach(Transform tr in mapGrid.transform) {
      TileInMap tl = tr.GetComponent<TileInMap>();
      tl.Deselect();
      if (tl.id != 0)
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
    if (currentPaletteTile != null) currentPaletteTile.Deselect();
    currentPaletteTile = t;
  }

  public void EditTile() {
    if (currentPaletteTile == null) return;

    // Setup sprite editor with the correct tile size and move to sprite editor
    Dev.inst.SpriteEditor();
    editor.ImportFrom(currentPaletteTile);
    gameObject.SetActive(false);
  }

  public void DeleteTile() {
    if (currentPaletteTile == null) return;

    foreach (TileInMap t in map)
      if (t.id == currentPaletteTile.id)
        t.Setup(SelectTileInMap, OverTileInMap, emptyTexture);
    Destroy(Palette[currentPaletteTile.id].gameObject);
    Palette.Remove(currentPaletteTile.id);
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
    StartCoroutine(Saving());
  }

  IEnumerator Saving() {
    yield return PBar.Show("Saving", 0, Palette.Count * 2 + h + 1);

    // Normalize the IDs of the tiles
    byte[] keys = new byte[Palette.Keys.Count];
    Palette.Keys.CopyTo(keys, 0);
    System.Array.Sort(keys);
    for (int i = 0; i < keys.Length; i++) {
      yield return PBar.Progress(i + 1);
      byte key = keys[i];
      TileInPalette tile = Palette[key];
      Palette.Remove(key);
      key = (byte)(i + 1);
      tile.id = key;
      Palette[key] = tile;
    }

    string res = "Tilemap:\nusehex\n";
    res += w.ToString("X2") + " " + h.ToString("X2") + " " + tw.ToString("X2") + " " + th.ToString("X2") + " " + Palette.Count.ToString("X2") + "\n";

    // w*h*2 bytes with the actual map
    for (int y = 0; y < h; y++) {
      PBar.Progress(Palette.Count + y + 1);
      for (int x = 0; x < w; x++) {
        res += map[x, y].id.ToString("X2") + map[x, y].rot.ToString("X2") + " ";
      }
      res += "\n";
    }

    // Tiles
    for (int i = 0; i < Palette.Count; i++) {
      yield return PBar.Progress(Palette.Count + h + i + 1);
      TileInPalette tile = Palette[(byte)(i + 1)];
      int len = tile.rawData.Length;
      for (int j = 0; j < len; j+=4) {
        res += tile.rawData[j].ToString("X2");
        if (j + 1 < len) res += tile.rawData[j + 1].ToString("X2");
        if (j + 2 < len) res += tile.rawData[j + 2].ToString("X2");
        if (j + 3 < len) res += tile.rawData[j + 3].ToString("X2") + " ";
      }
      res += "\n";
    }
    Values.text = res;
    PBar.Hide();
  }

  public void PreLoad() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }

  public void PostLoad() {
    if (!gameObject.activeSelf) return;
    StartCoroutine(Loading());
  }

  IEnumerator Loading() {
    string data = Values.text.Trim();
    PBar.Show("Loading", 0, 200);
    byte[] block;
    try {
      ByteReader.ReadBlock(data, out List<CodeLabel> labels, out block);
    } catch (System.Exception e) {
      Values.text = "Parsing error: " + e.Message + "\n" + Values.text;
      PBar.Hide();
      yield break;
    }

    yield return PBar.Progress(50);
    int pw = block[0];
    int ph = block[1];
    MapSizeW.SetValueWithoutNotify(pw);
    MapSizeH.SetValueWithoutNotify(ph);
    updateMapSize = StartCoroutine(UpdateMapSize(w, h));

    // We need to wait for the coroutine to end before proceeding
    StartCoroutine(CompleteLoading(block));
  }

  IEnumerator CompleteLoading(byte[] block) {
    yield return null;
    while (updateMapSize != null)
      yield return new WaitForSeconds(.5f);
    yield return PBar.Progress(100);

    tw = block[2];
    th = block[3];
    byte numtiles = block[4];
    int pos = 5;

    // w*h*2 bytes with the actual map
    for (int y = 0; y < h; y++) {
      for (int x = 0; x < w; x++) {
        map[x, y].id = block[pos++];
        map[x, y].rot = block[pos++];
      }
    }
    yield return PBar.Progress(120);

    // Tiles
    Palette.Clear();
    foreach (Transform t in tilesGrid.transform) {
      t.SetParent(null);
      Destroy(t.gameObject);
    }
    for (int i = 0; i < numtiles; i++) {
      TileInPalette t = Instantiate(TileInPaletteTemplate, tilesGrid.transform).GetComponent<TileInPalette>();
      t.Setup((byte)(i + 1), SelectTileInPalette, tw, th);
      t.gameObject.SetActive(true);
      Palette[t.id] = t;
      byte[] rawData = new byte[tw * th];
      for (int b = 0; b < tw * th; b++)
        rawData[b] = block[pos++];
      t.UpdateTexture(rawData);
    }
    yield return PBar.Progress(150);

    // Update the tiles in the map
    for (int y = 0; y < h; y++) {
      yield return PBar.Progress(150 + 50 * y / h);
      for (int x = 0; x < w; x++) {
        TileInMap t = map[x, y];
        t.x = (byte)x;
        t.y = (byte)y;
        if (t.id == 0)
          t.img.texture = emptyTexture;
        else
          t.img.texture = Palette[t.id].img.texture;
        currentPaletteMap = t;
        Rot(t.rot);
      }
    }

    PBar.Hide();
    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
  }


  public Image[] SelectionButtons;
  public Image[] RotationButtons;


  DrawMode drawMode = DrawMode.None;
  enum DrawMode { None, Draw, Line, Box, Fill, Clear, ImportPic };
  Steps lineStep = Steps.None;
  Steps boxStep = Steps.None;
  Steps importStep = Steps.None;
  bool fillStep = false;
  int x1, x2, y1, y2;
  enum Steps { None, Start, End };

  public void Draw() {
    drawMode = DrawMode.Draw;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 0;
    lineStep = Steps.None;
    boxStep = Steps.None;
    importStep = Steps.None;
    fillStep = false;
  }

  public void Line() {
    drawMode = DrawMode.Line;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 1;
    lineStep = Steps.Start;
    boxStep = Steps.None;
    importStep = Steps.None;
    fillStep = false;
  }

  public void Box() {
    drawMode = DrawMode.Box;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 2;
    lineStep = Steps.None;
    boxStep = Steps.Start;
    importStep = Steps.None;
    fillStep = false;
  }

  public void Fill() {
    drawMode = DrawMode.Fill;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 3;
    lineStep = Steps.None;
    boxStep = Steps.None;
    importStep = Steps.None;
    fillStep = true;
  }

  public void Clear() {
    drawMode = DrawMode.Clear;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 4;
    lineStep = Steps.None;
    boxStep = Steps.None;
    importStep = Steps.None;
    fillStep = false;
  }

  public void DestroyTilemap() {
    foreach (byte b in Palette.Keys)
      Destroy(Palette[b].gameObject);
    Palette.Clear();
    foreach(TileInMap b in map) {
      b.img.texture = emptyTexture;
      b.id = 0;
    }
  }

  public void SelectTileInMap(TileInMap tile) {
    if (currentPaletteMap != null) currentPaletteMap.Deselect();
    currentPaletteMap = tile;

    for (int i = 0; i < RotationButtons.Length; i++)
      RotationButtons[i].enabled = tile.rot == i;

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


      case DrawMode.Line:
        if (lineStep == Steps.Start) {
          x1 = tile.x;
          y1 = tile.y;
          tile.border.color = Color.red;
          lineStep = Steps.End;
        }
        else if (lineStep == Steps.End) {
          x2 = tile.x;
          y2 = tile.y;
          DrawLine(x1, y1, x2, y2, false); // Reset color here
          lineStep = Steps.Start;
        }
      break;

      case DrawMode.Box:
        if (boxStep == Steps.Start) {
          x1 = tile.x;
          y1 = tile.y;
          tile.border.color = Color.red;
          boxStep = Steps.End;
        }
        else if (boxStep == Steps.End) {
          x2 = tile.x;
          y2 = tile.y;
          DrawBox(x1, y1, x2, y2, false); // Reset color here
          boxStep = Steps.Start;
        }
      break;

      case DrawMode.ImportPic:
        if (importStep == Steps.Start) {
          x1 = tile.x;
          y1 = tile.y;
          tile.border.color = Color.red;
          importStep = Steps.End;
        }
        else if (importStep == Steps.End) {
          x2 = tile.x;
          y2 = tile.y;
          importStep = Steps.Start;
          ImportTilesFromPicture(x1, y1, x2, y2);
        }
      break;

      case DrawMode.Fill:
        if (fillStep) Fill(tile.x, tile.y);
        break;
    }

  }

  public void OverTileInMap(TileInMap tile, bool enter) {
    if (!enter) return;
    if (lineStep == Steps.End) DrawLine(x1, y1, tile.x, tile.y, true);
    else if (boxStep == Steps.End) DrawBox(x1, y1, tile.x, tile.y, true);
    else if (importStep == Steps.End) DrawBox(x1, y1, tile.x, tile.y, true);
  }

  void DrawLine(int x1, int y1, int x2, int y2, bool border) {
    for (int bx = 0; bx < w; bx++)
      for (int by = 0; by < h; by++)
        map[bx, by].Deselect();

    int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
    dx = x2 - x1; dy = y2 - y1;
    if (dx == 0) { // Vertical
      if (y2 < y1) { int tmp = y1; y1 = y2; y2 = tmp; }
      for (y = y1; y <= y2; y++) {
        TileInMap t = map[x1, y];
        if (border)
          t.Select();
        else {
          if (currentPaletteTile == null) return;
          t.img.texture = currentPaletteTile.img.texture;
          t.id = currentPaletteTile.id;
        }
      }
      return;
    }

    if (dy == 0) { // Horizontal
      if (x2 < x1) { int tmp = x1; x1 = x2; x2 = tmp; }
      for (x = x1; x <= x2; x++) {
        TileInMap t = map[x, y1];
        if (border)
          t.Select();
        else {
          if (currentPaletteTile == null) return;
          t.img.texture = currentPaletteTile.img.texture;
          t.id = currentPaletteTile.id;
        }
      }
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

      TileInMap t = map[x, y];
      if (border)
        t.Select();
      else if (currentPaletteTile != null) {
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
        t = map[x, y];
        if (border)
          t.Select();
        else if (currentPaletteTile != null) {
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
      TileInMap t = map[x, y];
      if (border)
        t.Select();
      else if (currentPaletteTile != null) {
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
        t = map[x, y];
        if (border)
          t.Select();
        else if (currentPaletteTile != null) {
          t.img.texture = currentPaletteTile.img.texture;
          t.id = currentPaletteTile.id;
        }
      }
    }
  }

  void DrawBox(int x1, int y1, int x2, int y2, bool border) {
    for (int bx = 0; bx < w; bx++)
      for (int by = 0; by < h; by++)
        map[bx, by].Deselect();

    if (x1 > x2) { int tmp = x1; x1 = x2; x2 = tmp; }
    if (y1 > y2) { int tmp = y1; y1 = y2; y2 = tmp; }

    for (int x = x1; x <= x2; x++) {
      if (border || currentPaletteTile == null) {
        map[x, y1].Select();
        map[x, y2].Select();
      }
      else {
        map[x, y1].img.texture = currentPaletteTile.img.texture;
        map[x, y1].id = currentPaletteTile.id;
        map[x, y2].img.texture = currentPaletteTile.img.texture;
        map[x, y2].id = currentPaletteTile.id;
      }
    }
    for (int y = y1 + 1; y < y2; y++) {
      if (border || currentPaletteTile == null) {
        map[x1, y].Select();
        map[x2, y].Select();
      }
      else {
        map[x1, y].img.texture = currentPaletteTile.img.texture;
        map[x1, y].id = currentPaletteTile.id;
        map[x2, y].img.texture = currentPaletteTile.img.texture;
        map[x2, y].id = currentPaletteTile.id;
      }
    }
  }

  bool ValidCoord(int x, int y) {
    return !(x < 0 || y < 0 || x >= w || y >= h);
  }

  public void Fill(int x, int y) {
    if (currentPaletteTile == null) return;
    // Visited pixels array
    bool[,] vis = new bool[w, h];

    // Initialing all as zero
    for (int i = 0; i < w; i++)
      for (int j = 0; j < h; j++)
        vis[i, j] = false;

    Queue<Vector2Int> obj = new Queue<Vector2Int>();
    obj.Enqueue(new Vector2Int(x, y));

    // Marking {x, y} as visited
    vis[x, y] = true;

    // Untill queue is empty
    while (obj.Count != 0) {
      // Extrating front pair
      Vector2Int coord = obj.Dequeue();
      int x1 = coord.x;
      int y1 = coord.y;
      byte preColor = map[x1, y1].id;
      map[x1, y1].img.texture = currentPaletteTile.img.texture;
      map[x1, y1].id = currentPaletteTile.id;

      if (ValidCoord(x1 + 1, y1) && !vis[x1 + 1, y1] && map[x1 + 1, y1].id == preColor) {
        obj.Enqueue(new Vector2Int(x1 + 1, y1));
        vis[x1 + 1, y1] = true;
      }

      if (ValidCoord(x1 - 1, y1) && !vis[x1 - 1, y1] && map[x1 - 1, y1].id == preColor) {
        obj.Enqueue(new Vector2Int(x1 - 1, y1));
        vis[x1 - 1, y1] = true;
      }

      if (ValidCoord(x1, y1 + 1) && !vis[x1, y1 + 1] && map[x1, y1 + 1].id == preColor) {
        obj.Enqueue(new Vector2Int(x1, y1 + 1));
        vis[x1, y1 + 1] = true;
      }

      if (ValidCoord(x1, y1 - 1) && !vis[x1, y1 - 1] && map[x1, y1 - 1].id == preColor) {
        obj.Enqueue(new Vector2Int(x1, y1 - 1));
        vis[x1, y1 - 1] = true;
      }
    }
  }

  internal void UpdateTile(Pixel[] pixels) {
    currentPaletteTile.UpdateTexture(pixels);
  }

  public void Rot(int rot) {
    if (currentPaletteMap == null) return;

    for (int i = 0; i < RotationButtons.Length; i++)
      RotationButtons[i].enabled = rot == i;
    currentPaletteMap.rot = (byte)rot;
    switch (rot) {
      case 0:
        currentPaletteMap.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentPaletteMap.transform.localScale = new Vector3(1, 1, 1);
        break;
      case 1:
        currentPaletteMap.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentPaletteMap.transform.localScale = new Vector3(-1, 1, 1);
        break;
      case 2:
        currentPaletteMap.transform.rotation = Quaternion.Euler(0, 0, -90);
        currentPaletteMap.transform.localScale = new Vector3(1, 1, 1);
        break;
      case 3:
        currentPaletteMap.transform.rotation = Quaternion.Euler(0, 0, -90);
        currentPaletteMap.transform.localScale = new Vector3(-1, 1, 1);
        break;
      case 4:
        currentPaletteMap.transform.rotation = Quaternion.Euler(0, 0, 180);
        currentPaletteMap.transform.localScale = new Vector3(1, 1, 1);
        break;
      case 5:
        currentPaletteMap.transform.rotation = Quaternion.Euler(0, 0, 180);
        currentPaletteMap.transform.localScale = new Vector3(-1, 1, 1);
        break;
      case 6:
        currentPaletteMap.transform.rotation = Quaternion.Euler(0, 0, 90);
        currentPaletteMap.transform.localScale = new Vector3(1, 1, 1);
        break;
      case 7:
        currentPaletteMap.transform.rotation = Quaternion.Euler(0, 0, 90);
        currentPaletteMap.transform.localScale = new Vector3(-1, 1, 1);
        break;
    }

  }

  public void ImportTilesFromPicture() {
    // Select a rectangle
    drawMode = DrawMode.ImportPic;
    lineStep = Steps.None;
    boxStep = Steps.None;
    importStep = Steps.Start;
    for (int i = 0; i < SelectionButtons.Length; i++)
      SelectionButtons[i].enabled = i == 2;
  }

  int importX1, importY1, importX2, importY2, iw, ih;
  void ImportTilesFromPicture(int x1, int y1, int x2, int y2) {
    if (x1 > x2) { int tmp = x1; x1 = x2; x2 = tmp; }
    if (y1 > y2) { int tmp = y1; y1 = y2; y2 = tmp; }
    importX1 = x1;
    importY1 = y1;
    importX2 = x2;
    importY2 = y2;

    for (int bx = x1; bx <= x2; bx++) {
      map[bx, y1].Select();
      map[bx, y2].Select();
    }
    for (int by = y1; by <= y2; by++) {
      map[x1, by].Select();
      map[x2, by].Select();
    }

    iw = tw * (importX2 - importX1 + 1);
    ih = th * (importY2 - importY1 + 1);

    // Show the file browser
    FileBrowser.Show(ImportTilesFromPicture, FileBrowser.FileType.Pics);
  }

  public void ImportTilesFromPicture(string path) {
    // Load the image and scale it
    StartCoroutine(LoadImageCoroutine(path));
  }

  IEnumerator LoadImageCoroutine(string path) {
    string url = string.Format("file://{0}", path);

    yield return PBar.Show("Loading file", 0, 100);

    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url)) {
      yield return www.SendWebRequest();
      yield return PBar.Show("Loading file", 5, 100);
      Texture2D texture = DownloadHandlerTexture.GetContent(www);
      // Get the top-left part of the image fitting in the sprite size
      yield return PBar.Show("Loading file", 10, 100);
      TextureScale.Point(texture, iw, ih);
      byte[] pixels = new byte[iw * ih];
      yield return PBar.Show("Loading file", 15, 100);

      // Create the tiles
      int maxy = importY2 - importY1;
      int maxx = importX2 - importX1;

      yield return PBar.Show("Generating textures", 1, 1 + ih + maxy * maxy + 1);

      // Normalize the color
      Color32[] tps = texture.GetPixels32();
      for (int y = 0; y < ih; y++) {
        int ty = ih - y - 1;
        for (int x = 0; x < iw; x++) {
          pixels[x + iw * y] = Col.GetColorByte(tps[x + iw * ty]);
        }
        yield return PBar.Progress(1 + y);
      }

      Dictionary<byte, byte[]> ts = new Dictionary<byte, byte[]>();
      foreach(byte key in Palette.Keys) {
        TileInPalette tip = Palette[key];
        ts.Add(key, tip.rawData);
      }
      for (int y = 0; y <= maxy; y++) {
        for (int x = 0; x <= maxx; x++) {
          yield return PBar.Progress(1 + ih + x + maxx * y);

          byte[] tileData = new byte[tw * th];
          bool empty = true;
          for (int ty = 0; ty < th; ty++) {
            for (int tx = 0; tx < tw; tx++) {
              tileData[tx + tw * ty] = pixels[x * tw + tx + y * th * iw + (ty * iw)];
              if (tileData[tx + tw * ty] != 255) empty = false;
            }
          }

          if (!empty) {
            // Check if the tile is already there
            byte def = 0;
            foreach (byte id in ts.Keys) {
              byte[] done = ts[id];
              bool good = true;
              for (int pos = 0; pos < th * tw; pos++) {
                if (done[pos] != tileData[pos]) {
                  good = false;
                  break;
                }
              }
              if (good) {
                def = id;
                break;
              }
            }

            TileInMap tile = map[importX1 + x, importY1 + y];
            if (def == 0) {
              CreateNewTile();
              currentPaletteTile.UpdateTexture(tileData);
              tile.img.texture = currentPaletteTile.img.texture;
              tile.id = currentPaletteTile.id;
              ts.Add(tile.id, tileData);
            }
            else {
              TileInPalette palt = Palette[def];
              tile.img.texture = palt.img.texture;
              tile.id = palt.id;
            }
          }
          else { // Empty tile
            map[importX1 + x, importY1 + y].img.texture = emptyTexture;
            map[importX1 + x, importY1 + y].id = 0;
          }

        }
      }
      ts.Clear();
    }

    yield return PBar.Complete();
    for (int bx = 0; bx < w; bx++)
      for (int by = 0; by < h; by++)
        map[bx, by].Deselect();

    PBar.Hide();
  }

}
