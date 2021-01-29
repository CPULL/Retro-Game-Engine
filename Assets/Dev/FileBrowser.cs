using System;
using System.IO;
using TMPro;
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
  public TextMeshProUGUI PathText;
  public TextMeshProUGUI FileInfoText;
  public Button LoadButton;
  public enum FileType { Music, Pics, Cartridges };
  FileType fileType;
  string lastFolder;

  private void Awake() {
    inst = this;
    FileBrowserContents.SetActive(false);
  }

  public static void Show(Action<string> action, FileType ft) {
    inst.FileBrowserContents.SetActive(true);
    inst.postLoadAction = action;
    inst.LoadButton.interactable = false;

    // Does the directory exist?
    if (inst.lastFolder == null && !Directory.Exists(inst.lastFolder))
      inst.lastFolder = Application.dataPath;

    DirectoryInfo di = new DirectoryInfo(inst.lastFolder);
    inst.fileType = ft;
    inst.ShowFolder(di.FullName);
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
        go.GetComponentInChildren<TextMeshProUGUI>().text = dir.Name;
      }
      foreach(string dp in fils) {
        FileInfo fi = new FileInfo(dp);
        string ext = fi.Extension.ToLowerInvariant();
        switch (fileType) {
          case FileType.Music:
            if (ext != ".mp3" && ext != ".ogg" && ext != ".wav") continue;
            break;
          case FileType.Pics:
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif") continue;
            break;
          case FileType.Cartridges:
            if (ext != ".cartridge") continue;
            break;
        }
        GameObject go = Instantiate(FileTemplate, Items);
        go.SetActive(true);
        go.GetComponent<Button>().onClick.AddListener(() => { SelectFile(fi.FullName); });
        go.GetComponentInChildren<TextMeshProUGUI>().text = fi.Name;
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
    FileInfo fi = new FileInfo(currentpath);
    lastFolder = fi.Directory.FullName;
    FileBrowserContents.SetActive(false);
    postLoadAction?.Invoke(currentpath);
  }

  public void Exit() {
    FileBrowserContents.SetActive(false);
  }
}
