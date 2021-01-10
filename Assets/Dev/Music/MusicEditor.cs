using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicEditor : MonoBehaviour {
  public Audio sounds;
  public Transform Contents;
  public RectTransform SelectedCol;
  public Scrollbar scroll;

  private List<Block> blocks = null;
  private List<Wave> waves = null;
  private Block currentBlock = null;
  private Wave currentWave = null;
  private List<MusicLine> mlines = new List<MusicLine>();
  private List<BlockLine> blines = new List<BlockLine>();
  private List<BlockListLine> bllines = new List<BlockListLine>();
  private List<WaveLine> wlines = new List<WaveLine>();

  private Color32 SelectedColor = new Color32(36, 52, 36, 255);
  private Color32 Transparent = new Color32(0, 0, 0, 0);
  public Sprite[] NoteTypeSprites;
  public Sprite[] WaveSprites;
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
    waves = new List<Wave>();
    foreach (Transform t in Contents)
      Destroy(t.gameObject);
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
      if (Input.GetKeyDown(KeyCode.PageUp)) ChangeBlockLength(true);
      if (Input.GetKeyDown(KeyCode.PageDown)) ChangeBlockLength(false);

    }
    else if (status == MusicEditorStatus.Music) {
      if (Input.GetKey(KeyCode.UpArrow) && mlines != null && row > 0 && autoRepeat < 0) { row--; update = true; autoRepeat = .1f; }
      if (Input.GetKey(KeyCode.DownArrow) && mlines != null && row < mlines.Count - 1 && autoRepeat < 0) { row++; update = true; autoRepeat = .1f; }
    }
    else if (status == MusicEditorStatus.Waveforms) {
      if (Input.GetKey(KeyCode.UpArrow) && wlines != null && row > 0 && autoRepeat < 0) { row--; update = true; autoRepeat = .1f; }
      if (Input.GetKey(KeyCode.DownArrow) && wlines != null && row < wlines.Count - 1 && autoRepeat < 0) { row++; update = true; autoRepeat = .1f; }
    }
    else if (status == MusicEditorStatus.BlockList) {
      if (Input.GetKey(KeyCode.UpArrow) && bllines != null && row > 0 && autoRepeat < 0) { row--; update = true; autoRepeat = .1f; }
      if (Input.GetKey(KeyCode.DownArrow) && bllines != null && row < bllines.Count - 1 && autoRepeat < 0) { row++; update = true; autoRepeat = .1f; }
    }


    if (status == MusicEditorStatus.BlockEdit && row > -1 && row < blines.Count && !inputsSelected) {
      BlockLine l = blines[row];
      // Space change type
      if (Input.GetKeyDown(KeyCode.Space)) {
        int t = (int)l.note[col].type;
        t++;
        if (t == 5) t = 0;
        l.note[col].type = (NoteType)t;
        l.note[col].TypeImg.sprite = NoteTypeSprites[t];
        currentBlock.chs[col][row].Set(l.note[col]);
      }
      // Piano keys
      for (int i = 0; i < keyNotes.Length; i++) {
        if (Input.GetKeyDown(keyNotes[i])) {
          // Set the current cell as note with the given note/frequency, update the text to be the note notation
          l.note[col].type = NoteType.Note;
          l.note[col].TypeImg.sprite = NoteTypeSprites[1];
          l.note[col].ValTxt.text = noteNames[i + 24];
          l.note[col].val = freqs[i + 24];
          l.note[col].len = noteLen;
          l.note[col].LenTxt.text = noteLen.ToString();
          l.note[col].back.sizeDelta = new Vector2(38, noteLen * 32);
          currentBlock.chs[col][row].Set(l.note[col]);
          // Move to the next row
          if (row + noteLen < currentBlock.chs[0].Count) { row += noteLen; update = true; }
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


      CurrentBlock.text = "???";
      BlockLengthText.text = "???";
      BlockBPMText.text = "???";

      int id = music.blocks[line];
      foreach(Block b in blocks) {
        if (b.id == id) {
          CurrentBlock.text = "[" + id + "] " + b.name;
          BlockLengthText.text = " Block Len: " + b.chs[0].Count.ToString();
          BlockBPMText.text = " Block BPM: " + b.bpm.ToString();
          break;
        }
      }

    }
    else if (status == MusicEditorStatus.BlockEdit) {
      int max = blines.Count;
      for (int i = 0; i < max; i++)
        blines[i].Background.color = Transparent;
      blines[line].Background.color = SelectedColor;
    }
    else if (status == MusicEditorStatus.BlockList) {
      int max = bllines.Count;
      for (int i = 0; i < max; i++)
        bllines[i].Background.color = Transparent;
      bllines[line].Background.color = SelectedColor;
    }
    else if (status == MusicEditorStatus.Waveforms) {
      int max = wlines.Count;
      for (int i = 0; i < max; i++)
        wlines[i].Background.color = Transparent;
      wlines[line].Background.color = SelectedColor;
    }
  }

  public GameObject TitleMusic;
  public GameObject TitleBlock;
  public GameObject TitleBlockList;
  public GameObject TitleWaves;

  public InputField NameInput;
  public Text NumVoicesTxt;

  public Text TotalNumBlocks;
  public Text NumWaves;
  public Text NumBlocks;
  public Text CurrentBlock;
  public InputField BlockNameInput;
  public InputField WaveNameInput;

  public GameObject MusicLineTempate;
  public GameObject BlockLineTempate;
  public GameObject BlockListLineTemplate;
  public GameObject WaveLineTemplate;
  public GameObject CreateNewBlockInMusic;
  public GameObject CreateNewBlockInList;
  public GameObject CreateNewWaveInList;

  bool inputsSelected = false;

  #region Music

  public void Music() { // Show what we have as music
    status = MusicEditorStatus.Music;
    foreach (Transform t in Contents)
      Destroy(t.gameObject);

    TitleMusic.SetActive(true);
    TitleBlock.SetActive(false);
    TitleBlockList.SetActive(false);
    TitleWaves.SetActive(false);
    SelectedCol.gameObject.SetActive(false);

    NameInput.text = music.name;
    int numv = 0;
    for (int i = 0; i < music.voices.Length; i++)
      if (music.voices[i] != 255) numv++;
    NumVoicesTxt.text = " # Voices: " + numv;
    TotalNumBlocks.text = music.blocks.Count + " Lenght";
    NumWaves.text = "??? Waveforms";
    NumBlocks.text = "??? Blocks";

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
      ml.BlockLen.text = b.chs[0].Count.ToString();
      ml.Delete.onClick.AddListener(() => RemoveCurrentMusicLine(ml));
      ml.Up.onClick.AddListener(() => MoveCurrentMusicLineUp(ml));
      ml.Down.onClick.AddListener(() => MoveCurrentMusicLineDown(ml));
      ml.Edit.onClick.AddListener(() => EditCurrentMusicLineBlock(ml));
      ml.Pick.onClick.AddListener(() => PickCurrentMusicLineBlock(ml));
      mlines.Add(ml);
    }

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
    ml.Edit.onClick.AddListener(() => EditCurrentMusicLineBlock(ml));
    ml.Pick.onClick.AddListener(() => PickCurrentMusicLineBlock(ml));
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

  public void EditCurrentMusicLineBlock(MusicLine line) {
    // pick the current block and show it, if missing create a new one
    int pos = -1;
    for (int i = 0; i < mlines.Count; i++)
      if (mlines[i] == line) {
        pos = i;
        break;
      }
    if (pos == -1 || music.blocks[pos] == -1) return;
    int id = music.blocks[pos];
    foreach (Block b in blocks)
      if (b.id == id) {
        currentBlock = b;
        ShowBlock();
        return;
      }
  }

  public void PickCurrentMusicLineBlock(MusicLine line) {
    // pick the current block and show it, if missing create a new one
    inMusicLine = -1;
    for (int i = 0; i < mlines.Count; i++)
      if (mlines[i] == line) {
        inMusicLine = i;
        break;
      }
    if (inMusicLine == -1) return;
    BlockPickContainer.parent.gameObject.SetActive(true);
    PickBlockSetup();
  }

  public void UpdateMusicName(bool completed) {
    music.name = NameInput.text;
    inputsSelected = !completed;
  }

  #endregion

  #region Block
  public Transform BlockPickContainer;
  public GameObject SelctBlockButton;

  int inMusicLine = -1;
  public void PickBlock() {
    inMusicLine = -1;
    bool active = !BlockPickContainer.parent.gameObject.activeSelf;
    BlockPickContainer.parent.gameObject.SetActive(active);
    if (active) PickBlockSetup();
  }

  void PickBlockSetup() {
    if (blocks.Count == 0) {
      BlockPickContainer.parent.gameObject.SetActive(false);
      return;
    }

    foreach (Transform t in BlockPickContainer)
      Destroy(t.gameObject);

    foreach(Block b in blocks) {
      GameObject sbb = Instantiate(SelctBlockButton, BlockPickContainer);
      sbb.SetActive(true);
      sbb.transform.GetChild(0).GetComponent<Text>().text = "[" + b.id + "] " + b.name;
      sbb.GetComponent<Button>().onClick.AddListener(() => { DoPickBlock(b); });
    }
  }

  private void DoPickBlock(Block b) {
    BlockPickContainer.parent.gameObject.SetActive(false);
    currentBlock = b;

    if (inMusicLine == -1)
      ShowBlock();
    else {
      BlockNameInput.text = currentBlock.name;
      BlockLengthText.text = " Block Len: " + currentBlock.chs[0].Count;
      BlockBPMText.text = " Block BPM: " + currentBlock.bpm;
      music.blocks[inMusicLine] = b.id;
      MusicLine ml = mlines[inMusicLine];
      ml.BlockID.text = b.id.ToString();
      ml.BlockName.text = b.name;
      ml.BlockLen.text = b.chs[0].Count.ToString();
    }
  }

  public void CreateBlock() {
    int id = 1;
    if (blocks.Count > 0) id = blocks[blocks.Count - 1].id + 1;
    Block b = new Block() { id = id, name = "New BLock", bpm = music.bpm };
    int numv = 0;
    for (int i = 0; i < music.voices.Length; i++)
      if (music.voices[i] != 255) numv++;

    b.chs = new List<BlockNote>[8];
    for (int i = 0; i < 8; i++) {
      b.chs[i] = new List<BlockNote>();
    }
    for (int n = 0; n < 64; n++) {
      for (int j = 0; j < 8; j++) {
        b.chs[j].Add(new BlockNote()); 
      }
    }
    blocks.Add(b);
    currentBlock = b;
    ShowBlock(); // Make it visible
    SelectRow(0);
  }

  public Text BlockLengthText;
  public void ChangeBlockLength(bool up) {
    if (CurrentBlock == null || status != MusicEditorStatus.BlockEdit) return;
    Block b = currentBlock;
    int len = b.chs[0].Count;
    if (up && len < 128) len++;
    if (!up && len > 1) len--;

    if (b.chs[0].Count < len) {
      for (int i = b.chs[0].Count; i <= len; i++) {
        for (int j = 0; j < 8; j++) {
          b.chs[j].Add(new BlockNote());
        }

        GameObject line = Instantiate(BlockLineTempate, Contents);
        BlockLine bl = line.GetComponent<BlockLine>();
        bl.index = i;
        bl.IndexTxt.text = i.ToString("d2");
        for (int j = 0; j < 8; j++)
          bl.note[j].SetZeroValues(NoteTypeSprites);
        blines.Add(bl);
      }
    }
    else {
      for (int i = b.chs[0].Count; i <= len; i++) {
        int pos = b.chs[0].Count - 1;
        for (int j = 0; j < 8; j++) {
          b.chs[j].RemoveAt(pos);
        }
        pos = blines.Count - 1;
        Destroy(blines[pos]);
        blines.RemoveAt(pos);
      }
    }
    BlockLengthText.text = " Block Len: " + len;
  }

  public Text BlockBPMText;
  public void ChangeBlockBPM(bool up) {
    if (CurrentBlock == null || status != MusicEditorStatus.BlockEdit) return;
    Block b = currentBlock;
    if (up && b.bpm < 280) b.bpm++;
    if (!up && b.bpm > 32) b.bpm--;
    BlockBPMText.text = " Block BPM: " + b.bpm;
  }

  int noteLen = 1;
  public Text NoteLengthText;
  public void ChangeNoteLength(bool up) {
    if (CurrentBlock == null || status != MusicEditorStatus.BlockEdit) return;
    if (up && noteLen < 16) noteLen++;
    if (!up && noteLen > 1) noteLen--;
    NoteLengthText.text = " Note Len: " + noteLen;
  }

  public void ShowBlock() { // Show the current block
    if (currentBlock == null) return;
    status = MusicEditorStatus.BlockEdit;
    foreach (Transform t in Contents)
      Destroy(t.gameObject);
    TitleMusic.SetActive(false);
    TitleBlock.SetActive(true);
    TitleBlockList.SetActive(false);
    TitleWaves.SetActive(false);
    SelectedCol.gameObject.SetActive(true);

    BlockNameInput.text = currentBlock.name;
    BlockLengthText.text = " Block Len: " + currentBlock.chs[0].Count;
    BlockBPMText.text = " Block BPM: " + currentBlock.bpm;

    blines.Clear();
    for (int i = 0; i < currentBlock.chs[0].Count; i++) {
      GameObject line = Instantiate(BlockLineTempate, Contents);
      line.SetActive(true);
      BlockLine bl = line.GetComponent<BlockLine>();
      bl.index = i;
      bl.IndexTxt.text = i.ToString("d2");
      for (int j = 0; j < 8; j++) {
        bl.note[j].SetValues(currentBlock.chs[j][i], NoteTypeSprites, freqs, noteNames);
      }
      blines.Add(bl);
    }
  }

  public void UpdateBlockName(bool completed) {
    if (currentBlock == null || status != MusicEditorStatus.BlockEdit) return;
    currentBlock.name = BlockNameInput.text;
    inputsSelected = !completed;
  }

  #endregion

  #region Block list

  public void Blocks() { // Show a list of blocks
    status = MusicEditorStatus.BlockList;
    foreach (Transform t in Contents)
      Destroy(t.gameObject);

    bllines.Clear();
    TitleMusic.SetActive(false);
    TitleBlock.SetActive(false);
    TitleBlockList.SetActive(true);
    TitleWaves.SetActive(false);
    SelectedCol.gameObject.SetActive(false);

    foreach (Block b in blocks) {
      GameObject line = Instantiate(BlockListLineTemplate, Contents);
      BlockListLine bll = line.GetComponent<BlockListLine>();
      bll.BlockID.text = b.id.ToString();
      bll.BlockName.text = b.name;
      bll.BlockLen.text = b.chs[0].Count.ToString();
      bll.BlockBPM.text = b.bpm.ToString();
      bll.Delete.onClick.AddListener(() => DeleteBlockFromList(b));
      bll.Edit.onClick.AddListener(() => EditBlockFromList(b));
      bllines.Add(bll);
    }

    Instantiate(CreateNewBlockInList, Contents).SetActive(true);
  }

  private void EditBlockFromList(Block b) {
    currentBlock = b;
    ShowBlock();
  }

  private void DeleteBlockFromList(Block b) {
    int id = b.id;
    blocks.Remove(b);
    for (int i = 0; i < music.blocks.Count; i++)
      if (music.blocks[i] == id) music.blocks[i] = -1;

    Blocks();
  }

  #endregion

  public void Waves() { // Show a list of waves
    status = MusicEditorStatus.Waveforms;
    foreach (Transform t in Contents)
      Destroy(t.gameObject);

    TitleMusic.SetActive(false);
    TitleBlock.SetActive(false);
    TitleBlockList.SetActive(false);
    TitleWaves.SetActive(true);
    SelectedCol.gameObject.SetActive(false);
    wlines.Clear();

    foreach (Wave w in waves) {
      GameObject line = Instantiate(WaveLineTemplate, Contents);
      WaveLine wl = line.GetComponent<WaveLine>();
      line.SetActive(true);
      wl.WaveID.text = w.id.ToString();
      wl.WaveName.text = w.name;
      wl.WaveType.text = w.wave.ToString();
      wl.WaveTypeImg.sprite = WaveSprites[(int)w.wave];
      wl.Delete.onClick.AddListener(() => DeleteWaveFromList(w));
      wl.Edit.onClick.AddListener(() => EditWaveFromList(w));
      wlines.Add(wl);
    }

    Instantiate(CreateNewWaveInList, Contents).SetActive(true);
  }

  private void EditWaveFromList(Wave w) {
    throw new NotImplementedException();
  }

  private void DeleteWaveFromList(Wave w) {
    throw new NotImplementedException();
  }

  public void CreateNewWave() {
    int id = waves.Count > 0 ? waves[waves.Count - 1].id + 1 : 1;
    Wave w = new Wave() { id = id, name = "No name" };
    waves.Add(w);
    currentWave = w;
    Waves();
    ShowWave();
  }

  void ShowWave() {

  }

  public void CopyFromWaveEditor() {

  }

  public void CopyToWaveEditor() {

  }

  public void UpdateWaveName(bool completed) {
    if (currentWave == null) return;
    currentWave.name = WaveNameInput.text;
    inputsSelected = !completed;
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


public class Wave {
  public int id;
  public string name;
  public Waveform wave;
  public float phase;
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
  public List<BlockNote>[] chs;
}

public class BlockNote {
  public NoteType type;
  public int val;
  public int len;

  internal void Set(MusicNote note) {
    type = note.type;
    val = note.val;
    len = note.len;
  }
}


/*

changing music name has no effect
add name of current block editable
have a valid way to add/remove voices, the block voices should be changed accordingly
add multiple selection of rows to enalbe cleanup and copy/paste
 
 */


