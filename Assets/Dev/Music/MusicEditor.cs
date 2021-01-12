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
  readonly private List<MusicLine> mlines = new List<MusicLine>();
  readonly private List<BlockLine> blines = new List<BlockLine>();
  readonly private List<BlockListLine> bllines = new List<BlockListLine>();
  readonly private List<WaveLine> wlines = new List<WaveLine>();

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
      voices = new byte[] { 0, 1, 2, 3, 255, 255, 255, 255 },
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

    if (playing) return;

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
          sounds.Play(col, freqs[i + 24], .25f);
        }
      }
    }

    if (status == MusicEditorStatus.Waveforms && row > -1 && row < wlines.Count && !inputsSelected) {
      // Piano keys
      for (int i = 0; i < keyNotes.Length; i++) {
        if (Input.GetKeyDown(keyNotes[i])) {
          // Set the current cell as note with the given note/frequency, update the text to be the note notation
          sounds.Play(0, freqs[i + 24], .25f);
        }
      }
    }


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
    if (playing) return;
    if (status == MusicEditorStatus.Music) {
      if (mlines.Count == 0) return;
      for (int i = 0; i < mlines.Count; i++)
        mlines[i].Background.color = Transparent;
      row = line;
      if (row < 0) row = 0;
      if (row >= mlines.Count) row = mlines.Count - 1;
      mlines[line].Background.color = SelectedColor;
      BlockNameInput.SetTextWithoutNotify("???");
      CurrentBlock.text = "???";
      BlockLengthText.text = "???";
      BlockBPMText.text = "???";
      int id = music.blocks[line];
      foreach (Block b in blocks) {
        if (b.id == id) {
          currentBlock = b;
          ShowBlockInfo();
          break;
        }
      }
    }
    else if (status == MusicEditorStatus.BlockEdit) {
      if (blines.Count == 0) return;
      row = line;
      if (row < 0) row = 0;
      if (row >= blines.Count) row = blines.Count - 1;
      for (int i = 0; i < blines.Count; i++)
        blines[i].Background.color = Transparent;
      blines[line].Background.color = SelectedColor;

      List<BlockNote> notes = currentBlock.chs[col];
      for (int i = row; i >= 0; i--) {
        if (notes[i].type == NoteType.Wave) {
          Wave w = null;
          for (int widx = 0; widx < waves.Count; widx++) {
            if (waves[widx].id== notes[i].val) {
              w = waves[widx];
            }
          }
          if (w != null) {
            sounds.Wave(col, w.wave, w.phase);
            sounds.ADSR(col, w.a, w.d, w.s, w.r);
            if (w.rawPCM != null) sounds.Wave(col, w.rawPCM);
          }
        }
      }
    }
    else if (status == MusicEditorStatus.BlockList) {
      if (bllines.Count == 0) return;
      row = line;
      if (row < 0) row = 0;
      if (row >= bllines.Count) row = bllines.Count - 1;
      for (int i = 0; i < bllines.Count; i++)
        bllines[i].Background.color = Transparent;
      bllines[line].Background.color = SelectedColor;
      currentBlock = blocks[line];
      ShowBlockInfo();
    }
    else if (status == MusicEditorStatus.Waveforms) {
      if (wlines.Count == 0) return;
      row = line;
      if (row < 0) row = 0;
      if (row >= wlines.Count) row = wlines.Count - 1;
      for (int i = 0; i < wlines.Count; i++)
        wlines[i].Background.color = Transparent;
      wlines[line].Background.color = SelectedColor;
      currentWave = waves[line];
      ShowWave();
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
  public Text WaveTypeName;
  public Image WaveTypeImg;

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
    NumVoicesTxt.text = " # Voices: " + music.numVoices;
    TotalNumBlocks.text = music.blocks.Count + " Lenght";
    NumWaves.text = waves.Count + " Waveforms";
    NumBlocks.text = blocks.Count + " Blocks";

    int pos = 1;
    mlines.Clear();
    for (int i = 0; i < music.blocks.Count; i++) {
      int bi = music.blocks[i];
      Block b = null;
      for (int j = 0; j < blocks.Count; j++)
        if (blocks[j].id == bi) {
          b = blocks[j];
          break;
        }

      GameObject line = Instantiate(MusicLineTempate, Contents);
      line.SetActive(true);
      MusicLine ml = line.GetComponent<MusicLine>();
      ml.index = pos;
      ml.IndexTxt.text = pos.ToString();
      ml.BlockID.text = bi.ToString();
      ml.BlockName.text = bi == -1 || b == null ? "" : b.name;
      ml.BlockLen.text = bi == -1 || b == null ? "0" : b.chs[0].Count.ToString();
      ml.Delete.onClick.AddListener(() => RemoveCurrentMusicLine(ml));
      ml.Up.onClick.AddListener(() => MoveCurrentMusicLineUp(ml));
      ml.Down.onClick.AddListener(() => MoveCurrentMusicLineDown(ml));
      int linenum = i;
      ml.Edit.onClick.AddListener(() => EditCurrentMusicLineBlock(linenum));
      ml.Pick.onClick.AddListener(() => PickCurrentMusicLineBlock(linenum));
      ml.LineButton.onClick.AddListener(() => SelectRow(linenum));
      pos++;
      mlines.Add(ml);
    }
    Instantiate(CreateNewBlockInMusic, Contents).SetActive(true);
  }

  public void ChangeMusicVoices(bool up) {
    int numv = music.numVoices;
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
    int linenum = mlines.Count;
    ml.Edit.onClick.AddListener(() => EditCurrentMusicLineBlock(linenum));
    ml.Pick.onClick.AddListener(() => PickCurrentMusicLineBlock(linenum));
    ml.LineButton.onClick.AddListener(() => SelectRow(linenum));
    last.SetParent(Contents);
    mlines.Add(ml);
    SelectRow(mlines.Count - 1);
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

  public void EditCurrentMusicLineBlock(int pos) {
    // pick the current block and show it, if missing create a new one
    if (pos == -1 || music.blocks[pos] == -1) return;
    int id = music.blocks[pos];
    foreach (Block b in blocks)
      if (b.id == id) {
        currentBlock = b;
        ShowBlock();
        return;
      }
  }

  public void PickCurrentMusicLineBlock(int rowp) {
    // pick the current block and show it, if missing create a new one
    row = rowp;
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
  public GameObject SelectBlockButton;

  public void PickBlock() {
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
      GameObject sbb = Instantiate(SelectBlockButton, BlockPickContainer);
      sbb.SetActive(true);
      sbb.transform.GetChild(0).GetComponent<Text>().text = "[" + b.id + "] " + b.name;
      sbb.GetComponent<Button>().onClick.AddListener(() => { DoPickBlock(b); });
    }
  }

  private void DoPickBlock(Block b) {
    BlockPickContainer.parent.gameObject.SetActive(false);
    currentBlock = b;

    if (status == MusicEditorStatus.Music) {
      music.blocks[row] = b.id;
      ShowBlockInfo();
      MusicLine ml = mlines[row];
      ml.BlockID.text = b.id.ToString();
      ml.BlockName.text = b.name;
      ml.BlockLen.text = b.chs[0].Count.ToString();
    }
    else
      ShowBlock();
  }

  public void CreateBlock() {
    int id = 1;
    if (blocks.Count > 0) id = blocks[blocks.Count - 1].id + 1;
    Block b = new Block() { id = id, name = "New Block", bpm = music.bpm };
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
    ShowBlockInfo();
    Blocks();
    SelectRow(bllines.Count - 1);
    NumBlocks.text = blocks.Count + " Blocks";
  }

  public Text BlockLengthText;
  public void ChangeBlockLength(bool up) {
    if (CurrentBlock == null) return;
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
        line.SetActive(true);
        BlockLine bl = line.GetComponent<BlockLine>();
        bl.index = i;
        bl.IndexTxt.text = i.ToString("d2");
        int linenum = i;
        bl.LineButton.onClick.AddListener(() => SelectRow(linenum));
        for (int j = 0; j < 8; j++)
          bl.note[j].SetZeroValues(NoteTypeSprites);
        blines.Add(bl);
      }
    }
    else {
      while (b.chs[0].Count > len) {
        int pos = b.chs[0].Count - 1;
        for (int j = 0; j < 8; j++) {
          b.chs[j].RemoveAt(pos);
        }
        pos = blines.Count - 1;
        if (pos > -1 && pos < blines.Count) {
          blines[pos].transform.SetParent(null);
          Destroy(blines[pos]);
          blines.RemoveAt(pos);
        }
      }
    }
    BlockLengthText.text = " Block Len: " + len;

    if (status == MusicEditorStatus.BlockList) {
      foreach(BlockListLine bll in bllines) {
        if (bll.BlockID.text == currentBlock.id.ToString()) {
          bll.BlockLen.text = len.ToString();
        }
      }
    }
    if (status == MusicEditorStatus.Music) {
      foreach(MusicLine ml in mlines) {
        if (ml.BlockID.text == currentBlock.id.ToString()) {
          ml.BlockLen.text = len.ToString();
        }
      }
    }
  }

  public Text BlockBPMText;
  public void ChangeBlockBPM(bool up) {
    if (CurrentBlock == null) return;
    Block b = currentBlock;
    if (up && b.bpm < 280) b.bpm++;
    if (!up && b.bpm > 32) b.bpm--;
    BlockBPMText.text = " Block BPM: " + b.bpm;
  }

  int noteLen = 1;
  public Text NoteLengthText;
  public void ChangeNoteLength(bool up) {
    if (CurrentBlock == null) return;
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

    ShowBlockInfo();
    blines.Clear();
    for (int i = 0; i < currentBlock.chs[0].Count; i++) {
      GameObject line = Instantiate(BlockLineTempate, Contents);
      line.SetActive(true);
      BlockLine bl = line.GetComponent<BlockLine>();
      bl.index = i;
      bl.IndexTxt.text = i.ToString("d2");
      int linenum = i;
      bl.LineButton.onClick.AddListener(() => SelectRow(linenum));
      for (int j = 0; j < music.numVoices; j++) {
        bl.note[j].SetValues(currentBlock.chs[j][i], NoteTypeSprites, freqs, noteNames);
      }
      for (int j = music.numVoices; j < 8; j++) {
        bl.note[j].Hide();
      }
      blines.Add(bl);
    }
  }

  void ShowBlockInfo() {
    if (currentBlock == null) return;
    BlockNameInput.SetTextWithoutNotify(currentBlock.name);
    CurrentBlock.text = "[" + currentBlock.id + "] " + currentBlock.name;
    BlockLengthText.text = " Block Len: " + currentBlock.chs[0].Count;
    BlockBPMText.text = " Block BPM: " + currentBlock.bpm;
  }

  public void UpdateBlockName(bool completed) {
    if (currentBlock == null) return;
    currentBlock.name = BlockNameInput.text;
    inputsSelected = !completed;

    if (status == MusicEditorStatus.BlockList) {
      foreach(BlockListLine bl in bllines) {
        if (bl.BlockID.text == currentBlock.id.ToString()) {
          bl.BlockName.text = currentBlock.name;
        }
      }
    }
    else if (status == MusicEditorStatus.Music) {
      foreach(MusicLine ml in mlines) {
        if (ml.BlockID.text == currentBlock.id.ToString()) {
          ml.BlockName.text = currentBlock.name;
        }
      }
    }
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

    int pos = 0;
    foreach (Block b in blocks) {
      GameObject line = Instantiate(BlockListLineTemplate, Contents);
      line.SetActive(true);
      BlockListLine bll = line.GetComponent<BlockListLine>();
      bll.BlockID.text = b.id.ToString();
      bll.BlockName.text = b.name;
      bll.BlockLen.text = b.chs[0].Count.ToString();
      bll.BlockBPM.text = b.bpm.ToString();
      bll.Delete.onClick.AddListener(() => DeleteBlockFromList(b));
      bll.Edit.onClick.AddListener(() => EditBlockFromList(b));
      int linenum = pos++;
      bll.LineButton.onClick.AddListener(() => SelectRow(linenum));
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

  #region Waves
  public Transform WavePickContainer;
  public GameObject SelectWaveButton;

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

    int pos = 0;
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
      int linenum = pos++;
      wl.LineButton.onClick.AddListener(() => SelectRow(linenum));
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
    SelectRow(waves.Count - 1);
    NumWaves.text = waves.Count + " Waveforms";
  }



  void ShowWave() {
    if (currentWave == null) return;
    WaveNameInput.SetTextWithoutNotify(currentWave.name);
    WaveTypeName.text = currentWave.wave.ToString();
    WaveTypeImg.sprite = WaveSprites[(int)currentWave.wave];

    sounds.Wave(0, currentWave.wave, currentWave.phase);
    sounds.ADSR(0, currentWave.a, currentWave.d, currentWave.s, currentWave.r);
  }

  public WaveformEditor editor;

  public void CopyFromWaveEditor() {
    if (currentWave == null) return;
    Wave w = editor.Export();
    currentWave.CopyForm(w);
    ShowWave();
  }

  public void CopyToWaveEditor() {
    if (currentWave == null) return;
    editor.Import(currentWave);
  }

  public void UpdateWaveName(bool completed) {
    if (currentWave == null) return;
    currentWave.name = WaveNameInput.text;
    inputsSelected = !completed;

    if (status != MusicEditorStatus.Waveforms || wlines == null || wlines.Count == 0) return;
    foreach(WaveLine wl in wlines) {
      if (wl.WaveID.text == currentWave.id.ToString()) {
        wl.WaveName.text = currentWave.name;
        wl.WaveType.text = currentWave.wave.ToString();
        wl.WaveTypeImg.sprite = editor.WaveSprites[(int)currentWave.wave];
        return;
      }
    }
  }

  public void SetWave() {
    bool active = !WavePickContainer.parent.gameObject.activeSelf;
    WavePickContainer.parent.gameObject.SetActive(active);
    if (active) PickWaveSetup();
  }


  void PickWaveSetup() {
    if (waves.Count == 0) {
      WavePickContainer.parent.gameObject.SetActive(false);
      return;
    }

    foreach (Transform t in WavePickContainer)
      Destroy(t.gameObject);

    foreach (Wave w in waves) {
      GameObject sbb = Instantiate(SelectWaveButton, WavePickContainer);
      sbb.SetActive(true);
      sbb.transform.GetChild(0).GetComponent<Image>().sprite = editor.WaveSprites[(int)w.wave];
      sbb.transform.GetChild(1).GetComponent<Text>().text = "[" + w.id + "] " + w.name;
      sbb.GetComponent<Button>().onClick.AddListener(() => { DoPickWave(w); });
    }
  }

  private void DoPickWave(Wave w) {
    WavePickContainer.parent.gameObject.SetActive(false);
    currentWave = w;
    ShowWave();
    if (status != MusicEditorStatus.BlockEdit) return;

    if (col < 0 || col >= music.numVoices) return;
    if (row < 0 || row >= currentBlock.chs[col].Count) return;

    currentBlock.chs[col][row].type = NoteType.Wave;
    currentBlock.chs[col][row].val = w.id;

    blines[row].note[col].SetWave(w.id, w.name, NoteTypeSprites[(int)NoteType.Wave]);
  }


  #endregion


  #region Play
  public void Record() {
    // FIXME
  }

  public void Rewind() {
    SelectRow(0);
  }

  public void FastForward() {
    SelectRow(10000);
  }

  bool playing = false;
  public void Play() {
    playing = true;
  }

  public void Pause() {
    playing = false;
  }

  public void Stop() {
    playing = false;
    SelectRow(0);
  }


  #endregion

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
  public byte[] rawPCM;

  internal void CopyForm(Wave w) {
    wave = w.wave;
    phase = w.phase;
    a = w.a;
    d = w.d;
    s = w.s;
    r = w.r;
    rawPCM = w.rawPCM;
  }
}


public class Music {
  public string name;
  public int bpm;
  public byte[] voices;
  public List<int> blocks;

  public int numVoices { get {
      int numv = 0;
      for (int i = 0; i < voices.Length; i++)
        if (voices[i] != 255) numv++;
      return numv;
    }
  }
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

If enter is pressed select block (music editor) or wave (block editor)

add play/pause/rev/ff
add multiple selection of rows to enalbe cleanup and copy/paste
 */


