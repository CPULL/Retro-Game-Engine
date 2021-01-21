using UnityEngine;
using UnityEngine.UI;

public class PaletteCube : MonoBehaviour {
  public PixelCube[] colors;
  public Pixel[] alphas;
  public Image selected;
  byte r = 255, g = 255, b = 255, a = 255;

  public Pixel BasicWhite;
  public Pixel BasicRed;
  public Pixel BasicYellow;
  public Pixel BasicGreen;
  public Pixel BasicCyan;
  public Pixel BasicBlue;
  public Pixel BasicPurple;
  public Pixel BasicBlack;

  private void Start() {
    for (int i = 0; i < colors.Length; i++)
      colors[i].Init(SetColor);

    for (int i = 0; i < alphas.Length; i++)
      alphas[i].Init(alphas[i].pos, SetAlpha, null);

    selected.color = Color.white;
    BasicWhite.Init(0, SetPrimary, null);
    BasicRed.Init(1, SetPrimary, null);
    BasicYellow.Init(2, SetPrimary, null);
    BasicGreen.Init(3, SetPrimary, null);
    BasicCyan.Init(4, SetPrimary, null);
    BasicBlue.Init(5, SetPrimary, null);
    BasicPurple.Init(6, SetPrimary, null);
    BasicBlack.Init(7, SetPrimary, null);
  }

  public void SetPrimary(int pos) {
    switch(pos) {
      case 0: r = 3; g = 3; b = 3; a = 3; break;
      case 1: r = 3; g = 0; b = 0; a = 3; break;
      case 2: r = 3; g = 3; b = 0; a = 3; break;
      case 3: r = 0; g = 3; b = 0; a = 3; break;
      case 4: r = 0; g = 3; b = 3; a = 3; break;
      case 5: r = 0; g = 0; b = 3; a = 3; break;
      case 6: r = 3; g = 0; b = 3; a = 3; break;
      case 7: r = 0; g = 0; b = 0; a = 3; break;
    }
    byte r256 = (byte)(r * 85);
    byte g256 = (byte)(g * 85);
    byte b256 = (byte)(b * 85);
    byte a256 = (byte)(a * 85);

    foreach (PixelCube p in colors) {
      Color32 pcol = p.img.color;
      if (p.r == -1) pcol.r = r256;
      if (p.g == -1) pcol.g = g256;
      if (p.b == -1) pcol.b = b256;
      p.img.color = pcol;
    }
    selected.color = new Color32(r256, g256, b256, a256);
    for (int i = 5; i < 10; i++) 
      alphas[i].img.color = new Color32(r256, g256, b256, (byte)(alphas[i].img.color.a * 255.9f));
  }

  public void SetColor(int r, int g, int b) {
    Color32 col = selected.color;
    if (r == -1) {
      col.g = (byte)(g * 85);
      col.b = (byte)(b * 85);
    }
    else if (g == -1) {
      col.r = (byte)(r * 85);
      col.b = (byte)(b * 85);
    }
    else if (b == -1) {
      col.r = (byte)(r * 85);
      col.g = (byte)(g * 85);
    }

    foreach (PixelCube p in colors) {
      Color32 pcol = p.img.color;
      if (p.r == -1) pcol.r = col.r;
      if (p.g == -1) pcol.g = col.g;
      if (p.b == -1) pcol.b = col.b;
      p.img.color = pcol;
    }

    selected.color = col;
    for (int i = 5; i < 10; i++) {
      col.a = (byte)(alphas[i].img.color.a * 255.9f);
      alphas[i].img.color = col;
    }
  }

  public void SetAlpha(int pos) {
    Color32 col = selected.color;
    col.a = (byte)pos;
    if (pos < 40) { col = new Color32(0, 0, 0, 0); }
    selected.color = col;
  }
}
