using UnityEngine;
using UnityEngine.UI;

public class WaveformEditor : MonoBehaviour {
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

  int attack;
  int decay;
  int sustain;
  int release;
  Waveform wave = Waveform.Triangular;
  float phase = 0;

  private void Start() {
    CleanADSR();
  }

  public void OnSliderChange(Slider slider) {
    int val = (int)slider.value;
    if (slider == Attack) {
      attack = val;
      float t = 0.015625f * attack + 0.001f;
      string time;
      if (t>.099f) {
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
      float t = 0.02344f * decay + 0.001f;
      string time;
      if (t > .099f) {
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
      float t = 0.02344f * release + 0.001f;
      string time;
      if (t > .099f) {
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

    float a = (0.015625f * attack + 0.001f);
    float d = (0.02344f * decay + 0.001f);
    float r = (0.02344f * release + 0.001f);
    float total = a + d + r + 8;

    lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
    lineRenderer.SetPosition(1, new Vector3(512 * a / total, 255, 0));
    lineRenderer.SetPosition(2, new Vector3(512 * (a + d) / total, sustain, 0));
    lineRenderer.SetPosition(3, new Vector3(512 * (total - r) / total, sustain, 0));
    lineRenderer.SetPosition(4, new Vector3(511, 0, 0));
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
  }
}
