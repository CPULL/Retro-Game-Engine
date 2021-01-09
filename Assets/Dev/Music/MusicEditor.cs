using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicEditor : MonoBehaviour {
  public Audio sounds;
  public Transform Contents;
  public GameObject LineTemplate;
  public RectTransform SelectedCol;
  public Scrollbar scroll;


  private List<Block> blocks = null;


  private List<MusicLine> mlines = new List<MusicLine>();
  private List<BlockLine> blines = new List<BlockLine>();

  private Color32 SelectedColor = new Color32(36, 52, 36, 255);
  private Color32 Transparent = new Color32(0, 0, 0, 0);
  public Sprite[] NoteTypeSprites;
  Music music;

  MusicEditorStatus status = MusicEditorStatus.Idle;
  int row = 0;
  int col = 0;

  private void Start() {
    music = new Music() {
      name = "Music",
      bpm = 120,
      voices = new byte[] { 0, 255, 255, 255, 255, 255, 255, 255 },
      blocks = new List<int>()
    };
    blocks = new List<Block>();
    foreach (Transform t in Contents)
      Destroy(t.gameObject);
    for (int i = 0; i < InfoParts.Length; i++)
      InfoParts[i].SetActive(false);


    /*
    // FIXME do not do it at startup
    lines.Clear(); <-- this is bad, do not use it

    for (int i = 0; i < 65; i++) {
      BlockLine line = Instantiate(LineTemplate, Contents).GetComponent<BlockLine>();
      line.gameObject.SetActive(true);
      line.index = i;
      lines.Add(line);
      if (i == 0)
        line.IndexTxt.text = "";
      else
        line.IndexTxt.text = i.ToString("d2");
    }
    lines[0].Background.color = SelectedColor;
    */
  }

  int len = 1;
  public Text LengthText;
  public void ChangeLength(bool up) {
    if (up && len < 16) len++;
    if (!up && len > 1) len--;
    LengthText.text = " Length: " + len;
  }

  float autoRepeat = 0;
  private void Update() {
    bool update = false;
    autoRepeat -= Time.deltaTime;

    if (status == MusicEditorStatus.BlockEdit) {
      if (Input.GetKeyDown(KeyCode.LeftArrow) && col > 0) { col--; update = true; autoRepeat = .25f; }
      if (Input.GetKeyDown(KeyCode.RightArrow) && col < 7) { col++; update = true; autoRepeat = .25f; }
      if (Input.GetKey(KeyCode.UpArrow) && blines != null && row > 0 && autoRepeat < 0) { row--; update = true; autoRepeat = .1f; }
      if (Input.GetKey(KeyCode.DownArrow) && blines != null && row < blines.Count - 1 && autoRepeat < 0) { row++; update = true; autoRepeat = .1f; }
      if (Input.GetKeyDown(KeyCode.PageUp)) ChangeLength(true);
      if (Input.GetKeyDown(KeyCode.PageDown)) ChangeLength(false);

    }
    else if (status == MusicEditorStatus.Music) {
      if (Input.GetKey(KeyCode.UpArrow) && mlines != null && row > 0 && autoRepeat < 0) { row--; update = true; autoRepeat = .1f; }
      if (Input.GetKey(KeyCode.DownArrow) && mlines != null && row < mlines.Count - 1 && autoRepeat < 0) { row++; update = true; autoRepeat = .1f; }
    }


    if (status == MusicEditorStatus.BlockEdit) {
      BlockLine l = blines[row];
      // Space change type
      if (Input.GetKeyDown(KeyCode.Space)) {
        int t = (int)l.note[col].type;
        t++;
        if (t == 5) t = 0;
        l.note[col].type = (NoteType)t;
        l.note[col].TypeImg.sprite = NoteTypeSprites[t];
      }
      // Piano keys
      for (int i = 0; i < keyNotes.Length; i++) {
        if (Input.GetKeyDown(keyNotes[i])) {
          // Set the current cell as note with the given note/frequency, update the text to be the note notation
          l.note[col].TypeImg.sprite = NoteTypeSprites[1];
          l.note[col].ValTxt.text = noteNames[i + 24];
          l.note[col].val = freqs[i + 24];
          l.note[col].len = len;
          l.note[col].LenTxt.text = len.ToString();
          l.note[col].back.sizeDelta = new Vector2(38, len * 32);
          // Move to the next row
          if (row + len < 64) { row += len; update = true; }
          // Play the actual sound (find the wave that should be used, if none is defined use a basic triangle wave)
          sounds.Play(0, freqs[i + 24], .25f);
        }
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

      SelectRow(row);
    }
  }


  void SelectRow(int line) {
    if (status == MusicEditorStatus.Music) {
      int max = mlines.Count;
      for (int i = 0; i < max; i++)
        mlines[i].Background.color = Transparent;
      mlines[line].Background.color = SelectedColor;
    }
    else if (status == MusicEditorStatus.BlockEdit) {
      int max = blines.Count;
      for (int i = 0; i < max; i++)
        blines[i].Background.color = Transparent;
      blines[line].Background.color = SelectedColor;
    }
  }

  public GameObject TitleMusic;
  public GameObject TitleBlock;

  public InputField NameInput;
  public Text NumVoicesTxt;
  public GameObject[] InfoParts;
  public Text[] Infos;

  public GameObject MusicLineTempate;
  public GameObject CreateNewBlockInMusic;

  public void Music() { // Show what we have as music
    status = MusicEditorStatus.Music;
    foreach (Transform t in Contents)
      Destroy(t.gameObject);

    TitleMusic.SetActive(true);
    TitleBlock.SetActive(false);
    SelectedCol.gameObject.SetActive(false);

    NameInput.text = music.name;
    int numv = 0;
    for (int i = 0; i < music.voices.Length; i++)
      if (music.voices[i] != 255) numv++;
    NumVoicesTxt.text = " # Voices: " + numv;
    // FIXME assign the channels
    Infos[0].text = music.blocks.Count + " Lenght";
    Infos[1].text = "??? Waveforms";
    Infos[2].text = "??? Blocks";

    // FIXME
    // ----
    // Selected block (with input and dropdown to change it)

    int pos = 1;
    mlines.Clear();
    foreach (int bi in music.blocks) {
      Block b = null;
      for (int i = 0; i < blocks.Count; i++)
        if (blocks[i].id == bi) {
          b = blocks[i];
          break;
        }

      GameObject line = Instantiate(MusicLineTempate, Contents);
      MusicLine ml = line.GetComponent<MusicLine>();
      ml.index = pos;
      ml.IndexTxt.text = pos.ToString();
      ml.BlockID.text = bi.ToString();
      ml.BlockName.text = b.name;
      ml.BlockLen.text = (b.ch0 == null ? 0 : b.ch0.Count).ToString();
      ml.Delete.onClick.AddListener(() => RemoveCurrentMusicLine(ml));
      ml.Up.onClick.AddListener(() => MoveCurrentMusicLineUp(ml));
      ml.Down.onClick.AddListener(() => MoveCurrentMusicLineDown(ml));
      ml.Edit.onClick.AddListener(() => EditCurrentMusicLine(ml));
      mlines.Add(ml);
    }

    for (int i = 0; i < InfoParts.Length; i++)
      InfoParts[i].SetActive(i < 5);
    InfoParts[InfoParts.Length - 1].SetActive(true); // Filler

    Instantiate(CreateNewBlockInMusic, Contents).SetActive(true);
  }

  public void ChangeMusicVoices(bool up) {
    int numv = 0;
    for (int i = 0; i < music.voices.Length; i++)
      if (music.voices[i] != 255) numv++;
    if (up && numv < 8) numv++;
    if (!up && numv > 1) numv--;
    for (int i = 0; i < 8; i++)
      music.voices[i] = (byte)((i < numv) ? i : 255);
    NumVoicesTxt.text = " # Voices: " + numv;
  }

  public void AddNewBlockInMusic() {
    // Each block should have the ID (hex number), and a name. Remove, MoveUp, Down, Edit
    music.blocks.Add(-1);
    Transform last = Contents.GetChild(Contents.childCount - 1);
    last.SetParent(null);
    GameObject line = Instantiate(MusicLineTempate, Contents);
    line.SetActive(true);
    MusicLine ml = line.GetComponent<MusicLine>();
    ml.index = music.blocks.Count;
    ml.IndexTxt.text = music.blocks.Count.ToString();
    ml.BlockID.text = "";
    ml.BlockName.text = "<i>empty</i> (" + ml.index + ")";
    ml.BlockLen.text = "0";
    ml.Delete.onClick.AddListener(() => RemoveCurrentMusicLine(ml));
    ml.Up.onClick.AddListener(() => MoveCurrentMusicLineUp(ml));
    ml.Down.onClick.AddListener(() => MoveCurrentMusicLineDown(ml));
    ml.Edit.onClick.AddListener(() => EditCurrentMusicLine(ml));
    last.SetParent(Contents);
    mlines.Add(ml);

  }

  public void RemoveCurrentMusicLine(MusicLine line) {
    int pos = -1;
    for (int i = 0; i < mlines.Count; i++)
      if (mlines[i] == line) {
        pos = i;
        break;
      }
    if (pos == -1) return;

    row = pos - 1;
    if (row < 0) row = 0;
    music.blocks.RemoveAt(pos);
    Destroy(line.gameObject);
    mlines.RemoveAt(pos);
    for (int i = 0; i < music.blocks.Count; i++)
      mlines[i].Background.color = Transparent;
    if (row >= 0) SelectRow(row);
  }

  public void MoveCurrentMusicLineUp(MusicLine line) {
    int pos = -1;
    for (int i = 0; i < mlines.Count; i++)
      if (mlines[i] == line) {
        pos = i;
        break;
      }
    if (pos < 1) return;

    MusicLine tmp = mlines[pos - 1];
    mlines[pos - 1] = mlines[pos];
    mlines[pos] = tmp;
    Contents.GetChild(pos).SetSiblingIndex(pos - 1);
  }

  public void MoveCurrentMusicLineDown(MusicLine line) {
    int pos = -1;
    for (int i = 0; i < mlines.Count; i++)
      if (mlines[i] == line) {
        pos = i;
        break;
      }
    if (pos == -1 || pos + 1 >= mlines.Count) return;

    MusicLine tmp = mlines[pos + 1];
    mlines[pos + 1] = mlines[pos];
    mlines[pos] = tmp;
    Contents.GetChild(pos).SetSiblingIndex(pos + 1);
  }

  public void EditCurrentMusicLine(MusicLine line) {

  }

  public void PickBlock() {

  }

  // FIXME I need to divide the block data from the display data. Block data should never be destroyed, while UI data can be destroyed all the times

  public void Blocks() { // Show a list of blocks

  }

  public void Block() { // Show the current block

  }

  public void Waves() { // Show a list of waves

  }


  readonly KeyCode[] keyNotes = new KeyCode[] {
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

  readonly string[] noteNames = new string[] {
    "C2", "C2#",
    "D2", "E2b",
    "E2",
    "F2", "F2#",
    "G2", "G2#",
    "A3", "B2b",
    "B2",
    "C3", "C3#",
    "D3", "E3b",
    "E3",
    "F3", "F3#",
    "G3", "G3#",
    "A3", "B3b",
    "B3",
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
    "C6", "C6#",
    "D6", "E6b",
    "E6",
    "F6", "F6#",
    "G6", "G6#",
    "A6",
    "C7", "C7#",
    "D7", "E7b",
    "E7",
    "F7", "F7#",
    "G7", "G7#",
    "A7",
  };

  readonly int[] freqs = new int[] {
    65, 69,
    73, 77,
    82,
    87, 92,
    98, 103,
    110, 116,
    123,

    130, 138,
    146, 155,
    164,
    174, 185,
    196, 207,
    220, 233,
    246,

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

    1046, 1108,
    1174, 1244, 
    1318,
    1396, 1479,
    1567, 1661,
    1760, 1864,
    1975,

    2093, 2217,
    2349, 2489,
    2637,
    2793, 2959,
    3135, 3322,
    3520, 3729,
    3951,
  };
}

public enum MusicEditorStatus {
  Idle, Music, BlockList, BlockEdit, Waveforms
}


public struct Wave {
  public string name;
  public byte wave;
  public byte phase1;
  public byte phase2;
  public byte a;
  public byte d;
  public byte s;
  public byte r;
}


public class Music {
  public string name;
  public int bpm;
  public byte[] voices;
  public List<int> blocks;
}

public class Block {
  public int id;
  public string name;
  public int bpm;
  public int numVoices;
  public List<BlockNote> ch0;
  public List<BlockNote> ch1;
  public List<BlockNote> ch2;
  public List<BlockNote> ch3;
  public List<BlockNote> ch4;
  public List<BlockNote> ch5;
  public List<BlockNote> ch6;
  public List<BlockNote> ch7;
}

public class BlockNote {
  public NoteType type;
  public int val;
  public int len;
}


/*
 
Music->blockrefList 
 
block->notelist
 
 */


