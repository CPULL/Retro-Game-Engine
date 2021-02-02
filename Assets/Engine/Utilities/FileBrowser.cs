using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour {
  private static FileBrowser inst;
  Action<string> postLoadAction;
  Action<string, string> postSaveAction;
  string currentpath;

  public GameObject FileBrowserContents;
  public GameObject FileTemplate;
  public GameObject FolderTemplate;
  public Transform Items;
  public TextMeshProUGUI PathText;
  public TextMeshProUGUI FileInfoText1;
  public TextMeshProUGUI FileInfoText2;
  public Button LoadButton;
  public Button SaveButton;
  public TMP_InputField FileName;
  public enum FileType { Music, Pics, Cartridges, Rom };
  FileType fileType;
  string lastFolder;
  bool load = true;
  public Confirm Confirm;

  private void Awake() {
    inst = this;
    FileBrowserContents.SetActive(false);
  }

  public static void Load(Action<string> action, FileType ft) {
    inst.FileBrowserContents.SetActive(true);
    inst.postLoadAction = action;
    inst.LoadButton.interactable = false;
    inst.SaveButton.interactable = false;
    inst.LoadButton.gameObject.SetActive(true);
    inst.SaveButton.gameObject.SetActive(false);
    inst.FileName.gameObject.SetActive(false);
    inst.load = true;

    // Does the directory exist?
    inst.lastFolder = PlayerPrefs.GetString("LastFolder", Application.dataPath);
    if (inst.lastFolder == null || !Directory.Exists(inst.lastFolder))
      inst.lastFolder = Application.dataPath;

    DirectoryInfo di = new DirectoryInfo(inst.lastFolder);
    inst.fileType = ft;
    inst.ShowFolder(di.FullName);
  }

  internal static bool IsVisible() {
    if (inst == null) return false;
    return inst.gameObject.activeSelf;
  }

  public static void Save(Action<string, string> action, FileType ft) {
    inst.FileBrowserContents.SetActive(true);
    inst.postSaveAction = action;
    inst.LoadButton.interactable = false;
    inst.SaveButton.interactable = false;
    inst.LoadButton.gameObject.SetActive(false);
    inst.SaveButton.gameObject.SetActive(true);
    inst.FileName.gameObject.SetActive(true);
    inst.load = false;

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
    FileInfoText1.gameObject.SetActive(false);
    FileInfoText2.gameObject.SetActive(false);
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
          case FileType.Rom:
            if (ext != ".rom") continue;
            break;
        }
        GameObject go = Instantiate(FileTemplate, Items);
        go.SetActive(true);
        if (load)
          go.GetComponent<Button>().onClick.AddListener(() => { SelectFileLoad(fi.FullName); });
        else
          go.GetComponent<Button>().onClick.AddListener(() => { SelectFileSave(fi.FullName); });
        go.GetComponentInChildren<TextMeshProUGUI>().text = fi.Name;
      }
    }
    catch (Exception e) {
      PathText.text = "Error: " + e.Message;
    }
  }

  public void SelectFileLoad(string path) {
    currentpath = path;
    LoadButton.interactable = true;
    FileInfo fi = new FileInfo(path);
    FileInfoText1.gameObject.SetActive(true);
    FileInfoText2.gameObject.SetActive(true);
    FileInfoText1.text = "File: " + fi.Name + "\nPath: " + fi.Directory.FullName;
    FileInfoText2.text = "Size: " + fi.Length + "\nExtension: " + fi.Extension;
  }

  public void SelectFileSave(string path) {
    SaveButton.interactable = true;
    FileInfo fi = new FileInfo(path);
    FileInfoText1.gameObject.SetActive(true);
    FileInfoText2.gameObject.SetActive(true);
    FileInfoText1.text = "File: " + fi.Name + "\nPath: " + fi.Directory.FullName;
    FileInfoText2.text = "Size: " + fi.Length + "\nExtension: " + fi.Extension;
    FileName.SetTextWithoutNotify(fi.Name);
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
    PlayerPrefs.SetString("LastFolder", lastFolder);
    FileBrowserContents.SetActive(false);
    postLoadAction?.Invoke(currentpath);
  }

  public void SaveNameUpdate() {
    SaveButton.interactable = !string.IsNullOrEmpty(FileName.text.Trim());
  }

  public void SaveFile() {
    string name = FileName.text.Trim();
    savedname = null;
    if (string.IsNullOrEmpty(name)) return;
    string extcheck = ("    " + name).ToLowerInvariant();
    string ext = "";
    switch (fileType) {
      case FileType.Music: ext = ".wav"; break;
      case FileType.Pics: ext = ".png"; break;
      case FileType.Cartridges: ext = ".cartridge"; break;
      case FileType.Rom: ext = ".rom"; break;
    }
    if (extcheck.Substring(extcheck.Length - ext.Length) != ext) name += ext;

    lastFolder = currentpath;
    PlayerPrefs.SetString("LastFolder", lastFolder);

    if (File.Exists(Path.Combine(currentpath, name))) {
      savedname = name;
      Confirm.Set("File " + name + " already exists.\nDo you want to overwrite?", ConfirmOverwrite);
    }
    else {
      postSaveAction?.Invoke(currentpath, name);
      FileBrowserContents.SetActive(false);
    }
  }

  string savedname = null;
  public void ConfirmOverwrite() {
    FileBrowserContents.SetActive(false);
    postSaveAction?.Invoke(currentpath, savedname);
    savedname = null;
  }

  public void Exit() {
    FileBrowserContents.SetActive(false);
  }
}
