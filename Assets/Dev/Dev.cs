using System;
using System.Collections.Generic;
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
  Color32 BorderNormal = new Color32(206, 224, 223, 120);
  DoShape shape = DoShape.No;
  Vector2Int start = Vector2Int.zero;
  int w, h;
  public Sprite[] boxes;

  public Text dbg; // FIXME remove



  public void ChangeSpriteSize() {
    WidthSliderText.text = "Width: " + WidthSlider.value;
    HeightSliderText.text = "Height: " + HeightSlider.value;
    Rect rt = SpriteGrid.transform.GetComponent<RectTransform>().rect;
    SpriteGrid.cellSize = new Vector2(rt.width / WidthSlider.value, rt.height / HeightSlider.value);

    int oldw = w;
    int oldh = h;
    Pixel[] oldps = pixels;

    w = (int)WidthSlider.value;
    h = (int)HeightSlider.value;

    int num = w * h;
    foreach(Transform t in SpriteGrid.transform)
      t.SetParent(null);


    pixels = new Pixel[num];
    Sprite box = w <= 8 && h <= 8 ? boxes[2] : boxes[1];
    if (w >= 24 || h >= 24) box = boxes[0];

    for (int i = 0; i < num; i++) {
      Pixel pixel = Instantiate(PixelPrefab, SpriteGrid.transform).GetComponent<Pixel>();
      pixel.Init(i, Transparent, ClickPixel, OverPixel);
      pixel.border.sprite = box;
      pixels[i] = pixel;
    }

    for (int x = 0; x < w; x++) {
      for (int y = 0; y < h; y++) {
        if (x < oldw && y < oldh)
          pixels[x + w * y].Set(oldps[x + oldw * y].Get());
      }
    }

    foreach (Pixel p in oldps)
      Destroy(p.gameObject);

  }

  private void ClickPixel(int pos) {
    int x = pos % w;
    int y = (pos - start.x) / w;

    if (shape == DoShape.LineStart) {
      start.x = x;
      start.y = y;
      pixels[pos].border.color = Color.red;
      shape = DoShape.LineEnd;
      return;
    }
    if (shape == DoShape.LineEnd) {
      int x1 = start.x;
      int x2 = x;
      int y1 = start.y;
      int y2 = y;
      DrawLine(x1, y1, x2, y2, false);
      shape = DoShape.No;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      return;
    }

    if (shape == DoShape.BoxStart) {
      start.x = x;
      start.y = y;
      pixels[pos].border.color = Color.red;
      shape = DoShape.BoxEnd;
      return;
    }
    if (shape == DoShape.BoxEnd) {
      int x1 = start.x;
      int x2 = x;
      int y1 = start.y;
      int y2 = y;
      DrawBox(x1, y1, x2, y2, false);
      shape = DoShape.No;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      return;
    }

    if (shape == DoShape.EllipseStart) {
      start.x = x;
      start.y = y;
      pixels[pos].border.color = Color.red;
      shape = DoShape.EllipseEnd;
      return;
    }
    if (shape == DoShape.EllipseEnd) {
      int x1 = start.x;
      int x2 = x;
      int y1 = start.y;
      int y2 = y;
      DrawEllipse(x1, y1, x2, y2, false);
      shape = DoShape.No;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      return;
    }

    if (shape == DoShape.Fill) {
      Fill(x, y, CurrentColor.color);
      shape = DoShape.No;
      return;
    }

    if (pixels[pos].Get() == CurrentColor.color)
      pixels[pos].Set(Transparent);
    else
      pixels[pos].Set(CurrentColor.color);
  }

  private void OverPixel(int pos) {
    if (shape == DoShape.LineEnd) {
      int x1 = start.x;
      int x2 = pos % w;
      int y1 = start.y;
      int y2 = (pos - x2) / w;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      DrawLine(x1, y1, x2, y2, true);
    }

    if (shape == DoShape.BoxEnd) {
      int x1 = start.x;
      int x2 = pos % w;
      int y1 = start.y;
      int y2 = (pos - x2) / w;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      DrawBox(x1, y1, x2, y2, true);
    }

    if (shape == DoShape.EllipseEnd) {
      int x1 = start.x;
      int x2 = pos % w;
      int y1 = start.y;
      int y2 = (pos - x2) / w;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      DrawEllipse(x1, y1, x2, y2, true);
    }




  }

  public void Clear() {
    int num = w * h;
    for (int i = 0; i < num; i++)
      pixels[i].Set(Transparent);
  }

  public void Line() {
    shape = DoShape.LineStart;
  }

  public void Box() {
    shape = DoShape.BoxStart;
  }

  public void Ellipse() {
    shape = DoShape.EllipseStart;
  }

  void DrawLine(int x1, int y1, int x2, int y2, bool border) {
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
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y = y + 1; else y -= 1;
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
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1; else x -= 1;
          py += 2 * (dx1 - dy1);
        }
        DrawPixel(x, y, border);
      }
    }
  }

  void DrawBox(int x1, int y1, int x2, int y2, bool border) {
    int sx = x1; if (sx > x2) sx = x2;
    int sy = y1; if (sy > y2) sy = y2;
    int ex = x1; if (ex < x2) ex = x2;
    int ey = y1; if (ey < y2) ey = y2;

    for (int x = sx; x <= ex; x++) {
      DrawPixel(x, y1, border);
      DrawPixel(x, y2, border);
    }
    for (int y = sy; y <= ey; y++) {
      DrawPixel(x1, y, border);
      DrawPixel(x2, y, border); 
    }
  }

  void DrawEllipse(int x1, int y1, int x2, int y2, bool border) {
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
      DrawPixel(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy + y), border); 
      DrawPixel(Mathf.RoundToInt(cx - x), Mathf.RoundToInt(cy + y), border); 
      DrawPixel(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy - y), border); 
      DrawPixel(Mathf.RoundToInt(cx - x), Mathf.RoundToInt(cy - y), border);
      // Calculate the error, new y should be the same or 1 pixel off
      float oldx = x, oldy = y, incr = .01f;
      while (Mathf.Abs(x - oldx) < 1f && Mathf.Abs(y - oldy) < 1f) {
        x += incr;
        y = Mathf.Sqrt((a2 - x * x) * b2 / a2);
      }
    }

    for (float y = 0; y <= ry; ) {
      float x = Mathf.Sqrt((b2 - y*y) * a2 / b2);
      DrawPixel(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy + y), border); 
      DrawPixel(Mathf.RoundToInt(cx - x), Mathf.RoundToInt(cy + y), border); 
      DrawPixel(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy - y), border); 
      DrawPixel(Mathf.RoundToInt(cx - x), Mathf.RoundToInt(cy - y), border);
      // Calculate the error, new y should be the same or 1 pixel off
      float oldx = x, oldy = y, incr = .01f;
      while (Mathf.Abs(x - oldx) < 1f && Mathf.Abs(y - oldy) < 1f) {
        y += incr;
        x = Mathf.Sqrt((b2 - y * y) * a2 / b2);
      }
    }
  }

  public void Shift(int dir) {
    if (dir == 0) {
      for (int x = 0; x < w; x++) {
        Color32 tmp = pixels[x + 0].Get();
        for (int y = 0; y < h - 1; y++) {
          pixels[x + w * y].Set(pixels[x + w * (y + 1)].Get());
        }
        pixels[x + w * (h - 1)].Set(tmp);
      }
    }
    else if (dir == 2) {
      for (int x = 0; x < w; x++) {
        Color32 tmp = pixels[x + (h - 1)].Get();
        for (int y = h - 1; y > 0; y--) {
          pixels[x + w * y].Set(pixels[x + w * (y - 1)].Get());
        }
        pixels[x + w * 0].Set(tmp);
      }
    }
    else if (dir == 3) {
      for (int y = 0; y < h; y++) {
        Color32 tmp = pixels[0 + w * y].Get();
        for (int x = 0; x < w - 1; x++) {
          pixels[x + w * y].Set(pixels[x + 1 + w * y].Get());
        }
        pixels[w - 1 + w * y].Set(tmp);
      }
    }
    else if (dir == 1) {
      for (int y = 0; y < h; y++) {
        Color32 tmp = pixels[w - 1 + w * y].Get();
        for (int x = w - 1; x > 0; x--) {
          pixels[x + w * y].Set(pixels[x - 1 + w * y].Get());
        }
        pixels[0 + w * y].Set(tmp);
      }
    }
  }

  public void Flip(bool horiz) {
    if (horiz) {
      for (int y = 0; y < h; y++) {
        for (int x = 0; x < w / 2; x++) {
          Color32 tmp = pixels[x + w * y].Get();
          pixels[x + w * y].Set(pixels[(w - x - 1) + w * y].Get());
          pixels[(w - x - 1) + w * y].Set(tmp);
        }
      }
    }
    else {
      for (int x = 0; x < h; x++) {
        for (int y = 0; y < h / 2; y++) {
          Color32 tmp = pixels[x + w * y].Get();
          pixels[x + w * y].Set(pixels[x + w * (h - y - 1)].Get());
          pixels[x + w * (h - y - 1)].Set(tmp);
        }
      }
    }
  }

  public void Rotate(bool back) {
    int max = w > h ? w : h;
    int nw = h;
    int nh = w;
    // Extend to max size
    Color32[] dst1 = new Color32[max * max];
    Color32[] dst2 = new Color32[max * max];
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

    /*

    00 01 02 03 04
    10 11 12 13 14
    20 21 22 23 24
    30 31 32 33 34
    40 41 42 43 44

    [y][x] = [x][w-y-1]
    [4][0] = [0][0]
    [0][0] = [0][4]
    [0][4] = [4][4]

    */

    // Re-create using actual size
    WidthSlider.SetValueWithoutNotify(nw);
    HeightSlider.SetValueWithoutNotify(nh);
    ChangeSpriteSize();

    for (int x = 0; x < max; x++) {
      for (int y = 0; y < max; y++) {
        if (x < 0 || x >= w || y < 0 || y > h) continue;
        pixels[x + w * y].Set(dst2[x + max * y]);
      }
    }
  }

  void DrawPixel(int x, int y, bool border) {
    if (x < 0 || x >= w || y < 0 || y >= h) return;
    if (border)
      pixels[x + w * y].border.color = Color.red;
    else
      pixels[x + w * y].Set(CurrentColor.color);
  }

  public void Fill() {
    shape = DoShape.Fill;
  }

  public void Save() {
    string res = "SpriteSize:\n";
    byte sizex = ((byte)((byte)w & 31));
    byte sizey = ((byte)((byte)h & 31));
    res += "0x" + sizex.ToString("X2") + " 0x" + sizey.ToString("X2") + "\n";
    res += "Sprite:";
    int num = w * h;
    for (int i = 0; i < num; i++) {
      if (i % sizex == 0) res += "\n";
      Color32 c = pixels[i].Get();
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

  readonly Regex rgComments = new Regex("([^\\n]*)(//[^\\n]*)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgLabels = new Regex("[\\s]*[a-z0-9]+:[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgHex = new Regex("[\\s]*0x([a-f0-9]+)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  public void PostLoad() {
    string data = Values.text.Trim();
    data = rgComments.Replace(data, " ");
    data = rgLabels.Replace(data, " ");
    data = data.Replace('\n', ' ').Trim();
    while (data.IndexOf("  ") != -1) data = data.Replace("  ", " ");

    data = ReadNextByte(data, out byte wb);
    data = ReadNextByte(data, out byte hb);
    if (wb < 8 || hb < 8 || wb > 32 || hb > 32) {
      Values.text = "This does not look like a sprite.\n" + Values.text;
      return;
    }

    WidthSlider.SetValueWithoutNotify(wb);
    HeightSlider.SetValueWithoutNotify(hb);
    w = wb;
    h = hb;
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

  public bool ValidCoord(int x, int y) {
    return !(x < 0 || y < 0 || x >= w || y >= h);
  }

  public void Fill(int x, int y, Color32 color) {
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
      Color32 preColor = pixels[x1 + w * y1].Get();
      pixels[x1 + w * y1].Set(color);

      if (ValidCoord(x1 + 1, y1) && !vis[x1 + 1, y1] && pixels[x1 + 1 + w * y1].img.color == preColor) {
        obj.Enqueue(new Vector2Int(x1 + 1, y1));
        vis[x1 + 1, y1] = true;
      }

      if (ValidCoord(x1 - 1, y1) && !vis[x1 - 1, y1] && pixels[x1 - 1 + w * y1].img.color == preColor) {
        obj.Enqueue(new Vector2Int(x1 - 1, y1));
        vis[x1 - 1, y1] = true;
      }

      if (ValidCoord(x1, y1 + 1) && !vis[x1, y1 + 1] && pixels[x1 + w * (y1 + 1)].img.color == preColor) {
        obj.Enqueue(new Vector2Int(x1, y1 + 1));
        vis[x1, y1 + 1] = true;
      }

      if (ValidCoord(x1, y1 - 1) && !vis[x1, y1 - 1] && pixels[x1 + w * (y1 - 1)].img.color == preColor) {
        obj.Enqueue(new Vector2Int(x1, y1 - 1));
        vis[x1, y1 - 1] = true;
      }
    }
  }

  private void Update() {
    if (Input.GetMouseButtonDown(1) && shape != DoShape.No) {
      shape = DoShape.No;
      for (int i = 0; i < w * h; i++)
        pixels[i].border.color = BorderNormal;
    }
  }

  #endregion Sprite Editor

}

public enum DoShape { No, LineStart, LineEnd, BoxStart, BoxEnd, EllipseStart, EllipseEnd, Fill }

// Add freedraw, if rmb and overitem set pixel

