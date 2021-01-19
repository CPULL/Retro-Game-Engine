using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteLine : MonoBehaviour {
  public NoteType type;
  public byte btype;

  public int val; // FIXME remove
  public int len; // FIXME remove

  public Image TypeImg;
  public Text ValTxt;
  public Text LenTxt;
  public RectTransform back;
  public Button ColButton;

  internal void SetValues(NoteData blockNote, Sprite[] sprites,  int[] freqs, string[] notenames, List<Wave> waves) {
    btype = blockNote.type;
    gameObject.SetActive(true);
    ValTxt.fontSize = 28;
    LenTxt.fontSize = 28;

    switch (btype) {
      case 0: // Empty
        TypeImg.sprite = sprites[0];
        ValTxt.text = "";
        LenTxt.text = "";
        back.sizeDelta = new Vector2(38, 0);
        break;

      // Note
      case 1: {
        TypeImg.sprite = sprites[1];
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames);
        int len = GetNoteLen(blockNote);
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Wave
      case 2: {
        TypeImg.sprite = sprites[2];
        ValTxt.text = GetWaveVal(blockNote, waves);
        LenTxt.text = "";
        back.sizeDelta = new Vector2(38, 0);
      }
      break;

      // Note + wave
      case 3: {
        TypeImg.sprite = sprites[8];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames) + "\n"+ GetWaveVal(blockNote, waves);
        int len = GetNoteLen(blockNote);
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Volume
      case 4: {
        TypeImg.sprite = sprites[3];
        ValTxt.text = GetVolVal(blockNote);
        int len = GetVolLen(blockNote);
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;


      case 29: // Note + Vol + Pitch + Pan
      case 21: // Note + Vol + Pan
      case 13: // Note + Vol + Pitch
      // Note + Volume
      case 5: {
        TypeImg.sprite = sprites[9];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames) + "\n" + GetVolVal(blockNote);
        int len = GetNoteLen(blockNote);
        int lenv = GetVolLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      case 30: // Wave + Vol + Pitch + Pan
      case 22: // Wave + Vol + Pan
      case 14: // Wave + Vol + Pitch
      // Wave + Volume
      case 6: {
        TypeImg.sprite = sprites[7];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetWaveVal(blockNote, waves) + "\n" + GetVolVal(blockNote);
        int len = GetVolLen(blockNote);
        LenTxt.text = "\n" + len;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      case 23: // Note + Wave + Vol + Pan
      case 15: // Note + Wave + Vol + Pitch
      // Note + Wave + Volume
      case 7: {
        TypeImg.sprite = sprites[8];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames) + "\n" + GetWaveVal(blockNote, waves) + " " + GetVolVal(blockNote);
        int len = GetNoteLen(blockNote);
        int lenv = GetVolLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Pitch
      case 8: {
        TypeImg.sprite = sprites[4];
        ValTxt.text = GetPitchVal(blockNote);
        int len = GetPitchLen(blockNote);
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      case 25: // Note + Pitch + Pan
      // Note + Pitch
      case 9: {
        TypeImg.sprite = sprites[10];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames) + "\n" + GetPitchVal(blockNote);
        int len = GetNoteLen(blockNote);
        int lenv = GetPitchLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      
      case 26: // Wave + Pitch + Pan
      // Wave + Pitch
      case 10: {
        TypeImg.sprite = sprites[2];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetWaveVal(blockNote, waves) + "\n" + GetPitchVal(blockNote);
        int len = GetPitchLen(blockNote);
        LenTxt.text = "\n" + len;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      
      case 27: // Note + Wave + Pitch + Pan
      // Note + Wave + Pitch
      case 11: {
        TypeImg.sprite = sprites[8];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames) + " " + GetWaveVal(blockNote, waves) + "\n" + GetPitchVal(blockNote);
        int len = GetNoteLen(blockNote);
        int lenv = GetPitchLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      case 28: // Vol + Pitch + Pan
      // Vol + Pitch
      case 12: {
        TypeImg.sprite = sprites[7];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetVolVal(blockNote) + "\n" + GetPitchVal(blockNote);
        int len = GetVolLen(blockNote);
        int lenv = GetPitchLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Pan
      case 16: {
        TypeImg.sprite = sprites[5];
        ValTxt.text = GetPanVal(blockNote);
        int len = GetPanLen(blockNote);
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Note + Pan
      case 17: {
        TypeImg.sprite = sprites[11];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames) + "\n" + GetPanVal(blockNote);
        int len = GetNoteLen(blockNote);
        int lenv = GetPanLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Wave + Pan
      case 18: {
        TypeImg.sprite = sprites[2];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetWaveVal(blockNote, waves) + "\n" + GetPanVal(blockNote);
        int len = GetPanLen(blockNote);
        LenTxt.text = "\n" + len;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Note + Wave + Pan
      case 19: {
        TypeImg.sprite = sprites[8];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames) + " " + GetWaveVal(blockNote, waves) + "\n" + GetPanVal(blockNote);
        int len = GetNoteLen(blockNote);
        int lenv = GetPanLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Vol + Pan
      case 20: {
        TypeImg.sprite = sprites[3];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetVolVal(blockNote) + "\n" + GetPanVal(blockNote);
        int len = GetVolLen(blockNote);
        int lenv = GetPanLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // Pitch + Pan
      case 24: {
        TypeImg.sprite = sprites[4];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetPitchVal(blockNote) + "\n" + GetPanVal(blockNote);
        int len = GetPitchLen(blockNote);
        int lenv = GetPanLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;

      // All
      case 31: {
        TypeImg.sprite = sprites[6];
        ValTxt.fontSize = 14;
        LenTxt.fontSize = 14;
        ValTxt.text = GetNoteVal(blockNote, freqs, notenames) + "\n" + GetWaveVal(blockNote, waves) + " " + GetVolVal(blockNote);
        int len = GetNoteLen(blockNote);
        int lenv = GetVolLen(blockNote);
        LenTxt.text = len + "\n" + lenv;
        if (len < lenv) len = lenv;
        back.sizeDelta = new Vector2(38, len * 32);
      }
      break;
    }
  }

  private string GetNoteVal(NoteData n, int[] freqs, string[] notenames) {
    int val = n.GetVal(NoteType.Note);
    if (val == 0) {
      val = 440;
      n.SetVal(NoteType.Note, 440);
    }
    string res = val.ToString();
    for (int i = 0; i < freqs.Length; i++)
      if (val == freqs[i]) {
        res = notenames[i] + " - " + val;
        break;
      }
    return res;
  }
  private int GetNoteLen(NoteData n) {
    int len = n.GetLen(NoteType.Note);
    if (len < 1) len = 1;
    return len;
  }

  private string GetWaveVal(NoteData n, List<Wave> waves) {
    int val = n.GetVal(NoteType.Wave);
    foreach (Wave w in waves)
      if (w.id == val) {
        return w.id + " " + w.name;
      }
    return val.ToString();
  }
  private int GetWaveLen(NoteData n) {
    return 0;
  }

  private string GetVolVal(NoteData n) {
    int val = n.GetVal(NoteType.Volume);
    return ((int)(val / 1024f)) + "%";
  }
  private int GetVolLen(NoteData n) {
    int len = n.GetLen(NoteType.Volume);
    if (len < 1) len = 1;
    return len;
  }

  private string GetPitchVal(NoteData n) {
    float val = n.GetVal(NoteType.Pitch) / 100f;
    if (val == 0)
      return "reset";
    else if (val - (int)val == 0) {
      if (val > 0)
        return "+" + ((int)val).ToString();
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
  private int GetPitchLen(NoteData n) {
    int len = n.GetLen(NoteType.Pitch);
    if (len < 1) len = 1;
    return len;
  }

  private string GetPanVal(NoteData n) {
    float pan = n.GetVal(NoteType.Pan) / 127f;
    if (pan < -1) pan = -1;
    if (pan > 1) pan = 1;
    return pan.ToString();
  }
  private int GetPanLen(NoteData n) {
    int len = n.GetLen(NoteType.Pan);
    if (len < 1) len = 1;
    return len;
  }

  internal void SetWave(int id, string name, Sprite spr) {
    /* FIXME

    type = NoteType.Wave;
    TypeImg.sprite = spr;
    val = id;
    len = 0;
    back.sizeDelta = new Vector2(38, 0);
    gameObject.SetActive(true);
    ValTxt.fontSize = 14;
    ValTxt.text = id + "\n" + name;
    LenTxt.text = "";
    */
  }

    internal void SetZeroValues(Sprite[] sprites) {
    gameObject.SetActive(true);
    /* FIXME
    type = NoteType.Empty;
    TypeImg.sprite = sprites[0];
    val = 0;
    ValTxt.text = "";
    len = 0;
    LenTxt.text = "";
    */
    back.sizeDelta = new Vector2(38, 0 * 32);
  }

}


public enum NoteType { Empty=0, Note=1, Wave=2, Volume=3, Pitch=4, Pan=5 };
