using UnityEngine;

public class Rhsv {
  public float h;
  public float s;
  public float v;
  public int num;
  public float dist;

  public Rhsv(float hh, float ss, float vv) {
    h = hh;
    s = ss;
    v = vv;
    num = 1;
    dist = 0;
  }

  public override string ToString() {
    return (h * 255).ToString("X2") + (s * 255).ToString("X2") + (v * 255).ToString("X2");
  }

  public int ID() {
    return ((short)(1024 * h) << 24) + ((byte)(255 * s) << 8) + (byte)(v * 255);
  }
  public int IDH() {
    return (int)(1024 * h) << 2 + (s > .5f ? 2 : 0) + (v > .5f ? 1 : 0);
  }
  public int IDS() {
    return (int)(255 * s) << 4 + (int)(h * 8) * 2 + (v > .5f ? 1 : 0);
  }
  public int IDV() {
    return (int)(255 * v) << 4 + (int)(h * 8) * 2 + (s > .5f ? 1 : 0);
  }
  internal Rhsv JoinH(Rhsv val) {
    Rhsv res = new Rhsv(h, (s + val.s) * .5f, (v + val.v) * .5f) {
      num = num + val.num
    };
    if (num > val.num) res.h = h; else res.h = val.h;
    return res;
  }
  internal Rhsv JoinSV(Rhsv val) {
    Rhsv res = new Rhsv(h, (s + val.s) * .5f, (v + val.v) * .5f);
    if (num > val.num) res.h = h; else res.h = val.h;
    res.num = num + val.num;
    return res;
  }
  internal Rhsv JoinA(Rhsv val) {
    Rhsv res = new Rhsv(h, (s + val.s) * .5f, v);
    if (num > val.num) res.h = h; else res.h = val.h;
    res.num = num + val.num;
    return res;
  }

  internal void Dist(Rhsv val, float hhh, float sss, float vvv) {
    float d = Mathf.Sqrt((h - val.h) * (h - val.h) * hhh + (s - val.s) * (s - val.s) * sss + (v - val.v) * (v - val.v) * vvv);
    dist += d;
  }

  internal Color32 C32() {
    return Color.HSVToRGB(h, s, v);
  }

}

