[System.Serializable]
public class CodeLabel {
  public string name;
  public int start;
  public LabelType type;
}

public enum LabelType {
  RawData = 0,
  Config = 1,
  CodeStart = 2,
  CodeUpdate = 3,
  CodeFunction = 4,

  Image=5,
  Sprite=6,
  Palette = 7,
  Tilemap = 8,
  Tile=9,
  Wave=10,
  Music=11,
  MusicBlock=12,
  Font=13,
}
