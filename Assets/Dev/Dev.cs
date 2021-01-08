using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Dev : MonoBehaviour {

  public GameObject EmptyEditor;
  public SpriteEditor spriteEditor;
  public WaveformEditor waveformEditor;
  public TileEditor tileEditor;
  public TilemapEditor tilemapEditor;
  public MusicEditor musicEditor;
  public FontsEditor fontsEditor;
  public CodeEditor codeEditor;

  private void Start() {
  }

  public void SpriteEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(true);
    waveformEditor.gameObject.SetActive(false);
    tileEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
  }

  public void WaveformEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(true);
    tileEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
  }

  public void MusicEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tileEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(true);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
  }

  public void TilesEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tileEditor.gameObject.SetActive(true);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
  }

  public void TilemapEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tileEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(true);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(false);
  }

  public void FontsEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tileEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(true);
    codeEditor.gameObject.SetActive(false);
  }

  public void CodeEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(false);
    tileEditor.gameObject.SetActive(false);
    tilemapEditor.gameObject.SetActive(false);
    musicEditor.gameObject.SetActive(false);
    fontsEditor.gameObject.SetActive(false);
    codeEditor.gameObject.SetActive(true);
  }
}

