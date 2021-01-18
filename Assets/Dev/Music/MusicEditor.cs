﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MusicEditor : MonoBehaviour {
  #region References and global variables **********************************************************************************************************************************************************

  public Audio sounds;
  public Transform ContentsMusic;
  public Transform ContentsBlocks;
  public Transform ContentsBlock;
  public Transform ContentsWaves;
  public GameObject ContainerMusic;
  public GameObject ContainerBlocks;
  public GameObject ContainerBlock;
  public GameObject ContainerWaves;
  public RectTransform SelectedCol;
  public Scrollbar scrollMusic;
  public Scrollbar scrollBlocks;
  public Scrollbar scrollBlock;
  public Scrollbar scrollWaves;
  public EventSystem EventSystemManager;

  private List<BlockData> blocks = null;
  private List<Wave> waves = null;
  private BlockData currentBlock = null;
  private Wave currentWave = null;
  readonly private List<MusicLine> mlines = new List<MusicLine>();
  readonly private List<BlockListLine> bllines = new List<BlockListLine>();
  readonly private List<BlockLine> blines = new List<BlockLine>();
  readonly private List<WaveLine> wlines = new List<WaveLine>();

  public Transform Selection;
  public RectTransform SelectionBox;
  private Color32 SelectedColor = new Color32(36, 52, 36, 255);
  private Color32 Transparent = new Color32(0, 0, 0, 0);
  private Color32 TapeButtonColor = new Color32(191, 202, 219, 255);
  private Color32 TapeButtonBlue = new Color32(47, 112, 212, 255);
  public Sprite[] NoteTypeSprites;
  public Sprite[] WaveSprites;
  public Button[] TapeButtons;
  public GameObject TitleMusic;
  public GameObject TitleBlock;
  public GameObject TitleBlockList;
  public GameObject TitleWaves;

  public InputField NameInput;
  public InputField NumVoicesInputField;
  public InputField MusicBPMInputField;
  public InputField MusicDefLenInputField;
  public InputField BlockBPMInputField;
  public InputField BlockLenInputField;
  public InputField NoteLenInputField;
  public InputField StepLenInputField;

  public Text CurrentBlockID;
  public InputField BlockNameInput;
  public InputField WaveNameInput;
  public Text WaveNameID;
  public Text WaveTypeName;
  public Image WaveTypeImg;

  public GameObject MusicLineTempate;
  public GameObject BlockListLineTemplate;
  public GameObject WaveLineTemplate;
  public GameObject CreateNewBlockInMusic;
  public GameObject CreateNewBlockInList;
  public GameObject CreateNewWaveInList;

  public Text SelInfo;
  int selectionYStart = -1;
  int selectionYEnd = -1;
  int selectionYDir = 0;

  MusicData music;
  float timeForNextBeat = 0;
  float autoRepeat = 0;
  int currentPlayedMusicBlock = 0;
  int currentPlayedMusicLine = 0;
  bool inputsSelected = false;
  int row = 0;
  int col = 0;
  bool recording = false;
  float countInForRecording = 0;
  bool playing = false;
  bool repeat = false;
  readonly Swipe[] swipes = new Swipe[] {
    new Swipe(), new Swipe(), new Swipe(), new Swipe(),
    new Swipe(), new Swipe(), new Swipe(), new Swipe()
  };
  MusicEditorStatus status = MusicEditorStatus.Idle;
  readonly List<NoteData> CopiedNotes = new List<NoteData>();

  System.Globalization.NumberStyles numberstyle = System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;
  System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");

  #endregion

  private void Start() {
    music = new MusicData() {
      name = "Music",
      bpm = 120,
      defLen = 64,
      voices = new byte[] { 0, 1, 2, 3, 255, 255, 255, 255 },
      blocks = new List<int>()
    };
    NameInput.SetTextWithoutNotify(music.name);
    blocks = new List<BlockData>();
    waves = new List<Wave>();
    int numv = music.NumVoices;

    for (int i = 0; i < 128; i++) {
      BlockLine bl = ContentsBlock.GetChild(i).GetComponent<BlockLine>();
      bl.index = i;
      bl.IndexTxt.text = i.ToString("d2");
      bl.LineButton.onClick.AddListener(() => SelectRow(i));
      int linenum = i;
      for (int c = 0; c < 8; c++) {
        int colnum = c;
        bl.note[c].SetZeroValues(NoteTypeSprites);
        bl.note[c].ColButton.onClick.AddListener(() => SelectRowColumn(linenum, colnum));
        bl.note[c].gameObject.SetActive(c < numv);
      }
      blines.Add(bl);
    }
    ShowNote(null);
    Instantiate(CreateNewBlockInMusic, ContentsMusic).SetActive(true);
    Instantiate(CreateNewBlockInList, ContentsBlocks).SetActive(true);
    Instantiate(CreateNewWaveInList, ContentsWaves).SetActive(true);
    ContainerMusic.SetActive(false);
    ContainerBlocks.SetActive(false);
    ContainerBlock.SetActive(false);
    ContainerWaves.SetActive(false);
    Selection.gameObject.SetActive(false);
  }

  void HandleSwipes() {
    for (int c = 0; c < 8; c++) {
      Swipe s = swipes[c];
      if (s.vollen != 0) {
        float step = s.voltime / s.vollen;
        sounds.Volume(c, s.vole * step + s.vols * (1 - step));
        s.voltime += Time.deltaTime;
        if (s.voltime >= s.vollen) {
          sounds.Volume(c, s.vole);
          s.vollen = 0;
        }
      }

      if (s.pitchlen != 0) {
        float step = s.pitchtime / s.pitchlen;
        sounds.Pitch(c, s.pitche * step + s.pitchs * (1 - step));
        s.pitchtime += Time.deltaTime;
        if (s.pitchtime >= s.pitchlen) {
          sounds.Pitch(c, s.pitche);
          s.pitchlen = 0;
        }
      }

      if (s.panlen != 0) {
        float step = s.pantime / s.panlen;
        sounds.Pan(c, s.pane * step + s.pans * (1 - step));
        s.pantime += Time.deltaTime;
        if (s.pantime >= s.panlen) {
          sounds.Pan(c, s.pane);
          s.panlen = 0;
        }
      }
    }
  }

  void PlayMusic() {
    // Check for swipes
    HandleSwipes();

    // Wait the time to play
    if (timeForNextBeat > 0) {
      timeForNextBeat -= Time.deltaTime;
      if (timeForNextBeat < 0)
        timeForNextBeat = 0;
      else
        return;
    }

    // if block>max or <0 start from 0
    if (currentPlayedMusicBlock < 0 || currentPlayedMusicBlock >= music.blocks.Count) {
      currentPlayedMusicBlock = 0;
      currentPlayedMusicLine = 0;
    }

    // Pick block
    BlockData block = null;
    int id = music.blocks[currentPlayedMusicBlock];
    foreach (BlockData b in blocks) {
      if (b.id == id) {
        block = b;
        break;
      }
    }
    if (block == null) {
      playing = false;
      currentPlayedMusicBlock = 0;
      currentPlayedMusicLine = 0;
      SetTapeButtonColor(-1);
      return;
    }

    // has block current note?
    // if note<0 start from 0
    if (currentPlayedMusicLine < 0) currentPlayedMusicLine = 0;
    // if note > blen go to next block
    if (currentPlayedMusicLine >= block.len) {
      currentPlayedMusicLine = 0;
      currentPlayedMusicBlock++;
      // no next block? restart if repeat
      if (currentPlayedMusicBlock >= music.blocks.Count) {
        currentPlayedMusicBlock = 0;
        currentPlayedMusicLine = 0;
        if (!repeat) {
          playing = false;
          SetTapeButtonColor(-1);
        }
      }
      return;
    }

    // Show the line
    SelectRow(currentPlayedMusicBlock);
    ScrollViews(currentPlayedMusicBlock);

    // music: get and play note.
    PlayNote(block);
  }

  void PlayBlock() {
    // Check for swipes
    HandleSwipes();

    // Wait the time to play
    if (timeForNextBeat > 0) {
      timeForNextBeat -= Time.deltaTime;
      if (timeForNextBeat < 0)
        timeForNextBeat = 0;
      else
        return;
    }

    // Pick block
    BlockData block = currentBlock;
    if (block == null) {
      playing = false;
      currentPlayedMusicBlock = 0;
      currentPlayedMusicLine = 0;
      SetTapeButtonColor(-1);
      return;
    }

    // has block current note?
    // if note<0 start from 0
    if (currentPlayedMusicLine < 0) currentPlayedMusicLine = 0;
    // if note > blen go to next block
    if (currentPlayedMusicLine >= block.len) {
      currentPlayedMusicLine = 0;
      if (!repeat) {
        playing = false;
        SetTapeButtonColor(-1);
      }
      return;
    }

    // Show the line
    ScrollViews(currentPlayedMusicLine);
    SelectRow(currentPlayedMusicLine);

    // music: get and play note.
    PlayNote(block);
  }

  private void Update() {
    bool update = false;
    autoRepeat -= Time.deltaTime;

    if (countInForRecording > 0) { // Handle count-in for recording
      countInForRecording -= Time.deltaTime;
      if ((countInForRecording % timeForNextBeat) < timeForNextBeat * .75f)
        SetTapeButtonColor(-1);
      else
        SetTapeButtonColor(0);

      if (countInForRecording <= 0) {
        SetTapeButtonColor(0);
        recording = true;
      }
      return;
    }

    if (playing && status == MusicEditorStatus.Music) {
      PlayMusic();
    }
    else if (playing && status == MusicEditorStatus.BlockEdit) {
      PlayBlock();
    }
    else if (inputsSelected) {
      if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)) {
        BlockPickContainer.parent.gameObject.SetActive(false);
        WavePickContainer.parent.gameObject.SetActive(false);
        inputsSelected = false;
        EventSystemManager.SetSelectedGameObject(null);
      }
    }
    else {
      if (recording) PlayBlock();

      if (!recording) {
        if (status == MusicEditorStatus.BlockEdit) {
          if (Input.GetKey(KeyCode.UpArrow) && blines != null && row > 0 && autoRepeat < 0) { row--; update = true; autoRepeat = .1f; }
          if (Input.GetKey(KeyCode.DownArrow) && blines != null && row < currentBlock.len - 1 && autoRepeat < 0) { row++; update = true; autoRepeat = .1f; }
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
      }
      if (status == MusicEditorStatus.BlockEdit) {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && col > 0) { col--; update = true; autoRepeat = .25f; }
        if (Input.GetKeyDown(KeyCode.RightArrow) && col < 7) { col++; update = true; autoRepeat = .25f; }
      }

      if (!recording) {
        if (status == MusicEditorStatus.Music && row > -1 && row < mlines.Count) {
          if (Input.GetKeyDown(KeyCode.Return)) {
            PickBlock();
          }
        }

        if (status == MusicEditorStatus.Waveforms && row > -1 && row < wlines.Count) {
          // Piano keys
          for (int i = 0; i < keyNotes.Length; i++) {
            if (Input.GetKeyDown(keyNotes[i])) {
              // Set the current cell as note with the given note/frequency, update the text to be the note notation
              sounds.Play(0, freqs[i + 24], .25f);
            }
          }
        }
      }

      if (status == MusicEditorStatus.BlockEdit && row > -1 && row < blines.Count) {
        BlockLine l = blines[row];
        if (!recording) {
          // Enter select wave if there is not a note
          if (Input.GetKeyDown(KeyCode.Return) && (l.note[col].type == NoteType.Empty || l.note[col].type == NoteType.Wave)) {
            SetWave();
            return;
          }
          // Remove cell values
          if (Input.GetKeyDown(KeyCode.Delete)) {
            currentBlock.chs[col][row].Zero();
            blines[row].note[col].SetZeroValues(NoteTypeSprites);
          }
          // Change length
          if (l.note[col].type == NoteType.Note || l.note[col].type == NoteType.Pitch || l.note[col].type == NoteType.Volume) {
            if (Input.GetKeyDown(KeyCode.PageUp) && l.note[col].len > 1) {
              UpdateNoteLength(l.note[col].len - 1);
            }
            if (Input.GetKeyDown(KeyCode.PageDown) && l.note[col].len < blines.Count - row) {
              UpdateNoteLength(l.note[col].len + 1);
            }
          }
          // Change wave
          if (l.note[col].type == NoteType.Wave) {
            if (Input.GetKeyDown(KeyCode.PageUp)) {
              int id = l.note[col].val;
              int pos;
              for (pos = 0; pos < waves.Count; pos++) {
                if (waves[pos].id == id) {
                  if (pos == 0) {
                    l.note[col].val = waves[waves.Count - 1].id;
                  }
                  else {
                    l.note[col].val = waves[pos - 1].id;
                  }
                }
              }
              currentBlock.chs[col][row].val = l.note[col].val;
              blines[row].note[col].SetValues(currentBlock.chs[col][row], NoteTypeSprites, freqs, noteNames, waves);
              ShowNote(currentBlock.chs[col][row]);
            }
            if (Input.GetKeyDown(KeyCode.PageDown)) {
              int id = l.note[col].val;
              int pos;
              for (pos = 0; pos < waves.Count; pos++) {
                if (waves[pos].id == id) {
                  if (pos == waves.Count - 1) {
                    l.note[col].val = waves[0].id;
                  }
                  else {
                    l.note[col].val = waves[pos + 1].id;
                  }
                }
              }
              currentBlock.chs[col][row].val = l.note[col].val;
              blines[row].note[col].SetValues(currentBlock.chs[col][row], NoteTypeSprites, freqs, noteNames, waves);
              ShowNote(currentBlock.chs[col][row]);
            }
          }
          // Space change type
          if (Input.GetKeyDown(KeyCode.Space)) {
            int t = (int)l.note[col].type;
            t++;
            if (t == 5) t = 0;
            ChangeNoteTypePost(t);
          }
        }
        // Piano keys
        for (int i = 0; i < keyNotes.Length; i++) {
          if (Input.GetKeyDown(keyNotes[i])) {
            // Set the current cell as note with the given note/frequency, update the text to be the note notation
            l.note[col].type = NoteType.Note;
            l.note[col].TypeImg.sprite = NoteTypeSprites[(int)NoteType.Note];
            l.note[col].ValTxt.text = noteNames[i + 24];
            l.note[col].val = freqs[i + 24];
            l.note[col].len = noteLen;
            l.note[col].LenTxt.text = noteLen.ToString();
            l.note[col].back.sizeDelta = new Vector2(38, noteLen * 32);
            currentBlock.chs[col][row].Set(l.note[col]);
            // Move to the next row
            if (!recording && row + stepLen < currentBlock.len) { row += stepLen; update = true; }
            // Play the actual sound (find the wave that should be used, if none is defined use a basic triangle wave)
            sounds.Play(col, freqs[i + 24], .25f);
          }
        }
      }
    }

    // Ctrl+C, Ctrl+V, Ctrl+X, ShiftUp, ShiftDown
    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {

      if (Input.GetKeyDown(KeyCode.DownArrow)) {
        if (selectionYStart == -1 || row > selectionYEnd + 1 || row < selectionYStart - 1) { // No selection
          selectionYStart = row - 1;
          selectionYEnd = row;
          selectionYDir = 1;
        }
        else {
          if (selectionYDir == 1) selectionYEnd++;
          else if (selectionYDir == -1) selectionYStart++;
          if (selectionYEnd > currentBlock.len - 1) selectionYEnd = currentBlock.len - 1;
          if (selectionYEnd < selectionYStart) {
            selectionYStart = -1;
            selectionYEnd = -1;
          }
        }
        ShowSelection();
      }

      if (Input.GetKeyDown(KeyCode.UpArrow)) {
        if (selectionYStart == -1 || row > selectionYEnd + 1 || row < selectionYStart - 1) { // No selection
          selectionYStart = row;
          selectionYEnd = row + 1;
          selectionYDir = -1;
        }
        else {
          if (selectionYDir == 1) selectionYEnd--;
          else if (selectionYDir == -1) selectionYStart--;
          if (selectionYStart < 0) selectionYStart = 0;
          if (selectionYEnd < selectionYStart) {
            selectionYStart = -1;
            selectionYEnd = -1;
          }
        }
        ShowSelection();
      }
    }
    if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) { // Ctrl+C
      CopiedNotes.Clear();
      for (int i = selectionYStart; i <= selectionYEnd; i++) {
        CopiedNotes.Add(currentBlock.chs[col][i].Duplicate());
      }
      HideSelection();
      if (CopiedNotes.Count == 0)
        SelInfo.text = "";
      else if (CopiedNotes.Count == 1)
        SelInfo.text = CopiedNotes.Count + " cell copied";
      else
        SelInfo.text = CopiedNotes.Count + " cells copied";
    }
    if (Input.GetKeyDown(KeyCode.X) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) { // Ctrl+X
      CopiedNotes.Clear();
      for (int i = selectionYStart; i <= selectionYEnd; i++) {
        CopiedNotes.Add(currentBlock.chs[col][i].Duplicate());
        currentBlock.chs[col][i].Zero();
        blines[i].note[col].SetZeroValues(NoteTypeSprites);
      }
      HideSelection();
      if (CopiedNotes.Count == 0)
        SelInfo.text = "";
      else if (CopiedNotes.Count == 1)
        SelInfo.text = CopiedNotes.Count + " cell copied";
      else
        SelInfo.text = CopiedNotes.Count + " cells copied";
    }
    if (Input.GetKeyDown(KeyCode.V) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && CopiedNotes.Count > 0) { // Ctrl+V
      // If we have something, paste starting from the current position
      for (int i = 0; i < CopiedNotes.Count && row + i < currentBlock.len; i++) {
        currentBlock.chs[col][row + i].Set(CopiedNotes[i]);
        blines[row + i].note[col].SetValues(CopiedNotes[i], NoteTypeSprites, freqs, noteNames, waves);
      }
      HideSelection();
    }

    if (update) {
      ScrollViews(row); // Scroll if needed
      SelectedCol.anchoredPosition = new Vector3(48 + col * 142, 30, 0);
      SelectRow(row);
    }
  }

  void ShowSelection() {
    Selection.SetParent(ContentsBlock);
    Selection.SetAsLastSibling();
    Selection.gameObject.SetActive(true);
    if (selectionYStart == -1) {
      selectionYStart = row;
      selectionYEnd = row;
    }
    Vector2 pos = Vector2.zero;
    pos.x = 48 + col * 142;
    pos.y = -blines[currentBlock.len - 1].GetComponent<RectTransform>().anchoredPosition.y + 32.5f - 32 * selectionYStart;
    SelectionBox.anchoredPosition = pos;
    SelectionBox.sizeDelta = new Vector2(142, 34 * (selectionYEnd - selectionYStart + 1) * .95f);
  }

  void HideSelection() {
    selectionYStart = -1;
    selectionYEnd = -1;
    Selection.gameObject.SetActive(false);
  }

  void ScrollViews(int where) {
    Scrollbar bar;
    int len;
    if (status == MusicEditorStatus.Music) {
      bar = scrollMusic;
      len = mlines.Count - 14;
    }
    else if (status == MusicEditorStatus.BlockList) {
      bar = scrollBlocks;
      len = bllines.Count - 14;
    }
    else if (status == MusicEditorStatus.BlockEdit) {
      bar = scrollBlock;
      len = currentBlock.len - 14;
    }
    else if (status == MusicEditorStatus.Waveforms) {
      bar = scrollWaves;
      len = wlines.Count - 14;
    }
    else return;

    if (where < 13) bar.value = 1;
    else if (where > len) bar.value = 0;
    else bar.value = -0.0276f * where + 1.333333333333333f;
  }

  private void ShowSection(MusicEditorStatus mode) {
    switch (status) {
      case MusicEditorStatus.Idle: break;
      case MusicEditorStatus.Music:
        ContainerMusic.SetActive(false);
        TitleMusic.SetActive(false);
        break;
      case MusicEditorStatus.BlockList:
        ContainerBlocks.SetActive(false);
        TitleBlockList.SetActive(false);
        break;
      case MusicEditorStatus.BlockEdit:
        ContainerBlock.SetActive(false);
        TitleBlock.SetActive(false);
        break;
      case MusicEditorStatus.Waveforms:
        ContainerWaves.SetActive(false);
        TitleWaves.SetActive(false);
        break;
    }
    status = mode;
    switch (status) {
      case MusicEditorStatus.Idle: break;
      case MusicEditorStatus.Music:
        ContainerMusic.SetActive(true);
        TitleMusic.SetActive(true);
        break;
      case MusicEditorStatus.BlockList:
        ContainerBlocks.SetActive(true);
        TitleBlockList.SetActive(true);
        break;
      case MusicEditorStatus.BlockEdit:
        ContainerBlock.SetActive(true);
        TitleBlock.SetActive(true);
        break;
      case MusicEditorStatus.Waveforms:
        ContainerWaves.SetActive(true);
        TitleWaves.SetActive(true);
        break;
    }
  }

  private bool PlayNote(BlockData block) {
    if (block == null) return true;

    timeForNextBeat = 15f / block.bpm;
    for (int c = 0; c < music.NumVoices; c++) {
      NoteData n = block.chs[c][currentPlayedMusicLine];
      switch (n.type) {
        case NoteType.Empty: break;

        case NoteType.Volume:
          if (n.len < 2) {
            sounds.Volume(c, n.val / 255f);
          }
          else {
            swipes[c].vols = sounds.Volume(c);
            swipes[c].vole = n.val / 255f;
            swipes[c].voltime = 0;
            swipes[c].vollen = (n.len - 1) * 15f / block.bpm;
          }
          break;

        case NoteType.Pitch:
          if (n.len < 2) {
            sounds.Pitch(c, n.val);
          }
          else {
            swipes[c].pitchs = sounds.Pitch(c);
            swipes[c].pitche = n.val;
            swipes[c].pitchtime = 0;
            swipes[c].pitchlen = (n.len - 1) * 15f / block.bpm;
          }
          break;

        case NoteType.Pan:
          if (n.len < 2) {
            sounds.Pan(c, n.val / 255f);
          }
          else {
            swipes[c].pans = sounds.Pan(c);
            swipes[c].pane = n.val / 255f;
            swipes[c].pantime = 0;
            swipes[c].panlen = (n.len - 1) * 15f / block.bpm;
          }
          break;

        case NoteType.Note:
          sounds.Play(c, n.val, n.len * timeForNextBeat);
          break;

        case NoteType.Wave:
          Wave w = GetWave(n.val);
          if (w != null) {
            sounds.Wave(c, w.wave, w.phase);
            sounds.ADSR(c, w.a, w.d, w.s, w.r);
            if (w.rawPCM != null) sounds.Wave(c, w.rawPCM);
          }
          break;
      }
    }
    currentPlayedMusicLine++;

    return false;
  }

  void SelectRow(int line) {
    if (status == MusicEditorStatus.Music) {
      if (mlines.Count == 0) return;
      for (int i = 0; i < mlines.Count; i++)
        mlines[i].Background.color = Transparent;
      row = line;
      if (row < 0) row = 0;
      if (row >= mlines.Count) row = mlines.Count - 1;
      mlines[line].Background.color = SelectedColor;
      CurrentBlockID.text = "[]";
      BlockNameInput.SetTextWithoutNotify("???");
      BlockLenInputField.SetTextWithoutNotify("64");
      BlockBPMInputField.SetTextWithoutNotify("120");
      int id = music.blocks[line];
      foreach (BlockData b in blocks) {
        if (b.id == id) {
          currentBlock = b;
          ShowBlockInfo();
          break;
        }
      }
    }
    else if (status == MusicEditorStatus.BlockEdit) {
      if (line >= blines.Count) return;
      row = line;
      if (row < 0) row = 0;
      if (row >= blines.Count) row = blines.Count - 1;
      for (int i = 0; i < blines.Count; i++)
        blines[i].Background.color = Transparent;
      blines[row].Background.color = SelectedColor;

      List<NoteData> notes = currentBlock.chs[col];
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

      ShowNote(currentBlock.chs[col][row]);
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

    ScrollViews(line);
  }

  void SelectRowColumn(int line, int column) {
    col = column;
    SelectedCol.anchoredPosition = new Vector3(48 + col * 142, 30, 0);
    SelectRow(line);
  }


  #region Music **********************************************************************************************************************************************************

  public void MusicRegenerate() {
    int pos = 1;
    foreach (Transform t in ContentsMusic)
      Destroy(t.gameObject);
    mlines.Clear();
    for (int i = 0; i < music.blocks.Count; i++) {
      int bi = music.blocks[i];
      BlockData b = null;
      for (int j = 0; j < blocks.Count; j++)
        if (blocks[j].id == bi) {
          b = blocks[j];
          break;
        }

      GameObject line = Instantiate(MusicLineTempate, ContentsMusic);
      line.SetActive(true);
      MusicLine ml = line.GetComponent<MusicLine>();
      ml.index = pos;
      ml.IndexTxt.text = pos.ToString();
      ml.BlockID.text = bi.ToString();
      ml.BlockName.text = bi == -1 || b == null ? "" : b.name;
      ml.BlockLen.text = bi == -1 || b == null ? "0" : b.len.ToString();
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
    Instantiate(CreateNewBlockInMusic, ContentsMusic).SetActive(true);
  }

  public void Music() { // Show what we have as music
    ShowSection(MusicEditorStatus.Music);
    SelectedCol.gameObject.SetActive(false);
    NameInput.SetTextWithoutNotify(music.name);
  }

  public void ChangeMusicVoices(bool up) {
    int numv = music.NumVoices;
    if (up && numv < 8) numv++;
    if (!up && numv > 1) numv--;
    for (int i = 0; i < 8; i++)
      music.voices[i] = (byte)((i < numv) ? i : 255);
    NumVoicesInputField.SetTextWithoutNotify(numv.ToString());
    inputsSelected = false;
    if (status == MusicEditorStatus.BlockEdit) StartCoroutine(UpdateVisiblityOfColumnsDelayed());
  }
  public void ChangeMusicVoicesType(bool completed) {
    inputsSelected = !completed;
    if (completed) {
      int.TryParse(NumVoicesInputField.text, out int numv);
      if (numv < 1 || numv > 8) {
        NumVoicesInputField.SetTextWithoutNotify(music.NumVoices.ToString());
        return;
      }
      int prev = music.NumVoices;
      for (int i = 0; i < 8; i++)
        music.voices[i] = (byte)((i < numv) ? i : 255);
      if (prev != numv && status == MusicEditorStatus.BlockEdit) ShowBlock();
      if (status == MusicEditorStatus.BlockEdit) StartCoroutine(UpdateVisiblityOfColumnsDelayed());
    }
  }

  public void ChangeMusicBPM(bool up) {
    if (up && music.bpm < 240) music.bpm++;
    if (!up && music.bpm > 20) music.bpm--;
    MusicBPMInputField.SetTextWithoutNotify(music.bpm.ToString());
    inputsSelected = false;
  }
  public void ChangeMusicBPMType(bool completed) {
    inputsSelected = !completed;
    if (completed) {
      int.TryParse(MusicBPMInputField.text, out int bpm);
      if (bpm < 20 || bpm > 240) {
        MusicBPMInputField.SetTextWithoutNotify(music.bpm.ToString());
        return;
      }
      music.bpm = bpm;
    }
  }

  public void ChangeMusicLen(bool up) {
    if (up && music.defLen < 128) music.defLen++;
    if (!up && music.defLen > 1) music.defLen--;
    MusicDefLenInputField.SetTextWithoutNotify(music.defLen.ToString());
    inputsSelected = false;
  }
  public void ChangeMusicLenType(bool completed) {
    inputsSelected = !completed;
    if (completed) {
      int.TryParse(MusicDefLenInputField.text, out int len);
      if (len < 1 || len > 128) {
        MusicDefLenInputField.SetTextWithoutNotify(music.defLen.ToString());
        return;
      }
      music.defLen = len;
    }
  }

  public void AddNewBlockInMusic() {
    // Each block should have the ID (hex number), and a name. Remove, MoveUp, Down, Edit
    music.blocks.Add(-1);
    Transform last = ContentsMusic.GetChild(ContentsMusic.childCount - 1);
    GameObject line = Instantiate(MusicLineTempate, ContentsMusic);
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
    last.SetAsLastSibling();
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
    ContentsMusic.GetChild(pos).SetSiblingIndex(pos - 1);
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
    ContentsMusic.GetChild(pos).SetSiblingIndex(pos + 1);
  }

  public void EditCurrentMusicLineBlock(int pos) {
    // pick the current block and show it, if missing create a new one
    if (pos == -1 || music.blocks[pos] == -1) return;
    int id = music.blocks[pos];
    foreach (BlockData b in blocks)
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
    inputsSelected = !completed;
    music.name = NameInput.text;
  }

  #endregion

  #region Block **********************************************************************************************************************************************************
  public Transform BlockPickContainer;
  public GameObject SelectBlockButton;

  public void PickBlock() {
    bool active = !BlockPickContainer.parent.gameObject.activeSelf;
    BlockPickContainer.parent.gameObject.SetActive(active);
    if (active) {
      PickBlockSetup();
      inputsSelected = true;
    }
  }

  void PickBlockSetup() {
    if (blocks.Count == 0) {
      BlockPickContainer.parent.gameObject.SetActive(false);
      return;
    }

    foreach (Transform t in BlockPickContainer)
      Destroy(t.gameObject);

    GameObject first = null;
    foreach(BlockData b in blocks) {
      GameObject sbb = Instantiate(SelectBlockButton, BlockPickContainer);
      sbb.SetActive(true);
      sbb.transform.GetChild(0).GetComponent<Text>().text = "[" + b.id + "] " + b.name;
      sbb.GetComponent<Button>().onClick.AddListener(() => { DoPickBlock(b); });
      if (first == null) first = sbb;
    }
    EventSystemManager.SetSelectedGameObject(first);
  }

  private void DoPickBlock(BlockData b) {
    BlockPickContainer.parent.gameObject.SetActive(false);
    currentBlock = b;
    inputsSelected = false;

    if (status == MusicEditorStatus.Music) {
      music.blocks[row] = b.id;
      ShowBlockInfo();
      MusicLine ml = mlines[row];
      ml.BlockID.text = b.id.ToString();
      ml.BlockName.text = b.name;
      ml.BlockLen.text = b.len.ToString();
    }
    else
      ShowBlock();
  }

  public void ChangeBlockLen(bool up) {
    if (currentBlock == null) return;
    BlockData b = currentBlock;
    int len = b.len;
    if (up && len < 128) len++;
    if (!up && len > 1) len--;
    BlockLenInputField.SetTextWithoutNotify(len.ToString());
    inputsSelected = false;
    UpdateBlockLen(b, len);
  }
  public void ChangeBlockLenType(bool completed) {
    inputsSelected = !completed;
    if (currentBlock == null) return;
    if (completed) {
      BlockData b = currentBlock;
      int.TryParse(BlockLenInputField.text, out int len);
      if (len < 1 || len > 128) {
        BlockLenInputField.SetTextWithoutNotify(b.len.ToString());
        return;
      }
      UpdateBlockLen(b, len);
    }
  }
  void UpdateBlockLen(BlockData b, int len) {
    if (currentBlock == null) return;
    currentBlock.len = len;
    for (int r = 0; r < 128; r++) {
      blines[r].gameObject.SetActive(r < len);
    }

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


  public void ChangeBlockBPM(bool up) {
    if (currentBlock == null) return;
    BlockData b = currentBlock;
    if (up && b.bpm < 240) b.bpm++;
    if (!up && b.bpm > 20) b.bpm--;
    BlockBPMInputField.SetTextWithoutNotify(b.bpm.ToString());
    inputsSelected = false;
  }
  public void ChangeBlockBPMType(bool completed) {
    inputsSelected = !completed;
    if (currentBlock == null) return;
    if (completed) {
      BlockData b = currentBlock;
      int.TryParse(BlockBPMInputField.text, out int bpm);
      if (bpm < 20 || bpm > 240) {
        BlockBPMInputField.SetTextWithoutNotify(b.bpm.ToString());
        return;
      }
      b.bpm = bpm;
    }
  }

  int noteLen = 1;
  int stepLen = 2;

  public void ChangeNoteLen(bool up) {
    if (up && noteLen < 16) noteLen++;
    if (!up && noteLen > 1) noteLen--;
    NoteLenInputField.SetTextWithoutNotify(noteLen.ToString());
    inputsSelected = false;
    UpdateNoteLength();
  }
  public void ChangeNoteLenType(bool completed) {
    inputsSelected = !completed;
    if (completed) {
      int.TryParse(NoteLenInputField.text, out int len);
      if (len < 20 || len > 16) {
        NoteLenInputField.SetTextWithoutNotify(len.ToString());
        return;
      }
      noteLen = len;
      UpdateNoteLength();
    }
  }

  public void ChangeStepLen(bool up) {
    if (up && stepLen < 16) stepLen++;
    if (!up && stepLen > 1) stepLen--;
    StepLenInputField.SetTextWithoutNotify(stepLen.ToString());
    inputsSelected = false;
  }
  public void ChangeStepLenType(bool completed) {
    inputsSelected = !completed;
    if (completed) {
      int.TryParse(NoteLenInputField.text, out int len);
      if (len < 20 || len > 16) {
        StepLenInputField.SetTextWithoutNotify(len.ToString());
        return;
      }
      stepLen = len;
    }
  }

  public void ShowBlock() { // Show the current block
    if (currentBlock == null) return;
    ShowSection(MusicEditorStatus.BlockEdit);
    SelectedCol.gameObject.SetActive(true);

    ShowBlockInfo();
    int numv = music.NumVoices;
    for (int r = 0; r < currentBlock.len; r++) {
      BlockLine bl = blines[r];
      bl.gameObject.SetActive(true);
      for (int c = 0; c < numv; c++) {
        bl.note[c].SetValues(currentBlock.chs[c][r], NoteTypeSprites, freqs, noteNames, waves);
      }
    }
    StartCoroutine(HideLinesDelayed(currentBlock.len));
    StartCoroutine(UpdateVisiblityOfColumnsDelayed());
  }

  IEnumerator HideLinesDelayed(int start) {
    yield return null;
    for (int r = start; r < 128; r++) {
      blines[r].gameObject.SetActive(false);
    }
  }

  IEnumerator UpdateVisiblityOfColumnsDelayed() {
    yield return null;
    int numv = music.NumVoices;
    for (int r = 0; r < currentBlock.len; r++) {
      BlockLine bl = blines[r];
      for (int c = 0; c < 8; c++) {
        bl.note[c].gameObject.SetActive(c < numv);
      }
    }
  }

  void ShowBlockInfo() {
    if (currentBlock == null) return;
    BlockNameInput.SetTextWithoutNotify(currentBlock.name);
    CurrentBlockID.text = "[" + currentBlock.id + "]";
    BlockLenInputField.SetTextWithoutNotify(currentBlock.len.ToString());
    BlockBPMInputField.SetTextWithoutNotify(currentBlock.bpm.ToString());
  }

  public void UpdateBlockName(bool completed) {
    inputsSelected = !completed;
    if (currentBlock == null) return;
    if (completed) {
      currentBlock.name = BlockNameInput.text;

      if (status == MusicEditorStatus.BlockList) {
        foreach (BlockListLine bl in bllines) {
          if (bl.BlockID.text == currentBlock.id.ToString()) {
            bl.BlockName.text = currentBlock.name;
          }
        }
      }
      else if (status == MusicEditorStatus.Music) {
        foreach (MusicLine ml in mlines) {
          if (ml.BlockID.text == currentBlock.id.ToString()) {
            ml.BlockName.text = currentBlock.name;
          }
        }
      }
    }
  }

  private void UpdateNoteLength(int len = -1) {
    NoteLine note = blines[row].note[col];
    NoteData bn = currentBlock.chs[col][row];
    if (note.type != NoteType.Note && note.type != NoteType.Pitch && note.type != NoteType.Volume) {
      note.len = 0;
      note.LenTxt.text = "";
      note.back.sizeDelta = new Vector2(38, 0 * 32);
      bn.len = 0;
      ShowNote(bn);
      return;
    }

    int val = (len == -1) ? noteLen : len;
    note.len = val;
    bn.len = val;
    note.LenTxt.text = val.ToString();
    note.back.sizeDelta = new Vector2(38, val * 32);
    ShowNote(bn);
  }

  #endregion

  #region Cell **********************************************************************************************************************************************************
  public Image CellTypeImg;
  public Text CellTypeTxt;
  public GameObject CellValContainer;
  public InputField CellValInput;
  public Text CellValPostText;
  public GameObject CellLenContainer;
  public Text CellLenPreText;
  public InputField CellLenInput;
  public Text CellLenPostText;
  public Text CellInfoTxt;
  public GameObject CellTypeContainer;

  private void ShowNote(NoteData note) {
    if (note==null) {
      CellTypeImg.sprite = NoteTypeSprites[(int)NoteType.Empty];
      CellTypeTxt.text = "";
      CellValContainer.SetActive(false);
      CellLenContainer.SetActive(false);
      CellInfoTxt.text = "<i>select a cell...</i>";
      return;
    }

    switch (note.type) {
      case NoteType.Empty: {
        CellTypeImg.sprite = NoteTypeSprites[(int)note.type];
        CellTypeTxt.text = "";
        CellValContainer.SetActive(false);
        CellLenContainer.SetActive(false);
      }
      break;

      case NoteType.Note: {
        CellTypeImg.sprite = NoteTypeSprites[(int)note.type];
        CellTypeTxt.text = "Note";
        CellValContainer.SetActive(true);
        CellLenContainer.SetActive(true);
        CellValPostText.gameObject.SetActive(true);
        // Find the closest note and the freq
        string nv = null;
        for (int i = 0; i < freqs.Length - 1; i++) {
          if (note.val == freqs[i]) {
            nv = noteNames[i] + " - " + note.val;
            break;
          }
          if (note.val > freqs[i] && note.val < freqs[i + 1]) {
            if (note.val - freqs[i] < freqs[i + 1] - note.val)
              nv = "~" + noteNames[i] + " - " + note.val;
            else
              nv = "~" + noteNames[i + 1] + " - " + note.val;
            break;
          }
        }
        if (nv == null) nv = note.val.ToString();
        CellValInput.SetTextWithoutNotify(nv);
        CellValInput.GetComponent<RectTransform>().sizeDelta = new Vector2(224, 48);
        CellValPostText.text = "Hz";
        CellLenPreText.gameObject.SetActive(true);
        CellLenPostText.gameObject.SetActive(true);
        CellLenPreText.text = "For ";
        CellLenInput.SetTextWithoutNotify(note.len.ToString());
        CellLenPostText.text = " beats";
      }
      break;

      case NoteType.Wave: {
        CellTypeImg.sprite = NoteTypeSprites[(int)note.type];
        CellTypeTxt.text = "Wave";
        CellValContainer.SetActive(true);
        CellLenContainer.SetActive(false);
        // Find the closest note and the freq
        CellValInput.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 48);
        CellValInput.SetTextWithoutNotify("");
        CellValPostText.text = "???";
        foreach (Wave w in waves) {
          if (w.id == note.val) {
            CellValInput.SetTextWithoutNotify(w.id.ToString());
            CellValPostText.text = w.name;
            break;
          }
        }
        CellValPostText.gameObject.SetActive(true);
      }
      break;

      case NoteType.Volume: {
        CellTypeImg.sprite = NoteTypeSprites[(int)note.type];
        CellTypeTxt.text = "Volume" + ((note.len > 1) ? " slide" : "");
        CellValContainer.SetActive(true);
        CellLenContainer.SetActive(true);
        CellValPostText.gameObject.SetActive(true);
        CellValPostText.text = "%";
        CellValInput.SetTextWithoutNotify(((int)((note.val / 255f) * 100)).ToString());
        CellValInput.GetComponent<RectTransform>().sizeDelta = new Vector2(224, 48);
        CellLenPreText.gameObject.SetActive(false);
        CellLenPostText.gameObject.SetActive(true);
        CellLenPreText.text = "In ";
        CellLenInput.SetTextWithoutNotify(note.len.ToString());
        CellLenPostText.text = " beats";
      }
      break;

      case NoteType.Pitch: {
        CellTypeImg.sprite = NoteTypeSprites[(int)note.type];
        CellTypeTxt.text = "Pitch" + ((note.len > 1) ? " slide" : "");
        CellValContainer.SetActive(true);
        CellLenContainer.SetActive(true);
        CellValPostText.gameObject.SetActive(true);
        CellValPostText.text = "Semitones";
        float val = note.val / 100f;
        if (val==0)
          CellValInput.SetTextWithoutNotify("0");
        else if (val - (int)val == 0) {
          if (val > 0)
            CellValInput.SetTextWithoutNotify("+" + ((int)val).ToString());
          else
            CellValInput.SetTextWithoutNotify(((int)val).ToString());
        }
        else {
          if (val > 0)
            CellValInput.SetTextWithoutNotify(val.ToString());
          else
            CellValInput.SetTextWithoutNotify(val.ToString());
        }
        CellValInput.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 48);
        CellLenPreText.gameObject.SetActive(true);
        CellLenPostText.gameObject.SetActive(true);
        CellLenPreText.text = "In ";
        CellLenInput.SetTextWithoutNotify(note.len.ToString());
        CellLenPostText.text = " beats";
      }
      break;

      case NoteType.Pan: {
        CellTypeImg.sprite = NoteTypeSprites[(int)note.type];
        CellTypeTxt.text = "Pan" + ((note.len > 1) ? " slide" : "");
        CellValContainer.SetActive(true);
        CellLenContainer.SetActive(true);
        CellValPostText.gameObject.SetActive(false);
        float pan = (note.val - 127) / 127f;
        if (pan < -1) pan = -1;
        if (pan > 1) pan = 1;
        CellValInput.SetTextWithoutNotify(pan.ToString());
        CellValInput.GetComponent<RectTransform>().sizeDelta = new Vector2(224, 48);
        CellLenPreText.gameObject.SetActive(true);
        CellLenPostText.gameObject.SetActive(true);
        CellLenPreText.text = "In ";
        CellLenInput.SetTextWithoutNotify(note.len.ToString());
        CellLenPostText.text = " beats";
      }
      break;
    }
    CellInfoTxt.text = "Row: " + row + "\nChannel: " + (col + 1);
  }

  public void ChangeNoteType() {
    CellTypeContainer.SetActive(!CellTypeContainer.activeSelf);
  }

  public void ChangeNoteTypePost(int type) {
    // Empty=0, Note=1, Wave=2, Volume=3, Freq=4, Pan=5

    NoteLine note = blines[row].note[col];
    note.type = (NoteType)type;
    note.TypeImg.sprite = NoteTypeSprites[type];
    NoteData bn = currentBlock.chs[col][row];
    bn.Set(note);
    blines[row].note[col].SetValues(bn, NoteTypeSprites, freqs, noteNames, waves);
    ShowNote(bn);
    CellTypeContainer.SetActive(false);
  }

  public void ChangeNoteVal(bool up) {
    if (currentBlock == null || currentBlock.chs[col][row] == null) return;
    NoteData bn = currentBlock.chs[col][row];
    if (up) bn.val++; else bn.val--;
    blines[row].note[col].SetValues(bn, NoteTypeSprites, freqs, noteNames, waves);
    ShowNote(bn);
  }

  public void ChangeCellInputVal(bool completed) {
    inputsSelected = !completed;
    if (currentBlock == null) return;
    if (completed) {
      NoteData note = currentBlock.chs[col][row];
      switch (note.type) {
        case NoteType.Empty: break;

        case NoteType.Note: {
          string val = CellValInput.text.Trim().ToLowerInvariant();
          // The value can be a note or a frequency
          int notepos = -1;
          for (int i = 0; i < noteNames.Length; i++) {
            if (noteNames[i].ToLowerInvariant() == val) {
              notepos = i;
              break;
            }
          }
          if (notepos != -1) {
            note.val = freqs[notepos];
            blines[row].note[col].SetValues(note, NoteTypeSprites, freqs, noteNames, waves);
            ShowNote(note);
          }
          else {
            if (int.TryParse(val, out int freq)) {
              note.val = freq;
              blines[row].note[col].SetValues(note, NoteTypeSprites, freqs, noteNames, waves);
              ShowNote(note);
            }
          }
        }
        break;

        case NoteType.Wave: {
          string val = CellValInput.text.Trim().ToLowerInvariant();
          // The value can be an id or a name
          int.TryParse(val, out int waveid);
          for (int i = 0; i < waves.Count; i++) {
            if (waves[i].name.Trim().ToLowerInvariant() == val || waves[i].id == waveid) {
              note.val = waves[i].id;
              blines[row].note[col].SetValues(note, NoteTypeSprites, freqs, noteNames, waves);
              ShowNote(note);
              break;
            }
          }
        }
        break;

        case NoteType.Volume: {
          if (int.TryParse(CellValInput.text.Trim(), out int vol)) {
            if (vol < 0) vol = 0;
            if (vol > 100) vol = 100;
            note.val = (int)(vol * 255f / 100);
            blines[row].note[col].SetValues(note, NoteTypeSprites, freqs, noteNames, waves);
            ShowNote(note);
            break;
          }
        }
        break;

        case NoteType.Pitch: {
          // 1.05946^numsemitones
          // Values can be +[0-9]+(.[0-9]+)? and -[0-9]+(.[0-9]+)?
          if (float.TryParse(CellValInput.text.Trim(), numberstyle, culture, out float fVal)) {
            // The result is stored as value multiplied by 100 truncated to 2 bytes
            int val = (int)(fVal * 100);
            if (val > 32767) val = 32767;
            if (val < -32767) val = -32767;
            note.val = val;
            blines[row].note[col].SetValues(note, NoteTypeSprites, freqs, noteNames, waves);
            ShowNote(note);
            break;
          }
        }
        break;

        case NoteType.Pan: {
          if (float.TryParse(CellValInput.text.Trim(), numberstyle, culture, out float pval)) {
            if (pval < -1) pval = -1;
            if (pval > 1) pval = 1;
            note.val = (int)(pval * 127 + 127);
            blines[row].note[col].SetValues(note, NoteTypeSprites, freqs, noteNames, waves);
            ShowNote(note);
            break;
          }
        }
        break;
      }
    }
  }

  public void ChangeCellInputLen(bool completed) {
    inputsSelected = !completed;
    if (currentBlock == null) return;
    if (completed) {
      NoteData note = currentBlock.chs[col][row];

      if (int.TryParse(CellLenInput.text.Trim(), out int len)) {
        if (len < 1) len = 1;
        if (len > currentBlock.len) len = currentBlock.len;
        note.len = len;
        blines[row].note[col].SetValues(note, NoteTypeSprites, freqs, noteNames, waves);
        ShowNote(note);
      }
    }
  }

  #endregion

  #region Block list **********************************************************************************************************************************************************

  void BlocksRegenerate() {
    foreach (Transform t in ContentsBlocks)
      Destroy(t.gameObject);
    int pos = 0;
    foreach (BlockData b in blocks) {
      GameObject line = Instantiate(BlockListLineTemplate, ContentsBlocks);
      line.SetActive(true);
      BlockListLine bll = line.GetComponent<BlockListLine>();
      bll.BlockID.text = b.id.ToString();
      bll.BlockName.text = b.name;
      bll.BlockLen.text = b.len.ToString();
      bll.BlockBPM.text = b.bpm.ToString();
      bll.Delete.onClick.AddListener(() => DeleteBlockFromList(b));
      bll.Edit.onClick.AddListener(() => EditBlockFromList(b));
      int linenum = pos++;
      bll.LineButton.onClick.AddListener(() => SelectRow(linenum));
      bllines.Add(bll);
    }

    Instantiate(CreateNewBlockInList, ContentsBlocks).SetActive(true);
  }

  public void Blocks() { // Show a list of blocks
    ShowSection(MusicEditorStatus.BlockList);
    SelectedCol.gameObject.SetActive(false);
  }

  private void EditBlockFromList(BlockData b) {
    currentBlock = b;
    ShowBlock();
  }

  private void DeleteBlockFromList(BlockData b) {
    int id = b.id;
    blocks.Remove(b);
    for (int i = 0; i < music.blocks.Count; i++)
      if (music.blocks[i] == id) music.blocks[i] = -1;

    Blocks();
  }

  public void CreateBlock() {
    Transform last = ContentsBlocks.GetChild(ContentsBlocks.childCount - 1);
    // Find the ID
    int id = 0;
    foreach (BlockData bb in blocks)
      if (bb.id > id) id = bb.id;
    id++;
    BlockData b = new BlockData() { id = id, name = "New Block", bpm = music.bpm, len = music.defLen };
    b.chs = new List<NoteData>[8];
    for (int i = 0; i < 8; i++) {
      b.chs[i] = new List<NoteData>();
    }
    for (int n = 0; n < music.defLen; n++) {
      for (int j = 0; j < 8; j++) {
        b.chs[j].Add(new NoteData());
      }
    }
    blocks.Add(b);
    currentBlock = b;

    GameObject line = Instantiate(BlockListLineTemplate, ContentsBlocks);
    line.SetActive(true);
    BlockListLine bll = line.GetComponent<BlockListLine>();
    bll.BlockID.text = b.id.ToString();
    bll.BlockName.text = b.name;
    bll.BlockLen.text = b.len.ToString();
    bll.BlockBPM.text = b.bpm.ToString();
    bll.Delete.onClick.AddListener(() => DeleteBlockFromList(b));
    bll.Edit.onClick.AddListener(() => EditBlockFromList(b));
    int linenum = bllines.Count;
    bll.LineButton.onClick.AddListener(() => SelectRow(linenum));
    bllines.Add(bll);
    last.SetAsLastSibling();
    ShowBlockInfo();
    Blocks();
    if (status == MusicEditorStatus.BlockList)
      SelectRow(bllines.Count - 1);
    else if (status == MusicEditorStatus.BlockEdit)
      SelectRow(0);
  }


  #endregion

  #region Waves **********************************************************************************************************************************************************
  public Transform WavePickContainer;
  public GameObject SelectWaveButton;

  void WavesRegenerate() {
    foreach (Transform t in ContentsWaves)
      Destroy(t.gameObject);
    int pos = 0;
    wlines.Clear();
    foreach (Wave w in waves) {
      GameObject line = Instantiate(WaveLineTemplate, ContentsWaves);
      WaveLine wl = line.GetComponent<WaveLine>();
      line.SetActive(true);
      wl.id = w.id;
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
    Instantiate(CreateNewWaveInList, ContentsWaves).SetActive(true);
  }

  public void Waves() { // Show a list of waves
    ShowSection(MusicEditorStatus.Waveforms);
    SelectedCol.gameObject.SetActive(false);
  }

  private void EditWaveFromList(Wave w) {
    throw new NotImplementedException();
  }

  private void DeleteWaveFromList(Wave w) {
    throw new NotImplementedException();
  }

  public void CreateNewWave() {
    Transform last = ContentsWaves.GetChild(ContentsWaves.childCount - 1);

    // Find the ID
    int id = 0;
    foreach (Wave ww in waves)
      if (ww.id > id) id = ww.id;
    id++;
    Wave w = new Wave() { id = id, name = "No name" };
    waves.Add(w);
    currentWave = w;
    Waves();

    GameObject line = Instantiate(WaveLineTemplate, ContentsWaves);
    WaveLine wl = line.GetComponent<WaveLine>();
    line.SetActive(true);
    wl.id = w.id;
    wl.WaveID.text = w.id.ToString();
    wl.WaveName.text = w.name;
    wl.WaveType.text = w.wave.ToString();
    wl.WaveTypeImg.sprite = WaveSprites[(int)w.wave];
    wl.Delete.onClick.AddListener(() => DeleteWaveFromList(w));
    wl.Edit.onClick.AddListener(() => EditWaveFromList(w));
    int linenum = wlines.Count;
    wl.LineButton.onClick.AddListener(() => SelectRow(linenum));
    wlines.Add(wl);
    last.SetAsLastSibling();
    ShowWave();
    SelectRow(waves.Count - 1);
  }



  void ShowWave() {
    if (currentWave == null) return;
    WaveNameInput.SetTextWithoutNotify(currentWave.name);
    WaveTypeName.text = currentWave.wave.ToString();
    WaveTypeImg.sprite = WaveSprites[(int)currentWave.wave];
    WaveNameID.text = "[" + currentWave.id.ToString() + "]";
    sounds.Wave(0, currentWave.wave, currentWave.phase);
    sounds.ADSR(0, currentWave.a, currentWave.d, currentWave.s, currentWave.r);
  }

  public WaveformEditor editor;

  public void CopyFromWaveEditor() {
    if (currentWave == null) return;
    Wave w = editor.Export();
    currentWave.CopyForm(w);
    foreach(WaveLine wl in wlines) {
      if (wl.id == currentWave.id) {
        wl.WaveID.text = currentWave.id.ToString();
        wl.WaveName.text = currentWave.name;
        wl.WaveType.text = currentWave.wave.ToString();
        wl.WaveTypeImg.sprite = WaveSprites[(int)currentWave.wave];
        break;
      }
    }
    ShowWave();
  }

  public void CopyToWaveEditor() {
    if (currentWave == null) return;
    editor.Import(currentWave);
  }

  public void EditInWaveEditor() {
    CopyToWaveEditor();
    editor.gameObject.SetActive(true);
    gameObject.SetActive(false);
  }

  public void UpdateWaveName(bool completed) {
    inputsSelected = !completed;
    if (currentWave == null) return;
    if (completed) {
      currentWave.name = WaveNameInput.text;

      if (status != MusicEditorStatus.Waveforms || wlines == null || wlines.Count == 0) return;
      foreach (WaveLine wl in wlines) {
        if (wl.id == currentWave.id) {
          wl.WaveName.text = currentWave.name;
          wl.WaveType.text = currentWave.wave.ToString();
          wl.WaveTypeImg.sprite = editor.WaveSprites[(int)currentWave.wave];
          return;
        }
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

    if (col < 0 || col >= music.NumVoices) return;
    if (row < 0 || row >= currentBlock.chs[col].Count) return;

    NoteData bn = currentBlock.chs[col][row];
    bn.type = NoteType.Wave;
    bn.val = w.id;
    blines[row].note[col].SetWave(w.id, w.name, NoteTypeSprites[(int)NoteType.Wave]);
    ShowNote(bn);
  }

  private Wave GetWave(int val) {
    foreach (Wave w in waves)
      if (w.id == val) return w;
    return null;
  }


  #endregion

  #region Play **********************************************************************************************************************************************************

  void SetTapeButtonColor(int num) {
    ColorBlock cols;
    for (int i = 0; i < TapeButtons.Length; i++) {
      cols = TapeButtons[i].colors;
      cols.normalColor = TapeButtonColor;
      TapeButtons[i].colors = cols;
    }
    if (num != -1) {
      cols = TapeButtons[num].colors;
      cols.normalColor = TapeButtonBlue;
      TapeButtons[num].colors = cols;
    }
  }

  public void Record() {
    if (currentBlock == null || blocks.Count == 0) return;
    if (recording) {
      recording = false;
      SetTapeButtonColor(-1);
      return;
    }
    recording = true;
    timeForNextBeat = 60f / currentBlock.bpm;
    countInForRecording = 4 * timeForNextBeat;
    playing = false;
    ShowSection(MusicEditorStatus.BlockEdit);
    row = 0;
    SelectRow(row);
    SetTapeButtonColor(0);
  }

  public void Rewind() {
    SetTapeButtonColor(-1);
  }

  public void Play() {
    currentPlayedMusicLine = row;
    timeForNextBeat = 0;
    playing = true;
    repeat = false;
    SetTapeButtonColor(2);
  }

  public void Repeat() {
    row--;
    timeForNextBeat = 0;
    playing = true;
    repeat = true;
    SetTapeButtonColor(3);
  }

  public void Pause() {
    playing = !playing;
    if (playing) {
      if (repeat)
        SetTapeButtonColor(3);
      else
        SetTapeButtonColor(2);
    }
    else
      SetTapeButtonColor(4);
  }

  public void Stop() {
    recording = false;
    playing = false;
    SelectRow(0);
    SetTapeButtonColor(-1);
  }

  public void FastForward() {
    SelectRow(10000);
    SetTapeButtonColor(-1);
  }

  #endregion


  #region Load/Save **********************************************************************************************************************************************************

  public InputField Values;
  public Button LoadSubButton;

  /*
 musicname:
  numvoices, numblocks, numwaves, numlines
  [for each line]
    block id

 [for each block]
  blockname:
   id, len, bpm
   [for each line]
    [for each note]
      type, val, len
 [for each wave]
  wavename:
  type, phase[2bytes], a, d, s, r
  [in case PCM] len[4 bytes] [pcm data]
 */

  public void Save() {
    int numv = music.NumVoices;
    string res = music.name.Replace(":", "") + ":\n" +
                numv.ToString("X2") + " " +
                blocks.Count.ToString("X2") + " " +
                waves.Count.ToString("X2") + " " +
                music.blocks.Count.ToString("X2") + "\n";
    for (int i = 0; i < music.blocks.Count; i++) {
      byte val = (byte)(music.blocks[i] < 1 ? 255 : music.blocks[i]);
      res += val.ToString("X2") + " ";
    }
    res += "\n";

    foreach(BlockData b in blocks) {
      res += b.name.Replace(":", "") + ":\n" +
            b.id.ToString("X2") + " " +
            b.len.ToString("X2") + " " +
            b.bpm.ToString("X2") + "\n";
      for (int r = 0; r < b.len; r++) {
        for (int c = 0; c < numv; c++) {
          NoteData note = b.chs[c][r];
          byte ph = (byte)((note.val & 0xff00) >> 8);
          byte pl = (byte)(note.val & 0xff);
          res += ((int)note.type).ToString("X2") + " " +
                ph.ToString("X2") + " " + pl.ToString("X2") + " " +
                note.len.ToString("X2") + " ";
        }
      }
      res += "\n";
    }

    foreach(Wave w in waves) {
      byte ph = (byte)((((int)(w.phase * 100)) & 0xff00) >> 8);
      byte pl = (byte)(((int)(w.phase * 100)) & 0xff);

      res += w.name.Replace(":", "") + ":\n" +
            w.id.ToString("X2") + " " +
            ((int)w.wave).ToString("X2") + " " +
            ph.ToString("X2") + " " +
            pl.ToString("X2") + " " +
            w.a.ToString("X2") + " " +
            w.d.ToString("X2") + " " +
            w.s.ToString("X2") + " " +
            w.r.ToString("X2");
      if (w.wave == Waveform.PCM) {
        res += "\n";
        byte l3 = (byte)((w.rawPCM.Length & 0xff000000) >> 24);
        byte l2 = (byte)((w.rawPCM.Length & 0xff0000) >> 16);
        byte l1 = (byte)((w.rawPCM.Length & 0xff00) >> 8);
        byte l0 = (byte)((w.rawPCM.Length & 0xff) >> 0);
        res += l3.ToString("X2") + " " +
              l2.ToString("X2") + " " +
              l1.ToString("X2") + " " +
              l0.ToString("X2") + "\n";
        for (int i = 0; i < w.rawPCM.Length; i++) {
          res += w.rawPCM[i].ToString("X2") + " ";
        }
      }
      res += "\n";
    }

    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = false;
    Values.text = res;
  }

  public void PreLoad() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }
  readonly Regex rgComments = new Regex("([^\\n]*)(//[^\\n]*)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgLabels = new Regex("[\\s]*[a-z0-9]+:[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgHex1 = new Regex("[\\s]*0x([a-f0-9]+)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgHex2 = new Regex("[\\s]*([a-f0-9]+)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  public void PostLoad() {
    if (!gameObject.activeSelf) return;
    string data = Values.text.Trim();
    data = rgComments.Replace(data, " ").Replace('\n', ' ').Trim();
    while (data.IndexOf("  ") != -1) data = data.Replace("  ", " ");

    waves.Clear();
    blocks.Clear();
    MusicData m = new MusicData() {
      bpm = 120,
      voices = new byte[] { 0, 1, 2, 3, 255, 255, 255, 255 },
      blocks = new List<int>()
    };
    int pos = data.IndexOf(':');
    if (pos == -1) throw new Exception("Missing Music label");
    m.name = data.Substring(0, pos).Trim();
    data = data.Substring(pos + 1).Trim();
    byte data1, data2, data3, data4;
    byte numw, numb, numv;

    data = ReadNextByte(data, out data1);
    numv = data1;
    for (int i = 0; i < 8; i++)
      m.voices[i] = (byte)((i < numv) ? i : 255);
    data = ReadNextByte(data, out numb);
    data = ReadNextByte(data, out numw);
    data = ReadNextByte(data, out data2);
    for (int i = 0; i < data2; i++) {
      data = ReadNextByte(data, out data1);
      m.blocks.Add(data1 == 255 ? -1 : data1);
    }

    for (int i = 0; i < numb; i++) {
      pos = data.IndexOf(':');
      if (pos == -1) throw new Exception("Missing Block label for block #" + (i + 1));
      BlockData b = new BlockData {
        name = data.Substring(0, pos).Trim(),
        chs = new List<NoteData>[] {
          new List<NoteData>(), new List<NoteData>(), new List<NoteData>(), new List<NoteData>(), 
          new List<NoteData>(), new List<NoteData>(), new List<NoteData>(), new List<NoteData>() 
        }
      };

      data = data.Substring(pos + 1).Trim();
      data = ReadNextByte(data, out data1);
      data = ReadNextByte(data, out data2);
      data = ReadNextByte(data, out data3);
      b.id = data1;
      b.len = data2;
      b.bpm = data3;

      for (int r = 0; r < b.len; r++) {
        for (int c = 0; c < numv; c++) {
          data = ReadNextByte(data, out data1);
          data = ReadNextByte(data, out data2);
          data = ReadNextByte(data, out data3);
          data = ReadNextByte(data, out data4);

          NoteData note = new NoteData() {
            type = (NoteType)data1,
            val = (data2 << 8) + data3,
            len = data4
          };

          b.chs[c].Add(note);
        }
      }

      blocks.Add(b);
    }

    for (int i = 0; i < numw; i++) {
      pos = data.IndexOf(':');
      if (pos == -1) throw new Exception("Missing Wave label for wave #" + (i + 1));
      Wave w = new Wave {
        name = data.Substring(0, pos).Trim()
      };
      data = data.Substring(pos + 1).Trim();

      data = ReadNextByte(data, out data1);
      data = ReadNextByte(data, out data2);
      data = ReadNextByte(data, out data3);
      data = ReadNextByte(data, out data4);

      w.id = data1;
      w.wave = (Waveform)data2;

      w.phase = ((data3 << 8) + data4) / 100f;
      data = ReadNextByte(data, out w.a);
      data = ReadNextByte(data, out w.d);
      data = ReadNextByte(data, out w.s);
      data = ReadNextByte(data, out w.r);

      if (w.wave == Waveform.PCM) {
        data = ReadNextByte(data, out data1);
        data = ReadNextByte(data, out data2);
        data = ReadNextByte(data, out data3);
        data = ReadNextByte(data, out data4);

        int len = (data1 << 24) + (data2 << 16) + (data3 << 8) + (data4 << 0);
        w.rawPCM = new byte[len];
        for (int b = 0; b < len; b++) {
          data = ReadNextByte(data, out data1);
          w.rawPCM[b] = data1;
        }
      }

      waves.Add(w);
    }

    music = m;

    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;

    MusicRegenerate();
    BlocksRegenerate();
    WavesRegenerate();
  }

  string ReadNextByte(string data, out byte res) {
    int pos1 = data.IndexOf(' ');
    int pos2 = data.IndexOf('\n');
    int pos3 = data.Length;
    if (pos1 == -1) pos1 = int.MaxValue;
    if (pos2 == -1) pos2 = int.MaxValue;
    if (pos3 == -1) pos3 = int.MaxValue;
    int pos = pos1;
    if (pos > pos2) pos = pos2;
    if (pos > pos3) pos = pos3;
    if (pos < 1) {
      res = 0;
      return "";
    }

    string part = data.Substring(0, pos);
    Match m = rgHex1.Match(part);
    if (m.Success) {
      res = (byte)Convert.ToInt32(m.Groups[1].Value, 16);
      return data.Substring(pos).Trim();
    }
    else {
      m = rgHex2.Match(part);
      if (m.Success) {
        res = (byte)Convert.ToInt32(m.Groups[1].Value, 16);
        return data.Substring(pos).Trim();
      }
    }

    res = 0;
    return data;
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


public class MusicData {
  public string name;
  public int bpm;
  public int defLen;
  public byte[] voices;
  public List<int> blocks;

  public int NumVoices { get {
      int numv = 0;
      for (int i = 0; i < voices.Length; i++)
        if (voices[i] != 255) numv++;
      return numv;
    }
  }
}

public class BlockData {
  public int id;
  public string name;
  public int bpm;
  public int len;
  public List<NoteData>[] chs;
}

public class Note {
  public NoteType type;
  public int val;
  public int len;

  internal Note Duplicate() {
    return new Note { type = this.type, val = this.val, len = this.len };
  }

  internal void Set(Note src) {
    type = src.type;
    val = src.val;
    len = src.len;
  }

  internal void Set(NoteLine note) {
    type = note.type;
    val = note.val;
    len = note.len;
  }

  internal void Zero() {
    type = NoteType.Empty;
    val = 0;
    len = 0;
  }
}


public class Swipe {
  public float vols;
  public float vole;
  public float voltime;
  public float vollen;
  public float pitchs;
  public float pitche;
  public float pitchtime;
  public float pitchlen;
  public float pans;
  public float pane;
  public float pantime;
  public float panlen;

  public Swipe() {
    vols = 0;
    vole = 0;
    voltime = 0;
    vollen = 0;
    pitchs = 0;
    pitche = 0;
    pitchtime = 0;
    pitchlen = 0;
    pans = 0;
    pane = 0;
    pantime = 0;
    panlen = 0;
  }
}

/*

Define better the way the cells are done. So we can save meory and have cells with multiple infos.

[type] if 0 then next cell, used also as terminator (len=0 can be a terminator and in this case len will be set as 1)
[type=note] [freq] [len]



Empty=0
Note=1
Wave=2
Volume=3
Pitch=4
Pan=5

[byte] -> bitfield with items
[2 bytes] -> value + len <= for each of the selected types



 */


public class NoteData {
  public byte type { get; private set; }

  struct vl {
    public short val;
    public byte len;
  };

  vl[] vls = new vl[5];

  public void Zero() {
    type = 0;
    for (int i = 0; i < 5; i++) {
      vls[i].val = 0;
      vls[i].len = 0;
    }
  }

  public void Zero(NoteType t) {
    if (t == NoteType.Empty) {
      Zero();
      return;
    }
    int pos = (byte)type - 1;
    type &= (byte)(255 - (1 << pos));
    vls[pos].val = 0;
    vls[pos].len = 0;
  }

  public bool IsType(NoteType t) {
    if (t == NoteType.Empty) return type == 0;
    int pos = (byte)type - 1;
    return (type & (1 << pos)) != 0;
  }

  public short GetVal(NoteType t) {
    if (t == NoteType.Empty) return 0;
    int pos = (byte)type - 1;
    return vls[pos].val;
  }

  public short GetLen(NoteType t) {
    if (t == NoteType.Empty) return 0;
    int pos = (byte)type - 1;
    return vls[pos].len;
  }

  public void SetVal(NoteType t, short val) {
    if (t == NoteType.Empty) return;
    int pos = (byte)type - 1;
    type |= (byte)(1 << pos);
    vls[pos].val = val;
  }

  public void SetLen(NoteType t, byte len) {
    if (t == NoteType.Empty) return;
    int pos = (byte)type - 1;
    type |= (byte)(1 << pos);
    vls[pos].len = len;
  }

  internal NoteData Duplicate() {
    NoteData n = new NoteData { type = this.type };
    for (int i = 0; i < 5; i++) {
      n.vls[i].val = vls[i].val;
      n.vls[i].len = vls[i].len;
    }
    return n;
  }

  internal void Set(NoteData src) {
    type = src.type;
    for (int i = 0; i < 5; i++) {
      vls[i].val = src.vls[i].val;
      vls[i].len = src.vls[i].len;
    }
  }

  internal void Set(NoteLine note) {
    // FIXME this will not work anymore
  }


}