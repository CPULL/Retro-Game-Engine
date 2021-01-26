using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WaveformEditor : MonoBehaviour {
  public Audio sounds;
  public Slider Attack;
  public TextMeshProUGUI AttackTxt;
  public Slider Decay;
  public TextMeshProUGUI DecayTxt;
  public Slider Sustain;
  public TextMeshProUGUI SustainTxt;
  public Slider Release;
  public TextMeshProUGUI ReleaseTxt;
  public LineRenderer lineRenderer;
  public Toggle ShowOscillometer;
  public LineRenderer oscilloscope;
  public Slider Phase;
  public TextMeshProUGUI PhaseTxt;
  public GameObject WaveValues;
  public Sprite[] WaveSprites;
  public string[] WaveNames;
  public Image WaveSprite;
  public TextMeshProUGUI WaveName;
  public GameObject LoadPCMButton;

  int attack = 0;
  int decay = 0;
  int sustain = 0;
  int release = 0;
  Waveform wave = Waveform.Triangular;
  float phase = 0;
  byte[] rawPCM;

  public PianoKeyboard[] AllKeys;
  readonly Vector3[] oscilloscopeValues = new Vector3[512];

  private void Start() {
    CleanADSR();
    WaveChange(0);
    UpdateWaveforms();

    sounds.Init();

    for (int i = 0; i < 512; i++)
      oscilloscopeValues[i] = new Vector3(i * 2, 128, 0);
    oscilloscope.positionCount = 512;
    oscilloscope.SetPositions(oscilloscopeValues);
  }

  private void Update() {
    if (Values.gameObject.activeSelf) return;

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


  public void LoadPCM() {
    FileBrowser.Show(PostLoadPCM);
  }

  public void PostLoadPCM(string path) {
    StartCoroutine(LoadPCMCoroutine(path));
  }

  IEnumerator LoadPCMCoroutine(string path) {
    string url = string.Format("file://{0}", path);
    string ext = path.Substring(path.LastIndexOf('.') + 1).ToLowerInvariant();
    AudioType at = AudioType.UNKNOWN;
    if (ext == "wav") at = AudioType.WAV;
    if (ext == "ogg") at = AudioType.OGGVORBIS;
    if (ext == "mp3") at = AudioType.MPEG;

    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, at)) {
      yield return www.SendWebRequest();
      AudioClip pcm = DownloadHandlerAudioClip.GetContent(www);
      int freq = pcm.frequency;
      float diff = freq / 22050f;
      int len = (int)(pcm.samples / diff);

      float[] res = new float[pcm.samples * pcm.channels];
      rawPCM = new byte[len];
      pcm.GetData(res, 0);

      for (int i = 0; i < len; i++) {
        // we need to mix all the values from the pos in the new array to the pos+1
        int srcpos1 = (int)(i * diff);
        int srcpos2 = (int)((i + 1) * diff);

        float val = 0;
        for (int p = 0; p < srcpos2 - srcpos1; p++)
          for (int c = 0; c < pcm.channels; c++)
            val += res[srcpos1 + p * pcm.channels + c];
        val /= pcm.channels * (srcpos2 - srcpos1);
        if (val < -1) val = -1;
        if (val > 1) val = 1;
        rawPCM[i] = (byte)(255 * (val + 1) * .5f);
      }
      wave = Waveform.PCM;
      UpdateWaveforms();
    }
  }

  void RenderOscilloscope() {
    float[] data = sounds.Oscillator;

    for (int i = 0; i < data.Length; i++)
      oscilloscopeValues[i].y = 128 + 127 * data[i];
    oscilloscope.SetPositions(oscilloscopeValues);
  }

  public void OnSliderChange() {
    attack = (int)Attack.value;
    decay = (int)Decay.value;
    sustain = (int)Sustain.value;
    release = (int)Release.value;

    if (attack == 0 && decay == 0 && sustain == 0 && release == 0) {
      AttackTxt.text = "Attack\n0ms";
      DecayTxt.text = "Decay\n0ms";
      SustainTxt.text = "Sustain\n100%";
      ReleaseTxt.text = "Release\n0ms";
      UpdateADSRGraph();
      return;
    }
    if (sustain == 0) sustain = 255;

    float t = 0.0078392156f * attack + 0.001f;
    string time;
    if (t > .5f) {
      time = ((int)(t * 100)) / 100f + "s";
    }
    else {
      time = (int)(t * 1000) + "ms";
    }
    AttackTxt.text = "Attack\n" + time;

    t = 0.0117607843f * decay + 0.001f;
    if (t > .5f) {
      time = ((int)(t * 100)) / 100f + "s";
    }
    else {
      time = (int)(t * 1000) + "ms";
    }
    DecayTxt.text = "Decay\n" + time;

    string perc = (((int)(sustain * 1000 / 255f)) / 10f) + "%";
    SustainTxt.text = "Sustain\n" + perc;

    t = 0.0117607843f * release + 0.001f;
    if (t > .5f) {
      time = ((int)(t * 100)) / 100f + "s";
    }
    else {
      time = (int)(t * 1000) + "ms";
    }
    ReleaseTxt.text = "Release\n" + time;
    UpdateADSRGraph();
  }
  public void CleanADSR() {
    Attack.SetValueWithoutNotify(0);
    attack = 0;
    AttackTxt.text = "Attack\n0";
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
    UpdateWaveforms();
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

  public void WaveOpenDD() {
    WaveValues.SetActive(!WaveValues.activeSelf);
  }

  public void WaveChange(int w) {
    WaveValues.SetActive(false);
    wave = (Waveform)w;
    WaveSprite.sprite = WaveSprites[w];
    WaveName.text = WaveNames[w];
    PhaseChange();
    LoadPCMButton.SetActive(w == 14); // PCM
    Phase.gameObject.SetActive(w != 14);
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

  public void ResetPhase() {
    Phase.SetValueWithoutNotify(0);
    PhaseChange();
  }

  void UpdateWaveforms() {
    for (int i = 0; i < 8; i++) {
      if (wave == Waveform.PCM && rawPCM != null)
        sounds.Wave(i, rawPCM);
      else
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
      case "c ": freq = obase; break;
      case "c#": freq = obase * step; break;
      case "d ": freq = obase * step * step; break;
      case "eb": freq = obase * step * step * step; break;
      case "e ": freq = obase * step4; break;
      case "f ": freq = obase * step4 * step; break;
      case "f#": freq = obase * step4 * step * step; break;
      case "g ": freq = obase * step4 * step * step * step; break;
      case "g#": freq = obase * step4 * step4; break;
      case "a ": freq = obase * step4 * step4 * step; break;
      case "bb": freq = obase * step4 * step4 * step * step; break;
      case "b ": freq = obase * step4 * step4 * step * step * step; break;
    }
    return (int)freq;
  }


  public TMP_InputField Values;
  public Button LoadSubButton;

  public void PreLoad() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }
  readonly Regex rgComments = new Regex("([^\\n]*)(//[^\\n]*)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgLabels = new Regex("[\\s]*[a-z0-9]+:[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgHex = new Regex("[\\s]*0x([a-f0-9]+)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  public void PostLoad() {
    if (!gameObject.activeSelf) return;
    string data = Values.text.Trim();
    data = rgComments.Replace(data, " ");
    data = rgLabels.Replace(data, " ");
    data = data.Replace('\n', ' ').Trim();
    while (data.IndexOf("  ") != -1) data = data.Replace("  ", " ");

    data = ReadNextByte(data, out byte waveb);
    wave = (Waveform)waveb;
    data = ReadNextByte(data, out byte phaseb1);
    data = ReadNextByte(data, out byte phaseb2);
    phase = (phaseb1 * 256 + phaseb2) / 1000f;

    data = ReadNextByte(data, out byte attackb);
    attack = attackb;
    data = ReadNextByte(data, out byte decayb);
    decay = decayb;
    data = ReadNextByte(data, out byte sustainb);
    sustain = sustainb;
    ReadNextByte(data, out byte releaseb);
    release = releaseb;

    Attack.SetValueWithoutNotify(attack);
    Decay.SetValueWithoutNotify(decay);
    Sustain.SetValueWithoutNotify(sustain);
    Release.SetValueWithoutNotify(release);

    float val;
    if (wave == Waveform.Square) {
      if (phase < 0.01f) phase = .01f;
      if (phase > 0.99f) phase = .99f;
      val = 20f * phase - 10;
    }
    else {
      if (phase < 0.01f) phase = .01f;
      if (phase > 10f) phase = 10f;
      val = 10 * phase - 10;
      if (val > 0) val = 1.1f * phase - 1;
      if (val == 0) phase = 1;
    }
    Phase.SetValueWithoutNotify(val);

    OnSliderChange();
    WaveChange((int)wave);

    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
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
    Match m = rgHex.Match(part);
    if (m.Success) {
      res = (byte)Convert.ToInt32(m.Groups[1].Value, 16);
      return data.Substring(pos).Trim();
    }

    res = 0;
    return data;
  }


  public void Save() {
    string res = "Wave:\n0x" +
      ((int)wave).ToString("X2") + " 0x";

    int pbyte = (int)(phase * 1000);
    res += ((byte)((pbyte & 0xff00) >> 8)).ToString("X2") + " 0x" +
    ((byte)(pbyte & 0xff)).ToString("X2") + " 0x";

    res += attack.ToString("X2") + " 0x" + decay.ToString("X2") + " 0x" + sustain.ToString("X2") + " 0x" + release.ToString("X2"); 

    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = false;
    Values.text = res;
  }

  public Wave Export() {
    Wave w = new Wave {
      id = 0,
      name = "Wave from editor",
      wave = wave,
      phase = phase,
      a = (byte)attack,
      d = (byte)decay,
      s = (byte)sustain,
      r = (byte)release,
      rawPCM = rawPCM
    };
    return w;
  }

  public void Import(Wave w) {
    wave = w.wave;
    phase = w.phase;
    attack = w.a;
    decay = w.d;
    sustain = w.s;
    release = w.r;
    rawPCM = w.rawPCM;

    // Update the interface
    Attack.SetValueWithoutNotify(attack);
    Decay.SetValueWithoutNotify(decay);
    Sustain.SetValueWithoutNotify(sustain);
    Release.SetValueWithoutNotify(release);

    float val;
    if (wave == Waveform.PCM) {
      val = 0;
    }
    else if (wave == Waveform.Square) {
      if (phase < 0.01f) phase = .01f;
      if (phase > 0.99f) phase = .99f;
      val = 20f * phase - 10;
    }
    else {
      if (phase < 0.01f) phase = .01f;
      if (phase > 10f) phase = 10f;
      val = 10 * phase - 10;
      if (val > 0) val = 1.1f * phase - 1;
      if (val == 0) phase = 1;
    }
    Phase.SetValueWithoutNotify(val);

    OnSliderChange();
    WaveChange((int)wave);
  }



  public Button Done;

  public void CompleteWaveEditing() {
    musiceditor.CopyFromWaveEditor();
    Done.gameObject.SetActive(false);
    Dev.inst.MusicEditor();
    gameObject.SetActive(false);
  }

  public MusicEditor musiceditor;


}
