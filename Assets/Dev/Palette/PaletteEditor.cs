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

  void Start() {
    int pos = 0;
    foreach(Transform t in PaletteContainer) {
      pixels[pos] = t.GetComponent<Pixel>();
      pixels[pos].Init(pos, Col.GetColor((byte)pos), SelectPalettePixel, Color.black);
      string id = "_Color" + pos.ToString("X2");
      RGEPalette.SetColor(id, pixels[pos].Get32());
      pos++;
    }

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
    string id = "_Color" + selectedPixel.pos.ToString("X2");
    RGEPalette.SetColor(id, pixels[selectedPixel.pos].Get32());
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
    int tw = texture.width;
    int th = texture.height;
    Color[] tcs = texture.GetPixels();
    Dictionary<int, Rhsv> hash = new Dictionary<int, Rhsv>();
    yield return PBar.Show("Generating", 0, th + 100); ;
    int pos = 0;


    ColorImageQuantizer ciq = new ColorImageQuantizer(new MedianCutQuantizer());
    Color32[] colorTable = ciq.CalculatePalette(texture, 254);


    Color32[] colors = new Color32[256];
    colors[0] = Black;
    colors[255] = Transparent;
    for (int i = 1; i < 255; i++) {
      colors[i] = colorTable[i - 1];
      pixels[i].Set32(colors[i]);
    }
    ShufflePalette();

    Texture2D newImage = ciq.ReduceColors(texture, colors);
    newImage.Apply();
    MainPicPalette.texture = newImage;

    PBar.Hide();

    yield break;


    // Reconstruct the texture by picking the best color from the final values
    Color32[] res = new Color32[tcs.Length];
    for (int y = 0; y < th; y++) {
      yield return PBar.Progress(th + y);
      for (int x = 0; x < tw; x++) {
        pos = x + tw * y;

        if (tcs[pos].a < .9f) res[pos] = Transparent;
        else {
          int best = 0;
          float dist = float.MaxValue;
          for (int i = 0; i < 256; i++) {
            float d =
              (tcs[pos].r - colors[i].r) * (tcs[pos].r - colors[i].r) +
              (tcs[pos].g - colors[i].g) * (tcs[pos].g - colors[i].g) +
              (tcs[pos].b - colors[i].b) * (tcs[pos].b - colors[i].b);
            if (d < dist) {
              dist = d;
              best = i;
            }
          }
          res[pos] = colors[best];
        }
      }
    }

    /*
    for (int y = 0; y < th; y++) {
      yield return PBar.Progress(y);
      for (int x = 0; x < tw; x++) {
        pos = x + tw * y;
        Color.RGBToHSV(tcs[pos], out float h, out float s, out float v);
        if (tcs[pos].a < .9f) continue; // Pixels with alpha will be excluded
        Rhsv val = new Rhsv(h, s, v);
        int id = val.ID();
        if (hash.ContainsKey(id)) hash[id].num++;
        else hash[id] = val;
      }
    }
    Dictionary<int, Rhsv> final = null;
    int tries = 5;
    while (tries > 0) {
      tries--;

      if (hash.Count < 254) {
        final = hash;
        break;
      }

      // Split in 3 ones, by the different hey
      Dictionary<int, Rhsv> hashH = new Dictionary<int, Rhsv>();
      Dictionary<int, Rhsv> hashS = new Dictionary<int, Rhsv>();
      Dictionary<int, Rhsv> hashV = new Dictionary<int, Rhsv>();
      foreach (int key in hash.Keys) {
        Rhsv val = hash[key];

        int id = val.IDH();
        if (hashH.ContainsKey(id)) {
          Rhsv n = hashH[id].JoinH(val);
          hashH.Remove(id);
          hashH[n.IDH()] = n;
        }
        else hashH[id] = val;

        id = val.IDS();
        if (hashS.ContainsKey(id)) {
          Rhsv n = hashS[id].JoinSV(val);
          hashS.Remove(id);
          hashS[n.IDS()] = n;
        }
        else hashS[id] = val;

        id = val.IDV();
        if (hashV.ContainsKey(id)) {
          Rhsv n = hashV[id].JoinSV(val);
          hashV.Remove(id);
          hashV[n.IDV()] = n;
        }
        else hashV[id] = val;
      }

      if (hashH.Count < 254) { // Use HUE
        final = hashV;
        Debug.Log("Hue wins");
        break;
      }
      if (hashS.Count < 254) { // Use SAT
        final = hashS;
        Debug.Log("Sat wins");
        break;
      }
      if (hashV.Count < 254) { // Use VAL
        final = hashV;
        Debug.Log("Val wins");
        break;
      }

      // Re-construct the hashes
      hash.Clear();
      foreach(Rhsv val in hashH.Values) {
        int id = val.ID();
        if (hash.ContainsKey(id)) hash[id] = hash[id].JoinH(val);
        else hash[id] = new Rhsv(val.h, val.s, val.v) { num = val.num };
      }
      foreach(Rhsv val in hashS.Values) {
        int id = val.ID();
        if (hash.ContainsKey(id)) hash[id] = hash[id].JoinH(val);
        else hash[id] = new Rhsv(val.h, val.s, val.v) { num = val.num };
      }
      foreach (Rhsv val in hashV.Values) {
        int id = val.ID();
        if (hash.ContainsKey(id)) hash[id] = hash[id].JoinH(val);
        else hash[id] = new Rhsv(val.h, val.s, val.v) { num = val.num };
      }

      // Reduce the amount of colors by reducing the hash mode
      Rhsv[] vals = new Rhsv[hash.Count];
      pos = 0;
      // Check for each value we have the sum of all distances from all other values.
      foreach (Rhsv val in hash.Values) {
        vals[pos++] = val;
        val.dist = 0;
        foreach (Rhsv val2 in hash.Values) {
          val.Dist(val2, hhh, sss, vvv);
        }
      }

      // Then sort by distance (higher first) and get only the first 90%
      Array.Sort(vals, (x, y) => y.dist.CompareTo(x.dist));
      hash.Clear();
      int len = (int)(vals.Length * .95f);
      for (int i = 0; i < len; i++) {
        Rhsv val = vals[i];
        hash[val.ID()] = val;
      }
      final = hash;
    }



    // We have now the final array to be used, sort it by HSV and luma
    Rhsv[] colorsToSort = new Rhsv[254];
    pos = 0;
    foreach (Rhsv val in final.Values) {
      colorsToSort[pos++] = val;
      if (pos == 254) break;
    }
    for (int i = pos; i < 254; i++) {
      colorsToSort[i] = new Rhsv(0, 0, 1);
    }
    Array.Sort(colorsToSort, (x, y) => (int)((x.h - y.h) + (x.v - y.v) * 2) );

    */
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

    for (int i = 1; i < 255; i++) {
      pixels[i].Set32(cs[i - 1].C32());
      string id = "_Color" + i.ToString("X2");
      RGEPalette.SetColor(id, cs[i - 1].C32());
    }
  }


  IEnumerator ApplyingPalette() {
    int w = MainPicOrig.texture.width;
    int h = MainPicOrig.texture.height;
    yield return PBar.Show("Applying palette", 0, h);

    Texture2D origt = (Texture2D)MainPicOrig.texture;
    Texture2D palt = (Texture2D)MainPicPalette.texture;

    // For each texture pixel, find the bast pixel in the palette
    Color32[] cols = origt.GetPixels32();
    for (int y = 0; y < h; y++) {
      PBar.Progress(y);
      for (int x = 0; x < w; x++) {
        int pos = x + w * y;
        Color32 col = cols[pos];
        Color.RGBToHSV(col, out _, out float sat, out float val);

        int best = -1;
        float bestdist = float.MaxValue;
        for (int i = 0; i < 256; i++) {
          Color32 pc = pixels[i].Get32();
          Color.RGBToHSV(pc, out _, out float psat, out float pval);

          float dr = (col.r - pc.r);
          float dg = (col.g - pc.g);
          float db = (col.b - pc.b);
          float da = (col.a - pc.a);
          float ds = (sat - psat);
          float dv = (val - pval);
          float dist = (dr * dr + dg * dg + db * db) / 3 + da * da + ds * ds + dv * dv * dv;
          if (bestdist > dist) {
            bestdist = dist;
            best = i;
          }
        }
        int hi = (best & 0xF0) >> 4;
        int lo = (best & 0xF);
        palt.SetPixel(x, y, new Color(hi / 15f, lo / 15f, 0, 255));
      }
    }
    // Update the "paletized" texture with the found color index
    palt.Apply();
    MainPicPalette.texture = palt;

    // Show the paletized rawimage and toggle the flag
    // FIXME

    PBar.Hide();
  }

  Color32 Transparent = new Color32(0, 0, 0, 0);
  Color32 Black = new Color32(0, 0, 0, 255);
  public Material RGEPalette;

  public float hhh = 1;
  public float sss = 1;
  public float vvv = 1;


}

/*


slider to decide how many colors to use
toggle to enable/disable palette
Add second texture for image to use a palette material

Load Sprite
Load Tilemap
Save Image/Sprite
Generate palette from image
Update image with current palette
Toggle Normal/Palette modes


a way to load a sprite and see it with a palette
a way to get a sprite from normal and convert to palette (generating palette or using current palette)
a way to save a converted sprite
a way to convert back a sprite to normal mode

load/save buttons (bin and text)


*/