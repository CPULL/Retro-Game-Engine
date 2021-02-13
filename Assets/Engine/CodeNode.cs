using System;
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
    return ToString(0);
  }
  public string ToString(int indent) {
    string res = "";
    string id = "";
    for (int i = 0; i < indent; i++) id += " ";

    try {
      switch (type) {
        case BNF.Program: {
          res = "Program:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1) + "\n";
        }
        break;
        case BNF.Start: {
          res = "Start:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1) + "\n";
        }
        break;
        case BNF.Update: {
          res = "Update:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1) + "\n";
        }
        break;
        case BNF.Functions: {
          res = "Functions:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1) + "\n";
        }
        break;
        case BNF.Config: {
          res = "Config:\n";
          int num = children == null ? 0 : children.Count;
          for (int i = 0; i < num; i++) {
            res += children[i].ToString(indent + 1);
          }
        }
        break;
        case BNF.Data: {
          res = "Data:\n";
          int num = children == null ? 0 : children.Count;
          for (int i = 0; i < num; i++) {
            res += children[i].ToString(indent + 1);
          }
        }
        break;
        case BNF.REG: res += id + " R" + Reg; break;
        case BNF.INT: res += id + " " + iVal; break;
        case BNF.COLOR: res += id + Col.GetColorString(iVal) + "c"; break;
        case BNF.PAL: res += id + iVal + "p"; break;
        case BNF.FLT: res += id + " " + fVal; break;
        case BNF.STR: res += id + " \"" + sVal + "\""; break;
        case BNF.MEM: res += id + " [" + CN1?.ToString(indent + 1) + "]"; break;
        case BNF.MEMlong: res += id + " [" + CN1?.ToString(indent + 1) + "@]"; break;
        case BNF.MEMlongb: res += id + " [" + CN1?.ToString(indent + 1) + "@b]"; break;
        case BNF.MEMlongi: res += id + " [" + CN1?.ToString(indent + 1) + "@i]"; break;
        case BNF.MEMlongf: res += id + " [" + CN1?.ToString(indent + 1) + "@f]"; break;
        case BNF.MEMlongs: res += id + " [" + CN1?.ToString(indent + 1) + "@s]"; break;
        case BNF.MEMchar: res += id + " [" + CN1?.ToString(indent + 1) + "@c]"; break;

        case BNF.ARRAY: res += id + " R" + Reg + "[" + CN1?.ToString(indent + 1) + "]"; break;

        case BNF.OPpar:
          res += "(" + CN1?.ToString(indent + 1) + ")";
          break;
        case BNF.OPsum: res += "(" + CN1?.ToString(indent + 1) + "+" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPsub: res += "(" + CN1?.ToString(indent + 1) + "-" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPmul: res += "(" + CN1?.ToString(indent + 1) + "*" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPdiv: res += "(" + CN1?.ToString(indent + 1) + "/" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPmod: res += "(" + CN1?.ToString(indent + 1) + "%" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPand: res += "(" + CN1?.ToString(indent + 1) + "&" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPor: res += "(" + CN1?.ToString(indent + 1) + "|" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPxor: res += "(" + CN1?.ToString(indent + 1) + "^" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPlsh: res += "(" + CN1?.ToString(indent + 1) + "<<" + CN2?.ToString(indent + 1) + ")"; break;
        case BNF.OPrsh: res += "(" + CN1?.ToString(indent + 1) + ">>" + CN2?.ToString(indent + 1) + ")"; break;

        case BNF.ASSIGN: res += CN1?.ToString(indent + 1) + " = " + CN2?.ToString(indent + 1); break;
        case BNF.ASSIGNsum: res += CN1?.ToString(indent + 1) + " += " + CN2?.ToString(indent + 1); break;
        case BNF.ASSIGNsub: res += CN1?.ToString(indent + 1) + " -= " + CN2?.ToString(indent + 1); break;
        case BNF.ASSIGNmul: res += CN1?.ToString(indent + 1) + " *= " + CN2?.ToString(indent + 1); break;
        case BNF.ASSIGNdiv: res += CN1?.ToString(indent + 1) + " /= " + CN2?.ToString(indent + 1); break;
        case BNF.ASSIGNand: res += CN1?.ToString(indent + 1) + " &= " + CN2?.ToString(indent + 1); break;
        case BNF.ASSIGNmod: res += CN1?.ToString(indent + 1) + " %= " + CN2?.ToString(indent + 1); break;
        case BNF.ASSIGNor: res += CN1?.ToString(indent + 1) + " |= " + CN2?.ToString(indent + 1); break;
        case BNF.ASSIGNxor: res += CN1?.ToString(indent + 1) + " ^= " + CN2?.ToString(indent + 1); break;

        case BNF.UOneg: res += id + " !" + CN1?.ToString(indent); break;
        case BNF.UOinv: res += id + " ~" + CN1?.ToString(indent); break;
        case BNF.UOsub: res += id + " -" + CN1?.ToString(indent); break;

        case BNF.CASTb: res += id + CN1?.ToString(indent) + "_b"; break;
        case BNF.CASTi: res += id + CN1?.ToString(indent) + "_i"; break;
        case BNF.CASTf: res += id + CN1?.ToString(indent) + "_f"; break;
        case BNF.CASTs: res += id + CN1?.ToString(indent) + "_s"; break;

        case BNF.LEN: res += CN1?.ToString(indent) + ".len"; break;
        case BNF.PLEN: res += CN1?.ToString(indent) + ".plen"; break;
        case BNF.CLR: res += id + "Clr(" + CN1?.ToString(indent) + ")"; break;
        case BNF.DTIME: res += "dateTime"; break;

        case BNF.LUMA: return id + "Luma(" + CN1?.ToString(indent) + ")";
        case BNF.CONTRAST: return id + "Contrast(" + CN1?.ToString(indent) + ")";

        case BNF.WRITE: {
          res += id + "Write(" +
            CN1.ToString(indent) + ", " +
            CN2.ToString(indent) + ", " +
            CN3.ToString(indent) + ", " +
            CN4.ToString(indent);
          if (children.Count > 4) res += ", " + CN5.ToString(indent);
          res += ")";
        }
        break;

        case BNF.WAIT: {
          res += id + "Wait(" + CN1.ToString(indent) + ")";
        }
        break;

        case BNF.LINE:
          res += id + "line(" +
            CN1.ToString(indent) + ", " +
            CN2.ToString(indent) + ", " +
            CN3.ToString(indent) + ", " +
            CN4.ToString(indent) + ", " +
            CN5.ToString(indent) + ")";
          break;

        case BNF.BOX:
          res += id + "line(" +
            CN1.ToString(indent) + ", " +
            CN2.ToString(indent) + ", " +
            CN3.ToString(indent) + ", " +
            CN4.ToString(indent) + ", " +
            CN5.ToString(indent);
          if (children.Count > 5)
            res += CN6.ToString(indent) + ")";
          else
            res += ")";
          break;

        case BNF.CIRCLE:
          res += id + "circle(" +
            CN1.ToString(indent) + ", " +
            CN2.ToString(indent) + ", " +
            CN3.ToString(indent) + ", " +
            CN4.ToString(indent) + ", " +
            CN5.ToString(indent);
          if (children.Count > 5)
            res += CN6.ToString(indent) + ")";
          else
            res += ")";
          break;

        case BNF.Inc: res += id + CN1.ToString(indent) + "++"; break;
        case BNF.Dec: res += id + CN1.ToString(indent) + "--"; break;

        case BNF.COMPeq: res += "(" + CN1.ToString(indent + 1) + "==" + CN2.ToString(indent + 1) + ")"; break;
        case BNF.COMPne: res += "(" + CN1.ToString(indent + 1) + "!=" + CN2.ToString(indent + 1) + ")"; break;
        case BNF.COMPlt: res += "(" + CN1.ToString(indent + 1) + "<" + CN2.ToString(indent + 1) + ")"; break;
        case BNF.COMPle: res += "(" + CN1.ToString(indent + 1) + "<=" + CN2.ToString(indent + 1) + ")"; break;
        case BNF.COMPgt: res += "(" + CN1.ToString(indent + 1) + ">" + CN2.ToString(indent + 1) + ")"; break;
        case BNF.COMPge: res += "(" + CN1.ToString(indent + 1) + ">=" + CN2.ToString(indent + 1) + ")"; break;

        case BNF.IF: {
          res += id + "if (" + CN1.ToString(indent) + ") { ..." + CN2?.ToString(indent) + "... }";
          if (CN3 != null) res += id + " else { ..." + CN3.ToString(indent) + "... }";
          break;
        }
        case BNF.Else: res += id + "else { ..." + (children.Count - 1) + "... }"; break;
        case BNF.WHILE: res += id + "while (" + CN1.ToString(indent) + ") { ..." + (children.Count - 1) + "... }"; break;

        case BNF.SCREEN: {
          res += id + "screen(" +
            CN1.ToString(indent) + ", " +
            CN2.ToString(indent);
          if (children.Count > 2) res += ", " + CN3.ToString(indent);
          if (children.Count > 3) res += ", " + CN4.ToString(indent);
          res += ")";
        }
        break;

        case BNF.BLOCK: {
          res = "{ ";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1) + "\n";
          res += " }";
        }
        break;

        case BNF.ScrConfig:
          res += id + "screencfg(" +
                  (int)fVal + ", " + iVal +
                  (sVal == "*" ? ",f" : "") +
                  ")";
          break;
        case BNF.Ram: res += id + "ram(" + iVal + ")"; break;
        case BNF.Rom: res += id + "rom(" + iVal + ")"; break;
        case BNF.PaletteConfig: res += id + "Palette(" + iVal + ")"; break;

        case BNF.Label:
        case BNF.LAB: res += id + "[" + sVal + iVal + "]"; break;
        case BNF.LABG: res += id + "Label(" + CN1.ToString(indent) + ")"; break;

        case BNF.FRAME: res += id + "frame"; break;

        case BNF.KEY: res += id + "key" + "LLLRRRUUUDDDAAABBBCCCFFFEEE"[iVal] + (iVal % 3 == 1 ? "u" : (iVal % 3 == 2 ? "d" : "")); break;
        case BNF.KEYx: res += id + "keyX"; break;
        case BNF.KEYy: res += id + "keyX"; break;

        case BNF.FOR: {
          return id + "for(" +
            CN1.ToString(indent) + ", " +
            CN2.ToString(indent) + ", ..." + ")" + "\n" +
            CN3.ToString(indent + 1);
        }

        case BNF.SPRITE: {
          res += id + "sprite(";
          res += CN1.ToString(indent) + ", " + CN2.ToString(indent);
          if (CN3 != null) res += ", " + CN3.ToString(indent);
          if (CN4 != null) res += ", " + CN4.ToString(indent);
          if (CN5 != null) res += ", " + CN5.ToString(indent);
          res += ")";
        }
        break;
        case BNF.DESTROY: return id + "destroy(" + CN1.ToString(indent) + ")";
        case BNF.SPOS:
          return id + "SPos(" +
                  CN1.ToString(indent) + ", " + CN2.ToString(indent) + ", " + CN3.ToString(indent) +
                  (CN4 != null ? ", " + CN4.ToString(indent) : "") + ")";
        case BNF.SROT: return id + "SRot(" + CN1.ToString(indent) + ", " + CN2.ToString(indent) + ", " + CN3.ToString(indent) + ")";
        case BNF.SPEN: return id + "SPEn(" + CN1.ToString(indent) + ", " + CN2.ToString(indent) + ")";
        case BNF.STINT: return id + "STint(" + CN1.ToString(indent) + ", " + CN2.ToString(indent) + ")";
        case BNF.SSCALE: return id + "SScale(" + CN1.ToString(indent) + ", " + CN2.ToString(indent) + ", " + CN3.ToString(indent) + ")";
        case BNF.SPRI: return id + "SPri(" + CN1.ToString(indent) + ", " + CN2.ToString(indent) + ")";

        case BNF.FunctionDef:
          return id + sVal + (CN1 == null ? "()" : CN1.ToString(indent)) + " {" + (CN2 == null ? "" : (CN2.children == null ? CN2.ToString(indent) : CN2.children.Count.ToString())) + "}";

        case BNF.FunctionCall: return id + sVal + (CN1 == null ? "()" : CN1.ToString(indent));

        case BNF.Params: {
          if (children == null) return "()";
          string pars = "(";
          for (int i = 0; i < children.Count; i++) {
            if (i > 0) pars += ", ";
            pars += children[i].ToString(indent);
          }
          return pars + ")";
        }

        case BNF.RETURN: return id + "return " + (CN1 == null ? "" : CN1.ToString(indent));

        case BNF.NOP: return "";
        case BNF.SETP: return "SetPixel(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) + ")";
        case BNF.GETP: return "GetPixel(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";
        case BNF.SIN: return "Sin(" + CN1?.ToString(indent) + ")";
        case BNF.COS: return "Cos(" + CN1?.ToString(indent) + ")";
        case BNF.TAN: return "Tan(" + CN1?.ToString(indent) + ")";
        case BNF.ATAN2: return "Atan2(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";
        case BNF.SQR: return "Sqrt(" + CN1?.ToString(indent) + ")";
        case BNF.POW: return "Pow(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";
        case BNF.SUBSTRING: return "SubString(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";
        case BNF.TRIM: return "Trim()";
        case BNF.SOUND:
          if (CN3 == null) return "Sound(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";
          else return "Sound(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) + ")";
        case BNF.WAVE:
          if (children.Count == 2) return "Wave(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";
          else return "Wave(" +
              CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " +
              CN3?.ToString(indent) + ", " + CN4?.ToString(indent) + ", " +
              CN5?.ToString(indent) + ", " + CN6?.ToString(indent) + ")";
        case BNF.MUTE: return "Mute(" + CN1?.ToString(indent) + ")";
        case BNF.VOLUME:
          if (CN2 == null) return "Volume(" + CN1?.ToString(indent) + ")";
          else return "Volume(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";
        case BNF.PITCH: return "Pitch(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";
        case BNF.PAN: return "Pan(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ")";

        case BNF.MUSICLOAD: return "LoadMusic(" + CN1?.ToString(indent) + ")";
        case BNF.MUSICPLAY: return "PlayMusic(" + CN1?.ToString(indent) + ")";
        case BNF.MUSICSTOP: return "StopMusic()";
        case BNF.MUSICPOS: return "MusicPos";
        case BNF.MUSICVOICES: {
          res = "MusicVoices(";
          if (children.Count > 0) res += CN1?.ToString(indent);
          if (children.Count > 1) res += ", " + CN2?.ToString(indent);
          if (children.Count > 2) res += ", " + CN3?.ToString(indent);
          if (children.Count > 3) res += ", " + CN4?.ToString(indent);
          if (children.Count > 4) res += ", " + CN5?.ToString(indent);
          if (children.Count > 5) res += ", " + CN6?.ToString(indent);
          if (children.Count > 6) res += ", " + CN7?.ToString(indent);
          if (children.Count > 7) res += ", " + CN8?.ToString(indent);
          res += ")";
          return res;
        }

        case BNF.TILEMAP: return "TileMap(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) + ")";
        case BNF.TILEPOS:
          return "TilePos(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) +
          (CN4 == null ? "" : (", " + CN4.ToString(indent) +
          (CN5 == null ? "" : ", " + CN5.ToString(indent))
          )) + ")";

        case BNF.TILESET:
          return "TileSet(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) +
                ", " + CN4.ToString(indent) + (CN5 == null ? "" : ", " + CN5.ToString(indent)) + ")";

        case BNF.TILEGET: return "TileGet(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) + ")";
        case BNF.TILEGETROT: return "TileGetRot(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) + ")";

        case BNF.IMAGE: {
          res = "Image(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) + ", " + CN4?.ToString(indent) + ", " + CN5?.ToString(indent);
          if (children.Count > 5) res += ", " + CN6?.ToString(indent) + ", " + CN7?.ToString(indent);
          res += ")";
          return res;
        }

        case BNF.PALETTE: return "UsePalette(" + CN1?.ToString(indent) + ")";
        case BNF.SETPALETTECOLOR: return "SetPaletteColor(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) + ", " + CN4?.ToString(indent) + ", " + CN5?.ToString(indent) + ", " + ")";

        case BNF.MEMCPY: return "MemCpy(" + CN1?.ToString(indent) + ", " + CN2?.ToString(indent) + ", " + CN3?.ToString(indent) + ")";

        default:
          res += "[[Missing:" + type + "]]";
          break;
      }
    } catch (Exception e) {
      res += "[[INVALID!!]]" + e.Message;
    }

    return res.Replace("  ", " ");
  }

  internal string Format(Variables variables) {
    switch (type) {
      case BNF.Program: return "<color=#8080ff>Program</color> (<color=#ff6060>INVALID</color>)";
      case BNF.Start: return "<color=#8080ff>Start</color> {";
      case BNF.Update: return "<color=#8080ff>Update</color> {";
      case BNF.Config: return "<color=#8080ff>Config</color> {";
      case BNF.Data: return "<color=#8080ff>Data</color> {";
      case BNF.Functions:
        break;
      case BNF.FunctionDef:
        break;
      case BNF.FunctionCall:
        break;
      case BNF.RETURN:
        break;
      case BNF.Params:
        break;
      case BNF.ScrConfig:
        break;
      case BNF.PaletteConfig:
        break;
      case BNF.Ram:
        break;
      case BNF.Rom:
        break;
      case BNF.Label:
        break;
      case BNF.REG: return "<color=#f6fC06>" + variables.GetRegName(Reg) + "</color>";
      case BNF.ARRAY:
        break;
      case BNF.INT: return "<color=#B5CEA8>" + iVal + "</color>";
      case BNF.FLT: return "<color=#B5CEA8>" + fVal + "</color>";
      case BNF.COLOR: {
        UnityEngine.Color32 c = Col.GetColor((byte)iVal);
        return "<mark=#" + c.r.ToString("x2") + c.g.ToString("x2") + c.b.ToString("x2") + "80>" + Col.GetColorString(iVal) + "c</mark>";
      }
      case BNF.PAL:
        break;
      case BNF.LUMA:
        break;
      case BNF.CONTRAST:
        break;
      case BNF.STR: return "<color=#CA9581><mark=#1A151140>\"" + sVal + "\"</mark></color>";
      case BNF.MEM:
        break;
      case BNF.MEMlong:
        break;
      case BNF.MEMlongb:
        break;
      case BNF.MEMlongi:
        break;
      case BNF.MEMlongf:
        break;
      case BNF.MEMlongs:
        break;
      case BNF.MEMchar:
        break;
      case BNF.OPpar: return "<color=#66aCe6>(</color>" + CN1?.Format(variables) + "<color=#66aCe6>)</color>";
      case BNF.OPsum: return CN1?.Format(variables) + " <color=#66aCe6>+</color> " + CN2?.Format(variables);
      case BNF.OPsub: return CN1?.Format(variables) + " <color=#66aCe6>-</color> " + CN2?.Format(variables);
      case BNF.OPmul: return CN1?.Format(variables) + " <color=#66aCe6>*</color> " + CN2?.Format(variables);
      case BNF.OPdiv: return CN1?.Format(variables) + " <color=#66aCe6>/</color> " + CN2?.Format(variables);
      case BNF.OPmod: return CN1?.Format(variables) + " <color=#66aCe6>%</color> " + CN2?.Format(variables);
      case BNF.OPand: return CN1?.Format(variables) + " <color=#66aCe6>&</color> " + CN2?.Format(variables);
      case BNF.OPor:  return CN1?.Format(variables) + " <color=#66aCe6>|</color> " + CN2?.Format(variables);
      case BNF.OPxor: return CN1?.Format(variables) + " <color=#66aCe6>^</color> " + CN2?.Format(variables);
      case BNF.OPlsh: return CN1?.Format(variables) + " <color=#66aCe6><<</color> " + CN2?.Format(variables);
      case BNF.OPrsh: return CN1?.Format(variables) + " <color=#66aCe6>>></color> " + CN2?.Format(variables);
      case BNF.LAB:
        break;
      case BNF.LABG:
        break;
      case BNF.CASTb:
        break;
      case BNF.CASTi:
        break;
      case BNF.CASTf:
        break;
      case BNF.CASTs:
        break;
      case BNF.UOneg:
        break;
      case BNF.UOinv:
        break;
      case BNF.UOsub:
        break;
      case BNF.COMPeq:
        break;
      case BNF.COMPne:
        break;
      case BNF.COMPlt:
        break;
      case BNF.COMPle:
        break;
      case BNF.COMPgt:
        break;
      case BNF.COMPge:
        break;
      case BNF.ASSIGN: return CN1?.Format(variables) + " = " + CN2?.Format(variables);
      case BNF.ASSIGNsum: return CN1?.Format(variables) + " += " + CN2?.Format(variables);
      case BNF.ASSIGNsub: return CN1?.Format(variables) + " -= " + CN2?.Format(variables);
      case BNF.ASSIGNmul: return CN1?.Format(variables) + " *= " + CN2?.Format(variables);
      case BNF.ASSIGNdiv: return CN1?.Format(variables) + " /= " + CN2?.Format(variables);
      case BNF.ASSIGNmod: return CN1?.Format(variables) + " %= " + CN2?.Format(variables);
      case BNF.ASSIGNand: return CN1?.Format(variables) + " &= " + CN2?.Format(variables);
      case BNF.ASSIGNor:  return CN1?.Format(variables) + " |= " + CN2?.Format(variables);
      case BNF.ASSIGNxor: return CN1?.Format(variables) + " ^= " + CN2?.Format(variables);
      case BNF.Inc: return CN1?.Format(variables) + "++";
      case BNF.Dec: return CN1?.Format(variables) + "--";
      case BNF.BLOCK:
        break;
      case BNF.IF:
        break;
      case BNF.Else:
        break;
      case BNF.WHILE:
        break;
      case BNF.FOR:
        break;
      case BNF.CLR: return "<color=#569CD6>Clr(</color>" + CN1?.Format(variables) + "<color=#569CD6>)</color>";
      case BNF.WRITE:
        break;
      case BNF.WAIT: return "<color=#569CD6>Wait(</color>" + CN1?.Format(variables) + "<color=#569CD6>)</color>";
      case BNF.DESTROY:
        break;
      case BNF.SCREEN:
        break;
      case BNF.SPRITE:
        break;
      case BNF.SPEN:
        break;
      case BNF.SPOS:
        break;
      case BNF.SROT:
        break;
      case BNF.SPRI:
        break;
      case BNF.STINT:
        break;
      case BNF.SSCALE:
        break;
      case BNF.SETP:
        break;
      case BNF.GETP:
        break;
      case BNF.LINE:
        break;
      case BNF.BOX:
        break;
      case BNF.CIRCLE:
        break;
      case BNF.IMAGE:
        break;
      case BNF.FRAME:
        break;
      case BNF.DTIME:
        break;
      case BNF.LEN:
        break;
      case BNF.PLEN:
        break;
      case BNF.SUBSTRING:
        break;
      case BNF.TRIM:
        break;
      case BNF.KEY:
        break;
      case BNF.KEYx:
        break;
      case BNF.KEYy:
        break;
      case BNF.SIN:
        break;
      case BNF.COS:
        break;
      case BNF.TAN:
        break;
      case BNF.ATAN2:
        break;
      case BNF.SQR:
        break;
      case BNF.POW:
        break;
      case BNF.MEMCPY:
        break;
      case BNF.SOUND:
        break;
      case BNF.WAVE:
        break;
      case BNF.MUTE:
        break;
      case BNF.VOLUME:
        break;
      case BNF.PITCH:
        break;
      case BNF.PAN:
        break;
      case BNF.MUSICLOAD:
        break;
      case BNF.MUSICPLAY:
        break;
      case BNF.MUSICSTOP:
        break;
      case BNF.MUSICPOS:
        break;
      case BNF.MUSICVOICES:
        break;
      case BNF.TILEMAP:
        break;
      case BNF.TILEPOS:
        break;
      case BNF.TILESET:
        break;
      case BNF.TILEGET:
        break;
      case BNF.TILEGETROT:
        break;
      case BNF.NOP:
        break;
      case BNF.PALETTE:
        break;
      case BNF.SETPALETTECOLOR:
        break;
    }
    return "NOT DONE! " + type;
  }

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
      case BNF.OPand:
      case BNF.OPor:
      case BNF.OPxor:
      case BNF.OPlsh:
      case BNF.OPrsh:
      case BNF.Label:
      case BNF.LAB:
      case BNF.LABG:
      case BNF.UOneg:
      case BNF.UOinv:
      case BNF.UOsub:
      case BNF.Inc:
      case BNF.Dec:
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
  ScrConfig,
  PaletteConfig,
  Ram,
  Rom, // This is used to store the data
  Label, // This is used to parse the Rom section (Data)
  REG,
  ARRAY,
  INT, 
  FLT,
  COLOR, PAL, LUMA, CONTRAST,
  STR,
  MEM, MEMlong, MEMlongb, MEMlongi, MEMlongf, MEMlongs, MEMchar,
  OPpar, OPsum, OPsub, OPmul, OPdiv, OPmod, OPand, OPor, OPxor, OPlsh, OPrsh,
  LAB, // This is used to reference some data, just a literal for an address in memory
  LABG, // This is a function to calculate a label from a string
  CASTb, CASTi, CASTf, CASTs,
  UOneg, UOinv, UOsub,
  COMPeq, COMPne, COMPlt, COMPle, COMPgt, COMPge,
  ASSIGN, ASSIGNsum, ASSIGNsub, ASSIGNmul, ASSIGNdiv, ASSIGNmod, ASSIGNand, ASSIGNor, ASSIGNxor,
  Inc, Dec,
  BLOCK,
  IF, Else, WHILE, FOR,
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
  
  
  SOUND, WAVE, MUTE, VOLUME, PITCH, PAN,
  MUSICLOAD, MUSICPLAY, MUSICSTOP, MUSICPOS, MUSICVOICES,
  TILEMAP, TILEPOS, TILESET, TILEGET, TILEGETROT,
  NOP,
  PALETTE,
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
