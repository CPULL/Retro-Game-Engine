using UnityEngine;
using UnityEngine.UI;

public class PaletteCube : MonoBehaviour {
  public Pixel[] colors;
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
      colors[i].Init(colors[i].pos, SetColor, null);

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

    foreach(Pixel p in colors) {
      int idx = p.pos;
      Color32 col = p.img.color;
      if ((idx / 25) == 4) col.r = r256;
      if ((idx / 5) % 5 == 4) col.g = g256;
      if (idx % 5 == 4) col.b = b256;
      p.img.color = col;
    }
    selected.color = new Color32(r256, g256, b256, a256);
  }

  public void SetColor(int idx) {
    Color32 col = selected.color;
    int r = idx / 25;
    int g = (idx / 5) % 5;
    int b = idx % 5;
    if (r == 4) {
      col.g = (byte)(g * 85);
      col.b = (byte)(b * 85);
    }
    else if (g == 4) {
      col.r = (byte)(r * 85);
      col.b = (byte)(b * 85);
    }
    else if (b == 4) {
      col.r = (byte)(r * 85);
      col.g = (byte)(g * 85);
    }

    foreach (Pixel p in colors) {
      int pos = p.pos;
      Color32 pcol = p.img.color;
      if ((pos / 25) == 4) pcol.r = col.r;
      if ((pos / 5) % 5 == 4) pcol.g = col.g;
      if (pos % 5 == 4) pcol.b = col.b;
      p.img.color = pcol;
    }

    selected.color = col;
  }

  public void SetAlpha(int pos) {
    Color32 col = selected.color;
    col.a = (byte)pos;
    selected.color = col;
  }
}
