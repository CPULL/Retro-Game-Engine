using UnityEngine;
using UnityEngine.UI;

public class MusicNote : MonoBehaviour {
  public NoteType type;
  public Image TypeImg;
  public int val;
  public Text ValTxt;
  public int len;
  public Text LenTxt;
  public RectTransform back;
}


public enum NoteType { Empty=0, Note=1, Volume=2, Wave=3, Freq=4 };
/*
 
  [type] [val] [steps]


 note [freq] [steps]
 volume [val] [steps]
 wave [numwave]
 freq [val] [steps]
 */