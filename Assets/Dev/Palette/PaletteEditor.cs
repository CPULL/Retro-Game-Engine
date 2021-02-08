using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PaletteEditor : MonoBehaviour {
  readonly Pixel[] pixels = new Pixel[256];
  public Transform PaletteContainer;
  public Slider HueSlider;
  public Slider RSlider;
  public TextMeshProUGUI RVal;
  public Slider GSlider;
  public TextMeshProUGUI GVal;
  public Slider BSlider;
  public TextMeshProUGUI BVal;
  public Slider ASlider;
  public TextMeshProUGUI AVal;
  public TMP_InputField HexColor;
  public RawImage SelectedColor;
  public RawImage ColorPicker;
  Texture2D ColorPickerTexture;
  public RectTransform ColorPickerH;
  public RectTransform ColorPickerV;
  readonly ColorImageQuantizer ciq = new ColorImageQuantizer(new MedianCutQuantizer());
  readonly Color[] palette = new Color[256];


  void Start() {
    int pos = 0;
    foreach(Transform t in PaletteContainer) {
      pixels[pos] = t.GetComponent<Pixel>();
      palette[pos] = Col.GetColor((byte)pos);
      pixels[pos].Init(pos, palette[pos], SelectPalettePixel, Color.black);
      pos++;
    }
    RGEPalette.SetColorArray("_Colors", palette);

    RSlider.SetValueWithoutNotify(255);
    GSlider.SetValueWithoutNotify(255);
    BSlider.SetValueWithoutNotify(255);
    ASlider.SetValueWithoutNotify(255);
    SetColor();
  }

  public void SetColor() {
    byte r = (byte)RSlider.value;
    byte g = (byte)GSlider.value;
    byte b = (byte)BSlider.value;
    byte a = (byte)ASlider.value;
    SelectedColor.color = new Color32(r, g, b, a);
    RVal.text = r.ToString();
    GVal.text = g.ToString();
    BVal.text = b.ToString();
    AVal.text = a.ToString();
    Color.RGBToHSV(SelectedColor.color, out float h, out float s, out float v);
    HueSlider.SetValueWithoutNotify(h * 360);
    ColorPickerV.anchoredPosition = new Vector2(s * 256, 0);
    ColorPickerH.anchoredPosition = new Vector2(0, -256 * (1 - v));
    ColorPickerTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
    for (int y = 255; y >= 0; y--)
      for (int x = 0; x < 256; x++)
        ColorPickerTexture.SetPixel(
          x, y,
          Color.HSVToRGB(h, x / 255f, y / 255f));
    ColorPickerTexture.Apply();
    ColorPicker.texture = ColorPickerTexture;
    if (a == 255)
      HexColor.SetTextWithoutNotify(r.ToString("X2") + g.ToString("X2") + g.ToString("X2"));
    else
      HexColor.SetTextWithoutNotify(r.ToString("X2") + g.ToString("X2") + g.ToString("X2") + a.ToString("X2"));
    SetSelectedPixel();
  }

  readonly Regex rghex = new Regex("[0-9a-f]{3,8}", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));
  public void SetColorHex() {
    string hex = HexColor.text.Trim();
    if ((hex.Length != 3 && hex.Length != 4 && hex.Length != 6 && hex.Length != 8) || !rghex.IsMatch(hex)) {
      uint hx = (uint)(((byte)RSlider.value << 24) + ((byte)GSlider.value << 16) + ((byte)BSlider.value << 8) + ((byte)ASlider.value));
      HexColor.SetTextWithoutNotify(hx.ToString("x4"));
      return;
    }
    byte r=0, g=0, b=0, a=255;
    if (hex.Length == 3) {
      r = (byte)System.Convert.ToInt32(hex.Substring(0, 1), 16);
      g = (byte)System.Convert.ToInt32(hex.Substring(1, 1), 16);
      b = (byte)System.Convert.ToInt32(hex.Substring(2, 1), 16);
      r *= 0x11;
      g *= 0x11;
      b *= 0x11;
      a = (byte)ASlider.value;
    }
    else if (hex.Length == 4) {
      r = (byte)System.Convert.ToInt32(hex.Substring(0, 1), 16);
      g = (byte)System.Convert.ToInt32(hex.Substring(1, 1), 16);
      b = (byte)System.Convert.ToInt32(hex.Substring(2, 1), 16);
      a = (byte)System.Convert.ToInt32(hex.Substring(3, 1), 16);
      r *= 0x11;
      g *= 0x11;
      b *= 0x11;
      a *= 0x11;
    }
    else if (hex.Length == 6) {
      r = (byte)System.Convert.ToInt32(hex.Substring(0, 2), 16);
      g = (byte)System.Convert.ToInt32(hex.Substring(2, 2), 16);
      b = (byte)System.Convert.ToInt32(hex.Substring(4, 2), 16);
      a = (byte)ASlider.value;
    }
    else if (hex.Length == 8) {
      r = (byte)System.Convert.ToInt32(hex.Substring(0, 2), 16);
      g = (byte)System.Convert.ToInt32(hex.Substring(2, 2), 16);
      b = (byte)System.Convert.ToInt32(hex.Substring(4, 2), 16);
      a = (byte)System.Convert.ToInt32(hex.Substring(6, 2), 16);
    }

    RSlider.SetValueWithoutNotify(r);
    GSlider.SetValueWithoutNotify(g);
    BSlider.SetValueWithoutNotify(b);
    ASlider.SetValueWithoutNotify(a);
    SelectedColor.color = new Color32(r, g, b, a);
    RVal.text = r.ToString();
    GVal.text = g.ToString();
    BVal.text = b.ToString();
    AVal.text = a.ToString();
    Color.RGBToHSV(SelectedColor.color, out float h, out float s, out float v);
    HueSlider.SetValueWithoutNotify(h * 360);
    ColorPickerV.anchoredPosition = new Vector2(s * 256, 0);
    ColorPickerH.anchoredPosition = new Vector2(0, -256 * (1 - v));
    ColorPickerTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
    for (int y = 255; y >= 0; y--)
      for (int x = 0; x < 256; x++)
        ColorPickerTexture.SetPixel(
          x, y,
          Color.HSVToRGB(h, x / 255f, y / 255f));
    ColorPickerTexture.Apply();
    ColorPicker.texture = ColorPickerTexture;
    SetSelectedPixel();
  }

  public void SetColorH() {
    float h = HueSlider.value / 360f;
    Color.RGBToHSV(SelectedColor.color, out _, out float s, out float v);
    Color c = Color.HSVToRGB(h, s, v);
    byte r = (byte)(c.r * 255);
    byte g = (byte)(c.g * 255);
    byte b = (byte)(c.b * 255);
    RSlider.SetValueWithoutNotify(r);
    GSlider.SetValueWithoutNotify(g);
    BSlider.SetValueWithoutNotify(b);
    byte a = (byte)ASlider.value;
    SelectedColor.color = new Color32(r, g, b, a);
    RVal.text = r.ToString();
    GVal.text = g.ToString();
    BVal.text = b.ToString();
    AVal.text = a.ToString();
    ColorPickerV.anchoredPosition = new Vector2(s * 256, 0);
    ColorPickerH.anchoredPosition = new Vector2(0, -256 * (1 - v));
    ColorPickerTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
    for (int y = 255; y >= 0; y--)
      for (int x = 0; x < 256; x++)
        ColorPickerTexture.SetPixel(
          x, y,
          Color.HSVToRGB(h, x / 255f, y / 255f));
    ColorPickerTexture.Apply();
    ColorPicker.texture = ColorPickerTexture;
    if (a == 255)
      HexColor.SetTextWithoutNotify(r.ToString("X2") + g.ToString("X2") + g.ToString("X2"));
    else
      HexColor.SetTextWithoutNotify(r.ToString("X2") + g.ToString("X2") + g.ToString("X2") + a.ToString("X2"));
    SetSelectedPixel();
  }

  public void SetColorSV(float s, float v) {
    float h = HueSlider.value / 360f;
    Color c = Color.HSVToRGB(h, s, v);
    byte r = (byte)(c.r * 255);
    byte g = (byte)(c.g * 255);
    byte b = (byte)(c.b * 255);
    RSlider.SetValueWithoutNotify(r);
    GSlider.SetValueWithoutNotify(g);
    BSlider.SetValueWithoutNotify(b);
    byte a = (byte)ASlider.value;
    SelectedColor.color = new Color32(r, g, b, a);
    RVal.text = r.ToString();
    GVal.text = g.ToString();
    BVal.text = b.ToString();
    AVal.text = a.ToString();
    ColorPickerTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
    for (int y = 255; y >= 0; y--)
      for (int x = 0; x < 256; x++)
        ColorPickerTexture.SetPixel(
          x, y,
          Color.HSVToRGB(h, x / 255f, y / 255f));
    ColorPickerTexture.Apply();
    ColorPicker.texture = ColorPickerTexture;
    if (a == 255)
      HexColor.SetTextWithoutNotify(r.ToString("X2") + g.ToString("X2") + g.ToString("X2"));
    else
      HexColor.SetTextWithoutNotify(r.ToString("X2") + g.ToString("X2") + g.ToString("X2") + a.ToString("X2"));
    SetSelectedPixel();
  }

  Pixel selectedPixel = null;
  void SelectPalettePixel(Pixel p) {
    if (selectedPixel != null) selectedPixel.Deselect();
    Color32 c = p.Get32();
    RSlider.SetValueWithoutNotify(c.r);
    GSlider.SetValueWithoutNotify(c.g);
    BSlider.SetValueWithoutNotify(c.b);
    ASlider.SetValueWithoutNotify(c.a);
    p.Select();
    selectedPixel = p;
    SetColor();
  }

  void SetSelectedPixel() {
    if (selectedPixel == null) return;
    selectedPixel.Set32(SelectedColor.color);
    palette[selectedPixel.pos] = pixels[selectedPixel.pos].Get32();
    RGEPalette.SetColorArray("_Colors", palette);
  }


  public RawImage MainPicOrig;
  public RawImage MainPicPalette;
  public void LoadFile() {
    FileBrowser.Load(PostLoadImage, FileBrowser.FileType.Pics);
  }

  void PostLoadImage(string path) {
    StartCoroutine(LoadImageCoroutine(path));
  }
  IEnumerator LoadImageCoroutine(string path) {
    string url = string.Format("file://{0}", path);

    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url)) {
      yield return www.SendWebRequest();
      yield return PBar.Show("Loading file", 5, 10);
      Texture2D texture = DownloadHandlerTexture.GetContent(www);
      // Get the top-left part of the image fitting in the sprite size
      yield return PBar.Progress(5);

      float sw = (int)PicSizeH.value * 8;
      if (sw < 8) sw = 8;
      if (sw > 320) sw = 320;
      float sh = (int)PicSizeV.value * 4;
      if (sh < 8) sh = 8;
      if (sh > 256) sh = 256;
      TextureScale.Point(texture, (int)sw, (int)sh);
      texture.filterMode = FilterMode.Point;
      texture.Apply();
      Texture2D palText = new Texture2D((int)sw, (int)sh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
      palText.SetPixels32(texture.GetPixels32());
      texture.Apply();
      palText.Apply();

      yield return PBar.Progress(10);
      PBar.Hide();
      MainPicOrig.texture = texture;
      MainPicPalette.texture = palText;
      ChangePicSizeCompleted();
    }
  }
  public GameObject PicSizeVals;
  public Slider PicSizeH;
  public Slider PicSizeV;
  public TextMeshProUGUI PicSizeText;
  public TextMeshProUGUI PicSizeSubText;
  public void ChangePicSize() {
    PicSizeVals.SetActive(true);
  }

  public void ChangePicSizeCompleted() {
    PicSizeVals.SetActive(false);
    float w = (int)PicSizeH.value * 8;
    if (w < 8) w = 8;
    if (w > 320) w = 320;
    float h = (int)PicSizeV.value * 4;
    if (h < 8) h = 8;
    if (h > 256) h = 256;

    if (w > h) {
      h = 512 * h / w;
      w = 512;
    }
    else {
      w = 512 * w / h;
      h = 512;
    }
    MainPicOrig.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
  }

  public void ChangeScreenSlider() {
    float w = (int)PicSizeH.value * 8;
    if (w < 8) w = 8;
    if (w > 320) w = 320;
    float h = (int)PicSizeV.value * 4;
    if (h < 8) h = 8;
    if (h > 256) h = 256;
    PicSizeText.text = "Size\n" + w + "x" + h;
    PicSizeSubText.text = w + "x" + h;
  }

  public void GenerateBestPalette() {
    StartCoroutine(GeneratingBestPalette());
  }
  public void ApplyPalette() {
    StartCoroutine(ApplyingPalette());
  }
  public void ApplyDefaultPalette() {
    StartCoroutine(GeneratingBestPalette());
  }

  IEnumerator GeneratingBestPalette() {
    Texture2D texture = (Texture2D)MainPicOrig.texture;
    yield return PBar.Show("Generating", 0, 2); ;

    int num = (int)NumColorsS.value;
    int sa = (int)StartAtS.value;

    Color32[] colorTable = ciq.CalculatePalette(texture, num);
    yield return PBar.Progress(1);

    Color32[] colors = new Color32[256];
    colors[0] = Black;
    colors[255] = Transparent;
    for (int i = 0; i < 256; i++) {
      if (i < sa || i >= sa + num)
        colors[i] = pixels[i].Get32();
      else {
        colors[i] = colorTable[i - sa];
        pixels[i].Set32(colors[i]);
      }
    }
    yield return PBar.Progress(2);

    Texture2D newImage = ciq.ReduceColors(texture, colors);
    newImage.Apply();
    MainPicPalette.texture = newImage;
    yield return PBar.Progress(3);

    PBar.Hide();
  }

  public void ShufflePalette() {
    Rhsv[] cs = new Rhsv[254];
    for (int i = 1; i < 255; i++) {
      Color.RGBToHSV(pixels[i].Get32(), out float h, out float s, out float v);
      cs[i - 1] = new Rhsv(h, s, v);
    }
    Array.Sort(cs, delegate (Rhsv x, Rhsv y) {
      Color32 a = x.C32();
      Color32 b = y.C32();
      int c = a.r.CompareTo(b.r);
      if (c != 0) return c;
      c = a.g.CompareTo(b.g);
      if (c != 0) return c;
      return a.b.CompareTo(b.b);
    });

    palette[0] = Color.black;
    palette[255] = Transparent;
    for (int i = 1; i < 255; i++) {
      palette[i] = cs[i - 1].C32();
      pixels[i].Set32(palette[i]);
    }
    RGEPalette.SetColorArray("_Colors", palette);
  }

  public void UseDefaultPalette() {
    for (int i = 0; i < 256; i++) {
      pixels[i].Set32(Col.GetColor((byte)i));
      string id = "_Color" + i.ToString("X2");
      RGEPalette.SetColor(id, pixels[i].Get32());
    }
  }

  IEnumerator ApplyingPalette() {
    int w = MainPicOrig.texture.width;
    int h = MainPicOrig.texture.height;
    yield return PBar.Show("Applying palette", 0, 1 + h);

    Color32[] colors = new Color32[256];
    for (int i = 0; i < 256; i++)
      colors[i] = palette[i];
    Texture2D newImage = ciq.ReduceColors((Texture2D)MainPicOrig.texture, colors);
    PBar.Progress(1);
    Texture2D palt = (Texture2D)MainPicPalette.texture;
    Color32[] cols = newImage.GetPixels32();
    for (int y = 0; y < h; y++) {
      PBar.Progress(1 + y);
      for (int x = 0; x < w; x++) {
        int pos = x + w * y;
        for (int i = 0; i < 256; i++) {
          if (cols[pos].Equals(colors[i])) {
            int hi = ((i & 0xF0) >> 4) * 8 + 4;
            int lo = (i & 0xF) * 8 + 4;
            palt.SetPixel(x, y, new Color(hi / 255f, lo / 255f, 0, 255));
            break;
          }
        }
      }
    }
    palt.Apply();
    MainPicPalette.texture = palt;
    PBar.Hide();
  }

  public Toggle PaletteModeToggle;
  public void AlterPaletteMode() {
    MainPicOrig.enabled = !PaletteModeToggle.isOn;
    MainPicPalette.enabled = PaletteModeToggle.isOn;
  }

  public Slider NumColorsS;
  public TextMeshProUGUI NumColorsT;
  public Slider StartAtS;
  public TextMeshProUGUI StartAtT;

  public void ChangeMaxColors() {
    int num = (int)NumColorsS.value;
    NumColorsT.text = "Num colors: " + num;
    if (num + StartAtS.value > 254) {
      StartAtS.SetValueWithoutNotify(254 - num);
      StartAtT.text = "Start at: " + (254 - num);
    }
  }

  public void ChangeStartAt() {
    int num = (int)NumColorsS.value;
    int sa = (int)StartAtS.value;
    StartAtT.text = "Start at: " + sa;
    if (num + StartAtS.value > 254) {
      NumColorsS.SetValueWithoutNotify(254 - sa);
      NumColorsT.text = "Num colors: " + (254 - sa);
    }
  }

  Color32 Transparent = new Color32(0, 0, 0, 0);
  Color32 Black = new Color32(0, 0, 0, 255);
  public Material RGEPalette;
}

/*

Use some sort of selection tool (left and right clicks on pixels) to select the start and end of the part of the palette to handle
Implement move and copy/paste of selected palette range
Fix color mapping (to convert to indexed colors)



Load Sprite
Load Tilemap
Save Image/Sprite
Generate palette from image
Update image with current palette
Toggle Normal/Palette modes


a way to get a sprite from normal and convert to palette (generating palette or using current palette)
a way to save a converted sprite
a way to convert back a sprite to normal mode

load/save buttons (bin and text)


*/