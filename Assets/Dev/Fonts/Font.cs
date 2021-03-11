using System.Collections.Generic;
using UnityEngine;

public class Font {
  public int Name;
  public int w;
  public int h;
  public Dictionary<char, byte[]> chars = new Dictionary<char, byte[]>();
  public Dictionary<char, byte[]> graphs = new Dictionary<char, byte[]>();

  Color32[] charMtrx = new Color32[32 * 32];

  public void Generate() {
    int bsperline = (w >> 3) + 1;
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


  void Write(string txt, int x, int y, FontStyle fs = null) {
    int pos = x;


    /*
    go line by line, increase on newlines or /r th evertical position
    start all chars one pixel before and check the over
    Do not paint the over over the normal colors of previous chars
    Try to respect bold/italic/underline
    Try to respect font size multiplications (.25, .5, 1, 1.5, 2, 3, 4)

     */




    foreach (char c in txt) {
      if (c == '\n' || c == '\r') {
        y += 8;
        pos = x;
        if (y > 127) return;
        continue;
      }
      byte[] gliph;
      if (!grahs.ContainsKey(c))
        gliph = grahs['*'];
      else
        gliph = grahs[c];
      for (int h = 0; h < 8; h++) {
        for (int w = 0; w < 8; w++) {
          if ((gliph[h] & (1 << (7 - w))) != 0)
            SetPixel(pos + w, y + h, frontc);
          else if (back != 255 || rawTarget == rawUI)
            SetPixel(pos + w, y + h, backc);
        }
      }
      pos += 8;
      if (pos > wm1) return;
    }
  }
}


public class FontStyle {
  public bool bold;
  public bool italic;
  public bool underline;
  public byte front;
  public byte back;
  public byte outline;
  public byte size;
}





