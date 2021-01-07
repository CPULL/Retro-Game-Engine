﻿using UnityEngine;
using UnityEngine.UI;

public class WaveformEditor : MonoBehaviour {
  public Audio sounds;
  public Slider Attack;
  public Text AttackTxt;
  public Slider Decay;
  public Text DecayTxt;
  public Slider Sustain;
  public Text SustainTxt;
  public Slider Release;
  public Text ReleaseTxt;
  public LineRenderer lineRenderer;
  public Toggle ShowOscillometer;
  public LineRenderer oscilloscope;
  public Slider Phase;
  public Text PhaseTxt;
  public Dropdown Wave;
  public Image WaveSprite;
  public Sprite[] WaveSprites;

  int attack = 0;
  int decay = 0;
  int sustain = 0;
  int release = 0;
  Waveform wave = Waveform.Triangular;
  float phase = 0;

  public PianoKeyboard[] AllKeys;
  Vector3[] oscilloscopeValues = new Vector3[512];

  private void Start() {
    CleanADSR();
    WaveChange();
    UpdateWaveforms();

    sounds.Volume(-1, 0);
    for (int i = 0; i < 8; i++)
      sounds.Play(i, 440, 0.01f);
    sounds.Volume(-1, 1);

    for (int i = 0; i < 512; i++)
      oscilloscopeValues[i] = new Vector3(i * 2, 128, 0);
    oscilloscope.positionCount = 512;
    oscilloscope.SetPositions(oscilloscopeValues);
  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.Tab)) StartNote("C4", true);
    if (Input.GetKeyDown(KeyCode.Alpha1)) StartNote("C4#", true);
    if (Input.GetKeyDown(KeyCode.Q)) StartNote("D4", true);
    if (Input.GetKeyDown(KeyCode.Alpha2)) StartNote("E4b", true);
    if (Input.GetKeyDown(KeyCode.W)) StartNote("E4", true);
    if (Input.GetKeyDown(KeyCode.E)) StartNote("F4", true);
    if (Input.GetKeyDown(KeyCode.Alpha3)) StartNote("F4#", true);
    if (Input.GetKeyDown(KeyCode.R)) StartNote("G4", true);
    if (Input.GetKeyDown(KeyCode.Alpha5)) StartNote("G4#", true);
    if (Input.GetKeyDown(KeyCode.T)) StartNote("A4", true);
    if (Input.GetKeyDown(KeyCode.Alpha6)) StartNote("B4b", true);
    if (Input.GetKeyDown(KeyCode.Y)) StartNote("B4", true);

    if (Input.GetKeyDown(KeyCode.U)) StartNote("C5", true);
    if (Input.GetKeyDown(KeyCode.Alpha8)) StartNote("C5#", true);
    if (Input.GetKeyDown(KeyCode.I)) StartNote("D5", true);
    if (Input.GetKeyDown(KeyCode.Alpha9)) StartNote("E5b", true);
    if (Input.GetKeyDown(KeyCode.O)) StartNote("E5", true);
    if (Input.GetKeyDown(KeyCode.P)) StartNote("F5", true);
    if (Input.GetKeyDown(KeyCode.Minus)) StartNote("F5#", true);
    if (Input.GetKeyDown(KeyCode.LeftBracket)) StartNote("G5", true);
    if (Input.GetKeyDown(KeyCode.Equals)) StartNote("G5#", true);
    if (Input.GetKeyDown(KeyCode.RightBracket)) StartNote("A5", true);
    if (Input.GetKeyDown(KeyCode.Backslash)) StartNote("B5b", true);
    if (Input.GetKeyDown(KeyCode.Return)) StartNote("B5", true);

    if (Input.GetKeyUp(KeyCode.Tab)) StopNote("C4", true);
    if (Input.GetKeyUp(KeyCode.Alpha1)) StopNote("C4#", true);
    if (Input.GetKeyUp(KeyCode.Q)) StopNote("D4", true);
    if (Input.GetKeyUp(KeyCode.Alpha2)) StopNote("E4b", true);
    if (Input.GetKeyUp(KeyCode.W)) StopNote("E4", true);
    if (Input.GetKeyUp(KeyCode.E)) StopNote("F4", true);
    if (Input.GetKeyUp(KeyCode.Alpha3)) StopNote("F4#", true);
    if (Input.GetKeyUp(KeyCode.R)) StopNote("G4", true);
    if (Input.GetKeyUp(KeyCode.Alpha5)) StopNote("G4#", true);
    if (Input.GetKeyUp(KeyCode.T)) StopNote("A4", true);
    if (Input.GetKeyUp(KeyCode.Alpha6)) StopNote("B4b", true);
    if (Input.GetKeyUp(KeyCode.Y)) StopNote("B4", true);

    if (Input.GetKeyUp(KeyCode.U)) StopNote("C5", true);
    if (Input.GetKeyUp(KeyCode.Alpha8)) StopNote("C5#", true);
    if (Input.GetKeyUp(KeyCode.I)) StopNote("D5", true);
    if (Input.GetKeyUp(KeyCode.Alpha9)) StopNote("E5b", true);
    if (Input.GetKeyUp(KeyCode.O)) StopNote("E5", true);
    if (Input.GetKeyUp(KeyCode.P)) StopNote("F5", true);
    if (Input.GetKeyUp(KeyCode.Minus)) StopNote("F5#", true);
    if (Input.GetKeyUp(KeyCode.LeftBracket)) StopNote("G5", true);
    if (Input.GetKeyUp(KeyCode.Equals)) StopNote("G5#", true);
    if (Input.GetKeyUp(KeyCode.RightBracket)) StopNote("A5", true);
    if (Input.GetKeyUp(KeyCode.Backslash)) StopNote("B5b", true);
    if (Input.GetKeyUp(KeyCode.Return)) StopNote("B5", true);

    if (ShowOscillometer.isOn) RenderOscilloscope();
  }



  void RenderOscilloscope() {
    float[] data = sounds.Oscillator;

    for (int i = 0; i < data.Length; i++)
      oscilloscopeValues[i].y = 128 + 127 * data[i];
    oscilloscope.SetPositions(oscilloscopeValues);
  }

  public void OnSliderChange(Slider slider) {
    int val = (int)slider.value;
    if (slider == Attack) {
      attack = val;
      float t = 0.0078392156f * attack + 0.001f;
      string time;
      if (t>.5f) {
        time = ((int)(t * 100))/100f + "s";
      }
      else {
        time = (int)(t * 1000) + "ms";
      }
      AttackTxt.text = "Attack\n" + time;
      UpdateADSRGraph();
    }
    if (slider == Decay) {
      decay = val;
      float t = 0.0117607843f * decay + 0.001f;
      string time;
      if (t > .5f) {
        time = ((int)(t * 100)) / 100f + "s";
      }
      else {
        time = (int)(t * 1000) + "ms";
      }
      DecayTxt.text = "Decay\n" + time;
      UpdateADSRGraph();
    }
    if (slider == Sustain) {
      sustain = val;
      string perc = (((int)(sustain * 1000 / 255f))/10f) + "%";
      SustainTxt.text = "Sustain\n" + perc;
      UpdateADSRGraph();
    }
    if (slider == Release) {
      release = val;
      float t = 0.0117607843f * release + 0.001f;
      string time;
      if (t > .5f) {
        time = ((int)(t * 100)) / 100f + "s";
      }
      else {
        time = (int)(t * 1000) + "ms";
      }
      ReleaseTxt.text = "Release\n" + time;
      UpdateADSRGraph();
    }
  }
  public void CleanADSR() {
    Attack.SetValueWithoutNotify(0);
    attack = 0;
    AttackTxt.text = "Attak\n0";
    Decay.SetValueWithoutNotify(0);
    decay = 0;
    DecayTxt.text = "Decay\n0";
    Sustain.SetValueWithoutNotify(0);
    sustain = 0;
    SustainTxt.text = "Sustain\n100%";
    Release.SetValueWithoutNotify(0);
    release = 0;
    ReleaseTxt.text = "Release\n0";
    UpdateADSRGraph();
  }

  void UpdateADSRGraph() {
    if (attack == 0 && decay == 0 && sustain == 0 && release == 0) {
      lineRenderer.SetPosition(0, new Vector3(0,  128, 0));
      lineRenderer.SetPosition(1, new Vector3(128, 128, 0));
      lineRenderer.SetPosition(2, new Vector3(256, 128, 0));
      lineRenderer.SetPosition(3, new Vector3(384, 128, 0));
      lineRenderer.SetPosition(4, new Vector3(512, 128, 0));
      return;
    }

    float a = (0.0078392156f * attack + 0.001f);
    float d = (0.0117607843f * decay + 0.001f);
    float r = (0.0117607843f * release + 0.001f);
    float total = a + d + r + 8;

    lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
    lineRenderer.SetPosition(1, new Vector3(512 * a / total, 255, 0));
    lineRenderer.SetPosition(2, new Vector3(512 * (a + d) / total, sustain, 0));
    lineRenderer.SetPosition(3, new Vector3(512 * (total - r) / total, sustain, 0));
    lineRenderer.SetPosition(4, new Vector3(511, 0, 0));

    UpdateWaveforms();
  }

  public void WaveChange() {
    wave = (Waveform)Wave.value;
    WaveSprite.sprite = WaveSprites[Wave.value];
    PhaseChange();
  }

  public void PhaseChange() {
    float val = Phase.value;
    if (wave == Waveform.Square) {
      phase = (val + 10) / 20f;
      if (phase < 0.01f) phase = .01f;
      if (phase > 0.99f) phase = .99f;
    }
    else {
      if (val < 0) phase = (10 + val) / 10;
      else if (val > 0) phase = (1 + val) / 1.1f;
      else phase = 1;
      if (phase < 0.01f) phase = .01f;
      if (phase > 10f) phase = 10f;
    }
    PhaseTxt.text = "Phase: " + ((int)(100 * phase)) / 100f;
    UpdateWaveforms();
  }

  void UpdateWaveforms() {
    for (int i = 0; i < 8; i++) {
      sounds.Wave(i, wave, phase);
      sounds.ADSR(i, (byte)attack, (byte)decay, (byte)sustain, (byte)release);
    }
  }

  int nextNotePos = 0;
  readonly int[] playedNotes = new int[8];

  public void StartNote(string note, bool show = false) {
    int freq = CalculateNoteFrequency(note);
    playedNotes[nextNotePos] = freq;
    sounds.Play(nextNotePos, freq, -1);
    nextNotePos++;
    if (nextNotePos >= 8) nextNotePos = 0;

    if (show) {
      foreach (PianoKeyboard key in AllKeys)
        if (key.note == note) {
          key.image.color = key.pressedColor;
          return;
        }
    }
  }

  public void StopNote(string note, bool show = false) {
    int freq = CalculateNoteFrequency(note);
    for (int i = 0; i < 8; i++)
      if (playedNotes[i] == freq) {
        sounds.Stop(i);
        playedNotes[i] = -1;
        break;
      }

    if (show) {
      foreach (PianoKeyboard key in AllKeys)
        if (key.note == note) {
          key.image.color = key.normalColor;
          return;
        }
    }
  }

  const float step = 1.059463f;
  const float step4 = 1.059463f * 1.059463f * 1.059463f * 1.059463f;
  int CalculateNoteFrequency(string note) {
    string n = note.ToLowerInvariant()[0] + "" + (note + " ").ToLowerInvariant()[2];
    int.TryParse(note.Substring(1, 1), out int oct);
    float obase = (13081 << (oct - 3)) / 100f;
    float freq = 440;
    switch(n) {
      case "c ": freq = obase * step; break;
      case "c#": freq = obase * step * step; break;
      case "d ": freq = obase * step * step * step; break;
      case "eb": freq = obase * step4; break;
      case "e ": freq = obase * step4 * step; break;
      case "f ": freq = obase * step4 * step * step; break;
      case "f#": freq = obase * step4 * step * step; break;
      case "g ": freq = obase * step4 * step * step * step; break;
      case "g#": freq = obase * step4 * step4; break;
      case "a ": freq = obase * step4 * step4 * step; break;
      case "bb": freq = obase * step4 * step4 * step * step; break;
      case "b ": freq = obase * step4 * step4 * step * step * step; break;
    }
    return (int)freq;
  }
}