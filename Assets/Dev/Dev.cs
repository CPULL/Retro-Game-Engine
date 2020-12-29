using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Dev : MonoBehaviour {

  private void Start() {
    WidthSlider.SetValueWithoutNotify(8);
    HeightSlider.SetValueWithoutNotify(8);
    pixels = new Pixel[0];
    ChangeSpriteSize();
  }

  #region Sprite Editor ************************************************************************************************************************************************************************************

  public GameObject PixelPrefab;
  public GridLayoutGroup SpriteGrid;
  Pixel[] pixels;
  public Slider WidthSlider;
  public Slider HeightSlider;
  public Text WidthSliderText;
  public Text HeightSliderText;
  public InputField Values;
  public Button LoadSubButton;

  public Image CurrentColor;
  Color32 Transparent = new Color32(0, 0, 0, 0);

  public void ChangeSpriteSize() {
    WidthSliderText.text = "Width: " + WidthSlider.value;
    HeightSliderText.text = "Height: " + HeightSlider.value;
    Rect rt = SpriteGrid.transform.GetComponent<RectTransform>().rect;
    SpriteGrid.cellSize = new Vector2(rt.width / WidthSlider.value, rt.height / HeightSlider.value);

    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    int numnow = num;
    Pixel[] pixels2 = new Pixel[num];
    if (pixels.Length < num) num = pixels.Length;
    for (int i = 0; i < num; i++)
      pixels2[i] = pixels[i];
    pixels = pixels2;
    if (num < numnow) {
      for (int i = num; i < numnow; i++) {
        Pixel pixel = Instantiate(PixelPrefab, SpriteGrid.transform).GetComponent<Pixel>();
        pixels2[i] = pixel;
        pixel.Init(i, Transparent, ClickPixel);
      }
    }
    else if (num > numnow) {
      for (int i = numnow; i < num; i++) {
        Destroy(SpriteGrid.transform.GetChild(i));
      }
    }
  }

  private void ClickPixel(int pos) {
    if (pixels[pos].img.color == CurrentColor.color)
      pixels[pos].Set(Transparent);
    else
      pixels[pos].Set(CurrentColor.color);
  }

  public void Clear() {
    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    for (int i = 0; i < num; i++)
      pixels[i].Set(Transparent);
  }

  public void Fill() {
    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    for (int i = 0; i < num; i++)
      pixels[i].Set(CurrentColor.color);
  }

  public void Save() {
    string res = "SpriteSize:\n";
    byte sizex = ((byte)((byte)WidthSlider.value & 31));
    byte sizey = ((byte)((byte)HeightSlider.value & 31));
    res += "0x" + sizex.ToString("X2") + " 0x" + sizey.ToString("X2") + "\n";
    res += "Sprite:";
    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    for (int i = 0; i < num; i++) {
      if (i % sizex == 0) res += "\n";
      Color32 c = pixels[i].img.color;
      int r = c.r / 85;
      int g = c.g / 85;
      int b = c.b / 85;
      int a = 255 - (c.a / 85); 
      byte col = (byte)((a << 6) + (r << 4) + (g << 2) + (b << 0));
      res += "0x" + col.ToString("X2") + " ";
    }
    Values.gameObject.SetActive(true);
    Values.text = res;
  }

  public void PreLoad() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }

  Regex rgComments = new Regex("([^\\n]*)(//[^\\n]*)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgLabels = new Regex("[\\s]*[a-z0-9]+:[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgHex = new Regex("[\\s]*0x([a-f0-9]+)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  public void PostLoad() {
    string data = Values.text.Trim();
    data = rgComments.Replace(data, " ");
    data = rgLabels.Replace(data, " ");
    data = data.Replace('\n', ' ').Trim();
    while (data.IndexOf("  ") != -1) data = data.Replace("  ", " ");

    data = ReadNextByte(data, out byte w);
    data = ReadNextByte(data, out byte h);
    if (w < 8 || h < 8 || w > 32 || h > 32) {
      Values.text = "This does not look like a sprite.\n" + Values.text;
      return;
    }

    WidthSlider.SetValueWithoutNotify(w);
    HeightSlider.SetValueWithoutNotify(h);
    ChangeSpriteSize();
    for (int i = 0; i < w * h; i++) {
      data = ReadNextByte(data, out byte col);

      int r = (col & 0b110000) >> 4;
      int g = (col & 0b001100) >> 2;
      int b = (col & 0b000011) >> 0;
      int a = 3 - ((col & 0b11000000) >> 6);
      pixels[i].Set(new Color32((byte)(r * 85), (byte)(g * 85), (byte)(b * 85), (byte)(a * 85)));
    }

    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
  }

  string ReadNextByte(string data, out byte res) {
    int pos1 = data.IndexOf(' ');
    int pos2 = data.Length;
    if (pos1 == -1) pos1 = int.MaxValue;
    if (pos2 == -1) pos1 = int.MaxValue;
    int pos = pos1;
    if (pos > pos2) pos = pos2;
    if (pos < 1) {
      res = 0;
      return "";
    }

    string part = data.Substring(0, pos);
    Match m = rgHex.Match(part);
    if (m.Success) {
      res = (byte)Convert.ToInt32(m.Groups[1].Value, 16);
      return data.Substring(pos).Trim();
    }

    res = 0;
    return data;
  }

  public void CloseValues() {
    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
  }



  #endregion Sprite Editor
}
