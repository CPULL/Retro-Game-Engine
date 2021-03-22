using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour {
  public AudioSource[] srcs;
  public Channel[] channels;
  const int samplerate = 44100;
  const float oneOversamplerate = 1f / 44100;
  private readonly float[] oscValues = new float[512];
  private float[][] outputs;

  public AudioClip[] Tones;
  float[][] noiseSamples = new float[10][];
  int[] nslens = new int[10];
  public float[] nsvals = new float[10];

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
        AudioClip.Create("Channel" + i, samplerate * 2, 1, samplerate, true, readers[i], positions[i]));
    }

    outputs = new float[channels.Length][];
    for (int i = 0; i < channels.Length; i++)
      outputs[i] = new float[512];

    for (int i = 0; i < 10; i++) {
      nslens[i] = Tones[i].samples * Tones[i].channels;
      noiseSamples[i] = new float[nslens[i]];
      Tones[i].GetData(noiseSamples[i], 0);
    }
  }

  IEnumerator DelayedInit() {
    yield return new WaitForSeconds(.1f);
    for (int i = 0; i < channels.Length; i++)
      channels[i].SetVol(0);
    yield return null;
    for (int i = 0; i < channels.Length; i++)
      channels[i].Play(440, .001f);
    yield return new WaitForSeconds(.05f);
    for (int i = 0; i < channels.Length; i++)
      channels[i].SetVol(1.0f);
  }

  internal void Init() {
    StartCoroutine(DelayedInit());
  }

  internal void StopAll() {
    playing = false;
    StopAllCoroutines();
    StartCoroutine(StopAllDelayed());
    for (int i = 0; i < channels.Length; i++) {
      channels[i].SetVol(0);
      channels[i].Play(440, .001f);
      channels[i].SetVol(1.0f);
      channels[i].audio.Stop();
      Stop(i);
      channels[i].stopnow = true;
      OnAudioSetPosition(i, 0);
    }
  }

  IEnumerator StopAllDelayed() {
    for (int i = 0; i < channels.Length; i++) {
      channels[i].SetVol(0);
      channels[i].Play(440, .001f);
      channels[i].audio.Stop();
      Stop(i);
      channels[i].stopnow = true;
    }
    yield return null;
    for (int i = 0; i < channels.Length; i++) {
      channels[i].SetVol(1.0f);
      OnAudioSetPosition(i, 0);
    }
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

  public float Volume(int channel) {
    if (channel < -1 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    return channels[channel].GetVol();
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
  public float Pan(int channel) {
    if (channel < -1 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    return channels[channel].audio.panStereo;
  }

  internal void Pitch(int channel, float pitch) {
    if (channel < -1 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);

    // 1.05946^numsemitones
    float p = Mathf.Pow(1.05946f, pitch);
    channels[channel].audio.pitch = p;
  }

  internal float Pitch(int channel) {
    if (channel < -1 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);

    // p = 1.05946^numsemitones
    float p = channels[channel].audio.pitch;
    return 17.3132f * Mathf.Log(p);
  }



  public void Play(int channel, float freq, float length = -1) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (freq == 0) return;
    if (freq < 50) freq = 50;
    if (freq > 18000) freq = 18000;
    OnAudioSetPosition(channel, 0);
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

  public void Wave(int channel, byte[] data, int start) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);

    channels[channel].wave = (Waveform)data[start];
    channels[channel].phase = (((short)data[start + 1] << 8) + data[start + 2]) / 100f;
    ADSR(channel, data[start + 3], data[start + 4], data[start + 5], data[start + 6]);
    if (channels[channel].wave == Waveform.PCM) {
      int len = (data[start + 7] << 24) + (data[start + 8] << 16) + (data[start + 9] << 8) + (data[start + 10] << 0);
      channels[channel].pcmdata = new byte[len];
      for (int b = 0; b < len; b++) {
        channels[channel].pcmdata[b] = data[start + 11 + b];
      }
    }
    channels[channel].position = 0;
  }

  public void Wave(int channel, byte[] data) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    channels[channel].wave = Waveform.PCM;
    channels[channel].position = 0;
    channels[channel].phase = 0;
    channels[channel].pcmdata = data;
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
    channels[channel].position = 0;
  }

  private void Update() {
    if (playing) PlayMusic();

    for (int i = 0; i < channels.Length; i++) {
      if (!channels[i].audio.isPlaying) continue;

      if (channels[i].stopnow) {
        channels[i].audio.volume *= .75f;
        channels[i].audio.Stop();
        if (channels[i].audio.volume <= 0.01f) {
          channels[i].stopnow = false;
        }
        OnAudioSetPosition(i, 0);
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

  }

  public float MiP;
  public float MaP;

  const float piP2 = Mathf.PI * 2f;
  const float piH2 = Mathf.PI * .5f;
  const float piD1 = 1f / Mathf.PI;
  const float piD2 = 2f / Mathf.PI;
  const float piD325 = 3.25f / Mathf.PI;
  const float o3rd = 1f / 3f;
  const float o5th = 1f / 5f;
  const float o7th = 1f / 7f;
  const float o9th = 1f / 9f;
  const float o11th = 1f / 11f;
  const float o255th = 1f / 255f;
  uint seed = 0;

  void OnAudioRead(float[] data, int channel) {
    if (channels[channel].clip == null) return;
    switch (channels[channel].wave) {
      // FIXME OK
      case Waveform.Triangular: {
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position * oneOversamplerate;
          data[i] = piD2 * Mathf.Asin(Mathf.Cos(piP2 * pos)) * channels[channel].phase;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
      }
      break;

      // FIXME OK
      case Waveform.Saw: {
        int step = (int)(88200 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= step) channels[channel].position -= step;
          float pos = channels[channel].freq * channels[channel].position * oneOversamplerate - 1; // 0->2
          if (pos == 0)
            data[i] = 0;
          else
            data[i] = 1f - (piH2 - Mathf.Atan(channels[channel].phase * Mathf.Tan(piD2 * pos))) * piD2;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
      }
      break;

      // FIXME OK
      case Waveform.SuperSaw: {
        int step = (int)(220500 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= step) channels[channel].position -= step;
          float pos = channels[channel].freq * channels[channel].position * oneOversamplerate - 1; // 0->5
          pos = (pos - 0.02371f) * 0.986957f; // Enforce 0-5 values
          float pt = channels[channel].phase;
          float v = pt * .1f;
          float v2 = v * v;
          float y1 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(piD2 * pos))) * piD2;
          float y2 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(2 * piD2 * (pos + piD1 * .1f)))) * piD2;
          float y3 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(3 * piD2 * (pos + piD1 * .083333333333f)))) * piD2;
          float y4 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(4 * piD2 * (pos + piD1 * .066666666666f)))) * piD2;
          float y5 = 1f - (piH2 - Mathf.Atan(Mathf.Tan(4 * piD2 * (pos + piD1 * .0625f)))) * piD2;

          data[i] = ((1 - v) * y1 + v * y2 + v2 * y3 + v2 * v2 * y4 + v2 * v2 * v2 * y5) / (1.1f + pt * .2f);
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
      }
      break;

      // FIXME OK
      case Waveform.Square: {
        float pow = channels[channel].phase + .5f;
        int step = (int)(44100 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;
          float pos = channels[channel].freq * channels[channel].position * oneOversamplerate; // 0->1
          pos = Mathf.Pow(pos, pow);
          data[i] = .75f * Mathf.Atan(256 * Mathf.Sin(piP2 * pos));
        }
      }
      break;

      // FIXME OK
      case Waveform.Sin: {
        float pow = channels[channel].phase * .1f + .9f;
        int step = (int)(44100 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;
          float pos = channels[channel].freq * channels[channel].position * oneOversamplerate; // 0->1
          if (pos > 1) pos -= 1;
          pos = Mathf.Pow(pos, pow);
          data[i] = Mathf.Sin(piP2 * pos);
        }
      }
      break;

      // FIXME OK
      case Waveform.SuperSin: {
        float ph = channels[channel].phase;
        int step = (int)(44100 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;
          float pos = channels[channel].freq * channels[channel].position * oneOversamplerate; // 0->1
          if (pos > 1) pos -= 1;
          float a = Mathf.Sin(piP2 * pos);
          float b = (Mathf.Sin(piP2 * a * ph) + 2 * a) * .25f;
          float c = (Mathf.Sin(piH2 * ph * b) + Mathf.Sin(piP2 * a + piP2 * ph) + 3 * a) * .2f;
          data[i] = (a + b + c) * .3f;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
      }
      break;

      // FIXME OK
      case Waveform.Bass1: {
        float ph = channels[channel].phase;
        float fu = 8 / channels[channel].freq;
        float fo = channels[channel].freq / 8;
        int step = (int)(44100 * fu);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;
          float pos = fo * channels[channel].position * oneOversamplerate; // 0->1
          if (pos > 1) pos -= 1;
          float a = Mathf.Sin(piP2 * pos);
          float b = Mathf.Sin(23 * ph * piD2 * pos);
          data[i] = (.95f * a + .05f * b) * a * 1.8f - .9f;
        }
      }
      break;

      case Waveform.Bass2: {
        float fu = 4 / channels[channel].freq;
        float fo = channels[channel].freq / 4;
        float detune = 1.0293f * channels[channel].phase;
        int step = (int)(44100 * fu);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;
          float pos = fo * channels[channel].position * oneOversamplerate; // 0->1
          if (pos > 1) pos -= 1;
          float v =
            Mathf.Sin(piP2 * pos) + .3f * Mathf.Sin(piP2 * pos * 2) + .15f * Mathf.Sin(piP2 * pos * 3) +
            Mathf.Sin(piP2 * pos * detune) + .25f * Mathf.Sin(piP2 * pos * 2 * detune) + .125f * Mathf.Sin(piP2 * pos * 3 * detune);
          data[i] = v * .4f;
        }

      }
      break;

      case Waveform.Noise: {
        seed++;
        int step = (int)(4410000 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;
          float pos = channels[channel].freq * channels[channel].position * oneOversamplerate;
          data[i] = Squirrel3.Norm((int)pos, seed);
        }
      }
      break;

      // FIXME OK
      case Waveform.PinkNoise: { // All at the same level, centered on the phase (1-> all equals, 0-> out higher, 10 -> center higher
        float ph = channels[channel].phase;
        if (ph != 1) {
          MiP = 0;
          float pf = ph;
          if (ph > 1) pf = (10 - ph) * .1f;
          pf = (1 + pf) * (1 + pf);
          for (int i = 0; i < 10; i++) {
            float pow = (4.5f - i) / (.1f + pf * pf);
            nsvals[i] = pow * pow;
            if (ph < 1) nsvals[i] = (nsvals[i] * nsvals[i] + nsvals[i]) * .5f + .1f;
            else nsvals[i] = .25f * (6 - ph * .1f) - nsvals[i];
            if (nsvals[i] < 0) nsvals[i] = 0;
            MiP += nsvals[i];
          }
          for (int i = 0; i < 10; i++)
            nsvals[i] /= MiP;
        }
        else {
          for (int i = 0; i < 10; i++)
            nsvals[i] = .1f;
        }

        int step = (int)(44100 * 160 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;

          float pos = 0.00625f * channels[channel].freq * channels[channel].position * oneOversamplerate; // 0->1
          if (pos > 1) pos -= 1;
          data[i] = 0;
          for (int n = 0; n < 10; n++)
            data[i] += nsvals[n] * noiseSamples[n][(int)(pos * nslens[n])];
        }
      }
      break;

      // FIXME OK
      case Waveform.BrownNoise: { // Linear down -> 1: 50%->0%; 0: 50%->25%; 10: 100%->0%
        /*
         s = ph<1 ? 50 : 50+ph*5
         e = ph<1 ? 25-ph*25 : 0
         */

        float ph = channels[channel].phase;
        float s = ph <= 1 ? 50 : 50 + (ph - 1) * 5.55f;
        float e = ph <= 1 ? 25 - ph * 25 : 0;
        MiP = 0;
        for (int i = 0; i < 10; i++) {
          nsvals[i] = (i * .111f) * s + (1 - i * .111f) * e;
          if (nsvals[i] < 0) nsvals[i] = 0;
          MiP += nsvals[i];
        }
        for (int i = 0; i < 10; i++)
          nsvals[i] /= MiP;

        int step = (int)(44100 * 160 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;

          float pos = 0.00625f * channels[channel].freq * channels[channel].position * oneOversamplerate; // 0->1
          if (pos > 1) pos -= 1;
          data[i] = 0;
          for (int n = 0; n < 10; n++)
            data[i] += nsvals[n] * noiseSamples[n][(int)(pos * nslens[n])];
        }
      }
      break;

      // FIXME OK
      case Waveform.BlackNoise: { // From hi to low decreasing exp. 1: ends at 50%, 10: ends at 100%, 0: only first two
        /*
         s = ph<1 ? 25-ph*25 : 0
         e = ph<1 ? 50 : 50+ph*5
         */

        float ph = channels[channel].phase;
        float s = ph <= 1 ? 50 : 50 + (ph - 1) * 5.55f;
        float e = ph <= 1 ? 25 - ph * 25 : 0;
        MiP = 0;
        for (int i = 0; i < 10; i++) {
          nsvals[i] = (i * .111f) * e + (1 - i * .111f) * s;
          if (nsvals[i] < 0) nsvals[i] = 0;
          MiP += nsvals[i];
        }
        for (int i = 0; i < 10; i++)
          nsvals[i] /= MiP;

        int step = (int)(44100 * 160 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;

          float pos = 0.00625f * channels[channel].freq * channels[channel].position * oneOversamplerate; // 0->1
          if (pos > 1) pos -= 1;
          data[i] = 0;
          for (int n = 0; n < 10; n++)
            data[i] += nsvals[n] * noiseSamples[n][(int)(pos * nslens[n])];
        }
      }
      break;

      // FIXME OK
      case Waveform.SoftNoise: { // From low to hi decreasing exponentially. 1: ends at 50%, 10: ends at 0%, 0: only last two
        float ph = channels[channel].phase;
        MiP = 0;
        for (int i = 0; i < 10; i++) {
          nsvals[i] = 1 / 9f + ph - 4 * Mathf.Log(i + 1);
          if (nsvals[i] < 0) nsvals[i] = 0;
          MiP += nsvals[i];
        }
        for (int i = 0; i < 10; i++)
          nsvals[i] /= MiP;

        int step = (int)(44100 * 160 / channels[channel].freq);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position > step) channels[channel].position -= step;

          float pos = 0.00625f * channels[channel].freq * channels[channel].position * oneOversamplerate; // 0->1
          if (pos > 1) pos -= 1;
          data[i] = 0;
          for (int n = 0; n < 10; n++)
            data[i] += nsvals[n] * noiseSamples[n][(int)(pos * nslens[n])];
        }
      }
      break;

      case Waveform.Drums:
        seed++;
        float maxn = 10 * channels[channel].phase / (channels[channel].freq * 4096);
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;

          float x = channels[channel].freq * channels[channel].position / 1760 + Squirrel3.Norm(channels[channel].position, (uint)channels[channel].freq);
          float y = Mathf.Sin(piP2 * Mathf.Sqrt(.5f * (x + 31.5f))) * Mathf.Cos(Mathf.PI * (x + 31.5f) * .0001245f) * (-.25f * x + 1000) / 1000;
          if (channels[channel].position < 2500 * channels[channel].phase)
            y += Squirrel3.Norm((int)x, seed) * x * maxn * (-.25f * x + 1000) / 1000;
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

      case Waveform.PCM:
        if (channels[channel].pcmdata == null) return;
        int pcmsize = channels[channel].pcmdata.Length;
        if (pcmsize == 0) return;

        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          int pos = (int)(channels[channel].freq * channels[channel].position / 440);
          pos %= channels[channel].pcmdata.Length;

          data[i] = 2 * channels[channel].pcmdata[pos] * o255th - 1f;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        break;

    }
  }

  void OnAudioSetPosition(int newPosition, int channel) {
    channels[channel].position = newPosition;
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

  #region Music play *********************************************************************************************************

  Music music = null;
  bool playing = false;
  float timeForNextBeat = 0;
  int currentPlayedMusicLine = 0;
  int currentPlayedMusicBlock = 0;

  public void LoadMusic(byte[] data, int start) {
    if (music != null)
      music.Clear();
    else
      music = new Music();

    int pos = start;
    music.numvoices = data[pos++];
    int numblocks = data[pos++];
    int numwaves = data[pos++];
    music.numblocks = data[pos++];
    music.mblocks = new byte[music.numblocks];
    for (int i = 0; i < music.numblocks; i++)
      music.mblocks[i] = data[pos++];

    for (int i = 0; i < numwaves; i++) {
      Wave w = new Wave {
        id = data[pos++],
        wave = (Waveform)data[pos++],
        phase = data[pos++] << 8
      };
      w.phase += data[pos++];
      w.phase /= 100f;
      w.a = data[pos++];
      w.d = data[pos++];
      w.s = data[pos++];
      w.r = data[pos++];

      if (w.wave == Waveform.PCM) {
        int len = data[pos++];
        len = (len << 8) + data[pos++];
        len = (len << 8) + data[pos++];
        len = (len << 8) + data[pos++];
        w.rawPCM = new byte[len];
        for (int b = 0; b < len; b++)
          w.rawPCM[b] = data[pos++];
      }
      music.waves.Add(w.id, w);
    }

    for (int i = 0; i < numblocks; i++) {
      Block b = new Block {
        id = data[pos++],
        len = data[pos++],
        bpm = data[pos++]
      };
      b.notes = new Note[b.len, music.numvoices];
      for (int r = 0; r < b.len; r++)
        for (int c = 0; c < music.numvoices; c++) {
          byte type = data[pos++];
          Note n = new Note();
          if ((type & 1) == 1) {
            n.freq = (short)((short)(data[pos++] << 8) + data[pos++]);
            n.nlen = data[pos++];
          }
          else n.freq = 0;

          if ((type & 2) == 2) {
            n.wave = (byte)((short)(data[pos++] << 8) + data[pos++]);
          }
          else n.wave = 0;

          if ((type & 4) == 4) {
            n.vol = NoteData.GetVolVal((short)((short)(data[pos++] << 8) + data[pos++]));
            n.vlen = data[pos++];
          }
          else n.vol = -1;

          if ((type & 8) == 8) {
            n.pitch = NoteData.GetPitchVal((short)((short)(data[pos++] << 8) + data[pos++]));
            n.plen = data[pos++];
          }
          else n.plen = 255;

          if ((type & 8) == 8) {
            n.pan = NoteData.GetPanVal((short)((short)(data[pos++] << 8) + data[pos++]));
            n.panlen = data[pos++];
          }
          else n.panlen = 255;

          b.notes[r, c] = n;
        }
      music.blocks.Add(b.id, b);
    }
    playing = false;
    timeForNextBeat = 0;
    currentPlayedMusicLine = 0;
    currentPlayedMusicBlock = 0;
  }

  public void MusicVoices(byte a, byte b = 255, byte c = 255, byte d = 255, byte e = 255, byte f = 255, byte g = 255, byte h = 255) {
    if (music == null) music = new Music();
    music.MusicVoices[0] = a;
    music.MusicVoices[1] = b;
    music.MusicVoices[2] = c;
    music.MusicVoices[3] = d;
    music.MusicVoices[4] = e;
    music.MusicVoices[5] = f;
    music.MusicVoices[6] = g;
    music.MusicVoices[7] = h;
  }

  public int GetMusicPos() {
    return currentPlayedMusicBlock * 255 + currentPlayedMusicLine;
  }

  public void PlayMusic(int pos = -1) {
    timeForNextBeat = 0;
    if (pos == -1) {
      currentPlayedMusicLine = 0;
      currentPlayedMusicBlock = 0;
    }
    else {
      currentPlayedMusicLine = pos & 255;
      currentPlayedMusicBlock = pos / 256;
    }
    playing = true;
  }

  public void StopMusic() {
    playing = false;
    if (music != null) {
      for (int i = 0; i < music.numvoices; i++) {
        Stop(music.MusicVoices[i]);
      }
    }
  }

  readonly Swipe[] swipes = new Swipe[] { new Swipe(), new Swipe(), new Swipe(), new Swipe(), new Swipe(), new Swipe(), new Swipe(), new Swipe() };
  void HandleSwipes() {
    for (int c = 0; c < 8; c++) {
      Swipe s = swipes[c];
      if (s.vollen != 0) {
        float step = s.voltime / s.vollen;
        channels[c].SetVol(s.vole * step + s.vols * (1 - step));
        s.voltime += Time.deltaTime;
        if (s.voltime >= s.vollen) {
          channels[c].SetVol(s.vole);
          s.vollen = 0;
        }
      }

      if (s.pitchlen != 0) {
        float step = s.pitchtime / s.pitchlen;
        Pitch(c, s.pitche * step + s.pitchs * (1 - step));
        s.pitchtime += Time.deltaTime;
        if (s.pitchtime >= s.pitchlen) {
          Pitch(c, s.pitche);
          s.pitchlen = 0;
        }
      }

      if (s.panlen != 0) {
        float step = s.pantime / s.panlen;
        Pan(c, s.pane * step + s.pans * (1 - step));
        s.pantime += Time.deltaTime;
        if (s.pantime >= s.panlen) {
          Pan(c, s.pane);
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
    int id = music.mblocks[currentPlayedMusicBlock];
    if (!music.blocks.ContainsKey(id)) throw new System.Exception("INvalid music block ID in music (" + currentPlayedMusicBlock + "th)");
    Block block = music.blocks[id];

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
        playing = false;
      }
      return;
    }

    // music: get and play note.
    PlayNote(block);
  }

  void PlayNote(Block block) {
    timeForNextBeat = 15f / block.bpm;
    for (int xc = 0; xc < music.numvoices; xc++) {
      int c = music.MusicVoices[xc];
      Note n = block.notes[currentPlayedMusicLine, c];
      if (n.freq != 0) Play(c, n.freq, n.nlen * timeForNextBeat);
      if (n.wave != 0) {
        Wave w =  music.waves.ContainsKey(n.wave) ? music.waves[n.wave] : null;
        if (w != null) {
          Wave(c, w.wave, w.phase);
          ADSR(c, w.a, w.d, w.s, w.r);
          if (w.rawPCM != null) Wave(c, w.rawPCM);
        }
      }
      if (n.vol != -1) {
        if (n.vlen < 2) {
          Volume(c, n.vol);
        }
        else {
          swipes[c].vols = Volume(c);
          swipes[c].vole = n.vol;
          swipes[c].voltime = 0;
          swipes[c].vollen = (n.vlen - 1) * 15f / block.bpm;
        }
      }
      if (n.plen < 2) {
        Pitch(c, n.pitch);
      }
      else if (n.plen != 255) {
        swipes[c].pitchs = Pitch(c);
        swipes[c].pitche = n.pitch;
        swipes[c].pitchtime = 0;
        swipes[c].pitchlen = (n.plen - 1) * 15f / block.bpm;
      }
      if (n.panlen < 2) {
        Pan(c, n.pan);
      }
      else if (n.panlen != 255) {
        swipes[c].pans = Pan(c);
        swipes[c].pane = n.pan;
        swipes[c].pantime = 0;
        swipes[c].panlen = (n.panlen - 1) * 15f / block.bpm;
      }
    }
    currentPlayedMusicLine++;
  }


  #endregion
}

public enum Waveform { Triangular=0, Saw=1, Square=2, Sin=3, Bass1=4, Bass2=5, Noise=6, PinkNoise=7, BrownNoise=8, BlackNoise=9, SoftNoise=10, Drums=11, SuperSaw=12, SuperSin=13, PCM=14 };

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
  public byte[] pcmdata;
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
    pcmdata = null;
    stopnow = false;
  }

  internal void SetVol(float vol) {
    audio.volume = vol;
    this.vol = vol;
  }

  internal float GetVol() {
    return vol;
  }

  internal void Play(float frequency, float length) {
    freq = frequency;
    time = 0;
    timeout = length;
    stopnow = false;
    audio.volume = vol;
    audio.Play();
  }
}

public class Music {
  public int numvoices;
  public byte[] MusicVoices = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
  public int numblocks;
  public byte[] mblocks;
  public Dictionary<int, Wave> waves = new Dictionary<int, Wave>();
  public Dictionary<int, Block> blocks = new Dictionary<int, Block>();

  public void Clear() {
    waves.Clear();
    blocks.Clear();
    mblocks = null;
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
}

public class Block {
  public int id;
  public int bpm;
  public int len;
  public Note[,] notes;
}

public struct Note {
  public short freq;
  public byte nlen;
  public byte wave;
  public float vol;
  public byte vlen;
  public float pitch;
  public byte plen;
  public float pan;
  public byte panlen;
}

/*
 * Define a way to play music: 2 bytes total len + 1 byte num channles + [1 byte channel, 2 bytes freq, 2 bytes len] * num channels
 * 
 * 
 */
