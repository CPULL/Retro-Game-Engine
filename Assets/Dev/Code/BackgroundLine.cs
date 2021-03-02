using System;
using TMPro;
using UnityEngine;

public class BackgroundLine : MonoBehaviour {
  public int lineNumber = 0;
  public TextMeshProUGUI Text;
  public Action<int> CallBack { get; internal set; }
  public Action<int> CallClick { get; internal set; }
  bool over = false;

  void OnMouseEnter() {
    CallBack?.Invoke(lineNumber);
    over = true;
  }
  void OnMouseExit() {
    over = false;
  }

  private void Update() {
    if (CallClick == null) return;
    if (Input.GetMouseButtonDown(0) && over) {
      CallClick?.Invoke(lineNumber);
    }
  }

}
