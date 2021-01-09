using System.Collections.Generic;
using UnityEngine.UI;

public class MusicBlock : ListLine {
  public int blockNum;
  public Text BlockNumTxt;
  public string blockName;
  public Text BlockNameTxt;
  public int blockLen;
  public Text BlockLenTxt;
  public byte bpm;
  public List<BlockLine> Lines;
}
