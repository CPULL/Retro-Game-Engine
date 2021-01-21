using TMPro;
using UnityEngine.UI;

public class MusicLine : ListLine {
  public TextMeshProUGUI BlockID;
  public TextMeshProUGUI BlockName;
  public TextMeshProUGUI BlockLen;
  public Button Delete;
  public Button Up;
  public Button Down;
  public Button Edit;
  public Button Pick;

  public override string ToString() {
    return index + ") " + BlockID.text + " " + BlockName.text;
  }
}
