using UnityEngine;
using UnityEngine.UI;

public class Dev : MonoBehaviour {
  public static Dev inst;
  public GameObject EmptyEditor;
  public SpriteEditor spriteEditor;
  public WaveformEditor waveformEditor;
  public TilemapEditor tilemapEditor;
  public MusicEditor musicEditor;
  public FontsEditor fontsEditor;
  public CodeEditor codeEditor;
  public RomEditor romEditor;
  public Image[] Selection;

  private void Awake() {
    inst = this;
  }

  public void CodeEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 0;
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(true);
    romEditor.gameObject.SetActive(false);
  }

  public void RomEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 1;
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(true);
  }

  public void FontsEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 2;
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(true);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
  }

  public void SpriteEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 3;
    spriteEditor.gameObject.SetActive(true);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
  }

  public void TilemapEditor() {
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 4;
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(true);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
  }

  public void WaveformEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 5;
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(true);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
  }

  public void MusicEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 6;
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(true);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
  }


}

