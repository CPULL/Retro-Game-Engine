using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageEditor : MonoBehaviour {
  public Slider WidthSlider;
  public Slider HeightSlider;
  public TextMeshProUGUI WidthTxt;
  public TextMeshProUGUI HeightTxt;
  public RawImage Picture;
  public TMP_InputField Values;
  public Button LoadSubButton;

  public void OnSliderChange() {
    WidthTxt.text = "Width: " + (int)(WidthSlider.value * 8);
    HeightTxt.text = "Heigth: " + (int)(HeightSlider.value * 8);
  }

  public void ImportImagePreC() {
    FileBrowser.Load(ImportImagePostC, FileBrowser.FileType.Pics);
  }
  public void ImportImagePostC(string path) {
    StartCoroutine(LoadImageCoroutine(path, true));
  }
  public void ImportImagePreK() {
    FileBrowser.Load(ImportImagePostK, FileBrowser.FileType.Pics);
  }
  public void ImportImagePostK(string path) {
    StartCoroutine(LoadImageCoroutine(path, false));
  }

  IEnumerator LoadImageCoroutine(string path, bool scale) {
    string url = string.Format("file://{0}", path);

    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url)) {
      yield return www.SendWebRequest();
      yield return PBar.Show("Loading file", 1, 20);
      Texture2D texture = DownloadHandlerTexture.GetContent(www);
      // Get the top-left part of the image fitting in the sprite size
      yield return PBar.Progress(3);
      if (scale)
        TextureScale.Point(texture, (int)WidthSlider.value * 8, (int)HeightSlider.value * 8);
      else {
        int w = texture.width >> 3;
        if (w > 128) w = 128;
        int h = texture.height >> 3;
        if (h > 128) h = 128;
        TextureScale.Point(texture, w * 8, h * 8);
        WidthSlider.SetValueWithoutNotify(w);
        HeightSlider.SetValueWithoutNotify(h);
        WidthTxt.text = "Width: " + (int)(WidthSlider.value * 8);
        HeightTxt.text = "Heigth: " + (int)(HeightSlider.value * 8);
      }
      yield return PBar.Progress(6);

      // Normalize the color
      Color32[] tps = texture.GetPixels32();
      yield return PBar.Progress(9);
      float prgs = 44f / texture.height;
      float prg = 9;
      for (int y = 0; y < texture.height; y++) {
        yield return PBar.Progress((int)prg);
        if ((y & 3) == 0) prg += prgs;
        int ty = texture.height - y - 1;
        for (int x = 0; x < texture.width; x++) {
          int pos = x + texture.width * ty;
          tps[pos] = Col.GetColor(Col.GetBestColor(tps[pos]));
        }
      }

      texture.SetPixels32(tps);
      texture.Apply();
      texture.filterMode = FilterMode.Point;
      Picture.texture = texture;
    }
    PBar.Hide();
  }

  public void SaveTxt() {
    StartCoroutine(SavingTxt());
  }
  IEnumerator SavingTxt() {
    int w = (int)WidthSlider.value * 8;
    int h = (int)HeightSlider.value * 8;
    yield return PBar.Show("Saving", 0, 1 + w * h);
    string res = "Image:\nusehex\n";
    byte sizexh = (byte)((w & 0xff00) >> 8);
    byte sizexl = (byte)(w & 0xff);
    byte sizeyh = (byte)((h & 0xff00) >> 8);
    byte sizeyl = (byte)(h & 0xff);

    res += sizexh.ToString("X2") + " " + sizexl.ToString("X2") + " " + sizeyh.ToString("X2") + " " + sizeyl.ToString("X2") + "\n";
    int num = w * h;
    Texture2D texture = (Texture2D)Picture.texture;
    Color32[] tps = texture.GetPixels32();
    yield return PBar.Progress(1);

    int prg = 1;
    for (int y = 0; y < texture.height; y++) {
      yield return PBar.Progress(prg);
      if ((y & 3) == 0) prg += 4;
      int ty = texture.height - y - 1;
      for (int x = 0; x < texture.width; x+=4) {
        int pos = x + texture.width * ty;
        res += Col.GetBestColor(tps[pos + 0]).ToString("X2");
        res += Col.GetBestColor(tps[pos + 1]).ToString("X2");
        res += Col.GetBestColor(tps[pos + 2]).ToString("X2");
        res += Col.GetBestColor(tps[pos + 3]).ToString("X2");
        res += " ";
      }
    }
    res += "\n";
    Values.gameObject.SetActive(true);
    Values.text = res;
    PBar.Hide();
  }

  public void LoadTxtPre() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }

  public void LoadTxtPost() {
    if (!gameObject.activeSelf) return;
    StartCoroutine(LoadingTxt());
  }

  IEnumerator LoadingTxt() {
    yield return PBar.Show("Loading", 0, 256);
    string data = Values.text.Trim();

    byte[] block;
    try {
      ByteReader.ReadBlock(data, out _, out block);
    } catch (System.Exception e) {
      Dev.inst.HandleError("Parsing error: " + e.Message + "\n" + Values.text);
      yield break;
    }

    PBar.Progress(128);
    int wb = (block[0] << 8) + block[1];
    int hb = (block[2] << 8) + block[3];
    if (wb < 8 || hb < 8 || wb > 512 || hb > 512) {
      Dev.inst.HandleError("This does not look like an image.");
      PBar.Hide();
      yield break;
    }
    WidthSlider.SetValueWithoutNotify(wb / 8);
    HeightSlider.SetValueWithoutNotify(hb / 8);
    WidthTxt.text = "Width: " + (int)(WidthSlider.value * 8);
    HeightTxt.text = "Heigth: " + (int)(HeightSlider.value * 8);

    yield return PBar.Show("Loading", 50, 50 + (int)HeightSlider.value);

    if (block.Length < 4 + wb * hb) { Dev.inst.HandleError("Invalid data block.\nNot enough data for an image"); PBar.Hide(); yield break; }

    Texture2D texture = new Texture2D(wb, hb, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    Color32[] tps = texture.GetPixels32();
    for (int i = 0; i < wb * hb; i++) {
      if (i % (4 * wb) == 0) yield return PBar.Progress(50 + i / wb);
      tps[i] = Col.GetColor(block[4 + i]);
    }
    texture.SetPixels32(tps);
    texture.Apply();
    Picture.texture = texture;

    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
    PBar.Hide();
  }



  public void SaveBin() {
    // Show FileBrowser in select file mode
    FileBrowser.Save(SaveBinPost, FileBrowser.FileType.Rom);
  }
  public void SaveBinPost(string path, string name) {
    StartCoroutine(SavingBinPost(path, name));
  }
  public IEnumerator SavingBinPost(string path, string name) {
    int w = (int)WidthSlider.value * 8;
    int h = (int)HeightSlider.value * 8;
    yield return PBar.Show("Saving", 0, 1 + w * h);
    ByteChunk chunk = new ByteChunk();

    byte[] block = new byte[4 + w * h];
    block[0] = (byte)((w & 0xff00) >> 8);
    block[1] = (byte)(w & 0xff);
    block[2] = (byte)((h & 0xff00) >> 8);
    block[3] = (byte)(h & 0xff);
    Color32[] tps = ((Texture2D)Picture.texture).GetPixels32();
    yield return PBar.Progress(1);

    for (int y = h - 1; y >= 0; y--) {
      for (int x = 0; x < w; x++) {
        if (x == 0) yield return PBar.Progress(2 + x * y);
        block[4 + x + w * y] = Col.GetBestColor(tps[x + w * (h - 1 - y)]);
      }
    }
    chunk.AddBlock("Image", LabelType.Image, block);

    ByteReader.SaveBinBlock(path, name, chunk);
    PBar.Hide();
  }

  public void LoadBin() {
    FileBrowser.Load(PostLoadBin, FileBrowser.FileType.Rom);
  }

  public void PostLoadBin(string path) {
    StartCoroutine(PostLoadingBin(path));
  }
  public IEnumerator PostLoadingBin(string path) {
    yield return PBar.Show("Loading", 0, 256);
    ByteChunk res = new ByteChunk();
    try {
      ByteReader.ReadBinBlock(path, res);
    } catch (System.Exception e) {
      Dev.inst.HandleError("Parsing error: " + e.Message);
      yield break;
    }

    if (res.block.Length <= 2) { Dev.inst.HandleError("Invalid data block.\nNot enough data for a sprite"); PBar.Hide(); yield break; }

    PBar.Progress(128);
    int wb = (res.block[0] << 8) + res.block[1];
    int hb = (res.block[2] << 8) + res.block[3];
    if (wb < 8 || hb < 8 || wb > 512 || hb > 512) {
      Dev.inst.HandleError("This does not look like an image.");
      PBar.Hide();
      yield break;
    }


    WidthSlider.SetValueWithoutNotify(wb / 8);
    HeightSlider.SetValueWithoutNotify(hb / 8);
    WidthTxt.text = "Width: " + (int)(WidthSlider.value * 8);
    HeightTxt.text = "Heigth: " + (int)(HeightSlider.value * 8);

    yield return PBar.Show("Loading", 50, 50 + (int)HeightSlider.value);

    if (res.block.Length < 4 + wb * hb) { Dev.inst.HandleError("Invalid data block.\nNot enough data for an image"); PBar.Hide(); yield break; }

    Texture2D texture = new Texture2D(wb, hb, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    Color32[] tps = texture.GetPixels32();
    for (int y = hb - 1; y >= 0; y--) {
      for (int x = 0; x < wb; x++) {
        if (x == 0) yield return PBar.Progress(50 + x * y);
        tps[x + wb * (hb - 1 - y)] = Col.GetColor(res.block[4 + x + wb * y]);
      }
    }
    texture.SetPixels32(tps);
    texture.Apply();
    Picture.texture = texture;
    PBar.Hide();
  }






  /*
  Done (from rom editor)
   
   */
}
