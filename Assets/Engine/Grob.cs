using UnityEngine;
using UnityEngine.UI;

public class Grob : MonoBehaviour {
  [SerializeField] private RawImage sprite;
  [SerializeField] private RectTransform rt;
  Texture2D texture;
  byte[] raw;
  public bool notDefined = true;

  public Grob(RawImage img, int sw, int sh) {
    sprite = img;
    rt = img.GetComponent<RectTransform>();
    texture = (Texture2D)img.texture;
    rt.sizeDelta = new Vector2(32 * 1920f / sw, 32 * 1080f / sh);
  }

  public Texture2D Set(int w, int h, byte[] data, int pos, float scaleW, float scaleH, bool filter) {
    notDefined = false;
    if (w < 8) w = 8;
    if (w > 64) w = 64;
    if (h < 8) h = 8;
    if (h > 64) h = 64;
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
        Color32 col = Col.GetColor(data[p]);
        raw[dst + 0] = col.r;
        raw[dst + 1] = col.g;
        raw[dst + 2] = col.b;
        raw[dst + 3] = col.a;
        dst+=4;
      }
    }
    texture.LoadRawTextureData(raw);
    texture.Apply();
    sprite.texture = texture;
    rt.sizeDelta = new Vector2(w * scaleW, h * scaleH);
    return texture;
  }

  internal void Set(int w, int h, Texture2D texture2D, float scaleW, float scaleH, bool filter) {
    if (w < 8) w = 8;
    if (w > 64) w = 64;
    if (h < 8) h = 8;
    if (h > 64) h = 64;
    notDefined = false;
    texture = texture2D;
    texture.filterMode = filter ? FilterMode.Bilinear : FilterMode.Point;
    sprite.texture = texture;
    rt.sizeDelta = new Vector2(w * scaleW, h * scaleH);
  }


  internal void Pos(int x, int y, float scaleW, float scaleH, bool enable) {
    rt.anchoredPosition = new Vector2(scaleW * x, -scaleH * y);
    sprite.enabled = enable;
  }

  internal void Enable(bool enable) {
    sprite.enabled = enable;
  }

  internal void Rot(int rot, bool flip) {
    switch(rot % 4) {
      case 0: rt.localRotation = Quaternion.Euler(0, 0, 0); break;
      case 1: rt.localRotation = Quaternion.Euler(0, 0, 270); break;
      case 2: rt.localRotation = Quaternion.Euler(0, 0, 180); break;
      case 3: rt.localRotation = Quaternion.Euler(0, 0, 90); break;
    }
    Vector3 scale = rt.localScale;
    scale.x = (scale.x < 0 ? -scale.x : scale.x);
    scale.y = (scale.y < 0 ? -scale.y : scale.y);
    scale.x *= flip ? -1 : 1;
    rt.localScale = scale;
    /*
     n0, false   : 0 1,1
     n0, true    : 0, -1,1
     e1, false   : 270, 1
     e1, true    : 270, -1
     s2, false   : 180, 1
     s2, true    : 180, -1
     w3, false   : 0,0,90 1,1
     w3, true    : 0,0,90 -1,1
     */
  }

  internal void Tint(byte col) {
    sprite.color = Col.GetColor(col);
  }

  internal void Scale(byte sx, byte sy) {
    if (sx < 1) sx = 1;
    if (sx > 8) sx = 8;
    if (sy < 1) sy = 1;
    if (sy > 8) sy = 8;
    Vector3 scale = rt.localScale;
    scale.x = sx * (scale.x < 0 ? -1 : 1);
    scale.y = sy * (scale.y < 0 ? -1 : 1);
    rt.localScale = scale;
  }

  internal void Init(int x, int y, int sw, int sh) {
    float scalew = 1920f / sw;
    float scaleh = 1080f / sh;
    rt.sizeDelta = new Vector2(16 * scalew, 16 * scaleh);
    Pos(x, y, scalew, scaleh, true);
  }

  internal void Parent(Transform parent) {
    rt.transform.SetParent(parent);
  }

}

