using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ByteReader {
  public enum ReadMode { Dec, Hex, Bin };

  readonly static Regex rgHex0X = new Regex("0x([a-f0-9]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly static Regex rgHexX = new Regex("x([a-f0-9]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly static Regex rgHex = new Regex("([a-f0-9]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly static Regex rgDec = new Regex("([0-9]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly static Regex rgBin0B = new Regex("(0b[0-1]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly static Regex rgBinB = new Regex("(b[0-1]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly static Regex rgBin = new Regex("([0-1]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  public static string ReadBytes(string data, int len, out byte[] res, ReadMode mode = ReadMode.Hex, bool noFail = false) {
    res = new byte[len];
    int pos = 0;

    while (data.Length > 0) {
      data = data.Trim(' ', '\n', '\r', '\t');
      // get the values until a whitespace
      string part = "";
      foreach (char c in data) {
        if (c == ' ' || c == '\n' || c == '\r' || c == '\t') break;
        part += c;
      }
      data = data.Substring(part.Length);
      part = part.ToLowerInvariant();
      if (part.Equals("usehex")) { mode = ReadMode.Hex; continue; }
      if (part.Equals("usedec")) { mode = ReadMode.Dec; continue; }
      if (part.Equals("usebin")) { mode = ReadMode.Bin; continue; }

      if (mode == ReadMode.Hex) {
        // is it HEX?
        Match h0x = rgHex0X.Match(part);
        Match hx = rgHexX.Match(part);
        Match h = rgHex.Match(part);
        if (h0x.Success) {
          string hex = h0x.Value;
          while (hex.Length > 0) {
            string val = (hex + "0").Substring(0, 2);
            res[pos++] = (byte)Convert.ToInt32(val, 16);
            hex = hex.Substring(2);
            if (pos == len) {
              return hex + data;
            }
          }
        }
        else if (hx.Success) {
          string hex = hx.Value;
          while (hex.Length > 0) {
            string val = (hex + "0").Substring(0, 2);
            res[pos++] = (byte)Convert.ToInt32(val, 16);
            hex = hex.Substring(2);
            if (pos == len) {
              return hex + data;
            }
          }
        }
        else if (h.Success) {
          string hex = h.Value;
          while (hex.Length > 0) {
            string val = (hex + "0").Substring(0, 2);
            res[pos++] = (byte)Convert.ToInt32(val, 16);
            hex = hex.Substring(2);
            if (pos == len) {
              return hex + data;
            }
          }
        }
      }
      else if (mode == ReadMode.Bin) {
        // is it Bin?
        Match b0b = rgBin0B.Match(part);
        Match bb = rgBinB.Match(part);
        Match b = rgBin.Match(part);
        if (b0b.Success) {
          string bin = b0b.Value;
          res[pos++] = (byte)Convert.ToInt32(bin, 2);
          if (pos == len) {
            return data;
          }
        }
        else if (bb.Success) {
          string bin = bb.Value;
          res[pos++] = (byte)Convert.ToInt32(bin, 2);
          if (pos == len) {
            return data;
          }
        }
        else if (b.Success) {
          string bin = b.Value;
          res[pos++] = (byte)Convert.ToInt32(bin, 2);
          if (pos == len) {
            return data;
          }
        }
      }
      else {
        Match d = rgDec.Match(part);
        if (d.Success) {
          string dec = d.Value;
          res[pos++] = (byte)Convert.ToInt32(dec, 10);
          if (pos == len) {
            return data;
          }
        }
      }
    }
    if (!noFail) throw new Exception("Not enough data to parse. Expected " + len + " bytes");
    return "";
  }

  public static string ReadByte(string data, out byte res, ReadMode mode = ReadMode.Hex) {
    while (data.Length > 0) {
      data = data.Trim(' ', '\n', '\r', '\t');
      // get the values until a whitespace
      string part = "";
      foreach (char c in data) {
        if (c == ' ' || c == '\n' || c == '\r' || c == '\t') break;
        part += c;
      }
      data = data.Substring(part.Length);
      part = part.ToLowerInvariant();
      if (part.Equals("usehex")) { mode = ReadMode.Hex; continue; }
      if (part.Equals("usedec")) { mode = ReadMode.Dec; continue; }
      if (part.Equals("usebin")) { mode = ReadMode.Bin; continue; }

      if (mode == ReadMode.Hex) {
        // is it HEX?
        Match h0x = rgHex0X.Match(part);
        Match hx = rgHexX.Match(part);
        Match h = rgHex.Match(part);
        if (h0x.Success) {
          string hex = h0x.Value;
          while (hex.Length > 0) {
            string val = (hex + "0").Substring(0, 2);
            res = (byte)Convert.ToInt32(val, 16);
            hex = hex.Substring(2);
            return hex + data;
          }
        }
        else if (hx.Success) {
          string hex = hx.Value;
          while (hex.Length > 0) {
            string val = (hex + "0").Substring(0, 2);
            res = (byte)Convert.ToInt32(val, 16);
            hex = hex.Substring(2);
            return hex + data;
          }
        }
        else if (h.Success) {
          string hex = h.Value;
          while (hex.Length > 0) {
            string val = (hex + "0").Substring(0, 2);
            res = (byte)Convert.ToInt32(val, 16);
            hex = hex.Substring(2);
            return hex + data;
          }
        }
      }
      else if (mode == ReadMode.Bin) {
        // is it Bin?
        Match b0b = rgBin0B.Match(part);
        Match bb = rgBinB.Match(part);
        Match b = rgBin.Match(part);
        if (b0b.Success) {
          string bin = b0b.Value;
          res = (byte)Convert.ToInt32(bin, 2);
          return data;
        }
        else if (bb.Success) {
          string bin = bb.Value;
          res = (byte)Convert.ToInt32(bin, 2);
          return data;
        }
        else if (b.Success) {
          string bin = b.Value;
          res = (byte)Convert.ToInt32(bin, 2);
          return data;
        }
      }
      else {
        Match d = rgDec.Match(part);
        if (d.Success) {
          string dec = d.Value;
          res = (byte)Convert.ToInt32(dec, 10);
          return data;
        }
      }
    }
    throw new Exception("No data to parse.");
  }


  public static string ReadNextByte(string data, out byte res) {
    int pos1 = data.IndexOf(' ');
    int pos2 = data.IndexOf('\n');
    int pos3 = data.Length;
    if (pos1 == -1) pos1 = int.MaxValue;
    if (pos2 == -1) pos2 = int.MaxValue;
    if (pos3 == -1) pos3 = int.MaxValue;
    int pos = pos1;
    if (pos > pos2) pos = pos2;
    if (pos > pos3) pos = pos3;
    if (pos < 1) {
      res = 0;
      return "";
    }

    string part = data.Substring(0, pos);
    Match m = rgHex0X.Match(part);
    if (m.Success) {
      res = (byte)Convert.ToInt32(m.Groups[1].Value, 16);
      return data.Substring(pos).Trim();
    }

    res = 0;
    return data;
  }

  readonly static Regex rgComments = new Regex("/\\*(?>(?:(?>[^*]+)|\\*(?!/))*)\\*/", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly static Regex rgComment = new Regex("//(.*?)\r?\n", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  internal static void ReadBlock(string data, out List<CodeLabel> labels, out byte[] block) {
    ReadMode mode = ReadMode.Dec;
    data = rgComments.Replace(data, "");
    data = rgComment.Replace(data, "");
    data = data.Trim().Replace('\r', ' ').Replace('\n', ' ');
    while (data.IndexOf("  ") != -1) data = data.Replace("  ", " ");

    int datasize = 0;
    labels = new List<CodeLabel>();
    block = new byte[0];
    Consolidator consolidator = new Consolidator();
    while (data.Length > 0) {
      // Get the part, it is whatever we got until a whitespace
      string part = "";
      foreach (char c in data) {
        if (c == ' ') break;
        part += c;
      }
      part = part.ToLowerInvariant();
      // What we have?
      if (part[part.Length - 1] == ':') { // Label
        labels.Add(new CodeLabel { name = part, start = datasize, len = 0 });
        data = data.Substring(part.Length).Trim();
        continue;
      }
      else if (part.Equals("}")) {
        // Consolidate all parts we found
        block = consolidator.Consolidate();
        return;
      }
      else if (part.Equals("usehex")) {
        mode = ReadMode.Hex;
        data = data.Substring(part.Length).Trim();
        continue;
      }
      else if (part.Equals("usebin")) {
        mode = ReadMode.Bin;
        data = data.Substring(part.Length).Trim();
        continue;
      }
      else if (part.Equals("usedec")) {
        mode = ReadMode.Dec;
        data = data.Substring(part.Length).Trim();
        continue;
      }
      // Data

      data = data.Substring(part.Length).Trim();
      if ((part.Length > 2 && part[0] == '0' && part[1] == 'x') || (part.Length > 1 && part[0] == 'x')) { // Do we start with 0x?
        if (part[0] == 'x')
          part = part.Substring(1);
        else
          part = part.Substring(2);
        // Get 2 chars as a byte in hex, and continue till the end of the string
        part = part.Substring(2);
        while (part.Length > 0) {
          string val = part[0] + (part.Length > 1 ? part[1].ToString() : " ");
          consolidator.AddByte((byte)Convert.ToInt32(val, 16));
        }
      }
      else if ((part.Length > 2 && part[0] == '0' && part[1] == 'b') || (part.Length > 1 && part[0] == 'b')) { // Do we start with 0b?
        if (part[0] == 'b')
          part = part.Substring(1);
        else
          part = part.Substring(2);
        // Parse all value, then split in bytes
        part = part.Substring(2);
        int b = Convert.ToInt32(part, 2);
        if (b < 256) consolidator.AddByte((byte)b);
        else if (b < 65536) {
          byte l = (byte)(b & 0xff);
          byte h = (byte)((b & 0xff00) >> 8);
          consolidator.AddByte(h);
          consolidator.AddByte(l);
        }
        else {
          byte b0 = (byte)(b & 0xff);
          byte b1 = (byte)((b & 0xff00) >> 8);
          byte b2 = (byte)((b & 0xff0000) >> 16);
          byte b3 = (byte)((b & 0xff000000) >> 24);
          consolidator.AddByte(b3);
          consolidator.AddByte(b2);
          consolidator.AddByte(b1);
          consolidator.AddByte(b0);
        }
      }

      if (mode == ReadMode.Dec) { // Parse it as dec, split in bytes
        int b = Convert.ToInt32(part, 10);
        if (b < 256) consolidator.AddByte((byte)b);
        else if (b < 65536) {
          byte l = (byte)(b & 0xff);
          byte h = (byte)((b & 0xff00) >> 8);
          consolidator.AddByte(h);
          consolidator.AddByte(l);
        }
        else {
          byte b0 = (byte)(b & 0xff);
          byte b1 = (byte)((b & 0xff00) >> 8);
          byte b2 = (byte)((b & 0xff0000) >> 16);
          byte b3 = (byte)((b & 0xff000000) >> 24);
          consolidator.AddByte(b3);
          consolidator.AddByte(b2);
          consolidator.AddByte(b1);
          consolidator.AddByte(b0);
        }
      }
      else if (mode == ReadMode.Hex) { // Parse it as hex
        while (part.Length > 0) {
          string val = part[0] + (part.Length > 1 ? part[1].ToString() : " ");
          consolidator.AddByte((byte)Convert.ToInt32(val, 16));
          if (part.Length < 3) break;
          part = part.Substring(2);
        }
      }
      else if (mode == ReadMode.Bin) { // Parse it as bin, split in bytes
        int b = Convert.ToInt32(part, 2);
        if (b < 256) consolidator.AddByte((byte)b);
        else if (b < 65536) {
          byte l = (byte)(b & 0xff);
          byte h = (byte)((b & 0xff00) >> 8);
          consolidator.AddByte(h);
          consolidator.AddByte(l);
        }
        else {
          byte b0 = (byte)(b & 0xff);
          byte b1 = (byte)((b & 0xff00) >> 8);
          byte b2 = (byte)((b & 0xff0000) >> 16);
          byte b3 = (byte)((b & 0xff000000) >> 24);
          consolidator.AddByte(b3);
          consolidator.AddByte(b2);
          consolidator.AddByte(b1);
          consolidator.AddByte(b0);
        }
      }
    }
    // Consolidate all parts we found
    block = consolidator.Consolidate();
  }
}

public class Consolidator {
  List<byte[]> parts = new List<byte[]>();
  int part = 0;
  int pos = 0;

  internal void AddByte(byte b) {
    if (parts.Count - 1 < part) {
      parts.Add(new byte[4096]);
    }
    if (pos == 4096) {
      parts.Add(new byte[4096]);
      part++;
      pos = 0;
    }
    byte[] block = parts[part];
    block[pos++] = b;
  }

  internal byte[] Consolidate() {
    if (parts.Count == 0) return new byte[0];
    int len = (parts.Count - 1) * 4096 + pos;
    byte[] res = new byte[len];
    len = 0;
    for (int i = 0; i < parts.Count - 1; i++) {
      byte[] block = parts[i];
      foreach (byte b in block) res[len++] = b;
    }
    byte[] last = parts[parts.Count - 1];
    for (int i = 0; i < pos; i++) res[len++] = last[i];
    parts.Clear();
    return res;
  }
}