﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
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

  public TMP_InputField NameInput;
  public TMP_InputField NumVoicesInputField;
  public TMP_InputField MusicBPMInputField;
  public TMP_InputField MusicDefLenInputField;
  public TMP_InputField BlockBPMInputField;
  public TMP_InputField BlockLenInputField;
  public TMP_InputField NoteLenInputField;
  public TMP_InputField StepLenInputField;

  public TextMeshProUGUI CurrentBlockID;
  public TMP_InputField BlockNameInput;
  public TMP_InputField WaveNameInput;
  public TextMeshProUGUI WaveNameID;
  public TextMeshProUGUI WaveTypeName;
  public Image WaveTypeImg;

  public GameObject MusicLineTempate;
  public GameObject BlockListLineTemplate;
  public GameObject WaveLineTemplate;
  public GameObject CreateNewBlockInMusic;
  public GameObject CreateNewBlockInList;
  public GameObject CreateNewWaveInList;

  public TextMeshProUGUI SelInfo;
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
  int quickPlayPos = -1;
  int octave = 4;
  bool editMode = true;
  float countInForRecording = 0;
  bool playing = false;
  bool repeat = false;
  readonly Swipe[] swipes = new Swipe[] {
    new Swipe(), new Swipe(), new Swipe(), new Swipe(),
    new Swipe(), new Swipe(), new Swipe(), new Swipe()
  };
  MusicEditorStatus status = MusicEditorStatus.Idle;
  readonly List<NoteData> CopiedNotes = new List<NoteData>();
  readonly System.Globalization.NumberStyles numberstyle = System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;
  readonly System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");

  public GameObject InfoTooltip;
  public TextMeshProUGUI InfoTooltipTxt;
  float infoTooltipDelay = 0;
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
      BlockLine bl = ContentsBlock.GetChild(i + 7).GetComponent<BlockLine>();
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
    sounds.Init();
  }

  private void Update() {
    bool update = false;
    autoRepeat -= Time.deltaTime;
    bool textedit = Values.gameObject.activeSelf || FileBrowser.IsVisible();

    if (InfoTooltip.activeSelf) {
      infoTooltipDelay -= Time.deltaTime;
      if (infoTooltipDelay<0) InfoTooltip.SetActive(false);
    }

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

    bool shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

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
      if (recording || quickPlayPos != -1) PlayBlock();
      if (Input.GetKeyDown(KeyCode.Space)) {
        if (quickPlayPos == -1) {
          quickPlayPos = row;
          currentPlayedMusicLine = row;
          ShowTooltip("Quick play");
        }
        else {
          row = quickPlayPos;
          update = true;
          quickPlayPos = -1;
        }
      }

      if (!recording && !textedit) {
        if (status == MusicEditorStatus.BlockEdit) {
          if (!shift && Input.GetKey(KeyCode.UpArrow) && blines != null && row > 0 && autoRepeat < 0) { row--; update = true; autoRepeat = .1f; }
          if (!shift && Input.GetKey(KeyCode.DownArrow) && blines != null && row < currentBlock.len - 1 && autoRepeat < 0) { row++; update = true; autoRepeat = .1f; }
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
      if (status == MusicEditorStatus.BlockEdit && !textedit) {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && col > 0) { col--; update = true; autoRepeat = .25f; }
        if (Input.GetKeyDown(KeyCode.RightArrow) && col < 7) { col++; update = true; autoRepeat = .25f; }
      }

      if (!recording && !textedit) {
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

      if (status == MusicEditorStatus.BlockEdit && row > -1 && row < blines.Count && !textedit) {
        BlockLine l = blines[row];
        if (!recording) {
          // Remove cell values
          if (Input.GetKeyDown(KeyCode.Delete)) {
            currentBlock.chs[col][row].Zero();
            blines[row].note[col].SetZeroValues(NoteTypeSprites);
          }
          // Change length
          if (l.note[col].IsNote()) {
            if (Input.GetKeyDown(KeyCode.PageUp)) UpdateNoteLength(currentBlock.chs[col][row].GetLen(NoteType.Note) - 1, currentBlock.len - row);
            if (Input.GetKeyDown(KeyCode.PageDown)) UpdateNoteLength(currentBlock.chs[col][row].GetLen(NoteType.Note) + 1, currentBlock.len - row);
          }
          else if (l.note[col].IsWave()) { // Change wave
            if (Input.GetKeyDown(KeyCode.PageUp)) {
              UpdateNoteWave(currentBlock.chs[col][row], -1);
            }
            if (Input.GetKeyDown(KeyCode.PageDown)) {
              UpdateNoteWave(currentBlock.chs[col][row], +1);
            }
          }
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) {
          editMode = !editMode;
          if (editMode)
            ShowTooltip("Edit mode");
          else
            ShowTooltip("Free play mode");
        }

        if (Input.GetKeyDown(KeyCode.F1)) { octave = 0; ShowTooltip("1st Octave"); }
        if (Input.GetKeyDown(KeyCode.F2)) { octave = 1; ShowTooltip("2nd Octave"); }
        if (Input.GetKeyDown(KeyCode.F3)) { octave = 2; ShowTooltip("3rd Octave"); }
        if (Input.GetKeyDown(KeyCode.F4)) { octave = 3; ShowTooltip("4th Octave"); }
        if (Input.GetKeyDown(KeyCode.F5)) { octave = 4; ShowTooltip("5th Octave"); }
        if (Input.GetKeyDown(KeyCode.F6)) { octave = 5; ShowTooltip("6th Octave"); }
        if (Input.GetKeyDown(KeyCode.F7)) { octave = 6; ShowTooltip("7th Octave"); }
        if (Input.GetKeyDown(KeyCode.F8)) { octave = 7; ShowTooltip("8th Octave"); }

        if (!textedit) // Piano keys
          for (int i = 0; i < keyNotes.Length; i++) {
            if (Input.GetKeyDown(keyNotes[i])) {
              if (editMode) {
                // Set the current cell as note with the given note/frequency, update the text to be the note notation
                NoteData nd = currentBlock.chs[col][row];
                nd.Set(NoteType.Note, (short)freqs[i + octave * 12], (byte)noteLen);
                l.note[col].SetValues(nd, NoteTypeSprites, freqs, noteNames, waves);
                // Move to the next row
                if (!recording && row + stepLen < currentBlock.len) { row += stepLen; update = true; }
              }
              // Play the actual sound (find the wave that should be used, if none is defined use a basic triangle wave)
              sounds.Play(col, freqs[i + octave * 12], .25f);
            }
          }
      }
    }

    // We shoul avoid the keys if we are with wrong lines selected or saving/loading
    // Ctrl+C, Ctrl+V, Ctrl+X, ShiftUp, ShiftDown
    if (!textedit) {
      if (shift) {
        seldelay -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.DownArrow) || (Input.GetKey(KeyCode.DownArrow) && seldelay < 0)) {
          seldelay = .1f;
          if (selectionYStart == -1 || row > selectionYEnd + 2 || row < selectionYStart - 2) { // No selection
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

        if (Input.GetKeyDown(KeyCode.UpArrow) || (Input.GetKey(KeyCode.UpArrow) && seldelay < 0)) {
          seldelay = .1f;
          if (selectionYStart == -1 || row > selectionYEnd + 2 || row < selectionYStart - 2) { // No selection
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
      } else seldelay = 0;
      if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) { // Ctrl+C
        CopiedNotes.Clear();
        if (selectionYStart == -1 || selectionYEnd == -1) {
          selectionYStart = row;
          selectionYEnd = row;
        }
        for (int i = selectionYStart; i <= selectionYEnd; i++) {
          CopiedNotes.Add(currentBlock.chs[col][i].Duplicate());
        }
        HideSelection();
        if (CopiedNotes.Count == 0)
          SelInfo.text = "";
        else if (CopiedNotes.Count == 1) {
          SelInfo.text = CopiedNotes.Count + " cell copied";
          ShowTooltip(SelInfo.text);
        }
        else {
          SelInfo.text = CopiedNotes.Count + " cells copied";
          ShowTooltip(SelInfo.text);
        }
      }
      if (Input.GetKeyDown(KeyCode.X) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) { // Ctrl+X
        CopiedNotes.Clear();
        if (selectionYStart == -1 || selectionYEnd == -1) {
          selectionYStart = row;
          selectionYEnd = row;
        }
        for (int i = selectionYStart; i <= selectionYEnd; i++) {
          CopiedNotes.Add(currentBlock.chs[col][i].Duplicate());
          currentBlock.chs[col][i].Zero();
          blines[i].note[col].SetZeroValues(NoteTypeSprites);
        }
        HideSelection();
        if (CopiedNotes.Count == 0)
          SelInfo.text = "";
        else if (CopiedNotes.Count == 1) {
          SelInfo.text = CopiedNotes.Count + " cell cut";
          ShowTooltip(SelInfo.text);
        }
        else {
          SelInfo.text = CopiedNotes.Count + " cells cut";
          ShowTooltip(SelInfo.text);
        }
        }
        if (Input.GetKeyDown(KeyCode.V) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && CopiedNotes.Count > 0) { // Ctrl+V
                                                                                                                                               // If we have something, paste starting from the current position
        for (int i = 0; i < CopiedNotes.Count && row + i < currentBlock.len; i++) {
          currentBlock.chs[col][row + i].Set(CopiedNotes[i]);
          blines[row + i].note[col].SetValues(CopiedNotes[i], NoteTypeSprites, freqs, noteNames, waves);
        }
        HideSelection();
      }
    }
    if (update) {
      ScrollViews(row); // Scroll if needed
      SelectedCol.anchoredPosition = new Vector3(48 + col * 142, 30, 0);
      SelectRow(row);
    }
  }

  float seldelay = 0;

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
    pos.y = -blines[currentBlock.len - 1].GetComponent<RectTransform>().anchoredPosition.y + 477.8f - 32 * selectionYStart;
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
      len = currentBlock.len;
    }
    else if (status == MusicEditorStatus.Waveforms) {
      bar = scrollWaves;
      len = wlines.Count - 14;
    }
    else return;

//    if (where < 13) bar.value = 1;
//    if (where > len) bar.value = 0;
//    else bar.value = -0.0276f * where + 1.333333333333333f;
//    bar.value = -0.0158f * where + 1;

    bar.value = (0.0001243125f * len - 0.023776f) * where + 1;


    // 64 lines 0->1
    // 64 lines 20->6.840796
    // 64 lines 40->3.674583
    // 64 lines 60->0.51287
    // 64 lines 63->0.03242


    // 128 0->1
    // 128 60>0.5287688
    // 128 100->0.213911
    // 128 127->0.001255

    // -0.01582 for 64
    // -0.007864 for 128

    /*
     
     -0.01582 = 64a+b
     -0.007864 = 128a+b
     

     64a+0.01582 = -b

     0.01582 - 0.007864= 64a
     
    a = 0.0001243125
    b = -0.023776
     
     */



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
      if (quickPlayPos!=-1) {
        currentPlayedMusicLine = quickPlayPos;
        return;
      }
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


  private bool PlayNote(BlockData block) {
    if (block == null) return true;

    timeForNextBeat = 15f / block.bpm;
    for (int c = 0; c < music.NumVoices; c++) {
      NoteData n = block.chs[c][currentPlayedMusicLine];
      if (n.IsType(NoteType.Note)) {
        sounds.Play(c, n.GetVal(NoteType.Note), n.GetLen(NoteType.Note) * timeForNextBeat);
      }
      if (n.IsType(NoteType.Wave)) {
        Wave w = GetWave(n.GetVal(NoteType.Wave));
        if (w != null) {
          sounds.Wave(c, w.wave, w.phase);
          sounds.ADSR(c, w.a, w.d, w.s, w.r);
          if (w.rawPCM != null) sounds.Wave(c, w.rawPCM);
        }
      }
      if (n.IsType(NoteType.Volume)) {
        if (n.GetLen(NoteType.Volume) < 2) {
          sounds.Volume(c, n.GetVol());
        }
        else {
          swipes[c].vols = sounds.Volume(c);
          swipes[c].vole = n.GetVol();
          swipes[c].voltime = 0;
          swipes[c].vollen = (n.GetLen(NoteType.Volume) - 1) * 15f / block.bpm;
        }
      }
      if (n.IsType(NoteType.Pitch)) {
        if (n.GetLen(NoteType.Pitch) < 2) {
          sounds.Pitch(c, n.GetPitch());
        }
        else {
          swipes[c].pitchs = sounds.Pitch(c);
          swipes[c].pitche = n.GetPitch();
          swipes[c].pitchtime = 0;
          swipes[c].pitchlen = (n.GetLen(NoteType.Pitch) - 1) * 15f / block.bpm;
        }
      }
      if (n.IsType(NoteType.Pan)) {
        if (n.GetLen(NoteType.Pan) < 2) {
          sounds.Pan(c, n.GetPan());
        }
        else {
          swipes[c].pans = sounds.Pan(c);
          swipes[c].pane = n.GetPan();
          swipes[c].pantime = 0;
          swipes[c].panlen = (n.GetLen(NoteType.Pan) - 1) * 15f / block.bpm;
        }
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
        if (notes[i].IsType(NoteType.Wave)) {
          Wave w = null;
          int id = notes[i].GetVal(NoteType.Wave);
          for (int widx = 0; widx < waves.Count; widx++) {
            if (waves[widx].id == id) {
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
      ml.Delete.onClick.AddListener(() => RemoveCurrentMusicLine(ml)); // Fixme, when the lines are deleted th eindex are no more valid.
      ml.Up.onClick.AddListener(() => MoveCurrentMusicLineUp(ml));
      ml.Down.onClick.AddListener(() => MoveCurrentMusicLineDown(ml));
      ml.Edit.onClick.AddListener(() => EditCurrentMusicLineBlock(ml));
      ml.Pick.onClick.AddListener(() => PickCurrentMusicLineBlock(ml));
      ml.LineButton.onClick.AddListener(() => SelectMusicLineRow(ml));
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
    ml.Edit.onClick.AddListener(() => EditCurrentMusicLineBlock(ml));
    ml.Pick.onClick.AddListener(() => PickCurrentMusicLineBlock(ml));
    ml.LineButton.onClick.AddListener(() => SelectMusicLineRow(ml));
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

  public void EditCurrentMusicLineBlock(MusicLine line) {
    int id = -1;
    for (int i = 0; i < mlines.Count; i++) 
      if (mlines[i] == line) {
        id = music.blocks[i];
        break;
    }
    if (id == -1) return;
    foreach (BlockData b in blocks)
      if (b.id == id) {
        currentBlock = b;
        ShowBlock();
        return;
      }
  }

  public void PickCurrentMusicLineBlock(MusicLine line) {
    // pick the current block and show it, if missing create a new one
    for (int i = 0; i < mlines.Count; i++)
      if (mlines[i] == line) {
        row = i;
        break;
      }
    BlockPickContainer.parent.gameObject.SetActive(true);
    PickBlockSetup();
  }

  void SelectMusicLineRow(MusicLine line) {
    for (int i = 0; i < mlines.Count; i++)
      if (mlines[i] == line) {
        SelectRow(i);
        return;
      }
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
      sbb.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "[" + b.id + "] " + b.name;
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
    UpdateBlockLen(len);
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
      UpdateBlockLen(len);
    }
  }
  void UpdateBlockLen(int len) {
    if (currentBlock == null) return;
    currentBlock.len = len;
    for (int r = 0; r < 128; r++) {
      blines[r].gameObject.SetActive(r < len);
    }
    while (currentBlock.chs[0].Count < len) {
      for (int c = 0; c < currentBlock.chs.Length; c++)
        currentBlock.chs[c].Add(new NoteData());
    }
    if (currentBlock.chs[0].Count > len) {
      for (int c = 0; c < currentBlock.chs.Length; c++)
        currentBlock.chs[c].RemoveRange(len, currentBlock.chs[c].Count - len);
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
  int stepLen = 1;

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

  private void UpdateNoteLength(int len = -1, int max = 99999) {
    NoteLine note = blines[row].note[col];
    NoteData bn = currentBlock.chs[col][row];
    if (!note.IsNote()) {
      note.LenTxt.text = "";
      note.back.sizeDelta = new Vector2(38, 0 * 32);
      ShowNote(bn);
      return;
    }

    byte val = (byte)((len == -1) ? noteLen : len);
    if (val < 1) val = 1;
    if (val > max) val = (byte)max;
    bn.SetLen(NoteType.Note, val);
    note.LenTxt.text = val.ToString();
    note.back.sizeDelta = new Vector2(38, val * 32);
    ShowNote(bn);
  }

  private void UpdateNoteWave(NoteData nd, int dir) {
    int id = nd.GetVal(NoteType.Wave);
    int newid = id;
    if (dir == 1) {
      for (int i = 0; i < waves.Count; i++) {
        if (waves[i].id == id) {
          if (i == waves.Count - 1)
            newid = waves[0].id;
          else
            newid = waves[i + 1].id;
        }
      }
    }
    else {
      for (int i = 0; i < waves.Count; i++) {
        if (waves[i].id == id) {
          if (i == 0)
            newid = waves[waves.Count - 1].id;
          else
            newid = waves[i - 1].id;
        }
      }
    }
    nd.SetVal(NoteType.Wave, (short)newid);
    NoteLine nl = blines[row].note[col];
    nl.SetValues(nd, NoteTypeSprites, freqs, noteNames, waves);
    ShowNote(nd);
  }

  #endregion

  #region Cell **********************************************************************************************************************************************************
  public TextMeshProUGUI CellInfoTxt;

  public TMP_InputField[] CellValInputs;
  public TMP_InputField[] CellLenInputs;
  public Image[] CellSelecteds;
  public Sprite Unchecked;
  public Sprite Checked;

  private void ShowNote(NoteData note) {
    if (note == null) {
      for (int i = 0; i < 5; i++)
        CellSelecteds[i].sprite = Checked;
      CellInfoTxt.text = "<i>select a cell...</i>";
      return;
    }

    if (note.IsType(NoteType.Note)) {
      CellSelecteds[0].sprite = Checked;
      // Find the closest note and the freq
      string nv = null;
      int num = note.GetVal(NoteType.Note);
      for (int i = 0; i < freqs.Length - 1; i++) {
        if (num == freqs[i]) {
          nv = noteNames[i] + " - " + num;
          break;
        }
        if (num > freqs[i] && num < freqs[i + 1]) {
          if (num - freqs[i] < freqs[i + 1] - num)
            nv = "~" + noteNames[i] + " - " + num;
          else
            nv = "~" + noteNames[i + 1] + " - " + num;
          break;
        }
      }
      if (nv == null) nv = num.ToString();
      CellValInputs[0].SetTextWithoutNotify(nv);
      CellLenInputs[0].SetTextWithoutNotify(note.GetLen(NoteType.Note).ToString());
    }
    else {
      CellSelecteds[0].sprite = Unchecked;
    }

    if (note.IsType(NoteType.Wave)) {
      CellSelecteds[1].sprite = Checked;
      int num = note.GetVal(NoteType.Wave);
      CellValInputs[1].SetTextWithoutNotify(num.ToString());
      foreach (Wave w in waves) {
        if (w.id == num) {
          CellValInputs[1].SetTextWithoutNotify(num.ToString() + " " + w.name);
          break;
        }
      }
    }
    else {
      CellSelecteds[1].sprite = Unchecked;
    }

    if (note.IsType(NoteType.Volume)) {
      CellSelecteds[2].sprite = Checked;
      CellValInputs[2].SetTextWithoutNotify(NoteData.ConvertVal2Vol(note.GetVal(NoteType.Volume)));
      CellLenInputs[2].SetTextWithoutNotify(note.GetLen(NoteType.Volume).ToString());
    }
    else {
      CellSelecteds[2].sprite = Unchecked;
    }

    if (note.IsType(NoteType.Pitch)) {
      CellSelecteds[3].sprite = Checked;
      CellValInputs[3].SetTextWithoutNotify(NoteData.ConvertVal2Pitch(note.GetVal(NoteType.Pitch)));
      CellLenInputs[3].SetTextWithoutNotify(note.GetLen(NoteType.Pitch).ToString());
    }
    else {
      CellSelecteds[3].sprite = Unchecked;
    }

    if (note.IsType(NoteType.Pan)) {
      CellSelecteds[4].sprite = Checked;
      CellValInputs[4].SetTextWithoutNotify(NoteData.ConvertVal2Pan(note.GetVal(NoteType.Pan)));
      CellLenInputs[4].SetTextWithoutNotify(note.GetLen(NoteType.Pan).ToString());
    }
    else {
      CellSelecteds[4].sprite = Unchecked;
    }

    CellInfoTxt.text = "Row: " + row + "  Channel: " + (col + 1);
  }

  public void ClearNote() {
    inputsSelected = false;
    NoteData nd = currentBlock.chs[col][row];
    nd.Zero();
    CellSelecteds[0].sprite = Unchecked;
    CellSelecteds[1].sprite = Unchecked;
    CellSelecteds[2].sprite = Unchecked;
    CellSelecteds[3].sprite = Unchecked;
    CellSelecteds[4].sprite = Unchecked;
    NoteLine nl = blines[row].note[col];
    nl.SetValues(nd, NoteTypeSprites, freqs, noteNames, waves);
  }

  public void CellAlterNote(int mode) {
    CellAlterType(0, mode);
  }
  public void CellAlterWave(int mode) {
    CellAlterType(1, mode);
  }
  public void CellAlterVol(int mode) {
    CellAlterType(2, mode);
  }
  public void CellAlterPitch(int mode) {
    CellAlterType(3, mode);
  }
  public void CellAlterPan(int mode) {
    CellAlterType(4, mode);
  }

  void CellAlterType(int type, int mode) {
    if (mode == 0) { // Used to only start typing on input fields
      inputsSelected = true;
      return;
    }

    NoteData nd = currentBlock.chs[col][row];
    NoteType t = (NoteType)(type + 1);
    if (mode == 1) { // Completed typing on input fields
      inputsSelected = false;
    }
    else if (mode == 2) { // Just changed the type
      inputsSelected = false;
      if (nd.IsType(t)) {
        nd.Zero(t);
        CellSelecteds[type].sprite = Unchecked;
        blines[row].note[col].SetValues(nd, NoteTypeSprites, freqs, noteNames, waves);
        return;
      }
    }

    CellSelecteds[type].sprite = Checked;
    // Val depends on the type
    short val = 0;
    switch(t) {
      case NoteType.Empty: break;
      case NoteType.Note: {
        string note = CellValInputs[type].text.Trim().ToLowerInvariant();
        // The value can be a note or a frequency
        int notepos = -1;
        for (int i = 0; i < noteNames.Length; i++)
          if (noteNames[i].ToLowerInvariant() == note) {
            notepos = i;
            break;
          }
        if (notepos != -1) val = (short)freqs[notepos];
        else if (int.TryParse(note, out int freq)) val = (short)freq;
      }
      break;

      case NoteType.Wave: {
        string note = CellValInputs[type].text.Trim().ToLowerInvariant();
        // The value can be an id or a name
        int.TryParse(note, out int waveid);
        if (waveid == 0 && waves.Count > 0) waveid = waves[0].id;
        for (int i = 0; i < waves.Count; i++) {
          if (waves[i].name.Trim().ToLowerInvariant() == note || waves[i].id == waveid) {
            val = (short)waves[i].id;
            break;
          }
        }
      }
      break;

      case NoteType.Volume: {
        if (int.TryParse(CellValInputs[type].text.Trim(), out int vol)) {
          if (vol < 0) vol = 0;
          if (vol > 100) vol = 100;
          val = NoteData.ConvertVol2Val(vol);
          break;
        }
      }
      break;

      case NoteType.Pitch: {
        // 1.05946^numsemitones
        // Values can be +[0-9]+(.[0-9]+)? and -[0-9]+(.[0-9]+)?
        if (float.TryParse(CellValInputs[type].text.Trim(), numberstyle, culture, out float fVal)) {
          if (fVal > 320) fVal = 320;
          if (fVal < -320) fVal = -320;
          val = NoteData.ConvertPitch2Val(fVal);
          break;
        }
      }
      break;

      case NoteType.Pan: {
        if (float.TryParse(CellValInputs[type].text.Trim(), numberstyle, culture, out float pval)) {
          if (pval < -1) pval = -1;
          if (pval > 1) pval = 1;
          val = NoteData.ConvertPan2Val(pval);
          break;
        }
      }
      break;
    }

    if (t != NoteType.Wave) {
      int.TryParse(CellLenInputs[type].text.Trim(), out int len);
      nd.Set(t, val, (byte)len);
    }
    else {
      nd.SetVal(t, val);
    }

    NoteLine nl = blines[row].note[col];
    nl.SetValues(nd, NoteTypeSprites, freqs, noteNames, waves);
    ShowNote(nd);
  }




  public void CellAlterNoteVal(bool up) {
    CellAlterVal(0, up);
  }
  public void CellAlterWaveVal(bool up) {
    CellAlterVal(1, up);
  }
  public void CellAlterVolVal(bool up) {
    CellAlterVal(2, up);
  }
  public void CellAlterPitchVal(bool up) {
    CellAlterVal(3, up);
  }
  public void CellAlterPanVal(bool up) {
    CellAlterVal(4, up);
  }

  void CellAlterVal(int type, bool up) {
    if (currentBlock == null || currentBlock.chs[col][row] == null) return;
    NoteData nd = currentBlock.chs[col][row];
    short num;
    NoteType t = (NoteType)(type + 1);
    num = nd.GetVal(t);
    switch (t) {
      case NoteType.Note:
        if (up) num++;
        else num--;
        if (num < 50) num = 50;
        if (num > 22000) num = 22000;
        nd.SetVal(NoteType.Note, num);
        break;

      case NoteType.Wave: // num is the ID, pick the exact one or the id closest and inferior
        if (up) num++;
        else num--;
        if (num < 1) num = 1;
        short closest = 0;
        foreach(Wave w in waves) {
          if (w.id == num) {
            closest = -1;
            break;
          }
          if (closest < w.id) closest = (short)w.id;
        }
        if (closest == -1) 
          nd.SetVal(NoteType.Wave, num);
        else
          nd.SetVal(NoteType.Wave, closest);
        break;

      case NoteType.Volume:
        if (up) num += 10;
        else num -= 10;
        if (num < 0) num = 0;
        if (num > 1024) num = 1024;
        nd.SetVal(NoteType.Volume, num);
        break;

      case NoteType.Pitch:
        if (up) num += 50;
        else num -= 50;
        if (num < -32767) num = -32767;
        if (num > 32767) num = 32767;
        nd.SetVal(NoteType.Pitch, num);
        break;

      case NoteType.Pan:
        if (up) num += 25;
        else num -= 25;
        if (num < 0) num = 0;
        if (num > 1000) num = 1000;
        nd.SetVal(NoteType.Pan, num);
        break;
    }
    blines[row].note[col].SetValues(nd, NoteTypeSprites, freqs, noteNames, waves);
    ShowNote(nd);
  }



  public void CellAlterNoteLen(bool up) {
    CellAlterLen(0, up);
  }
  public void CellAlterVolLen(bool up) {
    CellAlterLen(2, up);
  }
  public void CellAlterPitchLen(bool up) {
    CellAlterLen(3, up);
  }
  public void CellAlterPanLen(bool up) {
    CellAlterLen(4, up);
  }

  void CellAlterLen(int type, bool up) {
    if (currentBlock == null || currentBlock.chs[col][row] == null) return;
    NoteData nd = currentBlock.chs[col][row];
    byte num;
    NoteType t = (NoteType)(type + 1);
    num = nd.GetLen(t);
    if (up) num++;
    else num--;
    if (num < 1) num = 1;
    if (num > currentBlock.len - row - 2) num = (byte)(currentBlock.len - row - 2);
    nd.SetLen(t, num);
    blines[row].note[col].SetValues(nd, NoteTypeSprites, freqs, noteNames, waves);
    ShowNote(nd);
  }


  #endregion

  #region Block list **********************************************************************************************************************************************************

  void BlocksRegenerate() {
    foreach (Transform t in ContentsBlocks)
      Destroy(t.gameObject);
    foreach (BlockData b in blocks) {
      GameObject line = Instantiate(BlockListLineTemplate, ContentsBlocks);
      line.SetActive(true);
      BlockListLine bll = line.GetComponent<BlockListLine>();
      bll.BlockID.text = b.id.ToString();
      bll.BlockName.text = b.name;
      bll.BlockLen.text = b.len.ToString();
      bll.BlockBPM.text = b.bpm.ToString();
      bll.BlockID.fontSize = 28;
      bll.BlockName.fontSize = 28;
      bll.BlockLen.fontSize = 28;
      bll.BlockBPM.fontSize = 28;
      bll.Delete.onClick.AddListener(() => DeleteBlockFromList(b));
      bll.Edit.onClick.AddListener(() => EditBlockFromList(b));
      bll.LineButton.onClick.AddListener(() => SelectBLockFromList(b));
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
    for (int i = 0; i < bllines.Count; i++) {
      if (bllines[i].BlockID.text == id.ToString()) {
        BlockListLine bl = bllines[i];
        bllines.RemoveAt(i);
        Destroy(bl.gameObject);
        return;
      }
    }
    Blocks();
  }

  private void SelectBLockFromList(BlockData b) {
    for (int i = 0; i < blocks.Count; i++)
      if (blocks[i] == b) {
        SelectRow(i);
      }
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
    bll.BlockID.fontSize = 28;
    bll.BlockName.fontSize = 28;
    bll.BlockLen.fontSize = 28;
    bll.BlockBPM.fontSize = 28;
    bll.Delete.onClick.AddListener(() => DeleteBlockFromList(b));
    bll.Edit.onClick.AddListener(() => EditBlockFromList(b));
    bll.LineButton.onClick.AddListener(() => SelectBLockFromList(b));
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
    currentWave = w;
    ShowWave();
    CopyToWaveEditor();
    Dev.inst.WaveformEditor();
    editor.Done.gameObject.SetActive(true);
    gameObject.SetActive(false);
  }

  private void DeleteWaveFromList(Wave w) {
    for (int i = 0; i < waves.Count; i++) {
      if (waves[i].id == w.id) {
        WaveLine wl = wlines[i];
        wlines.RemoveAt(i);
        Destroy(wl.gameObject);
        waves.RemoveAt(i);
        return;
      }
    }
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
    wl.LineButton.onClick.AddListener(() => SelectWaveRow(w));
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
    Dev.inst.WaveformEditor();
    editor.Done.gameObject.SetActive(true);
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
  void SelectWaveRow(Wave w) {
    for (int i = 0; i < wlines.Count; i++) {
      if (wlines[i].id == w.id) {
        SelectRow(i);
        return;
      }
    }
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
      sbb.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "[" + w.id + "] " + w.name;
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
    bn.SetVal(NoteType.Wave, (short)w.id);
    blines[row].note[col].SetValues(bn, NoteTypeSprites, freqs, noteNames, waves);
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
    SelectRow(0);
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

  public TMP_InputField Values;
  public Button LoadSubButton;

  void NormalizeWaveIDs() {
    Dictionary<byte, byte> ids = new Dictionary<byte, byte>();
    byte pos = 1;
    foreach(Wave w in waves)
      ids[(byte)w.id] = pos++;

    int nv = music.NumVoices;
    foreach (BlockData b in blocks) {
      for (int i = 0; i < b.len; i++) {
        for (int c = 0; c < nv; c++) {
          NoteData note = b.chs[c][i];
          if (note.IsType(NoteType.Wave)) {
            byte id = (byte)note.GetVal(NoteType.Wave);
            note.SetVal(NoteType.Wave, ids[id]);
          }
        }
      }
    }
  }

  public void Save() {
    NormalizeWaveIDs();

    int numv = music.NumVoices;
    string res = NormLabel.Normalize(music.name) + ":\n" +
                numv.ToString("X2") + " " +
                blocks.Count.ToString("X2") + " " +
                waves.Count.ToString("X2") + " " +
                music.blocks.Count.ToString("X2") + "\n";
    for (int i = 0; i < music.blocks.Count; i++) {
      byte val = (byte)(music.blocks[i] < 1 ? 255 : music.blocks[i]);
      res += val.ToString("X2") + " ";
    }
    res += "\n";

    foreach (Wave w in waves) {
      byte ph = (byte)((((int)(w.phase * 100)) & 0xff00) >> 8);
      byte pl = (byte)(((int)(w.phase * 100)) & 0xff);
      res +=
            NormLabel.Normalize(w.name) + ":\n" +
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
          res += w.rawPCM[i].ToString("X2");
        }
        res += "\n";
      }
      res += "\n";
    }


    /*
    name label
    num voices [byte]
    num blocks [byte]
    num waves [byte]
    num blocks in music [byte]

    for each block in music
      id [byte]
    
    for each wave
      name label
      type [byte]
      phase [2bytes]
      a [byte]
      d [byte]
      s [byte]
      r [byte]
      pcmlen [4bytes, only if tpye is PCM]
      (pcmlen) bytes of data

    for each block
      name label
      id, len, bpm
      for each row
        for each column
          cell type [byte]
          note info [3bytes, only if has note]
          wave info [2bytes, only if has wave]
          vol info [3bytes, only if has vol]
          pitch info [3bytes, only if has pitch]
          pan info [3bytes, only if has pan]
     */


    foreach (BlockData b in blocks) {
      res += NormLabel.Normalize(b.name) + ":\n" +
            b.id.ToString("X2") + " " +
            b.len.ToString("X2") + " " +
            b.bpm.ToString("X2") + "\n";
      for (int r = 0; r < b.len; r++) {
        for (int c = 0; c < numv; c++) {
          NoteData note = b.chs[c][r];

          if (note.IsEmpty()) {
            res += "00  ";
            continue;
          }
          else res += note.NoteType.ToString("X2") + " ";
          if (note.IsType(NoteType.Note)) {
            short val = note.GetVal(NoteType.Note);
            byte ph = (byte)((val & 0xff00) >> 8);
            byte pl = (byte)(val & 0xff);
            res += ph.ToString("X2") + " " + pl.ToString("X2") + " " + note.GetLen(NoteType.Note).ToString("X2") + "  ";
          }
          if (note.IsType(NoteType.Wave)) {
            short val = note.GetVal(NoteType.Wave);
            byte ph = (byte)((val & 0xff00) >> 8);
            byte pl = (byte)(val & 0xff);
            res += ph.ToString("X2") + " " + pl.ToString("X2") + "  ";
          }
          if (note.IsType(NoteType.Volume)) {
            short val = note.GetVal(NoteType.Volume);
            byte ph = (byte)((val & 0xff00) >> 8);
            byte pl = (byte)(val & 0xff);
            res += ph.ToString("X2") + " " + pl.ToString("X2") + " " + note.GetLen(NoteType.Volume).ToString("X2") + "  ";
          }
          if (note.IsType(NoteType.Pitch)) {
            short val = note.GetVal(NoteType.Pitch);
            byte ph = (byte)((val & 0xff00) >> 8);
            byte pl = (byte)(val & 0xff);
            res += ph.ToString("X2") + " " + pl.ToString("X2") + " " + note.GetLen(NoteType.Pitch).ToString("X2") + "  ";
          }
          if (note.IsType(NoteType.Pan)) {
            short val = note.GetVal(NoteType.Pan);
            byte ph = (byte)((val & 0xff00) >> 8);
            byte pl = (byte)(val & 0xff);
            res += ph.ToString("X2") + " " + pl.ToString("X2") + " " + note.GetLen(NoteType.Pan).ToString("X2") + "  ";
          }
        }
        res += "\n";
      }
    }

    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = false;
    Values.text = res;
  }

  public void PreLoad() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }

  public void PostLoadText() {
    if (!gameObject.activeSelf) return;
    string dataz = Values.text.Trim();

    byte[] block;
    List<CodeLabel> labels;
    try {
      ByteReader.ReadBlock(dataz, out labels, out block);
    } catch (System.Exception e) {
      Values.text = "Parsing error: " + e.Message + "\n" + Values.text;
      return;
    }

    StartCoroutine(PostLoading(labels, block));
  }
  IEnumerator PostLoading(List<CodeLabel> labels, byte[] block) {
    yield return PBar.Show("Loading", 0, 100);

    int pos = 0;
    waves.Clear();
    blocks.Clear();
    MusicData m = new MusicData() {
      bpm = 120,
      voices = new byte[] { 0, 1, 2, 3, 255, 255, 255, 255 },
      blocks = new List<int>()
    };
    if (labels.Count == 0) { PBar.Hide(); throw new Exception("Missing Music label"); }
    m.name = labels[0].name;
    NameInput.SetTextWithoutNotify(m.name);

    byte numv = block[pos++];
    for (int i = 0; i < 8; i++)
      m.voices[i] = (byte)((i < numv) ? i : 255);
    byte numb = block[pos++];
    byte numw = block[pos++];
    byte numm = block[pos++];
    for (int i = 0; i < numm; i++) {
      byte b = block[pos++];
      m.blocks.Add(b == 255 ? -1 : b);
    }

    yield return PBar.Show("Loading", 3, numw + numb + 3);

    for (int i = 0; i < numw; i++) {
      if (labels.Count < i + 2) { PBar.Hide(); throw new Exception("Missing Wave label for wave #" + (i + 1)); }
      Wave w = new Wave {
        name = labels[i + 1].name.Trim()
      };

      w.id = (byte)(i + 1);
      w.wave = (Waveform)block[pos++];
      byte ph = block[pos++];
      byte pl = block[pos++];
      w.phase = ((ph << 8) + pl) / 100f;
      w.a = block[pos++];
      w.d = block[pos++];
      w.s = block[pos++];
      w.r = block[pos++];

      if (w.wave == Waveform.PCM) {
        byte len1 = block[pos++];
        byte len2 = block[pos++];
        byte len3 = block[pos++];
        byte len4 = block[pos++];

        int len = (len1 << 24) + (len2 << 16) + (len3 << 8) + (len4 << 0);
        w.rawPCM = new byte[len];
        for (int b = 0; b < len; b++)
          w.rawPCM[b] = block[pos++];
      }

      waves.Add(w);
      yield return PBar.Progress(4 + i);
    }


    for (int i = 0; i < numb; i++) {
      if (labels.Count < i + numw + 2) { PBar.Hide(); throw new Exception("Missing Block label for block #" + (i + 1)); }
      BlockData b = new BlockData {
        name = labels[i + numw + 1].name,
        chs = new List<NoteData>[] {
          new List<NoteData>(), new List<NoteData>(), new List<NoteData>(), new List<NoteData>(), 
          new List<NoteData>(), new List<NoteData>(), new List<NoteData>(), new List<NoteData>() 
        }
      };

      b.id = block[pos++];
      b.len = block[pos++];
      b.bpm = block[pos++];

      for (int r = 0; r < b.len; r++) {
        for (int c = 0; c < numv; c++) {
          byte data1 = block[pos++]; ;
          NoteData note = new NoteData();
          // data1 has the types, according to the required ones read the due amount of bytes
          if ((data1 & 1) == 1) { // Note
            byte v0 = block[pos++];
            byte v1 = block[pos++];
            byte v2 = block[pos++];
            note.Set(NoteType.Note, (short)(v1 + (short)(v0 << 8)), v2);
          }
          if ((data1 & 2) == 2) { // Wave
            byte v0 = block[pos++];
            byte v1 = block[pos++];
            note.Set(NoteType.Wave, (short)(v1 + (short)(v0 << 8)), 0);
          }
          if ((data1 & 4) == 4) { // Vol
            byte v0 = block[pos++];
            byte v1 = block[pos++];
            byte v2 = block[pos++];
            note.Set(NoteType.Volume, (short)(v1 + (short)(v0 << 8)), v2);
          }
          if ((data1 & 8) == 8) { // Pitch
            byte v0 = block[pos++];
            byte v1 = block[pos++];
            byte v2 = block[pos++];
            note.Set(NoteType.Pitch, (short)(v1 + (short)(v0 << 8)), v2);
          }
          if ((data1 & 16) == 16) { // Pan
            byte v0 = block[pos++];
            byte v1 = block[pos++];
            byte v2 = block[pos++];
            note.Set(NoteType.Pan, (short)(v1 + (short)(v0 << 8)), v2);
          }
          b.chs[c].Add(note);
        }
      }

      blocks.Add(b);
      yield return PBar.Progress(4 + numw + i);
    }

    music = m;

    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;

    MusicRegenerate();
    BlocksRegenerate();
    WavesRegenerate();
    PBar.Hide();
  }

  public void SaveBin() {
    // Show FileBrowser in select file mode
    FileBrowser.Save(SaveBinPost, FileBrowser.FileType.Rom);
  }
  public void SaveBinPost(string path, string name) {
    StartCoroutine(SavingBinPost(path, name));
  }
  public IEnumerator SavingBinPost(string path, string name) {
    yield return PBar.Show("Saving", 0, 5 + waves.Count + blocks.Count);
    ByteChunk chunk = new ByteChunk();

    // Normalize the IDs of waves
    NormalizeWaveIDs();
    yield return PBar.Progress(1);

    byte[] block = new byte[4 + music.blocks.Count];
    block[0] = (byte)music.NumVoices;
    block[1] = (byte)blocks.Count;
    block[2] = (byte)waves.Count;
    block[3] = (byte)music.blocks.Count;
    for (int i = 0; i < music.blocks.Count; i++)
      block[4+i] = (byte)(music.blocks[i] < 1 ? 255 : music.blocks[i]);
    chunk.AddBlock(music.name, LabelType.Music, block);
    yield return PBar.Progress(2);

    int step = 3;
    foreach (Wave w in waves) {
      int len = 7;
      if (w.wave == Waveform.PCM && w.rawPCM != null) len += 4 + w.rawPCM.Length;
      block = new byte[len];

      block[0] = (byte)w.wave;
      int pbyte = (int)(w.phase * 1000);
      block[1] = (byte)((pbyte & 0xff00) >> 8);
      block[2] = (byte)(pbyte & 0xff);

      block[3] = w.a;
      block[4] = w.d;
      block[5] = w.s;
      block[6] = w.r;
      yield return PBar.Progress(-1);

      if (w.wave == Waveform.PCM) {
        block[7] = (byte)((w.rawPCM.Length & 0xff000000) >> 24);
        block[8] = (byte)((w.rawPCM.Length & 0xff0000) >> 16);
        block[9] = (byte)((w.rawPCM.Length & 0xff00) >> 8);
        block[10] = (byte)((w.rawPCM.Length & 0xff) >> 0);
        for (int i = 0; i < w.rawPCM.Length; i++) {
          block[11 + i] = w.rawPCM[i];
        }
      }
      chunk.AddBlock(w.name, LabelType.Wave, block);
      yield return PBar.Progress(step++);
    }

    // Calculate the size of the whole set of blocks
    int numv = music.NumVoices;
    foreach (BlockData b in blocks) {
      int len = 3;
      for (int r = 0; r < b.len; r++) {
        for (int c = 0; c < numv; c++) {
          NoteData note = b.chs[c][r];
          len++;
          if (note.IsEmpty())continue;
          if (note.IsType(NoteType.Note)) len += 3;
          if (note.IsType(NoteType.Wave)) len += 2;
          if (note.IsType(NoteType.Volume)) len += 3;
          if (note.IsType(NoteType.Pitch)) len += 3;
          if (note.IsType(NoteType.Pan)) len += 3;
        }
      }
      block = new byte[len];

      block[0] = (byte)b.id;
      block[1] = (byte)b.len;
      block[2] = (byte)b.bpm;
      int pos = 3;
      for (int r = 0; r < b.len; r++) {
        for (int c = 0; c < numv; c++) {
          NoteData note = b.chs[c][r];
          if (note.IsEmpty()) {
            block[pos++] = 0;
            continue;
          }
          block[pos++] = (byte)note.NoteType;
          if (note.IsType(NoteType.Note)) {
            short val = note.GetVal(NoteType.Note);
            block[pos++] = (byte)((val & 0xff00) >> 8);
            block[pos++] = (byte)(val & 0xff);
            block[pos++] = note.GetLen(NoteType.Note);
          }
          if (note.IsType(NoteType.Wave)) {
            short val = note.GetVal(NoteType.Wave);
            block[pos++] = (byte)((val & 0xff00) >> 8);
            block[pos++] = (byte)(val & 0xff);
          }
          if (note.IsType(NoteType.Volume)) {
            short val = note.GetVal(NoteType.Volume);
            block[pos++] = (byte)((val & 0xff00) >> 8);
            block[pos++] = (byte)(val & 0xff);
            block[pos++] = note.GetLen(NoteType.Volume);
          }
          if (note.IsType(NoteType.Pitch)) {
            short val = note.GetVal(NoteType.Pitch);
            block[pos++] = (byte)((val & 0xff00) >> 8);
            block[pos++] = (byte)(val & 0xff);
            block[pos++] = note.GetLen(NoteType.Pitch);
          }
          if (note.IsType(NoteType.Pan)) {
            short val = note.GetVal(NoteType.Pan);
            block[pos++] = (byte)((val & 0xff00) >> 8);
            block[pos++] = (byte)(val & 0xff);
            block[pos++] = note.GetLen(NoteType.Pan);
          }
        }
      }
      chunk.AddBlock(b.name, LabelType.MusicBlock, block);
      yield return PBar.Progress(step++);
    }

    ByteReader.SaveBinBlock(path, name, chunk);
    PBar.Hide();
  }

  public void LoadBin() {
    FileBrowser.Load(PostLoadBin, FileBrowser.FileType.Rom);
  }

  public void PostLoadBin(string path) {
    ByteChunk res = new ByteChunk();
    ByteReader.ReadBinBlock(path, res);
    StartCoroutine(PostLoading(res.labels, res.block));
  }


  #endregion

  void ShowTooltip(string txt) {
    InfoTooltip.SetActive(true);
    infoTooltipDelay = 1;
    InfoTooltipTxt.text = txt;
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
  };

  readonly string[] noteNames = new string[] {
    "C0", "C#0",
    "D0", "Eb0",
    "E0",
    "F0", "F#0",
    "G0", "G#0",
    "A0", "Bb0",
    "B0",
    "C1", "C#1",
    "D1", "Eb1",
    "E1",
    "F1", "F#1",
    "G1", "G#1",
    "A1", "Bb1",
    "B1",
    "C2", "C#2",
    "D2", "Eb2",
    "E2",
    "F2", "F#2",
    "G2", "G#2",
    "A2", "Bb2",
    "B2",
    "C3", "C#3",
    "D3", "Eb3",
    "E3",
    "F3", "F#3",
    "G3", "G#3",
    "A3", "Bb3",
    "B3",
    "C4", "C#4",
    "D4", "Eb4",
    "E4",
    "F4", "F#4",
    "G4", "G#4",
    "A4", "Bb4",
    "B4",
    "C5", "C#5",
    "D5", "Eb5",
    "E5",
    "F5", "F#5",
    "G5", "G#5",
    "A5", "Bb5",
    "B5",
    "C6", "C#6",
    "D6", "Eb6",
    "E6",
    "F6", "F#6",
    "G6", "G#6",
    "A6", "Bb6",
    "B6",
    "C7", "C#7",
    "D7", "Eb7",
    "E7",
    "F7", "F#7",
    "G7", "G#7",
    "A7", "Bb7",
    "B7",
    "C8", "C#8",
    "D8", "Eb8",
    "E8",
    "F8", "F#8",
    "G8", "G#8",
    "A8", "Bb8",
    "B8",
  };

  readonly float[] freqs = new float[] {
    16.35f, 17.32f, 18.35f, 19.45f,
    20.6f, 21.83f, 23.12f, 24.5f,
    25.96f, 27.5f, 29.14f, 30.87f,
    32.7f, 34.65f, 36.71f, 38.89f,
    41.2f, 43.65f, 46.25f, 49f,
    51.91f, 55f, 58.27f, 61.74f,
    65.41f, 69.3f, 73.42f, 77.78f,
    82.41f, 87.31f, 92.5f, 98f,
    103.83f, 110f, 116.54f, 123.47f,
    130.81f, 138.59f, 146.83f, 155.56f,
    164.81f, 174.61f, 185f, 196f,
    207.65f, 220f, 233.08f, 246.94f,
    261.63f, 277.18f, 293.66f, 311.13f,
    329.63f, 349.23f, 369.99f, 392f,
    415.3f, 440f, 466.16f, 493.88f,
    523.25f, 554.37f, 587.33f, 622.25f,
    659.25f, 698.46f, 739.99f, 783.99f,
    830.61f, 880f, 932.33f, 987.77f,
    1046.5f, 1108.73f, 1174.66f, 1244.51f,
    1318.51f, 1396.91f, 1479.98f, 1567.98f,
    1661.22f, 1760f, 1864.66f, 1975.53f,
    2093f, 2217.46f, 2349.32f, 2489.02f,
    2637.02f, 2793.83f, 2959.96f, 3135.96f,
    3322.44f, 3520f, 3729.31f, 3951.07f,
    4186.01f, 4434.92f, 4698.63f, 4978.03f,
    5274.04f, 5587.65f, 5919.91f, 6271.93f,
    6644.88f, 7040f, 7458.62f, 7902.13f,
  };
}

public enum MusicEditorStatus {
  Idle, Music, BlockList, BlockEdit, Waveforms
}

