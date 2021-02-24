using System;
using UnityEngine;

public class BackgroundLine : MonoBehaviour {
  public int lineNumber = 0;
  public Action<int> CallBack { get; internal set; }

  void OnMouseEnter() {
    CallBack?.Invoke(lineNumber);
  }
}
