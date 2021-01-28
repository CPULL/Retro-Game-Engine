﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ByteReader {
  public enum ReadMode { Dec, Hex, Bin };

  readonly static Regex rgComments = new Regex("/\\*(?>(?:(?>[^*]+)|\\*(?!/))*)\\*/", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly static Regex rgComment = new Regex("//(.*?)\r?\n", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  internal static void ReadBlock(string data, out List<CodeLabel> labels, out byte[] block) {
    ReadMode mode = ReadMode.Dec;
    data = rgComments.Replace(data, "");
    data = rgComment.Replace(data, "");
    data = data.Trim().Replace('\r', ' ').Replace('\n', ' ');
    while (data.IndexOf("  ") != -1) data = data.Replace("  ", " ");

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
        labels.Add(new CodeLabel { name = part, start = consolidator.GetPos() });
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
        while (part.Length > 0) {
          string val = part[0] + (part.Length > 1 ? part[1].ToString() : " ");
          try {
            consolidator.AddByte((byte)Convert.ToInt32(val, 16));
          } catch(Exception) {
            throw new Exception("Cannot parse \"" + val + "\" as Hex");
          }
        }
      }
      else if ((part.Length > 2 && part[0] == '0' && part[1] == 'b') || (part.Length > 1 && part[0] == 'b')) { // Do we start with 0b?
        if (part[0] == 'b')
          part = part.Substring(1);
        else
          part = part.Substring(2);
        // Parse all value, then split in bytes
        int b;
        try {
          b = Convert.ToInt32(part, 2);
        } catch (Exception) {
          throw new Exception("Cannot parse \"" + part + "\" as Binary");
        }
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
        int b;
        try {
          b = Convert.ToInt32(part, 10);
        } catch (Exception) {
          throw new Exception("Cannot parse \"" + part + "\" as Decimal");
        }
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
          try {
            consolidator.AddByte((byte)Convert.ToInt32(val, 16));
          } catch (Exception) {
            throw new Exception("Cannot parse \"" + val + "\" as Hex");
          }
          if (part.Length < 3) break;
          part = part.Substring(2);
        }
      }
      else if (mode == ReadMode.Bin) { // Parse it as bin, split in bytes
        int b;
        try {
          b = Convert.ToInt32(part, 2);
        } catch (Exception) {
          throw new Exception("Cannot parse \"" + part + "\" as Binary");
        }
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
  readonly List<byte[]> parts = new List<byte[]>();
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

  internal int GetPos() {
    if (parts.Count == 0) return 0;
    return (parts.Count - 1) * 4096 + pos;
  }
}