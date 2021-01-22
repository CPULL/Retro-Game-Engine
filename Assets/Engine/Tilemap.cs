using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tilemap : MonoBehaviour {
  int w, h;
  int x, y;
  int tw, th;
  byte order;
  Dictionary<byte, TileDef> tileDefs = new Dictionary<byte, TileDef>();
  Tile[,] tiles;
  public GameObject TileTemplate;
  public GridLayoutGroup gridLayout;



  public void Set(byte[] data, int start, byte order, float screenw, float screenh) {
    //   width, height, tilewidth, tileheight,
    //numtiles format[0 = tile by tile, else is number of horizontal tiles in the big sprite]

    // 1) define the grid parameters and the scale

    int pos = start;
    this.order = order;
    w = data[pos++];
    h = data[pos++];
    x = 0;
    y = 0;
    tw = data[pos++];
    th = data[pos++];
    int numtiles = data[pos++];
    if (tw < 8) tw = 8;
    if (tw > 32) tw = 32;
    if (th < 8) th = 8;
    if (th > 32) th = 32;
    byte format = data[pos++];

    int mapstart = pos; // Save for later
    pos += w * h * 2;

    // 2) Load all the textures, create the dictionary
    byte[] raw = new byte[tw * th * 4];
    int limit = data.Length;
    for (byte i = 0; i < numtiles; i++) {
      Texture2D texture = new Texture2D(tw, th, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
      int rowincrease = tw;
      if (format != 0) { // Single sprite with multiple tiles
        rowincrease *= format;
      }
      int dst = 0;
      for (int y = th - 1; y >= 0; y--) {
        for (int x = 0; x < tw; x++) {
          int p = pos + x + rowincrease * y;
          if (p >= limit) continue;
          byte col = data[p];
          byte a = (byte)(255 - ((col & 0b11000000) >> 6) * 85);
          byte r = (byte)(((col & 0b00110000) >> 4) * 85);
          byte g = (byte)(((col & 0b00001100) >> 2) * 85);
          byte b = (byte)(((col & 0b00000011) >> 0) * 85);
          if (a == 0 && (r != 0 || g != 0 || b != 0)) a = 40;
          raw[dst + 0] = r;
          raw[dst + 1] = g;
          raw[dst + 2] = b;
          raw[dst + 3] = a;
          dst += 4;
        }
        if (format == 0)
          pos += tw * th;
        else
          pos += tw;
      }
      texture.LoadRawTextureData(raw);
      texture.Apply();

      TileDef td = new TileDef(i, texture);
      tileDefs[i] = td;
    }

    // 3) initialize the array of tiles, Instantiate(TileTemplate) and set the texture
    tiles = new Tile[w, h];
    gridLayout.cellSize = new Vector2(tw, th); // FIXME find the right value
    gridLayout.constraintCount = w;
    string dbg = "";
    for (int i = 0; i < w; i++) {
      for (int j = 0; j < h; j++) {
        Tile tile = Instantiate(TileTemplate, transform).GetComponent<Tile>();
        tiles[i, j] = tile;
        tile.gameObject.SetActive(true);
        byte def = data[mapstart++];
        byte rot = data[mapstart++];

        // Set it up with the right texture
        tile.id = def;
        if (tileDefs.ContainsKey(def))
          tile.sprite.texture = tileDefs[def].texture;
        else
          Debug.Log("Invalid tile key " + def + " position " + (mapstart - 2));
        dbg += def + " ";
        // FIXME scale the object

        // FIXME rotate and flip the object (like a sprite)
      }
    }
    Debug.Log(dbg);
  }

  private void Pos(int px, int py, byte order) {
    x = px;
    y = py;
    this.order = order;
  }

  void SetTile(int x, int y, byte tile, byte rot) {

  }

  byte GetTile(int x, int y) {
    if (x < 0 || x >= w || y < 0 || y >= h) return 0;
    return tiles[x, y].id;
  }

  void UpdateTileDef(byte id, byte[] data) {

  }

  public void Destroy() {
    // FIXME release all the images and all the textures
  }
}

public class TileDef {
  public byte id;
  public Texture2D texture;

  public TileDef(byte id, Texture2D texture) {
    this.id = id;
    this.texture = texture;
  }
}



/*
  Tilemap:
  width, height, tilewidth, tileheight,
  numtiles format[0=tile by tile, else is number of horizontal tiles in the big sprite]

  foreach tile:
    set of bytes

  big sprite:
    set of bytes

  Tile format
  just a set of bytes for each pixel

 
 
 */ 