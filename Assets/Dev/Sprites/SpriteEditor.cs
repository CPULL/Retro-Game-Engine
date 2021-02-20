using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SpriteEditor : MonoBehaviour {
  readonly Color32[] paletteCols = new Color32[256];
  public Material RGEPalette;

  private void Start() {
    if (pixels == null) {
      WidthSlider.SetValueWithoutNotify(1);
      HeightSlider.SetValueWithoutNotify(1);
      pixels = new Pixel[0];
      selected = new bool[0];
      ChangeSpriteSize();
      SetUndo(false);
    }

    int pos = 0;
    Color[] cols = new Color[256];
    palPixels = new Pixel[256];
    foreach (Transform t in PaletteContainer) {
      palPixels[pos] = t.GetComponent<Pixel>();
      paletteCols[pos] = Col.GetColor((byte)pos);
      cols[pos] = paletteCols[pos];
      palPixels[pos].Init(pos, (byte)pos, SelectPalettePixel, null, Color.black, Color.red, Color.yellow);
      pos++;
    }
    RGEPalette.SetColorArray("_Colors", cols);
  }

  public GameObject PixelPrefab;
  public GridLayoutGroup SpriteGrid;
  Pixel[] pixels = null;
  Pixel[] palPixels = null;
  public Slider WidthSlider;
  public Slider HeightSlider;
  public TextMeshProUGUI WidthSliderText;
  public TextMeshProUGUI HeightSliderText;
  public TMP_InputField Values;
  public Button LoadSubButton;
  public Palette palette;
  bool[] selected = null;

  public Image CurrentColorImg;
  byte CurrentColor = 255;
  ActionVal action = ActionVal.No;
  Vector2Int start = Vector2Int.zero;
  int w, h;
  public Sprite[] boxes;
  public TextMeshProUGUI Message;
  readonly List<byte[]> undo = new List<byte[]>();
  byte lastPixelColor;
  readonly int[] sizes = new int[] { 8, 16, 24, 32, 40, 48, 56, 64 };

  public void SetCurrentColor(int col) {
    if (col < 0) col = 0;
    if (col > 255) col = 255;
    CurrentColor = (byte)col;
  }

  public void ChangeSpriteSizeText() {
    WidthSliderText.text = "Width: " + sizes[(int)WidthSlider.value];
    HeightSliderText.text = "Height: " + sizes[(int)HeightSlider.value];
  }

  public void ChangeSpriteSize() {
    int oldw = w;
    int oldh = h;
    w = sizes[(int)WidthSlider.value];
    h = sizes[(int)HeightSlider.value];

    WidthSliderText.text = "Width: " + w;
    HeightSliderText.text = "Height: " + h;
    Rect rt = SpriteGrid.transform.GetComponent<RectTransform>().rect;
    SpriteGrid.cellSize = new Vector2(rt.width / w, rt.height / h);

    Pixel[] oldps = pixels;
    int num = w * h;
    foreach(Transform t in SpriteGrid.transform)
      t.SetParent(null);

    pixels = new Pixel[num];
    selected = new bool[num];
    Sprite box = w <= 16 && h <= 16 ? boxes[2] : boxes[1];
    if (w >= 40 || h >= 40) box = boxes[0];

    for (int i = 0; i < num; i++) {
      Pixel pixel = Instantiate(PixelPrefab, SpriteGrid.transform).GetComponent<Pixel>();
      pixel.Init(i, 255, ClickPixel, OverPixel, Color.black, Color.red, Color.yellow);
      pixel.SetBorderSprite(box);
      pixels[i] = pixel;
    }

    for (int x = 0; x < w; x++) {
      for (int y = 0; y < h; y++) {
        if (x < oldw && y < oldh && oldps[x + oldw * y] != null)
          pixels[x + w * y].Set(oldps[x + oldw * y].Get());
      }
    }

    foreach (Pixel p in oldps)
      if (p != null)
        Destroy(p.gameObject);

    undo.Clear();
  }

  private void ClickPixel(Pixel p, bool left) {
    int x = p.pos % w;
    int y = (p.pos - x) / w;

    if (action == ActionVal.Pick) {
      CurrentColor = p.Get();
      palette.AlterColor(CurrentColor);
      return;
    }

    if (action == ActionVal.SelectAll) {
      CurrentColor = p.Get();
      palette.AlterColor(CurrentColor);

      // Check all pixels that had the exact same color and keep all of them selected
      for (int i = 0; i < pixels.Length; i++) {
        Pixel px = pixels[i];
        selected[i] = px.Get() == CurrentColor;
        if (selected[i]) px.Select();
        else px.Deselect();
      }
      return;
    }

    if (action == ActionVal.LineStart) {
      start.x = x;
      start.y = y;
      p.Select();
      action = ActionVal.LineEnd;
      return;
    }
    if (action == ActionVal.LineEnd) {
      int x1 = start.x;
      int x2 = x;
      int y1 = start.y;
      int y2 = y;
      DrawLine(x1, y1, x2, y2, false);
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].Deselect();
      return;
    }

    if (action == ActionVal.BoxStart) {
      start.x = x;
      start.y = y;
      p.Select();
      action = ActionVal.BoxEnd;
      return;
    }
    if (action == ActionVal.BoxEnd) {
      int x1 = start.x;
      int x2 = x;
      int y1 = start.y;
      int y2 = y;
      DrawBox(x1, y1, x2, y2, false, false);
      for (int i = 0; i < pixels.Length; i++)
        p.Deselect();
      return;
    }

    if (action == ActionVal.BoxStartF) {
      start.x = x;
      start.y = y;
      p.Select();
      action = ActionVal.BoxEndF;
      return;
    }
    if (action == ActionVal.BoxEndF) {
      int x1 = start.x;
      int x2 = x;
      int y1 = start.y;
      int y2 = y;
      DrawBox(x1, y1, x2, y2, false, true);
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].Deselect();
      return;
    }

    if (action == ActionVal.EllipseStart) {
      start.x = x;
      start.y = y;
      p.Select();
      action = ActionVal.EllipseEnd;
      return;
    }
    if (action == ActionVal.EllipseEnd) {
      int x1 = start.x;
      int x2 = x;
      int y1 = start.y;
      int y2 = y;
      DrawEllipse(x1, y1, x2, y2, false, false);
      for (int i = 0; i < pixels.Length; i++)
        p.Deselect();
      return;
    }

    if (action == ActionVal.EllipseStartF) {
      start.x = x;
      start.y = y;
      p.Select();
      action = ActionVal.EllipseEndF;
      return;
    }
    if (action == ActionVal.EllipseEndF) {
      int x1 = start.x;
      int x2 = x;
      int y1 = start.y;
      int y2 = y;
      DrawEllipse(x1, y1, x2, y2, false, true);
      for (int i = 0; i < pixels.Length; i++)
        p.Deselect();
      return;
    }

    if (action == ActionVal.Fill) {
      Fill(x, y, CurrentColor);
      return;
    }

    if (p.Get() == CurrentColor)
      p.Set(255);
    else
      p.Set(CurrentColor);
    SetUndo(true);
    lastPixelColor = CurrentColor;
  }

  private void OverPixel(Pixel p) {
    if (action == ActionVal.LineEnd) {
      int x1 = start.x;
      int x2 = p.pos % w;
      int y1 = start.y;
      int y2 = (p.pos - x2) / w;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].Deselect();
      DrawLine(x1, y1, x2, y2, true);
    }

    if (action == ActionVal.BoxEnd || action == ActionVal.BoxEndF) {
      int x1 = start.x;
      int x2 = p.pos % w;
      int y1 = start.y;
      int y2 = (p.pos - x2) / w;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].Deselect();
      DrawBox(x1, y1, x2, y2, true, action == ActionVal.BoxEndF);
    }

    if (action == ActionVal.EllipseEnd || action == ActionVal.EllipseEndF) {
      int x1 = start.x;
      int x2 = p.pos % w;
      int y1 = start.y;
      int y2 = (p.pos - x2) / w;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].Deselect();
      DrawEllipse(x1, y1, x2, y2, true, action == ActionVal.EllipseEndF);
    }

    if (action == ActionVal.FreeDraw && Input.GetMouseButton(0)) {
      p.Set(CurrentColor);
    }
  }

  public Button[] Buttons;
  public Sprite UISpriteSel;
  public Sprite UISpriteNot;

  void SetButtons(int num) {
    foreach (Button b in Buttons)
      b.GetComponent<Image>().sprite = UISpriteNot;
    if (num != 0 && num != 5 && num != 9)
      for (int i = 0; i < w * h; i++)
        pixels[i].Deselect();
    if (num != -1)
      Buttons[num].GetComponent<Image>().sprite = UISpriteSel;
  }

  public void Clear() {
    Confirm.Set("Reinit sprite?", DestroySpriteConfirmed);
  }

  public void DestroySpriteConfirmed() {
    SetUndo(false);
    int num = w * h;
    for (int i = 0; i < num; i++)
      pixels[i].Set(255);
  }



  public void FreeDraw() {
    if (action == ActionVal.FreeDraw) {
      action = ActionVal.No;
      SetButtons(-1);
    }
    else {
      action = ActionVal.FreeDraw;
      SetButtons(1);
    }
  }

  public void Line() {
    if (action == ActionVal.LineStart) {
      action = ActionVal.No;
      SetButtons(-1);
    }
    else {
      action = ActionVal.LineStart;
      SetButtons(6);
    }
  }

  public void Box(bool filled) {
    if (filled) {
      if (action == ActionVal.BoxStartF) {
        action = ActionVal.No;
        SetButtons(-1);
      }
      else {
        action = ActionVal.BoxStartF;
        SetButtons(7);
      }
    }
    else {
      if (action == ActionVal.BoxStart) {
        action = ActionVal.No;
        SetButtons(-1);
      }
      else {
        action = ActionVal.BoxStart;
        SetButtons(2);
      }
    }
  }

  public void Ellipse(bool filled) {
    if (filled) {
      if (action == ActionVal.EllipseStartF) {
        action = ActionVal.No;
        SetButtons(-1);
      }
      else {
        action = ActionVal.EllipseStartF;
        SetButtons(8);
      }
    }
    else {
      if (action == ActionVal.EllipseStart) {
        action = ActionVal.No;
        SetButtons(-1);
      }
      else {
        action = ActionVal.EllipseStart;
        SetButtons(3);
      }
    }
  }

  void DrawLine(int x1, int y1, int x2, int y2, bool border) {
    if (!border) {
      SetUndo(false);
      action = ActionVal.LineStart;
    }
    int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
    dx = x2 - x1; dy = y2 - y1;
    if (dx == 0) { // Vertical
      if (y2 < y1) { int tmp = y1; y1 = y2; y2 = tmp; }
      for (y = y1; y <= y2; y++) DrawPixel(x1, y, border);
      return;
    }

    if (dy == 0) { // Horizontal
      if (x2 < x1) { int tmp = x1; x1 = x2; x2 = tmp; }
      for (x = x1; x <= x2; x++) DrawPixel(x, y1, border);
      return;
    }

    // Diagonal
    dx1 = dx;
    if (dx1 < 0) dx1 = -dx1;
    dy1 = dy;
    if (dy1 < 0) dy1 = -dy1;
    px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
    if (dy1 <= dx1) {
      if (dx >= 0) {
        x = x1; y = y1; xe = x2;
      }
      else {
        x = x2; y = y2; xe = x1;
      }
      DrawPixel(x, y, border);
      for (i = 0; x < xe; i++) {
        x += 1;
        if (px < 0)
          px += 2 * dy1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y += 1; else y -= 1;
          px += 2 * (dy1 - dx1);
        }
        DrawPixel(x, y, border);
      }
    }
    else {
      if (dy >= 0) {
        x = x1; y = y1; ye = y2;
      }
      else {
        x = x2; y = y2; ye = y1;
      }
      DrawPixel(x, y, border);
      for (i = 0; y < ye; i++) {
        y += 1;
        if (py <= 0)
          py += 2 * dx1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x += 1; else x -= 1;
          py += 2 * (dx1 - dy1);
        }
        DrawPixel(x, y, border);
      }
    }
  }

  void DrawBox(int x1, int y1, int x2, int y2, bool border, bool filled) {
    if (!border) {
      SetUndo(false);
      if (filled)
        action = ActionVal.BoxStartF;
      else
        action = ActionVal.BoxStart;
    }
    int sx = x1; if (sx > x2) sx = x2;
    int sy = y1; if (sy > y2) sy = y2;
    int ex = x1; if (ex < x2) ex = x2;
    int ey = y1; if (ey < y2) ey = y2;

    if (filled) {
      for (int y = sy; y <= ey; y++)
        for (int x = sx; x <= ex; x++)
          DrawPixel(x, y, border);
    }
    else {
      for (int x = sx; x <= ex; x++) {
        DrawPixel(x, y1, border);
        DrawPixel(x, y2, border);
      }
      for (int y = sy; y <= ey; y++) {
        DrawPixel(x1, y, border);
        DrawPixel(x2, y, border);
      }
    }
  }

  void DrawEllipse(int x1, int y1, int x2, int y2, bool border, bool filled) {
    if (!border) {
      SetUndo(false);
      if (filled)
        action = ActionVal.EllipseStartF;
      else
        action = ActionVal.EllipseStart;
    }

    if (x1 > x2) { int tmp = x1; x1 = x2; x2 = tmp; }
    if (y1 > y2) { int tmp = y1; y1 = y2; y2 = tmp; }
    float cx = (x1 + x2) / 2f;
    float cy = (y1 + y2) / 2f;
    float rx = cx - x1;
    float ry = cy - y1;
    if (rx < .5f) rx = .5f;
    if (ry < .5f) ry = .5f;
    float a2 = rx * rx;
    float b2 = ry * ry;

    for (float x = 0; x <= rx; ) {
      float y = Mathf.Sqrt((a2 - x*x) * b2 / a2);
      if (filled) {
        for (int px = Mathf.RoundToInt(cx - x); px <= Mathf.RoundToInt(cx + x); px++) {
          for (int py = Mathf.RoundToInt(cy - y); py <= Mathf.RoundToInt(cy + y); py++) {
            DrawPixel(px, py, border);
          }
        }
      }
      else {
        DrawPixel(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy + y), border);
        DrawPixel(Mathf.RoundToInt(cx - x), Mathf.RoundToInt(cy + y), border);
        DrawPixel(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy - y), border);
        DrawPixel(Mathf.RoundToInt(cx - x), Mathf.RoundToInt(cy - y), border);
      }
      // Calculate the error, new y should be the same or 1 pixel off
      float oldx = x, oldy = y, incr = .01f;
      while (Mathf.Abs(x - oldx) < 1f && Mathf.Abs(y - oldy) < 1f) {
        x += incr;
        y = Mathf.Sqrt((a2 - x * x) * b2 / a2);
      }
    }

    for (float y = 0; y <= ry;) {
      float x = Mathf.Sqrt((b2 - y * y) * a2 / b2);
      if (filled) {
        for (int px = Mathf.RoundToInt(cx - x); px <= Mathf.RoundToInt(cx + x); px++) {
          for (int py = Mathf.RoundToInt(cy - y); py <= Mathf.RoundToInt(cy + y); py++) {
            DrawPixel(px, py, border);
          }
        }
      }
      else {
        DrawPixel(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy + y), border); 
        DrawPixel(Mathf.RoundToInt(cx - x), Mathf.RoundToInt(cy + y), border); 
        DrawPixel(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy - y), border); 
        DrawPixel(Mathf.RoundToInt(cx - x), Mathf.RoundToInt(cy - y), border);
      }
      // Calculate the error, new y should be the same or 1 pixel off
      float oldx = x, oldy = y, incr = .01f;
      while (Mathf.Abs(x - oldx) < 1f && Mathf.Abs(y - oldy) < 1f) {
        y += incr;
        x = Mathf.Sqrt((b2 - y * y) * a2 / b2);
      }
    }
  }

  public void Shift(int dir) {
    action = ActionVal.No;
    SetButtons(-1);
    SetUndo(false);
    if (dir == 0) {
      for (int x = 0; x < w; x++) {
        byte tmp = pixels[x + 0].Get();
        for (int y = 0; y < h - 1; y++) {
          pixels[x + w * y].Set(pixels[x + w * (y + 1)].Get());
        }
        pixels[x + w * (h - 1)].Set(tmp);
      }
    }
    else if (dir == 2) {
      for (int x = 0; x < w; x++) {
        byte tmp = pixels[x + (h - 1)].Get();
        for (int y = h - 1; y > 0; y--) {
          pixels[x + w * y].Set(pixels[x + w * (y - 1)].Get());
        }
        pixels[x + w * 0].Set(tmp);
      }
    }
    else if (dir == 3) {
      for (int y = 0; y < h; y++) {
        byte tmp = pixels[0 + w * y].Get();
        for (int x = 0; x < w - 1; x++) {
          pixels[x + w * y].Set(pixels[x + 1 + w * y].Get());
        }
        pixels[w - 1 + w * y].Set(tmp);
      }
    }
    else if (dir == 1) {
      for (int y = 0; y < h; y++) {
        byte tmp = pixels[w - 1 + w * y].Get();
        for (int x = w - 1; x > 0; x--) {
          pixels[x + w * y].Set(pixels[x - 1 + w * y].Get());
        }
        pixels[0 + w * y].Set(tmp);
      }
    }
  }

  public void Flip(bool horiz) {
    action = ActionVal.No;
    SetButtons(-1);
    SetUndo(false);
    if (horiz) {
      for (int y = 0; y < h; y++) {
        for (int x = 0; x < w / 2; x++) {
          byte tmp = pixels[x + w * y].Get();
          pixels[x + w * y].Set(pixels[(w - x - 1) + w * y].Get());
          pixels[(w - x - 1) + w * y].Set(tmp);
        }
      }
    }
    else {
      for (int x = 0; x < h; x++) {
        for (int y = 0; y < h / 2; y++) {
          byte tmp = pixels[x + w * y].Get();
          pixels[x + w * y].Set(pixels[x + w * (h - y - 1)].Get());
          pixels[x + w * (h - y - 1)].Set(tmp);
        }
      }
    }
  }

  public void Rotate(bool back) {
    action = ActionVal.No;
    SetButtons(-1);
    SetUndo(false);
    int max = w > h ? w : h;
    int nw = h;
    int nh = w;
    // Extend to max size
    byte[] dst1 = new byte[max * max];
    byte[] dst2 = new byte[max * max];
    for (int x = 0; x < w; x++) {
      for (int y = 0; y < h; y++) {
        dst1[x + max * y] = pixels[x + w * y].Get();
      }
    }

    // rotate
    for (int x = 0; x < max; x++) {
      for (int y = 0; y < max; y++) {
        if (back)
          dst2[y + max * x] = dst1[x + max * (max - y - 1)];
        else
          dst2[x + max * (max - y - 1)] = dst1[y + max * x];
      }
    }

    // Re-create using actual size
    for (int i = 0; i < sizes.Length; i++) {
      if (sizes[i] <= nw) WidthSlider.SetValueWithoutNotify(sizes[i]);
      if (sizes[i] <= nh) HeightSlider.SetValueWithoutNotify(sizes[i]);
    }
    ChangeSpriteSize();

    for (int x = 0; x < max; x++) {
      for (int y = 0; y < max; y++) {
        if (x < 0 || x >= w || y < 0 || y > h) continue;
        pixels[x + w * y].Set(dst2[x + max * y]);
      }
    }
  }

  public void PickColor() {
    action = ActionVal.Pick;
    SetButtons(0);
  }

  public void SelectAll() {
    action = ActionVal.SelectAll;
    SetButtons(5);
  }

  void DrawPixel(int x, int y, bool border) {
    if (x < 0 || x >= w || y < 0 || y >= h) return;
    if (border)
      pixels[x + w * y].Select();
    else
      pixels[x + w * y].Set(CurrentColor);
  }

  public void Fill() {
    if (action == ActionVal.Fill) {
      action = ActionVal.No;
      SetButtons(-1);
    }
    else {
      action = ActionVal.Fill;
      SetButtons(4);
    }
  }

  public void Replace() {
    action = ActionVal.No;
    SetButtons(-1);
    SetUndo(false);
    for (int i = 0; i < selected.Length; i++) {
      if (selected[i])
        pixels[i].Set(CurrentColor);
    }
  }

  public void Save() {
    action = ActionVal.No;
    StartCoroutine(Saving());
  }
  IEnumerator Saving() {
    yield return PBar.Show("Saving", 0, 1 + w * h);
    SetButtons(-1);
    string res = "Sprite:\nusehex\n";
    byte sizexh = (byte)((w & 0xff00) >> 8);
    byte sizexl = (byte)(w & 0xff);
    byte sizeyh = (byte)((h & 0xff00) >> 8);
    byte sizeyl = (byte)(h & 0xff);

    res += sizexh.ToString("X2") + " " + sizexl.ToString("X2") + " " + sizeyh.ToString("X2") + " " + sizeyl.ToString("X2") + "\n";
    int num = w * h;
    yield return PBar.Progress(1);
    for (int i = 0; i < num; i += 4) {
      yield return PBar.Progress(1 + i);
      res += pixels[i].Get().ToString("X2");
      if (i + 1 < num) res += pixels[i + 1].Get().ToString("X2");
      if (i + 2 < num) res += pixels[i + 2].Get().ToString("X2");
      if (i + 3 < num) res += pixels[i + 3].Get().ToString("X2");
      res += " ";
    }
    res += "\n";
    Values.gameObject.SetActive(true);
    Values.text = res;
    PBar.Hide();
  }

  public void PreLoad() {
    action = ActionVal.No;
    SetButtons(-1);
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }

  public void PostLoad() {
    if (!gameObject.activeSelf) return;
    StartCoroutine(Loading());
  }

  IEnumerator Loading() {
    yield return PBar.Show("Loading", 0, 256);
    Message.text = "";
    string data = Values.text.Trim();

    byte[] block;
    try {
      ByteReader.ReadBlock(data, out List<CodeLabel> labels, out block);
    } catch (System.Exception e) {
      Dev.inst.HandleError("Parsing error: " + e.Message + "\n" + Values.text);
      yield break;
    }

    PBar.Progress(128);
    int wb = (block[0] << 8) + block[1];
    int hb = (block[2] << 8) + block[3];
    if (wb < 8 || hb < 8 || wb > 64 || hb > 64) {
      Message.text = "This does not look like a sprite.";
      PBar.Hide();
      yield break;
    }
    if (wb > 64 || hb > 64) { Dev.inst.HandleError("Invalid data block.\nSprite size too big"); yield break; }

    for (int i = 0; i < sizes.Length; i++) {
      if (sizes[i] <= wb) WidthSlider.SetValueWithoutNotify(i);
      if (sizes[i] <= hb) HeightSlider.SetValueWithoutNotify(i);
    }
    ChangeSpriteSize();
    yield return PBar.Show("Loading", 128, 128 + h);

    if (block.Length < 4 + w * h) { Dev.inst.HandleError("Invalid data block.\nNot enough data for a sprite"); yield break; }
    for (int i = 0; i < w * h; i++) {
      if (i % w == 0) yield return PBar.Progress(128 + i / w);
      pixels[i].Set(block[4 + i]);
    }

    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
    PBar.Hide();
  }

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
      yield return PBar.Show("Loading file", 5, 12 + h);
      Texture2D texture = DownloadHandlerTexture.GetContent(www);
      // Get the top-left part of the image fitting in the sprite size
      yield return PBar.Progress(5);
      TextureScale.Point(texture, w, h);
      yield return PBar.Progress(10);

      Color32[] tps = texture.GetPixels32();
      yield return PBar.Progress(12);

      for (int y = 0; y < h; y++) {
        int ty = h - y - 1;
        yield return PBar.Progress(12 + y);
        for (int x = 0; x < w; x++) {
          // Normalize the color
          int pos = x + w * ty;
          pixels[x + w * y].Set(Col.GetBestColor(tps[pos]));
        }
      }
    }
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
    yield return PBar.Show("Saving", 0, 1 + w * h);
    ByteChunk chunk = new ByteChunk();

    SetButtons(-1);
    byte[] block = new byte[4 + w * h];
    block[0] = (byte)((w & 0xff00) >> 8);
    block[1] = (byte)(w & 0xff);
    block[2] = (byte)((h & 0xff00) >> 8);
    block[3] = (byte)(h & 0xff);
    int num = w * h;
    yield return PBar.Progress(1);
    for (int i = 0; i < num; i++) {
      if (i % w == 0) yield return PBar.Progress(2 + i);
      block[4 + i] = pixels[i].Get();
    }
    chunk.AddBlock("Sprite", LabelType.Sprite, block);

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

    if (res.block.Length <= 2) { Dev.inst.HandleError("Invalid data block.\nNot enough data for a sprite"); yield break; }

    PBar.Progress(128);
    int wb = (res.block[0] << 8) + res.block[1];
    int hb = (res.block[2] << 8) + res.block[3];
    if (wb < 8 || hb < 8 || wb > 64 || hb > 64) {
      Dev.inst.HandleError("This does not look like a sprite.");
      yield break;
    }

    for (int i = 0; i < sizes.Length; i++) {
      if (sizes[i] <= wb) WidthSlider.SetValueWithoutNotify(i);
      if (sizes[i] <= hb) HeightSlider.SetValueWithoutNotify(i);
    }
    ChangeSpriteSize();
    yield return PBar.Show("Loading", 128, 128 + w * h);
    if (res.block.Length < 2 + w * h) { Dev.inst.HandleError("Invalid data block.\nNot enough data for a sprite"); yield break; }
    for (int i = 0; i < w * h; i++) {
      if (i % 4 == 0) yield return PBar.Progress(128 + i);
      pixels[i].Set(res.block[4 + i]);
    }
    PBar.Hide();
  }





  public void CloseValues() {
    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
  }

  public bool ValidCoord(int x, int y) {
    return !(x < 0 || y < 0 || x >= w || y >= h);
  }

  public void Fill(int x, int y, byte color) {
    SetUndo(false);
    // Visited pixels array
    bool[,] vis = new bool[w, h];

    // Initialing all as zero
    for (int i = 0; i < w; i++)
      for (int j = 0; j < h; j++)
        vis[i, j] = false;

    Queue<Vector2Int> obj = new Queue<Vector2Int>();
    obj.Enqueue(new Vector2Int(x, y));

    // Marking {x, y} as visited
    vis[x, y] = true;

    // Untill queue is emppty
    while (obj.Count != 0) {
      // Extrating front pair
      Vector2Int coord = obj.Dequeue();
      int x1 = coord.x;
      int y1 = coord.y;
      byte preColor = pixels[x1 + w * y1].Get();
      pixels[x1 + w * y1].Set(color);

      if (ValidCoord(x1 + 1, y1) && !vis[x1 + 1, y1] && pixels[x1 + 1 + w * y1].Get() == preColor) {
        obj.Enqueue(new Vector2Int(x1 + 1, y1));
        vis[x1 + 1, y1] = true;
      }

      if (ValidCoord(x1 - 1, y1) && !vis[x1 - 1, y1] && pixels[x1 - 1 + w * y1].Get() == preColor) {
        obj.Enqueue(new Vector2Int(x1 - 1, y1));
        vis[x1 - 1, y1] = true;
      }

      if (ValidCoord(x1, y1 + 1) && !vis[x1, y1 + 1] && pixels[x1 + w * (y1 + 1)].Get() == preColor) {
        obj.Enqueue(new Vector2Int(x1, y1 + 1));
        vis[x1, y1 + 1] = true;
      }

      if (ValidCoord(x1, y1 - 1) && !vis[x1, y1 - 1] && pixels[x1 + w * (y1 - 1)].Get() == preColor) {
        obj.Enqueue(new Vector2Int(x1, y1 - 1));
        vis[x1, y1 - 1] = true;
      }
    }
  }

  bool lastUndoWasPixel = false;
  void SetUndo(bool pixel) {
    byte[] val = new byte[w * h];
    for (int x = 0; x < w; x++)
      for (int y = 0; y < h; y++)
        val[x + w * y] = pixels[x + w * y].Get();
    if (pixel && lastUndoWasPixel && CurrentColor == lastPixelColor && undo.Count > 0)
      undo[undo.Count - 1] = val;
    else
      undo.Add(val);
    lastUndoWasPixel = pixel;
  }

  public void Undo() {
    action = ActionVal.No;
    SetButtons(-1);
    if (undo.Count == 0) return;
    byte[] val = undo[undo.Count - 1];
    undo.RemoveAt(undo.Count - 1);
    if (lastUndoWasPixel) {
      val = undo[undo.Count - 1];
      undo.RemoveAt(undo.Count - 1);
      lastUndoWasPixel = false;
    }
    for (int x = 0; x < w; x++)
      for (int y = 0; y < h; y++)
        pixels[x + w * y].Set(val[x + w * y]);
  }

  private void Update() {
    if (Input.GetMouseButtonDown(1) && action != ActionVal.No) {
      action = ActionVal.No;
      SetButtons(-1);
    }
  }

  public Button Done;
  public void ImportFrom(TileInPalette tile) {
    if (pixels == null) {
      pixels = new Pixel[tile.tw * tile.th];
      selected = new bool[tile.tw * tile.th];
      w = tile.tw;
      h = tile.th;
    }
    for (int i = 0; i < sizes.Length; i++) {
      if (sizes[i] <= tile.tw) WidthSlider.SetValueWithoutNotify(i);
      if (sizes[i] <= tile.th) HeightSlider.SetValueWithoutNotify(i);
    }
    ChangeSpriteSize();

    for (int x = 0; x < tile.tw; x++)
      for (int y = 0; y < tile.th; y++) {
        byte col = tile.rawData[x + w * y];
        pixels[x + w * y].Set(col);
      }

    SetUndo(false);
    Done.gameObject.SetActive(true);
    editFrom = EditComponent.TilesEditor;
  }

  public void ImportFrom(byte[] data) {
    int sw = (data[0] << 8) + data[1]; // Try new mode
    int sh = (data[2] << 8) + data[3];
    if (data.Length != 4 + sw * sh) { // Try old mode
      sw = data[0];
      sh = data[1];
      if (data.Length != 2 + sw * sh) { // Not valid
        Dev.inst.HandleError("Invalid Sprite/Image");
        return;
      }
    }

    w = sw;
    h = sh;
    if (pixels == null) {
      pixels = new Pixel[w * h];
      selected = new bool[w * h];
    }
    for (int i = 0; i < sizes.Length; i++) {
      if (sizes[i] <= w) WidthSlider.SetValueWithoutNotify(i);
      if (sizes[i] <= h) HeightSlider.SetValueWithoutNotify(i);
    }
    ChangeSpriteSize();

    for (int x = 0; x < w; x++)
      for (int y = 0; y < h; y++) {
        byte col = data[2 + x + w * y];
        pixels[x + w * y].Set(col);
      }
    SetUndo(false);
    Done.gameObject.SetActive(true);
    editFrom = EditComponent.RomEditor;
  }

  EditComponent editFrom;

  public void CompleteTileEditing() {
    if (editFrom == EditComponent.TilesEditor) {
      mapeditor.UpdateTile(pixels);
      Dev.inst.TilemapEditor();
    }
    else if (editFrom == EditComponent.RomEditor) {
      byte[] data = new byte[4 + pixels.Length];
      data[0] = (byte)((w & 0xff00) >> 8);
      data[1] = (byte)(w & 0xff);
      data[2] = (byte)((h & 0xff00) >> 8);
      data[3] = (byte)(h & 0xff);
      for (int i = 0; i < pixels.Length; i++) {
        data[4 + i] = pixels[i].Get();
      }
      romeditor.UpdateLine(data, LabelType.Sprite, LabelType.Image);
    }
    gameObject.SetActive(false);
  }


  public void TogglePalette() {
    LoadPaletteB.gameObject.SetActive(TogglePaletteB.isOn);
    ConvertPaletteB.gameObject.SetActive(TogglePaletteB.isOn);
    PaletteView.SetActive(TogglePaletteB.isOn);
    ColorsView.SetActive(!TogglePaletteB.isOn);
  }

  public void LoadPalette() {
    FileBrowser.Load(PostLoadPalette, FileBrowser.FileType.Rom);
  }

  public void PostLoadPalette(string path) {
    // Read the rom, get the first palette (show error if missing)
    // Set all pixels and the paletteCols[] with the palette values

    ByteChunk res = new ByteChunk();
    try {
      ByteReader.ReadBinBlock(path, res);
    } catch (System.Exception e) {
      Dev.inst.HandleError("Parsing error: " + e.Message);
      return;
    }

    foreach(CodeLabel l in res.labels) {
      if (l.type == LabelType.Palette) {
        int size = res.block[l.start];
        if (res.block.Length < l.start+size) { Dev.inst.HandleError("Invalid data block.\nNot enough data for a palette"); return; }
        Col.SetPalette(res.block, l.start, 0);
        break;
      }
    }
    RGEPalette.SetColorArray("_Colors", Col.GetPalette());

    // Update the colors of the image by picking the best color we have in the palette
    for (int i = 0; i < pixels.Length; i++) {
      Color32 c = pixels[i].Get32();
      byte pos = GetClosestColor(c);
      pixels[i].Set(pos);
    }
  }



  private byte GetClosestColor(Color32 color) {
    byte colorIndex = 0;
    int minError = int.MaxValue;
    for (int i = 0; i < 256; i++) {
      int dr = color.r - paletteCols[i].r;
      int dg = color.g - paletteCols[i].g;
      int db = color.b - paletteCols[i].b;
      int da = color.a - paletteCols[i].a;
      int error = dr * dr + dg * dg + db * db + da * da;
      if (error < minError) {
        minError = error;
        colorIndex = (byte)i;
      }
    }
    return colorIndex;
  }


  public void EditPalette() {
    Dev.inst.PaletteEditor();
    paletteEditor.EditPalette();
  }

  public void SelectPalettePixel(Pixel b, bool left) {
    CurrentColor = (byte)b.pos;
    CurrentColorImg.color = paletteCols[CurrentColor];
  }

  public TilemapEditor mapeditor;
  public RomEditor romeditor;
  public Confirm Confirm;
  public Toggle TogglePaletteB;
  public Button LoadPaletteB;
  public Button ConvertPaletteB;
  public GameObject PaletteView;
  public Transform PaletteContainer;
  public GameObject ColorsView;
  public PaletteEditor paletteEditor;
}

public enum ActionVal { No, LineStart, LineEnd, BoxStart, BoxEnd, EllipseStart, EllipseEnd, BoxStartF, BoxEndF, EllipseStartF, EllipseEndF, Fill, FreeDraw, Pick, SelectAll, Replace }

