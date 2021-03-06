﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileInMap : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
  public byte id;
  public byte x;
  public byte y;
  public byte rot;
  public Image border;
  public RawImage img;
  Color32 Normal;
  Color32 Over = new Color32(255, 180, 25, 255);
  public System.Action<TileInMap> CallBack;
  public System.Action<TileInMap, bool> OverCallBack;
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
    OverCallBack?.Invoke(this, true);
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (selected)
      border.color = Selected;
    else
      border.color = Normal;
    OverCallBack?.Invoke(this, false);
  }

  public void Deselect() {
    selected = false;
    border.color = Normal;
  }

  public void Select() {
    border.color = Over;
  }

  internal void Setup(System.Action<TileInMap> selectTileInMap, System.Action<TileInMap, bool> overTileInMap, Texture2D emptyTexture) {
    id = 0;
    CallBack = selectTileInMap;
    OverCallBack = overTileInMap;
    img.texture = emptyTexture;
    gameObject.SetActive(true);
    transform.localScale = new Vector3(1, 1, 1);
  }
}
