using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour {
  private static FileBrowser inst;
  Action<string> postLoadAction;
  string currentpath;

  public GameObject FileBrowserContents;
  public GameObject FileTemplate;
  public GameObject FolderTemplate;
  public Transform Items;
  public Text PathText;
  public Text FileInfoText;
  public Button LoadButton;

  private void Awake() {
    inst = this;
    FileBrowserContents.SetActive(false);
  }

  public static void Show(Action<string> action) {
    inst.FileBrowserContents.SetActive(true);
    inst.postLoadAction = action;
    inst.LoadButton.interactable = false;
    FileInfo fi = new FileInfo(Application.dataPath);
    inst.ShowFolder(fi.Directory.Parent.FullName);
  }

  private void ShowFolder(string path) {
    currentpath = path;
    PathText.text = path;
    FileInfoText.gameObject.SetActive(false);
    foreach (Transform t in Items)
      Destroy(t.gameObject);

    LoadButton.interactable = false;
    try {
      string[] dirs = Directory.GetDirectories(path);
      string[] fils = Directory.GetFiles(path);
      foreach(string dp in dirs) {
        DirectoryInfo dir = new DirectoryInfo(dp);
        GameObject go = Instantiate(FolderTemplate, Items);
        go.SetActive(true);
        go.GetComponent<Button>().onClick.AddListener(() => { SelectFolder(dir.FullName); });
        go.GetComponentInChildren<Text>().text = dir.Name;
      }
      foreach(string dp in fils) {
        FileInfo fi = new FileInfo(dp);
        string ext = fi.Extension.ToLowerInvariant();
        if (ext != ".mp3" && ext != ".ogg" && ext != ".wav") continue;
        GameObject go = Instantiate(FileTemplate, Items);
        go.SetActive(true);
        go.GetComponent<Button>().onClick.AddListener(() => { SelectFile(fi.FullName); });
        go.GetComponentInChildren<Text>().text = fi.Name;
      }
    }
    catch (Exception e) {
      PathText.text = "Error: " + e.Message;
    }
  }

  public void SelectFile(string path) {
    currentpath = path;
    foreach (Transform t in Items)
      Destroy(t.gameObject);
    LoadButton.interactable = true;
    FileInfo fi = new FileInfo(path);
    FileInfoText.gameObject.SetActive(true);
    FileInfoText.text = "File: " + fi.Name + "\nPath: " + fi.Directory.FullName + "\nSize: " + fi.Length + "\nExtension: " + fi.Extension;
  }

  public void SelectFolder(string path) {
    ShowFolder(path);
  }

  public void Parent() {
    LoadButton.interactable = true;
    DirectoryInfo di = new DirectoryInfo(currentpath);
    di = di.Parent;
    if (di == null) return;
    ShowFolder(di.FullName);
  }

  public void LoadFile() {
    FileBrowserContents.SetActive(false);
    postLoadAction?.Invoke(currentpath);
  }

  public void Exit() {
    FileBrowserContents.SetActive(false);
  }
}
