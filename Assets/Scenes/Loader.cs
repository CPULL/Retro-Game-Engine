using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour {
 
  void Start() {
    string[] args = System.Environment.GetCommandLineArgs();
    if (args.Length == 0) SceneManager.LoadScene("Arcade");
    if (args[0].ToLowerInvariant() == "-sel") SceneManager.LoadScene("ArcadePlus");
    if (args[0].ToLowerInvariant() == "-dev") SceneManager.LoadScene("Developer");
    SceneManager.LoadScene("Arcade");
  }
}
