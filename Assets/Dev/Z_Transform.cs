using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class Z_Transform : MonoBehaviour {
}


[CustomEditor(typeof(Z_Transform))]
public class Z_TransformEditor : Editor {
  public override void OnInspectorGUI() {
    GameObject t = (target as Z_Transform).gameObject;
    if (GUILayout.Button("Transform to TMPro")) {
      Text txt = t.GetComponent<Text>();
      if (txt == null) {
        Debug.Log("No TEXT found :(");
        return;
      }

      // text
      string text = txt.text;
      // size
      int size = txt.fontSize;
      // alignment
      VerticalAlignmentOptions alignv = VerticalAlignmentOptions.Middle;
      HorizontalAlignmentOptions alignh = HorizontalAlignmentOptions.Right;
      switch (txt.alignment) {
        case TextAnchor.UpperLeft:   alignh = HorizontalAlignmentOptions.Left; alignv = VerticalAlignmentOptions.Top; break;
        case TextAnchor.UpperCenter: alignh = HorizontalAlignmentOptions.Center; alignv = VerticalAlignmentOptions.Top; break;
        case TextAnchor.UpperRight:  alignh = HorizontalAlignmentOptions.Right; alignv = VerticalAlignmentOptions.Top; break;
        case TextAnchor.MiddleLeft: alignh = HorizontalAlignmentOptions.Left; alignv = VerticalAlignmentOptions.Middle; break;
        case TextAnchor.MiddleCenter: alignh = HorizontalAlignmentOptions.Center; alignv = VerticalAlignmentOptions.Middle; break;
        case TextAnchor.MiddleRight: alignh = HorizontalAlignmentOptions.Right; alignv = VerticalAlignmentOptions.Middle; break;
        case TextAnchor.LowerLeft:   alignh = HorizontalAlignmentOptions.Left; alignv = VerticalAlignmentOptions.Bottom; break;
        case TextAnchor.LowerCenter: alignh = HorizontalAlignmentOptions.Center; alignv = VerticalAlignmentOptions.Bottom; break;
        case TextAnchor.LowerRight: alignh = HorizontalAlignmentOptions.Right; alignv = VerticalAlignmentOptions.Bottom; break;
      }

      // bold, italic
      FontStyles style = FontStyles.Normal;
      switch (txt.fontStyle) {
        case FontStyle.Normal: style = FontStyles.Normal; break;
        case FontStyle.Bold: style = FontStyles.Bold; break;
        case FontStyle.Italic: style = FontStyles.Italic; break;
        case FontStyle.BoldAndItalic: style = FontStyles.Bold; break;
      }
      // color
      Color32 color = txt.color;
      // wrapping
      bool wrap = txt.horizontalOverflow == HorizontalWrapMode.Wrap;
      // raycast tgt
      bool rct = txt.raycastTarget;
      
      DestroyImmediate(txt);

      TextMeshProUGUI tm = t.AddComponent<TextMeshProUGUI>();
      tm.text = text;
      tm.fontSize = size;
      tm.verticalAlignment = alignv;
      tm.horizontalAlignment = alignh;
      tm.fontStyle = style;
      tm.color = color;
      tm.enableWordWrapping = wrap;
      tm.raycastTarget = rct;

      DestroyImmediate(t.GetComponent<Z_Transform>());
    }
  }
}