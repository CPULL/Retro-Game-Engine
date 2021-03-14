using System.Collections.Generic;
using UnityEngine;

public class Font {
  public string name;
  public int w;
  public int h;
  public Dictionary<char, byte[]> chars = null;
  public Dictionary<char, byte[]> graphs = null;

  public Font(string name, int width, int height, Dictionary<char, byte[]> definition) {
    this.name = name;
    w = width;
    h = height;
    chars = definition;
    graphs = new Dictionary<char, byte[]>();
    Generate();
  }

  private void Generate() {
    int bsperline = (w + 1) >> 3;
    // Converts the packed bytes to a byte array (1 byte per pixel) to be used when drawing
    foreach (char c in chars.Keys) {
      byte[] graph = new byte[(w + 2) * (h + 2)];
      byte[] chbs = chars[c];
      for (int y = 0; y < h; y++) {
        for (int x = 0; x < w; x++) {
          int spos = (x >> 3) + bsperline * y;
          int dpos = 1 + x + (w + 2) * (y + 1);

          byte p = (byte)(x & 7);
          if ((chbs[spos] & (1 << (7 - p))) != 0)
            graph[dpos] = 1;
          else
            graph[dpos] = 0;
        }
      }
      // Generate the outline
      for (int y = 0; y < h + 2; y++) {
        for (int x = 0; x < w + 2; x++) {
          int pos = x + (w + 2) * y;
          if (graph[pos]==0) {
            if (x > 0 && y > 0 && graph[pos - 3 - w] == 1) graph[pos] = 2;
            else if (x > 0 && graph[pos - 1] == 1) graph[pos] = 2;
            else if (x > 0 && y < h - 1 && graph[pos + w + 1] == 1) graph[pos] = 2;
            else if (x < w - 1 && y > 0 && graph[pos - 1 - w] == 1) graph[pos] = 2;
            else if (x < w - 1 && graph[pos + 1] == 1) graph[pos] = 2;
            else if (x < w - 1 && y < h - 1 && graph[pos + w + 3] == 1) graph[pos] = 2;
            else if (y > 0 && graph[pos - w - 2] == 1) graph[pos] = 2;
            else if (y < h - 1 && graph[pos + w + 2] == 1) graph[pos] = 2;
          }
        }
      }
      // Save it
      graphs[c] = graph;
    }
  }

}


public class FontStyle {
  public byte front;
  public byte back;
  public byte outline;
  public bool useback;
  public bool useoutline;
  public Font font;

  public FontStyle(Font fnt, byte f, int o, int b) {
    font = fnt;
    front = f;
    if (o == -1) {
      useoutline = false;
      outline = 255;
    }
    else {
      useoutline = true;
      outline = (byte)o;
    }
    if (b == -1) {
      useback = false;
      back = 255;
    }
    else {
      useback = true;
      back = (byte)b;
    }
  }
}





