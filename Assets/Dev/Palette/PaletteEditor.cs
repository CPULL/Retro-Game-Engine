using UnityEngine;

public class PaletteEditor : MonoBehaviour {
  Pixel[] pixels = new Pixel[256];
  public Transform PaletteContainer;

  void Start() {
    int pos = 0;
    foreach(Transform t in PaletteContainer) {
      pixels[pos] = t.GetComponent<Pixel>();
      pixels[pos].Init(pos, Col.GetColor((byte)pos), SelectPalettePixel, Color.black);
      pos++;
    }

  }

  // Update is called once per frame
  void Update() {

  }

  void SelectPalettePixel(Pixel p) {
    Debug.Log(p.pos);
  }

}

/*

some way to get a color by rgb and hsl
a way to load a sprite and see it with a palette
a way to get a sprite from normal and convert to palette (generating palette or using current palette)
a way to save a converted sprite
a way to convert back a sprite to normal mode

load/save buttons (bin and text)


*/