using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Dev : MonoBehaviour {

  public GameObject EmptyEditor;
  public SpriteEditor spriteEditor;
  public WaveformEditor waveformEditor;

  private void Start() {
  }

  public void SpriteEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(true);
    waveformEditor.gameObject.SetActive(false);
  }

  public void WaveformEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(false);
    waveformEditor.gameObject.SetActive(true);
  }
}

