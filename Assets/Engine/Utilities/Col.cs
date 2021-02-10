﻿using UnityEngine;

public class Col {

  /*
    6*6*6 = 216 full colors no alpha
    40 for alphas (1 bit each chroma + 5 level of alpha)
   */

  internal readonly static Color32[] alphas = new Color32[] {
    new Color32(222, 20, 20, 170), new Color32(222, 222, 20, 170), new Color32(20, 222, 20, 170),
    new Color32(20, 222, 222, 170), new Color32(20, 20, 222, 170), new Color32(222, 20, 222, 170),

    new Color32(180, 64, 64, 170), new Color32(180, 180, 64, 170), new Color32(64, 180, 64, 170),
    new Color32(64, 180, 180, 170), new Color32(64, 64, 180, 170), new Color32(180, 64, 180, 170),

    new Color32(222, 20, 20, 90), new Color32(222, 222, 20, 90), new Color32(20, 222, 20, 90),
    new Color32(20, 222, 222, 90), new Color32(20, 20, 222, 90), new Color32(222, 20, 222, 90),

    new Color32(180, 64, 64, 90), new Color32(180, 180, 64, 90), new Color32(64, 180, 64, 90),
    new Color32(64, 180, 180, 90), new Color32(64, 64, 180, 90), new Color32(180, 64, 180, 90),

    new Color32(240, 80, 80, 170), new Color32(240, 240, 80, 170), new Color32(80, 240, 80, 170),
    new Color32(80, 240, 240, 170), new Color32(80, 80, 240, 170), new Color32(240, 80, 240, 170),

    new Color32(255, 255, 255, 170), new Color32(170, 170, 170, 170), new Color32(85, 85, 85, 170), new Color32(0, 0, 0, 170),
    new Color32(255, 255, 255, 90), new Color32(170, 170, 170, 90), new Color32(85, 85, 85, 90), new Color32(0, 0, 0, 90),
    new Color32(0, 0, 0, 200), new Color32(0, 0, 0, 0),
  };

  readonly static Color32[] Palette = new Color32[256];
  readonly static Color32[] PaletteIndex = new Color32[256];
  static bool UsingPalette;

  public static void UsePalette(bool use) {
    UsingPalette = use;
    if (use) {
      Palette[0] = new Color32(0, 0, 0, 255);
      Palette[255] = new Color32(0, 0, 0, 0);
      for (int col = 0; col < 256; col++) {
        int hi = ((col & 0xF0) >> 4) * 8 + 4;
        int lo = (col & 0xF) * 8 + 4;
        PaletteIndex[col]= new Color32((byte)hi, (byte)lo, 0, 255); ;
      }
    }
  }

  public static bool UsePalette() {
    return UsingPalette;
  }

  public static void SetPalette(byte[] data, int start) {
    byte num = data[start];
    if (num < 1) return;
    if (num > 254) num = 254;
    for (int i = 0; i < num; i++) {
      int pos = start + 1 + i * 4;
      byte r = data[pos + 0];
      byte g = data[pos + 1];
      byte b = data[pos + 2];
      byte a = data[pos + 3];
      Palette[i + 1] = new Color32(r, g, b, a);
    }
  }

  public static void SetPalette(int col, byte r, byte g, byte b, byte a) {
    if (col < 1 || col > 254) return;
    Palette[col] = new Color32(r, g, b, a);
  }

  public static Color32 GetColor(byte col) {
    if (UsingPalette) return PaletteIndex[col];
    if (col < 216) {
      byte b = (byte)(col % 6);
      col -= b;
      col /= 6;
      byte g = (byte)(col % 6);
      col -= g;
      col /= 6;
      byte r = (byte)(col % 6);
      r = (byte)(r * 51f);
      g = (byte)(g * 51f);
      b = (byte)(b * 51f);
      return new Color32(r, g, b, 255);
    }
    col = (byte)(col - 216); // -> 0-39
    return alphas[col];
  }

  public static Color32 GetColorFrom6(byte r, byte g, byte b) {
    r = (byte)(r * 51f);
    g = (byte)(g * 51f);
    b = (byte)(b * 51f);
    return new Color32(r, g, b, 255);
  }

  public static Color32 NormalizeColor(int rs, int gs, int bs, int a) {
    if (rs < 0) rs = 0;
    if (rs > 255) rs = 255;
    if (gs < 0) gs = 0;
    if (gs > 255) gs = 255;
    if (bs < 0) bs = 0;
    if (bs > 255) bs = 255;
    if (a < 0) a = 0;
    if (a > 255) a = 255;
    if (a > 250) {
      rs = (int)(((byte)(rs / 51f)) * 51f);
      gs = (int)(((byte)(gs / 51f)) * 51f);
      bs = (int)(((byte)(bs / 51f)) * 51f);
      return new Color32((byte)rs, (byte)gs, (byte)bs, 255);
    }

    float mindist =
      (a - alphas[0].a) * (a - alphas[0].a) + Mathf.Sqrt(
      (rs - alphas[0].r) * (rs - alphas[0].r) +
      (gs - alphas[0].g) * (gs - alphas[0].g) +
      (bs - alphas[0].b) * (bs - alphas[0].b));
    byte best = 0;
    for (byte i = 1; i < alphas.Length; i++) {
      float dist =
          (a - alphas[i].a) * (a - alphas[i].a) + Mathf.Sqrt(
          (rs - alphas[i].r) * (rs - alphas[i].r) +
          (gs - alphas[i].g) * (gs - alphas[i].g) +
          (bs - alphas[i].b) * (bs - alphas[i].b));
      if (mindist > dist) {
        mindist = dist;
        best = i;
      }
    }
    return alphas[best];
  }

  public static int GetByteFrom6(int r, int g, int b, int a) {
    r *= 51;
    g *= 51;
    b *= 51;
    if (a == -1)
      return GetColorByte(NormalizeColor(r, g, b, 255));
    else {
      a *= 80;
      a += 10;
      return GetColorByte(NormalizeColor(r, g, b, a));
    }
  }


  public static byte GetColorByte(int rs, int gs, int bs, int a) {
    return GetColorByte(new Color32((byte)rs, (byte)gs, (byte)bs, (byte)a));
  }

  public static byte GetColorByte(Color32 col) {
    if (col.a > 250) {
      byte r = (byte)Mathf.RoundToInt(col.r / 51f);
      byte g = (byte)Mathf.RoundToInt(col.g / 51f);
      byte b = (byte)Mathf.RoundToInt(col.b / 51f);
      return (byte)(r * 36 + g * 6 + b);
    }
    float mindist = 
      (col.a - alphas[0].a) * (col.a - alphas[0].a) + Mathf.Sqrt(
      (col.r - alphas[0].r) * (col.r - alphas[0].r) + 
      (col.g - alphas[0].g) * (col.g - alphas[0].g) + 
      (col.b - alphas[0].b) * (col.b - alphas[0].b));
    byte best = 0;
    for (byte i = 1; i < alphas.Length; i++) {
      float dist =
      (col.a - alphas[i].a) * (col.a - alphas[i].a) + Mathf.Sqrt(
      (col.r - alphas[i].r) * (col.r - alphas[i].r) +
      (col.g - alphas[i].g) * (col.g - alphas[i].g) +
      (col.b - alphas[i].b) * (col.b - alphas[i].b));
      if (mindist > dist) {
        mindist = dist;
        best = i;
      }
    }
    best += 216;
    return best;
  }

  public static string GetColorString(int c) {
    byte col = (byte)c;
    byte r;
    byte g;
    byte b;
    byte a;
    if (c < 216) {
      b = (byte)(col % 6);
      col -= b;
      col /= 6;
      g = (byte)(col % 6);
      col -= g;
      col /= 6;
      r = (byte)(col % 6);
      return r.ToString() + g.ToString() + b.ToString();
    }
    col = (byte)(col - 216); // -> 0-39
    r = alphas[col].r;
    g = alphas[col].g;
    b = alphas[col].b;
    a = alphas[col].a;
    r /= 51;
    g /= 51;
    b /= 51;
    a /= 85;
    return r.ToString() + g.ToString() + b.ToString() + (a + 1).ToString();
  }

  public static byte C(byte r, byte g, byte b) {
    return GetColorByte(new Color32((byte)(r * 51), (byte)(g * 51), (byte)(b * 51), 255));
  }
}
