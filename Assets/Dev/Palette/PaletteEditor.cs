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
  readonly Color32[] defaultPalette = new Color32[256];

  void Start() {
    int pos = 0;
    Color[] dp = new Color[256];
    foreach (Transform t in PaletteContainer) {
      pixels[pos] = t.GetComponent<Pixel>();
      palette[pos] = Col.GetColor((byte)pos);
      defaultPalette[pos] = Col.GetColor((byte)pos);
      dp[pos] = defaultPalette[pos];
      pixels[pos].Init(pos, palette[pos], SelectPalettePixel, Color.black);
      Pixels[pos] = pixels[pos];
      pos++;
    }
    RGEPalette.SetColorArray("_Colors", palette);
    DefaultPalette.SetColorArray("_Colors", dp);

    RSlider.SetValueWithoutNotify(255);
    GSlider.SetValueWithoutNotify(255);
    BSlider.SetValueWithoutNotify(255);
    ASlider.SetValueWithoutNotify(255);
    SetColor();
    AlterPaletteMode(0);
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
  public RawImage PicDefault;
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

      yield return PBar.Progress(10);
      PicOrig.texture = texture;

      Texture2D palText = new Texture2D((int)sw, (int)sh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
      palText.SetPixels32(texture.GetPixels32());
      palText.Apply();
      PicPalette.texture = palText;

      Texture2D defText = new Texture2D((int)sw, (int)sh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
      defText.SetPixels32(texture.GetPixels32());
      defText.Apply();
      PicDefault.texture = defText;

      ChangePicSizeCompleted();
      PBar.Hide();
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
    yield return PBar.Show("Generating", 0, 4); ;

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
    newImage = ciq.ReduceColors(texture, defaultPalette);
    newImage.Apply();
    PicDefault.texture = newImage;
    yield return PBar.Progress(4);
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
    yield return PBar.Show("Applying palette", 0, 2 + 2 * h);

    Color32[] colors = new Color32[256];
    for (int i = 0; i < 256; i++)
      colors[i] = palette[i];

    Texture2D newImage = ciq.ReduceColors((Texture2D)PicOrig.texture, colors);
    PBar.Progress(1);
    Texture2D palt = (Texture2D)PicPalette.texture;
    Color32[] cols = newImage.GetPixels32();
    for (int y = 0; y < h; y++) {
      if (y % 4 == 0) yield return PBar.Progress(1 + y);
      for (int x = 0; x < w; x++) {
        int pos = x + w * y;
        for (int i = 0; i < 256; i++) {
          if (ColorEqual(cols[pos], colors[i])) {
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

    newImage = ciq.ReduceColors((Texture2D)PicOrig.texture, defaultPalette);
    PBar.Progress(1 + h);
    Texture2D pald = (Texture2D)PicDefault.texture;
    cols = newImage.GetPixels32();
    for (int y = 0; y < h; y++) {
      if (y % 4 == 0) yield return PBar.Progress(2 + h + y);
      for (int x = 0; x < w; x++) {
        int pos = x + w * y;
        for (int i = 0; i < 256; i++) {
          if (ColorEqual(cols[pos], defaultPalette[i])) {
            int hi = ((i & 0xF0) >> 4) * 8 + 4;
            int lo = (i & 0xF) * 8 + 4;
            pald.SetPixel(x, y, new Color(hi / 255f, lo / 255f, 0, 255));
            break;
          }
        }
      }
    }
    pald.Apply();
    PicDefault.texture = pald;

    RGEPalette.SetColorArray("_Colors", palette); // Should not be necessary but just in case
    PBar.Hide();
  }

  public Image[] SelectedModes;

  public void AlterPaletteMode(int mode) {
    PicOrig.enabled = mode == 0;
    PicPalette.enabled = mode == 1;
    PicDefault.enabled = mode == 2;
    SelectedModes[0].enabled = mode == 0;
    SelectedModes[1].enabled = mode == 1;
    SelectedModes[2].enabled = mode == 2;
  }

  Color32 Transparent = new Color32(0, 0, 0, 0);
  public Material RGEPalette;
  public Material DefaultPalette;

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
      pixels[i].Set32(palette[i]);
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
      pixels[i].Set32(palette[i]);
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
    chunk.AddBlock("Palette", LabelType.Palette, block);
    ByteReader.SaveBinBlock(path, name, chunk);
  }

  public void SaveItemAsRom() {
    if (PicPalette.texture == null) return;
    FileBrowser.Save(SaveItemAsRomPost, FileBrowser.FileType.Rom);
  }

  void SaveItemAsRomPost(string path, string name) {
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
    chunk.AddBlock("Palette", LabelType.Palette, block);

    Texture2D pic = (Texture2D)PicPalette.texture;
    int w = pic.width;
    int h = pic.height;
    if (w < 8) w = 8;
    if (h < 8) h = 8;
    if (w > 320) w = 320;
    if (h > 256) h = 256;
    Color32[] cols = pic.GetPixels32();
    byte[] img = new byte[4 + w * h];
    img[0] = (byte)((w & 0xff00) >> 8);
    img[1] = (byte)(w & 0xff);
    img[2] = (byte)((h & 0xff00) >> 8);
    img[3] = (byte)(h & 0xff);
    pos = 4;
    for (int y = 0; y < h; y++) {
      for (int x = 0; x < w; x++) {
        Color32 c = cols[x + 2 * y];
        byte b = (byte)(((c.r - 4) / 8) << 4 + (c.g - 4) / 8);
        img[pos++] = b;
      }
    }
    chunk.AddBlock("Image", LabelType.Image, img);
    ByteReader.SaveBinBlock(path, name, chunk);
  }

  readonly List<RomLine> lines = new List<RomLine>();

  public void LoadRom() {
    FileBrowser.Load(LoadRomPost, FileBrowser.FileType.Rom);
  }
  public void LoadRomPost(string path) {
    foreach (Transform t in RomContent)
      Destroy(t.gameObject);
    lines.Clear();
    StartCoroutine(LoadingRomPost(path));
    RomList.SetActive(true);
  }

  IEnumerator LoadingRomPost(string path) {
    yield return PBar.Show("Loading", 25, 350);
    ByteChunk res = new ByteChunk();
    ByteReader.ReadBinBlock(path, res);
    yield return PBar.Progress(50);

    int num = res.labels.Count;
    int step = 0;
    int start = lines.Count;

    foreach (CodeLabel l in res.labels) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(50 + 100 * step / num);
      RomLine line = Instantiate(RomLineTemplate, RomContent).GetComponent<RomLine>();

      line.gameObject.name = l.name;
      line.gameObject.SetActive(true);
      line.Label.SetTextWithoutNotify(l.name);
      line.Type.text = (int)l.type + " " + l.type.ToString();
      line.ltype = l.type;
      lines.Add(line);
      line.Check.onValueChanged.AddListener((check) => { SelectLine(line, check); });
    }
    step = 0;
    for (int i = 0; i < res.labels.Count - 1; i++) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(150 + 100 * step / num);
      int size = res.labels[i + 1].start - res.labels[i].start;
      lines[start + i].size = size;
      lines[start + i].Size.text = size.ToString();
    }
    lines[start + res.labels.Count - 1].size = res.block.Length - res.labels[res.labels.Count - 1].start;
    lines[start + res.labels.Count - 1].Size.text = lines[start + res.labels.Count - 1].size.ToString();

    step = 0;
    for (int i = 0; i < res.labels.Count; i++) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(250 + 100 * step / num);
      int size = lines[start + i].size;
      byte[] data = new byte[size];
      for (int j = 0; j < size; j++) {
        data[j] = res.block[res.labels[i].start + j];
      }
      lines[start + i].Data = data;
    }
    PBar.Hide();
  }


  public void SaveRom() { }
  public void ConvertRom() { }

  public void SelectLine(RomLine line, bool check) {
    if (!check) return;
    int.TryParse(line.Type.text.Substring(0, 2).Trim(), out int t);
    LabelType type = (LabelType)t;

    if (type == LabelType.Image || type == LabelType.Sprite) { // Find w and h and load (update maybe the screensize)
      int w = line.Data[0];
      int h = line.Data[1];
      LoadImages(w, h, line.Data, 2);
    }
    if (type == LabelType.Tile) { // Go up until we find the tilemap label (find current position) and then get w and h and then load tile
      for (int i = 0; i < lines.Count; i++) {
        if (lines[i] == line) {
          for (int j = i - 1; j >= 0; j--) {
            int.TryParse(lines[j].Type.text.Substring(0, 2).Trim(), out int tt);
            LabelType ttt = (LabelType)tt;
            if (ttt == LabelType.Tilemap) {
              int w = lines[j].Data[2];
              int h = lines[j].Data[3];
              LoadImages(w, h, line.Data, 0);
              return;
            }
          }
        }
      }
    }
    if (type == LabelType.Palette) // Ask if we should replace current palette
      confirm.Set("Load this palette?", () => { LoadPalette(line.Data); });

  }

  void LoadPalette(byte[] data) {
    byte len = data[0];
    int pos = 1;
    for (int i = 0; i < len; i++) {
      Color32 c = Transparent;
      c.r = data[pos++];
      c.g = data[pos++];
      c.b = data[pos++];
      c.a = data[pos++];
      palette[i + 1] = c;
      pixels[i + 1].Set32(c);
    }
  }

  void LoadImages(int w, int h, byte[] data, int start) {
    // Change screen size
    PicSizeH.SetValueWithoutNotify(Mathf.Ceil(w / 8));
    PicSizeV.SetValueWithoutNotify(Mathf.Ceil(h / 4));

    // Load data
    Texture2D palo = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    Texture2D palt = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    Texture2D pald = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    Color32[] rawo = new Color32[w * h];
    Color32[] rawp = new Color32[w * h];
    for (int y = 0; y < h; y++) {
      for (int x = 0; x < w; x++) {
        int pos = x + w * y;
        byte val = data[start + x + w * (h - y - 1)];
        int hi = ((val & 0xF0) >> 4) * 8 + 4;
        int lo = (val & 0xF) * 8 + 4;
        if (SelectedModes[1].enabled)
          rawo[pos] = palette[val];
        else
          rawo[pos] = defaultPalette[val];
        rawp[pos] = new Color32((byte)hi, (byte)lo, 0, 255);
      }
    }
    palo.SetPixels32(rawo);
    palt.SetPixels32(rawp);
    pald.SetPixels32(rawp);
    palo.Apply();
    palt.Apply();
    pald.Apply();
    PicOrig.texture = palo;
    PicPalette.texture = palt;
    PicDefault.texture = pald;
  }

  bool ColorEqual(Color32 a, Color32 b) {
    if (a.r == b.r && a.g == b.g && a.b == b.b) {
      if (Math.Abs(a.a - b.a) < 16) return true;
    }
    return false;
  }
}


/*
Have src and dst palettes
Convert should load all images, recreate some sort of original rgb texture and then re-apply the color adaptation with the new palette, then update every item
Add button to update the current item in the rom
Add button to save current item as image rom + palette
*/