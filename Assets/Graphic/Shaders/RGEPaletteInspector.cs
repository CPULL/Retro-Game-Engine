using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RGEPaletteInspector : MonoBehaviour {
  public Material Material;
  public List<Color> Colors = new List<Color>(256);
}

#if UNITY_EDITOR
[CustomEditor(typeof(RGEPaletteInspector))]
public class DbgE : Editor {
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();
    RGEPaletteInspector myself = (RGEPaletteInspector)target;
    if (GUILayout.Button("Apply colors")) {
      myself.Material.SetColorArray("_Colors", myself.Colors);
    }
    if (GUILayout.Button("Read colors")) {
      myself.Material.GetColorArray("_Colors", myself.Colors);
    }
    if (GUILayout.Button("Apply default colors")) {
      for (int i = 0; i < 256; i++) {
        myself.Colors[i] = Col.GetColor((byte)i);
      }
      myself.Material.SetColorArray("_Colors", myself.Colors);
    }
  }
}
#endif