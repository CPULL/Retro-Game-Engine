using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class RomEditor : MonoBehaviour {
  public GameObject LineTemplate;
  public Transform Container;
  readonly List<RomLine> lines = new List<RomLine>();
  readonly Dictionary<string, RomLine> names = new Dictionary<string, RomLine>();
  public Confirm Confirm;

  readonly Regex rgNumPart = new Regex("([^0-9]*([0-9]*))+", RegexOptions.IgnoreCase, System.TimeSpan.FromSeconds(1));


  string HandleDuplicateNames(string name, RomLine line) {
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
      lines.Add(line);
      line.Delete.onClick.AddListener(() => { Delete(line); });
      line.MoveUp.onClick.AddListener(() => { MoveUp(line); });
      line.MoveDown.onClick.AddListener(() => { MoveDown(line); });
      line.Label.onEndEdit.AddListener((name) => { UpdateName(line, name); });
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
      int size = lines[i].size;
      byte[] data = new byte[size];
      for (int j = 0; j < size; j++) {
        data[j] = res.block[res.labels[i].start + j];
      }
      lines[i].Data = data;
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
      chunk.AddBlock(line.Label.text.Trim(), line.Data);
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
    Destroy(toDelete.gameObject);
    lines.Remove(toDelete);
    string name = toDelete.Label.text.Trim();
    if (names.ContainsKey(name)) names.Remove(name);
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
}

