using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteLine : MonoBehaviour {
  public NoteType type;
  public Image TypeImg;
  public int val;
  public Text ValTxt;
  public int len;
  public Text LenTxt;
  public RectTransform back;
  public Button ColButton;

  internal void SetValues(NoteData blockNote, Sprite[] sprites,  int[] freqs, string[] notenames, List<Wave> waves) {
    type = blockNote.type;
    TypeImg.sprite = sprites[(int)blockNote.type];
    val = blockNote.val;
    len = blockNote.len;
    gameObject.SetActive(true);
    ValTxt.fontSize = 28;

    switch (type) {
      case NoteType.Empty: // Nothing required
        ValTxt.text = "";
        LenTxt.text = "";
        back.sizeDelta = new Vector2(38, 0);
        break;

      case NoteType.Note: // val should be the frequency with the text being the visible note
        if (len == 0) {
          len = 1;
          blockNote.len = 1;
        }
        if (val == 0) {
          val = 440;
          blockNote.val = 440;
        }
        ValTxt.text = blockNote.val.ToString();
        for (int i = 0; i < freqs.Length; i++)
          if (blockNote.val == freqs[i]) {
            ValTxt.text = notenames[i];
            break;
          }
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
        break;

      case NoteType.Wave: // val should be the wave id
        ValTxt.fontSize = 14;
        LenTxt.text = "";
        back.sizeDelta = new Vector2(38, 0);
        ValTxt.text = val.ToString();
        foreach (Wave w in waves)
          if (w.id == val) {
            ValTxt.text = w.id + "\n" + w.name;
            break;
          }
        break;

      case NoteType.Volume: // val should be the volume
        ValTxt.text = val.ToString();
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
        break;

      case NoteType.Freq: // val should be the frequency
        ValTxt.text = ">" + blockNote.val.ToString();
        for (int i = 0; i < freqs.Length; i++)
          if (blockNote.val == freqs[i]) {
            ValTxt.text = ">" + notenames[i];
            break;
          }
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
        break;

      case NoteType.Pan: // val should go from -127 to 127
        float pan = blockNote.val / 127f;
        if (pan < -1) pan = -1;
        if (pan > 1) pan = 1;
        ValTxt.text = pan.ToString();
        LenTxt.text = len.ToString();
        back.sizeDelta = new Vector2(38, len * 32);
        break;
    }
  }

  internal void SetWave(int id, string name, Sprite spr) {
    type = NoteType.Wave;
    TypeImg.sprite = spr;
    val = id;
    len = 0;
    back.sizeDelta = new Vector2(38, 0);
    gameObject.SetActive(true);
    ValTxt.fontSize = 14;
    ValTxt.text = id + "\n" + name;
    LenTxt.text = "";
  }

    internal void SetZeroValues(Sprite[] sprites) {
    gameObject.SetActive(true);
    type = NoteType.Empty;
    TypeImg.sprite = sprites[0];
    val = 0;
    ValTxt.text = "";
    len = 0;
    LenTxt.text = "";
    back.sizeDelta = new Vector2(38, len * 32);
  }

}


public enum NoteType { Empty=0, Note=1, Wave=2, Volume=3, Freq=4, Pan=5 };
