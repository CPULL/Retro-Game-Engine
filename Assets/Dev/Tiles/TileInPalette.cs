﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileInPalette : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public byte id;
  public Image border;
  public RawImage img;
  public int tw, th;
  Color32 Normal;
  Color32 Over = new Color32(255, 180, 25, 255);
  Color32 FullWhite = new Color32(255, 255, 255, 255);
  public byte[] rawData;
  public System.Action<TileInPalette> CallBack;
  Color32 Selected = new Color32(255, 0, 0, 255);
  bool selected = false;

  void Start() {
    Normal = border.color;
  }

  public void OnPointerClick(PointerEventData eventData) {
    CallBack?.Invoke(this);
    selected = true;
    border.color = Selected;
  }

  public void OnPointerEnter(PointerEventData eventData) {
    border.color = Over;
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (selected)
      border.color = Selected;
    else
      border.color = Normal;
  }

  public void Deselect() {
    selected = false;
    border.color = Normal;
  }

  internal void Setup(byte v, System.Action<TileInPalette> callBack, int w, int h) {
    id = v;
    tw = w;
    th = h;
    CallBack = callBack;
    rawData = new byte[w * h];
    Texture2D texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
    for (int x = 0; x < w; x++)
      for (int y = 0; y < h; y++) {
        rawData[x + w * y] = 63;
        texture.SetPixel(x, y, FullWhite);
      }
    texture.Apply();
    img.texture = texture;
  }

  internal void UpdateSize(int w, int h) {
    byte[] newData = new byte[w * h];
    Texture2D newTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
    Texture2D oldt = (Texture2D)img.texture;
    for (int x = 0; x < w; x++)
      for (int y = 0; y < h; y++) {
        if (x >= w || y >= h) continue;
        if (x >= tw || y >= th) {
          newData[x + w * y] = 63;
          newTexture.SetPixel(x, y, FullWhite);
        }
        else {
          newData[x + w * y] = rawData[w + tw * y];
          newTexture.SetPixel(x, y, oldt.GetPixel(x, y));
        }
      }
    newTexture.Apply();
    img.texture = newTexture;
    rawData = newData;
    Destroy(oldt);
    tw = w;
    th = h;
  }
}
