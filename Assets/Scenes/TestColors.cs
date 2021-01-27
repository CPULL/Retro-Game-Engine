using UnityEngine;
using UnityEngine.UI;

public class TestColors : MonoBehaviour {
  public RawImage img;

  Texture2D text;

  private void Start() {
    text = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };

    for (int x = 0; x < 16; x++)
      for (int y = 0; y < 16; y++) {
        int p = x + (y << 4);
          text.SetPixel(x, 15 - y, Col.GetColor((byte)p));
        }

    text.Apply();

    img.texture = text;
  }
}

/*

rgba ???0 -> 1 bit per color and alpha 33% or 66%
rrgg bb?1 -> 2 bits per color + 4 light level?

16 for partial alpha
1 for full alpha
216 for full colors (6 values for each chroma)
 23

6*6*6 = 216
40 for alphas (1 bit each chroma + 5 level of alpha)

*/
