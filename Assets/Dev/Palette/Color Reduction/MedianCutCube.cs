// Based on code from AForge Image Processing Library
using System.Collections.Generic;
using UnityEngine;

// Color cube used by Median Cut color quantization algorithm
internal class MedianCutCube {
  private readonly List<Color32> colors;

  private readonly byte minR, maxR;
  private readonly byte minG, maxG;
  private readonly byte minB, maxB;

  private Color32? cubeColor = null;

  // Length of the "red side" of the cube
  public int RedSize {
    get { return maxR - minR; }
  }

  // Length of the "green size" of the cube
  public int GreenSize {
    get { return maxG - minG; }
  }

  // Length of the "blue size" of the cube
  public int BlueSize {
    get { return maxB - minB; }
  }

  // Mean cube's color
  public Color32 Color {
    get {
      if (cubeColor == null) {
        int red = 0, green = 0, blue = 0;

        foreach (Color32 color in colors) {
          red += color.r;
          green += color.g;
          blue += color.b;
        }

        int colorsCount = colors.Count;

        if (colorsCount != 0) {
          red /= colorsCount;
          green /= colorsCount;
          blue /= colorsCount;
        }

        cubeColor = new Color32((byte)red, (byte)green, (byte)blue, 255);
      }

      return cubeColor.Value;
    }
  }

  public MedianCutCube(List<Color32> colors) {
    this.colors = colors;

    // get min/max values for each RGB component of specified colors
    minR = minG = minB = 255;
    maxR = maxG = maxB = 0;

    foreach (Color32 color in colors) {
      if (color.r < minR)
        minR = color.r;
      if (color.r > maxR)
        maxR = color.r;

      if (color.g < minG)
        minG = color.g;
      if (color.g > maxG)
        maxG = color.g;

      if (color.b < minB)
        minB = color.b;
      if (color.b > maxB)
        maxB = color.b;
    }
  }

  // Split the cube into 2 smaller cubes using the specified color side for splitting
  public void SplitAtMedian(RGB rgbComponent, out MedianCutCube cube1, out MedianCutCube cube2) {
    switch (rgbComponent) {
      case RGB.R:
        colors.Sort(new RedComparer());
        break;
      case RGB.G:
        colors.Sort(new GreenComparer());
        break;
      case RGB.B:
        colors.Sort(new BlueComparer());
        break;
    }

    int median = colors.Count / 2;

    cube1 = new MedianCutCube(colors.GetRange(0, median));
    cube2 = new MedianCutCube(colors.GetRange(median, colors.Count - median));
  }

  #region Different comparers used for sorting colors by different components
  private class RedComparer : IComparer<Color32> {
    public int Compare(Color32 c1, Color32 c2) {
      return c1.r.CompareTo(c2.r);
    }
  }

  private class GreenComparer : IComparer<Color32> {
    public int Compare(Color32 c1, Color32 c2) {
      return c1.g.CompareTo(c2.g);
    }
  }

  private class BlueComparer : IComparer<Color32> {
    public int Compare(Color32 c1, Color32 c2) {
      return c1.b.CompareTo(c2.b);
    }
  }
  #endregion
}