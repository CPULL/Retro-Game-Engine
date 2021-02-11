using UnityEngine;
using UnityEngine.UI;

public class Palette : MonoBehaviour {
  public RawImage[] blocks;
  public Slider[] blockSliders;
  readonly Texture2D[] texts = new Texture2D[3];
  public Color32 MainColor = Color.white;
  public Image FrontSelectedColor;
  public Pixel[] BasicColors;
  public Pixel[] AlphaColors;
  public SpriteEditor spriteEditor;

  private void Start() {
    for (int i = 0; i < BasicColors.Length; i++) {
      Pixel p = BasicColors[i];
      p.InitBasic(i, SetColor);
    }

    for (int i = 0; i < 40; i++) {
      AlphaColors[i].InitBasic(i, Col.alphas[i], SetAlpha);
    }

    for (int i = 0; i < 3; i++) {
      blockSliders[i].SetValueWithoutNotify(5);
      texts[i] = new Texture2D(6, 6, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
      for (int x = 0; x < 6; x++)
        for (int y = 0; y < 6; y++) {
          byte col = 0;
          if (i == 2) col = (byte)(x * 36 + y * 6 + 5);
          else if (i == 1) col = (byte)(x * 36 + 30 + y);
          else if (i == 0) col = (byte)(180 + x * 6 + y);
          texts[i].SetPixel(x, y, Col.GetColor(col));
        }
      texts[i].Apply();
      blocks[i].texture = texts[i];
    }
  }

  private void SetColor(Pixel p, bool _) {
    switch (p.pos) {
      case 0:
        blockSliders[0].SetValueWithoutNotify(0);
        blockSliders[1].SetValueWithoutNotify(0);
        blockSliders[2].SetValueWithoutNotify(0);
        break;
      case 1:
        blockSliders[0].SetValueWithoutNotify(2);
        blockSliders[1].SetValueWithoutNotify(2);
        blockSliders[2].SetValueWithoutNotify(2);
        break;
      case 2:
        blockSliders[0].SetValueWithoutNotify(5);
        blockSliders[1].SetValueWithoutNotify(5);
        blockSliders[2].SetValueWithoutNotify(5);
        break;
      case 3:
        blockSliders[0].SetValueWithoutNotify(5);
        blockSliders[1].SetValueWithoutNotify(0);
        blockSliders[2].SetValueWithoutNotify(0);
        break;
      case 4:
        blockSliders[0].SetValueWithoutNotify(5);
        blockSliders[1].SetValueWithoutNotify(5);
        blockSliders[2].SetValueWithoutNotify(0);
        break;
      case 5:
        blockSliders[0].SetValueWithoutNotify(0);
        blockSliders[1].SetValueWithoutNotify(5);
        blockSliders[2].SetValueWithoutNotify(0);
        break;
      case 6:
        blockSliders[0].SetValueWithoutNotify(0);
        blockSliders[1].SetValueWithoutNotify(5);
        blockSliders[2].SetValueWithoutNotify(5);
        break;
      case 7:
        blockSliders[0].SetValueWithoutNotify(0);
        blockSliders[1].SetValueWithoutNotify(0);
        blockSliders[2].SetValueWithoutNotify(5);
        break;
      case 8:
        blockSliders[0].SetValueWithoutNotify(5);
        blockSliders[1].SetValueWithoutNotify(0);
        blockSliders[2].SetValueWithoutNotify(5);
        break;
    }
    AlterColor();
    spriteEditor.SetCurrentColor(Col.GetByteFrom6((int)blockSliders[0].value, (int)blockSliders[1].value, (int)blockSliders[2].value, 255));
  }

  private void SetAlpha(Pixel p, bool _) {
    MainColor = Col.alphas[p.pos];
    FrontSelectedColor.color = MainColor;
    spriteEditor.SetCurrentColor(216 + p.pos);
  }

  void UpdateColors(byte r, byte g, byte b) {
    for (byte x = 0; x < 6; x++)
      for (byte y = 0; y < 6; y++) {
        texts[0].SetPixel(x, y, Col.GetColorFrom6(r, x, y));
        texts[1].SetPixel(x, y, Col.GetColorFrom6(x, g, y));
        texts[2].SetPixel(x, y, Col.GetColorFrom6(x, y, b));
      }
    for (int i = 0; i < 3; i++) {
      texts[i].Apply();
      blocks[i].texture = texts[i];
    }
  }
  

  public void AlterColor() {
    byte r = (byte)blockSliders[0].value;
    byte g = (byte)blockSliders[1].value;
    byte b = (byte)blockSliders[2].value;
    UpdateColors(r, g, b);
    MainColor = Col.GetColorFrom6(r, g, b);
    FrontSelectedColor.color = MainColor;
    spriteEditor.SetCurrentColor(Col.GetByteFrom6(r, g, b, MainColor.a * 255));
  }

  public void AlterColor(byte col) {
    FrontSelectedColor.color = Col.GetColor(col);
    MainColor = FrontSelectedColor.color;
    blockSliders[0].SetValueWithoutNotify((int)(FrontSelectedColor.color.r * 5));
    blockSliders[1].SetValueWithoutNotify((int)(FrontSelectedColor.color.g * 5));
    blockSliders[2].SetValueWithoutNotify((int)(FrontSelectedColor.color.b * 5));
    UpdateColors((byte)blockSliders[0].value, (byte)blockSliders[1].value, (byte)blockSliders[2].value);
  }
}
