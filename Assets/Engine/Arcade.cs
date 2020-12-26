using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Arcade : MonoBehaviour {
  public RawImage Screen;
  public Text text;
  Texture2D texture;
  Color32[] pixels;
  byte[] raw;
  public int x, y;
  public int numPx = 16;
  CodeParser cp;
  const int w = 256;
  const int h = 160;
  const int wm1 = 255;
  const int hm1 = 159;
  public Register[] regs;
  public byte[] mem;

  float updateDelay = -1;
  bool startCompleted = false;
  CodeNode startCode;
  CodeNode updateCode;
  int pc = 0;
  readonly List<ExecStack> stacks = new List<ExecStack>();

  private void Update() {
    if (updateDelay < 0) return;
    if (updateDelay > 0) {
      updateDelay -= Time.deltaTime;

      if (updateDelay > 0) {
        int num = (int)(w * updateDelay * .5f);
        for (int i = 0; i < num; i++) {
          SetPixel(i, hm1 - 1, 0b111111);
          SetPixel(i, hm1, 0b111111);
        }
        for (int i = num; i < w; i++) {
          SetPixel(i, hm1 - 1, 0);
          SetPixel(i, hm1, 0);
        }
        texture.Apply();
        return;
      }
      updateDelay = 0;
    }


    bool something = false;
    if (!startCompleted && startCode != null) {
      while (stacks.Count > 0) {
        ExecStack stack = stacks[stacks.Count - 1];
        while (stack.step < stack.node.children.Count) {
          CodeNode n = stack.node.children[stack.step];
          something = true;
          stack.step++;
          if (Execute(n)) return; // Skip the execution for now so Unity can actually draw the frame
        }
        if (stack.cond != null && Evaluate(stack.cond).ToInt() != 0) {
          stack.step = 0;
        }
        else
          stacks.RemoveAt(stacks.Count - 1);
        if (something) texture.Apply();
      }

      while (pc < startCode.children.Count) {
        CodeNode n = startCode.children[pc];
        pc++;
        if (Execute(n)) return; // Skip the execution for now so Unity can actually draw the frame
      }
      startCompleted = true;
      pc = 0;
      texture.Apply();
      return;
    }

    // Update cycle
    if (updateCode == null) {
      if (something) texture.Apply();
      return;
    }

    while (stacks.Count > 0) {
      ExecStack stack = stacks[stacks.Count - 1];
      while (stack.step < stack.node.children.Count) {
        CodeNode n = stack.node.children[stack.step];
        something = true;
        stack.step++;
        if (Execute(n)) return; // Skip the execution for now so Unity can actually draw the frame
      }
      if (stack.cond != null && Evaluate(stack.cond).ToInt() != 0) {
        stack.step = 0;
      }
      else
        stacks.RemoveAt(stacks.Count - 1);
      if (something) texture.Apply();
    }

    while (pc < updateCode.children.Count) {
      CodeNode n = updateCode.children[pc];
      something = true;
      pc++;
      if (Execute(n)) return; // Skip the execution for now so Unity can actually draw the frame
    }
    pc = 0;

    if (something) texture.Apply();
  }

  private void Start() {
    regs = new Register[26];
    for (int i = 0; i < 26; i++)
      regs[i] = new Register((char)(97 + i));

    cp = GetComponent<CodeParser>();
    texture = new Texture2D(w, h, TextureFormat.RGBA32, false) {
      filterMode = FilterMode.Point
    };
    Screen.texture = texture;
    pixels = texture.GetPixels32();
    raw = new byte[w * h * 4];

    Clear(0);
    Write("--- MMM Arcade RGE ---", 35, 8, 60);
    Write(" virtual machine", 55, 14 + 4, 0b011010);
    Write(" Retro Game Engine", 45, 14 + 9, 0b011110);

    string codefile = null;
    try { codefile = File.ReadAllText(Application.dataPath + "\\Cartridges\\game.cartridge"); } catch (System.Exception) { }
    if (string.IsNullOrEmpty(codefile)) {
      Write("No cardridge found!", 4, 40, 48);
      texture.Apply();
      return;
    }

    try {
      CodeNode res = cp.Parse(codefile);
      Write("Cartridge:", 4, 39, 0b001011);
      if (res.sVal == null)
        Write("<no name>", 88, 39, 0b1001000);
      else
        Write(res.sVal, 88, 39, 0b1001000);

      startCode = res.Get(BNF.Start);
      if (startCode != null && startCode.children != null && startCode.children.Count > 0) {
        Write("Start:  Yes", 4, 48, 0b001011);
      }
      else {
        Write("Start:  ", 4, 48, 0b001011);
        Write("<missing>", 68, 48, 0b1001000);
        startCode = null;
      }

      updateCode = res.Get(BNF.Update);
      if (updateCode != null && updateCode.children != null && updateCode.children.Count > 0) {
        Write("Update: Yes", 4, 48 + 9, 0b001011);
      }
      else {
        Write("Update:  ", 4, 48 + 9, 0b001011);
        Write("<missing>", 68, 48 + 9, 0b1001000);
        updateCode = null;
      }

      if (res.HasNode(BNF.Data)) {
        Write("Data:   Yes", 4, 48 + 18, 0b001011);
        //FIXME
      }
      else {
        Write("Data:   ", 4, 48 + 18, 0b001011);
        Write("<missing>", 68, 48 + 18, 0b1001000);
        mem = new byte[256 * 1024];
      }

      Write("Screen: " + w + " x " + h, 10, 100, 0b001110);
      Write("Memory: " + (mem.Length / 1024) + "k (" + mem.Length + ")" , 10, 110, 0b001110);

      updateDelay = 2.5f;

    } catch (Exception e) {
      Write(e.Message, 4, 48, 48);
      Debug.Log("!!!!!!!! " + e.Message + "\n" + e.StackTrace);
    }
    texture.Apply();
  }

  #region Drawing functions

  void SetPixel(int x, int y, byte col) {
    if (x < 0 || x > wm1 || y < 0 || y > hm1) return;
    Color32 pixel = pixels[x + w * y];
    pixel.r = (byte)(((col & 0b00110000) >> 4) * 85);
    pixel.g = (byte)(((col & 0b00001100) >> 2) * 85);
    pixel.b = (byte)(((col & 0b00000011) >> 0) * 85);
    texture.SetPixel(x, hm1 - y, pixel);
  }

  void SetPixel(int x, int y, byte r, byte g, byte b) {
    if (x < 0 || x > wm1 || y < 0 || y > hm1) return;
    Color32 pixel = pixels[x + w * y];
    pixel.r = r;
    pixel.g = g;
    pixel.b = b;
    texture.SetPixel(x, hm1 - y, pixel);
  }

  void Write(string txt, int x, int y, byte col, byte back = 255) {
    int pos = x;
    byte r = (byte)(((col & 0b00110000) >> 4) * 85);
    byte g = (byte)(((col & 0b00001100) >> 2) * 85);
    byte b = (byte)(((col & 0b00000011) >> 0) * 85);
    byte rbk = (byte)(((back & 0b00110000) >> 4) * 85);
    byte gbk = (byte)(((back & 0b00001100) >> 2) * 85);
    byte bbk = (byte)(((back & 0b00000011) >> 0) * 85);
    foreach (char c in txt) {
      if (c == '\n' || c == '\r') {
        y += 8;
        pos = x;
        if (y > 127) return;
        continue;
      }
      byte[] gliph;
      if (!font.ContainsKey(c))
        gliph = font['*'];
      else
        gliph = font[c];
      for (int h = 0; h < 8; h++) {
        for (int w = 0; w < 8; w++) {
          if ((gliph[h] & (1 << (7 - w))) != 0)
            SetPixel(pos + w, y + h, r, g, b);
          else if (back != 255)
            SetPixel(pos + w, y + h, rbk, gbk, bbk);
        }
      }
      pos += 8;
      if (pos > wm1) return;
    }
  }

  void Clear(byte col) {
    byte r = (byte)(((col & 0b110000) >> 4) * (1 + 4 + 16 + 64));
    byte g = (byte)(((col & 0b1100) >> 2) * (1 + 4 + 16 + 64));
    byte b = (byte)((col & 0b11) * (1 + 4 + 16 + 64));

    int size = w * h * 4;
    for (int i = 0; i < size; i+=4) {
      raw[0] = r;
      raw[1] = g;
      raw[2] = b;
      raw[3] = 255;
    }
    texture.LoadRawTextureData(raw);
  }

  void Line(int x1, int y1, int x2, int y2, byte col) {
    int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
    dx = x2 - x1; dy = y2 - y1;

    if (dx == 0) { // Vertical
      if (y2 < y1) { int tmp = y1; y1 = y2; y2 = tmp; }
      for (y = y1; y <= y2; y++) SetPixel(x1, y, col);
      return;
    }

    if (dy == 0) { // Horizontal
      if (x2 < x1) { int tmp = x1; x1 = x2; x2 = tmp; }
      for (x = x1; x <= x2; x++) SetPixel(x, y1, col);
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

      SetPixel(x, y, col);

      for (i = 0; x < xe; i++) {
        x += 1;
        if (px < 0)
          px += 2 * dy1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y = y + 1; else y -= 1;
          px += 2 * (dy1 - dx1);
        }
        SetPixel(x, y, col);
      }
    }
    else {
      if (dy >= 0) {
        x = x1; y = y1; ye = y2;
      }
      else {
        x = x2; y = y2; ye = y1;
      }

      SetPixel(x, y, col);

      for (i = 0; y < ye; i++) {
        y += 1;
        if (py <= 0)
          py += 2 * dx1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1; else x -= 1;
          py += 2 * (dx1 - dy1);
        }
        SetPixel(x, y, col);
      }
    }
  }

  void Box(int x1, int y1, int x2, int y2, byte col, byte back = 255) {
    if (x1 > x2) { int tmp = x1; x1 = x2; x2 = tmp; }
    if (y1 > y2) { int tmp = y1; y1 = y2; y2 = tmp; }
    for (int x = x1; x <= x2; x++) {
      SetPixel(x, y1, col);
      SetPixel(x, y2, col);
    }
    for (int y = y1; y <= y2; y++) {
      SetPixel(x1, y, col);
      SetPixel(x2, y, col);
    }
    if (back != 255) {
      for (int x = x1 + 1; x < x2; x++) {
        for (int y = y1 + 1; y < y2; y++) {
          SetPixel(x, y, back);
        }
      }
    }
  }

  void Circle(float cx, float cy, float rx, float ry, byte col, byte back = 255) {
    if (rx <= 0 || ry <= 0) return;
    int minx = (int)(cx - rx);
    int maxx = (int)(cx + rx + 1);
    int miny = (int)(cy - ry);
    int maxy = (int)(cy + ry + 1);

    float invrx = 1 / (rx * rx);
    float invry = 1 / (ry * ry);

    for (int x = minx; x < maxx; x++) {
      for (int y = miny; y < maxy; y++) {
        float px = x - cx;
        float py = y - cy;
        float p = (px * px * invrx) + (py * py * invry);

        if (p < 1.02f) {
          if (p > .98f) SetPixel(x, y, col);
          else if (back != 255) SetPixel(x, y, back);
        }

      }
    }
  }

  #endregion Drawing functions

  bool Execute(CodeNode n) {
    try {
      switch (n.type) {
        case BNF.CLR: {
          Register tmp = Evaluate(n.First);
          Clear(tmp.ToByte());
        }
        break;

        case BNF.FRAME: {
          texture.Apply();
          return true; // We will skip to the next frame
        }

        case BNF.WRITE: {
          Register a = Evaluate(n.First);
          Register b = Evaluate(n.children[1]);
          Register c = Evaluate(n.children[2]);
          Register d = Evaluate(n.children[3]);
          if (n.children.Count == 5) {
            Register e = Evaluate(n.children[4]);
            Write(a.ToString(), b.ToInt(), c.ToInt(), d.ToByte(), e.ToByte());
          }
          else
            Write(a.ToString(), b.ToInt(), c.ToInt(), d.ToByte());
        }
        break;

        case BNF.Inc: {
          Register a = Evaluate(n.First);
          a.Incr();
        }
        break;

        case BNF.Dec: {
          Register a = Evaluate(n.First);
          a.Decr();
        }
        break;

        case BNF.ASSIGN:
        case BNF.ASSIGNsum:
        case BNF.ASSIGNsub:
        case BNF.ASSIGNmul:
        case BNF.ASSIGNdiv:
        case BNF.ASSIGNmod:
        case BNF.ASSIGNand:
        case BNF.ASSIGNor:
        case BNF.ASSIGNxor: {
          Register r = Evaluate(n.Second);
          if (n.First.type == BNF.REG) {
            Register a = Evaluate(n.First);
            a.Set(r, n.type);
          }
          else if (n.First.type == BNF.MEM) {
            int pos = Evaluate(n.First.First).ToInt();
            if (pos < 0 || pos > mem.Length) throw new Exception("Memory violation:" + pos + "from:" + n);
            switch (n.type) {
              case BNF.ASSIGN: mem[pos] = r.ToByte(); break;
              case BNF.ASSIGNsum: mem[pos] += r.ToByte(); break;
              case BNF.ASSIGNsub: mem[pos] -= r.ToByte(); break;
              case BNF.ASSIGNmul: mem[pos] *= r.ToByte(); break;
              case BNF.ASSIGNdiv: mem[pos] /= r.ToByte(); break;
              case BNF.ASSIGNmod: mem[pos] %= r.ToByte(); break;
              case BNF.ASSIGNand: mem[pos] &= r.ToByte(); break;
              case BNF.ASSIGNor: mem[pos] |= r.ToByte(); break;
              case BNF.ASSIGNxor: mem[pos] ^= r.ToByte(); break;
            }
          }
          else if (n.First.type == BNF.MEMlong) {
            int pos = Evaluate(n.First.First).ToInt();
            if (pos < 0 || pos > mem.Length) throw new Exception("Memory violation:" + pos + "from:" + n);

            // Get the value from memory, get the value from registry, do the operation, store it as sequence of bytes
            if (r.type == VT.None) {
              switch (n.type) {
                case BNF.ASSIGN: mem[pos] = 0; break;
                case BNF.ASSIGNmul: mem[pos] = 0; break;
                case BNF.ASSIGNdiv: throw new Exception("Division by zero: " + n);
                case BNF.ASSIGNmod: throw new Exception("Division by zero: " + n);
                case BNF.ASSIGNand: mem[pos] = 0; break;
              }
            }
            else if (r.type == VT.Int) {
              int val = BitConverter.ToInt32(mem, pos);
              switch (n.type) {
                case BNF.ASSIGN: val = r.ToInt(); break;
                case BNF.ASSIGNsum: val += r.ToInt(); break;
                case BNF.ASSIGNsub: val -= r.ToInt(); break;
                case BNF.ASSIGNmul: val *= r.ToInt(); break;
                case BNF.ASSIGNdiv: val /= r.ToInt(); break;
                case BNF.ASSIGNmod: val %= r.ToInt(); break;
                case BNF.ASSIGNand: val &= r.ToInt(); break;
                case BNF.ASSIGNor: val |= r.ToInt(); break;
                case BNF.ASSIGNxor: val ^= r.ToInt(); break;
              }
              byte[] vals = BitConverter.GetBytes(val);
              for (int i = 0; i < vals.Length; i++)
                mem[pos + i] = vals[i];
            }
            else if (r.type == VT.Float) {
              float val = BitConverter.ToSingle(mem, pos);
              switch (n.type) {
                case BNF.ASSIGN: val = r.ToFloat(); break;
                case BNF.ASSIGNsum: val += r.ToFloat(); break;
                case BNF.ASSIGNsub: val -= r.ToFloat(); break;
                case BNF.ASSIGNmul: val *= r.ToFloat(); break;
                case BNF.ASSIGNdiv: val /= r.ToFloat(); break;
                case BNF.ASSIGNmod: val %= r.ToFloat(); break;
              }
              byte[] vals = BitConverter.GetBytes(val);
              for (int i = 0; i < vals.Length; i++)
                mem[pos + i] = vals[i];
            }
            else if (r.type == VT.String) {
              byte[] vals = System.Text.Encoding.UTF8.GetBytes(r.ToString());
              for (int i = 0; i < vals.Length; i++) {
                mem[pos + i + 2] = vals[i];
              }
              mem[pos] = (byte)(vals.Length >> 8);
              mem[pos + 1] = (byte)(vals.Length & 0xFF);
            }
          }
          else if (n.First.type == BNF.MEMlongb) {
            int pos = Evaluate(n.First.First).ToInt();
            if (pos < 0 || pos > mem.Length) throw new Exception("Memory violation:" + pos + "from:" + n);
            switch (n.type) {
              case BNF.ASSIGN: mem[pos] = r.ToByte(); break;
              case BNF.ASSIGNsum: mem[pos] += r.ToByte(); break;
              case BNF.ASSIGNsub: mem[pos] -= r.ToByte(); break;
              case BNF.ASSIGNmul: mem[pos] *= r.ToByte(); break;
              case BNF.ASSIGNdiv: mem[pos] /= r.ToByte(); break;
              case BNF.ASSIGNmod: mem[pos] %= r.ToByte(); break;
              case BNF.ASSIGNand: mem[pos] &= r.ToByte(); break;
              case BNF.ASSIGNor: mem[pos] |= r.ToByte(); break;
              case BNF.ASSIGNxor: mem[pos] ^= r.ToByte(); break;
            }
          }
          else if (n.First.type == BNF.MEMlongi) {
            int pos = Evaluate(n.First.First).ToInt();
            if (pos < 0 || pos > mem.Length) throw new Exception("Memory violation:" + pos + "from:" + n);
            int val = BitConverter.ToInt32(mem, pos);
            switch (n.type) {
              case BNF.ASSIGN: val = r.ToInt(); break;
              case BNF.ASSIGNsum: val += r.ToInt(); break;
              case BNF.ASSIGNsub: val -= r.ToInt(); break;
              case BNF.ASSIGNmul: val *= r.ToInt(); break;
              case BNF.ASSIGNdiv: val /= r.ToInt(); break;
              case BNF.ASSIGNmod: val %= r.ToInt(); break;
              case BNF.ASSIGNand: val &= r.ToInt(); break;
              case BNF.ASSIGNor: val |= r.ToInt(); break;
              case BNF.ASSIGNxor: val ^= r.ToInt(); break;
            }
            byte[] vals = BitConverter.GetBytes(val);
            for (int i = 0; i < vals.Length; i++)
              mem[pos + i] = vals[i];
          }
          else if (n.First.type == BNF.MEMlongf) {
            int pos = Evaluate(n.First.First).ToInt();
            if (pos < 0 || pos > mem.Length) throw new Exception("Memory violation:" + pos + "from:" + n);
            float val = BitConverter.ToSingle(mem, pos);
            switch (n.type) {
              case BNF.ASSIGN: val = r.ToFloat(); break;
              case BNF.ASSIGNsum: val += r.ToFloat(); break;
              case BNF.ASSIGNsub: val -= r.ToFloat(); break;
              case BNF.ASSIGNmul: val *= r.ToFloat(); break;
              case BNF.ASSIGNdiv: val /= r.ToFloat(); break;
              case BNF.ASSIGNmod: val %= r.ToFloat(); break;
            }
            byte[] vals = BitConverter.GetBytes(val);
            for (int i = 0; i < vals.Length; i++)
              mem[pos + i] = vals[i];
          }
          else if (n.First.type == BNF.MEMlongs) {
            int pos = Evaluate(n.First.First).ToInt();
            if (pos < 0 || pos > mem.Length) throw new Exception("Memory violation:" + pos + "from:" + n);
            byte[] vals = System.Text.Encoding.UTF8.GetBytes(r.ToString());
            for (int i = 0; i < vals.Length; i++) {
              mem[pos + i + 2] = vals[i];
            }
            mem[pos] = (byte)(vals.Length >> 8);
            mem[pos + 1] = (byte)(vals.Length & 0xFF);
          }
        }
        break;

        case BNF.LINE: {
          Register x1 = Evaluate(n.children[0]);
          Register y1 = Evaluate(n.children[1]);
          Register x2 = Evaluate(n.children[2]);
          Register y2 = Evaluate(n.children[3]);
          Register col = Evaluate(n.children[4]);
          Line(x1.ToInt(), y1.ToInt(), x2.ToInt(), y2.ToInt(), col.ToByte());
        }
        break;

        case BNF.BOX: {
          Register x1 = Evaluate(n.children[0]);
          Register y1 = Evaluate(n.children[1]);
          Register x2 = Evaluate(n.children[2]);
          Register y2 = Evaluate(n.children[3]);
          Register col = Evaluate(n.children[4]);
          if (n.children.Count > 5) {
            Register back = Evaluate(n.children[5]);
            Box(x1.ToInt(), y1.ToInt(), x2.ToInt(), y2.ToInt(), col.ToByte(), back.ToByte());
          }
          else
            Box(x1.ToInt(), y1.ToInt(), x2.ToInt(), y2.ToInt(), col.ToByte());
        }
        break;

        case BNF.CIRCLE: {
          Register cx = Evaluate(n.children[0]);
          Register cy = Evaluate(n.children[1]);
          Register rx = Evaluate(n.children[2]);
          Register ry = Evaluate(n.children[3]);
          Register col = Evaluate(n.children[4]);
          if (n.children.Count > 5) {
            Register back = Evaluate(n.children[5]);
            Circle(cx.ToFloat(), cy.ToFloat(), rx.ToFloat(), ry.ToFloat(), col.ToByte(), back.ToByte());
          }
          else
            Circle(cx.ToFloat(), cy.ToFloat(), rx.ToFloat(), ry.ToFloat(), col.ToByte());
        }
        break;

        case BNF.IF: {
          Register cond = Evaluate(n.First);
          if (cond.ToInt() != 0) {
            Debug.Log("Executing IF");
            stacks.Add(new ExecStack { node = n.Second, step = 0 });
            return true;
          }
          else if (n.children.Count > 2) {
            Debug.Log("Executing ELSE");
            stacks.Add(new ExecStack { node = n.Third, step = 0 });
            return true;
          }
        }
        break;

        case BNF.WHILE: {
          Register cond = Evaluate(n.First);
          if (cond.ToInt() != 0) {
            Debug.Log("Executing IF");
            stacks.Add(new ExecStack { node = n.Second, cond = n.First, step = 0 });
            return true;
          }
        }
        break;

        default: {
          Clear(0b010000);
          Write("Not handled code:\n " + n.type + "\n" + n, 2, 2, 0b111100);
          updateDelay = -1;
          pc = int.MaxValue;
          texture.Apply();
        }
        break;
      }
    } catch (Exception e) {
      Clear(0b110000);
      Write(e.Message, 2, 2, 0);
      updateDelay = -1;
      pc = int.MaxValue;
      texture.Apply();
    }
    return false;
  }

  private Register Evaluate(CodeNode n) {
    if (!n.Evaluable()) throw new Exception("Not evaluable node: " + n);

    switch (n.type) {
      case BNF.REG: return regs[(int)n.Reg - 97];

      case BNF.MEM:
      case BNF.MEMlongb: {
        int pos = Evaluate(n.First).ToInt();
        if (pos < 0 || pos > mem.Length) throw new Exception("Memory violation:" + pos + "from:" + n);
        return new Register((int)mem[pos]);
      }

      case BNF.MEMlong:
      case BNF.MEMlongi: {
        int pos = Evaluate(n.First).ToInt();
        return new Register(BitConverter.ToInt32(mem, pos));
      }

      case BNF.MEMlongf: {
        int pos = Evaluate(n.First).ToInt();
        return new Register(BitConverter.ToSingle(mem, pos));
      }

      case BNF.MEMlongs: {
        int pos = Evaluate(n.First).ToInt();
        int len = (mem[pos] << 80) + mem[pos + 1];
        return new Register(System.Text.Encoding.UTF8.GetString(mem, pos+2, len));
      }

      case BNF.INT: return new Register(n.iVal);
      case BNF.FLT: return new Register(n.fVal);
      case BNF.COL: return new Register(n.iVal);
      case BNF.HEX: return new Register(n.iVal);
      case BNF.STR: return new Register(n.sVal);
      case BNF.STRcnst: return new Register(n.sVal);

      case BNF.Inc: {
        CodeNode exp = n.First;
        Register r = Evaluate(exp);
        r.Incr();
        return r;
      }
      case BNF.Dec: {
        CodeNode exp = n.First;
        Register r = Evaluate(exp);
        r.Decr();
        return r;
      }

      case BNF.DTIME: return new Register(Time.deltaTime);
      case BNF.OPpar: return Evaluate(n.First);
      case BNF.OPsum: return (new Register(Evaluate(n.First))).Sum(Evaluate(n.Second));
      case BNF.OPsub: return (new Register(Evaluate(n.First))).Sub(Evaluate(n.Second));
      case BNF.OPmul: return (new Register(Evaluate(n.First))).Mul(Evaluate(n.Second));
      case BNF.OPdiv: return (new Register(Evaluate(n.First))).Div(Evaluate(n.Second));
      case BNF.OPmod: return (new Register(Evaluate(n.First))).Mod(Evaluate(n.Second));
      case BNF.OPand: return (new Register(Evaluate(n.First))).And(Evaluate(n.Second));
      case BNF.OPor: return (new Register(Evaluate(n.First))).Or(Evaluate(n.Second));
      case BNF.OPxor: return (new Register(Evaluate(n.First))).Xor(Evaluate(n.Second));

      case BNF.LEN: 
        return new Register(Evaluate(n.First).ToString().Length);

      case BNF.PLEN: 
        return new Register(System.Text.Encoding.UTF8.GetByteCount(Evaluate(n.First).ToString()) + 2);

      case BNF.UOsub: return Evaluate(n.First).Sub();
      case BNF.UOinv: return Evaluate(n.First).Inv();
      case BNF.UOneg: return Evaluate(n.First).Neg();

      case BNF.COMPeq:
      case BNF.COMPne:
      case BNF.COMPgt:
      case BNF.COMPge:
      case BNF.COMPlt:
      case BNF.COMPle:
        return new Register(Evaluate(n.First).Compare(Evaluate(n.Second), n.type));

      case BNF.CASTb: return new Register(Evaluate(n.First).ToByte());
      case BNF.CASTi: return new Register(Evaluate(n.First).ToInt());
      case BNF.CASTf: return new Register(Evaluate(n.First).ToFloat());
      case BNF.CASTs: return new Register(Evaluate(n.First).ToString());

      case BNF.EXP:
        throw new Exception("Not yet implemented: " + n.type);
    }
    throw new Exception("Invalid node to evaluate: " + n.type);
  }

  readonly Dictionary<char, byte[]> font = new Dictionary<char, byte[]>{
    {'\t', new byte[]{
      0b11111110,
      0b01000000,
      0b00100000,
      0b00010000,
      0b00001000,
      0b00000100,
      0b00000010,
      0b00000111,
    } },

{'@', new byte[]{0x3C,0x66,0x6E,0x6E,0x60,0x62,0x3C,0x00} },
{'A', new byte[]{0x18,0x3C,0x66,0x7E,0x66,0x66,0x66,0x00} },
{'B', new byte[]{0x7C,0x66,0x66,0x7C,0x66,0x66,0x7C,0x00} },
{'C', new byte[]{0x3C,0x66,0x60,0x60,0x60,0x66,0x3C,0x00} },
{'D', new byte[]{0x78,0x6C,0x66,0x66,0x66,0x6C,0x78,0x00} },
{'E', new byte[]{0x7E,0x60,0x60,0x78,0x60,0x60,0x7E,0x00} },
{'F', new byte[]{0x7E,0x60,0x60,0x78,0x60,0x60,0x60,0x00} },
{'G', new byte[]{0x3C,0x66,0x60,0x6E,0x66,0x66,0x3C,0x00} },
{'H', new byte[]{0x66,0x66,0x66,0x7E,0x66,0x66,0x66,0x00} },
{'I', new byte[]{0x3C,0x18,0x18,0x18,0x18,0x18,0x3C,0x00} },
{'J', new byte[]{0x1E,0x0C,0x0C,0x0C,0x0C,0x6C,0x38,0x00} },
{'K', new byte[]{0x66,0x6C,0x78,0x70,0x78,0x6C,0x66,0x00} },
{'L', new byte[]{0x60,0x60,0x60,0x60,0x60,0x60,0x7E,0x00} },
{'M', new byte[]{0x63,0x77,0x7F,0x6B,0x63,0x63,0x63,0x00} },
{'N', new byte[]{0x66,0x76,0x7E,0x7E,0x6E,0x66,0x66,0x00} },
{'O', new byte[]{0x3C,0x66,0x66,0x66,0x66,0x66,0x3C,0x00} },
{'P', new byte[]{0x7C,0x66,0x66,0x7C,0x60,0x60,0x60,0x00} },
{'Q', new byte[]{0x3C,0x66,0x66,0x66,0x66,0x3C,0x0E,0x00} },
{'R', new byte[]{0x7C,0x66,0x66,0x7C,0x78,0x6C,0x66,0x00} },
{'S', new byte[]{0x3C,0x66,0x60,0x3C,0x06,0x66,0x3C,0x00} },
{'T', new byte[]{0x7E,0x18,0x18,0x18,0x18,0x18,0x18,0x00} },
{'U', new byte[]{0x66,0x66,0x66,0x66,0x66,0x66,0x3C,0x00} },
{'V', new byte[]{0x66,0x66,0x66,0x66,0x66,0x3C,0x18,0x00} },
{'W', new byte[]{0x63,0x63,0x63,0x6B,0x7F,0x77,0x63,0x00} },
{'X', new byte[]{0x66,0x66,0x3C,0x18,0x3C,0x66,0x66,0x00} },
{'Y', new byte[]{0x66,0x66,0x66,0x3C,0x18,0x18,0x18,0x00} },
{'Z', new byte[]{0x7E,0x06,0x0C,0x18,0x30,0x60,0x7E,0x00} },
{'[', new byte[]{0x3C,0x30,0x30,0x30,0x30,0x30,0x3C,0x00} },
{']', new byte[]{0x3C,0x0C,0x0C,0x0C,0x0C,0x0C,0x3C,0x00} },
{' ', new byte[]{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00} },
{'!', new byte[]{0x18,0x18,0x18,0x18,0x00,0x00,0x18,0x00} },
{'"', new byte[]{0x66,0x66,0x66,0x00,0x00,0x00,0x00,0x00} },
{'#', new byte[]{0x66,0x66,0xFF,0x66,0xFF,0x66,0x66,0x00} },
{'$', new byte[]{0x18,0x3E,0x60,0x3C,0x06,0x7C,0x18,0x00} },
{'%', new byte[]{0x62,0x66,0x0C,0x18,0x30,0x66,0x46,0x00} },
{'&', new byte[]{0x3C,0x66,0x3C,0x38,0x67,0x66,0x3F,0x00} },
{'\'', new byte[]{0x06,0x0C,0x18,0x00,0x00,0x00,0x00,0x00} },
{'(', new byte[]{0x0C,0x18,0x30,0x30,0x30,0x18,0x0C,0x00} },
{')', new byte[]{0x30,0x18,0x0C,0x0C,0x0C,0x18,0x30,0x00} },
{'*', new byte[]{0x00,0x66,0x3C,0xFF,0x3C,0x66,0x00,0x00} },
{'+', new byte[]{0x00,0x18,0x18,0x7E,0x18,0x18,0x00,0x00} },
{',', new byte[]{0x00,0x00,0x00,0x00,0x00,0x18,0x18,0x30} },
{'-', new byte[]{0x00,0x00,0x00,0x7E,0x00,0x00,0x00,0x00} },
{'.', new byte[]{0x00,0x00,0x00,0x00,0x00,0x18,0x18,0x00} },
{'/', new byte[]{0x00,0x03,0x06,0x0C,0x18,0x30,0x60,0x00} },
{'0', new byte[]{0x3C,0x66,0x6E,0x76,0x66,0x66,0x3C,0x00} },
{'1', new byte[]{0x18,0x18,0x38,0x18,0x18,0x18,0x7E,0x00} },
{'2', new byte[]{0x3C,0x66,0x06,0x0C,0x30,0x60,0x7E,0x00} },
{'3', new byte[]{0x3C,0x66,0x06,0x1C,0x06,0x66,0x3C,0x00} },
{'4', new byte[]{0x06,0x0E,0x1E,0x66,0x7F,0x06,0x06,0x00} },
{'5', new byte[]{0x7E,0x60,0x7C,0x06,0x06,0x66,0x3C,0x00} },
{'6', new byte[]{0x3C,0x66,0x60,0x7C,0x66,0x66,0x3C,0x00} },
{'7', new byte[]{0x7E,0x66,0x0C,0x18,0x18,0x18,0x18,0x00} },
{'8', new byte[]{0x3C,0x66,0x66,0x3C,0x66,0x66,0x3C,0x00} },
{'9', new byte[]{0x3C,0x66,0x66,0x3E,0x06,0x66,0x3C,0x00} },
{':', new byte[]{0x00,0x00,0x18,0x00,0x00,0x18,0x00,0x00} },
{';', new byte[]{0x00,0x00,0x18,0x00,0x00,0x18,0x18,0x30} },
{'<', new byte[]{0x0E,0x18,0x30,0x60,0x30,0x18,0x0E,0x00} },
{'=', new byte[]{0x00,0x00,0x7E,0x00,0x7E,0x00,0x00,0x00} },
{'>', new byte[]{0x70,0x18,0x0C,0x06,0x0C,0x18,0x70,0x00} },
{'?', new byte[]{0x3C,0x66,0x06,0x0C,0x18,0x00,0x18,0x00} },
{'a', new byte[]{0x00,0x00,0x3C,0x06,0x3E,0x66,0x3E,0x00} },
{'b', new byte[]{0x00,0x60,0x60,0x7C,0x66,0x66,0x7C,0x00} },
{'c', new byte[]{0x00,0x00,0x3C,0x60,0x60,0x60,0x3C,0x00} },
{'d', new byte[]{0x00,0x06,0x06,0x3E,0x66,0x66,0x3E,0x00} },
{'e', new byte[]{0x00,0x00,0x3C,0x66,0x7E,0x60,0x3C,0x00} },
{'f', new byte[]{0x00,0x0E,0x18,0x3E,0x18,0x18,0x18,0x00} },
{'g', new byte[]{0x00,0x00,0x3E,0x66,0x66,0x3E,0x06,0x7C} },
{'h', new byte[]{0x00,0x60,0x60,0x7C,0x66,0x66,0x66,0x00} },
{'i', new byte[]{0x00,0x18,0x00,0x38,0x18,0x18,0x3C,0x00} },
{'j', new byte[]{0x00,0x06,0x00,0x06,0x06,0x06,0x06,0x3C} },
{'k', new byte[]{0x00,0x60,0x60,0x6C,0x78,0x6C,0x66,0x00} },
{'l', new byte[]{0x00,0x38,0x18,0x18,0x18,0x18,0x3C,0x00} },
{'m', new byte[]{0x00,0x00,0x66,0x7F,0x7F,0x6B,0x63,0x00} },
{'n', new byte[]{0x00,0x00,0x7C,0x66,0x66,0x66,0x66,0x00} },
{'o', new byte[]{0x00,0x00,0x3C,0x66,0x66,0x66,0x3C,0x00} },
{'p', new byte[]{0x00,0x00,0x7C,0x66,0x66,0x7C,0x60,0x60} },
{'q', new byte[]{0x00,0x00,0x3E,0x66,0x66,0x3E,0x06,0x06} },
{'r', new byte[]{0x00,0x00,0x7C,0x66,0x60,0x60,0x60,0x00} },
{'s', new byte[]{0x00,0x00,0x3E,0x60,0x3C,0x06,0x7C,0x00} },
{'t', new byte[]{0x00,0x18,0x7E,0x18,0x18,0x18,0x0E,0x00} },
{'u', new byte[]{0x00,0x00,0x66,0x66,0x66,0x66,0x3E,0x00} },
{'v', new byte[]{0x00,0x00,0x66,0x66,0x66,0x3C,0x18,0x00} },
{'w', new byte[]{0x00,0x00,0x63,0x6B,0x7F,0x3E,0x36,0x00} },
{'x', new byte[]{0x00,0x00,0x66,0x3C,0x18,0x3C,0x66,0x00} },
{'y', new byte[]{0x00,0x00,0x66,0x66,0x66,0x3E,0x0C,0x78} },
{'z', new byte[]{0x00,0x00,0x7E,0x0C,0x18,0x30,0x7E,0x00} },
{'{', new byte[]{0x3C,0x30,0x30,0x30,0x30,0x30,0x3C,0x00} },

  };
}

public class ExecStack {
  public CodeNode node;
  public CodeNode cond;
  public int step;
}

/*  TODO

  FOR

  Replace Lists with SList
  replace registers with variables
  .plen for strings to return the physiccal number of bytes they will need

  Implement shifts and rols
  Implement Labels
  inputX, inputY, inputF, inputEsc, inputA, inputB, inputC, inputD

--- To decide ----
  key("key") -> key is pressed
  key("key",1) -> key is down
  key("key",0) -> key is up
  keys -> U, D, L, R, A, B, C, D, Fire, Esc
------------------

  Add Screenmodes (w,h,number of tilemaps, filter)
  Add priority byte to sprites and tilemaps
  Add sprites
  Tiles
  Add "rom" and "ram" sizes on the "boot screen" (we have to calculate them)
  Add the 4 colors lines as logo in the home screen as default sprite (copy the logo of the C65)
  Sounds
  functions

  pure background color?
  border size on circles?
  FPS on topleft as TextMesh?

FUTURE: graph editor
FUTURE: step by step debugger
 */