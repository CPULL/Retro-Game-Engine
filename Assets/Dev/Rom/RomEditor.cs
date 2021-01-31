using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RomEditor : MonoBehaviour {
  public GameObject LineTemplate;
  public Transform Container;
  List<RomLine> lines = new List<RomLine>();

  public void Load() {
    FileBrowser.Load(PostLoad, FileBrowser.FileType.Rom);
  }

  public void PostLoad(string path) {
    StartCoroutine(PostLoading(path));
  }

  IEnumerator PostLoading(string path) {
    yield return PBar.Show("Loading", 0, 350);
    foreach (Transform t in Container)
      Destroy(t.gameObject);
    lines.Clear();

    yield return PBar.Progress(25);
    ByteChunk res = new ByteChunk();
    ByteReader.ReadBinBlock(path, res);
    yield return PBar.Progress(50);

    int num = res.labels.Count;
    int step = 0;

    foreach (CodeLabel l in res.labels) {
      yield return PBar.Progress(50 + 100 * step++ / num);
      RomLine line = Instantiate(LineTemplate, Container).GetComponent<RomLine>();
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
      yield return PBar.Progress(150 + 100 * step++ / num);
      int size = res.labels[i + 1].start - res.labels[i].start;
      pos += size;
      lines[i].size = size;
      lines[i].Size.text = size.ToString();
    }
    lines[res.labels.Count - 1].size = pos - res.labels[res.labels.Count - 1].start;
    lines[res.labels.Count - 1].Size.text = lines[res.labels.Count - 1].size.ToString();

    step = 0;
    for (int i = 0; i < res.labels.Count; i++) {
      yield return PBar.Progress(250 + 100 * step++ / num);
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
  }

  public void Save() {
  }

  public void Delete(RomLine line) {

  }

  public void MoveUp(RomLine line) {

  }
  public void MoveDown(RomLine line) {

  }
}

