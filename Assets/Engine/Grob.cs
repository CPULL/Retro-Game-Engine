using UnityEngine;
using UnityEngine.UI;

public class Grob : MonoBehaviour {
  [SerializeField] private RawImage sprite;
  [SerializeField] private RectTransform rt;
  Texture2D texture;
  byte[] raw;
  public bool notDefined = true;
  int x, y, w, h;

  public Texture2D Set(int pw, int ph, byte[] data, int pos, bool filter) {
    notDefined = false;
    if (pw < 8) pw = 8;
    if (pw > 64) pw = 64;
    if (ph < 8) ph = 8;
    if (ph > 64) ph = 64;
    w = pw;
    h = ph;
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
    return texture;
  }

  public Texture2D Set(int iw, int ih, int px, int py, int sx, int sy, byte[] data, int pos, bool filter) {
    notDefined = false;
    if (sx < 8) sx = 8;
    if (sx > 64) sx = 64;
    if (sy < 8) sy = 8;
    if (sy > 64) sy = 64;
    w = sx;
    h = sy;
    raw = new byte[w * h * 4];
    texture = new Texture2D(w, h, TextureFormat.RGBA32, false) {
      filterMode = filter ? FilterMode.Bilinear : FilterMode.Point
    };
    int limit = data.Length;

    int dst = 0;
    for (int y = h - 1; y >= 0; y--) {
      for (int x = 0; x < w; x++) {
        int p = pos + px + x + iw * (py + y);
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
    return texture;
  }

  public static Texture2D DefineTexture(int iw, int ih, int px, int py, int w, int h, byte[] data, int pos, bool filter) {
    if (w < 8) w = 8;
    if (w > 64) w = 64;
    if (h < 8) h = 8;
    if (h > 64) h = 64;

    byte[] raw = new byte[w * h * 4];
    Texture2D texture = new Texture2D(w, h, TextureFormat.RGBA32, false) {
      filterMode = filter ? FilterMode.Bilinear : FilterMode.Point
    };
    int limit = data.Length;

    int dst = 0;
    for (int y = h - 1; y >= 0; y--) {
      for (int x = 0; x < w; x++) {
        int p = pos + px + x + iw * (py + y);
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
    return texture;
  }

  internal void Set(int pw, int ph, Texture2D texture2D, bool filter) {
    if (pw < 8) pw = 8;
    if (pw > 64) pw = 64;
    if (ph < 8) ph = 8;
    if (ph > 64) ph = 64;
    w = pw;
    h = ph;
    notDefined = false;
    texture = texture2D;
    texture.filterMode = filter ? FilterMode.Bilinear : FilterMode.Point;
    sprite.texture = texture;
  }


  internal void Pos(int px, int py, float scaleW, float scaleH, bool enable) {
    x = px;
    y = py;
    sprite.enabled = enable;
    rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x * scaleW, rt.sizeDelta.x);
    rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y * scaleH, rt.sizeDelta.y);
    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w * scaleW);
    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h * scaleH);
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

  internal void Parent(Transform parent) {
    rt.transform.SetParent(parent);
  }

  internal void ResetScale(float scaleW, float scaleH) {
    rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x * scaleW, rt.sizeDelta.x);
    rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y * scaleH, rt.sizeDelta.y);
    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w * scaleW);
    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h * scaleH);
  }
}

