using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicEditor : MonoBehaviour {
  public Audio sounds;
  public Transform Contents;
  public GameObject LineTemplate;
  public RectTransform SelectedCol;
  public Scrollbar scroll;
  readonly private List<MusicLine> lines = new List<MusicLine>();
  private Color32 SelectedColor = new Color32(36, 52, 36, 255);
  private Color32 Transparent = new Color32(0, 0, 0, 0);
  public Sprite[] NoteTypeSprites;
  

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

  int len = 1;
  float autoRepeat = 0;
  private void Update() {
    bool update = false;
    autoRepeat -= Time.deltaTime;
    if (Input.GetKeyDown(KeyCode.LeftArrow) && col > 0 && autoRepeat < 0)  { col--; update = true; autoRepeat = .25f; }
    if (Input.GetKeyDown(KeyCode.RightArrow) && col < 7 && autoRepeat < 0) { col++; update = true; autoRepeat = .25f; }
    if (Input.GetKey(KeyCode.UpArrow) && row > 0)    { row--; update = true; }
    if (Input.GetKey(KeyCode.DownArrow) && row < 64) { row++; update = true; }


    // Space change type
    if (Input.GetKeyDown(KeyCode.Space)) {
      int t = (int)lines[row].note[col].type;
      t++;
      if (t == 5) t = 0;
      lines[row].note[col].type = (NoteType)t;
      lines[row].note[col].TypeImg.sprite = NoteTypeSprites[t];
    }

    for (int i = 0; i < keyNotes.Length; i++) {
      if (Input.GetKeyDown(keyNotes[i])) {
        // Set the current cell as note with the given note/frequency, update the text to be the note notation
        lines[row].note[col].TypeImg.sprite = NoteTypeSprites[1];
        lines[row].note[col].ValTxt.text = noteNames[i];
        lines[row].note[col].val = freqs[i];
        lines[row].note[col].len = len;
        lines[row].note[col].LenTxt.text = len.ToString();
        lines[row].note[col].back.sizeDelta = new Vector2(38, len * 32);
        // Move to the next row
        if (row + len < 64) { row += len; update = true; }
        // Play the actual sound (find the wave that should be used, if none is defined use a basic triangle wave)
        sounds.Play(0, freqs[i], .25f);
      }
    }


    // top 2 rows -> set note
    // pgup/dwn -> change instrument/volume/freq/note
    // what to increse/decrease length?

    if (update) {
      // Scroll if needed
      if (row < 13) scroll.value = 1;
      else if (row > 48) scroll.value = 0;
      else scroll.value = -0.0276f * row + 1.333333333333333f;

      SelectedCol.anchoredPosition = new Vector3(48 + col * 142, 30, 0);
      lines[row].Background.color = SelectedColor;
      if (row > 0) lines[row - 1].Background.color = Transparent;
      if (row < lines.Count-1) lines[row + 1].Background.color = Transparent;
    }
  }



  KeyCode[] keyNotes = new KeyCode[] {
    KeyCode.Q, KeyCode.Alpha2, // C4 C4#
    KeyCode.W, KeyCode.Alpha3, // D4 E4b
    KeyCode.E,                 // E4
    KeyCode.R, KeyCode.Alpha5, // F4 F4#
    KeyCode.T, KeyCode.Alpha6, // G4 G4#
    KeyCode.Y, KeyCode.Alpha7, // A4 B4b
    KeyCode.U,                 // B4

    KeyCode.I, KeyCode.Alpha9, // C5 C5#
    KeyCode.O, KeyCode.Alpha0, // D5 E5b
    KeyCode.P,                 // E5
    KeyCode.LeftBracket, KeyCode.Equals, // F5 F5#
    KeyCode.RightBracket, KeyCode.Backslash, // G5 G5#
    KeyCode.Return,            // A5
  };

  string[] noteNames = new string[] {
    "C4", "C4#",
    "D4", "E4b",
    "E4",
    "F4", "F4#",
    "G4", "G4#",
    "A4", "B4b",
    "B4",
    "C5", "C5#",
    "D5", "E5b",
    "E5",
    "F5", "F5#",
    "G5", "G5#",
    "A5",
  };

  int[] freqs = new int[] {
    /*
    130, 138,
    146, 155,
    164,
    174, 185,
    196, 207,
    220, 233,
    246,
    */
    261, 277,
    293, 311,
    329,
    349, 369,
    392, 415,
    440, 466,
    493,
    523, 554,
    587, 622,
    659,
    698, 739,
    783, 830,
    880, 932,
    987,
  };
}

public enum MusicEditorStatus {
  Idle, BlockList, Music, BlockEdit, Waveforms
}

public class MusicBlock {
  int index;
  int numVoices;
  List<MusicLine> Lines;
}

