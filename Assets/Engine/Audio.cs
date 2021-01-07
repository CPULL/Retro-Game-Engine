using UnityEngine;

public class Audio : MonoBehaviour {
  public AudioSource[] srcs;
  public Channel[] channels;
  const int samplerate = 44100;
  private float[] oscValues = new float[512];
  private float[][] outputs;

  void Awake() {
    AudioClip.PCMReaderCallback[] readers = new AudioClip.PCMReaderCallback[8];
    readers[0] = OnAudioRead0;
    readers[1] = OnAudioRead1;
    readers[2] = OnAudioRead2;
    readers[3] = OnAudioRead3;
    readers[4] = OnAudioRead4;
    readers[5] = OnAudioRead5;
    readers[6] = OnAudioRead6;
    readers[7] = OnAudioRead7;
    AudioClip.PCMSetPositionCallback[] positions = new AudioClip.PCMSetPositionCallback[8];
    positions[0] = OnAudioSetPosition0;
    positions[1] = OnAudioSetPosition1;
    positions[2] = OnAudioSetPosition2;
    positions[3] = OnAudioSetPosition3;
    positions[4] = OnAudioSetPosition4;
    positions[5] = OnAudioSetPosition5;
    positions[6] = OnAudioSetPosition6;
    positions[7] = OnAudioSetPosition7;

    channels = new Channel[srcs.Length];
    for (int i = 0; i < channels.Length; i++) {
      int channel = i;
      channels[i] = new Channel(
        srcs[i],
        Waveform.Triangular,
        AudioClip.Create("Channel" + i, samplerate * 2, 1, samplerate, true, readers[i], positions[i])
      );
    }

    outputs = new float[channels.Length][];
    for (int i = 0; i < channels.Length; i++)
      outputs[i] = new float[512];
  }

  public float[] Oscillator {
    get {
      float max = 1;
      for (int c = 0; c < srcs.Length; c++)
        channels[c].audio.GetOutputData(outputs[c], 0);
      for (int i = 0; i < 512; i++) {
        float val = 0;
        for (int c = 0; c < srcs.Length; c++)
          val += outputs[c][i];
        oscValues[i] = val;
        if (max < val) max = val;
        if (max < -val) max = -val;
      }
      for (int i = 0; i < 512; i++)
        oscValues[i] /= max;
      return oscValues;
    }
  }



  public void Volume(int channel, float vol) {
    if (channel < -1 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (vol < 0) vol = 0;
    if (vol > 1) vol = 1;
    if (channel == -1) {
      for (int i = 0; i < channels.Length; i++) {
        channels[i].SetVol(vol);
      }
    }
    else {
      channels[channel].SetVol(vol);
    }
  }

  public void Pan(int channel, float pan) {
    if (channel < -1 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (pan < -1) pan = -1;
    if (pan > 1) pan = 1;
    if (channel == -1) {
      for (int i = 0; i < channels.Length; i++)
        channels[i].audio.panStereo = pan;
    }
    else
      channels[channel].audio.panStereo = pan;
  }

  public void Play(int channel, int freq, float length = -1) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (freq == 0) return;
    if (freq < 50) freq = 50;
    if (freq > 18000) freq = 18000;
    channels[channel].Play(freq, length);
  }

  public void Stop(int channel) {
    channels[channel].timeout = channels[channel].time;
  }

  public void Wave(int channel, Waveform wave, float phase) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (phase < .01f) phase = .01f;
    if (phase > 10) phase = 10;

    channels[channel].wave = wave;
    channels[channel].position = 0;
    if (wave == Waveform.Square) { // If square, make it between 0 and 1 excluded
      if (phase < 0.01f) phase = 0.01f;
      if (phase > 0.99f) phase = 0.99f;
    }
    channels[channel].phase = phase;
  }

  public void ADSR(int channel, byte attack, byte decay, byte sustain, byte release) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);

    if (attack == 0 && decay == 0 && sustain == 0 && release == 0) {
      channels[channel].adsrV = false;
      return;
    }
    channels[channel].adsrV = true;
    channels[channel].av = 0.0078392156f * attack + 0.001f; // 0.001s -> 2s
    channels[channel].dv = 0.0117607843f * decay + 0.001f; // 0.001s -> 3s
    channels[channel].sv = sustain / 255f; // % of volume = sv/255
    channels[channel].rv = 0.0117607843f * release + 0.001f; // 0.001s -> 3s
  }

  byte[] toplay = null;
  int playpos = -1;
  float musicsteplen = 0;
  public void PlayMusic(byte[] music) {
    toplay = music;
    playpos = 0;
  }

  private void Update() {
    for (int i = 0; i < channels.Length; i++) {
      if (!channels[i].audio.isPlaying) continue;

      if (channels[i].stopnow) {
        channels[i].audio.volume *= .75f;
        if (channels[i].audio.volume <= 0.01f) {
          channels[i].audio.Stop();
          channels[i].stopnow = false;
        }
      }
      else {
        channels[i].time += Time.deltaTime;
        float t = channels[i].time;
        if (channels[i].adsrV) {
          if (t < channels[i].av) channels[i].audio.volume = channels[i].vol * (1 - (channels[i].av - t) / channels[i].av);
          else if (t < channels[i].av + channels[i].dv) {
            float perc = (channels[i].dv - (t - channels[i].av)) / channels[i].dv;
            channels[i].audio.volume = channels[i].vol * (perc + (1 - perc) * channels[i].sv);
          }
          else channels[i].audio.volume = channels[i].vol * channels[i].sv;
        }
        if (channels[i].timeout != -1 && t >= channels[i].timeout) {
          if (channels[i].adsrV) {
            if (t >= channels[i].timeout + channels[i].rv) {
              channels[i].stopnow = true;
              channels[i].audio.volume = 0.01f;
            }
            else {
              float releaseVal = 1 - (t - channels[i].timeout) / channels[i].rv;
              channels[i].audio.volume = channels[i].vol * releaseVal * channels[i].sv;
            }
          }
          else {
            channels[i].stopnow = true;
          }
        }
      }
    }

    if (playpos == -1 || toplay == null) return;
    if (musicsteplen > 0) {
      musicsteplen -= Time.deltaTime;
      return;
    }
    // Get the next set of notes
    byte numchannels = toplay[playpos];
    if (playpos + 2 + numchannels * 5 >= toplay.Length) {
      playpos = -1;
      toplay = null;
      return;
    }
    musicsteplen = (toplay[playpos + 2] * 256 + toplay[playpos + 1] + 1) / 65535f;
    playpos += 3;
    for (int i = 0; i < numchannels; i++) {
      byte channel = toplay[playpos];
      int cfreq = toplay[playpos + 2] * 256 + toplay[playpos];
      float clen = (toplay[playpos + 4] * 256 + toplay[playpos + 3] + 1) / 65535f;
      Play(channel, cfreq, clen);
      playpos += 5;
    }
  }



  const float piP2 = Mathf.PI * 2f;
  const float piH2 = Mathf.PI * .5f;
  const float piD2 = 2f / Mathf.PI;


  void OnAudioRead(float[] data, int channel) {
    if (channels[channel].clip == null) return;
    switch (channels[channel].wave) {
      case Waveform.Triangular:
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          data[i] = piD2 * Mathf.Asin(Mathf.Cos(piP2 * pos)) * channels[channel].phase;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        break;

      case Waveform.Saw:
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          if (pos == 0)
            data[i] = 0;
          else
            data[i] = 1f - (piH2 - Mathf.Atan(channels[channel].phase * Mathf.Tan(piD2 * pos))) * piD2;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        break;

      case Waveform.SuperSaw:
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          float pt = channels[channel].phase;
          float pt2 = pt * pt;
          float y1 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(piD2 * pos))) * piD2;
          float y2 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(piD2 * pos * pt))) * piD2;
          float y3 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(piD2 * pos * pt2))) * piD2;
          float y4 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(piD2 * pos * pt2 * pt))) * piD2;
          float y5 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(piD2 * pos * pt2 * pt2))) * piD2;
          float y6 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(piD2 * pos * pt2 * pt2 * pt))) * piD2;
          data[i] = (y1 + y2 + y3 + y4 + y5 + y6) * .1666666f;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        break;

      case Waveform.Square:
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          pos -= (int)pos;
          if (pos < channels[channel].phase) data[i] = .99f; else data[i] = -.99f;
        }
        break;

      case Waveform.Sin:
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          data[i] = (Mathf.Sin(2 * Mathf.PI * pos) + Mathf.Sin(2 * Mathf.PI * pos * channels[channel].phase)) * .5f;
        }
        break;

      case Waveform.Bass1:
        seed++;
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;

          float x = channels[channel].position * .0005f;
          float xf = (channels[channel].position + 31.5f) * channels[channel].freq * .001f;
          float y = Mathf.Sin(piP2 * Mathf.Sqrt(.25f * xf)) * Mathf.Cos(Mathf.PI * xf * .0001245f) * (-.06f * x + 1000) / 1000;
          data[i] = y;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        for (int t = 0; t < 4; t++) {
          for (int i = 1; i < data.Length - 1; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1]) * .333f;
          for (int i = 2; i < data.Length - 2; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1] + data[i - 2] + data[i + 2]) * .2f;
          for (int i = 1; i < data.Length - 1; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1]) * .3333f;
        }
        break;

      case Waveform.Bass2:
        seed++;
        float p = channels[channel].phase;
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = .25f * channels[channel].freq * channels[channel].position / samplerate;

          float y =
            1.0f * Mathf.Sin(piP2 * pos) +
            0.1f * Mathf.Sin(piP2 * pos * 4 + p) +
            0.05f * Mathf.Sin(piP2 * pos * 8 + p * p) +
            0.02f * Mathf.Sin(piP2 * pos * 16 + p * p * p) +
            0.01f * Mathf.Sin(piP2 * pos * 32 + p * p * p * p);
          y *= 0.88f;
          y += .001f * (Squirrel3Norm((int)pos, seed) + Squirrel3Norm((int)pos, (uint)pos));
          if (y < -1f) y = -1f;
          if (y > 1f) y = 1f;
          data[i] = y;
        }
        for (int i = 1; i < data.Length; i++) { 
          data[i] = (data[i] + data[i - 1]) * .5f;
        }
        break;

      case Waveform.Noise:
        seed++;
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          data[i] = Squirrel3Norm((int)pos, seed);
        }
        break;

      case Waveform.PinkNoise:
        seed++;
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          data[i] = Squirrel3Norm((int)pos, seed);
          if (i > 0 && Mathf.Abs(data[i - 1] - data[i]) > .5f) data[i] *= -.5f;
        }
        break;

      case Waveform.BrownNoise:
        seed++;
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          data[i] = Squirrel3Norm((int)pos, seed);
          if (i > 1 && Mathf.Abs(data[i - 2] - data[i]) > .5f) data[i] *= -.25f;
          if (i > 0 && Mathf.Abs(data[i - 1] - data[i]) > .5f) data[i] *= -.25f;
        }
        break;

      case Waveform.BlackNoise:
        seed++;
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          data[i] = Squirrel3Norm((int)pos, seed);
          if (i > 3 && Mathf.Abs(data[i - 4] - data[i]) > .5f) data[i] *= -.25f;
          if (i > 2 && Mathf.Abs(data[i - 3] - data[i]) > .5f) data[i] *= -.25f;
          if (i > 1 && Mathf.Abs(data[i - 2] - data[i]) > .5f) data[i] *= -.25f;
          if (i > 0 && Mathf.Abs(data[i - 1] - data[i]) > .5f) data[i] *= -.25f;
        }
        break;

      case Waveform.SoftNoise:
        seed++;
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          if ((i % 3) == 0) {
            float x = 10 * channels[channel].phase / (pos + channels[channel].phase);
            x = pos * .25f;
            data[i] = Mathf.Sin(2 * Mathf.PI * x);
          }
          else
            data[i] = Squirrel3Norm((int)pos, seed) * .75f;
          if (i > 0 && Mathf.Abs(data[i] - data[i - 1]) > .01f) data[i] = .05f * data[i] + .95f * data[i - 1];
        }

        for (int t = 0; t < 2; t++) {
          for (int i = 1; i < data.Length - 1; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1]) * .333f;
          for (int i = 2; i < data.Length - 2; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1] + data[i - 2] + data[i + 2]) * .2f;
          for (int i = 1; i < data.Length - 1; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1]) * .3333f;
        }
        for (int i = 0; i < 16; i++) {
          data[i] *= .5f * (i + 16) / 16f;
          data[data.Length - i - 1] *= .5f * (i + 16) / 16f;
        }
        break;

      case Waveform.Drums:
        seed++;
        float maxn = 10 * channels[channel].phase / (channels[channel].freq * 4096);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;

          float x = channels[channel].freq * channels[channel].position / 1760;
          float y = Mathf.Sin(piP2 * Mathf.Sqrt(.5f * (x + 31.5f))) * Mathf.Cos(Mathf.PI * (x + 31.5f) * .0001245f) * (-.25f * x + 1000) / 1000;
          if (channels[channel].position < 5000)
            y += Squirrel3Norm((int)x, seed) * x * maxn * (-.25f * x + 1000) / 1000;
          if (x < 64) y *= x / 256;
          if (x < 72) y *= x / 128;
          if (x < 80) y *= x / 64;
          data[i] = y;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        for (int t = 0; t < 2; t++) {
          for (int i = 1; i < data.Length - 1; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1]) * .333f;
          for (int i = 2; i < data.Length - 2; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1] + data[i - 2] + data[i + 2]) * .2f;
          for (int i = 1; i < data.Length - 1; i++)
            data[i] = (data[i] + data[i - 1] + data[i + 1]) * .3333f;
        }
        break;
    }
  }

  void OnAudioSetPosition(int newPosition, int channel) {
    channels[channel].position = newPosition;
  }

  const uint NOISE1 = 0xb5297a4d;
  const uint NOISE2 = 0x68e31da4;
  const uint NOISE3 = 0x1b56c4e9;
  const uint CAP = 1 << 30;
  const float CAP2 = 1 << 29;
  uint seed = 0;

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

  #region commodity delegate functions
  void OnAudioRead0(float[] data) { OnAudioRead(data, 0); }
  void OnAudioRead1(float[] data) { OnAudioRead(data, 1); }
  void OnAudioRead2(float[] data) { OnAudioRead(data, 2); }
  void OnAudioRead3(float[] data) { OnAudioRead(data, 3); }
  void OnAudioRead4(float[] data) { OnAudioRead(data, 4); }
  void OnAudioRead5(float[] data) { OnAudioRead(data, 5); }
  void OnAudioRead6(float[] data) { OnAudioRead(data, 6); }
  void OnAudioRead7(float[] data) { OnAudioRead(data, 7); }

  void OnAudioSetPosition0(int pos) { OnAudioSetPosition(pos, 0); }
  void OnAudioSetPosition1(int pos) { OnAudioSetPosition(pos, 1); }
  void OnAudioSetPosition2(int pos) { OnAudioSetPosition(pos, 2); }
  void OnAudioSetPosition3(int pos) { OnAudioSetPosition(pos, 3); }
  void OnAudioSetPosition4(int pos) { OnAudioSetPosition(pos, 4); }
  void OnAudioSetPosition5(int pos) { OnAudioSetPosition(pos, 5); }
  void OnAudioSetPosition6(int pos) { OnAudioSetPosition(pos, 6); }
  void OnAudioSetPosition7(int pos) { OnAudioSetPosition(pos, 7); }

  #endregion
}

public enum Waveform { Triangular=0, Saw=1, Square=2, Sin=3, Bass1=4, Bass2=5, Noise=6, PinkNoise=7, BrownNoise=8, BlackNoise=9, SoftNoise=10, Drums=11, SuperSaw=12 };

[System.Serializable]
public struct Channel {
  public AudioSource audio;
  public AudioClip clip;
  public float timeout;
  public Waveform wave;
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
  public bool stopnow;

  public Channel(AudioSource src, Waveform w, AudioClip ac) {
    audio = src;
    clip = ac;
    audio.clip = clip;
    timeout = -1f;
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
    stopnow = false;
  }

  internal void SetVol(float vol) {
    audio.volume = vol;
    this.vol = vol;
  }

  internal void Play(int frequency, float length) {
    freq = frequency;
    time = 0;
    timeout = length;
    stopnow = false;
    audio.volume = vol;
    audio.Play();
  }
}



/*
 * Define a way to play music: 2 bytes total len + 1 byte num channles + [1 byte channel, 2 bytes freq, 2 bytes len] * num channels
 * 
 * Wave: bass
 * Wave: battery
 * Wave: 8bit PCM
 * 
 */
