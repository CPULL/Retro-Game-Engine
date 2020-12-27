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
  }

  public void Set(int w, int h, byte[] data, int pos, float scaleW, float scaleH, bool filter) {
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
        byte a = (byte)(255 - ((col & 0b11000000) >> 6) * 85);
        byte r = (byte)(((col & 0b00110000) >> 4) * 85);
        byte g = (byte)(((col & 0b00001100) >> 2) * 85);
        byte b = (byte)(((col & 0b00000011) >> 0) * 85);
        if (a == 0 && (r != 0 || g != 0 || b != 0)) a = 40;
        raw[dst + 0] = r;
        raw[dst + 1] = g;
        raw[dst + 2] = b;
        raw[dst + 3] = a;
        dst+=4;
      }
    }
    texture.LoadRawTextureData(raw);
    texture.Apply();
    sprite.texture = texture;
    rt.sizeDelta = new Vector2(w * scaleW, h * scaleH);
  }

  internal void Pos(int x, int y, float scaleW, float scaleH, bool enable) {
    rt.anchoredPosition = new Vector2(scaleW * x, -scaleH * y - 20.25f);
    sprite.enabled = enable;
  }

}

