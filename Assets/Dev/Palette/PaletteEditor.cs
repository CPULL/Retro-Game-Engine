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
  readonly Color32[] sourcePalette = new Color32[256];
  readonly Color32[] targetPalette = new Color32[256];
  bool usingTarget = true;

  void Start() {
    int pos = 0;
    Color[] dp = new Color[256];
    TargetPaletteTG.SetIsOnWithoutNotify(true);
    SourcePaletteTG.SetIsOnWithoutNotify(false);
    foreach (Transform t in PaletteContainer) {
      pixels[pos] = t.GetComponent<Pixel>();
      sourcePalette[pos] = targetPalette[pos] = Col.GetColor((byte)pos);
      dp[pos] = sourcePalette[pos];
      pixels[pos].Init(pos, (byte)pos, SelectPalettePixel, null, Color.black, Color.red, Color.yellow);
      Pixels[pos] = pixels[pos];
      pos++;
    }
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
    DefaultPalette.SetColorArray("_Colors", dp);

    RSlider.SetValueWithoutNotify(255);
    GSlider.SetValueWithoutNotify(255);
    BSlider.SetValueWithoutNotify(255);
    ASlider.SetValueWithoutNotify(255);
    SetColor();
    AlterPaletteMode(0);
    DoneB.gameObject.SetActive(doneAsActive);
  }

  public void SetColor() { // Called if we pick a color from the palette to update the main color in the palette wheel
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
      HexColor.SetTextWithoutNotify(r.ToString("X2") + g.ToString("X2") + b.ToString("X2"));
    else
      HexColor.SetTextWithoutNotify(r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + a.ToString("X2"));
    SetSelectedPixel();
  }

  readonly Regex rghex = new Regex("[0-9a-f]{3,8}", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));
  public void SetColorHex() { // Called by the UI when the Hex color is changed in the input field
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
    Color[] pal = Col.GetPalette();
    for (int i = 1; i < 255; i++) {
      Color c = pal[i];
      bool duplicated = false;
      for (int j = 0; j < 256; j++) {
        if (i != j && pal[j] == c) {
          duplicated = true;
          break;
        }
      }
      if (duplicated) {
        pal[i] = Transparent;
      }
    }
    Col.SetPalette(pal);
    RGEPalette.SetColorArray("_Colors", pal);
  }

  void SetSelectedPixel() {
    if (selectedPixel == null) return;
    Col.SetPalette(selectedPixel.pos, SelectedColor.color);
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
  }





  public void GenerateFromImage() {
    FileBrowser.Load(GenerateFromImagePost, FileBrowser.FileType.Pics);
  }

  public void GenerateFromImagePost(string path) {
    StartCoroutine(GenerateFromImagePostCR(path));
  }
  IEnumerator GenerateFromImagePostCR(string path) {
    string url = string.Format("file://{0}", path);
    yield return PBar.Show("Loading file", 5, 10);

    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url)) {
      yield return www.SendWebRequest();
      Texture2D texture = DownloadHandlerTexture.GetContent(www);
      yield return PBar.Progress(5);

      int start = minsel == -1 ? 1 : minsel;
      int end = maxsel == -1 ? 254 : maxsel;

      Color32[] cols = ciq.CalculatePalette(texture, end - start + 1);
      Color32[] palette = new Color32[256];
      for (int i = 0; i < 255; i++)
        palette[i] = usingTarget ? targetPalette[i] : sourcePalette[i];
      for (int i = start; i <= end; i++)
        palette[i] = cols[i - start];

      if (usingTarget)
        for (int i = 0; i < 256; i++) {
          targetPalette[i] = palette[i];
          pixels[i].Set32(palette[i]);
        }
      else
        for (int i = 0; i < 256; i++) {
          sourcePalette[i] = palette[i];
          pixels[i].Set32(palette[i]);
        }

      Texture2D t = ciq.ReduceColors(texture, palette);
      t.Apply();
      MainPic.texture = t;

      PBar.Hide();
    }
  }

  public Toggle SourcePaletteTG, TargetPaletteTG;
  public void TogglePalette() {
    usingTarget = TargetPaletteTG.isOn;
    if (usingTarget) {
      for (int i = 0; i < 256; i++) {
        pixels[i].Set32(targetPalette[i]);
        Col.SetPalette(i, targetPalette[i]);
      }
    }
    else {
      for (int i = 0; i < 256; i++) {
        pixels[i].Set32(sourcePalette[i]);
        Col.SetPalette(i, sourcePalette[i]);
      }
    }
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
  }

  public void SetDefault() {
    if (usingTarget) {
      for (int i = 0; i < 256; i++) {
        targetPalette[i] = Col.GetDefaultColor((byte)i);
        pixels[i].Set32(targetPalette[i]);
        Col.SetPalette(i, targetPalette[i]);
      }
    }
    else {
      for (int i = 0; i < 256; i++) {
        sourcePalette[i] = Col.GetDefaultColor((byte)i);
        pixels[i].Set32(sourcePalette[i]);
        Col.SetPalette(i, sourcePalette[i]);
      }
    }
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
  }










  public RawImage MainPic;
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

      float sw = 8;
      if (sw < 8) sw = 8;
      if (sw > 320) sw = 320;
      float sh = 8;
      if (sh < 8) sh = 8;
      if (sh > 256) sh = 256;
      TextureScale.Point(texture, (int)sw, (int)sh);
      texture.filterMode = FilterMode.Point;
      texture.Apply();

      yield return PBar.Progress(10);
      MainPic.texture = texture;

      Texture2D palText = new Texture2D((int)sw, (int)sh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
      palText.SetPixels32(texture.GetPixels32());
      palText.Apply();
      MainPic.texture = palText;

      Texture2D defText = new Texture2D((int)sw, (int)sh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
      defText.SetPixels32(texture.GetPixels32());
      defText.Apply();
      MainPic.texture = defText;

      PBar.Hide();
    }
  }

  public void ApplyPalette() {
    if (MainPic.texture == null) {
      Dev.inst.HandleError("No image loaded!");
      return;
    }
    StartCoroutine(ApplyingPalette());
  }


  public void ShufflePalette() {
    Color32[] tosort = new Color32[254];
    for (int i = 1; i < 255; i++)
      tosort[i - 1] = Col.GetPalette()[i];
    Array.Sort(tosort, delegate (Color32 x, Color32 y) {
      int c = y.a.CompareTo(x.a);
      if (c != 0) return c;
      c = x.r.CompareTo(y.r);
      if (c != 0) return c;
      c = x.g.CompareTo(y.g);
      if (c != 0) return c;
      return x.b.CompareTo(y.b);
    });

    Col.SetPalette(0,  Color.black);
    Col.SetPalette(255, Transparent);
    for (int i = 1; i < 255; i++) {
      Col.SetPalette(i, tosort[i - 1]);
    }
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
  }

  public void UseDefaultPalette() {
    for (int i = 0; i < 256; i++) {
      Col.SetPalette(i, Col.GetColor((byte)i));
    }
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
  }

  IEnumerator ApplyingPalette() {
    int w = MainPic.texture.width;
    int h = MainPic.texture.height;
    yield return PBar.Show("Applying palette", 0, 2 + 2 * h);

    Color32[] colors = new Color32[256];
    Color[] tmp = Col.GetPalette();
    for (int i = 0; i < 256; i++)
      colors[i] = tmp[i];

    Texture2D newImage = ciq.ReduceColors((Texture2D)MainPic.texture, colors);
    PBar.Progress(1);
    Texture2D palt = (Texture2D)MainPic.texture;
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
    MainPic.texture = palt;

    // FIXME newImage = ciq.ReduceColors((Texture2D)PicOrig.texture, defaultPalette);
    PBar.Progress(1 + h);
    Texture2D pald = (Texture2D)MainPic.texture;
    cols = newImage.GetPixels32();
    for (int y = 0; y < h; y++) {
      if (y % 4 == 0) yield return PBar.Progress(2 + h + y);
      for (int x = 0; x < w; x++) {
        int pos = x + w * y;
        for (int i = 0; i < 256; i++) {
//          if (ColorEqual(cols[pos], defaultPalette[i])) {
            int hi = ((i & 0xF0) >> 4) * 8 + 4;
            int lo = (i & 0xF) * 8 + 4;
            pald.SetPixel(x, y, new Color(hi / 255f, lo / 255f, 0, 255));
            break;
          }
//        }
      }
    }
    pald.Apply();
    MainPic.texture = pald;

    RGEPalette.SetColorArray("_Colors", Col.GetPalette()); // Should not be necessary but just in case
    PBar.Hide();
  }

  public Image[] SelectedModes;

  public void AlterPaletteMode(int mode) {
    MainPic.enabled = mode == 0;
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
      Color[] tmp = Col.GetPalette();
      copied = new Color32[maxsel - minsel + 1];
      for (int i = minsel; i <= maxsel; i++) {
        copied[i - minsel] = tmp[i];
      }
    }
    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V) && copied != null) {
      for (int i = 0; i < copied.Length; i++) {
        if (i + minsel > 254) break;
        Col.SetPalette(minsel + i,  copied[i]);
      }
      RGEPalette.SetColorArray("_Colors", Col.GetPalette());
    }
  }

  public GameObject RomLineTemplate;
  public GameObject RomList;
  public Transform RomContent;
  public TMP_InputField Values;
  public Button LoadSubButton;
  public Confirm confirm;
  public Button UpdateItemButton;

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
    Col.SetPalette(block, 0, 0);
    byte len = block[0];
    Color[] tmp = Col.GetPalette();
    for (int i = 1; i <= len; i++)
      Col.SetPalette(i, tmp[i]);
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
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
    Col.SetPalette(res.block, 0, 0);
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());

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
    Color[] palette = Col.GetPalette();
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
      Color32 c = Col.GetPaletteCol(i);
      block[pos++] = c.r;
      block[pos++] = c.g;
      block[pos++] = c.b;
      block[pos++] = c.a;
    }
    chunk.AddBlock("Palette", LabelType.Palette, block);
    ByteReader.SaveBinBlock(path, name, chunk);
  }

  public void SaveItemAsRom() {
    if (MainPic.texture == null) return;
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
      Color32 c = Col.GetPaletteCol(i);
      block[pos++] = c.r;
      block[pos++] = c.g;
      block[pos++] = c.b;
      block[pos++] = c.a;
    }
    chunk.AddBlock("Palette", LabelType.Palette, block);

    Texture2D pic = (Texture2D)MainPic.texture;
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
    Color32[] pcs = new Color32[256];
    for (int i = 0; i < 256; i++)
      pcs[i] = Col.GetPaletteCol(i);
    for (int y = 0; y < h; y++) {
      for (int x = 0; x < w; x++) {
        Color32 c = cols[x + w * (h - y - 1)];
        int b = (((c.r  - 4) / 8) << 4) + (c.g - 4) / 8;
        img[pos++] = (byte)b;
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

  RomLine currentLine = null;
  public void UpdateItem() {
    int w = MainPic.texture.width;
    int h = MainPic.texture.height;
    switch (currentLine.ltype) {
      case LabelType.Image:
      case LabelType.Sprite:
        currentLine.Data = SaveImages(w, h, (Texture2D)MainPic.texture, true);
        break;

      case LabelType.Tile:
        currentLine.Data = SaveImages(w, h, (Texture2D)MainPic.texture, false);
        break;
    }

  }

  public void SaveRom() {
    if (lines == null || lines.Count == 0) return;
    FileBrowser.Save(SaveRomPost, FileBrowser.FileType.Rom);
  }

  public void SaveRomPost(string path, string name) {
    ByteChunk chunk = new ByteChunk();
    foreach (RomLine line in lines) {
      chunk.AddBlock(line.Label.text.Trim(), line.ltype, line.Data);
    }
    ByteReader.SaveBinBlock(path, name, chunk);
  }

  public void SelectLine(RomLine line, bool check) {
    UpdateItemButton.gameObject.SetActive(false);
    if (!check) return;
    LabelType type = line.ltype;

    currentLine = line;
    if (type == LabelType.Image || type == LabelType.Sprite) { // Find w and h and load (update maybe the screensize)
      int w = (line.Data[0] << 8) + line.Data[1];
      int h = (line.Data[2] << 8) + line.Data[3];
      LoadImages(w, h, line.Data, 4);

    }
    else if (type == LabelType.Tile) { // Go up until we find the tilemap label (find current position) and then get w and h and then load tile
      for (int i = 0; i < lines.Count; i++) {
        if (lines[i] == line) {
          for (int j = i - 1; j >= 0; j--) {
            LabelType t = line.ltype;
            if (t == LabelType.Tilemap) {
              int w = lines[j].Data[2];
              int h = lines[j].Data[3];
              LoadImages(w, h, line.Data, 0);
              return;
            }
          }
        }
      }
    }
    else if (type == LabelType.Palette) // Ask if we should replace current palette
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
      Col.SetPalette(i + 1, c);
    }
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
  }

  void LoadImages(int w, int h, byte[] data, int start) {
    UpdateItemButton.gameObject.SetActive(true);
    // Change screen size

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
          rawo[pos] = Col.GetPaletteCol(val);
//        else
//          rawo[pos] = defaultPalette[val];
        rawp[pos] = new Color32((byte)hi, (byte)lo, 0, 255);
      }
    }
    palo.SetPixels32(rawo);
    palt.SetPixels32(rawp);
    pald.SetPixels32(rawp);
    palo.Apply();
    palt.Apply();
    pald.Apply();
    MainPic.texture = palo;
  }

  byte[] SaveImages(int w, int h, Texture2D palt, bool saveSize) {
    byte[] data = new byte[4 + w * h];
    int start = 0;
    if (saveSize) {
      data[0] = (byte)((w & 0xff00) >> 8);
      data[1] = (byte)(w & 0xff);
      data[2] = (byte)((h & 0xff00) >> 8);
      data[3] = (byte)(h & 0xff);
      start = 4;
    }
    Color32[] rawp = palt.GetPixels32();
    for (int y = 0; y < h; y++) {
      for (int x = 0; x < w; x++) {
        int pos = x + w * y;
        byte val = (byte)((((rawp[pos].r - 4) / 8) << 4) + (rawp[pos].g - 4) / 8);
        data[start + x + w * (h - y - 1)] = val;
      }
    }
    return data;
  }

  bool ColorEqual(Color32 a, Color32 b) {
    if (a.r == b.r && a.g == b.g && a.b == b.b) {
      if (Math.Abs(a.a - b.a) < 16) return true;
    }
    return false;
  }

  public GameObject ConvertRomArea;
  public void ConvertRom() {
    if (lines == null || lines.Count == 0) return;
    ConvertRomArea.SetActive(!ConvertRomArea.activeSelf);
  }
  readonly Color32[] dstPalette = new Color32[256];
  public void SetDestinationPalette() {
    for (int i = 1; i < 255; i++) {
      dstPalette[i] = Col.GetPaletteCol(i);
    }
    dstPalette[0] = Color.black;
    dstPalette[255] = Transparent;
  }
  public void GenerateBestPaletteFromImagesAndPalettesInRom() {
    StartCoroutine(GeneratingBestPaletteFromRom());
  }
  IEnumerator GeneratingBestPaletteFromRom() {
    yield return PBar.Show("Generating best palette", 0, lines.Count + 2);
    Color32[] pal = new Color32[256];
    for (int i = 0; i < 256; i++)
      pal[i] = Col.GetColor((byte)i); // Default palette as starting one

    // Get all rom lines, if a palette is defined grab it
    Dictionary<Color32, int> final = new Dictionary<Color32, int>();
    int tsize = 1;
    int pos = 0;
    for (int l = 0; l < lines.Count; l++) {
      RomLine line = lines[l];
      yield return PBar.Progress(l + 1);
      if (line.ltype == LabelType.Palette) {
        int num = line.Data[0];
        for (int i = 0; i < num; i++)
          pal[i + 1] = new Color32(line.Data[i * 4 + 1], line.Data[i * 4 + 2], line.Data[i * 4 + 3], line.Data[i * 4 + 4]);
      }
      else if (line.ltype == LabelType.Image || line.ltype == LabelType.Sprite) {
        // For each image, get the top 256 used colors
        Dictionary<Color32, int> map = new Dictionary<Color32, int>();
        int size = ((line.Data[0] << 8) + line.Data[1]) * ((line.Data[2] << 8) + line.Data[3]);
        for (int i = 0; i < size; i++) {
          Color32 c = pal[line.Data[4 + i]];
          if (map.ContainsKey(c)) map[c] = map[c] + 1;
          else map[c] = 1;
        }
        // Transform to array and sort
        CV[] vals = new CV[map.Count];
        pos = 0;
        foreach (Color32 c in map.Keys) {
          vals[pos].c = c;
          vals[pos].v = map[c];
          pos++;
        }
        Array.Sort(vals, (a, b) => a.v.CompareTo(b.v));
        for (int i = 0; i < vals.Length && i < 256; i++) {
          if (final.ContainsKey(vals[i].c)) final[vals[i].c] = final[vals[i].c] + vals[i].v;
          else final[vals[i].c] = vals[i].v;
        }
      }
      else if (line.ltype == LabelType.Tilemap) {
        tsize = line.Data[2] * line.Data[3];
      }
      else if (line.ltype == LabelType.Tile) {
        // For each tile, get the top 256 used colors
        Dictionary<Color32, int> map = new Dictionary<Color32, int>();
        for (int i = 0; i < tsize && i < line.Data.Length; i++) {
          Color32 c = pal[line.Data[i]];
          if (map.ContainsKey(c)) map[c] = map[c] + 1;
          else map[c] = 1;
        }
        // Transform to array and sort
        CV[] vals = new CV[map.Count];
        pos = 0;
        foreach (Color32 c in map.Keys) {
          vals[pos].c = c;
          vals[pos].v = map[c];
          pos++;
        }
        Array.Sort(vals, (a, b) => a.v.CompareTo(b.v));
        for (int i = 0; i < vals.Length && i < 256; i++) {
          if (final.ContainsKey(vals[i].c)) final[vals[i].c] = final[vals[i].c] + vals[i].v;
          else final[vals[i].c] = vals[i].v;
        }
      }
    }
    // final array of colors, about 1024 items max, and create a texture with the found colors proportional to the lenght
    CV[] fvals = new CV[final.Count];
    pos = 0;
    foreach (Color32 c in final.Keys) {
      fvals[pos].c = c;
      fvals[pos].v = final[c];
      pos++;
    }
    Array.Sort(fvals, (a, b) => a.v.CompareTo(b.v));
    int numpixels = 0;
    for (int i = 0; i < fvals.Length && i < 1024; i++) {
      numpixels += fvals[i].v;
    }
    tsize = Mathf.CeilToInt(Mathf.Sqrt(numpixels) + 1);
    Texture2D t = new Texture2D(tsize, tsize, TextureFormat.RGBA32, false);
    int vpos = 0;
    pos = 0;
    while (numpixels > 0) {
      int tot = fvals[vpos].v;
      Color32 c = fvals[vpos].c;

      for (int i = 0; i < tot; i++) {
        int x = pos % tsize;
        int y = pos / tsize;
        t.SetPixel(x, y, c);
        pos++;
      }
      vpos++;
      numpixels -= tot;
      if (vpos >= fvals.Length) break;
      if (pos >= tsize * tsize) break;
    }
    t.Apply();
    yield return PBar.Progress(lines.Count + 2);

    // Call the color reduction to generate the best palette
    Color32[] colorTable = ciq.CalculatePalette(t, 254);
    // Save palette as destination (and show it)
    for (int i = 1; i < 255; i++) {
      Col.SetPalette(i, colorTable[i - 1]);
      dstPalette[i] = colorTable[i - 1];
    }
    dstPalette[0] = Color.black;
    dstPalette[255] = Transparent;

    RGEPalette.SetColorArray("_Colors", Col.GetPalette());
    PBar.Hide();
  }
  public void ConvertAllImagesInRom() {
    if (dstPalette[0].a == 0) {
      Dev.inst.HandleError("No destination palette defined!");
      return;
    }

    StartCoroutine(ConvertingAllImagesInRom());
  }
  IEnumerator ConvertingAllImagesInRom() {
    // Same logic to find best palette, but in this case we convert to true color the images (with the previous palette in the rom) and then apply the palette
    yield return PBar.Show("Converting pictures", 0, lines.Count + 2);
    Color32[] pal = new Color32[256];
    for (int i = 0; i < 256; i++)
      pal[i] = Col.GetColor((byte)i); // Default palette as starting one

    // Get all rom lines, if a palette is defined grab it
    int tw = 1;
    int th = 1;
    for (int l = 0; l < lines.Count; l++) {
      RomLine line = lines[l];
      yield return PBar.Progress(l + 1);
      if (line.ltype == LabelType.Palette) {
        int num = line.Data[0];
        for (int i = 0; i < num; i++)
          pal[i + 1] = new Color32(line.Data[i * 4 + 1], line.Data[i * 4 + 2], line.Data[i * 4 + 3], line.Data[i * 4 + 4]);
      }
      else if (line.ltype == LabelType.Image || line.ltype == LabelType.Sprite) {
        int w = (line.Data[0] << 8) + line.Data[1];
        int h = (line.Data[2] << 8) + line.Data[3];
        line.Data = ConvertImages(w, h, line.Data, 4, pal);
      }
      else if (line.ltype == LabelType.Tilemap) {
        tw = line.Data[2];
        th = line.Data[3];
      }
      else if (line.ltype == LabelType.Tile) {
        line.Data = ConvertImages(tw, th, line.Data, 0, pal);
      }
    }
    PBar.Hide();
  }

  byte[] ConvertImages(int w, int h, byte[] data, int start, Color32[] pal) {
    Texture2D palo = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    Color32[] rawo = new Color32[w * h];
    for (int y = 0; y < h; y++) {
      for (int x = 0; x < w; x++) {
        int pos = x + w * y;
        byte val = data[start + x + w * (h - y - 1)];
        rawo[pos] = pal[val];
      }
    }
    palo.SetPixels32(rawo);
    palo.Apply();
    Texture2D rest = ciq.ReduceColors(palo, dstPalette);
    byte[] res = new byte[data.Length];
    if (start > 0) {
      res[0] = data[0]; // Same size
      res[1] = data[1];
      res[2] = data[2];
      res[3] = data[3];
    }
    Color32[] cols = rest.GetPixels32();
    for (int y = 0; y < h; y++) {
      for (int x = 0; x < w; x++) {
        for (int i = 0; i < 256; i++) {
          if (ColorEqual(cols[x + w * y], dstPalette[i])) {
            res[start + x + w * (h - y - 1)] = (byte)i;
            break;
          }
        }
      }
    }
    return res;
  }

  public void UpdateInRom() {
    if (lines == null || lines.Count == 0 || currentLine == null) return;
    if (currentLine.ltype == LabelType.Palette) {
      currentLine.Data = new byte[254 * 4 + 1];
      currentLine.Data[0] = 254;
      int pos = 1;
      for (int i = 1; i < 255; i++) {
        Color32 c = Col.GetPaletteCol(i);
        currentLine.Data[pos++] = c.r;
        currentLine.Data[pos++] = c.g;
        currentLine.Data[pos++] = c.b;
        currentLine.Data[pos++] = c.a;
      }
    }
    else if (currentLine.ltype == LabelType.Image || currentLine.ltype == LabelType.Sprite) {
      int w = 8;
      if (w < 8) w = 8;
      if (w > 320) w = 320;
      int h = 4;
      if (h < 8) h = 8;
      if (h > 256) h = 256;

      byte[] img = new byte[4 + w * h];
      img[0] = (byte)((w & 0xff00) >> 8);
      img[1] = (byte)(w & 0xff);
      img[2] = (byte)((h & 0xff00) >> 8);
      img[3] = (byte)(h & 0xff);
      Color32[] cols = ((Texture2D)MainPic.texture).GetPixels32();
      for (int y = 0; y < h; y++) {
        for (int x = 0; x < w; x++) {
          for (int i = 0; i < 256; i++) {
            if (ColorEqual(cols[x + w * y], Col.GetPaletteCol(i))) {
              img[4 + x + w * (h - y - 1)] = (byte)i;
              break;
            }
          }
        }
      }
      currentLine.Data = img;
    }
    else if (currentLine.ltype == LabelType.Tile) {
      int w = 8;
      if (w < 8) w = 8;
      if (w > 320) w = 320;
      int h = 4;
      if (h < 8) h = 8;
      if (h > 256) h = 256;

      byte[] img = new byte[w * h];
      Color32[] cols = ((Texture2D)MainPic.texture).GetPixels32();
      for (int y = 0; y < h; y++) {
        for (int x = 0; x < w; x++) {
          for (int i = 0; i < 256; i++) {
            if (ColorEqual(cols[x + w * y], Col.GetPaletteCol(i))) {
              img[x + w * (h - y - 1)] = (byte)i;
              break;
            }
          }
        }
      }
      currentLine.Data = img;
    }
  }

  public Button DoneB;
  bool doneAsActive = false;
  public void EditPalette() {
    // Load the palette from Col (do we need it?)


    // Show the Done button
    DoneB.gameObject.SetActive(true);
    doneAsActive = true;
  }

  public void Done() {
    DoneB.gameObject.SetActive(false);
    Dev.inst.SpriteEditor();
    doneAsActive = false;
  }

  struct CV {
    public Color32 c;
    public int v;
  }
}



/*
!Import image and generate palette (use subset if selected +button to select all/none) -> show resulting image
!Source palette (+ default palette button +show)
!Target Palette (+ default palette button +show)


Import image and palette from rom (sprite, image, tilemap), convert to target palette, re-save or export


save palette as text and rom, export converted roms
edit manually the palette, including ctrl+c/ctrl+v
 
load palette from file, txt (or immediate) using #rrggbb and #rrggbbaa values

 */