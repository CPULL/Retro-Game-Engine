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
  Color32 BorderNormal = new Color32(206, 224, 223, 120);
  DoShape shape = DoShape.No;
  Vector2Int start = Vector2Int.zero;
  int w, h;

  public void ChangeSpriteSize() {
    WidthSliderText.text = "Width: " + WidthSlider.value;
    HeightSliderText.text = "Height: " + HeightSlider.value;
    Rect rt = SpriteGrid.transform.GetComponent<RectTransform>().rect;
    SpriteGrid.cellSize = new Vector2(rt.width / WidthSlider.value, rt.height / HeightSlider.value);
    w = (int)WidthSlider.value;
    h = (int)HeightSlider.value;

    int num = w * h;
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
        pixel.Init(i, Transparent, ClickPixel, OverPixel);
      }
    }
    else if (num > numnow) {
      for (int i = numnow; i < num; i++) {
        Destroy(SpriteGrid.transform.GetChild(i));
      }
    }
  }

  private void ClickPixel(int pos) {
    if (shape == DoShape.LineStart) {
      start.x = pos % w;
      start.y = (pos - start.x) / w;
      pixels[pos].border.color = Color.red;
      shape = DoShape.LineEnd;
      return;
    }
    if (shape == DoShape.LineEnd) {
      int x1 = start.x;
      int x2 = pos % w;
      int y1 = start.y;
      int y2 = (pos - x2) / w;
      DrawLine(x1, y1, x2, y2, false);
      shape = DoShape.No;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      return;
    }

    if (shape == DoShape.BoxStart) {
      start.x = pos % w;
      start.y = (pos - start.x) / w;
      pixels[pos].border.color = Color.red;
      shape = DoShape.BoxEnd;
      return;
    }
    if (shape == DoShape.BoxEnd) {
      int x1 = start.x;
      int x2 = pos % w;
      int y1 = start.y;
      int y2 = (pos - x2) / w;
      DrawBox(x1, y1, x2, y2, false);
      shape = DoShape.No;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      return;
    }

    if (shape == DoShape.EllipseStart) {
      start.x = pos % w;
      start.y = (pos - start.x) / w;
      pixels[pos].border.color = Color.red;
      shape = DoShape.EllipseEnd;
      return;
    }
    if (shape == DoShape.EllipseEnd) {
      int x1 = start.x;
      int x2 = pos % w;
      int y1 = start.y;
      int y2 = (pos - x2) / w;
      DrawEllipse(x1, y1, x2, y2, false);
      shape = DoShape.No;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      return;
    }

    if (pixels[pos].img.color == CurrentColor.color)
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

  public Text dbg;

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



  void DrawPixel(int x, int y, bool border) {
    if (x < 0 || x >= w || y < 0 || y >= h) return;
    if (border)
      pixels[x + w * y].border.color = Color.red;
    else
      pixels[x + w * y].Set(CurrentColor.color);
  }

  public void Fill() {
    int num = w * h;
    for (int i = 0; i < num; i++)
      pixels[i].Set(CurrentColor.color);
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



  #endregion Sprite Editor
}

public enum DoShape { No, LineStart, LineEnd, BoxStart, BoxEnd, EllipseStart, EllipseEnd }