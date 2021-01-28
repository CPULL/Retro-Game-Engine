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
  public VT valType;
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
    return ToString(0, false);
  }
  public string ToString(int indent, bool sameLine) {
    string res = "";
    string id = "";
    for (int i = 0; !sameLine && i < indent; i++) id += " ";

    try {
      switch (type) {
        case BNF.Program: {
          res = "Program:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1, false) + "\n";
        }
        break;
        case BNF.Start: {
          res = "Start:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1, false) + "\n";
        }
        break;
        case BNF.Update: {
          res = "Update:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1, false) + "\n";
        }
        break;
        case BNF.Functions: {
          res = "Functions:\n";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1, false) + "\n";
        }
        break;
        case BNF.Data: {
          res = "Data:\n";
          int num = children == null ? 0 : children.Count;
          for (int i = 0; i < num; i++) {
            res += children[i].ToString(indent + 1, false);
          }
        }
        break;
        case BNF.REG: res += (sameLine ? "" : id) + " R" + Reg + (sameLine ? " " : "\n"); break;
        case BNF.INT: res += (sameLine ? "" : id) + " " + iVal + (sameLine ? " " : "\n"); break;
        case BNF.COLOR:
          res += (sameLine ? "" : id) + " c" + Col.GetColorString(iVal) + (sameLine ? " " : "\n"); break;
        case BNF.HEX: res += (sameLine ? "" : id) + " x" + iVal.ToString("X") + (sameLine ? " " : "\n"); break;
        case BNF.FLT: res += (sameLine ? "" : id) + " " + fVal + (sameLine ? " " : "\n"); break;
        case BNF.STR: res += (sameLine ? "" : id) + " \"" + sVal + (sameLine ? "\" " : "\"\n"); break;
        case BNF.MEM: res += (sameLine ? "" : id) + " [" + CN1.ToString(indent + 1, true) + "]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlong: res += (sameLine ? "" : id) + " [" + CN1.ToString(indent + 1, true) + "@]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlongb: res += (sameLine ? "" : id) + " [" + CN1.ToString(indent + 1, true) + "@b]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlongi: res += (sameLine ? "" : id) + " [" + CN1.ToString(indent + 1, true) + "@i]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlongf: res += (sameLine ? "" : id) + " [" + CN1.ToString(indent + 1, true) + "@f]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlongs: res += (sameLine ? "" : id) + " [" + CN1.ToString(indent + 1, true) + "@s]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMchar: res += (sameLine ? "" : id) + " [" + CN1.ToString(indent + 1, true) + "@c]" + (sameLine ? " " : "\n"); break;

        case BNF.OPpar:
          res += "(" + CN1.ToString(indent + 1, true) + ")";
          break;
        case BNF.OPsum: res += "(" + CN1.ToString(indent + 1, true) + "+" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPsub: res += "(" + CN1.ToString(indent + 1, true) + "-" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPmul: res += "(" + CN1.ToString(indent + 1, true) + "*" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPdiv: res += "(" + CN1.ToString(indent + 1, true) + "/" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPmod: res += "(" + CN1.ToString(indent + 1, true) + "%" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPand: res += "(" + CN1.ToString(indent + 1, true) + "&" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPor: res += "(" + CN1.ToString(indent + 1, true) + "|" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPxor: res += "(" + CN1.ToString(indent + 1, true) + "^" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPlsh: res += "(" + CN1.ToString(indent + 1, true) + "<<" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.OPrsh: res += "(" + CN1.ToString(indent + 1, true) + ">>" + CN2.ToString(indent + 1, true) + ")"; break;

        case BNF.ASSIGN: res += CN1.ToString(indent + 1, true) + " = " + CN2.ToString(indent + 1, true); break;
        case BNF.ASSIGNsum: res += CN1.ToString(indent + 1, true) + " += " + CN2.ToString(indent + 1, true); break;
        case BNF.ASSIGNsub: res += CN1.ToString(indent + 1, true) + " -= " + CN2.ToString(indent + 1, true); break;
        case BNF.ASSIGNmul: res += CN1.ToString(indent + 1, true) + " *= " + CN2.ToString(indent + 1, true); break;
        case BNF.ASSIGNdiv: res += CN1.ToString(indent + 1, true) + " /= " + CN2.ToString(indent + 1, true); break;
        case BNF.ASSIGNmod: res += CN1.ToString(indent + 1, true) + " %= " + CN2.ToString(indent + 1, true); break;
        case BNF.ASSIGNand: res += CN1.ToString(indent + 1, true) + " &= " + CN2.ToString(indent + 1, true); break;
        case BNF.ASSIGNor: res += CN1.ToString(indent + 1, true) + " |= " + CN2.ToString(indent + 1, true); break;
        case BNF.ASSIGNxor: res += CN1.ToString(indent + 1, true) + " ^= " + CN2.ToString(indent + 1, true); break;

        case BNF.UOneg: res += (sameLine ? "" : id) + " !" + CN1.ToString(indent, true); break;
        case BNF.UOinv: res += (sameLine ? "" : id) + " ~" + CN1.ToString(indent, true); break;
        case BNF.UOsub: res += (sameLine ? "" : id) + " -" + CN1.ToString(indent, true); break;

        case BNF.CASTb: res += (sameLine ? "" : id) + CN1.ToString(indent, true) + "_b"; break;
        case BNF.CASTi: res += (sameLine ? "" : id) + CN1.ToString(indent, true) + "_i"; break;
        case BNF.CASTf: res += (sameLine ? "" : id) + CN1.ToString(indent, true) + "_f"; break;
        case BNF.CASTs: res += (sameLine ? "" : id) + CN1.ToString(indent, true) + "_s"; break;

        case BNF.LEN: res += CN1.ToString(indent, true) + ".len"; break;
        case BNF.PLEN: res += CN1.ToString(indent, true) + ".plen"; break;
        case BNF.CLR: res += (sameLine ? "" : id) + "Clr(" + CN1.ToString(indent, true) + ")"; break;
        case BNF.DTIME: res += "dateTime"; break;

        case BNF.WRITE: {
          res += (sameLine ? "" : id) + "Write(" +
            CN1.ToString(indent, true) + ", " +
            CN2.ToString(indent, true) + ", " +
            CN3.ToString(indent, true) + ", " +
            CN4.ToString(indent, true);
          if (children.Count > 4) res += ", " + CN5.ToString(indent, true);
          res += ")";
        }
        break;

        case BNF.WAIT: {
          res += (sameLine ? "" : id) + "Wait(" + CN1.ToString(indent, true) + ")";
        }
        break;

        case BNF.LINE:
          res += (sameLine ? "" : id) + "line(" +
            CN1.ToString(indent, true) + ", " +
            CN2.ToString(indent, true) + ", " +
            CN3.ToString(indent, true) + ", " +
            CN4.ToString(indent, true) + ", " +
            CN5.ToString(indent, true) + ")";
          break;

        case BNF.BOX:
          res += (sameLine ? "" : id) + "line(" +
            CN1.ToString(indent, true) + ", " +
            CN2.ToString(indent, true) + ", " +
            CN3.ToString(indent, true) + ", " +
            CN4.ToString(indent, true) + ", " +
            CN5.ToString(indent, true);
          if (children.Count > 5)
            res += CN6.ToString(indent, true) + ")";
          else
            res += ")";
          break;

        case BNF.CIRCLE:
          res += (sameLine ? "" : id) + "circle(" +
            CN1.ToString(indent, true) + ", " +
            CN2.ToString(indent, true) + ", " +
            CN3.ToString(indent, true) + ", " +
            CN4.ToString(indent, true) + ", " +
            CN5.ToString(indent, true);
          if (children.Count > 5)
            res += CN6.ToString(indent, true) + ")";
          else
            res += ")";
          break;

        case BNF.Inc: res += (sameLine ? "" : id) + CN1.ToString(indent, true) + "++"; break;
        case BNF.Dec: res += (sameLine ? "" : id) + CN1.ToString(indent, true) + "--"; break;

        case BNF.COMPeq: res += "(" + CN1.ToString(indent + 1, true) + "==" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.COMPne: res += "(" + CN1.ToString(indent + 1, true) + "!=" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.COMPlt: res += "(" + CN1.ToString(indent + 1, true) + "<" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.COMPle: res += "(" + CN1.ToString(indent + 1, true) + "<=" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.COMPgt: res += "(" + CN1.ToString(indent + 1, true) + ">" + CN2.ToString(indent + 1, true) + ")"; break;
        case BNF.COMPge: res += "(" + CN1.ToString(indent + 1, true) + ">=" + CN2.ToString(indent + 1, true) + ")"; break;

        case BNF.IF: res += (sameLine ? "" : id) + "if (" + CN1.ToString(indent, true) + ") { ..." + (children.Count - 1) + "... }"; break;
        case BNF.Else: res += (sameLine ? "" : id) + "else { ..." + (children.Count - 1) + "... }"; break;
        case BNF.WHILE: res += (sameLine ? "" : id) + "while (" + CN1.ToString(indent, true) + ") { ..." + (children.Count - 1) + "... }"; break;

        case BNF.SCREEN: {
          res += (sameLine ? "" : id) + "screen(" +
            CN1.ToString(indent, true) + ", " +
            CN2.ToString(indent, true);
          if (children.Count > 2) res += ", " + CN3.ToString(indent, true);
          if (children.Count > 3) res += ", " + CN4.ToString(indent, true);
          res += ")";
        }
        break;

        case BNF.BLOCK: {
          res = "{ ";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1, false) + "\n";
          res += " }";
        }
        break;

        case BNF.ScrConfig:
          res += (sameLine ? "" : id) + "screencfg(" +
                  (int)fVal + ", " + iVal +
                  (sVal == "*" ? ",f" : "") +
                  ")";
          break;
        case BNF.Ram: res += (sameLine ? "" : id) + "ram(" + iVal + ")"; break;
        case BNF.Rom: res += (sameLine ? "" : id) + "rom(" + iVal + ")"; break;

        case BNF.Label:
        case BNF.LAB: res += (sameLine ? "" : id) + "[" + sVal + iVal + "]"; break;

        case BNF.FRAME: res += (sameLine ? "" : id) + "frame"; break;

        case BNF.KEY: res += (sameLine ? "" : id) + "key" + "LLLRRRUUUDDDAAABBBCCCFFFEEE"[iVal] + (iVal % 3 == 1 ? "u" : (iVal % 3 == 2 ? "d" : "")); break;
        case BNF.KEYx: res += (sameLine ? "" : id) + "keyX"; break;
        case BNF.KEYy: res += (sameLine ? "" : id) + "keyX"; break;

        case BNF.FOR: {
          return (sameLine ? "" : id) + "for(" +
            CN1.ToString(indent, true) + ", " +
            CN2.ToString(indent, true) + ", ..." + ")" + (sameLine ? "" : "\n") +
            CN3.ToString(indent + 1, true);
        }

        case BNF.SPRITE: {
          res += (sameLine ? "" : id) + "sprite(";
          res += CN1.ToString(indent, true) + ", " + CN2.ToString(indent, true);
          if (CN3 != null) res += ", " + CN3.ToString(indent, true);
          if (CN4 != null) res += ", " + CN4.ToString(indent, true);
          if (CN5 != null) res += ", " + CN5.ToString(indent, true);
          res += ")";
        }
        break;
        case BNF.DESTROY: return (sameLine ? "" : id) + "destroy(" + CN1.ToString(indent, true) + ")";
        case BNF.SPOS:
          return (sameLine ? "" : id) + "SPos(" +
                  CN1.ToString(indent, true) + ", " + CN2.ToString(indent, true) + ", " + CN3.ToString(indent, true) +
                  (CN4 != null ? ", " + CN4.ToString(indent, true) : "") + ")";
        case BNF.SROT: return (sameLine ? "" : id) + "SRot(" + CN1.ToString(indent, true) + ", " + CN2.ToString(indent, true) + ", " + CN3.ToString(indent, true) + ")";
        case BNF.SPEN: return (sameLine ? "" : id) + "SPEn(" + CN1.ToString(indent, true) + ", " + CN2.ToString(indent, true) + ")";
        case BNF.STINT: return (sameLine ? "" : id) + "STint(" + CN1.ToString(indent, true) + ", " + CN2.ToString(indent, true) + ")";
        case BNF.SSCALE: return (sameLine ? "" : id) + "SScale(" + CN1.ToString(indent, true) + ", " + CN2.ToString(indent, true) + ", " + CN3.ToString(indent, true) + ")";

        case BNF.FunctionDef:
          return (sameLine ? "" : id) + sVal + (CN1 == null ? "()" : CN1.ToString(indent, true)) + " {" + (CN2 == null ? "" : (CN2.children == null ? CN2.ToString(indent, true) : CN2.children.Count.ToString())) + "}";

        case BNF.FunctionCall: return (sameLine ? "" : id) + sVal + (CN1 == null ? "()" : CN1.ToString(indent, true));

        case BNF.Params: {
          if (children == null) return "()";
          string pars = "(";
          for (int i = 0; i < children.Count; i++) {
            if (i > 0) pars += ", ";
            pars += children[i].ToString(indent, true);
          }
          return pars + ")";
        }

        case BNF.RETURN: return (sameLine ? "" : id) + "return " + (CN1 == null ? "" : CN1.ToString(indent, true));

        case BNF.NOP: return "";
        case BNF.SETP: return "SetPixel(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ", " + CN3?.ToString(indent, true) + ")";
        case BNF.GETP: return "GetPixel(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";
        case BNF.SIN: return "Sin(" + CN1?.ToString(indent, true) + ")";
        case BNF.COS: return "Cos(" + CN1?.ToString(indent, true) + ")";
        case BNF.TAN: return "Tan(" + CN1?.ToString(indent, true) + ")";
        case BNF.ATAN2: return "Atan2(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";
        case BNF.SQR: return "Sqrt(" + CN1?.ToString(indent, true) + ")";
        case BNF.POW: return "Pow(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";
        case BNF.SUBSTRING: return "SubString(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";
        case BNF.TRIM: return "Trim()";
        case BNF.SOUND:
          if (CN3 == null) return "Sound("+ CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";
          else return "Sound(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ", " + CN3?.ToString(indent, true) + ")";
        case BNF.WAVE:
          if (children.Count == 2) return "Wave(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";
          else return "Wave(" + 
              CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ", " + 
              CN3?.ToString(indent, true) + ", " + CN4?.ToString(indent, true) + ", " + 
              CN5?.ToString(indent, true) + ", " + CN6?.ToString(indent, true) + ")";
        case BNF.MUTE: return "Mute(" + CN1?.ToString(indent, true) + ")";
        case BNF.VOLUME:
          if (CN2 == null) return "Volume(" + CN1?.ToString(indent, true) + ")";
          else return "Volume(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";
        case BNF.PITCH: return "Pitch(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";
        case BNF.PAN: return "Pan(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ")";

        case BNF.MUSICLOAD: return "FIXME";
        case BNF.MUSICPLAY: return "FIXME";
        case BNF.MUSICSTOP: return "FIXME";
        case BNF.MUSICPOS: return "FIXME";
        case BNF.MUSICVOICES: return "FIXME";

        case BNF.TILEMAP: return "TileMap(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true)  + ", " + CN3?.ToString(indent, true) + ")";
        case BNF.TILEPOS: return "TilePos(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ", " + CN3?.ToString(indent, true) +
            (CN4 == null ? "" : (", " + CN4.ToString(indent, true) +
              (CN5 == null ? "" : ", " + CN5.ToString(indent, true))
            )) + ")";

        case BNF.TILESET: return "TileSet(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ", " + CN3?.ToString(indent, true) +
            ", " + CN4.ToString(indent, true) + (CN5 == null ? "" : ", " + CN5.ToString(indent, true)) + ")";

        case BNF.TILEGET: return "TileGet(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ", " + CN3?.ToString(indent, true) + ")";
        case BNF.TILEGETROT: return "TileGetRot(" + CN1?.ToString(indent, true) + ", " + CN2?.ToString(indent, true) + ", " + CN3?.ToString(indent, true) + ")";

        default:
          res += "[[Missing:" + type + "]]";
          break;
      }
    } catch (Exception e) {
      res += "[[INVALID!!]]" + e.Message;
    }

    return res.Replace("  ", " ");
  }

  internal bool Evaluable() {
    switch (type) {
      case BNF.REG:
      case BNF.INT:
      case BNF.FLT:
      case BNF.HEX:
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
      case BNF.LAB:
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
  Data,
  Functions,
  FunctionDef,
  FunctionCall,
  RETURN,
  Params,
  ScrConfig,
  Ram,
  Rom, // This is used to store the data
  Label,
  REG,
  INT, 
  FLT,
  HEX,
  COLOR,//rename to color
  STR,
  MEM,
  MEMlong,
  MEMlongb,
  MEMlongi,
  MEMlongf,
  MEMlongs,
  MEMchar,
  OPpar,
  OPsum,
  OPsub,
  OPmul,
  OPdiv,
  OPmod,
  OPand,
  OPor,
  OPxor,
  OPlsh,
  OPrsh,
  LAB, // This is used to reference some data
  CASTb,
  CASTi,
  CASTf,
  CASTs,
  UOneg,
  UOinv,
  UOsub,
  COMPeq,
  COMPne,
  COMPlt,
  COMPle,
  COMPgt,
  COMPge,
  ASSIGN,
  ASSIGNsum,
  ASSIGNsub,
  ASSIGNmul,
  ASSIGNdiv,
  ASSIGNmod,
  ASSIGNand,
  ASSIGNor,
  ASSIGNxor,
  Inc,
  Dec,
  BLOCK,
  IF,
  Else,
  WHILE,
  FOR,
  CLR,
  WRITE,
  WAIT,
  DESTROY,
  SCREEN,
  SPRITE,
  SPEN,
  SPOS,
  SROT,
  STINT,
  SSCALE,
  SETP,
  GETP,
  LINE,
  BOX,
  CIRCLE,
  FRAME,
  DTIME,
  LEN,
  PLEN,
  KEY,
  KEYx,
  KEYy,
  SIN,
  COS,
  TAN,
  ATAN2,
  SQR,
  POW,
  SUBSTRING,
  TRIM,
  SOUND,
  WAVE,
  MUTE,
  VOLUME,
  PITCH,
  PAN,
  MUSICLOAD,
  MUSICPLAY,
  MUSICSTOP,
  MUSICPOS,
  MUSICVOICES,
  TILEMAP,
  TILEPOS,
  TILESET,
  TILEGET,
  TILEGETROT,
  NOP,
}

public enum VT {
  None,
  Int,
  Float,
  String
}

public enum MD {
  Dir,
  Reg,
  Mem
}
