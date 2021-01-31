using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RomEditor : MonoBehaviour {
  public GameObject LineTemplate;
  public Transform Container;
  List<RomLine> lines = new List<RomLine>();
  public Confirm Confirm;

  public void Load() {
    FileBrowser.Load(PostLoad, FileBrowser.FileType.Rom);
  }

  public void PostLoad(string path) {
    foreach (Transform t in Container)
      Destroy(t.gameObject);
    lines.Clear();
    StartCoroutine(PostLoading(path));
  }

  IEnumerator PostLoading(string path) {
    yield return PBar.Show("Loading", 25, 350);
    ByteChunk res = new ByteChunk();
    ByteReader.ReadBinBlock(path, res);
    yield return PBar.Progress(50);

    int num = res.labels.Count;
    int step = 0;

    foreach (CodeLabel l in res.labels) {
      step++;
      if (step % 3 == 0) yield return PBar.Progress(50 + 100 * step / num);
      RomLine line = Instantiate(LineTemplate, Container).GetComponent<RomLine>();
      line.gameObject.name = l.name;
      line.gameObject.SetActive(true);
      line.Label.SetTextWithoutNotify(l.name);
      lines.Add(line);
      line.Delete.onClick.AddListener(() => { Delete(line); });
      line.MoveUp.onClick.AddListener(() => { MoveUp(line); });
      line.MoveDown.onClick.AddListener(() => { MoveDown(line); });
    }
    int pos = 0;
    step = 0;
    for (int i = 0; i < res.labels.Count - 1; i++) {
      step++;
      if (step % 3 == 0) yield return PBar.Progress(150 + 100 * step / num);
      int size = res.labels[i + 1].start - res.labels[i].start;
      pos += size;
      lines[i].size = size;
      lines[i].Size.text = size.ToString();
    }
    lines[res.labels.Count - 1].size = pos - res.labels[res.labels.Count - 1].start;
    lines[res.labels.Count - 1].Size.text = lines[res.labels.Count - 1].size.ToString();

    step = 0;
    for (int i = 0; i < res.labels.Count; i++) {
      step++;
      if (step % 3 == 0) yield return PBar.Progress(250 + 100 * step / num);
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
  }

  RomLine toDelete = null;
  public void Delete(RomLine line) {
    toDelete = line;
    Confirm.Set("Confirm delete label\n" + line.Label.text, DeleteConfirmed);
  }

  public void DeleteConfirmed() {
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
}

