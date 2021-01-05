using UnityEngine;

public class Audio : MonoBehaviour {
  public AudioSource[] channels;
  AudioClip[] clips;
  public Waveform[] waveforms;
  float[] timeouts;
  const int samplerate = 44100;


  void Awake() {
    clips = new AudioClip[channels.Length];
    waveforms = new Waveform[channels.Length];
    timeouts = new float[channels.Length];
    for (int i = 0; i < channels.Length; i++) {
      int channel = i;
//      clips[i] = AudioClip.Create("Channel0", samplerate * 2, 1, samplerate, true, (pos) => { int c = channel; OnAudioRead(pos, c); }, (pos) => { int c = channel; OnAudioSetPosition(pos, c); });
      clips[i] = AudioClip.Create("Channel0", samplerate * 2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
      channels[i].clip = clips[i];
      timeouts[i] = -1f;
      waveforms[i] = new Waveform(Waveform.Wave.Triangular);
    }
  }

  /*
   Define ADSR volume
   Define ADSR frequncy
   Define ADSR phase
   Define global volume
   Define pan

   Define a way to play music: 2 bytes total len + 1 byte num channles + [1 byte channel, 2 bytes freq, 2 bytes len] * num channels
   */

  public void Volume(int channel, float vol) {
    if (channel < -1 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (vol < 0) vol = 0;
    if (vol > 1) vol = 1;
    if (channel == -1) {
      for (int i = 0; i < channels.Length; i++) {
        channels[i].volume = vol;
        waveforms[i].vol = vol;
      }
    }
    else {
      channels[channel].volume = vol;
      waveforms[channel].vol = vol;
    }
  }

  public void Pan(int channel, float pan) {
    if (channel < -1 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (pan < -1) pan = -1;
    if (pan > 1) pan = 1;
    if (channel == -1) {
      for (int i = 0; i < channels.Length; i++)
        channels[i].panStereo = pan;
    }
    else
      channels[channel].panStereo = pan;
  }

  public void Play(int channel, int freq, float length = -1) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (freq < 50) freq = 50;
    if (freq > 18000) freq = 18000;
    waveforms[channel].freq = freq;
    waveforms[channel].time = 0;
    channels[channel].Play();
    timeouts[channel] = length;
  }

  public void Wave(int channel, Waveform.Wave wave, float phase) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (phase < .01f) phase = .01f;
    if (phase > 10) phase = 10;

    waveforms[channel].wave = wave;
    waveforms[channel].position = 0;
    if (wave == Waveform.Wave.Square) { // If square, make it between 0 and 1 excluded
      if (phase < 0.01f) phase = 0.01f;
      if (phase > 0.99f) phase = 0.99f;
    }
    waveforms[channel].phase = phase;
  }

  public void ADSR(int channel, byte attack, byte decay, byte sustain, byte release) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);

    if (attack ==0 && decay == 0 && sustain == 0 && release == 0) {
      waveforms[channel].adsrV = false;
      return;
    }
    waveforms[channel].adsrV = true;
    waveforms[channel].av = 0.03137f * attack + 0.001f; // time = 0.03137f * av + 0.001f
    waveforms[channel].dv = 0.06274f * decay + 0.002f; // time = 0.06274f * dv + 0.002f
    waveforms[channel].sv = sustain / 255f; // % of volume = sv/255
    waveforms[channel].rv = 0.06274f * release + 0.002f; // time = 0.06274f * rv + 0.002f;
  }

  byte[] toplay = null;
  int playpos = -1;
  public void Play(byte[] music) {
    toplay = music;
    playpos = 0;
  }

  float gather = 0;

  private void Update() {
    for (int i = 0; i < channels.Length; i++) {
      if (!channels[i].isPlaying) continue;
      waveforms[i].time += Time.deltaTime;
      float t = waveforms[i].time;
      if (waveforms[i].adsrV) {
        if (t < waveforms[i].av) channels[i].volume = waveforms[i].vol * (1 - (waveforms[i].av - t) / waveforms[i].av);
        else if (t < waveforms[i].av + waveforms[i].dv) {
          float perc = (waveforms[i].dv - (t - waveforms[i].av)) / waveforms[i].dv;
          // perc = 1 -> 1
          // perc = 0 -> sv
          channels[i].volume = waveforms[i].vol * (perc + (1 - perc) * waveforms[i].sv);
        }
        else channels[i].volume = waveforms[i].vol * waveforms[i].sv;

        gather += Time.deltaTime;
        if (gather >= .05) {
          int vv = (int)(channels[i].volume * 20);
          Debug.Log(vv);
          gather = 0;
        }
      }
      if (t >= timeouts[i]) {
        if (waveforms[i].adsrV) {
          if (t >= timeouts[i] + waveforms[i].rv)
            channels[i].Stop();
          else
            channels[i].volume = waveforms[i].vol * (timeouts[i] + waveforms[i].rv - t) * waveforms[i].sv;
        }
        else {
          channels[i].Stop();
        }
      }
    }

    if (playpos == -1 || toplay == null) return;
    // Get the next set of notes
    byte numchannels = toplay[playpos];
    float len = (toplay[playpos + 1] * 256 + toplay[playpos] + 1) / 65535f;
  }

  const float piP2 = Mathf.PI * 2f;
  const float piH2 = Mathf.PI * .5f;
  const float piD2 = 2f / Mathf.PI;


  void OnAudioRead(float[] data) {
    int channel = 0;
    switch (waveforms[channel].wave) {
      case Waveform.Wave.Triangular:
        for (int i = 0; i < data.Length; i++) {
          waveforms[channel].position++;
          if (waveforms[channel].position >= samplerate) waveforms[channel].position = 0;
          float pos = waveforms[channel].freq * waveforms[channel].position / samplerate;
          data[i] = piD2 * Mathf.Asin(Mathf.Cos(piP2 * pos)) * waveforms[channel].phase;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        break;

      case Waveform.Wave.Saw:
        for (int i = 0; i < data.Length; i++) {
          waveforms[channel].position++;
          if (waveforms[channel].position >= samplerate) waveforms[channel].position = 0;
          float pos = waveforms[channel].freq * waveforms[channel].position / samplerate;
          if (pos == 0)
            data[i] = 0;
          else
            data[i] = 1f - (piH2 - Mathf.Atan(waveforms[channel].phase * Mathf.Tan(piD2 * pos))) * piD2;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        break;

      case Waveform.Wave.Square:
        for (int i = 0; i < data.Length; i++) {
          waveforms[channel].position++;
          if (waveforms[channel].position >= samplerate) waveforms[channel].position = 0;
          float pos = waveforms[channel].freq * waveforms[channel].position / samplerate;
          pos -= (int)pos;
          if (pos < waveforms[channel].phase)
            data[i] = .9f;
          else
            data[i] = -.9f;
        }
        break;

      case Waveform.Wave.Sin:
        for (int i = 0; i < data.Length; i++) {
          waveforms[channel].position++;
          if (waveforms[channel].position >= samplerate) waveforms[channel].position = 0;
          float pos = waveforms[channel].freq * waveforms[channel].position / samplerate;
          data[i] = (Mathf.Sin(2 * Mathf.PI * pos) + Mathf.Sin(2 * Mathf.PI * pos * waveforms[channel].phase)) * .5f;
        }
        break;

      case Waveform.Wave.Noise:
        for (int i = 0; i < data.Length; i++) {
          waveforms[channel].position++;
          if (waveforms[channel].position >= samplerate) waveforms[channel].position = 0;
          float pos = waveforms[channel].freq * waveforms[channel].position / samplerate;
          data[i] = Squirrel3Norm((int)pos, (uint)(waveforms[channel].phase * 1000));
        }
        break;
    }
  }

  void OnAudioSetPosition(int newPosition) {
    int channel = 0;
    waveforms[channel].position = newPosition;
  }

  const uint NOISE1 = 0xb5297a4d;
  const uint NOISE2 = 0x68e31da4;
  const uint NOISE3 = 0x1b56c4e9;
  const uint CAP = 1 << 30;
  const float CAP2 = 1 << 29;

  uint Squirrel3(int pos, uint seed = 0) {
    uint n = (uint)pos;
    n *= NOISE1;
    n += seed;
    n ^= n >> 8;
    n += NOISE2;
    n ^= n << 8;
    n *= NOISE3;
    n ^= n >> 8;
    return n % CAP;
  }

  float Squirrel3Norm(int pos, uint seed = 0) {
    uint n = (uint)pos;
    n *= NOISE1;
    n += seed;
    n ^= n >> 8;
    n += NOISE2;
    n ^= n << 8;
    n *= NOISE3;
    n ^= n >> 8;
    float res = (n % CAP) / CAP2 - 1f;
    return res;
  }

}

[System.Serializable]
public struct Waveform {
  public enum Wave { Triangular, Saw, Square, Sin, Noise };
  public Wave wave;
  public float phase;
  public float freq;
  public int position;
  public float time;
  public float vol;
  public bool adsrV;
  public float av;
  public float dv;
  public float sv;
  public float rv;
  public float af;
  public float df;
  public float sf;
  public float rf;
  public float ap;
  public float dp;
  public float sp;
  public float rp;

  public Waveform(Wave w) {
    wave = w;
    phase = 1f;
    freq = 440;
    position = 0;
    time = 0;
    vol = 1;
    adsrV = false;
    av = 0;
    dv = 0;
    sv = 0;
    rv = 0;
    af = 0;
    df = 0;
    sf = 0;
    rf = 0;
    ap = 0;
    dp = 0;
    sp = 0;
    rp = 0;
  }
}

// Merge all the structs in a single one, channel, wave, timeout
