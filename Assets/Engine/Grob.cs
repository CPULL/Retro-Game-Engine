using UnityEngine;
using UnityEngine.UI;

public class Grob {
  RawImage sprite;
  RectTransform rt;
  Texture2D texture;
  byte[] raw;

  public Grob(RawImage img, int sw, int sh, bool filter) {
    sprite = img;
    rt = img.GetComponent<RectTransform>();
    texture = (Texture2D)img.texture;
    rt.sizeDelta = new Vector2(32 * 1920f / sw, 32 * 1080f / sh);
    // FIXME save them to calculate correctly the size of the sprite
  }

  public void Set(int w, int h, byte[] data, int pos, int sw, int sh, bool filter) {
    if (w < 8) w = 8;
    if (w > 32) w = 32;
    if (h < 8) h = 8;
    if (h > 32) h = 32;
    raw = new byte[w * h * 4];
    texture = new Texture2D(w, h, TextureFormat.RGBA32, false) {
      filterMode = filter ? FilterMode.Bilinear : FilterMode.Point
    };
    int limit = data.Length;

    int dst = 0;
    for (int y = h - 1; y >= 0; y--) {
      for (int x = 0; x < w; x++) {
        int p = pos + x + w * y;
        if (p >= limit) continue;
        byte col = data[p];
        raw[dst + 0] = (byte)(((col & 0b00110000) >> 4) * 85);
        raw[dst + 1] = (byte)(((col & 0b00001100) >> 2) * 85);
        raw[dst + 2] = (byte)(((col & 0b00000011) >> 0) * 85);
        raw[dst + 3] = (byte)(255 - ((col & 0b11000000) >> 6) * 85);
        dst+=4;
      }
    }

    texture.LoadRawTextureData(raw);
    texture.Apply();
    sprite.texture = texture;
    rt.sizeDelta = new Vector2(w * 1920f / sw, h * 1080f / sh);
  }

  internal void Pos(int x, int y, int sw, int sh, bool enable) {

    /*

    1920

    0->0
    8->75

    f(w, 1920, 0) -> 0
    f(w, 1920, 8) -> 75


    f(256, 1920, 0) -> 0
    f(256, 1920, 8) -> 75

    y = (1920/sw)*x

    8-> -74.25
    64 -> -452.25
    120 -> -830.25

    y = -(1080/160)*x-20.25

    */
    rt.anchoredPosition = new Vector2((1920f / sw) * x, -(1080f / sh) * y - 20.25f);
    sprite.enabled = enable;
  }
  // y8 -> -74.25
}

