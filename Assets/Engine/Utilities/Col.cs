using UnityEngine;

public class Col {
  public enum ColMode { Linear, Brighter, Darker, Log, Exp };
  static byte[] linear = new byte[] { 0, 85, 170, 255 };
  static byte[] brighter = new byte[] { 50, 150, 200, 255 };
  static byte[] darker = new byte[] { 0, 50, 150, 200 };
  static byte[] log = new byte[] { 0, 25, 150, 250 };
  static byte[] exp = new byte[] { 0, 150, 200, 255 };

  public static Color32 GetColor32(byte col, ColMode mode = ColMode.Linear) {
    byte a = (byte)(3 - ((col & 0b11000000) >> 6));
    byte r = (byte)((col & 0b00110000) >> 4);
    byte g = (byte)((col & 0b00001100) >> 2);
    byte b = (byte)((col & 0b00000011) >> 0);
    if (a == 0 && (r != 0 || g != 0 || b != 0)) a = 40; else a *= 85;

    byte[] factor = linear;
    switch (mode) {
      case ColMode.Brighter: factor = brighter; break;
      case ColMode.Darker: factor = darker; break;
      case ColMode.Log: factor = log; break;
      case ColMode.Exp: factor = exp; break;
    }
    return new Color32(factor[r], factor[g], factor[b], a);
  }

  public static Color32 GetColor32(int r, int g, int b, int a=-1, ColMode mode = ColMode.Linear) { // Colors here are from 0 to 255, and may need to be trucated
    if (r < 0) r = 0;
    if (r > 255) r = 255;
    r /= 85;
    if (g < 0) g = 0;
    if (g > 255) g = 255;
    g /= 85;
    if (b < 0) b = 0;
    if (b > 255) b = 255;
    b /= 85;
    if (a != -1) {
      if (a < 0) a = 0;
      if (a > 255) a = 255;
      a /= 85;
    }
    if (a == 0 && (r != 0 || g != 0 || b != 0)) a = 40; else a *= 85;
    if (a == -1) a = 255;
    byte[] factor = linear;
    switch (mode) {
      case ColMode.Brighter: factor = brighter; break;
      case ColMode.Darker: factor = darker; break;
      case ColMode.Log: factor = log; break;
      case ColMode.Exp: factor = exp; break;
    }
    return new Color32(factor[r], factor[g], factor[b], (byte)a);
  }

  public static byte GetColorByte(Color32 col, ColMode mode = ColMode.Linear) {
    byte[] factor = linear;
    switch (mode) {
      case ColMode.Brighter: factor = brighter; break;
      case ColMode.Darker: factor = darker; break;
      case ColMode.Log: factor = log; break;
      case ColMode.Exp: factor = exp; break;
    }
    byte r = 0;
    if (factor[0] <= col.r && col.r < factor[1]) r = 0;
    else if (factor[1] <= col.r && col.r < factor[2]) r = 1;
    else if (factor[2] <= col.r && col.r < factor[3]) r = 2;
    else r = 3;

    byte g = 0;
    if (factor[0] <= col.g && col.g < factor[1]) g = 0;
    else if (factor[1] <= col.g && col.g < factor[2]) g = 1;
    else if (factor[2] <= col.g && col.g < factor[3]) g = 2;
    else g = 3;

    byte b = 0;
    if (factor[0] <= col.b && col.b < factor[1]) b = 0;
    else if (factor[1] <= col.b && col.b < factor[2]) b = 1;
    else if (factor[2] <= col.b && col.b < factor[3]) b = 2;
    else b = 3;

    byte a = 0;
    if (a == 40) a = 40;
    else if (linear[0] <= col.a && col.a < linear[1]) a = 0;
    else if (linear[1] <= col.a && col.a < linear[2]) a = 1;
    else if (linear[2] <= col.a && col.a < linear[3]) a = 2;
    else a = 3;

    return (byte)((a << 6) | (r << 4) | (g << 2) | b);
  }

}
