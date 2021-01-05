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
    clips[0] = AudioClip.Create("Channel0", samplerate * 2, 1, samplerate, true, OnAudioRead0, OnAudioSetPosition0);
    timeouts = new float[channels.Length];
    for (int i = 0; i < channels.Length; i++) {
      channels[i].clip = clips[i];
      timeouts[i] = -1f;
      waveforms[i] = new Waveform(Waveform.Wave.Triangular);
    }
  }


  public void Play(int channel, Waveform.Wave wave, float phase, int freq, float length = -1) {
    if (channel < 0 || channel >= channels.Length) throw new System.Exception("Invalid audio channel: " + channel);
    if (phase < .01f) phase = .01f;
    if (phase > 10) phase = 10;
    if (freq < 50) freq = 50;
    if (freq > 18000) freq = 18000;

    waveforms[channel].wave = wave;
//FIXME    waveforms[channel].phase = phase; // If square, make it between 0 and 1 excluded
//    waveforms[channel].freq = freq;
    waveforms[channel].position = 0;

    channels[channel].Play();
    timeouts[channel] = length;
  }

  private void Update() {
    for (int i = 0; i < channels.Length; i++) {
      if (timeouts[i] > -1) timeouts[i] -= Time.deltaTime;
      if (timeouts[i] > -1 && timeouts[i] <= 0) {
        timeouts[i] = -1;
        channels[i].Stop();
      }
    }
  }

  const float piP2 = Mathf.PI * 2f;
  const float piH2 = Mathf.PI * .5f;
  const float piD2 = 2f / Mathf.PI;


  void OnAudioRead0(float[] data) {
    switch (waveforms[0].wave) {
      case Waveform.Wave.Triangular:
        for (int i = 0; i < data.Length; i++) {
          waveforms[0].position++;
          if (waveforms[0].position > samplerate) waveforms[0].position = 0;
          float pos = waveforms[0].freq * waveforms[0].position / samplerate;
          data[i] = piD2 * Mathf.Asin(Mathf.Cos(piP2 * pos)) * waveforms[0].phase;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        break;

      case Waveform.Wave.Saw:
        for (int i = 0; i < data.Length; i++) {
          waveforms[0].position++;
          if (waveforms[0].position > samplerate) waveforms[0].position = 0;
          float pos = waveforms[0].freq * waveforms[0].position / samplerate;
          if (pos == 0)
            data[i] = 0;
          else
            data[i] = 1f - (piH2 - Mathf.Atan(waveforms[0].phase * Mathf.Tan(piD2 * pos))) * piD2;
          if (data[i] < -1f) data[i] = -1f;
          if (data[i] > 1f) data[i] = 1f;
        }
        break;

      case Waveform.Wave.Square:
        for (int i = 0; i < data.Length; i++) {
          waveforms[0].position++;
          if (waveforms[0].position > samplerate) waveforms[0].position = 0;
          float pos = waveforms[0].freq * waveforms[0].position / samplerate;
          pos -= (int)pos;
          if (pos < waveforms[0].phase)
            data[i] = .9f;
          else
            data[i] = -.9f;
        }
        break;

      case Waveform.Wave.Sin:
        for (int i = 0; i < data.Length; i++) {
          waveforms[0].position++;
          if (waveforms[0].position > samplerate) waveforms[0].position = 0;
          float pos = waveforms[0].freq * waveforms[0].position / samplerate;
          data[i] = (Mathf.Sin(2 * Mathf.PI * pos) + Mathf.Sin(2 * Mathf.PI * pos * waveforms[0].phase)) * .5f;
        }
        break;

      case Waveform.Wave.Noise:
        for (int i = 0; i < data.Length; i++) {
          waveforms[0].position++;
          if (waveforms[0].position > samplerate) waveforms[0].position = 0;
          float pos = waveforms[0].freq * waveforms[0].position / samplerate;

          data[i] = Squirrel3Norm((int)pos, (uint)(waveforms[0].phase * 1000));
        }
        break;
    }
  }

  void OnAudioSetPosition0(int newPosition) {
    waveforms[0].position = newPosition;
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

  /*



def squirrel3(n, seed=0):
    """Returns an unsigned integer containing 32 reasonably-well-scrambled
    bits, based on a given (signed) integer input parameter `n` and optional
    `seed`.  Kind of like looking up a value in an infinitely large
    non-existent table of previously generated random numbers.
    """


class Squirrel3Random(random.Random):

    _n = 0

    def seed(self, a=None):
        if a is None:
            a = 0
        self._seed = a

    def random(self):
        n = self._n
        self._n += 1
        return squirrel3(n, self._seed) / float(CAP)

  */
}

[System.Serializable]
public struct Waveform {
  public enum Wave { Triangular, Saw, Square, Sin, Noise };
  public Wave wave;
  public float phase;
  public float freq;
  public int position;

  public Waveform(Wave w) {
    wave = w;
    phase = 1f;
    freq = 440;
    position = 0;
  }
}

