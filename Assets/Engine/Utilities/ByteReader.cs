using System;
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

  public static string ReadBytes(string data, int len, out byte[] res, ReadMode mode = ReadMode.Hex) {
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
    throw new Exception("Not enough data to parse. Expected " + len + " bytes");
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


}
