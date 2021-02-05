using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaletteEditor : MonoBehaviour {
  Pixel[] pixels = new Pixel[256];
  public Transform PaletteContainer;
  public Slider HueSlider;
  public Slider RSlider;
  public TextMeshProUGUI RVal;
  public Slider GSlider;
  public TextMeshProUGUI GVal;
  public Slider BSlider;
  public TextMeshProUGUI BVal;
  public Slider ASlider;
  public TextMeshProUGUI AVal;
  public TMP_InputField HexColor;
  public RawImage SelectedColor;
  public RawImage ColorPicker;
  Texture2D ColorPickerTexture;

  void Start() {
    int pos = 0;
    foreach(Transform t in PaletteContainer) {
      pixels[pos] = t.GetComponent<Pixel>();
      pixels[pos].Init(pos, Col.GetColor((byte)pos), SelectPalettePixel, Color.black);
      pos++;
    }

    ColorPickerTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
    for (int y = 255; y >= 0; y--)
      for (int x = 0; x < 255; x++)
        ColorPickerTexture.SetPixel(x, y, new Color32((byte)x, (byte)y, 0, 255));
    ColorPickerTexture.Apply();
    ColorPicker.texture = ColorPickerTexture;
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