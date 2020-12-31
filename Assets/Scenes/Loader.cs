using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Loader : MonoBehaviour {
  public Text Loading;
  public Text Help;
  public GameObject Buttons;
 
  void Start() {
    string[] args = System.Environment.GetCommandLineArgs();
    if (args.Length == 0) SceneManager.LoadScene("Arcade");
    if (args[0].ToLowerInvariant() == "-sel") SceneManager.LoadScene("ArcadePlus");
    if (args[0].ToLowerInvariant() == "-dev") SceneManager.LoadScene("Developer");
    StartCoroutine(ShowButtonsDelayed());
  }

  IEnumerator ShowButtonsDelayed() {
    yield return new WaitForSeconds(1);
    Loading.enabled = false;
    Buttons.SetActive(true);
  }

  public void ShowHelp(int pos) {
    if (pos == -1) {
      Help.text = "";
    }
    else if (pos == 0) {
      Help.text = "<b>Arcade</b>\nStarts <b>RGE</b> and loads a program called <i>game.cartrige</i> from the <i>Cartriges</i> folder.\nThe program is run as soon it is loaded.\n\n<i>Command line option: <b>-game</b></i>";
    }
    else if (pos == 1) {
      Help.text = "<b>ArcadePlus</b>\nStart the <b>RetroGameEngine</b> without loading a cartridge.\nA menu will allow to select one of the availables cartridges and run it.\n\n<i>Command line option: <b>-sel</b></i>";
    }
    else if (pos == 2) {
      Help.text = "<b>Development</b>\nStart the development tool, used to create sprites, tiles, music, and debug the cartridge programs.\n\n<i>Command line option: <b>-dev</b></i>";
    }
  }

  public void StartArcade() {
    SceneManager.LoadScene("Arcade");
  }
  public void StartArcadePlus() {
    SceneManager.LoadScene("ArcadePlus");
  }
  public void StartDevelopment() {
    SceneManager.LoadScene("Developer");
  }
}
