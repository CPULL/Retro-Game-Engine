using UnityEngine;
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

  private void Start() {
    CleanADSR();
    WaveChange();
    UpdateWaveforms();

    sounds.Volume(-1, 0);
    for (int i = 0; i < 8; i++)
      sounds.Play(i, 440, 0.01f);
    sounds.Volume(-1, 1);
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

  public void StartNote(string note) {
    int freq = CalculateNoteFrequency(note);
    playedNotes[nextNotePos] = freq;
    sounds.Play(nextNotePos, freq, -1);
    nextNotePos++;
    if (nextNotePos >= 8) nextNotePos = 0;
  }

  public void StopNote(string note) {
    int freq = CalculateNoteFrequency(note);
    for (int i = 0; i < 8; i++)
      if (playedNotes[i] == freq) {
        sounds.Stop(i);
        playedNotes[i] = -1;
        break;
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
