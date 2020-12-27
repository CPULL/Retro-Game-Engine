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

  public Camera cam;
  public GameObject PixelPrefab;
  public GridLayoutGroup SpriteGrid;
  Pixel[] pixels;
  public Slider WidthSlider;
  public Slider HeightSlider;
  public Text WidthSliderText;
  public Text HeightSliderText;

  Color32 Current = new Color32(255, 255, 0, 255);
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
        pixel.Init(i, ClickPixel);
      }
    }
    else if (num > numnow) {
      for (int i = numnow; i < num; i++) {
        Destroy(SpriteGrid.transform.GetChild(i));
      }
    }
  }

  private void ClickPixel(int pos) {
    if (pixels[pos].img.color == Current)
      pixels[pos].Set(Transparent);
    else
      pixels[pos].Set(Current);
  }
  #endregion Sprite Editor
}
