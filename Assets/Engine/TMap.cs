using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TMap : MonoBehaviour {
  [HideInInspector] public int w, h;
  int tw, th;
  public byte order;
  readonly Dictionary<byte, TileDef> tileDefs = new Dictionary<byte, TileDef>();
  Tile[,] tiles;
  public GameObject TileTemplate;
  public GridLayoutGroup gridLayout;
  public Texture2D emptyTexture;
  public RectTransform rt;

  public void Set(byte[] data, int start) {
    //   width, height, tilewidth, tileheight, numtiles

    // 1) define the grid parameters and the scale
    int pos = start;
    w = data[pos++];
    h = data[pos++];
    tw = data[pos++];
    th = data[pos++];
    int numtiles = data[pos++];
    if (tw < 8) tw = 8;
    if (tw > 64) tw = 64;
    if (th < 8) th = 8;
    if (th > 64) th = 64;

    int mapstart = pos; // Save for later
    pos += w * h * 2;

    // 2) Load all the textures, create the dictionary
    byte[] raw = new byte[tw * th * 4];
    int limit = data.Length;
    for (byte i = 0; i < numtiles; i++) {
      Texture2D texture = new Texture2D(tw, th, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
      int dst = 0;
      for (int y = th - 1; y >= 0; y--) {
        for (int x = 0; x < tw; x++) {
          int p = pos + x + tw * y;
          if (p >= limit) continue;
          Color32 col = Col.GetColor(data[p]);
          raw[dst + 0] = col.r;
          raw[dst + 1] = col.g;
          raw[dst + 2] = col.b;
          raw[dst + 3] = col.a;
          dst += 4;
        }
      }
      pos += tw * th;
      texture.LoadRawTextureData(raw);
      texture.Apply();

      TileDef td = new TileDef((byte)(i + 1), texture);
      tileDefs[(byte)(i + 1)] = td;
    }

    // 3) initialize the array of tiles, Instantiate(TileTemplate) and set the texture
    tiles = new Tile[w, h];
    gridLayout.cellSize = new Vector2(tw, th); // FIXME find the right value
    gridLayout.constraintCount = w;
    for (int y = 0; y < h; y++) {
      for (int x = 0; x < w; x++) {
        Tile tile = Instantiate(TileTemplate, transform).GetComponent<Tile>();
        tiles[x, y] = tile;
        tile.gameObject.SetActive(true);
        byte def = data[mapstart++];
        byte rot = data[mapstart++];

        // Set it up with the right texture
        tile.id = def;
        tile.rot = rot;
        if (tileDefs.ContainsKey(def))
          tile.sprite.texture = tileDefs[def].texture;
        else if (def == 0)
          tile.sprite.texture = emptyTexture;
        else
          Debug.Log("Invalid tile key " + def + " position " + (mapstart - 2));
        // FIXME scale the object

        tile.Rot();
      }
    }
  }



  /*
  void UpdateTileDef(byte id, byte[] data) {

  }

  */
  public void Destroy() {
    // Release all the images and all the textures
    foreach (TileDef t in tileDefs.Values)
      DestroyImmediate(t.texture);
    tileDefs.Clear();

    foreach (Tile t in tiles)
      Destroy(t.gameObject);

    tiles = null;
    w = 0;
    h = 0;
    tw = 0;
    th = 0;
  }

  internal void SetTile(int x, int y, byte tile, byte rot) {
    tiles[x, y].id = tile;
    if (tileDefs.ContainsKey(tile))
      tiles[x, y].sprite.texture = tileDefs[tile].texture;
    else
      tiles[x, y].sprite.texture = emptyTexture;
    if (rot != 255) tiles[x, y].rot = rot;
    tiles[x, y].Rot();
  }

  internal byte GetTile(int x, int y) {
    if (x < 0 || x >= w || y < 0 || y >= h) return 0;
    return tiles[x, y].id;
  }

  internal byte GetTileRotation(int x, int y) {
    if (x < 0 || x >= w || y < 0 || y >= h) return 0;
    return tiles[x, y].rot;
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