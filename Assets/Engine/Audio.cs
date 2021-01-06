using UnityEngine;

public class Audio : MonoBehaviour {
  public AudioSource[] srcs;
  public Channel[] channels;
  const int samplerate = 44100;


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

    if (attack ==0 && decay == 0 && sustain == 0 && release == 0) {
      channels[channel].adsrV = false;
      return;
    }
    channels[channel].adsrV = true;
    channels[channel].av = 0.015625f * attack + 0.001f; // 0.001s -> 4s
    channels[channel].dv = 0.02344f * decay + 0.001f; // 0.001s -> 6s
    channels[channel].sv = sustain / 255f; // % of volume = sv/255
    channels[channel].rv = 0.02344f * release + 0.001f; // 0.001s -> 6s
  }

  byte[] toplay = null;
  int playpos = -1;
  float musicsteplen = 0;
  public void Play(byte[] music) {
    toplay = music;
    playpos = 0;
  }

  private void Update() {
    for (int i = 0; i < channels.Length; i++) {
      if (!channels[i].audio.isPlaying) continue;
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
      if (t >= channels[i].timeout) {
        if (channels[i].adsrV) {
          if (t >= channels[i].timeout + channels[i].rv)
            channels[i].audio.Stop();
          else
            channels[i].audio.volume = channels[i].vol * (channels[i].timeout + channels[i].rv - t) * channels[i].sv;
        }
        else {
          channels[i].audio.Stop();
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


  //  Define a way to play music: 2 bytes total len + 1 byte num channles + [1 byte channel, 2 bytes freq, 2 bytes len] * num channels



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

      case Waveform.Square:
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          pos -= (int)pos;
          if (pos < channels[channel].phase)
            data[i] = .9f;
          else
            data[i] = -.9f;
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

      case Waveform.Noise:
        for (int i = 0; i < data.Length; i++) {
          channels[channel].position++;
          if (channels[channel].position >= samplerate) channels[channel].position = 0;
          float pos = channels[channel].freq * channels[channel].position / samplerate;
          data[i] = Squirrel3Norm((int)pos, (uint)(channels[channel].phase * 1000));
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

public enum Waveform { Triangular, Saw, Square, Sin, Noise };

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
  }

  internal void SetVol(float vol) {
    audio.volume = vol;
    this.vol = vol;
  }

  internal void Play(int frequency, float length) {
    freq = frequency;
    time = 0;
    timeout = length;
    audio.Play();
  }
}
