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
  readonly Pixel[] Pixels = new Pixel[256];
  readonly Color[] palette = new Color[256];


  void Start() {
    int pos = 0;
    foreach(Transform t in PaletteContainer) {
      pixels[pos] = t.GetComponent<Pixel>();
      palette[pos] = Col.GetColor((byte)pos);
      pixels[pos].Init(pos, palette[pos], SelectPalettePixel, Color.black);
      Pixels[pos] = pixels[pos];
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
  int minsel = -1;
  int maxsel = -1;
  void SelectPalettePixel(Pixel p, bool left) {
    if (selectedPixel != null) selectedPixel.Deselect();
    Color32 c = p.Get32();
    RSlider.SetValueWithoutNotify(c.r);
    GSlider.SetValueWithoutNotify(c.g);
    BSlider.SetValueWithoutNotify(c.b);
    ASlider.SetValueWithoutNotify(c.a);
    p.Select();
    selectedPixel = p;
    SetColor();

    if (left)
      minsel = p.pos;
    else
      maxsel = p.pos;
    if (minsel != -1 && maxsel != -1) {
      if (minsel > maxsel) { int tmp = minsel; minsel = maxsel; maxsel = tmp; }
      if (minsel < 1) minsel = 1;
      if (maxsel > 254) maxsel = 254;
      for (int i = 0; i < 256; i++) {
        pixels[i].InRange(i >= minsel && i <= maxsel);
      }
    }
  }

  public void SelectColors(bool all) {
    if (all) {
      minsel = 1;
      maxsel = 254;
      for (int i = 1; i < 255; i++)
        pixels[i].InRange(true);
    }
    else {
      minsel = -1;
      maxsel = -1;
      for (int i = 1; i < 255; i++)
        pixels[i].InRange(false);
    }
  }

  public void RemoveDuplicates() {
    for (int i = 1; i < 255; i++) {
      Color32 c = palette[i];
      bool duplicated = false;
      for (int j = 0; j < 256; j++) {
        if (i != j && palette[j] == c) {
          duplicated = true;
          break;
        }
      }
      if (duplicated) {
        palette[i] = Transparent;
      }
      pixels[i].Set32(palette[i]);
    }
    RGEPalette.SetColorArray("_Colors", palette);
  }

  void SetSelectedPixel() {
    if (selectedPixel == null) return;
    selectedPixel.Set32(SelectedColor.color);
    palette[selectedPixel.pos] = pixels[selectedPixel.pos].Get32();
    RGEPalette.SetColorArray("_Colors", palette);
  }


  public RawImage PicOrig;
  public RawImage PicPalette;
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
      PicOrig.texture = texture;
      PicPalette.texture = palText;
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
    PicOrig.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
    PicPalette.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
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
    if (PicOrig.texture == null) {
      Dev.inst.HandleError("No image loaded!");
      return;
    }
    StartCoroutine(GeneratingBestPalette());
  }
  public void ApplyPalette() {
    if (PicOrig.texture == null) {
      Dev.inst.HandleError("No image loaded!");
      return;
    }
    StartCoroutine(ApplyingPalette());
  }
  public void ApplyDefaultPalette() {
    if (PicOrig.texture == null) {
      Dev.inst.HandleError("No image loaded!");
      return;
    }
    StartCoroutine(GeneratingBestPalette());
  }

  IEnumerator GeneratingBestPalette() {
    Texture2D texture = (Texture2D)PicOrig.texture;
    yield return PBar.Show("Generating", 0, 2); ;

    int num = 254;
    if (minsel != -1 && maxsel != -1) {
      num = maxsel - minsel + 1;
    }

    Color32[] colorTable = ciq.CalculatePalette(texture, num);
    yield return PBar.Progress(1);

    int start = minsel == -1 ? 1 : minsel;
    int end = maxsel == -1 ? 254 : maxsel;
    for (int i = start; i <= end; i++) {
      palette[i] = colorTable[i - start];
      pixels[i].Set32(palette[i]);
    }
    yield return PBar.Progress(2);

    Texture2D newImage = ciq.ReduceColors(texture, colorTable);
    newImage.Apply();
    PicPalette.texture = newImage;
    yield return PBar.Progress(3);
    RGEPalette.SetColorArray("_Colors", palette);

    PBar.Hide();
  }

  public void ShufflePalette() {
    Color32[] tosort = new Color32[254];
    for (int i = 1; i < 255; i++)
      tosort[i - 1] = palette[i];
    Array.Sort(tosort, delegate (Color32 x, Color32 y) {
      int c = y.a.CompareTo(x.a);
      if (c != 0) return c;
      c = x.r.CompareTo(y.r);
      if (c != 0) return c;
      c = x.g.CompareTo(y.g);
      if (c != 0) return c;
      return x.b.CompareTo(y.b);
    });

    palette[0] = Color.black;
    palette[255] = Transparent;
    for (int i = 1; i < 255; i++) {
      palette[i] = tosort[i - 1];
      pixels[i].Set32(palette[i]);
    }
    RGEPalette.SetColorArray("_Colors", palette);
  }

  public void UseDefaultPalette() {
    for (int i = 0; i < 256; i++) {
      palette[i] = Col.GetColor((byte)i);
      pixels[i].Set32(palette[i]);
    }
    RGEPalette.SetColorArray("_Colors", palette);
  }

  IEnumerator ApplyingPalette() {
    int w = PicOrig.texture.width;
    int h = PicOrig.texture.height;
    yield return PBar.Show("Applying palette", 0, 1 + h);

    Color32[] colors = new Color32[256];
    for (int i = 0; i < 256; i++)
      colors[i] = palette[i];
    Texture2D newImage = ciq.ReduceColors((Texture2D)PicOrig.texture, colors);
    PBar.Progress(1);
    Texture2D palt = (Texture2D)PicPalette.texture;
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
    PicPalette.texture = palt;
    RGEPalette.SetColorArray("_Colors", palette); // Should not be necessary but just in case
    PBar.Hide();
  }

  public Toggle PaletteModeToggle;
  public void AlterPaletteMode() {
    PicOrig.enabled = !PaletteModeToggle.isOn;
    PicPalette.enabled = PaletteModeToggle.isOn;
  }

  Color32 Transparent = new Color32(0, 0, 0, 0);
  public Material RGEPalette;

  Color32[] copied = null;
  private void Update() {
    if (minsel == -1 || maxsel == -1) return;
    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C)) {
      copied = new Color32[maxsel - minsel + 1];
      for (int i = minsel; i <= maxsel; i++) {
        copied[i - minsel] = palette[i];
      }
    }
    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V) && copied != null) {
      for (int i = 0; i < copied.Length; i++) {
        if (i + minsel > 254) break;
        palette[minsel + i] = copied[i];
        pixels[minsel + i].Set32(copied[i]);
      }
      RGEPalette.SetColorArray("_Colors", palette);
    }
  }

  public GameObject RomLineTemplate;
  public GameObject RomList;
  public Transform RomContent;
  public TMP_InputField Values;
  public Button LoadSubButton;
  public Confirm confirm;

  public void LoadTxt() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }

  public void PostLoadTxt() {
    if (!gameObject.activeSelf) return;
    StartCoroutine(LoadingTxt());
  }

  IEnumerator LoadingTxt() {
    yield return PBar.Show("Loading", 0, 2);
    string data = Values.text.Trim();

    byte[] block;
    try {
      ByteReader.ReadBlock(data, out List<CodeLabel> labels, out block);
    } catch (System.Exception e) {
      Dev.inst.HandleError("Parsing error: " + e.Message + "\n" + Values.text);
      yield break;
    }

    PBar.Progress(1);
    int pos = 0;
    byte len = block[pos++];
    for (int i = 1; i <= len; i++) {
      byte r = block[pos++];
      byte g = block[pos++];
      byte b = block[pos++];
      byte a = block[pos++];
      palette[i] = new Color32(r, g, b, a);
    }
    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
    PBar.Hide();
  }
  
  public void LoadBin() {
    FileBrowser.Load(PostLoadBin, FileBrowser.FileType.Rom);
  }

  public void PostLoadBin(string path) {
    StartCoroutine(PostLoadingBin(path));
  }
  public IEnumerator PostLoadingBin(string path) {
    yield return PBar.Show("Loading", 0, 3);
    ByteChunk res = new ByteChunk();
    try {
      ByteReader.ReadBinBlock(path, res);
    } catch (System.Exception e) {
      Dev.inst.HandleError("Parsing error: " + e.Message);
      yield break;
    }

    if (res.block.Length < 254 * 4 + 1) { Dev.inst.HandleError("Invalid data block.\nNot enough data for a palette"); yield break; }

    PBar.Progress(1);
    int pos = 0;
    byte len = res.block[pos++];
    for (int i = 1; i <= len; i++) {
      byte r = res.block[pos++];
      byte g = res.block[pos++];
      byte b = res.block[pos++];
      byte a = res.block[pos++];
      palette[i] = new Color32(r, g, b, a);
    }

    PBar.Hide();
  }

  public void SaveTxt() {
    if (minsel != -1 && maxsel != -1) {
      confirm.Set("Are you sure to save only " + (maxsel - minsel + 1) + " palette items?", SaveTextPost);
    }
    SaveTextPost();
  }
  void SaveTextPost() {
    int start = minsel == -1 ? 1 : minsel;
    int end = maxsel == -1 ? 254 : maxsel;
    string res = "Palette:\nusehex\n" + (end - start + 1).ToString("X2") + "\n";
    for (int i = start; i <= end; i++) {
      Color32 c = palette[i];
      res += c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2") + c.a.ToString("X2") + " ";
      if ((i - start) % 8 == 7) res += "\n";
    }
    res += "\n";
    Values.gameObject.SetActive(true);
    Values.text = res;
  }

  public void SaveBin() {
    if (minsel != -1 && maxsel != -1) {
      confirm.Set("Are you sure to save only " + (maxsel - minsel + 1) + " palette items?", SaveBinPost);
    }
    SaveBinPost();
  }

  void SaveBinPost() {
    FileBrowser.Save(SaveBinPost, FileBrowser.FileType.Rom);
  }

  void SaveBinPost(string path, string name) {
    ByteChunk chunk = new ByteChunk();
    int start = minsel == -1 ? 1 : minsel;
    int end = maxsel == -1 ? 254 : maxsel;
    byte[] block = new byte[1 + (end - start + 1) * 4];
    block[0] = (byte)(end - start + 1);
    int pos = 1;
    for (int i = start; i <= end; i++) {
      Color32 c = palette[i];
      block[pos++] = c.r;
      block[pos++] = c.g;
      block[pos++] = c.b;
      block[pos++] = c.a;
    }
    chunk.AddBlock("Palette", LabelType.Sprite, block);
    ByteReader.SaveBinBlock(path, name, chunk);
  }


  public void LoadRom() { }

  public void SaveRom() { }
  public void ConvertRom() { }
  public void SaveItemAsRom() { }


}

/*

Add rom list in the center
Add miniarea to see the other texture (swap between orig and palette)
When selecting a line in rom list, if it is an image load it

Load Sprite
Load Tilemap
Load part of a rom
Convert all graphical items in a rom
Save Image/Sprite
Saveload palette (bin and text)


a way to save a converted sprite
a way to convert back a sprite to normal mode


*/