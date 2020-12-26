using System;

[System.Serializable]
public class Register {
  public VT type;
  public int iVal;
  public float fVal;
  public string sVal;
  public char Reg;

  public Register(char r) {
    Reg = r;
    iVal = 0;
    fVal = 0;
    sVal = null;
    type = VT.None;
  }

  public Register(Register src) {
    type = src.type;
    iVal = src.iVal;
    fVal = src.fVal;
    sVal = src.sVal;
    Reg = src.Reg;
  }

  public Register(int i) {
    type = VT.Int;
    iVal = i;
    fVal = 0;
    sVal = null;
    Reg = '\0';
  }

  public Register(float f) {
    type = VT.Float;
    iVal = 0;
    fVal = f;
    sVal = null;
    Reg = '\0';
  }
  public Register(string s) {
    type = VT.String;
    iVal = 0;
    fVal = 0;
    sVal = s;
    Reg = '\0';
  }

  public override string ToString() {
    if (type == VT.Int) return iVal.ToString();
    if (type == VT.Float) return fVal.ToString();
    if (type == VT.String) return sVal??"";
    return "";
  }

  internal int ToInt() {
    if (type == VT.Int) return iVal;
    if (type == VT.Float) return (int)fVal;
    if (type == VT.String) {
      if (int.TryParse(sVal, out int res)) return res;
      try {
        res = Convert.ToInt32(sVal, 16);
        return res;
      } catch (Exception) { }
      if (float.TryParse(sVal, out float resf)) return (int)resf;
    }
    return 0;
  }

  internal float ToFloat() {
    if (type == VT.Int) return iVal;
    if (type == VT.Float) return (float)fVal;
    if (type == VT.String) {
      if (int.TryParse(sVal, out int res)) return res;
      try {
        res = Convert.ToInt32(sVal, 16);
        return res;
      } catch (Exception) { }
      if (float.TryParse(sVal, out float resf)) return resf;
    }
    return 0;
  }

  internal byte ToByte() {
    if (type == VT.Int) return (byte)(iVal & 255);
    if (type == VT.Float) return (byte)(((int)fVal) & 255);
    if (type == VT.String) {
      if (int.TryParse(sVal, out int res)) return (byte)(res & 255);
      try {
        res = Convert.ToInt32(sVal, 16);
        return (byte)(res & 255);
      } catch (Exception) { }
      if (float.TryParse(sVal, out float resf)) return (byte)(int)resf;
    }
    return 0;
  }

  internal void Incr() {
    if (type == VT.None) { type = VT.Int; iVal = 0; }
    if (type == VT.Int) iVal++;
    if (type == VT.Float) fVal += 1;
  }
  internal void Decr() {
    if (type == VT.None) { type = VT.Int; iVal = 0; }
    if (type == VT.Int) iVal--;
    if (type == VT.Float) fVal -= 1;
  }

  private VT GetType(VT l, VT r, BNF mode) {
    if (mode == BNF.ASSIGN || l == VT.None) return r;
    if (mode == BNF.ASSIGNsum || mode == BNF.ASSIGNmul) {
      if (l == VT.Int && r == VT.None) return VT.Int;
      if (l == VT.Int && r == VT.Int) return VT.Int;
      if (l == VT.Int && r == VT.Float) return VT.Float;
      if (l == VT.Int && r == VT.String) return VT.String;
      if (l == VT.Float && r == VT.String) return VT.String;
      if (l == VT.Float) return VT.Float;
      if ((l == VT.String || r == VT.String)) return VT.String;
    }
    if (mode == BNF.ASSIGNsub || mode == BNF.ASSIGNdiv || mode == BNF.ASSIGNmod) {
      if (l == VT.Int && r == VT.None) return VT.Int;
      if (l == VT.Int && r == VT.Int) return VT.Int;
      if (l == VT.Int && r == VT.Float) return VT.Float;
      if (l == VT.Int && r == VT.String) return VT.Int;
      if (l == VT.Float && r == VT.String) return VT.Float;
      if (l == VT.Float) return VT.Float;
      if ((l == VT.String || r == VT.String)) return VT.None;
    }
    if (mode == BNF.ASSIGNand || mode == BNF.ASSIGNor || mode == BNF.ASSIGNxor) {
      if (l != VT.Int || r != VT.Int) return VT.None;
      return VT.Int;
    }

    return VT.None;
  }

  internal void Set(Register r, BNF mode) {
    type = GetType(type, r.type, mode);

    switch (mode) {
      case BNF.ASSIGN:
        if (type == VT.Int) iVal = r.ToInt();
        else if (type == VT.Float) fVal = r.ToFloat();
        else if (type == VT.String) sVal = r.ToString();
        break;

      case BNF.ASSIGNsum:
        if (type == VT.Int) iVal += r.ToInt();
        else if (type == VT.Float) fVal += r.ToFloat();
        else if (type == VT.String) sVal += r.ToString();
        break;

      case BNF.ASSIGNsub:
        if (type == VT.Int) iVal -= r.ToInt();
        else if (type == VT.Float) fVal -= r.ToFloat();
        else throw new Exception("- cannot be used for strings");
        break;

      case BNF.ASSIGNmul:
        if (type == VT.Int) iVal *= r.ToInt();
        else if (type == VT.Float) fVal *= r.ToFloat();
        else if (type == VT.String) sVal += r.ToString();
        break;

      case BNF.ASSIGNdiv:
        if (type == VT.Int) iVal /= r.ToInt();
        else if (type == VT.Float) fVal /= r.ToFloat();
        else throw new Exception("/ cannot be used for strings");
        break;

      case BNF.ASSIGNmod:
        if (type == VT.Int) iVal %= r.ToInt();
        else if (type == VT.Float) fVal %= r.ToFloat();
        else throw new Exception("% cannot be used for strings");
        break;

      case BNF.ASSIGNand:
        if (r.type == VT.Int) iVal &= r.ToInt();
        else throw new Exception("AND requires integers");
        break;

      case BNF.ASSIGNor:
        if (r.type == VT.Int) iVal |= r.ToInt();
        else throw new Exception("OR requires integers");
        break;

      case BNF.ASSIGNxor:
        if (r.type == VT.Int) iVal ^= r.ToInt();
        else throw new Exception("XOR requires integers");
        break;
    }
  }

  internal Register Sum(Register s) {
    if (type == VT.None && s.type == VT.None) return this;
    if (type == VT.Int && s.type == VT.None) return this;
    if (type == VT.Float && s.type == VT.None) return this;
    if (type == VT.String && s.type == VT.None) return this;

    if ((type == VT.None && s.type == VT.Int) || (type == VT.None && s.type == VT.Float) || (type == VT.None && s.type == VT.String)) { Set(s, BNF.ASSIGN); return this; }

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

  internal Register Sub(Register s) {
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
        if (float.TryParse(s.sVal, out float f)) {
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
      if (float.TryParse(s.sVal, out float f)) {
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
      if (float.TryParse(s.sVal, out float f)) {
        fVal -= f;
      }
      else if (int.TryParse(s.sVal, out int i)) {
        iVal -= i;
      }
      return this; 
    }

    return this;
  }


  internal Register Mul(Register s) {
    if (type == VT.None || s.type == VT.None) return this;

    if (type == VT.Int && s.type == VT.Int) { iVal *= s.iVal; return this; }
    if (type == VT.Int && s.type == VT.Float) { type = VT.Float; fVal = iVal * s.fVal; return this; }
    if (type == VT.Float && s.type == VT.Int) { fVal *= s.iVal; return this; }
    if (type == VT.Float && s.type == VT.Float) { fVal *= s.fVal; return this; }

    if (type == VT.String) {
      if (float.TryParse(sVal, out float f)) {
        type = VT.Float;
        fVal = f;
        return Mul(s);
      }
      else if (int.TryParse(sVal, out int i)) {
        type = VT.Int;
        iVal = i;
        return Mul(s);
      }
      return this;
    }

    if (s.type == VT.String) {
      if (float.TryParse(s.sVal, out float f)) {
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
          iVal = iVal * i;
        }
        else {
          fVal *= i;
        }
        return this;
      }
    }
    return this;
  }

  internal Register Div(Register s) {
    if (type == VT.None || s.type == VT.None) return this;

    if (type == VT.Int && s.type == VT.Int) { iVal /= s.iVal; return this; }
    if (type == VT.Int && s.type == VT.Float) { type = VT.Float; fVal = iVal / s.fVal; return this; }
    if (type == VT.Float && s.type == VT.Int) { fVal /= s.iVal; return this; }
    if (type == VT.Float && s.type == VT.Float) { fVal /= s.fVal; return this; }

    if (type == VT.String) {
      if (float.TryParse(sVal, out float f)) {
        type = VT.Float;
        fVal = f;
        return Div(s);
      }
      else if (int.TryParse(sVal, out int i)) {
        type = VT.Int;
        iVal = i;
        return Div(s);
      }
      return this;
    }

    if (s.type == VT.String) {
      if (float.TryParse(s.sVal, out float f)) {
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
          iVal = iVal / i;
        }
        else {
          fVal /= i;
        }
        return this;
      }
    }
    return this;
  }

  internal Register Mod(Register s) {
    if (type == VT.None || s.type == VT.None) return this;

    if (type == VT.Int && s.type == VT.Int) { iVal %= s.iVal; return this; }
    if (type == VT.Int && s.type == VT.Float) { type = VT.Float; fVal = iVal % s.fVal; return this; }
    if (type == VT.Float && s.type == VT.Int) { fVal %= s.iVal; return this; }
    if (type == VT.Float && s.type == VT.Float) { fVal %= s.fVal; return this; }

    if (type == VT.String) {
      if (float.TryParse(sVal, out float f)) {
        type = VT.Float;
        fVal = f;
        return Mod(s);
      }
      else if (int.TryParse(sVal, out int i)) {
        type = VT.Int;
        iVal = i;
        return Mod(s);
      }
      return this;
    }

    if (s.type == VT.String) {
      if (float.TryParse(s.sVal, out float f)) {
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
          iVal = iVal % i;
        }
        else {
          fVal %= i;
        }
        return this;
      }
    }
    return this;
  }

  internal Register And(Register s) {
    if (type != VT.Int || s.type != VT.Int) {
      if (s.ToInt() == 0) {
        iVal = 0;
        type = VT.Int;
      }
      return this;
    }

    iVal &= s.ToInt();
    return this;
  }

  internal Register Or(Register s) {
    if (type != VT.Int || s.type != VT.Int) return this;

    iVal |= s.ToInt();
    return this;
  }

  internal Register Xor(Register s) {
    if (type != VT.Int || s.type != VT.Int) return this;

    iVal ^= s.ToInt();
    return this;
  }


  internal Register Sub() {
    if (type == VT.Int) return new Register(-iVal);
    if (type == VT.Float) return new Register(-fVal);
    if (type == VT.None || sVal == null) return new Register('\0');
    if (float.TryParse(sVal, out float f)) return new Register(-f);
    if (int.TryParse(sVal, out int i)) return new Register(-i);
    return new Register('\0');
  }

  internal Register Neg() {
    if (type == VT.Int) return new Register(iVal == 0 ? -1 : 0);
    if (type == VT.Float) return new Register(fVal == 0 ? -1 : 0);
    if (type == VT.String) return new Register(string.IsNullOrEmpty(sVal) ? "-1" : "");
    if (type == VT.None) return new Register(-1);
    return new Register('\0');
  }

  internal Register Inv() {
    if (type == VT.Int) return new Register(~iVal);
    if (type == VT.Float) return new Register(0);
    if (type == VT.String) return new Register("");
    if (type == VT.None) return new Register(-1);
    return new Register('\0');
  }

  internal int Compare(Register r, BNF mode) {
    switch(mode) {
      case BNF.COMPeq:
        if (type == VT.None) return r.type == VT.None ? -1 : 0;
        if (type == VT.Int) return iVal == r.ToInt() ? -1 : 0;
        if (type == VT.Float) {
          float diff = fVal - r.ToFloat();
          return (diff < .01f && diff > -.01f) ? -1 : 0;
        }
        if (type == VT.String) return sVal == r.ToString() ? -1 : 0;
        break;

      case BNF.COMPne:
        if (type == VT.None) return r.type == VT.None ? 0 : -1;
        if (type == VT.Int) return iVal == r.ToInt() ? 0 : -1;
        if (type == VT.Float) {
          float diff = fVal - r.ToFloat();
          return (diff < .01f && diff > -.01f) ? 0 : -1;
        }
        if (type == VT.String) return sVal == r.ToString() ? 0 : -1;
        break;

      case BNF.COMPlt:
        if (type == VT.None) return 0;
        if (type == VT.Int) return iVal < r.ToInt() ? -1 : 0;
        if (type == VT.Float) return fVal < r.ToFloat() ? -1 : 0;
        if (type == VT.String) return sVal.CompareTo(r.ToString()) < 0 ? -1 : 0;
        break;

      case BNF.COMPle:
        if (type == VT.None) return 0;
        if (type == VT.Int) return iVal <= r.ToInt() ? -1 : 0;
        if (type == VT.Float) return fVal <= r.ToFloat() ? -1 : 0;
        if (type == VT.String) return sVal.CompareTo(r.ToString()) <= 0 ? -1 : 0;
        break;

      case BNF.COMPgt:
        if (type == VT.None) return 0;
        if (type == VT.Int) return iVal > r.ToInt() ? -1 : 0;
        if (type == VT.Float) return fVal > r.ToFloat() ? -1 : 0;
        if (type == VT.String) return sVal.CompareTo(r.ToString()) > 0 ? -1 : 0;
        break;

      case BNF.COMPge:
        if (type == VT.None) return 0;
        if (type == VT.Int) return iVal >= r.ToInt() ? -1 : 0;
        if (type == VT.Float) return fVal >= r.ToFloat() ? -1 : 0;
        if (type == VT.String) return sVal.CompareTo(r.ToString()) >= 0 ? -1 : 0;
        break;
    }
    return 0;
  }
}
