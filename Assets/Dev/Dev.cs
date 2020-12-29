﻿using System;
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
  DoLine line = DoLine.No;
  Vector2Int lineStart = Vector2Int.zero;

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
    if (line == DoLine.SetStart) {
      lineStart.x = pos % (int)WidthSlider.value;
      lineStart.y = (pos - lineStart.x) / (int)WidthSlider.value;
      pixels[pos].border.color = Color.red;
      line = DoLine.SetEnd;
      return;
    }
    if (line == DoLine.SetEnd) {
      int x1 = lineStart.x;
      int x2 = pos % (int)WidthSlider.value;
      int y1 = lineStart.y;
      int y2 = (pos - x2) / (int)WidthSlider.value;
      DrawLine(x1, y1, x2, y2, false, CurrentColor.color);
      line = DoLine.No;
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
    if (line == DoLine.SetEnd) {
      int x1 = lineStart.x;
      int x2 = pos % (int)WidthSlider.value;
      int y1 = lineStart.y;
      int y2 = (pos - x2) / (int)WidthSlider.value;
      for (int i = 0; i < pixels.Length; i++)
        pixels[i].border.color = BorderNormal;
      DrawLine(x1, y1, x2, y2, true, Color.red);
    }




  }

  public void Clear() {
    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    for (int i = 0; i < num; i++)
      pixels[i].Set(Transparent);
  }

  public void Line() {
    line = DoLine.SetStart;
  }

  void DrawLine(int x1, int y1, int x2, int y2, bool border, Color32 col) {
    int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
    int w = (int)WidthSlider.value;
    dx = x2 - x1; dy = y2 - y1;
    if (dx == 0) { // Vertical
      if (y2 < y1) { int tmp = y1; y1 = y2; y2 = tmp; }
      for (y = y1; y <= y2; y++)
        if (border)
          pixels[x1 + w * y].border.color = Color.red;
        else
          pixels[x1 + w * y].Set(CurrentColor.color);
      return;
    }

    if (dy == 0) { // Horizontal
      if (x2 < x1) { int tmp = x1; x1 = x2; x2 = tmp; }
      for (x = x1; x <= x2; x++)
        if (border)
          pixels[x + w * y1].border.color = Color.red;
        else
          pixels[x + w * y1].Set(CurrentColor.color);
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
      if (border)
        pixels[x + w * y].border.color = Color.red;
      else
        pixels[x + w * y].Set(CurrentColor.color);
      for (i = 0; x < xe; i++) {
        x += 1;
        if (px < 0)
          px += 2 * dy1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y = y + 1; else y -= 1;
          px += 2 * (dy1 - dx1);
        }
        if (border)
          pixels[x + w * y].border.color = Color.red;
        else
          pixels[x + w * y].Set(CurrentColor.color);
      }
    }
    else {
      if (dy >= 0) {
        x = x1; y = y1; ye = y2;
      }
      else {
        x = x2; y = y2; ye = y1;
      }
      if (border)
        pixels[x + w * y].border.color = Color.red;
      else
        pixels[x + w * y].Set(CurrentColor.color);
      for (i = 0; y < ye; i++) {
        y += 1;
        if (py <= 0)
          py += 2 * dx1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1; else x -= 1;
          py += 2 * (dx1 - dy1);
        }
        if (border)
          pixels[x + w * y].border.color = Color.red;
        else
          pixels[x + w * y].Set(CurrentColor.color);
      }
    }
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

public enum DoLine { No, SetStart, SetEnd }