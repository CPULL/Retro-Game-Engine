using UnityEngine;
using UnityEngine.UI;

public class Palette : MonoBehaviour {
  public Pixel[] colors;
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
    for (int i = 0; i < colors.Length; i++) {
      colors[i].Init(i, SetColor);
    }
    selected.color = Color.white;
    BasicWhite.Init(0, SetPrimary);
    BasicRed.Init(1, SetPrimary);
    BasicYellow.Init(2, SetPrimary);
    BasicGreen.Init(3, SetPrimary);
    BasicCyan.Init(4, SetPrimary);
    BasicBlue.Init(5, SetPrimary);
    BasicPurple.Init(6, SetPrimary);
    BasicBlack.Init(7, SetPrimary);
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
    for (int i = 0; i < 4; i++) {
      colors[i + 0].Set(new Color32((byte)(255 - i * 85), g256, b256, 255));
      colors[i + 4].Set(new Color32(r256, (byte)(255 - i * 85), b256, 255));
      colors[i + 8].Set(new Color32(r256, g256, (byte)(255 - i * 85), 255));
    }

    selected.color = new Color32(r256, g256, b256, a256);
  }

  public void SetColor(int pos) {
    if (pos < 4) r = (byte)(3 - pos);
    else if (pos < 8) g = (byte)(3 - pos & 3);
    else if (pos < 12) b = (byte)(3 - pos & 3);
    else a = (byte)(3 - pos & 3);

    byte r256 = (byte)(r * 85);
    byte g256 = (byte)(g * 85);
    byte b256 = (byte)(b * 85);
    byte a256 = (byte)(a * 85);

    for (int i = 0; i < 4; i++) {
      colors[i + 0].Set(new Color32((byte)(255 - i * 85), g256, b256, 255));
      colors[i + 4].Set(new Color32(r256, (byte)(255 - i * 85), b256, 255));
      colors[i + 8].Set(new Color32(r256, g256, (byte)(255 - i * 85), 255));
    }

    selected.color = new Color32(r256, g256, b256, a256);
  }
}
