﻿using UnityEngine;
using UnityEngine.UI;

public class MusicNote : MonoBehaviour {
  public NoteType type;
  public Image TypeImg;
  public int val;
  public Text ValTxt;
  public int len;
  public Text LenTxt;
  public RectTransform back;

  internal void SetValues(BlockNote blockNote, Sprite[] sprites,  int[] freqs, string[] notenames) {
    type = blockNote.type;
    TypeImg.sprite = sprites[(int)blockNote.type];
    val = blockNote.val;
    len = blockNote.len;
    back.sizeDelta = new Vector2(38, len * 32);

    switch (type) {
      case NoteType.Empty: // Nothing required
        ValTxt.text = "";
        LenTxt.text = "";
        break;

      case NoteType.Note: // val should be the frequency with the text being the visible note
        ValTxt.text = blockNote.val.ToString();
        for (int i = 0; i < freqs.Length; i++)
          if (blockNote.val == freqs[i]) {
            ValTxt.text = notenames[i];
            break;
          }
        LenTxt.text = len.ToString();
        break;

      case NoteType.Wave: // val should be the wave id
        ValTxt.text = val.ToString();
        LenTxt.text = "";
        break;

      case NoteType.Volume: // val should be the volume
        ValTxt.text = val.ToString();
        LenTxt.text = len.ToString();
        break;

      case NoteType.Freq: // val should be the frequency
        ValTxt.text = ">" + blockNote.val.ToString();
        for (int i = 0; i < freqs.Length; i++)
          if (blockNote.val == freqs[i]) {
            ValTxt.text = ">" + notenames[i];
            break;
          }
        LenTxt.text = len.ToString();
        break;
    }
  }

  internal void SetZeroValues(Sprite[] sprites) {
    type = NoteType.Empty;
    TypeImg.sprite = sprites[0];
    val = 0;
    ValTxt.text = "";
    len = 0;
    LenTxt.text = "";
    back.sizeDelta = new Vector2(38, len * 32);
  }
}


public enum NoteType { Empty=0, Note=1, Volume=2, Wave=3, Freq=4 };
/*
 
  [type] [val] [steps]


 note [freq] [steps]
 volume [val] [steps]
 wave [numwave]
 freq [val] [steps]
 */