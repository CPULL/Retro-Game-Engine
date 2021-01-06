using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Dev : MonoBehaviour {

  public GameObject EmptyEditor;
  public SpriteEditor spriteEditor;

  private void Start() {
  }

  public void SpriteEditor() {
    EmptyEditor.SetActive(false);
    spriteEditor.gameObject.SetActive(true);
  }
}

