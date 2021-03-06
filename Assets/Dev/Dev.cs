﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dev : MonoBehaviour {
  public static Dev inst;
  public GameObject EmptyEditor;
  public PaletteEditor paletteEditor;
  public SpriteEditor spriteEditor;
  public ImageEditor imageEditor;
  public WaveformEditor waveformEditor;
  public TilemapEditor tilemapEditor;
  public MusicEditor musicEditor;
  public FontsEditor fontsEditor;
  public CodeEditor codeEditor;
  public RomEditor romEditor;
  public Image[] Selection;
  public Material RGEPalette;

  private void Awake() {
    inst = this;
    Col.InitPalette(RGEPalette);
  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.F11)) {
      Screen.fullScreen = !Screen.fullScreen;
    }
    if (Input.GetKeyUp(KeyCode.Escape) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) {
      UnityEngine.SceneManagement.SceneManager.LoadScene("Loader");
    }
  }

  public void CodeEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 0;
    spriteEditor.gameObject.SetActive(false);
    imageEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(true);
    romEditor.gameObject.SetActive(false);
    paletteEditor.gameObject.SetActive(false);
    RGEPalette.SetInt("_UsePalette", 1);
  }

  public void RomEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 1;
    spriteEditor.gameObject.SetActive(false);
    imageEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(true);
    paletteEditor.gameObject.SetActive(false);
  }

  public void FontsEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 2;
    spriteEditor.gameObject.SetActive(false);
    imageEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(true);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
    paletteEditor.gameObject.SetActive(false);
  }

  public void SpriteEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 3;
    spriteEditor.gameObject.SetActive(true);
    imageEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
    paletteEditor.gameObject.SetActive(false);
    RGEPalette.SetInt("_UsePalette", 1);
  }

  public void ImageEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 4;
    spriteEditor.gameObject.SetActive(false);
    imageEditor.gameObject.SetActive(true);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
    paletteEditor.gameObject.SetActive(false);
    RGEPalette.SetInt("_UsePalette", 1);
  }

  public void TilemapEditor() {
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 5;
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    imageEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(true);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
    paletteEditor.gameObject.SetActive(false);
    RGEPalette.SetInt("_UsePalette", 1);
  }

  public void PaletteEditor() {
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 6;
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    imageEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
    paletteEditor.gameObject.SetActive(true);
  }

  public void WaveformEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 7;
    spriteEditor.gameObject.SetActive(false);
    imageEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(true);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    romEditor.gameObject.SetActive(false);
    paletteEditor.gameObject.SetActive(false);
  }

  public void MusicEditor() {
    EmptyEditor.SetActive(false);
    for (int i = 0; i < Selection.Length; i++)
      Selection[i].enabled = i == 8;
    spriteEditor.gameObject.SetActive(false);
    imageEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(true);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
    paletteEditor.gameObject.SetActive(false);
  }

  internal void HandleError(string err) {
    PBar.Hide();
    FileBrowser.Hide();
    Error.SetActive(true);
    ErrorMsg.text = err;
    Debug.Log(err);
  }

  public GameObject Error;
  public TextMeshProUGUI ErrorMsg;
  public void CloseError() {
    Error.SetActive(false);
  }
}

public enum EditComponent {
  SpriteDitor,
  WaveEditor,
  MusicEditor,
  TilesEditor,
  RomEditor,
  CodeEditor,
  FontEditor,
}
