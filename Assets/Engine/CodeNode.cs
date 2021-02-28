using System.Collections.Generic;

public class CodeNode {
  public BNF type;
  public List<CodeNode> children;
  public string id;
  public int iVal;
  public float fVal;
  public string sVal;
  public byte[] bVal = null;
  public int Reg;
  public string origLine;
  public int origLineNum;
  public CodeNode parent;
  public NumFormat format = NumFormat.Dec;
  public string comment;
  public CommentType commentType = CommentType.None;

  public enum NumFormat {  Dec, Hex, Bin };
  public enum CommentType {  None, SingleLine, MultiLineFull, MultiLineOpen, MultiLineInner, MultiLineClose };

  internal CodeNode CN1 { get { return children?[0]; } }
  internal CodeNode CN2 { get { return children != null && children.Count > 1 ? children[1] : null; } }
  internal CodeNode CN3 { get { return children != null && children.Count > 2 ? children[2] : null; } }
  internal CodeNode CN4 { get { return children != null && children.Count > 3 ? children[3] : null; } }
  internal CodeNode CN5 { get { return children != null && children.Count > 4 ? children[4] : null; } }
  internal CodeNode CN6 { get { return children != null && children.Count > 5 ? children[5] : null; } }
  internal CodeNode CN7 { get { return children != null && children.Count > 6 ? children[6] : null; } }
  internal CodeNode CN8 { get { return children != null && children.Count > 7 ? children[7] : null; } }

  public CodeNode(BNF bnf, string line, int linenum) {
    type = bnf;
    origLine = line;
    origLineNum = linenum + 1;
  }

  public CodeNode(BNF bnf, string v, string line, int linenum) {
    type = bnf;
    id = v;
    origLine = line;
    origLineNum = linenum + 1;
  }

  public CodeNode(CodeNode block, CodeNode increment) {
    type = BNF.BLOCK;
    origLine = block.origLine;
    origLineNum = block.origLineNum;
    children = new List<CodeNode>(block.children) {
      increment
    };
  }

  internal void Add(CodeNode node) {
    if (children == null) children = new List<CodeNode>();
    children.Add(node);
    node.parent = this;
  }

  internal bool HasNode(BNF bnf) {
    if (children == null) return false;
    foreach (CodeNode n in children)
      if (n.type == bnf) return true;
    return false;
  }

  internal CodeNode Get(BNF bnf) {
    if (children == null) return null;
    foreach (CodeNode n in children)
      if (n.type == bnf) return n;
    return null;
  }

  public override string ToString() {
    string res = "";

    try {
      switch (type) {
        case BNF.Program: {
          res = "Program:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString() + "\n";
        }
        break;
        case BNF.Start: {
          res = "Start:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString() + "\n";
        }
        break;
        case BNF.Update: {
          res = "Update:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString() + "\n";
        }
        break;
        case BNF.Functions: {
          res = "Functions:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString() + "\n";
        }
        break;
        case BNF.Config: {
          res = "Config:\n";
          int num = children == null ? 0 : children.Count;
          for (int i = 0; i < num; i++) {
            res += children[i].ToString();
          }
        }
        break;
        case BNF.Data: {
          res = "Data:\n";
          int num = children == null ? 0 : children.Count;
          for (int i = 0; i < num; i++) {
            res += children[i].ToString();
          }
        }
        break;
        case BNF.REG: res += id + " R" + Reg; break;
        case BNF.INT: res += id + " " + iVal; break;
        case BNF.COLOR: res += id + Col.GetColorString(iVal) + "c"; break;
        case BNF.PAL: res += id + iVal + "p"; break;
        case BNF.FLT: res += id + " " + fVal; break;
        case BNF.STR: res += id + " \"" + sVal + "\""; break;
        case BNF.MEM: res += id + " [" + CN1?.ToString() + "]"; break;
        case BNF.MEMlong: res += id + " [" + CN1?.ToString() + "@]"; break;
        case BNF.MEMlongb: res += id + " [" + CN1?.ToString() + "@b]"; break;
        case BNF.MEMlongi: res += id + " [" + CN1?.ToString() + "@i]"; break;
        case BNF.MEMlongf: res += id + " [" + CN1?.ToString() + "@f]"; break;
        case BNF.MEMlongs: res += id + " [" + CN1?.ToString() + "@s]"; break;
        case BNF.MEMchar: res += id + " [" + CN1?.ToString() + "@c]"; break;

        case BNF.ARRAY: res += id + " R" + Reg + "[" + CN1?.ToString() + "]"; break;

        case BNF.OPpar:
          res += "(" + CN1?.ToString() + ")";
          break;
        case BNF.OPsum: res += "(" + CN1?.ToString() + "+" + CN2?.ToString() + ")"; break;
        case BNF.OPsub: res += "(" + CN1?.ToString() + "-" + CN2?.ToString() + ")"; break;
        case BNF.OPmul: res += "(" + CN1?.ToString() + "*" + CN2?.ToString() + ")"; break;
        case BNF.OPdiv: res += "(" + CN1?.ToString() + "/" + CN2?.ToString() + ")"; break;
        case BNF.OPmod: res += "(" + CN1?.ToString() + "%" + CN2?.ToString() + ")"; break;
        case BNF.OPand: res += "(" + CN1?.ToString() + "&" + CN2?.ToString() + ")"; break;
        case BNF.OPor: res += "(" + CN1?.ToString() + "|" + CN2?.ToString() + ")"; break;
        case BNF.OPxor: res += "(" + CN1?.ToString() + "^" + CN2?.ToString() + ")"; break;
        case BNF.OPlsh: res += "(" + CN1?.ToString() + "<<" + CN2?.ToString() + ")"; break;
        case BNF.OPrsh: res += "(" + CN1?.ToString() + ">>" + CN2?.ToString() + ")"; break;
        case BNF.OPland: res += "(" + CN1?.ToString() + "&&" + CN2?.ToString() + ")"; break;
        case BNF.OPlor: res += "(" + CN1?.ToString() + "||" + CN2?.ToString() + ")"; break;

        case BNF.ASSIGN: res += CN1?.ToString() + " = " + CN2?.ToString(); break;
        case BNF.ASSIGNsum: res += CN1?.ToString() + " += " + CN2?.ToString(); break;
        case BNF.ASSIGNsub: res += CN1?.ToString() + " -= " + CN2?.ToString(); break;
        case BNF.ASSIGNmul: res += CN1?.ToString() + " *= " + CN2?.ToString(); break;
        case BNF.ASSIGNdiv: res += CN1?.ToString() + " /= " + CN2?.ToString(); break;
        case BNF.ASSIGNand: res += CN1?.ToString() + " &= " + CN2?.ToString(); break;
        case BNF.ASSIGNmod: res += CN1?.ToString() + " %= " + CN2?.ToString(); break;
        case BNF.ASSIGNor: res += CN1?.ToString() + " |= " + CN2?.ToString(); break;
        case BNF.ASSIGNxor: res += CN1?.ToString() + " ^= " + CN2?.ToString(); break;

        case BNF.UOneg: res += id + " !" + CN1?.ToString(); break;
        case BNF.UOinv: res += id + " ~" + CN1?.ToString(); break;
        case BNF.UOsub: res += id + " -" + CN1?.ToString(); break;

        case BNF.CASTb: res += id + CN1?.ToString() + "_b"; break;
        case BNF.CASTi: res += id + CN1?.ToString() + "_i"; break;
        case BNF.CASTf: res += id + CN1?.ToString() + "_f"; break;
        case BNF.CASTs: res += id + CN1?.ToString() + "_s"; break;

        case BNF.LEN: res += CN1?.ToString() + ".len"; break;
        case BNF.PLEN: res += CN1?.ToString() + ".plen"; break;
        case BNF.CLR: res += id + "Clr(" + CN1?.ToString() + ")"; break;
        case BNF.DTIME: res += "dateTime"; break;

        case BNF.LUMA: return id + "Luma(" + CN1?.ToString() + ")";
        case BNF.CONTRAST: return id + "Contrast(" + CN1?.ToString() + ")";

        case BNF.WRITE: {
          res += id + "Write(" +
            CN1.ToString() + ", " +
            CN2.ToString() + ", " +
            CN3.ToString() + ", " +
            CN4.ToString();
          if (children.Count > 4) res += ", " + CN5.ToString();
          res += ")";
        }
        break;

        case BNF.WAIT: {
          res += id + "Wait(" + CN1.ToString() + ")";
        }
        break;

        case BNF.LINE:
          res += id + "line(" +
            CN1.ToString() + ", " +
            CN2.ToString() + ", " +
            CN3.ToString() + ", " +
            CN4.ToString() + ", " +
            CN5.ToString() + ")";
          break;

        case BNF.BOX:
          res += id + "line(" +
            CN1.ToString() + ", " +
            CN2.ToString() + ", " +
            CN3.ToString() + ", " +
            CN4.ToString() + ", " +
            CN5.ToString();
          if (children.Count > 5)
            res += CN6.ToString() + ")";
          else
            res += ")";
          break;

        case BNF.CIRCLE:
          res += id + "circle(" +
            CN1.ToString() + ", " +
            CN2.ToString() + ", " +
            CN3.ToString() + ", " +
            CN4.ToString() + ", " +
            CN5.ToString();
          if (children.Count > 5)
            res += CN6.ToString() + ")";
          else
            res += ")";
          break;

        case BNF.IncCmd: res += id + CN1.ToString() + "++"; break;
        case BNF.IncExp: res += id + CN1.ToString() + "++"; break;
        case BNF.DecCmd: res += id + CN1.ToString() + "--"; break;
        case BNF.DecExp: res += id + CN1.ToString() + "--"; break;

        case BNF.COMPeq: res += "(" + CN1.ToString() + "==" + CN2.ToString() + ")"; break;
        case BNF.COMPne: res += "(" + CN1.ToString() + "!=" + CN2.ToString() + ")"; break;
        case BNF.COMPlt: res += "(" + CN1.ToString() + "<" + CN2.ToString() + ")"; break;
        case BNF.COMPle: res += "(" + CN1.ToString() + "<=" + CN2.ToString() + ")"; break;
        case BNF.COMPgt: res += "(" + CN1.ToString() + ">" + CN2.ToString() + ")"; break;
        case BNF.COMPge: res += "(" + CN1.ToString() + ">=" + CN2.ToString() + ")"; break;

        case BNF.IF: {
          res += id + "if (" + CN1.ToString() + ") { ..." + CN2?.ToString() + "... }";
          if (CN3 != null) res += id + " else { ..." + CN3.ToString() + "... }";
          break;
        }
        case BNF.ELSE: res += id + "else { ..." + (children.Count) + "... }"; break;
        case BNF.WHILE: res += id + "while (" + CN1.ToString() + ") { ..." + (children.Count - 1) + "... }"; break;

        case BNF.SCREEN: {
          res += id + "screen(" +
            CN1.ToString() + ", " +
            CN2.ToString();
          if (children.Count > 2) res += ", " + CN3.ToString();
          if (children.Count > 3) res += ", " + CN4.ToString();
          res += ")";
        }
        break;

        case BNF.BLOCK: {
          res = "{ ";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString() + "\n";
          res += " }";
        }
        break;

        case BNF.Ram: res += id + "ram(" + iVal + ")"; break;
        case BNF.Rom: res += id + "rom(" + iVal + ")"; break;
        case BNF.PaletteConfig: res += id + "Palette(" + iVal + ")"; break;

        case BNF.Label: res += id + sVal + ":"; break;
        case BNF.LABG: res += id + "Label(" + CN1.ToString() + ")"; break;

        case BNF.FRAME: res += id + "frame"; break;

        case BNF.KEY: res += id + "key" + "LLLRRRUUUDDDAAABBBCCCFFFEEE"[iVal] + (iVal % 3 == 1 ? "u" : (iVal % 3 == 2 ? "d" : "")); break;
        case BNF.KEYx: res += id + "keyX"; break;
        case BNF.KEYy: res += id + "keyY"; break;

        case BNF.FOR: {
          return id + "for(" +
            CN1.ToString() + ", " +
            CN2.ToString() + ", ..." + ")" + "\n" +
            CN3.ToString();
        }

        case BNF.SPRITE: {
          res += id + "sprite(";
          res += CN1.ToString() + ", " + CN2.ToString();
          if (CN3 != null) res += ", " + CN3.ToString();
          if (CN4 != null) res += ", " + CN4.ToString();
          if (CN5 != null) res += ", " + CN5.ToString();
          res += ")";
        }
        break;
        case BNF.DESTROY: return id + "destroy(" + CN1.ToString() + ")";
        case BNF.SPOS:
          return id + "SPos(" +
                  CN1.ToString() + ", " + CN2.ToString() + ", " + CN3.ToString() +
                  (CN4 != null ? ", " + CN4.ToString() : "") + ")";
        case BNF.SROT: return id + "SRot(" + CN1.ToString() + ", " + CN2.ToString() + ", " + CN3.ToString() + ")";
        case BNF.SPEN: return id + "SPEn(" + CN1.ToString() + ", " + CN2.ToString() + ")";
        case BNF.STINT: return id + "STint(" + CN1.ToString() + ", " + CN2.ToString() + ")";
        case BNF.SSCALE: return id + "SScale(" + CN1.ToString() + ", " + CN2.ToString() + ", " + CN3.ToString() + ")";
        case BNF.SPRI: return id + "SPri(" + CN1.ToString() + ", " + CN2.ToString() + ")";

        case BNF.FunctionDef:
          return id + sVal + (CN1 == null ? "()" : CN1.ToString()) + " {" + (CN2 == null ? "" : (CN2.children == null ? CN2.ToString() : CN2.children.Count.ToString())) + "}";

        case BNF.FunctionCall: return id + sVal + (CN1 == null ? "()" : CN1.ToString());

        case BNF.Params: {
          if (children == null) return "()";
          string pars = "(";
          for (int i = 0; i < children.Count; i++) {
            if (i > 0) pars += ", ";
            pars += children[i].ToString();
          }
          return pars + ")";
        }

        case BNF.RETURN: return id + "return " + (CN1 == null ? "" : CN1.ToString());

        case BNF.NOP: return "";
        case BNF.SETP: return "SetPixel(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ")";
        case BNF.GETP: return "GetPixel(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
        case BNF.SIN: return "Sin(" + CN1?.ToString() + ")";
        case BNF.COS: return "Cos(" + CN1?.ToString() + ")";
        case BNF.TAN: return "Tan(" + CN1?.ToString() + ")";
        case BNF.ATAN2: return "Atan2(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
        case BNF.SQR: return "Sqrt(" + CN1?.ToString() + ")";
        case BNF.POW: return "Pow(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
        case BNF.SUBSTRING: return "SubString(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
        case BNF.TRIM: return "Trim()";
        case BNF.SOUND:
          if (CN3 == null) return "Sound(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
          else return "Sound(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ")";
        case BNF.WAVE:
          if (children.Count == 2) return "Wave(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
          else return "Wave(" +
              CN1?.ToString() + ", " + CN2?.ToString() + ", " +
              CN3?.ToString() + ", " + CN4?.ToString() + ", " +
              CN5?.ToString() + ", " + CN6?.ToString() + ")";
        case BNF.MUTE: return "Mute(" + CN1?.ToString() + ")";
        case BNF.VOLUME:
          if (CN2 == null) return "Volume(" + CN1?.ToString() + ")";
          else return "Volume(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
        case BNF.PITCH: return "Pitch(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
        case BNF.PAN: return "Pan(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";

        case BNF.MUSICLOAD: return "LoadMusic(" + CN1?.ToString() + ")";
        case BNF.MUSICPLAY: return "PlayMusic(" + CN1?.ToString() + ")";
        case BNF.MUSICSTOP: return "StopMusic()";
        case BNF.MUSICPOS: return "MusicPos";
        case BNF.MUSICVOICES: {
          res = "MusicVoices(";
          if (children.Count > 0) res += CN1?.ToString();
          if (children.Count > 1) res += ", " + CN2?.ToString();
          if (children.Count > 2) res += ", " + CN3?.ToString();
          if (children.Count > 3) res += ", " + CN4?.ToString();
          if (children.Count > 4) res += ", " + CN5?.ToString();
          if (children.Count > 5) res += ", " + CN6?.ToString();
          if (children.Count > 6) res += ", " + CN7?.ToString();
          if (children.Count > 7) res += ", " + CN8?.ToString();
          res += ")";
          return res;
        }

        case BNF.TILEMAP: return "TileMap(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ")";
        case BNF.TILEPOS:
          return "TilePos(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() +
          (CN4 == null ? "" : (", " + CN4.ToString() +
          (CN5 == null ? "" : ", " + CN5.ToString())
          )) + ")";

        case BNF.TILESET:
          return "TileSet(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() +
                ", " + CN4.ToString() + (CN5 == null ? "" : ", " + CN5.ToString()) + ")";

        case BNF.TILEGET: return "TileGet(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ")";
        case BNF.TILEGETROT: return "TileGetRot(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ")";

        case BNF.IMAGE: {
          res = "Image(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ", " + CN4?.ToString() + ", " + CN5?.ToString();
          if (children.Count > 5) res += ", " + CN6?.ToString() + ", " + CN7?.ToString();
          res += ")";
          return res;
        }

        case BNF.USEPALETTE: return "UsePalette(" + CN1?.ToString() + ")";
        case BNF.SETPALETTECOLOR: return "SetPaletteColor(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ", " + CN4?.ToString() + ", " + CN5?.ToString() + ")";

        case BNF.MEMCPY: return "MemCpy(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ")";
        case BNF.PERLIN: {
          if (CN2 == null)
            return "Perlin(" + CN1?.ToString() + ")";
          else if (CN3 == null)
            return "Perlin(" + CN1?.ToString() + ", " + CN2?.ToString() + ")";
          else
            return "Perlin(" + CN1?.ToString() + ", " + CN2?.ToString() + ", " + CN3?.ToString() + ")";
        }

        case BNF.ERROR: return sVal;

        default:
          res += "[[Missing:" + type + "]]";
          break;
      }
    } catch (System.Exception e) {
      res += "[[INVALID!!]]" + e.Message;
    }

    return res.Replace("  ", " ");
  }

  internal void SetComments(string com, CommentType cType) {
    comment = com;
    commentType = cType;
  }

  internal string Format(Variables variables, bool hadOpenBlock, bool coloring) {
    if (string.IsNullOrEmpty(comment) && commentType == CommentType.MultiLineClose) {
      if (coloring)
        return "<color=#70e688><mark=#30061880>" + comment + "</mark></color> " + Format(variables, coloring) + (hadOpenBlock ? "{" : "");
      else
        return comment + Format(variables, coloring) + (hadOpenBlock ? "{" : "");
    }
    if (string.IsNullOrEmpty(comment) || commentType == CommentType.None) return Format(variables, coloring) + (hadOpenBlock ? "{" : "");
    if (commentType == CommentType.MultiLineInner || type == BNF.ERROR) {
      if (coloring)
        return "<color=#70e688><mark=#30061880>" + comment + "</mark></color>";
      else
        return comment;
    }
    if (coloring)
      return Format(variables, coloring) + (hadOpenBlock ? "{" : "") + " <color=#70e688><mark=#30061880>" + comment + "</mark></color>";
    else
      return Format(variables, coloring) + (hadOpenBlock ? "{" : "") + " " + comment;
  }

  internal string Format(Variables variables, bool coloring, string comment, CommentType ct) {
    string line = Format(variables, coloring);
    if (string.IsNullOrEmpty(comment)) return line;
    if (coloring) {
      switch (ct) {
        case CommentType.None:
        case CommentType.MultiLineInner:
          return line;

        case CommentType.MultiLineOpen:
          return line + " <color=#70e688><mark=#30061880>" + comment;

        case CommentType.SingleLine:
        case CommentType.MultiLineFull:
          return line + " <color=#70e688><mark=#30061880>" + comment + "</mark></color>";

        case CommentType.MultiLineClose:
          return comment + "</mark></color> " + line;
      }
    }
    else {
      switch (ct) {
        case CommentType.None:
        case CommentType.MultiLineInner:
          return line;

        case CommentType.MultiLineOpen:
          return line + " " + comment;

        case CommentType.SingleLine:
        case CommentType.MultiLineFull:
          return line + " " + comment;

        case CommentType.MultiLineClose:
          return comment + " " + line;
      }
    }
    return line;
  }

  internal string Format(Variables variables, bool coloring) {
    if (coloring)
      switch (type) {
        case BNF.Program: return "<color=#8080ff>Name:</color> " + sVal;
        case BNF.Start: return "<color=#8080ff>Start</color> {";
        case BNF.Update: return "<color=#8080ff>Update</color> {";
        case BNF.Config: return "<color=#8080ff>Config</color> {";
        case BNF.Data: return "<color=#8080ff>Data</color> {";
        case BNF.Functions: return "<color=#8080ff>Functions:</color> <i>(" + children.Count + ")</i>";
        case BNF.FunctionDef: return "<color=#D65CA6>#" + sVal + "</color>" + CN1?.Format(variables, coloring) + " {";
        case BNF.FunctionCall: return "<color=#D65CA6>" + sVal + "</color>" + CN1?.Format(variables, coloring);
        case BNF.RETURN: {
          if (CN1 == null) return "<color=#569CD6>return</color>";
          else return "<color=#569CD6>return</color> " + CN1?.Format(variables, coloring);
        }
        case BNF.Params: {
          string res = "<color=#D65CA6>(</color>";
          if (CN1 != null) res += CN1.Format(variables, coloring);
          if (children != null) for (int i = 1; i < children.Count; i++) {
            if (children[i] != null) res += "<color=#D65CA6>, </color>" + children[i].Format(variables, coloring);
          }
          return res + "<color=#D65CA6>)</color>";
        }
        case BNF.PaletteConfig: return "<color=#569CD6>UsePalette(</color>" + (iVal == 0 ? "0" : "1") + "<color=#569CD6>)</color>";
        case BNF.Ram:
          return "<color=#569CD6>ram(</color>" +
                  (iVal < 1024 ? iVal.ToString() : (
                  iVal < 1024 * 1024 ? (((int)(10 * iVal / 1024f)) / 10f) + "k" :
                  (((int)(10 * iVal / (1024 * 1024f))) / 10f) + "m")) +
                  "<color=#569CD6>)</color>";
        case BNF.Rom: // FIXME in Data block
          break;
        case BNF.Label: return "<color=#56DC96>" + sVal + ":</color>";
        case BNF.REG: {
          if (variables.Get(Reg).type == VT.Array) {
            return "<color=#fce916>" + variables.GetRegName(Reg) + "[</color>" +
              CN1?.Format(variables, coloring) +
              "<color=#fce916>]</color>";
          }
          return "<color=#f6fC06>" + variables.GetRegName(Reg) + "</color>";
        }
        case BNF.ARRAY:
          return "<color=#fce916>" + variables.GetRegName(Reg) + "[</color>" +
            CN1?.Format(variables, coloring) +
            "<color=#fce916>]</color>";
        case BNF.INT: {
          if (format == NumFormat.Hex) return "<color=#B5CEA8>0x" + System.Convert.ToString(iVal, 16) + "</color>";
          if (format == NumFormat.Bin) return "<color=#B5CEA8>0b" + System.Convert.ToString(iVal, 2) + "</color>";
          return "<color=#B5CEA8>" + iVal + "</color>";
        }
        case BNF.FLT: return "<color=#B5CEA8>" + fVal + "</color>";
        case BNF.COLOR: {
          UnityEngine.Color32 c = Col.GetColor((byte)iVal);
          return "<mark=#" + c.r.ToString("x2") + c.g.ToString("x2") + c.b.ToString("x2") + "80>" + Col.GetColorString(iVal) + "c</mark>";
        }
        case BNF.PAL: {
          UnityEngine.Color32 c = Col.GetColor((byte)iVal);
          return "<mark=#" + c.r.ToString("x2") + c.g.ToString("x2") + c.b.ToString("x2") + "80>" + iVal + "p</mark>";
        }
        case BNF.LUMA: return "<color=#569CD6>Luma(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.CONTRAST: return "<color=#569CD6>Contrast(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.STR: return "<color=#CA9581><mark=#1A151140>\"" + sVal + "\"</mark></color>";
        case BNF.MEM: return "<color=#FCA626>[</color>" + CN1?.Format(variables, coloring) + "<color=#FCA626>]</color>";
        case BNF.MEMlong: return "<color=#FCA626>[</color>" + CN1?.Format(variables, coloring) + "<color=#FCA626>@]</color>";
        case BNF.MEMlongb: return "<color=#FCA626>[</color>" + CN1?.Format(variables, coloring) + "<color=#FCA626>@b]</color>";
        case BNF.MEMlongi: return "<color=#FCA626>[</color>" + CN1?.Format(variables, coloring) + "<color=#FCA626>@i]</color>";
        case BNF.MEMlongf: return "<color=#FCA626>[</color>" + CN1?.Format(variables, coloring) + "<color=#FCA626>@f]</color>";
        case BNF.MEMlongs: return "<color=#FCA626>[</color>" + CN1?.Format(variables, coloring) + "<color=#FCA626>@s]</color>";
        case BNF.MEMchar: return "<color=#FCA626>[</color>" + CN1?.Format(variables, coloring) + "<color=#FCA626>@c]</color>";
        case BNF.OPpar: return "<color=#66aCe6>(</color>" + CN1?.Format(variables, coloring) + "<color=#66aCe6>)";
        case BNF.OPsum: return CN1?.Format(variables, coloring) + " <color=#66aCe6>+</color> " + CN2?.Format(variables, coloring);
        case BNF.OPsub: return CN1?.Format(variables, coloring) + " <color=#66aCe6>-</color> " + CN2?.Format(variables, coloring);
        case BNF.OPmul: return CN1?.Format(variables, coloring) + " <color=#66aCe6>*</color> " + CN2?.Format(variables, coloring);
        case BNF.OPdiv: return CN1?.Format(variables, coloring) + " <color=#66aCe6>/</color> " + CN2?.Format(variables, coloring);
        case BNF.OPmod: return CN1?.Format(variables, coloring) + " <color=#66aCe6>%</color> " + CN2?.Format(variables, coloring);
        case BNF.OPland: return CN1?.Format(variables, coloring) + " <color=#66aCe6>&&</color> " + CN2?.Format(variables, coloring);
        case BNF.OPlor: return CN1?.Format(variables, coloring) + " <color=#66aCe6>||</color> " + CN2?.Format(variables, coloring);
        case BNF.OPand: return CN1?.Format(variables, coloring) + " <color=#66aCe6>&</color> " + CN2?.Format(variables, coloring);
        case BNF.OPor: return CN1?.Format(variables, coloring) + " <color=#66aCe6>|</color> " + CN2?.Format(variables, coloring);
        case BNF.OPxor: return CN1?.Format(variables, coloring) + " <color=#66aCe6>^</color> " + CN2?.Format(variables, coloring);
        case BNF.OPlsh: return CN1?.Format(variables, coloring) + " <color=#66aCe6><<</color> " + CN2?.Format(variables, coloring);
        case BNF.OPrsh: return CN1?.Format(variables, coloring) + " <color=#66aCe6>>></color> " + CN2?.Format(variables, coloring);
        case BNF.LABG: return "<color=#569CD6>Label(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.CASTb: return CN1?.Format(variables, coloring) + "<color=#66aCe6>_b</color>";
        case BNF.CASTi: return CN1?.Format(variables, coloring) + "<color=#66aCe6>_i</color>";
        case BNF.CASTf: return CN1?.Format(variables, coloring) + "<color=#66aCe6>_f</color>";
        case BNF.CASTs: return CN1?.Format(variables, coloring) + "<color=#66aCe6>_s</color>";
        case BNF.UOneg: return "<color=#66aCe6>!</color>" + CN1?.Format(variables, coloring);
        case BNF.UOinv: return "<color=#66aCe6>~</color>" + CN1?.Format(variables, coloring);
        case BNF.UOsub: return "<color=#66aCe6>-</color>" + CN1?.Format(variables, coloring);
        case BNF.COMPeq: return CN1?.Format(variables, coloring) + " <b><color=#66aCe6>==</color></b> " + CN2?.Format(variables, coloring);
        case BNF.COMPne: return CN1?.Format(variables, coloring) + " <b><color=#66aCe6>!=</color></b> " + CN2?.Format(variables, coloring);
        case BNF.COMPlt: return CN1?.Format(variables, coloring) + " <b><color=#66aCe6><</color></b> " + CN2?.Format(variables, coloring);
        case BNF.COMPle: return CN1?.Format(variables, coloring) + " <b><color=#66aCe6><=</color></b> " + CN2?.Format(variables, coloring);
        case BNF.COMPgt: return CN1?.Format(variables, coloring) + " <b><color=#66aCe6>></color></b> " + CN2?.Format(variables, coloring);
        case BNF.COMPge: return CN1?.Format(variables, coloring) + " <b><color=#66aCe6>>=</color></b> " + CN2?.Format(variables, coloring);
        case BNF.ASSIGN: return CN1?.Format(variables, coloring) + " = " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNsum: return CN1?.Format(variables, coloring) + " += " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNsub: return CN1?.Format(variables, coloring) + " -= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNmul: return CN1?.Format(variables, coloring) + " *= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNdiv: return CN1?.Format(variables, coloring) + " /= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNmod: return CN1?.Format(variables, coloring) + " %= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNand: return CN1?.Format(variables, coloring) + " &= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNor: return CN1?.Format(variables, coloring) + " |= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNxor: return CN1?.Format(variables, coloring) + " ^= " + CN2?.Format(variables, coloring);
        case BNF.IncCmd: return CN1?.Format(variables, coloring) + "++";
        case BNF.IncExp: return CN1?.Format(variables, coloring) + "++";
        case BNF.DecCmd: return CN1?.Format(variables, coloring) + "--";
        case BNF.DecExp: return CN1?.Format(variables, coloring) + "--";
        case BNF.BLOCK: {
          if (CN1 == null) return "<color=#569CD6>{}</color>";
          if (CN2 == null) return CN1.Format(variables, coloring);
          return "<color=#569CD6>{</color>" + CN1?.Format(variables, coloring) + ", ...<color=#569CD6>}</color>"; // FIXME
        }
        case BNF.WHILE: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "<color=#569CD6>while (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color> {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "<color=#569CD6>while (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color> " + CN2?.CN1.Format(variables, coloring);
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************

          return "<color=#569CD6>while (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.FOR: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "<color=#569CD6>for (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>,</color> " + CN2?.Format(variables, coloring) + "<color=#569CD6>,</color> " + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color> {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "<color=#569CD6>for (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>,</color> " + CN2?.Format(variables, coloring) + "<color=#569CD6>,</color> " + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color> " + CN4?.CN1?.Format(variables, coloring);
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "<color=#569CD6>for (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>,</color> " + CN2?.Format(variables, coloring) + "<color=#569CD6>,</color> " + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.IF: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "<color=#569CD6>if (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color> {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "<color=#569CD6>if (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color> " + CN2?.CN1?.Format(variables, coloring);
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "<color=#569CD6>if (</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.ELSE: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "<color=#569CD6>else</color> {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "<color=#569CD6>else</color> " + CN1?.CN1?.Format(variables, coloring);
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "<color=#569CD6>else</color>";
        }

        case BNF.CLR: return "<color=#569CD6>Clr(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.WRITE: { // Write(string txt, int x, int y, byte col, byte back = 255, byte mode = 0)
          if (children.Count <= 4)
            return "<color=#569CD6>Write(</color>" +
              CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else if (children.Count == 5)
            return "<color=#569CD6>Write(</color>" +
              CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN5.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Write(</color>" +
              CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN5?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN6?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.WAIT: return "<color=#569CD6>Wait(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.DESTROY: return "<color=#569CD6>Destroy(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.SCREEN: {
          if (CN3 == null)
            return "<color=#569CD6>Screen(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Screen(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.SPRITE: {
          if (CN3 == null)
            return "<color=#569CD6>Sprite(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Sprite(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.SPEN: return "<color=#569CD6>SpEn(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.SPOS: {
          if (CN4 == null)
            return "<color=#569CD6>SPos(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>SPos(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.SROT: return "<color=#569CD6>SRot(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.SPRI: return "<color=#569CD6>SPri(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.STINT: return "<color=#569CD6>STint(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.SSCALE: return "<color=#569CD6>SScale(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.SETP: return "<color=#569CD6>SetP(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.GETP: return "<color=#569CD6>GetP(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.LINE: {
          return "<color=#569CD6>Line(</color>" +
            CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
            CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN5?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.BOX: {
          if (children.Count == 5)
            return "<color=#569CD6>Box(</color>" +
              CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN5?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Box(</color>" +
              CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN5?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN6?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.CIRCLE: {
          if (children.Count == 5)
            return "<color=#569CD6>Circle(</color>" +
              CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN5?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Circle(</color>" +
              CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN5?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN6?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.IMAGE: {
          if (children.Count < 6)
            return "<color=#569CD6>Image(</color>" +
            CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
            CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
            CN5?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Image(</color>" +
              CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
              CN5?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN6?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN7?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.FRAME: return "<color=#569CD6>Frame</color>";
        case BNF.DTIME: return "<color=#569CD6>deltatime</color>";
        case BNF.LEN: return CN1?.Format(variables, coloring) + "<color=#569CD6>.Len</color>";
        case BNF.PLEN: return CN1?.Format(variables, coloring) + "<color=#569CD6>.PLen</color>";
        case BNF.SUBSTRING: {
          if (CN3 == null)
            return CN1?.Format(variables, coloring) + "<color=#569CD6>.Substring(</color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return CN1?.Format(variables, coloring) + "<color=#569CD6>.Substring(</color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.TRIM: return CN1?.Format(variables, coloring) + "<color=#569CD6>.Trim</color>";

        case BNF.KEY: return "<color=#569CD6>key" + "LLLRRRUUUDDDAAABBBCCCFFFEEE"[iVal] + (iVal % 3 == 1 ? "u" : (iVal % 3 == 2 ? "d" : "")) + "</color>";
        case BNF.KEYx: return "<color=#569CD6>keyX</color>";
        case BNF.KEYy: return "<color=#569CD6>keyY</color>";

        case BNF.SIN: return "<color=#569CD6>Sin(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.COS: return "<color=#569CD6>Cos(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.TAN: return "<color=#569CD6>Tan(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.ATAN2: return "<color=#569CD6>Atan2(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.SQR: return "<color=#569CD6>Sqrt(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.POW: return "<color=#569CD6>Pow(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.MEMCPY: return "<color=#569CD6>MemCpy(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.PERLIN: {
          if (CN2 == null)
            return "<color=#569CD6>Perlin(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else if (CN3 == null)
            return "<color=#569CD6>Perlin(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Perlin(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.SOUND: {
          if (CN3 == null)
            return "<color=#569CD6>Sound(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Sound(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.WAVE: {
          if (CN5 == null)
            return "<color=#569CD6>Sound(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) +
              "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) +
              "<color=#569CD6>)</color>";
          else
            return "<color=#569CD6>Sound(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) +
              "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN5?.Format(variables, coloring) +
              "<color=#569CD6>)</color>";
        }
        case BNF.MUTE: return "<color=#569CD6>Mute(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.VOLUME: {
          if (CN2 == null) return "<color=#569CD6>Volume(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
          else return "<color=#569CD6>Volume(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        }
        case BNF.PITCH: return "<color=#569CD6>Pitch(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.PAN: return "<color=#569CD6>Pan(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.MUSICLOAD: return "<color=#569CD6>LoadMusic(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.MUSICPLAY: return "<color=#569CD6>PlayMusic(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.MUSICSTOP: return "<color=#569CD6>StopMusic()</color>";
        case BNF.MUSICPOS: return "<color=#569CD6>MusicPos()</color>";
        case BNF.MUSICVOICES: {
          string res = "<color=#569CD6>MusicVoices(</color>" + CN1?.Format(variables, coloring);
          if (CN2 != null) res += "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring);
          if (CN3 != null) res += "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring);
          if (CN4 != null) res += "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring);
          if (CN5 != null) res += "<color=#569CD6>, </color>" + CN5?.Format(variables, coloring);
          if (CN6 != null) res += "<color=#569CD6>, </color>" + CN6?.Format(variables, coloring);
          if (CN7 != null) res += "<color=#569CD6>, </color>" + CN7?.Format(variables, coloring);
          if (CN8 != null) res += "<color=#569CD6>, </color>" + CN8?.Format(variables, coloring);
          return res + "<color=#569CD6>)</color>";
        }
        case BNF.TILEMAP: return "<color=#569CD6>TileMap(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.TILEPOS: {
          string res = "<color=#569CD6>TilePos(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring);
          if (CN4 != null) res += "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring);
          if (CN5 != null) res += "<color=#569CD6>, </color>" + CN5?.Format(variables, coloring);
          return res + "<color=#569CD6>)</color>";
        }
        case BNF.TILESET: {
          string res = "<color=#569CD6>TileSet(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) +
          "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN4?.Format(variables, coloring);
          if (CN5 != null) res += "<color=#569CD6>, </color>" + CN5?.Format(variables, coloring);
          return res + "<color=#569CD6>)</color>";
        }
        case BNF.TILEGET:
          return "<color=#569CD6>TileGet(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) +
                  "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.TILEGETROT:
          return "<color=#569CD6>TileGetRot(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN2?.Format(variables, coloring) +
              "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.NOP: return "";
        case BNF.USEPALETTE: return "<color=#569CD6>UsePalette(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.SETPALETTECOLOR:
          return "<color=#569CD6>SetPalette(</color>" + CN1?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
CN2?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN3?.Format(variables, coloring) + "<color=#569CD6>, </color>" +
CN4?.Format(variables, coloring) + "<color=#569CD6>, </color>" + CN5?.Format(variables, coloring) + "<color=#569CD6>)</color>";
        case BNF.ERROR: return "<color=#ff2010>" + sVal + "</color>";
      }

    else
      switch (type) {
        case BNF.Program: return "Name: " + sVal;
        case BNF.Start: return "Start {";
        case BNF.Update: return "Update {";
        case BNF.Config: return "Cofig {";
        case BNF.Data: return "Data {";
        case BNF.Functions: return "Functions: (" + children.Count + ")";
        case BNF.FunctionDef: return "#" + sVal + CN1?.Format(variables, coloring) + " {";
        case BNF.FunctionCall: return sVal + CN1?.Format(variables, coloring);
        case BNF.RETURN: {
          if (CN1 == null) return "return";
          else return "return " + CN1?.Format(variables, coloring);
        }
        case BNF.Params: {
          string res = "(";
          if (CN1 != null) res += CN1.Format(variables, coloring);
          if (children != null) for (int i = 1; i < children.Count; i++) {
            if (children[i] != null) res += ", " + children[i].Format(variables, coloring);
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
        case BNF.REG: return variables.GetRegName(Reg);
        case BNF.ARRAY:
          return variables.GetRegName(Reg) + "[" + CN1?.Format(variables, coloring) + "]";
        case BNF.INT: {
          if (format == NumFormat.Hex) return "0x" + System.Convert.ToString(iVal, 16);
          if (format == NumFormat.Bin) return "0b" + System.Convert.ToString(iVal, 2);
          return iVal.ToString();
        }
        case BNF.FLT: return fVal.ToString();
        case BNF.COLOR: return Col.GetColorString(iVal) + "c";
        case BNF.PAL: return iVal + "p";
        case BNF.LUMA: return "Luma(" + CN1?.Format(variables, coloring) + ")";
        case BNF.CONTRAST: return "Contrast(" + CN1?.Format(variables, coloring) + ")";
        case BNF.STR: return "\"" + sVal + "\"";
        case BNF.MEM: return "[" + CN1?.Format(variables, coloring) + "]";
        case BNF.MEMlong: return "[" + CN1?.Format(variables, coloring) + "]@";
        case BNF.MEMlongb: return "[" + CN1?.Format(variables, coloring) + "]b";
        case BNF.MEMlongi: return "[" + CN1?.Format(variables, coloring) + "]i";
        case BNF.MEMlongf: return "[" + CN1?.Format(variables, coloring) + "]f";
        case BNF.MEMlongs: return "[" + CN1?.Format(variables, coloring) + "]s";
        case BNF.MEMchar: return "[" + CN1?.Format(variables, coloring) + "]c";

        case BNF.OPpar: return "(" + CN1?.Format(variables, coloring) + ")";
        case BNF.OPsum: return CN1?.Format(variables, coloring) + " + " + CN2?.Format(variables, coloring);
        case BNF.OPsub: return CN1?.Format(variables, coloring) + " - " + CN2?.Format(variables, coloring);
        case BNF.OPmul: return CN1?.Format(variables, coloring) + " * " + CN2?.Format(variables, coloring);
        case BNF.OPdiv: return CN1?.Format(variables, coloring) + " / " + CN2?.Format(variables, coloring);
        case BNF.OPmod: return CN1?.Format(variables, coloring) + " % " + CN2?.Format(variables, coloring);
        case BNF.OPland: return CN1?.Format(variables, coloring) + " && " + CN2?.Format(variables, coloring);
        case BNF.OPlor: return CN1?.Format(variables, coloring) + " || " + CN2?.Format(variables, coloring);
        case BNF.OPand: return CN1?.Format(variables, coloring) + " & " + CN2?.Format(variables, coloring);
        case BNF.OPor: return CN1?.Format(variables, coloring) + " | " + CN2?.Format(variables, coloring);
        case BNF.OPxor: return CN1?.Format(variables, coloring) + " ^ " + CN2?.Format(variables, coloring);
        case BNF.OPlsh: return CN1?.Format(variables, coloring) + " << " + CN2?.Format(variables, coloring);
        case BNF.OPrsh: return CN1?.Format(variables, coloring) + " >> " + CN2?.Format(variables, coloring);
        case BNF.LABG: return "Label(" + CN1?.Format(variables, coloring) + ")";
        case BNF.CASTb: return CN1?.Format(variables, coloring) + "_b";
        case BNF.CASTi: return CN1?.Format(variables, coloring) + "_i";
        case BNF.CASTf: return CN1?.Format(variables, coloring) + "_f";
        case BNF.CASTs: return CN1?.Format(variables, coloring) + "_s";
        case BNF.UOneg: return "!" + CN1?.Format(variables, coloring);
        case BNF.UOinv: return "~" + CN1?.Format(variables, coloring);
        case BNF.UOsub: return "-" + CN1?.Format(variables, coloring);
        case BNF.COMPeq: return CN1?.Format(variables, coloring) + " == " + CN2?.Format(variables, coloring);
        case BNF.COMPne: return CN1?.Format(variables, coloring) + " != " + CN2?.Format(variables, coloring);
        case BNF.COMPlt: return CN1?.Format(variables, coloring) + " < " + CN2?.Format(variables, coloring);
        case BNF.COMPle: return CN1?.Format(variables, coloring) + " <= " + CN2?.Format(variables, coloring);
        case BNF.COMPgt: return CN1?.Format(variables, coloring) + " > " + CN2?.Format(variables, coloring);
        case BNF.COMPge: return CN1?.Format(variables, coloring) + " >= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGN: return CN1?.Format(variables, coloring) + " = " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNsum: return CN1?.Format(variables, coloring) + " += " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNsub: return CN1?.Format(variables, coloring) + " -= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNmul: return CN1?.Format(variables, coloring) + " *= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNdiv: return CN1?.Format(variables, coloring) + " /= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNmod: return CN1?.Format(variables, coloring) + " %= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNand: return CN1?.Format(variables, coloring) + " &= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNor: return CN1?.Format(variables, coloring) + " |= " + CN2?.Format(variables, coloring);
        case BNF.ASSIGNxor: return CN1?.Format(variables, coloring) + " ^= " + CN2?.Format(variables, coloring);
        case BNF.IncCmd: return CN1?.Format(variables, coloring) + "++";
        case BNF.IncExp: return CN1?.Format(variables, coloring) + "++";
        case BNF.DecCmd: return CN1?.Format(variables, coloring) + "--";
        case BNF.DecExp: return CN1?.Format(variables, coloring) + "--";
        case BNF.BLOCK: {
          if (CN1 == null) return "{}";
          if (CN2 == null) return CN1.Format(variables, coloring);
          return "{" + CN1?.Format(variables, coloring) + ", ...}"; // FIXME
        }

        case BNF.WHILE: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "while (" + CN1?.Format(variables, coloring) + ") {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "while (" + CN1?.Format(variables, coloring) + ") " + CN2?.CN1?.Format(variables, coloring);
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "while (" + CN1?.Format(variables, coloring) + ")";
        }
        case BNF.FOR: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "for (" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ") {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "for (" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ") " + CN4?.CN1?.Format(variables, coloring);
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "for (" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
        }
        case BNF.IF: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "if (" + CN1?.Format(variables, coloring) + ") {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "if (" + CN1?.Format(variables, coloring) + ") " + CN2?.CN1?.Format(variables, coloring);
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "if (" + CN1?.Format(variables, coloring) + ")";
        }
        case BNF.ELSE: {
          if (iVal == 1) // ******************* 1 block open same line *********************************************************
            return "else {";
          if (iVal == 2) // ****************** 2 single statement same line ****************************************************
            return "else " + CN1?.CN1?.Format(variables, coloring);
          // ****************** 3 block open next line **********************************************************
          // ****************** 4 single statement next line ****************************************************
          return "else";
        }

        case BNF.CLR: return "Clr(" + CN1?.Format(variables, coloring) + ")";
        case BNF.WRITE: { // Write(string txt, int x, int y, byte col, byte back = 255, byte mode = 0)
          if (children.Count == 4)
            return "Write(" +
              CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
              CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ")";
          else if (children.Count == 5)
            return "Write(" +
              CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
              CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " + CN5.Format(variables, coloring) + ")";
          else
            return "Write(" +
              CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
              CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " +
              CN5?.Format(variables, coloring) + ", " + CN6?.Format(variables, coloring) + ")";
        }
        case BNF.WAIT: return "Wait(" + CN1?.Format(variables, coloring) + ")";
        case BNF.DESTROY: return "Destroy(" + CN1?.Format(variables, coloring) + ")";
        case BNF.SCREEN: {
          if (CN3 == null)
            return "Screen(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ")";
          else
            return "Screen(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3.Format(variables, coloring) + ")";
        }
        case BNF.SPRITE: {
          if (CN3 == null)
            return "Sprite(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ")";
          else
            return "Sprite(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3.Format(variables, coloring) + ")";
        }
        case BNF.SPEN: return "SpEn(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ")";
        case BNF.SPOS: {
          if (CN4 == null)
            return "SPos(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
          else
            return "SPos(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ")";
        }
        case BNF.SROT: return "SRot(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
        case BNF.SPRI: return "SPri(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ")";
        case BNF.STINT: return "STint(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ")";
        case BNF.SSCALE: return "SScale(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
        case BNF.SETP: return "SetP(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ", " + CN3.Format(variables, coloring) + ")";
        case BNF.GETP: return "GetP(" + CN1?.Format(variables, coloring) + ", " + CN2.Format(variables, coloring) + ")";
        case BNF.LINE: {
          return "Line(" +
            CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
            CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " + CN5?.Format(variables, coloring) + ")";
        }
        case BNF.BOX: {
          if (children.Count == 5)
            return "Box(" +
              CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
              CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " + CN5?.Format(variables, coloring) + ")";
          else
            return "Box(" +
              CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
              CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " +
              CN5?.Format(variables, coloring) + ", " + CN6?.Format(variables, coloring) + ")";
        }
        case BNF.CIRCLE: {
          if (children.Count == 5)
            return "Circle(" +
              CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
              CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " + CN5?.Format(variables, coloring) + ")";
          else
            return "Circle(" +
              CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
              CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " +
              CN5?.Format(variables, coloring) + ", " + CN6?.Format(variables, coloring) + ")";
        }
        case BNF.IMAGE: {
          if (children.Count < 6)
            return "Image(" +
            CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
            CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " +
            CN5?.Format(variables, coloring) + ")";
          else
            return "Image(" +
              CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " +
              CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " +
              CN5?.Format(variables, coloring) + ", " + CN6?.Format(variables, coloring) + ", " + CN7?.Format(variables, coloring) + ")";
        }
        case BNF.FRAME: return "Frame";
        case BNF.DTIME: return "deltatime";
        case BNF.LEN: return CN1?.Format(variables, coloring) + ".Len";
        case BNF.PLEN: return CN1?.Format(variables, coloring) + ".PLen";
        case BNF.SUBSTRING: {
          if (CN3 == null)
            return CN1?.Format(variables, coloring) + ".Substring(" + CN2?.Format(variables, coloring) + ")";
          else
            return CN1?.Format(variables, coloring) + ".Substring(" + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
        }
        case BNF.TRIM: return CN1?.Format(variables, coloring) + ".Trim";

        case BNF.KEY: return "key" + "LLLRRRUUUDDDAAABBBCCCFFFEEE"[iVal] + (iVal % 3 == 1 ? "u" : (iVal % 3 == 2 ? "d" : "")) + "";
        case BNF.KEYx: return "keyX";
        case BNF.KEYy: return "keyY";

        case BNF.SIN: return "Sin(" + CN1?.Format(variables, coloring) + ")";
        case BNF.COS: return "Cos(" + CN1?.Format(variables, coloring) + ")";
        case BNF.TAN: return "Tan(" + CN1?.Format(variables, coloring) + ")";
        case BNF.ATAN2: return "Atan2(" + CN1?.Format(variables, coloring) + ")";
        case BNF.SQR: return "Sqrt(" + CN1?.Format(variables, coloring) + ")";
        case BNF.POW: return "Pow(" + CN1?.Format(variables, coloring) + ")";
        case BNF.MEMCPY: return "MemCpy(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
        case BNF.PERLIN: {
          if (CN2 == null)
            return "Perlin(" + CN1?.Format(variables, coloring) + ")";
          else if (CN3 == null)
            return "Perlin(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ")";
          else
            return "Perlin(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
        }
        case BNF.SOUND: {
          if (CN3 == null)
            return "Sound(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ")";
          else
            return "Sound(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
        }
        case BNF.WAVE: {
          if (CN5 == null)
            return "Sound(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) +
              ", " + CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) +
              ")";
          else
            return "Sound(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) +
              ", " + CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring) + ", " + CN5?.Format(variables, coloring) +
              ")";
        }
        case BNF.MUTE: return "Mute(" + CN1?.Format(variables, coloring) + ")";
        case BNF.VOLUME: {
          if (CN2 == null) return "Volume(" + CN1?.Format(variables, coloring) + ")";
          else return "Volume(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ")";
        }
        case BNF.PITCH: return "Pitch(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ")";
        case BNF.PAN: return "Pan(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ")";
        case BNF.MUSICLOAD: return "LoadMusic(" + CN1?.Format(variables, coloring) + ")";
        case BNF.MUSICPLAY: return "PlayMusic(" + CN1?.Format(variables, coloring) + ")";
        case BNF.MUSICSTOP: return "StopMusic()";
        case BNF.MUSICPOS: return "MusicPos()";
        case BNF.MUSICVOICES: {
          string res = "MusicVoices(" + CN1?.Format(variables, coloring);
          if (CN2 != null) res += ", " + CN2?.Format(variables, coloring);
          if (CN3 != null) res += ", " + CN3?.Format(variables, coloring);
          if (CN4 != null) res += ", " + CN4?.Format(variables, coloring);
          if (CN5 != null) res += ", " + CN5?.Format(variables, coloring);
          if (CN6 != null) res += ", " + CN6?.Format(variables, coloring);
          if (CN7 != null) res += ", " + CN7?.Format(variables, coloring);
          if (CN8 != null) res += ", " + CN8?.Format(variables, coloring);
          return res + ")";
        }
        case BNF.TILEMAP: return "TileMap(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ")";
        case BNF.TILEPOS: {
          string res = "TilePos(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring);
          if (CN4 != null) res += ", " + CN4?.Format(variables, coloring);
          if (CN5 != null) res += ", " + CN5?.Format(variables, coloring);
          return res + ")";
        }
        case BNF.TILESET: {
          string res = "TileSet(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) +
          ", " + CN3?.Format(variables, coloring) + ", " + CN4?.Format(variables, coloring);
          if (CN5 != null) res += ", " + CN5?.Format(variables, coloring);
          return res + ")";
        }
        case BNF.TILEGET:
          return "TileGet(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) +
                  ", " + CN3?.Format(variables, coloring) + ")";
        case BNF.TILEGETROT:
          return "TileGetRot(" + CN1?.Format(variables, coloring) + ", " + CN2?.Format(variables, coloring) +
              ", " + CN3?.Format(variables, coloring) + ")";
        case BNF.NOP: return "";
        case BNF.USEPALETTE: return "UsePalette(" + CN1?.Format(variables, coloring) + ")";
        case BNF.SETPALETTECOLOR:
          return "SetPalette(" + CN1?.Format(variables, coloring) + ", " +
            CN2?.Format(variables, coloring) + ", " + CN3?.Format(variables, coloring) + ", " +
            CN4?.Format(variables, coloring) + ", " + CN5?.Format(variables, coloring) + ")";
        case BNF.ERROR: return sVal;
      }


    throw new System.Exception(type + " NOT YET DONE!");
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
  internal bool Evaluable() {
    switch (type) {
      case BNF.REG:
      case BNF.ARRAY:
      case BNF.INT:
      case BNF.FLT:
      case BNF.COLOR:
      case BNF.STR:
      case BNF.MEM:
      case BNF.MEMlong:
      case BNF.MEMlongb:
      case BNF.MEMlongi:
      case BNF.MEMlongf:
      case BNF.MEMlongs:
      case BNF.MEMchar:
      case BNF.OPpar:
      case BNF.OPsum:
      case BNF.OPsub:
      case BNF.OPmul:
      case BNF.OPdiv:
      case BNF.OPmod:
      case BNF.OPland:
      case BNF.OPlor:
      case BNF.OPand:
      case BNF.OPor:
      case BNF.OPxor:
      case BNF.OPlsh:
      case BNF.OPrsh:
      case BNF.Label:
      case BNF.LABG:
      case BNF.UOneg:
      case BNF.UOinv:
      case BNF.UOsub:
      case BNF.IncExp:
      case BNF.DecExp:
      case BNF.DTIME:
      case BNF.LEN:
      case BNF.PLEN:
      case BNF.COMPeq:
      case BNF.COMPne:
      case BNF.COMPgt:
      case BNF.COMPge:
      case BNF.COMPlt:
      case BNF.COMPle:
      case BNF.CASTb:
      case BNF.CASTi:
      case BNF.CASTf:
      case BNF.CASTs:
      case BNF.KEY:
      case BNF.KEYx:
      case BNF.KEYy:
      case BNF.RETURN:
      case BNF.FunctionCall:
      case BNF.GETP:
      case BNF.SIN:
      case BNF.COS:
      case BNF.TAN:
      case BNF.ATAN2:
      case BNF.SQR:
      case BNF.POW:
      case BNF.PERLIN:
      case BNF.SUBSTRING:
      case BNF.TRIM:
      case BNF.MUSICPOS:
      case BNF.TILEGET:
      case BNF.TILEGETROT:
        return true;
    }
    return false;
  }

}



/*
  BNF

[REG]=a-z
[INT]=0-9+
[FLT]=0-9+.0-9+
[STR]="?*" | [STR] + [EXP] | [EXP] + [STR]
[LEN] = len([STR])
[MEM]=\[[INT]\] | \[[REG]\] | \[[DISP]\] | \[[MEM]\]
[DISP] = [INT] + [REG]
[EXP]= [EXP] [OP] [EXP] | [PAR] | [REG] | [INT] | [FLT] | [MEM] | [UO] | [LEN] | deltaTime
[par]= ([EXP])
[OP] = + | - | / | * | & | \| | ^
[UO] = ~[EXP] | ![EXP]
[CND] = [EXP] [COMP] [EXP] | [EXP]
[COMP] = == | != | < | <= | > | >=
[STATEMENT] = [ASS] | [IND] | [BLOCK] | [IF] | [WHILE] | [CLR] | [WRITE] | [SPRITE] | [SPEN] | [SPOS] | [SPIX] | [GPIX] | [LINE] | [BOX] | [CIR] | [FRAME]
[ASS] = [MEM] = [EXPR] | [REG] = [EXP]
[IND] = [REG]++ | [REG]-- | [MEM]++ | [MEM]--
[BLOCK] = { [STATEMENT]+ }
[IF] = if ([EXP]) [BLOCK] | if ([EXP]) [BLOCK] else [BLOCK]
[WHILE] = while ([EXP]) [BLOCK]
[FOR] = for([ASS], [CND], [ASS]|[IND]) [BLOCK]
[CLR] = clr([EXP])
[WRITE] = write([STR], [EXP], [EXP], [EXP])
[SPRITE] = sprite([INT],[INT],[INT],[MEM])
[SPEN] = spen([INT],[CND])
[SPOS] = spos([INT],[EXP],[EXP])
[SPIX] = setpixel([EXP], [EXP], [EXP])
[GPIX] = getpixel([EXP], [EXP])
[LINE] = line([EXP], [EXP], [EXP], [EXP], [EXP])
[BOX] = box([EXP], [EXP], [EXP], [EXP], [EXP], [CND])
[CIR] = circle([EXP], [EXP], [EXP], [EXP], [EXP], [CND])
[FRAME] = frame
[DTIME] = dateTime,
[DISP] = [INT] + [REG]

 */

public class Expected {
  private ulong val = 0;

  public Expected() {
    val = 0;
  }
  public void Set(Val v) {
    val = (ulong)(1 << (int)v);
  }
  public void Set(Val v1, Val v2) {
    val = (ulong)(1 << (int)v1) + (ulong)(1 << (int)v2);
  }
  public void Set(Val v1, Val v2, Val v3) {
    val = (ulong)(1 << (int)v1) + (ulong)(1 << (int)v2) + (ulong)(1 << (int)v3);
  }
  public bool IsGood(Val v) {
    return (val & (ulong)(1 << (int)v)) != 0;
  }

  public enum Val { None, Statement, MemReg, Expression, Block };

  public override string ToString() {
    string res = "";
    if ((val & 1) != 0) res += (Val)0;
    if ((val & 2) != 0) res += (Val)1;
    if ((val & 4) != 0) res += (Val)2;
    if ((val & 8) != 0) res += (Val)3;
    if ((val & 16) != 0) res += (Val)4;
    return res;
  }

}



public enum BNF {
  Program,
  Start,
  Update,
  Config,
  Data,
  Functions,
  FunctionDef,
  FunctionCall,
  RETURN,
  Params,
  PaletteConfig,
  Ram,
  Rom, // This is used to store the data
  Label, // This is used to parse the Rom section (Data)
  ERROR, // Used to show parsing errors
  REG,
  ARRAY,
  INT, 
  FLT,
  COLOR, PAL, LUMA, CONTRAST,
  STR,
  MEM, MEMlong, MEMlongb, MEMlongi, MEMlongf, MEMlongs, MEMchar,
  OPpar, OPsum, OPsub, OPmul, OPdiv, OPmod, OPand, OPor, OPxor, OPlsh, OPrsh, OPland, OPlor,
  LABG, // This is a function to calculate a label from a string
  CASTb, CASTi, CASTf, CASTs,
  UOneg, UOinv, UOsub,
  COMPeq, COMPne, COMPlt, COMPle, COMPgt, COMPge,
  ASSIGN, ASSIGNsum, ASSIGNsub, ASSIGNmul, ASSIGNdiv, ASSIGNmod, ASSIGNand, ASSIGNor, ASSIGNxor,
  IncCmd, DecCmd, IncExp, DecExp,
  BLOCK,
  IF, ELSE, WHILE, FOR,
  CLR,
  WRITE, WAIT, DESTROY, SCREEN,
  SPRITE, SPEN, SPOS, SROT, SPRI, STINT, SSCALE,
  SETP, GETP,
  LINE, BOX, CIRCLE,
  IMAGE,
  FRAME,
  DTIME,
  LEN, PLEN, SUBSTRING, TRIM,
  KEY, KEYx, KEYy,
  SIN, COS, TAN, ATAN2, SQR, POW,
  MEMCPY,
  PERLIN,
  
  
  SOUND, WAVE, MUTE, VOLUME, PITCH, PAN,
  MUSICLOAD, MUSICPLAY, MUSICSTOP, MUSICPOS, MUSICVOICES,
  TILEMAP, TILEPOS, TILESET, TILEGET, TILEGETROT,
  NOP,
  USEPALETTE,
  SETPALETTECOLOR,
}

public enum VT {
  None,
  Int,
  Float,
  String,
  Array
}

public enum MD {
  Dir,
  Reg,
  Mem
}
