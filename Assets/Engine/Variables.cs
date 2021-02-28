using System;
using System.Collections.Generic;

public class Variables {
  private Value[] vars = new Value[32];
  private int count = 0;
  private readonly Dictionary<string, int> pointers = new Dictionary<string, int>();

  public int Add(string name) {
    name = name.ToLowerInvariant();
    if (pointers.ContainsKey(name)) return pointers[name];

    if (count == vars.Length) {
      Value[] vars2 = new Value[vars.Length + 32];
      for (int i = 0; i < vars.Length; i++)
        vars2[i] = vars[i];
      vars = vars2;
    }

    vars[count] = new Value(MD.Reg) { idx = count };
    pointers[name] = count;
    count++;
    return count - 1;
  }

  internal Value Get(int reg) {
    if (reg >= pointers.Count) return new Value();
    return vars[reg];
  }

  internal bool Invalid(int reg) {
    return reg < 0 || reg >= pointers.Count;
  }

  internal string GetRegName(int reg) {
    foreach (string name in pointers.Keys)
      if (pointers[name] == reg) return name;
    return "<unknown>";
  }

  internal void Set(int reg, int v) {
    vars[reg].type = VT.Int;
    vars[reg].iVal = v;
  }
  internal void Set(int reg, float v) {
    vars[reg].type = VT.Float;
    vars[reg].fVal = v;
  }
  internal void Set(int reg, string v) {
    vars[reg].type = VT.String;
    vars[reg].sVal = v;
  }
  internal void Set(int reg, Value v) {
    vars[reg].type = v.type;
    vars[reg].iVal = v.iVal;
    vars[reg].fVal = v.fVal;
    vars[reg].sVal = v.sVal;
  }

  internal void Set(int reg) {
    vars[reg].type = VT.None;
  }

  internal void Set(int reg, int[] v) {
    vars[reg].type = VT.Array;
    vars[reg].aVals = v;
  }


  internal void SetArray(int reg, int idx, Value v) {
    Value r = vars[reg];
    if (r.aVals == null || idx < 0 || idx >= r.aVals.Length) {
      foreach (string name in pointers.Keys)
        if (pointers[name] == reg)
          throw new System.Exception("Invalid array assignement: variable " + name);
      throw new System.Exception("Invalid array assignement: <unknown variable>");
    }
    int pos = r.aVals[idx];
    vars[pos].type = v.type;
    vars[pos].iVal = v.iVal;
    vars[pos].fVal = v.fVal;
    vars[pos].sVal = v.sVal;
  }

  internal void Incr(int idx) {
    if (vars[idx].type == VT.None) { vars[idx].iVal = 0; vars[idx].type = VT.Int; }
    else if (vars[idx].type == VT.Int) vars[idx].iVal++;
    else if (vars[idx].type == VT.Float) vars[idx].fVal += 1;
  }
  internal void Decr(int idx) {
    if (vars[idx].type == VT.None) { vars[idx].iVal = 0; vars[idx].type = VT.Int; }
    else if (vars[idx].type == VT.Int) vars[idx].iVal--;
    else if (vars[idx].type == VT.Float) vars[idx].fVal -= 1;
  }

  public override string ToString() {
    string res = "";
    foreach(string key in pointers.Keys) {
      res += pointers[key] + ") " + key + " = " + vars[pointers[key]] + "\n";
    }
    return res;
  }

  public void Clear() {
    vars = new Value[32];
    count = 0;
    pointers.Clear();
  }


  public string GetFormattedValues(out int num) {
    string res = "";

    foreach (string key in pointers.Keys) {
      Value val = vars[pointers[key]];
      string line = "";
      switch (val.type) {
        case VT.None:   line = "<color=#20f350>NUL</color> "; break;
        case VT.Int:    line = "<color=#20f350>INT</color> "; break;
        case VT.Float:  line = "<color=#20f350>FLT</color> "; break;
        case VT.String: line = "<color=#20f350>STR</color> "; break;
        case VT.Array:  line = "<color=#20f350>ARR</color> "; break;
      }
      string name = key;
      while (name.Length < 16) name += " ";
      if (name.Length > 16) name = name.Substring(0, 16);
      line += name + "  <color=#0fe4f3>" + val.ToDebugStr() + "</color>\n";
      res += line;
    }
    num = pointers.Count;
    return res;
  }

  public void CopyValuesFrom(Variables src) {
    Clear();
    foreach(string name in src.pointers.Keys) {
      int reg = src.GetRegByName(name);
      if (reg == -1) continue;
      vars[Add(name)].Assign(src.Get(reg));
    }

    foreach (string name in src.pointers.Keys) {
      if (vars[pointers[name]].type != VT.Array) continue;
      int reg = src.GetRegByName(name);
      if (reg == -1) continue;
      Value v = src.Get(reg);
      for (int i = 0; i < vars[pointers[name]].aVals.Length; i++) {
        string sr = src.GetRegName(v.aVals[i]);
        if (!pointers.ContainsKey(sr)) continue;
        vars[pointers[name]].aVals[i] = GetRegByName(sr);
      }
    }
  }

  private int GetRegByName(string name) {
    if (!pointers.ContainsKey(name)) return -1;
    return pointers[name];
  }
}



public struct Value {
  const System.Globalization.NumberStyles numberstyle = System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;

  public MD mode;
  public int idx; // Used for reg and mem
  public VT type;
  public int iVal;
  public float fVal;
  public string sVal;
  public int[] aVals;

  public Value(MD m) {
    mode = m;
    idx = 0;
    type = VT.None;
    iVal = 0;
    fVal = 0;
    sVal = null;
    aVals = null;
  }
  public Value(int i) {
    mode = MD.Dir;
    idx = 0;
    type = VT.Int;
    iVal = i;
    fVal = 0;
    sVal = null;
    aVals = null;
  }
  public Value(float f) {
    mode = MD.Dir;
    idx = 0;
    type = VT.Float;
    iVal = 0;
    fVal = f;
    sVal = null;
    aVals = null;
  }
  public Value(string s) {
    mode = MD.Dir;
    idx = 0;
    type = VT.String;
    iVal = 0;
    fVal = 0;
    sVal = s;
    aVals = null;
  }

  public byte ToByte(System.Globalization.CultureInfo culture) {
    if (type == VT.None) return 0;
    if (type == VT.Int) return (byte)(iVal & 255);
    if (type == VT.Float) return (byte)((int)fVal & 255);
    if (string.IsNullOrEmpty(sVal)) return 0;
    if (float.TryParse(sVal, numberstyle, culture, out float f)) return (byte)((int)f & 255);
    if (int.TryParse(sVal, out int i)) return (byte)(i & 255);
    return 0;
  }
  public int ToInt(System.Globalization.CultureInfo culture) {
    if (type == VT.None) return 0;
    if (type == VT.Int) return iVal;
    if (type == VT.Float) return (int)fVal;
    if (string.IsNullOrEmpty(sVal)) return 0;
    if (float.TryParse(sVal, numberstyle, culture, out float f)) return (int)f;
    if (int.TryParse(sVal, out int i)) return i;
    return 0;
  }
  public float ToFlt(System.Globalization.CultureInfo culture) {
    if (type == VT.None) return 0;
    if (type == VT.Int) return iVal;
    if (type == VT.Float) return fVal;
    if (string.IsNullOrEmpty(sVal)) return 0;
    if (float.TryParse(sVal, numberstyle, culture, out float f)) return f;
    if (int.TryParse(sVal, out int i)) return i;
    return 0;
  }
  public string ToStr() {
    if (type == VT.None) return "";
    if (type == VT.Int) return iVal.ToString();
    if (type == VT.Float) return fVal.ToString("F3");
    if (string.IsNullOrEmpty(sVal)) return "";
    return sVal;
  }

  public bool ToBool(System.Globalization.CultureInfo culture) {
    return ToInt(culture) != 0;
  }

  public override string ToString() {
    string res = "[" + mode + "," + type + "]";
    if (type == VT.None) return res + "<none>";
    if (type == VT.Int) return res + iVal;
    if (type == VT.Float) return res + fVal.ToString("F3");
    if (type == VT.String) return res + (string.IsNullOrEmpty(sVal) ? "\"\"" : sVal);
    if (type == VT.Array) return res + "[..." + aVals?.Length + "...]";
    return "<unknown>";
  }

  internal bool IsReg() {
    return mode == MD.Reg;
  }
  internal bool IsMem() {
    return mode == MD.Mem;
  }

  internal Value Sum(Value s) {
    mode = MD.Dir;
    if (type == VT.None && s.type == VT.None) return this;
    if (type == VT.Int && s.type == VT.None) return this;
    if (type == VT.Float && s.type == VT.None) return this;
    if (type == VT.String && s.type == VT.None) return this;

    if ((type == VT.None && s.type == VT.Int) || (type == VT.None && s.type == VT.Float) || (type == VT.None && s.type == VT.String)) { return s; }

    if (type == VT.Int && s.type == VT.Int) { iVal += s.iVal; return this; }
    if (type == VT.Int && s.type == VT.Float) { type = VT.Float; fVal = iVal + s.fVal; return this; }
    if (type == VT.Int && s.type == VT.String) { type = VT.String; sVal = iVal + s.sVal; return this; }

    if (type == VT.Float && s.type == VT.Int) { fVal += s.iVal; return this; }
    if (type == VT.Float && s.type == VT.Float) { fVal += s.fVal; return this; }
    if (type == VT.Float && s.type == VT.String) { type = VT.String; sVal = fVal + s.sVal; return this; }

    if (type == VT.String && s.type == VT.Int) { sVal += s.iVal; return this; }
    if (type == VT.String && s.type == VT.Float) { sVal += s.fVal; return this; }
    if (type == VT.String && s.type == VT.String) { sVal += s.sVal; return this; }

    return this;
  }

  internal Value Sub(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type == VT.None && s.type == VT.None) return this;

    if (type == VT.None) {
      if (s.type == VT.Int) {
        type = VT.Int;
        iVal = -s.iVal;
      }
      else if (s.type == VT.Float) {
        type = VT.Float;
        fVal = -s.fVal;
      }
      else if (s.type == VT.String) {
        if (float.TryParse(s.sVal, numberstyle, culture, out float f)) {
          type = VT.Float;
          fVal = -f;
        }
        else if (int.TryParse(s.sVal, out int i)) {
          type = VT.Int;
          iVal = -i;
        }
      }
      return this;
    }

    if (type == VT.Int && s.type == VT.Int) { iVal -= s.iVal; return this; }
    if (type == VT.Int && s.type == VT.Float) { type = VT.Float; fVal = iVal - s.fVal; return this; }
    if (type == VT.Int && s.type == VT.String) {
      if (float.TryParse(s.sVal, numberstyle, culture, out float f)) {
        type = VT.Float;
        fVal = iVal - f;
      }
      else if (int.TryParse(s.sVal, out int i)) {
        iVal -= i;
      }
      return this;
    }

    if (type == VT.Float && s.type == VT.Int) { fVal -= s.iVal; return this; }
    if (type == VT.Float && s.type == VT.Float) { fVal -= s.fVal; return this; }
    if (type == VT.Float && s.type == VT.String) {
      if (float.TryParse(s.sVal, numberstyle, culture, out float f)) {
        fVal -= f;
      }
      else if (int.TryParse(s.sVal, out int i)) {
        iVal -= i;
      }
      return this;
    }

    return this;
  }

  internal Value Mul(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type == VT.None || s.type == VT.None) return this;

    if (type == VT.Int && s.type == VT.Int) { iVal *= s.iVal; return this; }
    if (type == VT.Int && s.type == VT.Float) { type = VT.Float; fVal = iVal * s.fVal; return this; }
    if (type == VT.Float && s.type == VT.Int) { fVal *= s.iVal; return this; }
    if (type == VT.Float && s.type == VT.Float) { fVal *= s.fVal; return this; }

    if (type == VT.String) {
      if (float.TryParse(sVal, numberstyle, culture, out float f)) {
        type = VT.Float;
        fVal = f;
        return Mul(s, culture);
      }
      else if (int.TryParse(sVal, out int i)) {
        type = VT.Int;
        iVal = i;
        return Mul(s, culture);
      }
      return this;
    }

    if (s.type == VT.String) {
      if (float.TryParse(s.sVal, numberstyle, culture, out float f)) {
        if (type == VT.Int) {
          type = VT.Float;
          fVal = iVal * f;
        }
        else {
          fVal *= f;
        }
        return this;
      }
      else if (int.TryParse(s.sVal, out int i)) {
        if (type == VT.Int) {
          iVal *= i;
        }
        else {
          fVal *= i;
        }
        return this;
      }
    }
    return this;
  }

  internal Value Div(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type == VT.None || s.type == VT.None) return this;

    if (type == VT.Int && s.type == VT.Int) { iVal /= s.iVal; return this; }
    if (type == VT.Int && s.type == VT.Float) { type = VT.Float; fVal = iVal / s.fVal; return this; }
    if (type == VT.Float && s.type == VT.Int) { fVal /= s.iVal; return this; }
    if (type == VT.Float && s.type == VT.Float) { fVal /= s.fVal; return this; }

    if (type == VT.String) {
      if (float.TryParse(sVal, numberstyle, culture, out float f)) {
        type = VT.Float;
        fVal = f;
        return Div(s, culture);
      }
      else if (int.TryParse(sVal, out int i)) {
        type = VT.Int;
        iVal = i;
        return Div(s, culture);
      }
      return this;
    }

    if (s.type == VT.String) {
      if (float.TryParse(s.sVal, numberstyle, culture, out float f)) {
        if (type == VT.Int) {
          type = VT.Float;
          fVal = iVal / f;
        }
        else {
          fVal /= f;
        }
        return this;
      }
      else if (int.TryParse(s.sVal, out int i)) {
        if (type == VT.Int) {
          iVal /= i;
        }
        else {
          fVal /= i;
        }
        return this;
      }
    }
    return this;
  }

  internal Value Mod(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type == VT.None || s.type == VT.None) return this;

    if (type == VT.Int && s.type == VT.Int) { iVal %= s.iVal; return this; }
    if (type == VT.Int && s.type == VT.Float) { type = VT.Float; fVal = iVal % s.fVal; return this; }
    if (type == VT.Float && s.type == VT.Int) { fVal %= s.iVal; return this; }
    if (type == VT.Float && s.type == VT.Float) { fVal %= s.fVal; return this; }

    if (type == VT.String) {
      if (float.TryParse(sVal, numberstyle, culture, out float f)) {
        type = VT.Float;
        fVal = f;
        return Mod(s, culture);
      }
      else if (int.TryParse(sVal, out int i)) {
        type = VT.Int;
        iVal = i;
        return Mod(s, culture);
      }
      return this;
    }

    if (s.type == VT.String) {
      if (float.TryParse(s.sVal, numberstyle, culture, out float f)) {
        if (type == VT.Int) {
          type = VT.Float;
          fVal = iVal % f;
        }
        else {
          fVal %= f;
        }
        return this;
      }
      else if (int.TryParse(s.sVal, out int i)) {
        if (type == VT.Int) {
          iVal %= i;
        }
        else {
          fVal %= i;
        }
        return this;
      }
    }
    return this;
  }

  internal Value LAnd(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    type = VT.Int;
    if (ToBool(culture) && s.ToBool(culture)) iVal = -1;
    else iVal = 0;
    return this;
  }

  internal Value LOr(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    type = VT.Int;
    if (ToBool(culture) || s.ToBool(culture)) iVal = -1;
    else iVal = 0;
    return this;
  }

  internal Value And(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type != VT.Int || s.type != VT.Int) {
      if (s.ToInt(culture) == 0) {
        iVal = 0;
        type = VT.Int;
      }
      return this;
    }

    iVal &= s.ToInt(culture);
    return this;
  }

  internal Value Or(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type != VT.Int || s.type != VT.Int) return this;

    iVal |= s.ToInt(culture);
    return this;
  }

  internal Value Xor(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type != VT.Int || s.type != VT.Int) return this;

    iVal ^= s.ToInt(culture);
    return this;
  }

  internal Value Lsh(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type != VT.Int) return this;
    iVal <<= s.ToInt(culture);
    return this;
  }

  internal Value Rsh(Value s, System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type != VT.Int) return this;
    iVal >>= s.ToInt(culture);
    return this;
  }


  internal Value Sub(System.Globalization.CultureInfo culture) {
    mode = MD.Dir;
    if (type == VT.Int) iVal = -iVal;
    if (type == VT.Float) fVal = -fVal;
    if (type == VT.None || sVal == null) return this;
    if (float.TryParse(sVal, numberstyle, culture, out float f)) fVal = -f;
    if (int.TryParse(sVal, out int i)) iVal = -i;
    return this;
  }

  internal Value Neg() {
    mode = MD.Dir;
    if (type == VT.Int) return new Value(iVal == 0 ? -1 : 0);
    if (type == VT.Float) return new Value(fVal == 0 ? -1 : 0);
    if (type == VT.String) return new Value(string.IsNullOrEmpty(sVal) ? "-1" : "");
    if (type == VT.None) return new Value(-1);
    return new Value(MD.Dir);
  }

  internal Value Inv() {
    mode = MD.Dir;
    if (type == VT.Int) return new Value(~iVal);
    return new Value();
  }

  internal int Compare(Value r, BNF mode, System.Globalization.CultureInfo culture) {
    switch (mode) {
      case BNF.COMPeq:
        if (type == VT.None) return r.type == VT.None ? -1 : 0;
        if (type == VT.Int) return iVal == r.ToInt(culture) ? -1 : 0;
        if (type == VT.Float) {
          float diff = fVal - r.ToFlt(culture);
          return (diff < .01f && diff > -.01f) ? -1 : 0;
        }
        if (type == VT.String) return sVal == r.ToStr() ? -1 : 0;
        break;

      case BNF.COMPne:
        if (type == VT.None) return r.type == VT.None ? 0 : -1;
        if (type == VT.Int) return iVal == r.ToInt(culture) ? 0 : -1;
        if (type == VT.Float) {
          float diff = fVal - r.ToFlt(culture);
          return (diff < .01f && diff > -.01f) ? 0 : -1;
        }
        if (type == VT.String) return sVal == r.ToStr() ? 0 : -1;
        break;

      case BNF.COMPlt:
        if (type == VT.None) return 0;
        if (type == VT.Int) return iVal < r.ToInt(culture) ? -1 : 0;
        if (type == VT.Float) return fVal < r.ToFlt(culture) ? -1 : 0;
        if (type == VT.String) return sVal.CompareTo(r.ToStr()) < 0 ? -1 : 0;
        break;

      case BNF.COMPle:
        if (type == VT.None) return 0;
        if (type == VT.Int) return iVal <= r.ToInt(culture) ? -1 : 0;
        if (type == VT.Float) return fVal <= r.ToFlt(culture) ? -1 : 0;
        if (type == VT.String) return sVal.CompareTo(r.ToStr()) <= 0 ? -1 : 0;
        break;

      case BNF.COMPgt:
        if (type == VT.None) return 0;
        if (type == VT.Int) return iVal > r.ToInt(culture) ? -1 : 0;
        if (type == VT.Float) return fVal > r.ToFlt(culture) ? -1 : 0;
        if (type == VT.String) return sVal.CompareTo(r.ToStr()) > 0 ? -1 : 0;
        break;

      case BNF.COMPge:
        if (type == VT.None) return 0;
        if (type == VT.Int) return iVal >= r.ToInt(culture) ? -1 : 0;
        if (type == VT.Float) return fVal >= r.ToFlt(culture) ? -1 : 0;
        if (type == VT.String) return sVal.CompareTo(r.ToStr()) >= 0 ? -1 : 0;
        break;
    }
    return 0;
  }

  internal void ConvertToArray(Variables variables, int reg, int pos) {
    if (pos < 0) pos = 1;
    if (pos > 1023) pos = 1023;
    aVals = new int[pos + 1];
    string name = variables.GetRegName(idx);
    if (name.Length > 2) name = name.Substring(0, name.Length - 2);
    for (int i = 0; i < aVals.Length; i++) {
      aVals[i] = variables.Add(name + "[" + i + "]");
      variables.Set(aVals[i], this);
    }
    type = VT.Array;
    // update the variables
    variables.Set(reg, aVals);
  }

  internal Value GetArrayValue(Variables variables, int pos) {
    if (aVals == null || pos < 0) return new Value();
    if (pos > 1023) pos = 1023;
    if (pos >= aVals.Length) { // Need to extend
      int len = aVals.Length;
      int[] newVals = new int[pos + 1];
      for (int i = 0; i < len; i++)
        newVals[i] = aVals[i];
      string name = variables.GetRegName(idx);
      if (name.Length > 2) name = name.Substring(0, name.Length - 2);
      for (int i = len; i < pos + 1; i++) {
        newVals[i] = variables.Add(name + "[" + i + "]");
        variables.Set(newVals[i]);
      }
      aVals = newVals;
      variables.Set(idx, aVals);
    }
    return variables.Get(aVals[pos]);
  }

  internal string ToDebugStr() {
    if (type == VT.None) return "";
    if (type == VT.Int) return iVal.ToString();
    if (type == VT.Float) return fVal.ToString("F3");
    if (type == VT.String) return "\"" + sVal.Replace("\"", "\\\"") + "\"";
    if (type == VT.Array) return "[" + aVals?.Length + "]";
    return "<i>unknown</i>";
  }

  internal void Assign(Value src) {
    mode = src.mode;
    type = src.type;
    iVal = src.iVal;
    fVal = src.fVal;
    sVal = src.sVal;
    if (src.aVals == null) aVals = null;
    else aVals = new int[src.aVals.Length];
  }
}
