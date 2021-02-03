using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RomEditor : MonoBehaviour {
  public GameObject LineTemplate;
  public Transform Container;
  readonly List<RomLine> lines = new List<RomLine>();
  readonly Dictionary<string, RomLine> names = new Dictionary<string, RomLine>();
  public Confirm Confirm;
  public TMP_InputField Values;
  public Button LoadSubButton;
  public TMP_InputField GlobalName;
  public Toggle GlobalCheckmark;
  public TMP_Dropdown ItemType;
  public SpriteEditor spriteEditor;
  public TilemapEditor tilemapEditor;
  public MusicEditor musicEditor;
  public WaveformEditor waveformpEditor;
  public Button EditButton;

  readonly Regex rgNumPart = new Regex("([^0-9]*([0-9]*))+", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));

  string HandleDuplicateNames(string name, RomLine line) {
    name = NormLabel.Normalize(name);

    if (!names.ContainsKey(name.ToLowerInvariant())) {
      names.Add(name.ToLowerInvariant(), line);
      return name;
    }

    // Find any numeric part at the end
    Match m = rgNumPart.Match(name);
    if (!m.Success) return HandleDuplicateNames(name + "_0", line);


    Group group = m.Groups[m.Groups.Count - 1];
    int num = 0;
    string val = "";
    for (int i = group.Captures.Count - 1; i >= 0; i--) {
      val = group.Captures[i].Value;
      if (string.IsNullOrEmpty(val)) continue;
      int.TryParse(val, out num);
    }
    if (string.IsNullOrEmpty(val))
      name += "_0";
    else
      name = name.Replace(val, (num + 1).ToString());
    return HandleDuplicateNames(name, line);
  }

  public void LoadTextPre() {
    Values.gameObject.SetActive(true);
    LoadSubButton.enabled = true;
  }

  public void LoadTextPost() {
    if (!gameObject.activeSelf) return;
    StartCoroutine(LoadingTextPost());
  }
  IEnumerator LoadingTextPost() {
    yield return PBar.Show("Loading", 0, 256);
    List<CodeLabel> labels;
    byte[] block;
    try {
      ByteReader.ReadBlock(Values.text.Trim(), out labels, out block);
    } catch (System.Exception e) {
      Values.text = "Parsing error: " + e.Message + "\n" + Values.text;
      PBar.Hide();
      yield break;
    }

    int num = labels.Count;
    int step = 0;
    int start = lines.Count;

    foreach (CodeLabel l in labels) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(50 + 100 * step / num);
      RomLine line = Instantiate(LineTemplate, Container).GetComponent<RomLine>();

      l.name = HandleDuplicateNames(l.name, line);
      line.gameObject.name = l.name;
      line.gameObject.SetActive(true);
      line.Label.SetTextWithoutNotify(l.name);
      line.Type.text = (int)l.type + " " + l.type.ToString();
      line.ltype = l.type;
      lines.Add(line);
      line.Delete.onClick.AddListener(() => { Delete(line); });
      line.MoveUp.onClick.AddListener(() => { MoveUp(line); });
      line.MoveDown.onClick.AddListener(() => { MoveDown(line); });
      line.Label.onEndEdit.AddListener((name) => { UpdateName(line, name); });
      line.Check.onValueChanged.AddListener((check) => { SelectLine(line, check); });
    }
    step = 0;
    for (int i = 0; i < labels.Count - 1; i++) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(150 + 100 * step / num);
      int size = labels[i + 1].start - labels[i].start;
      lines[start + i].size = size;
      lines[start + i].Size.text = size.ToString();
    }
    lines[start + labels.Count - 1].size = block.Length - labels[labels.Count - 1].start;
    lines[start + labels.Count - 1].Size.text = lines[start + labels.Count - 1].size.ToString();

    step = 0;
    for (int i = 0; i < labels.Count; i++) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(250 + 100 * step / num);
      int size = lines[start + i].size;
      byte[] data = new byte[size];
      for (int j = 0; j < size; j++) {
        data[j] = block[labels[i].start + j];
      }
      lines[start + i].Data = data;
    }
    PBar.Hide();

    Values.gameObject.SetActive(false);
    LoadSubButton.enabled = false;
  }

  public void Load() {
    FileBrowser.Load(PostLoad, FileBrowser.FileType.Rom);
  }

  public void PostLoad(string path) {
    foreach (Transform t in Container)
      Destroy(t.gameObject);
    lines.Clear();
    names.Clear();
    StartCoroutine(PostLoading(path));
  }

  IEnumerator PostLoading(string path) {
    yield return PBar.Show("Loading", 25, 350);
    ByteChunk res = new ByteChunk();
    ByteReader.ReadBinBlock(path, res);
    yield return PBar.Progress(50);

    int num = res.labels.Count;
    int step = 0;
    int start = lines.Count;

    foreach (CodeLabel l in res.labels) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(50 + 100 * step / num);
      RomLine line = Instantiate(LineTemplate, Container).GetComponent<RomLine>();

      l.name = HandleDuplicateNames(l.name, line);
      line.gameObject.name = l.name;
      line.gameObject.SetActive(true);
      line.Label.SetTextWithoutNotify(l.name);
      line.Type.text = (int)l.type + " " + l.type.ToString();
      line.ltype = l.type;
      lines.Add(line);
      line.Delete.onClick.AddListener(() => { Delete(line); });
      line.MoveUp.onClick.AddListener(() => { MoveUp(line); });
      line.MoveDown.onClick.AddListener(() => { MoveDown(line); });
      line.Label.onEndEdit.AddListener((name) => { UpdateName(line, name); });
      line.Check.onValueChanged.AddListener((check) => { SelectLine(line, check); });
    }
    step = 0;
    for (int i = 0; i < res.labels.Count - 1; i++) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(150 + 100 * step / num);
      int size = res.labels[i + 1].start - res.labels[i].start;
      lines[start + i].size = size;
      lines[start + i].Size.text = size.ToString();
    }
    lines[start + res.labels.Count - 1].size = res.block.Length - res.labels[res.labels.Count - 1].start;
    lines[start + res.labels.Count - 1].Size.text = lines[start + res.labels.Count - 1].size.ToString();

    step = 0;
    for (int i = 0; i < res.labels.Count; i++) {
      step++;
      if (step % 4 == 0) yield return PBar.Progress(250 + 100 * step / num);
      int size = lines[start + i].size;
      byte[] data = new byte[size];
      for (int j = 0; j < size; j++) {
        data[j] = res.block[res.labels[i].start + j];
      }
      lines[start + i].Data = data;
    }
    PBar.Hide();
  }

  public void Add() {
    FileBrowser.Load(PostAdd, FileBrowser.FileType.Rom);
  }

  public void PostAdd(string path) {
    StartCoroutine(PostLoading(path));
  }


  public void Save() {
    FileBrowser.Save(PostSave, FileBrowser.FileType.Rom);
  }

  public void PostSave(string path, string name) {
    StartCoroutine(PostSaving(path, name));
  }
  IEnumerator PostSaving(string path, string name) {
    yield return PBar.Show("Saving", 0, 2 + Container.childCount);
    int pos = 1;
    ByteChunk chunk = new ByteChunk();
    foreach (Transform t in Container) {
      yield return PBar.Progress(pos++);
      RomLine line = t.GetComponent<RomLine>();
      chunk.AddBlock(line.Label.text.Trim(), line.ltype, line.Data);
    }
    yield return PBar.Progress(pos++);
    ByteReader.SaveBinBlock(path, name, chunk);
    PBar.Hide();
  }

  RomLine toDelete = null;
  public void Delete(RomLine line) {
    toDelete = line;
    Confirm.Set("Confirm delete label\n" + line.Label.text, DeleteConfirmed);
  }

  public void DeleteConfirmed() {
    string name = toDelete.Label.text.ToLowerInvariant();
    if (names.ContainsKey(name)) names.Remove(name);
    name = NormLabel.Normalize(name);
    if (names.ContainsKey(name)) names.Remove(name);
    Destroy(toDelete.gameObject);
    lines.Remove(toDelete);
    toDelete = null;
  }

  public void MoveUp(RomLine line) {
    int pos = -1;
    for (int i = 0; i < lines.Count; i++) {
      if (lines[i] == line) {
        pos = i;
        break;
      }
    }
    if (pos < 1) return;
    RomLine tmp = lines[pos - 1];
    lines[pos - 1] = lines[pos];
    lines[pos] = tmp;
    for (int i = 0; i < lines.Count; i++) {
      lines[i].transform.SetSiblingIndex(i);
    }
  }
  public void MoveDown(RomLine line) {
    int pos = -1;
    for (int i = 0; i < lines.Count; i++) {
      if (lines[i] == line) {
        pos = i;
        break;
      }
    }
    if (pos > lines.Count - 2) return;
    RomLine tmp = lines[pos + 1];
    lines[pos + 1] = lines[pos];
    lines[pos] = tmp;
    for (int i = 0; i < lines.Count; i++) {
      lines[i].transform.SetSiblingIndex(i);
    }
  }

  public void UpdateName(RomLine line, string name) {
    foreach(string key in names.Keys)
      if (names[key] == line) {
        names.Remove(key);
        break;
      }
    name = HandleDuplicateNames(name, line);
    line.Label.SetTextWithoutNotify(name);
  }

  public void SelectLine(RomLine line, bool check) {
    if (!check || !Input.GetKey(KeyCode.LeftShift)) return;

    int start = -1;
    int end = -1;
    for (int i = 0; i < lines.Count; i++) {
      if (lines[i] == line) {
        end = i;
        if (start != -1) break;
      }
      else if (lines[i].Check.isOn) {
        start = i;
        if (end != -1) break;
      }
    }

    if (start == -1 || end == -1) return;
    if (start > end) {
      int tmp = start; start = end; end = tmp;
    }
    for (int i = start; i <= end; i++) {
      lines[i].Check.SetIsOnWithoutNotify(true);
    }
  }

  public void GlobalCheckChange() {
    foreach (Transform t in Container)
      t.GetComponent<RomLine>().Check.SetIsOnWithoutNotify(GlobalCheckmark.isOn);
  }

  public void RenameAllRows() {
    foreach (Transform t in Container) {
      RomLine line = t.GetComponent<RomLine>();
      if (!line.Check.isOn) continue;
      UpdateName(line, GlobalName.text);
    }
  }

  public void DeleteAllRows() {
    int num = 0;
    foreach (Transform t in Container)
      if (t.GetComponent<RomLine>().Check.isOn)
        num++;
    if (num > 0)
      Confirm.Set("Confirm to delete " + num + " items?", DeleteAllRowsConfirmed);
  }

  public void DeleteAllRowsConfirmed() {
    foreach (Transform t in Container) {
      RomLine line = t.GetComponent<RomLine>();
      if (!line.Check.isOn) continue;
      toDelete = line;
      DeleteConfirmed();
    }
  }

  public void MoveAllUp() {
    if (lines == null || lines.Count < 2 || lines[0].Check.isOn) return;
    for (int i = 0; i < lines.Count; i++) {
      if (lines[i].Check.isOn) {
        RomLine tmp = lines[i - 1];
        lines[i - 1] = lines[i];
        lines[i] = tmp;
      }
    }
    for (int i = 0; i < lines.Count; i++) {
      lines[i].transform.SetSiblingIndex(i);
    }
  }

  public void MoveAllDown() {
    if (lines == null || lines.Count < 2 || lines[lines.Count - 1].Check.isOn) return;
    for (int i = lines.Count - 1; i >= 0; i--) {
      if (lines[i].Check.isOn) {
        RomLine tmp = lines[i + 1];
        lines[i + 1] = lines[i];
        lines[i] = tmp;
      }
    }
    for (int i = 0; i < lines.Count; i++) {
      lines[i].transform.SetSiblingIndex(i);
    }
  }

  public void ChangeType() {
    foreach(RomLine l in lines) {
      if (l.Check.isOn) {
        l.Type.text = ItemType.value + " " + ((LabelType)ItemType.value).ToString();
        l.ltype = (LabelType)ItemType.value;
      }
    }
  }

  public void EditLine() {
    foreach(RomLine line in lines) {
      if (line.Check.isOn) {
        switch (line.ltype) {
          case LabelType.Sprite:
            Dev.inst.SpriteEditor();
            spriteEditor.ImportFrom(line.Data);
            gameObject.SetActive(false);
            break;

          case LabelType.Wave:
            Dev.inst.WaveformEditor();
            waveformpEditor.Import(line.Data);
            break;

          case LabelType.Tilemap:
            break;
          case LabelType.Music:
            break;
        }
        break;
      }
    }
  }

  internal void UpdateLine(byte[] data, LabelType lt) {
    Dev.inst.RomEditor();
    foreach (RomLine line in lines) {
      if (line.Check.isOn && line.ltype == lt) {
        line.Data = data;
        line.size = data.Length;
        line.Size.text = data.Length.ToString();
      }
      break;
    }
  }
}

