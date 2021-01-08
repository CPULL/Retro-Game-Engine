using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicEditor : MonoBehaviour {
  public Transform Contents;
  public GameObject LineTemplate;
  public RectTransform SelectedCol;
  public Scrollbar scroll;
  readonly private List<MusicLine> lines = new List<MusicLine>();
  private Color32 SelectedColor = new Color32(36, 52, 36, 255);
  private Color32 Transparent = new Color32(0, 0, 0, 0);

  MusicEditorStatus status = MusicEditorStatus.Idle;
  int row = 0;
  int col = 0;

  private void Start() {
    foreach (Transform t in Contents)
      Destroy(t.gameObject);
    lines.Clear();
    for (int i = 0; i < 65; i++) {
      MusicLine line = Instantiate(LineTemplate, Contents).GetComponent<MusicLine>();
      line.gameObject.SetActive(true);
      line.index = i;
      lines.Add(line);
      if (i == 0)
        line.IndexTxt.text = "";
      else
        line.IndexTxt.text = i.ToString("d2");
    }
    lines[0].Background.color = SelectedColor;
  }

  private void Update() {
    bool update = false;
    if (Input.GetKeyDown(KeyCode.LeftArrow) && col > 0)  { col--; update = true; }
    if (Input.GetKeyDown(KeyCode.RightArrow) && col < 7) { col++; update = true; }
    if (Input.GetKeyDown(KeyCode.UpArrow) && row > 0)    { row--; update = true; } // Scroll if needed
    if (Input.GetKeyDown(KeyCode.DownArrow) && row < 65) { row++; update = true; }


    // top 2 rows -> set note
    // Space change type
    // pgup/dwn -> change instrument/volume/freq/note
    // what to increse/decrease length?

    if (update) {
      if (row < 13) scroll.value = 1;
      else if (row > 48) scroll.value = 0;
      else scroll.value = -0.0276f * row + 1.333333333333333f;


      SelectedCol.anchoredPosition = new Vector3(48 + col * 142, 30, 0);
      lines[row].Background.color = SelectedColor;
      if (row > 0) lines[row - 1].Background.color = Transparent;
      if (row < lines.Count-1) lines[row + 1].Background.color = Transparent;
    }
  }

}

public enum MusicEditorStatus {
  Idle, BlockList, Music, BlockEdit, Waveforms
}

public class MusicBlock {
  int index;
  int numVoices;
  List<MusicLine> Lines;
}

