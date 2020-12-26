﻿using System;
using System.Collections.Generic;

public class CodeNode {
  public BNF type;
  public List<CodeNode> children;
  public string id;
  public int iVal;
  public float fVal;
  public string sVal;
  public char Reg;
  public VT valType;
  internal CodeNode First { get { return children?[0]; } }
  internal CodeNode Second { get { return children != null && children.Count > 1 ? children[1] : null; } }
  internal CodeNode Third { get { return children != null && children.Count > 2 ? children[2] : null; } }

  public CodeNode(BNF bnf) {
    type = bnf;
  }

  public CodeNode(BNF bnf, string v) {
    type = bnf;
    id = v;
  }

  internal void Add(CodeNode node) {
    if (children == null) children = new List<CodeNode>();
    children.Add(node);
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
        case BNF.Data: res = "Data:\n" + ToString(indent + 1, false); break;
        case BNF.REG: res += (sameLine ? "" : id) + " " + Reg + (sameLine ? " " : "\n"); break;
        case BNF.INT: res += (sameLine ? "" : id) + " " + iVal + (sameLine ? " " : "\n"); break;
        case BNF.COL: res += (sameLine ? "" : id) + " c" +
            (iVal > 64 ?
              ((iVal & 48) >> 4).ToString() +
              ((iVal & 12) >> 2).ToString() +
              ((iVal & 3) >> 0).ToString()
              :
              ((iVal & 192) >> 6).ToString() +
              ((iVal & 48) >> 4).ToString() +
              ((iVal & 12) >> 2).ToString() +
              ((iVal & 3) >> 0).ToString()
            ) + (sameLine ? " " : "\n"); break;
        case BNF.HEX: res += (sameLine ? "" : id) + " x" + iVal.ToString("X") + (sameLine ? " " : "\n"); break;
        case BNF.FLT: res += (sameLine ? "" : id) + " " + fVal + (sameLine ? " " : "\n"); break;
        case BNF.STR: res += (sameLine ? "" : id) + " \"" + sVal + (sameLine ? "\" " : "\"\n"); break;
        case BNF.STRcnst: res += "[[" + type + "]]"; break;
        case BNF.MEM: res += (sameLine ? "" : id) + " [" + children[0].ToString(indent + 1, true) + "]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlong: res += (sameLine ? "" : id) + " [" + children[0].ToString(indent + 1, true) + "@]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlongb: res += (sameLine ? "" : id) + " [" + children[0].ToString(indent + 1, true) + "@b]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlongi: res += (sameLine ? "" : id) + " [" + children[0].ToString(indent + 1, true) + "@i]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlongf: res += (sameLine ? "" : id) + " [" + children[0].ToString(indent + 1, true) + "@f]" + (sameLine ? " " : "\n"); break;
        case BNF.MEMlongs: res += (sameLine ? "" : id) + " [" + children[0].ToString(indent + 1, true) + "@s]" + (sameLine ? " " : "\n"); break;

        case BNF.EXP:
          res += (sameLine ? "" : id);
          foreach (CodeNode cn in children)
            res += cn.ToString(indent + 1, true);
          if (!sameLine) res += "\n";
          break;
        case BNF.OPpar:
          res += "(" + children[0].ToString(indent + 1, true) + ")";
          break;
        case BNF.OPsum: res += "("+children[0].ToString(indent + 1, true) + "+" + children[1].ToString(indent + 1, true)+")"; break;
        case BNF.OPsub: res += "("+children[0].ToString(indent + 1, true) + "-" + children[1].ToString(indent + 1, true)+")"; break;
        case BNF.OPmul: res += "("+children[0].ToString(indent + 1, true) + "*" + children[1].ToString(indent + 1, true)+")"; break;
        case BNF.OPdiv: res += "("+children[0].ToString(indent + 1, true) + "/" + children[1].ToString(indent + 1, true)+")"; break;
        case BNF.OPmod: res += "("+children[0].ToString(indent + 1, true) + "%" + children[1].ToString(indent + 1, true)+")"; break;
        case BNF.OPand: res += "("+children[0].ToString(indent + 1, true) + "&" + children[1].ToString(indent + 1, true)+")"; break;
        case BNF.OPor:  res += "("+children[0].ToString(indent + 1, true) + "|" + children[1].ToString(indent + 1, true)+")"; break;
        case BNF.OPxor: res += "("+children[0].ToString(indent + 1, true) + "^" + children[1].ToString(indent + 1, true)+")"; break;

        case BNF.ASSIGN: res += children[0].ToString(indent + 1, true) + " = " + children[1].ToString(indent + 1, true); break;
        case BNF.ASSIGNsum: res += children[0].ToString(indent + 1, true) + " += " + children[1].ToString(indent + 1, true); break;
        case BNF.ASSIGNsub: res += children[0].ToString(indent + 1, true) + " -= " + children[1].ToString(indent + 1, true); break;
        case BNF.ASSIGNmul: res += children[0].ToString(indent + 1, true) + " *= " + children[1].ToString(indent + 1, true); break;
        case BNF.ASSIGNdiv: res += children[0].ToString(indent + 1, true) + " /= " + children[1].ToString(indent + 1, true); break;
        case BNF.ASSIGNmod: res += children[0].ToString(indent + 1, true) + " %= " + children[1].ToString(indent + 1, true); break;
        case BNF.ASSIGNand: res += children[0].ToString(indent + 1, true) + " &= " + children[1].ToString(indent + 1, true); break;
        case BNF.ASSIGNor: res += children[0].ToString(indent + 1, true) + " |= " + children[1].ToString(indent + 1, true); break;
        case BNF.ASSIGNxor: res += children[0].ToString(indent + 1, true) + " ^= " + children[1].ToString(indent + 1, true); break;

        case BNF.UOneg: res += (sameLine ? "" : id) + " !" + children[0].ToString(indent, true); break;
        case BNF.UOinv: res += (sameLine ? "" : id) + " ~" + children[0].ToString(indent, true); break;
        case BNF.UOsub: res += (sameLine ? "" : id) + " -" + children[0].ToString(indent, true); break;

        case BNF.LEN: res += children[0].ToString(indent, true) + ".len"; break;
        case BNF.CLR: res += (sameLine ? "" : id) + "Clr(" + children[0].ToString(indent, true) + ")"; break;
        case BNF.DTIME: res += "dateTime"; break;

        case BNF.WRITE: {
          res += (sameLine ? "" : id) + "Write(" +
            children[0].ToString(indent, true) + ", " +
            children[1].ToString(indent, true) + ", " +
            children[2].ToString(indent, true) + ", " +
            children[3].ToString(indent, true);
          if (children.Count > 4) res += ", " + children[4].ToString(indent, true);
          res += ")";
        }
        break;

        case BNF.LINE: res += (sameLine ? "" : id) + "line(" +
            children[0].ToString(indent, true) + ", " +
            children[1].ToString(indent, true) + ", " +
            children[2].ToString(indent, true) + ", " +
            children[3].ToString(indent, true) + ", " +
            children[4].ToString(indent, true) + ")";
          break;

        case BNF.BOX:
          res += (sameLine ? "" : id) + "line(" +
            children[0].ToString(indent, true) + ", " +
            children[1].ToString(indent, true) + ", " +
            children[2].ToString(indent, true) + ", " +
            children[3].ToString(indent, true) + ", " +
            children[4].ToString(indent, true);
          if (children.Count > 5)
            res += children[5].ToString(indent, true) + ")";
          else
            res += ")";
          break;

        case BNF.Inc: res += (sameLine ? "" : id) + children[0].ToString(indent, true) + "++"; break;
        case BNF.Dec: res += (sameLine ? "" : id) + children[0].ToString(indent, true) + "--"; break;

        case BNF.COMPeq: res += "(" + children[0].ToString(indent + 1, true) + "==" + children[1].ToString(indent + 1, true) + ")"; break;
        case BNF.COMPne: res += "(" + children[0].ToString(indent + 1, true) + "!=" + children[1].ToString(indent + 1, true) + ")"; break;
        case BNF.COMPlt: res += "(" + children[0].ToString(indent + 1, true) + "<" + children[1].ToString(indent + 1, true) + ")"; break;
        case BNF.COMPle: res += "(" + children[0].ToString(indent + 1, true) + "<=" + children[1].ToString(indent + 1, true) + ")"; break;
        case BNF.COMPgt: res += "(" + children[0].ToString(indent + 1, true) + ">" + children[1].ToString(indent + 1, true) + ")"; break;
        case BNF.COMPge: res += "(" + children[0].ToString(indent + 1, true) + ">=" + children[1].ToString(indent + 1, true) + ")"; break;

        case BNF.IF: res += (sameLine ? "" : id) + "if (" + children[0].ToString(indent, true) + ") { ..." + (children.Count - 1) + "... }"; break;
        case BNF.IFelse: res += (sameLine ? "" : id) + "else { ..." + (children.Count - 1) + "... }"; break;

        case BNF.BLOCK: {
          res = "{ ";
          if (children == null) res += "[[empty]]";
          else
            foreach (CodeNode n in children)
              res += n.ToString(indent + 1, false) + "\n";
          res += " }";
        }
        break;

        case BNF.OP:
        case BNF.UO:
        case BNF.CND:
        case BNF.COMP:
        case BNF.STATEMENT:
        case BNF.STATEMENTlst:
        case BNF.INCDED:
        case BNF.WHILE:
        case BNF.FOR:
        case BNF.SPRITE:
        case BNF.SPEN:
        case BNF.SPOS:
        case BNF.SPIX:
        case BNF.GPIX:
        case BNF.CIR:
        case BNF.FRAME:
          res += "[[Missing:" + type + "]]";
          break;
      }
    } catch(Exception) {
      res += "[[INVALID!!]]";
    }

    return res.Replace("  ", " ");
  }

  internal bool Evaluable() {
    switch (type) {
      case BNF.REG:
      case BNF.INT:
      case BNF.FLT:
      case BNF.HEX:
      case BNF.COL:
      case BNF.STR:
      case BNF.STRcnst:
      case BNF.MEM:
      case BNF.MEMlong:
      case BNF.MEMlongb:
      case BNF.MEMlongi:
      case BNF.MEMlongf:
      case BNF.MEMlongs:
      case BNF.EXP:
      case BNF.OPpar:
      case BNF.OPsum:
      case BNF.OPsub:
      case BNF.OPmul:
      case BNF.OPdiv:
      case BNF.OPmod:
      case BNF.OPand:
      case BNF.OPor:
      case BNF.OPxor:
      case BNF.UOneg:
      case BNF.UOinv:
      case BNF.UOsub:
      case BNF.Inc:
      case BNF.Dec:
      case BNF.DTIME:
      case BNF.LEN:
      case BNF.COMPeq:
      case BNF.COMPne:
      case BNF.COMPgt:
      case BNF.COMPge:
      case BNF.COMPlt:
      case BNF.COMPle:
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
  private BNF mEM;
  private BNF rEG;

  public Expected() {
    val = 0;
  }
  public Expected(BNF bnf1) {
    val = 0;
    Add(bnf1);
  }
  public Expected(BNF bnf1, BNF bnf2) {
    val = 0;
    Add(bnf1, bnf2);
  }
  public Expected(BNF bnf1, BNF bnf2, BNF bnf3) {
    val = 0;
    Add(bnf1, bnf2, bnf3);
  }


  public void Clear() { val = 0; }
  public void Add(BNF bnf) {
    val |= (1ul << (int)bnf);
  }
  public void Add(BNF bnf1, BNF bnf2) {
    val |= (1ul << (int)bnf1);
    val |= (1ul << (int)bnf2);
  }
  public void Add(BNF bnf1, BNF bnf2, BNF bnf3) {
    val |= (1ul << (int)bnf1);
    val |= (1ul << (int)bnf2);
    val |= (1ul << (int)bnf3);
  }
  public void Remove(BNF bnf) {
    val &= ~(1ul << (int)bnf);
  }
  public bool IsGood(BNF bnf) {
    return (val & (1ul << (int)bnf)) != 0;
  }
  public bool IsGood(BNF bnf1, BNF bnf2) {
    return (val & (1ul << (int)bnf1)) != 0 || (val & (1ul << (int)bnf2)) != 0;
  }
  public bool IsGood(BNF bnf1, BNF bnf2, BNF bnf3) {
    return (val & (1ul << (int)bnf1)) != 0 || (val & (1ul << (int)bnf2)) != 0 || (val & (1ul << (int)bnf3)) != 0;
  }
  public bool IsGood(BNF bnf1, BNF bnf2, BNF bnf3, BNF bnf4) {
    return (val & (1ul << (int)bnf1)) != 0 || (val & (1ul << (int)bnf2)) != 0 || (val & (1ul << (int)bnf3)) != 0 || (val & (1ul << (int)bnf4)) != 0;
  }
}



public enum BNF {
  Program,
  Start,
  Update,
  Data,
  REG,
  INT, 
  FLT,
  HEX,
  COL,
  STR,
  STRcnst,
  MEM,
  MEMlong,
  MEMlongb,
  MEMlongi,
  MEMlongf,
  MEMlongs,
  EXP,
  OP,
  OPpar,
  OPsum,
  OPsub,
  OPmul,
  OPdiv,
  OPmod,
  OPand,
  OPor,
  OPxor,
  UO,
  UOneg,
  UOinv,
  UOsub,
  CND,
  COMP,
  COMPeq,
  COMPne,
  COMPlt,
  COMPle,
  COMPgt,
  COMPge,
  STATEMENT,
  STATEMENTlst,
  ASSIGN,
  ASSIGNsum,
  ASSIGNsub,
  ASSIGNmul,
  ASSIGNdiv,
  ASSIGNmod,
  ASSIGNand,
  ASSIGNor,
  ASSIGNxor,
  INCDED,
  Inc,
  Dec,
  BLOCK,
  IF,
  IFelse,
  WHILE,
  FOR,
  CLR,
  WRITE,
  SPRITE,
  SPEN,
  SPOS,
  SPIX,
  GPIX,
  LINE,
  BOX,
  CIRCLE,
  CIR,
  FRAME,
  DTIME,
  LEN,
}

public enum VT {
  None,
  Int,
  Float,
  String
}