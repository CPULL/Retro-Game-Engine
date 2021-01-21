using System.Collections.Generic;

public class Wave {
  public int id;
  public string name;
  public Waveform wave;
  public float phase;
  public byte a;
  public byte d;
  public byte s;
  public byte r;
  public byte[] rawPCM;

  internal void CopyForm(Wave w) {
    wave = w.wave;
    phase = w.phase;
    a = w.a;
    d = w.d;
    s = w.s;
    r = w.r;
    rawPCM = w.rawPCM;
  }
}


public class MusicData {
  public string name;
  public int bpm;
  public int defLen;
  public byte[] voices;
  public List<int> blocks;

  public int NumVoices {
    get {
      int numv = 0;
      for (int i = 0; i < voices.Length; i++)
        if (voices[i] != 255) numv++;
      return numv;
    }
  }
}

public class BlockData {
  public int id;
  public string name;
  public int bpm;
  public int len;
  public List<NoteData>[] chs;
}


public class Swipe {
  public float vols;
  public float vole;
  public float voltime;
  public float vollen;
  public float pitchs;
  public float pitche;
  public float pitchtime;
  public float pitchlen;
  public float pans;
  public float pane;
  public float pantime;
  public float panlen;

  public Swipe() {
    vols = 0;
    vole = 0;
    voltime = 0;
    vollen = 0;
    pitchs = 0;
    pitche = 0;
    pitchtime = 0;
    pitchlen = 0;
    pans = 0;
    pane = 0;
    pantime = 0;
    panlen = 0;
  }
}

/*

Define better the way the cells are done. So we can save meory and have cells with multiple infos.

[type] if 0 then next cell, used also as terminator (len=0 can be a terminator and in this case len will be set as 1)
[type=note] [freq] [len]



Empty=0
Note=1
Wave=2
Volume=3
Pitch=4
Pan=5

[byte] -> bitfield with items
[2 bytes] -> value + len <= for each of the selected types



 */


public class NoteData {
  public byte NoteType { get; private set; }

  struct NoteVal {
    public short val;
    public byte len;
  };

  readonly NoteVal[] vls = new NoteVal[5];

  public void Zero() {
    NoteType = 0;
    for (int i = 0; i < 5; i++) {
      vls[i].val = 0;
      vls[i].len = 0;
    }
  }

  public void Zero(NoteType t) {
    if (t == global::NoteType.Empty) {
      Zero();
      return;
    }
    int pos = (byte)t - 1;
    NoteType &= (byte)(255 - (1 << pos));
    vls[pos].val = 0;
    vls[pos].len = 0;
  }

  public bool IsType(NoteType t) {
    if (t == global::NoteType.Empty) return NoteType == 0;
    int pos = (byte)t - 1;
    return (NoteType & (1 << pos)) != 0;
  }

  public bool IsEmpty() {
    return NoteType == 0;
  }


  public short GetVal(NoteType t) {
    if (t == global::NoteType.Empty) return 0;
    int pos = (byte)t - 1;
    return vls[pos].val;
  }

  public byte GetLen(NoteType t) {
    if (t == global::NoteType.Empty) return 0;
    int pos = (byte)t - 1;
    return vls[pos].len;
  }

  public float GetVol() {
    return vls[2].val / 1000f;
  }
  public float GetPitch() {
    return vls[3].val / 100f;
  }
  public float GetPan() {
    return ((vls[4].val - 500) / 500f);
  }

  public void Set(NoteType t) {
    if (t == global::NoteType.Empty) return;
    int pos = (byte)t - 1;
    NoteType |= (byte)(1 << pos);
  }

  public void Set(NoteType t, short val, byte len) {
    if (t == global::NoteType.Empty) return;
    int pos = (byte)t - 1;
    NoteType |= (byte)(1 << pos);
    vls[pos].val = val;
    vls[pos].len = len;
  }

  public void SetVal(NoteType t, short val) {
    if (t == global::NoteType.Empty) return;
    int pos = (byte)t - 1;
    NoteType |= (byte)(1 << pos);
    vls[pos].val = val;
  }

  public void SetLen(NoteType t, byte len) {
    if (t == global::NoteType.Empty) return;
    int pos = (byte)t - 1;
    NoteType |= (byte)(1 << pos);
    vls[pos].len = len;
  }

  internal NoteData Duplicate() {
    NoteData n = new NoteData { NoteType = this.NoteType };
    for (int i = 0; i < 5; i++) {
      n.vls[i].val = vls[i].val;
      n.vls[i].len = vls[i].len;
    }
    return n;
  }

  internal void Set(NoteData src) {
    NoteType = src.NoteType;
    for (int i = 0; i < 5; i++) {
      vls[i].val = src.vls[i].val;
      vls[i].len = src.vls[i].len;
    }
  }


  // note -> ushort with frequency
  // wave -> ushort with id
  // vol -> 0-1000 short -> 0-100%
  // Pitch -> -32768 - +32768
  // Pan -> 0-1000 short -> -1<->+1 (val-500)/500


  public static string ConvertVal2Vol(short num) {
    return (num * 100 / 1000) + "%";
  }
  public static float GetVolVal(short num) {
    return (num * 100 / 1000);
  }
  public static short ConvertVol2Val(int vol) {
    return (short)(vol * 1000 / 100);
  }

  public static string ConvertVal2Pitch(short num) {
    float val = num / 100f;
    if (val == 0) return "0";
    else if (val - (int)val == 0) {
      if (val > 0)
        return "+" + (int)val;
      else
        return ((int)val).ToString();
    }
    else {
      if (val > 0)
        return "+" + val.ToString();
      else
        return val.ToString();
    }
  }
  public static float GetPitchVal(short num) {
    return num / 100f;
  }
  public static short ConvertPitch2Val(float pitch) {
    return (short)(pitch * 100);
  }

  public static string ConvertVal2Pan(short num) {
    float val = ((num - 500) / 500f);
    if (val > 0)
      return "+" + val.ToString("0.00");
    else
      return val.ToString("0.00");
  }
  public static float GetPanVal(short num) {
    return ((num - 500) / 500f);
  }
  public static short ConvertPan2Val(float pan) {
    return (short)((pan + 1) * 500);
  }

}

