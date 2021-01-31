using System;
using TMPro;
using UnityEngine;

public class Confirm : MonoBehaviour {
  public TextMeshProUGUI Message;
  Action yes;
  Action no;
  public void Set(string msg, Action yes, Action no = null) {
    gameObject.SetActive(true);
    if (msg == null)
      Message.text = "Do you confirm?";
    else
      Message.text = msg;
    this.yes = yes;
    this.no = no;
  }

  public void Yes() {
    gameObject.SetActive(false);
    yes?.Invoke();
  }
  public void No() {
    gameObject.SetActive(false);
    no?.Invoke();
  }
}
