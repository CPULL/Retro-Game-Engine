using UnityEngine;
using UnityEngine.UI;

public class Dev : MonoBehaviour {

  private void Start() {
    WidthSlider.SetValueWithoutNotify(8);
    HeightSlider.SetValueWithoutNotify(8);
    pixels = new Pixel[0];
    ChangeSpriteSize();
  }

  #region Sprite Editor ************************************************************************************************************************************************************************************

  public GameObject PixelPrefab;
  public GridLayoutGroup SpriteGrid;
  Pixel[] pixels;
  public Slider WidthSlider;
  public Slider HeightSlider;
  public Text WidthSliderText;
  public Text HeightSliderText;
  public InputField Values;

  public Image CurrentColor;
  Color32 Transparent = new Color32(0, 0, 0, 0);

  public void ChangeSpriteSize() {
    WidthSliderText.text = "Width: " + WidthSlider.value;
    HeightSliderText.text = "Height: " + HeightSlider.value;
    Rect rt = SpriteGrid.transform.GetComponent<RectTransform>().rect;
    SpriteGrid.cellSize = new Vector2(rt.width / WidthSlider.value, rt.height / HeightSlider.value);

    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    int numnow = num;
    Pixel[] pixels2 = new Pixel[num];
    if (pixels.Length < num) num = pixels.Length;
    for (int i = 0; i < num; i++)
      pixels2[i] = pixels[i];
    pixels = pixels2;
    if (num < numnow) {
      for (int i = num; i < numnow; i++) {
        Pixel pixel = Instantiate(PixelPrefab, SpriteGrid.transform).GetComponent<Pixel>();
        pixels2[i] = pixel;
        pixel.Init(i, Transparent, ClickPixel);
      }
    }
    else if (num > numnow) {
      for (int i = numnow; i < num; i++) {
        Destroy(SpriteGrid.transform.GetChild(i));
      }
    }
  }

  private void ClickPixel(int pos) {
    if (pixels[pos].img.color == CurrentColor.color)
      pixels[pos].Set(Transparent);
    else
      pixels[pos].Set(CurrentColor.color);
  }

  public void Clear() {
    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    for (int i = 0; i < num; i++)
      pixels[i].Set(Transparent);
  }

  public void Fill() {
    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    for (int i = 0; i < num; i++)
      pixels[i].Set(CurrentColor.color);
  }

  public void Save() {
    string res = "SpriteSize:\n";
    byte sizex = ((byte)((byte)WidthSlider.value & 31));
    byte sizey = ((byte)((byte)HeightSlider.value & 31));
    res += "0x" + sizex.ToString("X2") + " 0x" + sizey.ToString("X2") + "\n";
    res += "Sprite:";
    int num = (int)WidthSlider.value * (int)HeightSlider.value;
    for (int i = 0; i < num; i++) {
      if (i % sizex == 0) res += "\n";
      Color32 c = pixels[i].img.color;
      int r = c.r / 85;
      int g = c.g / 85;
      int b = c.b / 85;
      int a = 255 - (c.a / 85); 
      byte col = (byte)((a << 6) + (r << 4) + (g << 2) + (b << 0));
      res += "0x" + col.ToString("X2") + " ";
    }
    Values.gameObject.SetActive(true);
    Values.text = res;
  }

  public void CloseValues() {
    Values.gameObject.SetActive(false);
  }



  #endregion Sprite Editor
}
