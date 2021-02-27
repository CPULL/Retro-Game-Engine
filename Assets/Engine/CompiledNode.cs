using System.Collections.Generic;

[System.Serializable]
public class CompiledNode {
  public BNF type;
  public List<CompiledNode> children;
  public int iVal;
  public float fVal;
  public string sVal;
  public byte[] bVal = null;
  public int Reg;
  public int origLineNum;
  public CodeNode.NumFormat format = CodeNode.NumFormat.Dec;
  public string comment;
  public CodeNode.CommentType commentType = CodeNode.CommentType.None;


  internal CompiledNode CN1 { get { return children?[0]; } }
  internal CompiledNode CN2 { get { return children != null && children.Count > 1 ? children[1] : null; } }
  internal CompiledNode CN3 { get { return children != null && children.Count > 2 ? children[2] : null; } }
  internal CompiledNode CN4 { get { return children != null && children.Count > 3 ? children[3] : null; } }
  internal CompiledNode CN5 { get { return children != null && children.Count > 4 ? children[4] : null; } }
  internal CompiledNode CN6 { get { return children != null && children.Count > 5 ? children[5] : null; } }
  internal CompiledNode CN7 { get { return children != null && children.Count > 6 ? children[6] : null; } }
  internal CompiledNode CN8 { get { return children != null && children.Count > 7 ? children[7] : null; } }

  public CompiledNode(CodeNode node) {
    type = node.type;
    iVal = node.iVal;
    fVal = node.fVal;
    sVal = node.sVal;
    Reg = node.Reg;
    origLineNum = node.origLineNum;
    format = node.format;
    comment = node.comment;
    commentType = node.commentType;
    bVal = node.bVal;
    if (node.children == null) children = null;
    else {
      foreach (CodeNode cn in node.children)
        children.Add(new CompiledNode(cn));
    }
  }


  internal void Add(CompiledNode node) {
    if (children == null) children = new List<CompiledNode>();
    children.Add(node);
  }


  internal string Format(bool hadOpenBlock) {
    if (string.IsNullOrEmpty(comment) && commentType == CodeNode.CommentType.MultiLineClose) {
      return comment + Format() + (hadOpenBlock ? "{" : "");
    }
    if (string.IsNullOrEmpty(comment) || commentType == CodeNode.CommentType.None) return Format() + (hadOpenBlock ? "{" : "");
    if (commentType == CodeNode.CommentType.MultiLineInner || type == BNF.ERROR) {
      return comment;
    }
    return Format() + (hadOpenBlock ? "{" : "") + " " + comment;
  }

  internal string Format() {
      switch (type) {
        case BNF.Program: return "Name: " + sVal;
        case BNF.Start: return "Start {";
        case BNF.Update: return "Update {";
        case BNF.Config: return "Cofig {";
        case BNF.Data: return "Data {";
        case BNF.Functions: return "Functions: (" + children.Count + ")";
        case BNF.FunctionDef: return "#" + sVal + CN1?.Format() + " {";
        case BNF.FunctionCall: return sVal + CN1?.Format();
        case BNF.RETURN: {
          if (CN1 == null) return "return";
          else return "return " + CN1?.Format();
        }
        case BNF.Params: {
          string res = "(";
          if (CN1 != null) res += CN1.Format();
          if (children != null) for (int i = 1; i < children.Count; i++) {
            if (children[i] != null) res += ", " + children[i].Format();
          }
          return res + ")";
        }
        case BNF.PaletteConfig: return "UsePalette(" + (iVal == 0 ? "0" : "1") + ")";
        case BNF.Ram:
          return "ram" +
            (iVal < 1024 ? iVal.ToString() : (
            iVal < 1024 * 1024 ? (((int)(10 * iVal / 1024f)) / 10f) + "k" :
            (((int)(10 * iVal / (1024 * 1024f))) / 10f) + "m")) +
            ")";
        case BNF.Rom: // FIXME in Data block
          break;
        case BNF.Label: return sVal + ":";
        case BNF.REG: return "REG" + Reg;
        case BNF.ARRAY: return "REG" + Reg + "[" + CN1?.Format() + "]";
        case BNF.INT: {
          if (format == CodeNode.NumFormat.Hex) return "0x" + System.Convert.ToString(iVal, 16);
          if (format == CodeNode.NumFormat.Bin) return "0b" + System.Convert.ToString(iVal, 2);
          return iVal.ToString();
        }
        case BNF.FLT: return fVal.ToString();
        case BNF.COLOR: return Col.GetColorString(iVal) + "c";
        case BNF.PAL: return iVal + "p";
        case BNF.LUMA: return "Luma(" + CN1?.Format() + ")";
        case BNF.CONTRAST: return "Contrast(" + CN1?.Format() + ")";
        case BNF.STR: return "\"" + sVal + "\"";
        case BNF.MEM: return "[" + CN1?.Format() + "]";
        case BNF.MEMlong: return "[" + CN1?.Format() + "]@";
        case BNF.MEMlongb: return "[" + CN1?.Format() + "]b";
        case BNF.MEMlongi: return "[" + CN1?.Format() + "]i";
        case BNF.MEMlongf: return "[" + CN1?.Format() + "]f";
        case BNF.MEMlongs: return "[" + CN1?.Format() + "]s";
        case BNF.MEMchar: return "[" + CN1?.Format() + "]c";

        case BNF.OPpar: return "(" + CN1?.Format() + ")";
        case BNF.OPsum: return CN1?.Format() + " + " + CN2?.Format();
        case BNF.OPsub: return CN1?.Format() + " - " + CN2?.Format();
        case BNF.OPmul: return CN1?.Format() + " * " + CN2?.Format();
        case BNF.OPdiv: return CN1?.Format() + " / " + CN2?.Format();
        case BNF.OPmod: return CN1?.Format() + " % " + CN2?.Format();
        case BNF.OPand: return CN1?.Format() + " & " + CN2?.Format();
        case BNF.OPor: return CN1?.Format() + " | " + CN2?.Format();
        case BNF.OPxor: return CN1?.Format() + " ^ " + CN2?.Format();
        case BNF.OPlsh: return CN1?.Format() + " << " + CN2?.Format();
        case BNF.OPrsh: return CN1?.Format() + " >> " + CN2?.Format();
        case BNF.LABG: return "Label(" + CN1?.Format() + ")";
        case BNF.CASTb: return CN1?.Format() + "_b";
        case BNF.CASTi: return CN1?.Format() + "_i";
        case BNF.CASTf: return CN1?.Format() + "_f";
        case BNF.CASTs: return CN1?.Format() + "_s";
        case BNF.UOneg: return "!" + CN1?.Format();
        case BNF.UOinv: return "~" + CN1?.Format();
        case BNF.UOsub: return "-" + CN1?.Format();
        case BNF.COMPeq: return CN1?.Format() + " == " + CN2?.Format();
        case BNF.COMPne: return CN1?.Format() + " != " + CN2?.Format();
        case BNF.COMPlt: return CN1?.Format() + " < " + CN2?.Format();
        case BNF.COMPle: return CN1?.Format() + " <= " + CN2?.Format();
        case BNF.COMPgt: return CN1?.Format() + " > " + CN2?.Format();
        case BNF.COMPge: return CN1?.Format() + " >= " + CN2?.Format();
        case BNF.ASSIGN: return CN1?.Format() + " = " + CN2?.Format();
        case BNF.ASSIGNsum: return CN1?.Format() + " += " + CN2?.Format();
        case BNF.ASSIGNsub: return CN1?.Format() + " -= " + CN2?.Format();
        case BNF.ASSIGNmul: return CN1?.Format() + " *= " + CN2?.Format();
        case BNF.ASSIGNdiv: return CN1?.Format() + " /= " + CN2?.Format();
        case BNF.ASSIGNmod: return CN1?.Format() + " %= " + CN2?.Format();
        case BNF.ASSIGNand: return CN1?.Format() + " &= " + CN2?.Format();
        case BNF.ASSIGNor: return CN1?.Format() + " |= " + CN2?.Format();
        case BNF.ASSIGNxor: return CN1?.Format() + " ^= " + CN2?.Format();
        case BNF.IncCmd: return CN1?.Format() + "++";
        case BNF.IncExp: return CN1?.Format() + "++";
        case BNF.DecCmd: return CN1?.Format() + "--";
        case BNF.DecExp: return CN1?.Format() + "--";
        case BNF.BLOCK: {
          if (CN1 == null) return "{}";
          if (CN2 == null) return CN1.Format();
          return "{" + CN1?.Format() + ", ...}"; // FIXME
        }

        case BNF.WHILE: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "while (" + CN1?.Format() + ") {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "while (" + CN1?.Format() + ") " + CN2?.CN1?.Format();
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "while (" + CN1?.Format() + ")";
        }
        case BNF.FOR: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "for (" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3?.Format() + ") {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "for (" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3?.Format() + ") " + CN4?.CN1?.Format();
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "for (" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3?.Format() + ")";
        }
        case BNF.IF: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "if (" + CN1?.Format() + ") {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "if (" + CN1?.Format() + ") " + CN2?.CN1?.Format();
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "if (" + CN1?.Format() + ")";
        }
        case BNF.ELSE: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "else {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "else " + CN1?.CN1?.Format();
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "else";
        }

        case BNF.CLR: return "Clr(" + CN1?.Format() + ")";
        case BNF.WRITE: { // Write(string txt, int x, int y, byte col, byte back = 255, byte mode = 0)
          if (children.Count == 4)
            return "Write(" +
              CN1?.Format() + ", " + CN2?.Format() + ", " +
              CN3?.Format() + ", " + CN4?.Format() + ")";
          else if (children.Count == 5)
            return "Write(" +
              CN1?.Format() + ", " + CN2?.Format() + ", " +
              CN3?.Format() + ", " + CN4?.Format() + ", " + CN5.Format() + ")";
          else
            return "Write(" +
              CN1?.Format() + ", " + CN2?.Format() + ", " +
              CN3?.Format() + ", " + CN4?.Format() + ", " +
              CN5?.Format() + ", " + CN6?.Format() + ")";
        }
        case BNF.WAIT: return "Wait(" + CN1?.Format() + ")";
        case BNF.DESTROY: return "Destroy(" + CN1?.Format() + ")";
        case BNF.SCREEN: {
          if (CN3 == null)
            return "Screen(" + CN1?.Format() + ", " + CN2?.Format() + ")";
          else
            return "Screen(" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3.Format() + ")";
        }
        case BNF.SPRITE: {
          if (CN3 == null)
            return "Sprite(" + CN1?.Format() + ", " + CN2?.Format() + ")";
          else
            return "Sprite(" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3.Format() + ")";
        }
        case BNF.SPEN: return "SpEn(" + CN1?.Format() + ", " + CN2.Format() + ")";
        case BNF.SPOS: {
          if (CN4 == null)
            return "SPos(" + CN1?.Format() + ", " + CN2.Format() + ", " + CN3?.Format() + ")";
          else
            return "SPos(" + CN1?.Format() + ", " + CN2.Format() + ", " + CN3?.Format() + ", " + CN4?.Format() + ")";
        }
        case BNF.SROT: return "SRot(" + CN1?.Format() + ", " + CN2.Format() + ", " + CN3?.Format() + ")";
        case BNF.SPRI: return "SPri(" + CN1?.Format() + ", " + CN2.Format() + ")";
        case BNF.STINT: return "STint(" + CN1?.Format() + ", " + CN2.Format() + ")";
        case BNF.SSCALE: return "SScale(" + CN1?.Format() + ", " + CN2.Format() + ", " + CN3?.Format() + ")";
        case BNF.SETP: return "SetP(" + CN1?.Format() + ", " + CN2.Format() + ", " + CN3.Format() + ")";
        case BNF.GETP: return "GetP(" + CN1?.Format() + ", " + CN2.Format() + ")";
        case BNF.LINE: {
          return "Line(" +
            CN1?.Format() + ", " + CN2?.Format() + ", " +
            CN3?.Format() + ", " + CN4?.Format() + ", " + CN5?.Format() + ")";
        }
        case BNF.BOX: {
          if (children.Count == 5)
            return "Box(" +
              CN1?.Format() + ", " + CN2?.Format() + ", " +
              CN3?.Format() + ", " + CN4?.Format() + ", " + CN5?.Format() + ")";
          else
            return "Box(" +
              CN1?.Format() + ", " + CN2?.Format() + ", " +
              CN3?.Format() + ", " + CN4?.Format() + ", " +
              CN5?.Format() + ", " + CN6?.Format() + ")";
        }
        case BNF.CIRCLE: {
          if (children.Count == 5)
            return "Circle(" +
              CN1?.Format() + ", " + CN2?.Format() + ", " +
              CN3?.Format() + ", " + CN4?.Format() + ", " + CN5?.Format() + ")";
          else
            return "Circle(" +
              CN1?.Format() + ", " + CN2?.Format() + ", " +
              CN3?.Format() + ", " + CN4?.Format() + ", " +
              CN5?.Format() + ", " + CN6?.Format() + ")";
        }
        case BNF.IMAGE: {
          if (children.Count < 6)
            return "Image(" +
            CN1?.Format() + ", " + CN2?.Format() + ", " +
            CN3?.Format() + ", " + CN4?.Format() + ", " +
            CN5?.Format() + ")";
          else
            return "Image(" +
              CN1?.Format() + ", " + CN2?.Format() + ", " +
              CN3?.Format() + ", " + CN4?.Format() + ", " +
              CN5?.Format() + ", " + CN6?.Format() + ", " + CN7?.Format() + ")";
        }
        case BNF.FRAME: return "Frame";
        case BNF.DTIME: return "deltatime";
        case BNF.LEN: return CN1?.Format() + ".Len";
        case BNF.PLEN: return CN1?.Format() + ".PLen";
        case BNF.SUBSTRING: {
          if (CN3 == null)
            return CN1?.Format() + ".Substring(" + CN2?.Format() + ")";
          else
            return CN1?.Format() + ".Substring(" + CN2?.Format() + ", " + CN3?.Format() + ")";
        }
        case BNF.TRIM: return CN1?.Format() + ".Trim";

        case BNF.KEY: return "key" + "LLLRRRUUUDDDAAABBBCCCFFFEEE"[iVal] + (iVal % 3 == 1 ? "u" : (iVal % 3 == 2 ? "d" : "")) + "";
        case BNF.KEYx: return "keyX";
        case BNF.KEYy: return "keyY";

        case BNF.SIN: return "Sin(" + CN1?.Format() + ")";
        case BNF.COS: return "Cos(" + CN1?.Format() + ")";
        case BNF.TAN: return "Tan(" + CN1?.Format() + ")";
        case BNF.ATAN2: return "Atan2(" + CN1?.Format() + ")";
        case BNF.SQR: return "Sqrt(" + CN1?.Format() + ")";
        case BNF.POW: return "Pow(" + CN1?.Format() + ")";
        case BNF.MEMCPY: return "MemCpy(" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3?.Format() + ")";
        case BNF.SOUND: {
          if (CN3 == null)
            return "Sound(" + CN1?.Format() + ", " + CN2?.Format() + ")";
          else
            return "Sound(" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3?.Format() + ")";
        }
        case BNF.WAVE: {
          if (CN5 == null)
            return "Sound(" + CN1?.Format() + ", " + CN2?.Format() +
              ", " + CN3?.Format() + ", " + CN4?.Format() +
              ")";
          else
            return "Sound(" + CN1?.Format() + ", " + CN2?.Format() +
              ", " + CN3?.Format() + ", " + CN4?.Format() + ", " + CN5?.Format() +
              ")";
        }
        case BNF.MUTE: return "Mute(" + CN1?.Format() + ")";
        case BNF.VOLUME: {
          if (CN2 == null) return "Volume(" + CN1?.Format() + ")";
          else return "Volume(" + CN1?.Format() + ", " + CN2?.Format() + ")";
        }
        case BNF.PITCH: return "Pitch(" + CN1?.Format() + ", " + CN2?.Format() + ")";
        case BNF.PAN: return "Pan(" + CN1?.Format() + ", " + CN2?.Format() + ")";
        case BNF.MUSICLOAD: return "LoadMusic(" + CN1?.Format() + ")";
        case BNF.MUSICPLAY: return "PlayMusic(" + CN1?.Format() + ")";
        case BNF.MUSICSTOP: return "StopMusic()";
        case BNF.MUSICPOS: return "MusicPos()";
        case BNF.MUSICVOICES: {
          string res = "MusicVoices(" + CN1?.Format();
          if (CN2 != null) res += ", " + CN2?.Format();
          if (CN3 != null) res += ", " + CN3?.Format();
          if (CN4 != null) res += ", " + CN4?.Format();
          if (CN5 != null) res += ", " + CN5?.Format();
          if (CN6 != null) res += ", " + CN6?.Format();
          if (CN7 != null) res += ", " + CN7?.Format();
          if (CN8 != null) res += ", " + CN8?.Format();
          return res + ")";
        }
        case BNF.TILEMAP: return "TileMap(" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3?.Format() + ")";
        case BNF.TILEPOS: {
          string res = "TilePos(" + CN1?.Format() + ", " + CN2?.Format() + ", " + CN3?.Format();
          if (CN4 != null) res += ", " + CN4?.Format();
          if (CN5 != null) res += ", " + CN5?.Format();
          return res + ")";
        }
        case BNF.TILESET: {
          string res = "TileSet(" + CN1?.Format() + ", " + CN2?.Format() +
          ", " + CN3?.Format() + ", " + CN4?.Format();
          if (CN5 != null) res += ", " + CN5?.Format();
          return res + ")";
        }
        case BNF.TILEGET:
          return "TileGet(" + CN1?.Format() + ", " + CN2?.Format() +
                  ", " + CN3?.Format() + ")";
        case BNF.TILEGETROT:
          return "TileGetRot(" + CN1?.Format() + ", " + CN2?.Format() +
              ", " + CN3?.Format() + ")";
        case BNF.NOP: return "";
        case BNF.USEPALETTE: return "UsePalette(" + CN1?.Format() + ")";
        case BNF.SETPALETTECOLOR:
          return "SetPalette(" + CN1?.Format() + ", " +
            CN2?.Format() + ", " + CN3?.Format() + ", " +
            CN4?.Format() + ", " + CN5?.Format() + ")";
        case BNF.ERROR: return sVal;
      }


    throw new System.Exception(type + " NOT YET DONE!");
  }

}

