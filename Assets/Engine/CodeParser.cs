using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CodeParser : MonoBehaviour {
  Dictionary<string, CodeNode> nodes = null;
  int idcount = 0;
  Variables vars = null;
  readonly Expected expected = new Expected();
  readonly List<string> reserverdKeywords = new List<string> {
    "if",
    "else",
    "for",
    "while",
    "frame",
    "screen",
    "write",
    "line",
    "box",
    "circle",
    "dateTime",
    "clr",
    "keyl",
    "keyr",
    "keyu",
    "keyd",
    "keya",
    "keyb",
    "keyc",
    "keyf",
    "keye",
    "keylu",
    "keyru",
    "keyuu",
    "keydu",
    "keyau",
    "keybu",
    "keycu",
    "keyfu",
    "keyeu",
    "keyld",
    "keyrd",
    "keyud",
    "keydd",
    "keyad",
    "keybd",
    "keycd",
    "keyfd",
    "keyed",
    "",
    "",
    "",
    "",
    "",
    "",
  };

  #region Regex

  readonly Regex rgMLBacktick = new Regex("`", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgSLComment = new Regex("([^\\n]*)(//[^\\n]*)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgMLComment = new Regex("/\\*[^(\\*/)]*\\*/", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgBlockStart = new Regex(".*\\{[\\s]*", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgBlockEnd = new Regex("[\\s]*\\}[\\s]*", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));

  readonly Regex rgVar = new Regex("(?<=[^a-z0-9`]|^)([a-z][0-9a-z]{0,7})([^a-z0-9¶]|$)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgHex = new Regex("0x([0-9a-f]{8}|[0-9a-f]{4}|[0-9a-f]{2})", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgCol = new Regex("c([0-3])([0-3])([0-3])", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgString = new Regex("((?<![\\\\])\")((?:.(?!(?<![\\\\])\\1))*.?)\\1", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDeltat = new Regex("deltatime", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFloat = new Regex("[0-9]+\\.[0-9]+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgInt = new Regex("[0-9]+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgPars = new Regex("\\([\\s]*`[a-z]{3,}¶[\\s]*\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMem = new Regex("\\[[\\s]*`[a-z]{3,}¶[\\s]*]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemL = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemB = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@b[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemI = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@i[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemF = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@f[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemS = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@s[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemUnparsed = new Regex("[\\s]*\\[.+\\][\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgUOneg = new Regex("!`[a-z]{3,}¶", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgUOinv = new Regex("~`[a-z]{3,}¶", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgUOsub = new Regex("([^0-9\\s]+[\\s]*(\\-[\\s]*`[a-z]{3,}¶))|(^[\\s]*(\\-[\\s]*`[a-z]{3,}¶))", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgMul = new Regex("(`[a-z]{3,}¶)([\\s]*\\*[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDiv = new Regex("(`[a-z]{3,}¶)([\\s]*/[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMod = new Regex("(`[a-z]{3,}¶)([\\s]*%[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSum = new Regex("(`[a-z]{3,}¶)([\\s]*\\+[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSub = new Regex("(`[a-z]{3,}¶)([\\s]*\\-[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAnd = new Regex("(`[a-z]{3,}¶)([\\s]*&[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgOr = new Regex("(`[a-z]{3,}¶)([\\s]*\\|[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgXor = new Regex("(`[a-z]{3,}¶)([\\s]*\\^[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCastB = new Regex("(`[a-z]{3,}¶)_b", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCastI = new Regex("(`[a-z]{3,}¶)_i", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCastF = new Regex("(`[a-z]{3,}¶)_f", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCastS = new Regex("(`[a-z]{3,}¶)_s", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgLen = new Regex("([\\s]*`[a-z]{3,}¶)\\.len[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgPLen = new Regex("([\\s]*`[a-z]{3,}¶)\\.plen[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgTag = new Regex("([\\s]*`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));


  readonly Regex rgAssign = new Regex("[a-z\\][\\s]*=[^=]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssSum = new Regex("[a-z\\][\\s]*\\+=[^(\\+=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssSub = new Regex("[a-z\\][\\s]*\\-=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssMul = new Regex("[a-z\\][\\s]*\\*=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssDiv = new Regex("[a-z\\][\\s]*/=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssMod = new Regex("[a-z\\][\\s]*%=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssAnd = new Regex("[a-z\\][\\s]*&=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssOr = new Regex("[a-z\\][\\s]*\\|=[^(\\-=)]=", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssXor = new Regex("[a-z\\][\\s]*\\^=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgClr = new Regex("[\\s]*clr\\((.+)\\)[\\s]*", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgFrame = new Regex("frame", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgWrite1 = new Regex("[\\s]*(write\\()(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgWrite2 = new Regex("[\\s]*(write\\()(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgLine = new Regex("[\\s]*(line\\()(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBox1 = new Regex("[\\s]*(box\\()(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBox2 = new Regex("[\\s]*(box\\()(.*),(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCircle1 = new Regex("[\\s]*(circle\\()(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCircle2 = new Regex("[\\s]*(circle\\()(.*),(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgInc = new Regex("(.*)\\+\\+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDec = new Regex("(.*)\\-\\-", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgIf = new Regex("[\\s]*if[\\s]*\\(([^{}]+)\\)[\\s]*\\{", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgElse = new Regex("[\\s]*else[\\s]*\\{", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgWhile = new Regex("[\\s]*while[\\s]*\\(([^{}]+)\\)[\\s]*\\{", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgScreen = new Regex("[\\s]*screen[\\s]*\\(([^,]*),([^,]*)(,([^,]*)){0,1}(,([^,]*)){0,1}\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgCMPeq = new Regex("==", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPneq = new Regex("!=", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPlt = new Regex("\\<", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPleq = new Regex("\\<\\=", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPgt = new Regex("\\>", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPgeq = new Regex("\\>=", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPeqTag = new  Regex("(`[a-z]{3,}¶)([\\s]*`EQ[a-z]+¶[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPneqTag = new Regex("(`[a-z]{3,}¶)([\\s]*`NQ[a-z]+¶[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPltTag = new  Regex("(`[a-z]{3,}¶)([\\s]*`GT[a-z]+¶[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPleqTag = new Regex("(`[a-z]{3,}¶)([\\s]*`GE[a-z]+¶[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPgtTag = new  Regex("(`[a-z]{3,}¶)([\\s]*`LT[a-z]+¶[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPgeqTag = new Regex("(`[a-z]{3,}¶)([\\s]*`LE[a-z]+¶[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgKey = new Regex("[\\s]*key([udlrabcdfexyhv]|fire|esc)\\((`[a-z]{3,}¶)?\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  //   keys -> U, D, L, R, A, B, C, D, X, Y, H, V, Fire, Esc


  #endregion Regex

  public CodeNode Parse(string file, Variables variables) {
    file = file.Trim().Replace("\r", "").Replace("\t", " ");
    idcount = 0;
    CodeNode res = new CodeNode(BNF.Program);
    nodes = new Dictionary<string, CodeNode>();
    vars = variables;

    int pos = file.IndexOf("name:", System.StringComparison.CurrentCultureIgnoreCase);
    if (pos != -1) {
      int end = file.IndexOf('\n', pos);
      if (end != -1 && (pos == 0 || file[pos - 1] == '\n')) res.sVal = file.Substring(pos + 5, end - pos - 5).Trim(' ', '\n');
    }


    // find the Start
    pos = file.IndexOf("start", System.StringComparison.CurrentCultureIgnoreCase);
    if (pos != -1 && (pos == 0 || file[pos - 1] == '\n')) {
      CodeNode start = new CodeNode(BNF.Start);
      res.Add(start);

      FindBlock(file, pos + 5, out int bstart, out int bend);
      ParseBlock(file, bstart + 1, bend - 1, start);
    }

    // find the Update
    pos = file.IndexOf("update", System.StringComparison.CurrentCultureIgnoreCase);
    if (pos != -1 && (pos == 0 || file[pos - 1] == '\n')) {
      CodeNode update = new CodeNode(BNF.Update);
      res.Add(update);

      FindBlock(file, pos + 6, out int bstart, out int bend);
      ParseBlock(file, bstart + 1, bend - 1, update);
    }

    // find the Data
    pos = file.IndexOf("data", System.StringComparison.CurrentCultureIgnoreCase);
    if (pos != -1 && (pos == 0 || file[pos - 1] == '\n')) {
      CodeNode data = new CodeNode(BNF.Data);
      res.Add(data);

      FindBlock(file, pos + 4, out int bstart, out int bend);
      ParseBlock(file, bstart + 1, bend - 1, data);
    }

    return res;
  }

  private void FindBlock(string file, int from, out int bstart, out int bend) {
    bstart = file.IndexOf("{", from, StringComparison.CurrentCultureIgnoreCase);
    if (bstart == -1) throw new Exception("Invalid start of block,\nmissing open braket: {, position " + from);
    int num = 1;
    int pos = bstart + 1;
    while (num > 0) {
      int p1 = file.IndexOf("{", pos, System.StringComparison.CurrentCultureIgnoreCase);
      int p2 = file.IndexOf("}", pos, System.StringComparison.CurrentCultureIgnoreCase);
      int p3 = file.IndexOf("\"", pos, System.StringComparison.CurrentCultureIgnoreCase);
      if (p1 != -1 && (p1 < p2 || p2 == -1) && (p1 < p3 || p3 == -1)) {
        num++;
        pos = p1 + 1;
      }
      else if (p2 != -1 && (p2 < p1 || p1 == -1) && (p2 < p3 || p3 == -1)) {
        num--;
        if (num == 0) {
          bend = p2;
          return;
        }
        pos = p2 + 1;
      }
      else if (p3 != -1 && (p3 < p1 || p1 == -1) && (p3 < p2 || p2 == -1)) {
        pos = p3 + 1;
        int next = file.IndexOf("\"", pos, System.StringComparison.CurrentCultureIgnoreCase);
        if (next == -1) throw new System.Exception("Unterminated string: {, position " + p3);
        pos = next + 1;
      }
      else
        throw new System.Exception("Invalid block: {, position " + bstart);
    }
    throw new Exception("Strange ending...");
  }

  private int ParseBlock(string file, int start, int end, CodeNode parent, int linenum = 1) {
    // Find at what line this starts
    for (int i = 0; i < start; i++)
      if (file[i] == '\n') linenum++;

    // Remove the comments and some unwanted chars
    string clean = file.Substring(start, end - start).Trim(' ', '\t', '\r');
    clean = rgSLComment.Replace(clean, m => {
      if (m.Groups.Count>1) return m.Groups[1].Value;
      return m.Value;
    });
    clean = rgMLComment.Replace(clean, "");
    clean = rgMLBacktick.Replace(clean, "'");
    string[] parts = clean.Split('\n');

    // Follow the BNF rules to get the elements, one line at time
    for (int lineidx = 0; lineidx < parts.Length; lineidx++) {
      linenum++;
      if (string.IsNullOrWhiteSpace(parts[lineidx])) continue;
      expected.Set(Expected.Val.Statement);
      int step = ParseLine(parent, parts, lineidx, linenum);
      lineidx += step - 1;
      linenum += step - 1;
    }
    return parts.Length;
  }


  void ParseAssignment(string line, int linenum, CodeNode parent, BNF bnf, string match) {
    string dest = line.Substring(0, line.IndexOf(match));
    string[] dests = new string[] { dest };
    string val = line.Substring(line.IndexOf(match) + 2);
    CodeNode node = new CodeNode(bnf);
    expected.Set(Expected.Val.MemReg);
    ParseLine(node, dests, 0, linenum);
    parent.Add(node);
    node.Add(ParseExpression(val, 0, linenum));
  }

  int ParseLine(CodeNode parent, string[] lines, int lineidx, int linenum) {
    string line = lines[lineidx].Trim(' ', '\t', '\r');

    if (rgBlockEnd.IsMatch(line)) return 1;

    // Start by replacing all the problematic stuff
    // [STRING] STR => `STx¶
    line = rgString.Replace(line, m => {
      CodeNode n = new CodeNode(BNF.STR, GenId("ST")) {
        sVal = m.Value.Substring(1, m.Value.Length - 2)
      };
      nodes[n.id] = n;
      return n.id;
    });

    // [CMPeq] == => `EQx¶ ==> it will be completed later, this is just to avoid the probblematic symbols
    line = rgCMPeq.Replace(line, m => { CodeNode n = new CodeNode(BNF.COMPeq, GenId("EQ")); nodes[n.id] = n; return n.id; });

    // [CMPneq] == => `NQx¶ ==> it will be completed later, this is just to avoid the probblematic symbols
    line = rgCMPneq.Replace(line, m => { CodeNode n = new CodeNode(BNF.COMPne, GenId("NQ")); nodes[n.id] = n; return n.id; });

    // [CMPgte] == => `GEx¶ ==> it will be completed later, this is just to avoid the probblematic symbols
    line = rgCMPgeq.Replace(line, m => { CodeNode n = new CodeNode(BNF.COMPge, GenId("GE")); nodes[n.id] = n; return n.id; });

    // [CMPlte] == => `LEx¶ ==> it will be completed later, this is just to avoid the probblematic symbols
    line = rgCMPleq.Replace(line, m => { CodeNode n = new CodeNode(BNF.COMPle, GenId("LE")); nodes[n.id] = n; return n.id; });

    // [CMPgt] == => `GTx¶ ==> it will be completed later, this is just to avoid the probblematic symbols
    line = rgCMPgt.Replace(line, m => { CodeNode n = new CodeNode(BNF.COMPgt, GenId("GT")); nodes[n.id] = n; return n.id; });

    // [CMPlt] == => `LTx¶ ==> it will be completed later, this is just to avoid the probblematic symbols
    line = rgCMPlt.Replace(line, m => { CodeNode n = new CodeNode(BNF.COMPlt, GenId("LT")); nodes[n.id] = n; return n.id; });



    // Check what we have. Pick something in line with what is expected

    // [ASSp] = [MEM] += [EXPR] | [REG] += [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssSum.IsMatch(line)) {
      ParseAssignment(line, linenum, parent, BNF.ASSIGNsum, "+=");
      return 1;
    }

    // [ASSs] = [MEM] -= [EXPR] | [REG] -= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssSub.IsMatch(line)) {
      ParseAssignment(line, linenum, parent, BNF.ASSIGNsub, "-=");
      return 1;
    }

    // [ASSm] = [MEM] *= [EXPR] | [REG] *= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssMul.IsMatch(line)) {
      ParseAssignment(line, linenum, parent, BNF.ASSIGNmul, "*=");
      return 1;
    }

    // [ASSd] = [MEM] /= [EXPR] | [REG] /= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssDiv.IsMatch(line)) {
      ParseAssignment(line, linenum, parent, BNF.ASSIGNdiv, "/=");
      return 1;
    }

    // [ASSmod] = [MEM] %= [EXPR] | [REG] %= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssMod.IsMatch(line)) {
      ParseAssignment(line, linenum, parent, BNF.ASSIGNmod, "%=");
      return 1;
    }

    // [ASSand] = [MEM] &= [EXPR] | [REG] &= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssAnd.IsMatch(line)) {
      ParseAssignment(line, linenum, parent, BNF.ASSIGNand, "&=");
      return 1;
    }

    // [ASSor] = [MEM] %= [EXPR] | [REG] %= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssOr.IsMatch(line)) {
      ParseAssignment(line, linenum, parent, BNF.ASSIGNmod, "|=");
      return 1;
    }

    // [ASSxor] = [MEM] ^= [EXPR] | [REG] ^= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssXor.IsMatch(line)) {
      ParseAssignment(line, linenum, parent, BNF.ASSIGNmod, "^=");
      return 1;
    }

    // [ASS] = [MEM] = [EXPR] | [REG] = [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssign.IsMatch(line)) {
      string[] dests = new string[] { line.Substring(0, line.IndexOf('=')) };
      string val = line.Substring(line.IndexOf('=') + 1);
      CodeNode node = new CodeNode(BNF.ASSIGN);
      expected.Set(Expected.Val.MemReg);
      ParseLine(node, dests, 0, linenum);
      parent.Add(node);
      node.Add(ParseExpression(val, 0, linenum));
      return 1;
    }

    // [CLR] = clr([EXPR])
    if (expected.IsGood(Expected.Val.Statement) && rgClr.IsMatch(line)) {
      Match m = rgClr.Match(line);
      if (m.Groups.Count < 2) throw new Exception("Invalid Clr() command. Line: " + linenum);
      CodeNode node = new CodeNode(BNF.CLR);
      node.Add(ParseExpression(m.Groups[1].Value, 0, linenum));
      parent.Add(node);
      return 1;
    }

    // [WRITE] = write([EXPR], [EXPR], [EXPR], [EXPR], [EXPR]) ; text, x, y, col(front), col(back)
    if (expected.IsGood(Expected.Val.Statement)) {
      if (rgWrite2.IsMatch(line)) {
        Match m = rgWrite2.Match(line);
        if (m.Groups.Count < 7) throw new Exception("Invalid Write() command. Line: " + linenum);
        CodeNode node = new CodeNode(BNF.WRITE);
        node.Add(ParseExpression(m.Groups[2].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[3].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[4].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[5].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[6].Value, 0, linenum));
        parent.Add(node);
        return 1;
      }
      if (rgWrite1.IsMatch(line)) {
        Match m = rgWrite1.Match(line);
        if (m.Groups.Count < 5) throw new Exception("Invalid Write() command. Line: " + linenum);
        CodeNode node = new CodeNode(BNF.WRITE);
        node.Add(ParseExpression(m.Groups[2].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[3].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[4].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[5].Value, 0, linenum));
        parent.Add(node);
        return 1;
      }
    }

    // [LINE] = line([EXPR], [EXPR], [EXPR], [EXPR], [EXPR])
    if (expected.IsGood(Expected.Val.Statement) && rgLine.IsMatch(line)) {
      Match m = rgLine.Match(line);
      if (m.Groups.Count < 7) throw new Exception("Invalid Line() command. Line: " + linenum);
      CodeNode node = new CodeNode(BNF.LINE);
      node.Add(ParseExpression(m.Groups[2].Value, 0, linenum));
      node.Add(ParseExpression(m.Groups[3].Value, 0, linenum));
      node.Add(ParseExpression(m.Groups[4].Value, 0, linenum));
      node.Add(ParseExpression(m.Groups[5].Value, 0, linenum));
      node.Add(ParseExpression(m.Groups[6].Value, 0, linenum));
      parent.Add(node);
      return 1;
    }

    // [BOX] = box([EXP], [EXP], [EXP], [EXP], [EXP], [[EXP]])
    if (expected.IsGood(Expected.Val.Statement)) {
      if (rgBox2.IsMatch(line)) {
        Match m = rgBox2.Match(line);
        if (m.Groups.Count < 8) throw new Exception("Invalid Box() command. Line: " + linenum);
        CodeNode node = new CodeNode(BNF.BOX);
        node.Add(ParseExpression(m.Groups[2].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[3].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[4].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[5].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[6].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[7].Value, 0, linenum));
        parent.Add(node);
        return 1;
      }
      if (rgBox1.IsMatch(line)) {
        Match m = rgBox1.Match(line);
        if (m.Groups.Count < 7) throw new Exception("Invalid Box() command. Line: " + linenum);
        CodeNode node = new CodeNode(BNF.BOX);
        node.Add(ParseExpression(m.Groups[2].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[3].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[4].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[5].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[6].Value, 0, linenum));
        parent.Add(node);
        return 1;
      }
    }

    // [CIRCLE] = circle([EXP], [EXP], [EXP], [EXP], [EXP], [[EXP]])
    if (expected.IsGood(Expected.Val.Statement)) {
      if (rgCircle2.IsMatch(line)) {
        Match m = rgCircle2.Match(line);
        if (m.Groups.Count < 8) throw new Exception("Invalid Circle() command. Line: " + linenum);
        CodeNode node = new CodeNode(BNF.CIRCLE);
        node.Add(ParseExpression(m.Groups[2].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[3].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[4].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[5].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[6].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[7].Value, 0, linenum));
        parent.Add(node);
        return 1;
      }
      if (rgCircle1.IsMatch(line)) {
        Match m = rgCircle1.Match(line);
        if (m.Groups.Count < 7) throw new Exception("Invalid Circle() command. Line: " + linenum);
        CodeNode node = new CodeNode(BNF.CIRCLE);
        node.Add(ParseExpression(m.Groups[2].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[3].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[4].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[5].Value, 0, linenum));
        node.Add(ParseExpression(m.Groups[6].Value, 0, linenum));
        parent.Add(node);
        return 1;
      }
    }

    // [SCREEN] width, heigth, tiles, filter
    if (expected.IsGood(Expected.Val.Statement) && rgScreen.IsMatch(line)) {
      Match m = rgScreen.Match(line);
      if (m.Groups.Count < 3) throw new Exception("Invalid rgScreen() command. Line: " + linenum);
      CodeNode node = new CodeNode(BNF.SCREEN);
      node.Add(ParseExpression(m.Groups[1].Value, 0, linenum));
      node.Add(ParseExpression(m.Groups[2].Value, 0, linenum));
      if (m.Groups.Count > 4 && !string.IsNullOrEmpty(m.Groups[4].Value)) node.Add(ParseExpression(m.Groups[4].Value, 0, linenum));
      if (m.Groups.Count > 6 && !string.IsNullOrEmpty(m.Groups[6].Value)) node.Add(ParseExpression(m.Groups[6].Value, 0, linenum));
      parent.Add(node);
      return 1;
    }

    // [FRAME]
    if (expected.IsGood(Expected.Val.Statement) && rgFrame.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.FRAME);
      parent.Add(node);
      return 1;
    }

    // [Inc]
    if (expected.IsGood(Expected.Val.Statement) && rgInc.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.Inc);
      node.Add(ParseExpression(rgInc.Match(line).Groups[1].Value, 0, linenum));
      parent.Add(node);
      return 1;
    }

    // [Dec]
    if (expected.IsGood(Expected.Val.Statement) && rgDec.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.Dec);
      node.Add(ParseExpression(rgDec.Match(line).Groups[1].Value, 0, linenum));
      parent.Add(node);
      return 1;
    }

    // [IF] ([EXP]) {[BLOCK]}
    if (expected.IsGood(Expected.Val.Statement) && rgIf.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.IF);
      Match m = rgIf.Match(line);
      string exp = m.Groups[1].Value;
      node.Add(ParseExpression(exp, 0, linenum));
      CodeNode b = new CodeNode(BNF.BLOCK);
      node.Add(b);

      string block = "";
      int numBrackets = 1;
      for (int i = lineidx + 1; i < lines.Length; i++) {
        block += lines[i] + "\n";
        if (rgBlockStart.IsMatch(lines[i])) numBrackets++;
        if (rgBlockEnd.IsMatch(lines[i])) numBrackets--;
        if (numBrackets == 0) break;
      }
      block = " " + block.Trim(' ', '{', '}', '\n') + " ";
      int num = ParseBlock(block, 1, block.Length  -1, b, linenum+1);
      parent.Add(node);

      // Is the next non-empty line an "else"?
      int pos = lineidx + num + 1;
      for (int li = pos; li < lines.Length; li++) {
        string l = lines[li].Trim(' ', '\n', '}');
        if (string.IsNullOrEmpty(l)) continue;
        else if (rgElse.IsMatch(l)) {
          CodeNode nElse = new CodeNode(BNF.IFelse);
          m = rgElse.Match(line);

          block = "";
          for (int i = pos + 2; i < lines.Length; i++) {
            num++;
            block += lines[i] + "\n";
            if (rgBlockEnd.IsMatch(lines[i])) break;
          }
          block = " " + block.Trim(' ', '{', '}', '\n') + " ";
          num += ParseBlock(block, 1, block.Length - 1, nElse, linenum + num - 1);
          node.Add(nElse);
          Debug.Log("match else: " + line + " <=> " + node);
          break;
        }
        else break;
      }

      return num + 1;
    }

    // [WHILE] ([EXP]) {[BLOCK]}
    if (expected.IsGood(Expected.Val.Statement) && rgWhile.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.WHILE);
      Match m = rgWhile.Match(line);
      string exp = m.Groups[1].Value;
      node.Add(ParseExpression(exp, 0, linenum));
      CodeNode b = new CodeNode(BNF.BLOCK);
      node.Add(b);

      string block = "";
      int numBrackets = 1;
      for (int i = lineidx + 1; i < lines.Length; i++) {
        block += lines[i] + "\n";
        if (rgBlockStart.IsMatch(lines[i])) numBrackets++;
        if (rgBlockEnd.IsMatch(lines[i])) numBrackets--;
        if (numBrackets == 0) break;
      }
      block = " " + block.Trim(' ', '{', '}', '\n') + " ";
      int num = ParseBlock(block, 1, block.Length - 1, b, linenum + 1);
      parent.Add(node);
      Debug.Log("match while: " + line + " <=> " + node);

      return num + 1;
    }

    // [REG]=a-z
    if (expected.IsGood(Expected.Val.MemReg) && rgVar.IsMatch(line)) {
      string var = rgVar.Match(line).Groups[1].Value.ToLowerInvariant();
      if (!reserverdKeywords.Contains(var)) {
        CodeNode node = new CodeNode(BNF.REG) { Reg = vars.Add(var) };
        parent.Add(node);
        return 1;
      }
    }

    // [MEM]= \[<exp>\] | \[<exp>@<exp>\]
    if (expected.IsGood(Expected.Val.MemReg) && rgMemUnparsed.IsMatch(line)) {
      CodeNode node = ParseExpression(rgMemUnparsed.Match(line).Value, 0, linenum);
      if (node.type != BNF.MEM && node.type != BNF.MEMlong && node.type != BNF.MEMlongb && node.type != BNF.MEMlongi && node.type != BNF.MEMlongf && node.type != BNF.MEMlongs)
        throw new Exception("Expected Memory,\nfound " + node.type + "  at line: " + linenum);
      parent.Add(node);
      Debug.Log("match Mem: " + line + " <=> " + node);
      return 1;
    }

    throw new Exception("Invalid code at " + (linenum - 1) + "\n" + line);
  }

  // [EXP] [OP] [EXP] | [PAR] | [REG] | [INT] | [FLT] | [MEM] | [UO] | [LEN] | deltaTime
  CodeNode ParseExpression(string line, int count, int linenum) {
    line = line.Trim(' ', '\t', '\r');

    // First get all REG, INT, FLT, MEM, DTIME, LEN and replace with specific chars
    // Then parse the structure (recursive)
    // Then build the final CodeNode

    // Replace DTIME => `DTx
    line = rgDeltat.Replace(line, m => {
      CodeNode n = new CodeNode(BNF.DTIME, GenId("DT"));
      nodes[n.id] = n;
      return n.id;
    });

    // Replace FLT => `FTx
    line = rgFloat.Replace(line, m => {
      float.TryParse(m.Value, out float fVal);
      CodeNode n = new CodeNode(BNF.FLT, GenId("FT")) {
        fVal = fVal
      };
      nodes[n.id] = n;
      return n.id;
    });

    // Replace HEX => `HXx
    line = rgHex.Replace(line, m => {
      CodeNode n = new CodeNode(BNF.HEX, GenId("HX")) {
        iVal = Convert.ToInt32("0" + m.Value, 16)
      };
      nodes[n.id] = n;
      return n.id;
    });

    // Replace COL => `CLx
    line = rgCol.Replace(line, m => {
      int.TryParse(m.Groups[1].Value, out int r);
      int.TryParse(m.Groups[2].Value, out int g);
      int.TryParse(m.Groups[3].Value, out int b);
      int a = 0;
      if (m.Groups.Count > 4 && !string.IsNullOrEmpty(m.Groups[4].Value)) int.TryParse(m.Groups[4].Value, out a);
      if (r > 3) r = 3;
      if (g > 3) g = 3;
      if (b > 3) b = 3;
      if (a > 3) a = 3;
      CodeNode n = new CodeNode(BNF.COL, GenId("CL")) {
        iVal = a * 64 + r * 16 + g * 4 + b
      };
      nodes[n.id] = n;
      return n.id;
    });

    // Replace INT => `INx
    line = rgInt.Replace(line, m => {
      int.TryParse(m.Value, out int iVal);
      CodeNode n = new CodeNode(BNF.INT, GenId("IN")) {
        iVal = iVal
      };
      nodes[n.id] = n;
      return n.id;
    });

    // Replace REG => `RGx (I cannot find a Regex to match single letters, so here I will do it by code)
    line = rgVar.Replace(line, m => {
      string var = m.Groups[1].Value.ToLowerInvariant();
      if (!reserverdKeywords.Contains(var)) {
        CodeNode n = new CodeNode(BNF.REG, GenId("RG")) {
          Reg = vars.Add(var)
        };
        nodes[n.id] = n;
        return n.id + m.Groups[2].Value;
      }
      return m.Value;
    });


    // Now the expression is somewhat simpler, because we have only specific terms. Now parse the operators

    bool atLeastOneReplacement = true;
    while (atLeastOneReplacement) {
      atLeastOneReplacement = false;

      // STR.len
      // Replace LEN => `LNx
      line = rgLen.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.LEN, GenId("LN"));
        if (m.Groups.Count < 2) throw new Exception("Unhandled LEN case: " + m.Groups.Count + " Line:" + linenum);
        string left = m.Groups[1].Value.Trim();
        n.Add(nodes[left]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // STR.plen
      // Replace LEN => `PLx
      line = rgPLen.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.PLEN, GenId("PL"));
        if (m.Groups.Count < 2) throw new Exception("Unhandled PLEN case: " + m.Groups.Count + " Line:" + linenum);
        string left = m.Groups[1].Value.Trim();
        n.Add(nodes[left]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;


      // MEM
      // Replace MEM => `MMx
      line = rgMem.Replace(line, m => { 
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEM, "MM", m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemL.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlong, "MD", m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemB.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongb, "MD", m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemI.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongi, "MD", m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemF.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongf, "MD", m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemS.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongs, "MD", m);
      });
      if (atLeastOneReplacement) continue;

      // !
      // Replace UOneg => `UNx

      Match mneg = rgUOneg.Match(line);
      while (mneg.Success) {
        atLeastOneReplacement = true;
        string toReplace = mneg.Captures[0].Value.Trim();
        if (toReplace[0] != '!') {
          for (int i = 0; i < mneg.Groups.Count; i++) {
            toReplace = mneg.Groups[i].Value.Trim();
            if (toReplace[0] == '!') break;
          }
          if (toReplace[0] != '!') throw new Exception("Invalid negation");
        }
        CodeNode n = new CodeNode(BNF.UOneg, GenId("UN"));
        n.Add(nodes[toReplace.Substring(1)]);
        nodes[n.id] = n;
        line = line.Replace(toReplace, n.id);
        mneg = rgUOsub.Match(line);
      }
      if (atLeastOneReplacement) continue;

      // ~
      // Replace UOinv => `UIx
      Match minv = rgUOinv.Match(line);
      while (minv.Success) {
        atLeastOneReplacement = true;
        string toReplace = minv.Captures[0].Value.Trim();
        if (toReplace[0] != '~') {
          for (int i = 0; i < minv.Groups.Count; i++) {
            toReplace = minv.Groups[i].Value.Trim();
            if (toReplace[0] == '~') break;
          }
          if (toReplace[0] != '~') throw new Exception("Invalid one complement");
        }
        CodeNode n = new CodeNode(BNF.UOinv, GenId("UI"));
        n.Add(nodes[toReplace.Substring(1)]);
        nodes[n.id] = n;
        line = line.Replace(toReplace, n.id);
        minv = rgUOsub.Match(line);
      }
      if (atLeastOneReplacement) continue;

      // *
      // Replace OPmul => `MLx
      line = rgMul.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPmul, "ML", "multiplication", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // /
      // Replace OPdiv => `DVx
      line = rgDiv.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPdiv, "DV", "division", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // %
      // Replace OPmod => `MOx
      line = rgMod.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPmod, "MO", "modulo", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // -
      // Replace OPsub => `SUx
      line = rgSub.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPsub, "SB", "subtraction", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // +
      // Replace OPsum => `ADx
      line = rgSum.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPsum, "AD", "addition", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // - (unary)
      // Replace UOsub => `USx
      Match msub = rgUOsub.Match(line);
      while (msub.Success) {
        atLeastOneReplacement = true;
        string toReplace = msub.Captures[0].Value.Trim();
        if (toReplace[0] != '-') {
          for (int i = 0; i < msub.Groups.Count; i++) {
            toReplace = msub.Groups[i].Value.Trim();
            if (toReplace[0] == '-') break;
          }
          if (toReplace[0] != '-') throw new Exception("Invalid negative value");
        }
        CodeNode n = new CodeNode(BNF.UOsub, GenId("US"));
        n.Add(nodes[toReplace.Substring(1)]);
        nodes[n.id] = n;
        line = line.Replace(toReplace, n.id);
        msub = rgUOsub.Match(line);
      }
      if (atLeastOneReplacement) continue;

      // &
      // Replace OPand => `ANx
      line = rgAnd.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPand, "AN", "AND", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // |
      // Replace OPor => `ORx
      line = rgOr.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPor, "OR", "OR", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // ^
      // Replace OPxor => `XOx
      line = rgXor.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPor, "XO", "XOR", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // _i => QI
      line = rgCastI.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTi, GenId("QI"));
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _i => QB
      line = rgCastB.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTb, GenId("QB"));
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _i => QI
      line = rgCastF.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTf, GenId("QF"));
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _s => QS
      line = rgCastS.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTs, GenId("QS"));
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;


      // [CMPeq] == => `EQx¶
      line = rgCMPeqTag.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleComparator("==", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // [CMPneq] == => `NQx¶
      line = rgCMPneqTag.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleComparator("!=", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // [CMPgt] == => `GTx¶
      line = rgCMPgtTag.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleComparator(">", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // [CMPgte] == => `GEx¶
      line = rgCMPgeqTag.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleComparator(">=", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // [CMPlt] == => `LTx¶
      line = rgCMPltTag.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleComparator("<", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // [CMPleq] == => `LEx¶
      line = rgCMPleqTag.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleComparator("<=", linenum, m);
      });
      if (atLeastOneReplacement) continue;

      // [KEY] ([EXP])
      line = rgKey.Replace(line, m => {
        atLeastOneReplacement = true;
        char type = m.Groups[1].Value.Trim().ToLowerInvariant()[0];
        string exp = m.Groups[2].Value.Trim();
        CodeNode n;
        if (type == 'l') n = new CodeNode(BNF.KEYl, GenId("KL"));
        else if (type == 'r') n = new CodeNode(BNF.KEYr, GenId("KR"));
        else if (type == 'u') n = new CodeNode(BNF.KEYu, GenId("KU"));
        else if (type == 'd') n = new CodeNode(BNF.KEYd, GenId("KD"));
        else if (type == 'a') n = new CodeNode(BNF.KEYa, GenId("KA"));
        else if (type == 'b') n = new CodeNode(BNF.KEYb, GenId("KB"));
        else if (type == 'c') n = new CodeNode(BNF.KEYc, GenId("KC"));
        else if (type == 'f') n = new CodeNode(BNF.KEYf, GenId("KF"));
        else if (type == 'e') n = new CodeNode(BNF.KEYe, GenId("KE"));
        else if (type == 'x') n = new CodeNode(BNF.KEYx, GenId("KX"));
        else if (type == 'y') n = new CodeNode(BNF.KEYy, GenId("KY"));
        else throw new Exception("Invalid Key at " + linenum + "\n" + line);
        if (!string.IsNullOrEmpty(exp)) n.Add(nodes[exp]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // PAR
      // Replace PAR => `PRx
      line = rgPars.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.OPpar, GenId("PR"));
        string child = m.Value.Trim(' ', '(', ')');
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;


    }

    line = line.Trim(' ', '\t', '\r');
    if (!nodes.ContainsKey(line)) {
      line = rgTag.Replace(line, "").Trim();
      throw new Exception("Invalid expression at " + (linenum - 1) + "\n" + line);
    }
    return nodes[line];
  }

  private string ParseMem(BNF bnf, string id, Match m) {
    CodeNode n = new CodeNode(bnf, GenId(id));
    string child = m.Value.Trim(' ', '[', ']');
    // strip the @ at end
    if (child.IndexOf('@') != -1) child = child.Substring(0, child.IndexOf('@'));
    n.Add(nodes[child]);
    nodes[n.id] = n;
    return n.id;
  }

  private string HandleComparator(string name, int linenum, Match m) {
    CodeNode n = nodes[m.Groups[2].Value.Trim()];
    if (m.Groups.Count < 4) throw new Exception("Unhandled " + name + " case: " + m.Groups.Count + " Line:" + linenum);
    string left = m.Groups[1].Value.Trim();
    string right = m.Groups[3].Value.Trim();
    n.Add(nodes[left]);
    n.Add(nodes[right]);
    return n.id;
  }

  private string HandleOperand(BNF bnf, string code, string name, int linenum, Match m) {
    CodeNode n = new CodeNode(bnf, GenId(code));
    if (m.Groups.Count < 4) throw new Exception("Unhandled " + name + " case: " + m.Groups.Count + " Line:" + linenum);
    string left = m.Groups[1].Value.Trim();
    string right = m.Groups[3].Value.Trim();
    n.Add(nodes[left]);
    n.Add(nodes[right]);
    nodes[n.id] = n;
    return n.id;
  }

  string GenId(string tag) {
    if (idcount == 0) {
      idcount = 1;
      return "`" + tag + "a¶";
    }
    string res = "";
    int num = idcount;
    while (num > 0) {
      int p = num % 26;
      res += (char)(97 + p);
      num -= p;
      num /= 26;
    }
    idcount++;
    return "`" + tag + res + "¶";
  }



}
