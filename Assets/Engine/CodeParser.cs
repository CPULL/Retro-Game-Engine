using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CodeParser : MonoBehaviour {
  Dictionary<string, CodeNode> nodes = null;
  Dictionary<string, CodeNode> functions = null;
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
    "len",
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
    "sprite",
    "spo",
    "",
    "",
    "",
    "",
  };

  #region Regex

  readonly Regex rgMLBacktick = new Regex("`", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgSLComment = new Regex("([^\\n]*)(//[^\\n]*)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgMLComment = new Regex("/\\*[\\s\\S]*?\\*/", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline, TimeSpan.FromSeconds(1));
  readonly Regex rgOpenBracket = new Regex("[\\s]*\\{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBlockOpen = new Regex(".*\\{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBlockClose = new Regex("^[\\s]*\\}[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgVar = new Regex("(?<=[^a-z0-9`]|^)([a-z][0-9a-z]{0,7})([^a-z0-9¶]|$)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgHex = new Regex("0x([0-9a-f]{8}|[0-9a-f]{4}|[0-9a-f]{2})", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgCol = new Regex("c([0-3])([0-3])([0-3])", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgQString = new Regex("\\\\\"", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgString = new Regex("(\")([^\"]*)(\")", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDeltat = new Regex("deltatime", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFloat = new Regex("[0-9]+\\.[0-9]+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgInt = new Regex("[0-9]+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBin = new Regex("b([0-1]{1,31})", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));

  readonly Regex rgPars = new Regex("\\((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!))\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMem = new Regex("\\[[\\s]*`[a-z]{3,}¶[\\s]*]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemL = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemB = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@b[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemI = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@i[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemF = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@f[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemS = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@s[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemUnparsed = new Regex("[\\s]*\\[.+\\][\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgUOneg = new Regex("(^([^0-9a-z\\*/\\<\\>\\)\\=&\\|\\^]*))(\\![\\s]*[a-z0-9\\.]+)($|[\\+\\-\\*/&\\|^\\s:\\)])", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgUOinv = new Regex("(^([^0-9a-z\\*/\\<\\>\\)\\=&\\|\\^]*))(\\~[\\s]*[a-z0-9\\.]+)($|[\\+\\-\\*/&\\|^\\s:\\)])", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgUOsub = new Regex("(^([^0-9a-z\\*/\\<\\>\\)\\=&\\|\\^]*))(\\-[\\s]*[a-z0-9\\.]+)($|[\\+\\-\\*/&\\|^\\s:\\)])", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

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
  readonly Regex rgWrite3 = new Regex("[\\s]*(write\\()(.*),(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgLine = new Regex("[\\s]*(line\\()(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBox1 = new Regex("[\\s]*(box\\()(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBox2 = new Regex("[\\s]*(box\\()(.*),(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCircle1 = new Regex("[\\s]*(circle\\()(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCircle2 = new Regex("[\\s]*(circle\\()(.*),(.*),(.*),(.*),(.*),(.*)\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgInc = new Regex("(.*)\\+\\+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDec = new Regex("(.*)\\-\\-", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgIf = new Regex("[\\s]*if[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*(.*)$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgElse = new Regex("[\\s]*else[\\s]*(.*)$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgWhile = new Regex("[\\s]*while[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*(.*)$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFor = new Regex("[\\s]*for[\\s]*\\(([^,]*=[^,]*)?,([^,]*)?,([^,]*)?\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgScreen = new Regex("[\\s]*screen[\\s]*\\(([^,]*),([^,]*)(,([^,]*)){0,1}(,([^,]*)){0,1}\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgWait = new Regex("[\\s]*wait[\\s]*\\(([^,]+)(,[\\s]*([fn]))?\\)[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDestroy = new Regex("[\\s]*destroy[\\s]*\\(([^,]+)\\)[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgSpriteSz = new Regex("[\\s]*sprite[\\s]*\\(([^,]*),([^,]*)(,[\\s]*[fn])?\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSprite = new Regex("[\\s]*sprite[\\s]*\\(([^,]*),([^,]*),([^,]*),([^,]*)(,[\\s]*[fn])?\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSpos = new Regex("[\\s]*spos[\\s]*\\(([^,]+),([^,]+),([^,]+)(,([^,]+))?\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSrot = new Regex("[\\s]*srot[\\s]*\\(([^,]+),([^,]+),([^,]+)?\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgCMPlt = new Regex("(`[a-z]{3,}¶)([\\s]*\\<[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPle = new Regex("(`[a-z]{3,}¶)([\\s]*\\<\\=[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPgt = new Regex("(`[a-z]{3,}¶)([\\s]*\\>[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPge = new Regex("(`[a-z]{3,}¶)([\\s]*\\>\\=[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPeq = new Regex("(`[a-z]{3,}¶)([\\s]*\\=\\=[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPne = new Regex("(`[a-z]{3,}¶)([\\s]*\\!\\=[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgOPlsh = new Regex("\\<\\<", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgOPrsh = new Regex("\\>\\>", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgKey = new Regex("[\\s]*key([udlrabcfexyhv]|fire|esc)([ud]?)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  //   keys -> U, D, L, R, A, B, C, D, X, Y, H, V, Fire, Esc
  readonly Regex rgLabel = new Regex("[\\s]*[a-z0-9]+:[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgConfScreen = new Regex("screen[\\s]*\\([\\s]*([0-9]+)[\\s]*,[\\s]*([0-9]+)[\\s]*(,[\\s]*[fn])?[\\s]*\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgRam = new Regex("ram[\\s]*\\([\\s]*([0-9]+)[\\s]*([bkm])?[\\s]*\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  #endregion Regex

  readonly Regex rgName = new Regex("^name:[\\s]*([a-z0-9_\\s]+)$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgStart = new Regex("^start[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgUpdate = new Regex("^update[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgData = new Regex("^data[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFunction = new Regex("^#([a-z][a-z0-9]{0,11})[\\s]*\\((.*)\\)[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFunctionCall = new Regex("([a-z][a-z0-9]{0,11})[\\s]*\\((.*)\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  int linenumber = 0;

  public CodeNode Parse(string file, Variables variables) {
    try {
      // Start by replacing all the problematic stuff
      file = file.Trim().Replace("\r", "").Replace("\t", " ");
      // [QuotedStrings]
      file = rgQString.Replace(file, "ˠ");
      // Remove multiline-comments, but keep the newlines
      file = rgMLComment.Replace(file, m => {
        string inside = m.Value;
        string nls = "";
        foreach (char c in inside)
          if (c == '\n') nls += "\n";
        return nls;
      });

      idcount = 0;
      CodeNode res = new CodeNode(BNF.Program, null, 0);
      nodes = new Dictionary<string, CodeNode>();
      functions = new Dictionary<string, CodeNode>();
      vars = variables;


      string[] lines = file.Split('\n');

      // Find first all function definitions
      CodeNode funcs = new CodeNode(BNF.Functions, "", 0);
      for (int linenumber = 0; linenumber < lines.Length; linenumber++) {
        string line = lines[linenumber].Trim();
        Match m = rgFunction.Match(line);
        if (m.Success) {
          CodeNode n = new CodeNode(BNF.FunctionDef, line, linenumber) { sVal = m.Groups[1].Value.Trim().ToLowerInvariant() };
          CodeNode ps = new CodeNode(BNF.Params, line, linenumber);
          n.Add(ps);
          funcs.Add(n);
          functions.Add(n.sVal, n);
          // Parse the parameters, the parsing of th ecode will be done later because in the code other functions can be called
          string pars = m.Groups[2].Value.Trim(' ', '(', ')');
          foreach (string par in rgVar.Split(pars)) {
            string p = par.Trim(' ', ',');
            if (!string.IsNullOrWhiteSpace(p)) {
              CodeNode v = new CodeNode(BNF.REG, par, linenumber) { sVal = p.ToLowerInvariant() };
              ps.Add(v);
            }
          }
        }
      }
      if (functions.Count > 0) res.Add(funcs);

      // Then the sections
      for (int linenumber = 0; linenumber < lines.Length; linenumber++) {
        string line = lines[linenumber];

        Match m = rgName.Match(line);
        if (m.Success) {
          res.sVal = m.Groups[1].Value.Trim();
          continue;
        }

        m = rgStart.Match(line);
        if (m.Success) {
          // find the end of the block, and parse the result
          int end = FindEndOfBlock(lines, linenumber);
          if (end == -1) throw new Exception("\"START\" section does not end");

          CodeNode start = new CodeNode(BNF.Start, line, linenumber);
          res.Add(start);
          ParseBlock(lines, linenumber + 1, end, start);
          continue;
        }

        m = rgUpdate.Match(line);
        if (m.Success) {
          // find the end of the block, and parse the result
          int end = FindEndOfBlock(lines, linenumber);
          if (end == -1) throw new Exception("\"UPDATE\" section does not end");

          CodeNode update = new CodeNode(BNF.Update, line, linenumber);
          res.Add(update);
          ParseBlock(lines, linenumber + 1, end, update);
          continue;
        }

        m = rgData.Match(line);
        if (m.Success) {
          // find the end of the block, and parse the result
          int end = FindEndOfBlock(lines, linenumber);
          if (end == -1) throw new Exception("\"DATA\" section does not end");

          CodeNode data = new CodeNode(BNF.Data, line, linenumber);
          res.Add(data);
          ParseDataBlock(lines, linenumber, end, data);
          continue;
        }

        m = rgFunction.Match(line);
        if (m.Success) {
          string fname = m.Groups[1].Value.Trim().ToLowerInvariant();
          CodeNode f = functions[fname];
          CodeNode b = new CodeNode(BNF.BLOCK, null, 0);
          f.Add(b);

          // find the end of the block, and parse the result
          int end = FindEndOfBlock(lines, linenumber);
          if (end == -1) throw new Exception("\"FUNCTION\" " + fname + " section does not end");

          ParseBlock(lines, linenumber + 1, end, b);
          continue;
        }
      }

      return res;
    } catch (Exception e) {
      Debug.Log(e.Message + "\nCurrent line = " + (linenumber + 1));
      throw e;
    }
  }

  int FindEndOfBlock(string[] lines, int start) {
    int num = 0;
    for (int i = start; i < lines.Length; i++) {
      string line = lines[i].Trim();
      if (string.IsNullOrEmpty(line)) continue;
      line = rgString.Replace(line, "");
      line = rgSLComment.Replace(line, "");
      line = rgMLComment.Replace(line, "");
      if (rgBlockClose.IsMatch(line)) {
        num--;
        if (num == 0) return i;
      }
      if (rgBlockOpen.IsMatch(line)) num++;
    }
    return -1;
  }

  private void ParseBlock(string[] lines, int start, int end, CodeNode parent) {
    // Follow the BNF rules to get the elements, one line at time
    for (linenumber = start; linenumber < end; linenumber++) {
      string line = lines[linenumber].Trim();
      line = rgSLComment.Replace(line, m => {
        if (m.Groups.Count > 1) return m.Groups[1].Value;
        return m.Value;
      });
      line = rgMLComment.Replace(line, "");
      if (string.IsNullOrEmpty(line)) continue;
      lines[linenumber] = line;
      expected.Set(Expected.Val.Statement);
      ParseLine(parent, lines);
    }
  }

  string origForException = "";
  string origExpForException = "";
  void ParseLine(CodeNode parent, string[] lines) {
    ParseLine(parent, lines[linenumber].Trim(' ', '\t', '\r'), lines);
  }

  void ParseLine(CodeNode parent, string line, string[] lines) {
    origForException = line;

    if (rgBlockClose.IsMatch(line)) return;

    // [STRING] STR => `STx¶
    line = rgString.Replace(line, m => {
      string str = m.Groups[2].Value;
      CodeNode n = new CodeNode(BNF.STR, GenId("ST"), line, linenumber) {
        sVal = str.Replace("ˠ", "\"")
      };
      nodes[n.id] = n;
      return n.id;
    });


    // Check what we have. Pick something in line with what is expected

    // [IF] ([EXP]) [BLOCK]|[STATEMENT] [ [ELSE] [BLOCK]|[STATEMENT] ]
    if (expected.IsGood(Expected.Val.Statement) && rgIf.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.IF, line, linenumber);
      Match m = rgIf.Match(line);
      string exp = m.Groups[1].Value;
      node.Add(ParseExpression(exp));
      parent.Add(node);

      // check if we have a block just after (same line or next non-empty line)
      string after = m.Groups[2].Value.Trim();
      if (rgBlockOpen.IsMatch(after) || string.IsNullOrEmpty(after)) { //[IF] ([EXP]) [BLOCK]
        ParseIfBlock(node, after, lines);
        return;
      }
      else if (!string.IsNullOrEmpty(after)) { // [IF] ([EXP]) [STATEMENT]
        CodeNode b = new CodeNode(BNF.BLOCK, line, linenumber);
        node.Add(b);
        ParseLine(b, new string[] { after });
        ParseElseBlock(node, lines);
        return;
      }

      throw new Exception("Invalid block after IF statement: " + (linenumber + 1));
    }

    // [FOR] {[BLOCK]}
    if (expected.IsGood(Expected.Val.Statement) && rgFor.IsMatch(line)) {
      Match m = rgFor.Match(line);
      CodeNode node = new CodeNode(BNF.FOR, line, linenumber);
      parent.Add(node);
      if (!string.IsNullOrEmpty(m.Groups[1].Value.Trim())) {
        ParseLine(node, new string[] { m.Groups[1].Value.Trim() });
      }
      else node.Add(new CodeNode(BNF.NOP, line, linenumber));

      if (!string.IsNullOrEmpty(m.Groups[2].Value.Trim())) {
        node.Add(ParseExpression(m.Groups[2].Value.Trim()));
      }
      else throw new Exception("FOR need to have a condition to terminate: " + (linenumber + 1));

      CodeNode b = new CodeNode(BNF.BLOCK, line, linenumber);
      int end = FindEndOfBlock(lines, linenumber);
      if (end < 0) throw new Exception("\"FOR\" section does not end");
      ParseBlock(lines, linenumber + 1, end, b);
      node.Add(b);

      if (!string.IsNullOrEmpty(m.Groups[3].Value.Trim())) { // The last parst is added at the end of the block
        ParseLine(b, new string[] { m.Groups[3].Value.Trim() });
      }
    }


    // [ASSp] = [MEM] += [EXPR] | [REG] += [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssSum.IsMatch(line)) {
      ParseAssignment(line, parent, BNF.ASSIGNsum, "+=");
      return;
    }

    // [ASSs] = [MEM] -= [EXPR] | [REG] -= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssSub.IsMatch(line)) {
      ParseAssignment(line, parent, BNF.ASSIGNsub, "-=");
      return;
    }

    // [ASSm] = [MEM] *= [EXPR] | [REG] *= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssMul.IsMatch(line)) {
      ParseAssignment(line, parent, BNF.ASSIGNmul, "*=");
      return;
    }

    // [ASSd] = [MEM] /= [EXPR] | [REG] /= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssDiv.IsMatch(line)) {
      ParseAssignment(line, parent, BNF.ASSIGNdiv, "/=");
      return;
    }

    // [ASSmod] = [MEM] %= [EXPR] | [REG] %= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssMod.IsMatch(line)) {
      ParseAssignment(line, parent, BNF.ASSIGNmod, "%=");
      return;
    }

    // [ASSand] = [MEM] &= [EXPR] | [REG] &= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssAnd.IsMatch(line)) {
      ParseAssignment(line, parent, BNF.ASSIGNand, "&=");
      return;
    }

    // [ASSor] = [MEM] %= [EXPR] | [REG] %= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssOr.IsMatch(line)) {
      ParseAssignment(line, parent, BNF.ASSIGNmod, "|=");
      return;
    }

    // [ASSxor] = [MEM] ^= [EXPR] | [REG] ^= [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssXor.IsMatch(line)) {
      ParseAssignment(line,  parent, BNF.ASSIGNmod, "^=");
      return;
    }

    // [ASS] = [MEM] = [EXPR] | [REG] = [EXP]
    if (expected.IsGood(Expected.Val.Statement) && rgAssign.IsMatch(line)) {
      string dests = line.Substring(0, line.IndexOf('='));
      string val = line.Substring(line.IndexOf('=') + 1);
      CodeNode node = new CodeNode(BNF.ASSIGN, line, linenumber);
      expected.Set(Expected.Val.MemReg);
      ParseLine(node, dests, null);
      parent.Add(node);
      node.Add(ParseExpression(val));
      return;
    }

    // [CLR] = clr([EXPR])
    if (expected.IsGood(Expected.Val.Statement) && rgClr.IsMatch(line)) {
      Match m = rgClr.Match(line);
      if (m.Groups.Count < 2) throw new Exception("Invalid Clr() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.CLR, line, linenumber);
      node.Add(ParseExpression(m.Groups[1].Value));
      parent.Add(node);
      return;
    }

    // [WRITE] = write([EXPR], [EXPR], [EXPR], [EXPR], [EXPR]) ; text, x, y, col(front), col(back)
    if (expected.IsGood(Expected.Val.Statement)) {
      if (rgWrite3.IsMatch(line)) {
        Match m = rgWrite3.Match(line);
        if (m.Groups.Count < 8) throw new Exception("Invalid Write() command. Line: " + (linenumber + 1));
        CodeNode node = new CodeNode(BNF.WRITE, line, linenumber);
        node.Add(ParseExpression(m.Groups[2].Value));
        node.Add(ParseExpression(m.Groups[3].Value));
        node.Add(ParseExpression(m.Groups[4].Value));
        node.Add(ParseExpression(m.Groups[5].Value));
        node.Add(ParseExpression(m.Groups[6].Value));
        node.Add(ParseExpression(m.Groups[7].Value));
        parent.Add(node);
        return;
      }
      else if (rgWrite2.IsMatch(line)) {
        Match m = rgWrite2.Match(line);
        if (m.Groups.Count < 7) throw new Exception("Invalid Write() command. Line: " + (linenumber + 1));
        CodeNode node = new CodeNode(BNF.WRITE, line, linenumber);
        node.Add(ParseExpression(m.Groups[2].Value));
        node.Add(ParseExpression(m.Groups[3].Value));
        node.Add(ParseExpression(m.Groups[4].Value));
        node.Add(ParseExpression(m.Groups[5].Value));
        node.Add(ParseExpression(m.Groups[6].Value));
        parent.Add(node);
        return;
      }
      else if (rgWrite1.IsMatch(line)) {
        Match m = rgWrite1.Match(line);
        if (m.Groups.Count < 6) throw new Exception("Invalid Write() command. Line: " + (linenumber + 1));
        CodeNode node = new CodeNode(BNF.WRITE, line, linenumber);
        node.Add(ParseExpression(m.Groups[2].Value));
        node.Add(ParseExpression(m.Groups[3].Value));
        node.Add(ParseExpression(m.Groups[4].Value));
        node.Add(ParseExpression(m.Groups[5].Value));
        parent.Add(node);
        return;
      }
    }

    // [LINE] = line([EXPR], [EXPR], [EXPR], [EXPR], [EXPR])
    if (expected.IsGood(Expected.Val.Statement) && rgLine.IsMatch(line)) {
      Match m = rgLine.Match(line);
      if (m.Groups.Count < 7) throw new Exception("Invalid Line() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.LINE, line, linenumber);
      node.Add(ParseExpression(m.Groups[2].Value));
      node.Add(ParseExpression(m.Groups[3].Value));
      node.Add(ParseExpression(m.Groups[4].Value));
      node.Add(ParseExpression(m.Groups[5].Value));
      node.Add(ParseExpression(m.Groups[6].Value));
      parent.Add(node);
      return;
    }

    // [BOX] = box([EXP], [EXP], [EXP], [EXP], [EXP], [[EXP]])
    if (expected.IsGood(Expected.Val.Statement)) {
      if (rgBox2.IsMatch(line)) {
        Match m = rgBox2.Match(line);
        if (m.Groups.Count < 8) throw new Exception("Invalid Box() command. Line: " + (linenumber + 1));
        CodeNode node = new CodeNode(BNF.BOX, line, linenumber);
        node.Add(ParseExpression(m.Groups[2].Value));
        node.Add(ParseExpression(m.Groups[3].Value));
        node.Add(ParseExpression(m.Groups[4].Value));
        node.Add(ParseExpression(m.Groups[5].Value));
        node.Add(ParseExpression(m.Groups[6].Value));
        node.Add(ParseExpression(m.Groups[7].Value));
        parent.Add(node);
        return;
      }
      if (rgBox1.IsMatch(line)) {
        Match m = rgBox1.Match(line);
        if (m.Groups.Count < 7) throw new Exception("Invalid Box() command. Line: " + (linenumber + 1));
        CodeNode node = new CodeNode(BNF.BOX, line, linenumber);
        node.Add(ParseExpression(m.Groups[2].Value));
        node.Add(ParseExpression(m.Groups[3].Value));
        node.Add(ParseExpression(m.Groups[4].Value));
        node.Add(ParseExpression(m.Groups[5].Value));
        node.Add(ParseExpression(m.Groups[6].Value));
        parent.Add(node);
        return;
      }
    }

    // [CIRCLE] = circle([EXP], [EXP], [EXP], [EXP], [EXP], [[EXP]])
    if (expected.IsGood(Expected.Val.Statement)) {
      if (rgCircle2.IsMatch(line)) {
        Match m = rgCircle2.Match(line);
        if (m.Groups.Count < 8) throw new Exception("Invalid Circle() command. Line: " + (linenumber + 1));
        CodeNode node = new CodeNode(BNF.CIRCLE, line, linenumber);
        node.Add(ParseExpression(m.Groups[2].Value));
        node.Add(ParseExpression(m.Groups[3].Value));
        node.Add(ParseExpression(m.Groups[4].Value));
        node.Add(ParseExpression(m.Groups[5].Value));
        node.Add(ParseExpression(m.Groups[6].Value));
        node.Add(ParseExpression(m.Groups[7].Value));
        parent.Add(node);
        return;
      }
      if (rgCircle1.IsMatch(line)) {
        Match m = rgCircle1.Match(line);
        if (m.Groups.Count < 7) throw new Exception("Invalid Circle() command. Line: " + (linenumber + 1));
        CodeNode node = new CodeNode(BNF.CIRCLE, line, linenumber);
        node.Add(ParseExpression(m.Groups[2].Value));
        node.Add(ParseExpression(m.Groups[3].Value));
        node.Add(ParseExpression(m.Groups[4].Value));
        node.Add(ParseExpression(m.Groups[5].Value));
        node.Add(ParseExpression(m.Groups[6].Value));
        parent.Add(node);
        return;
      }
    }

    // [SCREEN] width, heigth, tiles, filter
    if (expected.IsGood(Expected.Val.Statement) && rgScreen.IsMatch(line)) {
      Match m = rgScreen.Match(line);
      if (m.Groups.Count < 3) throw new Exception("Invalid Screen() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.SCREEN, line, linenumber);
      node.Add(ParseExpression(m.Groups[1].Value));
      node.Add(ParseExpression(m.Groups[2].Value));
      if (m.Groups.Count > 4 && !string.IsNullOrEmpty(m.Groups[4].Value)) node.Add(ParseExpression(m.Groups[4].Value));
      if (m.Groups.Count > 6 && !string.IsNullOrEmpty(m.Groups[6].Value)) node.Add(ParseExpression(m.Groups[6].Value));
      parent.Add(node);
      return;
    }

    // [SPRITE] num, width, heigth, pointer[, filter]
    if (expected.IsGood(Expected.Val.Statement) && rgSprite.IsMatch(line)) {
      Match m = rgSprite.Match(line);
      if (m.Groups.Count < 5) throw new Exception("Invalid Sprite() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.SPRITE, line, linenumber);
      node.Add(ParseExpression(m.Groups[1].Value));
      node.Add(ParseExpression(m.Groups[2].Value));
      node.Add(ParseExpression(m.Groups[3].Value));
      node.Add(ParseExpression(m.Groups[4].Value));
      if (m.Groups.Count > 5 && !string.IsNullOrEmpty(m.Groups[5].Value)) node.sVal = "*";
      parent.Add(node);
      return;
    }

    // [SPRITE] num, pointer[, filter]
    if (expected.IsGood(Expected.Val.Statement) && rgSpriteSz.IsMatch(line)) {
      Match m = rgSpriteSz.Match(line);
      if (m.Groups.Count < 3) throw new Exception("Invalid Sprite() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.SPRITE, line, linenumber);
      node.Add(ParseExpression(m.Groups[1].Value));
      node.Add(ParseExpression(m.Groups[2].Value));
      if (m.Groups.Count > 3 && !string.IsNullOrEmpty(m.Groups[3].Value)) node.sVal = "*";
      parent.Add(node);
      return;
    }

    // [SPOS] num, x, y[, enble]
    if (expected.IsGood(Expected.Val.Statement) && rgSpos.IsMatch(line)) {
      Match m = rgSpos.Match(line);
      if (m.Groups.Count < 4) throw new Exception("Invalid SPos() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.SPOS, line, linenumber);
      node.Add(ParseExpression(m.Groups[1].Value));
      node.Add(ParseExpression(m.Groups[2].Value));
      node.Add(ParseExpression(m.Groups[3].Value));
      if (m.Groups.Count > 5 && !string.IsNullOrEmpty(m.Groups[5].Value)) node.Add(ParseExpression(m.Groups[5].Value));
      parent.Add(node);
      return;
    }

    // [SROT] num, dir, flip
    if (expected.IsGood(Expected.Val.Statement) && rgSrot.IsMatch(line)) {
      Match m = rgSrot.Match(line);
      if (m.Groups.Count < 4) throw new Exception("Invalid SRot() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.SROT, line, linenumber);
      node.Add(ParseExpression(m.Groups[1].Value));
      node.Add(ParseExpression(m.Groups[2].Value));
      node.Add(ParseExpression(m.Groups[3].Value));
      parent.Add(node);
      return;
    }

    // [FRAME]
    if (expected.IsGood(Expected.Val.Statement) && rgFrame.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.FRAME, line, linenumber);
      parent.Add(node);
      return;
    }

    // [Inc]
    if (expected.IsGood(Expected.Val.Statement) && rgInc.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.Inc, line, linenumber);
      node.Add(ParseExpression(rgInc.Match(line).Groups[1].Value));
      parent.Add(node);
      return;
    }

    // [Dec]
    if (expected.IsGood(Expected.Val.Statement) && rgDec.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.Dec, line, linenumber);
      node.Add(ParseExpression(rgDec.Match(line).Groups[1].Value));
      parent.Add(node);
      return;
    }

    // [Destroy] ([EXP])
    if (expected.IsGood(Expected.Val.Statement) && rgDestroy.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.DESTROY, line, linenumber);
      Match m = rgDestroy.Match(line);
      string exp = m.Groups[1].Value;
      node.Add(ParseExpression(exp));
      parent.Add(node);
      return;
    }

    // [WAIT] ([EXP])
    if (expected.IsGood(Expected.Val.Statement) && rgWait.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.WAIT, line, linenumber);
      Match m = rgWait.Match(line);
      string exp = m.Groups[1].Value;
      node.Add(ParseExpression(exp));
      if ((m.Groups[3].Value.Trim() + " ").ToLowerInvariant()[0] == 'f') node.sVal = "*";
      parent.Add(node);
      return;
    }

    // [WHILE] ([EXP]) {[BLOCK]}
    if (expected.IsGood(Expected.Val.Statement) && rgWhile.IsMatch(line)) {
      CodeNode node = new CodeNode(BNF.WHILE, line, linenumber);
      Match m = rgWhile.Match(line);
      string exp = m.Groups[1].Value;
      node.Add(ParseExpression(exp));
      parent.Add(node);

      // check if we have a block just after (same line or next non-empty line)
      string after = m.Groups[2].Value.Trim();
      if (rgBlockOpen.IsMatch(after) || string.IsNullOrEmpty(after)) { //[WHILE] ([EXP]) [BLOCK]
        ParseWhileBlock(node, after, lines);
        return;
      }
      else if (!string.IsNullOrEmpty(after)) { // [WHILE] ([EXP]) [STATEMENT]
        CodeNode b = new CodeNode(BNF.BLOCK, line, linenumber);
        node.Add(b);
        ParseLine(b, new string[] { after });
        return;
      }

      throw new Exception("Invalid block after WHILE statement: " + (linenumber + 1));
    }


    // [REG]=a-z[a-z0-9]*
    if (expected.IsGood(Expected.Val.MemReg) && rgVar.IsMatch(line)) {
      string var = rgVar.Match(line).Groups[1].Value.ToLowerInvariant();
      if (!reserverdKeywords.Contains(var)) {
        CodeNode node = new CodeNode(BNF.REG, line, linenumber) { Reg = vars.Add(var) };
        parent.Add(node);
        return;
      }
    }

    // [MEM]= \[<exp>\] | \[<exp>@<exp>\]
    if (expected.IsGood(Expected.Val.MemReg) && rgMemUnparsed.IsMatch(line)) {
      CodeNode node = ParseExpression(rgMemUnparsed.Match(line).Value);
      if (node.type != BNF.MEM && node.type != BNF.MEMlong && node.type != BNF.MEMlongb && node.type != BNF.MEMlongi && node.type != BNF.MEMlongf && node.type != BNF.MEMlongs)
        throw new Exception("Expected Memory,\nfound " + node.type + "  at line: " + (linenumber + 1));
      parent.Add(node);
      Debug.Log("match Mem: " + line + " <=> " + node);
      return;
    }

    if (rgOpenBracket.IsMatch(line)) return;


    if (expected.IsGood(Expected.Val.Statement) && rgFunctionCall.IsMatch(line)) {
      Match fm = rgFunctionCall.Match(line);
      string fnc = fm.Groups[1].Value.ToLowerInvariant();
      if (!reserverdKeywords.Contains(fnc)) {
        CodeNode node = new CodeNode(BNF.FunctionCall, line, linenumber) { sVal = fnc };
        parent.Add(node);
        // Parse the parameters and evaluate as expressions
        CodeNode ps = new CodeNode(BNF.Params, line, linenumber);
        node.Add(ps);
        string pars = fm.Groups[2].Value.Trim(' ', '(', ')');

        // We need to grab each single parameter, they are separated by commas (,) but other functions can be nested
        int nump = 0;
        string parline = "";
        CodeNode v;
        foreach (char c in pars) {
          if (c == '(') nump++;
          else if (c == ')') nump--;
          else if (c == ',' && nump == 0) {
            // Parse
            parline = parline.Trim(' ', ',');
            v = ParseExpression(parline);
            ps.Add(v);
            parline = "";
          }
          else parline += c;
        }
        // parse the remaining part
        parline = parline.Trim(' ', ',');
        if (!string.IsNullOrEmpty(parline)) {
          v = ParseExpression(parline);
          ps.Add(v);
        }

        if ((functions[fnc].CN1 == null && ps.children != null) || (functions[fnc].CN1.children?.Count != ps.children?.Count))
          throw new Exception("Function " + fnc + " has\na wrong number of parameters\n" + (linenumber + 1) + ": " + origForException);
        return;
      }
    }



    throw new Exception("Invalid code at " + (linenumber + 1) + "\n" + origForException);
  }



  void ParseIfBlock(CodeNode ifNode, string after, string[] lines) {
    // Block or single line?
    if (rgBlockOpen.IsMatch(after) || string.IsNullOrEmpty(after)) { // [IF] [BLOCK]
      CodeNode b = new CodeNode(BNF.BLOCK, after, linenumber);
      int end = FindEndOfBlock(lines, linenumber);
      if (end < 0) throw new Exception("\"IF\" section does not end");
      ParseBlock(lines, linenumber + 1, end, b);
      ifNode.Add(b);
      linenumber = end + 1;
    }
    else { // [IF] [STATEMENT]
      CodeNode b = new CodeNode(BNF.BLOCK, after, linenumber);
      ifNode.Add(b);
      ParseLine(b, new string[] { after });
      linenumber++;
    }
    ParseElseBlock(ifNode, lines);
  }


  void ParseElseBlock(CodeNode ifNode, string[] lines) {
    // Is the next non-empty line an "else"?
    for (int pos = linenumber; pos < lines.Length; pos++) {
      string l = lines[pos].Trim();
      if (string.IsNullOrEmpty(l)) continue;
      Match m = rgElse.Match(l);
      if (m.Success) {
        // Block or single line?
        string after = m.Groups[1].Value.Trim();
        CodeNode nElse = new CodeNode(BNF.BLOCK, after, linenumber);

        if (rgBlockOpen.IsMatch(after)) {  // [ELSE] {
          int end = FindEndOfBlock(lines, linenumber);
          if (end < 0) throw new Exception("\"ELSE\" section does not end");
          ParseBlock(lines, linenumber + 1, end, nElse);
          linenumber = end + 1;
          ifNode.Add(nElse);
          return;
        }
        if (string.IsNullOrEmpty(after)) { // [ELSE] \n* ({ | [^{ ])
          for (int i = pos + 1; i < lines.Length; i++) {
            l = lines[pos].Trim();
            if (string.IsNullOrEmpty(l)) continue;
            if (rgOpenBracket.IsMatch(l)) { // [ELSE] \n* {
              int end = FindEndOfBlock(lines, linenumber);
              if (end < 0) throw new Exception("\"ELSE\" section does not end");
              ParseBlock(lines, linenumber + 1, end, nElse);
              linenumber = end + 1;
              ifNode.Add(nElse);
              return;
            }
            else { // [ELSE] \n* [^{ ]
              linenumber = i;
              ParseLine(nElse, lines);
              linenumber++;
              ifNode.Add(nElse);
              return;
            }
          }
        }
      }
      else return; // No else
    }
  }


  void ParseWhileBlock(CodeNode ifNode, string after, string[] lines) {
    // Block or single line?
    if (rgBlockOpen.IsMatch(after) || string.IsNullOrEmpty(after)) { // [WHILE] [BLOCK]
      CodeNode b = new CodeNode(BNF.BLOCK, after, linenumber);
      int end = FindEndOfBlock(lines, linenumber);
      if (end < 0) throw new Exception("\"WHILE\" section does not end");
      ParseBlock(lines, linenumber + 1, end, b);
      ifNode.Add(b);
      linenumber = end + 1;
    }
    else { // [WHILE] [STATEMENT]
      CodeNode b = new CodeNode(BNF.BLOCK, after, linenumber);
      ifNode.Add(b);
      ParseLine(b, new string[] { after });
      linenumber++;
    }
  }

  // [EXP] [OP] [EXP] | [PAR] | [REG] | [INT] | [FLT] | [MEM] | [UO] | [LEN] | deltaTime
  CodeNode ParseExpression(string line) {
    line = line.Trim(' ', '\t', '\r');
    origExpForException = line;

    // First get all REG, INT, FLT, MEM, DTIME, LEN and replace with specific chars
    // Then parse the structure (recursive)
    // Then build the final CodeNode

    // - (unary)
    line = rgUOsub.Replace(line, m => {
      string toReplace = m.Captures[0].Value.Trim();
      toReplace.Trim();
      if (toReplace[0] != '-') throw new Exception("Invalid negative value: " + toReplace);
      toReplace = toReplace.Substring(1).Trim();
      CodeNode n = new CodeNode(BNF.UOsub, GenId("US"), origExpForException, linenumber);
      CodeNode exp = ParseExpression(toReplace);
      if (exp.type == BNF.INT) {
        n = exp;
        n.iVal = -n.iVal;
      }
      else if (exp.type == BNF.FLT) {
        n = exp;
        n.fVal = -n.fVal;
      }
      else
        n.Add(exp);
      nodes[n.id] = n;
      return n.id;
    });
        
    // !
    line = rgUOneg.Replace(line, m => {
      string toReplace = m.Captures[0].Value.Trim();
      toReplace.Trim();
      if (toReplace[0] != '!') throw new Exception("Invalid negation: " + toReplace);
      toReplace = toReplace.Substring(1).Trim();
      CodeNode n = new CodeNode(BNF.UOneg, GenId("US"), origExpForException, linenumber);
      CodeNode exp = ParseExpression(toReplace);
      n.Add(exp);
      nodes[n.id] = n;
      return n.id;
    });

    // ~
    line = rgUOinv.Replace(line, m => {
      string toReplace = m.Captures[0].Value.Trim();
      toReplace.Trim();
      if (toReplace[0] != '~') throw new Exception("Invalid unary complement: " + toReplace);
      toReplace = toReplace.Substring(1).Trim();
      CodeNode n = new CodeNode(BNF.UOinv, GenId("US"), origExpForException, linenumber);
      CodeNode exp = ParseExpression(toReplace);
      n.Add(exp);
      nodes[n.id] = n;
      return n.id;
    });

    // Replace DTIME => `DTx
    line = rgDeltat.Replace(line, m => {
      CodeNode n = new CodeNode(BNF.DTIME, GenId("DT"), origExpForException, linenumber);
      nodes[n.id] = n;
      return n.id;
    });

    // Replace FLT => `FTx
    line = rgFloat.Replace(line, m => {
      float.TryParse(m.Value, out float fVal);
      CodeNode n = new CodeNode(BNF.FLT, GenId("FT"), origExpForException, linenumber) {
        fVal = fVal
      };
      nodes[n.id] = n;
      return n.id;
    });

    // Replace HEX => `HXx
    line = rgHex.Replace(line, m => {
      CodeNode n = new CodeNode(BNF.HEX, GenId("HX"), origExpForException, linenumber) {
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
      CodeNode n = new CodeNode(BNF.COL, GenId("CL"), origExpForException, linenumber) {
        iVal = a * 64 + r * 16 + g * 4 + b
      };
      nodes[n.id] = n;
      return n.id;
    });

    // LAB
    line = rgLabel.Replace(line, m => {
      CodeNode n = new CodeNode(BNF.LAB, GenId("LB"), origExpForException, linenumber) {
        sVal = m.Value.Trim().ToLowerInvariant()
      };
      nodes[n.id] = n;
      return n.id;
    });

    // Replace REG => `RGx
    line = rgVar.Replace(line, m => {
      string var = m.Groups[1].Value.ToLowerInvariant();
      if (!reserverdKeywords.Contains(var)) {
        CodeNode n = new CodeNode(BNF.REG, GenId("RG"), origExpForException, linenumber) {
          Reg = vars.Add(var)
        };
        nodes[n.id] = n;
        return n.id + m.Groups[2].Value;
      }
      return m.Value;
    });

    // Replace INT => `INx
    line = rgInt.Replace(line, m => {
      int.TryParse(m.Value, out int iVal);
      CodeNode n = new CodeNode(BNF.INT, GenId("IN"), origExpForException, linenumber) {
        iVal = iVal
      };
      nodes[n.id] = n;
      return n.id;
    });


    // Now the expression is somewhat simpler, because we have only specific terms. Now parse the operators

    bool atLeastOneReplacement = true;
    while (atLeastOneReplacement) {
      atLeastOneReplacement = false;

      // PAR
      // Replace PAR => `PRx
      line = rgPars.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.OPpar, GenId("PR"), origExpForException, linenumber);
        string inner = m.Value.Trim();
        inner = inner.Substring(1, inner.Length - 2);
        n.Add(ParseExpression(inner));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // STR.len
      // Replace LEN => `LNx
      line = rgLen.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.LEN, GenId("LN"), origExpForException, linenumber);
        if (m.Groups.Count < 2) throw new Exception("Unhandled LEN case: " + m.Groups.Count + " Line:" + (linenumber + 1));
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
        CodeNode n = new CodeNode(BNF.PLEN, GenId("PL"), origExpForException, linenumber);
        if (m.Groups.Count < 2) throw new Exception("Unhandled PLEN case: " + m.Groups.Count + " Line:" + (linenumber + 1));
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
        return ParseMem(BNF.MEM, "MM", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemL.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlong, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemB.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongb, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemI.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongi, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemF.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongf, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@ => `MDx
      line = rgMemS.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongs, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // [KEY] ([EXP])
      line = rgKey.Replace(line, m => {
        atLeastOneReplacement = true;
        char type = m.Groups[1].Value.Trim().ToLowerInvariant()[0];
        string mode = m.Groups[2].Value.Trim();
        int pos = string.IsNullOrEmpty(mode) ? 0 : (mode.ToLowerInvariant()[0] == 'd' ? 1 : 2);
        CodeNode n;
        switch (type) {
          case 'l': pos += 0; break;
          case 'r': pos += 3; break;
          case 'u': pos += 6; break;
          case 'd': pos += 9; break;
          case 'a': pos += 12; break;
          case 'b': pos += 15; break;
          case 'c': pos += 18; break;
          case 'f': pos += 21; break;
          case 'e': pos += 24; break;
          case 'x':
            n = new CodeNode(BNF.KEYx, GenId("KX"), origExpForException, linenumber);
            nodes[n.id] = n;
            return n.id;
          case 'y':
            n = new CodeNode(BNF.KEYy, GenId("KY"), origExpForException, linenumber);
            nodes[n.id] = n;
            return n.id;
          default: throw new Exception("Invalid Key at " + (linenumber + 1) + "\n" + line);
        }
        n = new CodeNode(BNF.KEY, GenId("KK"), origExpForException, linenumber) { iVal = pos };
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // [<<] == => `LSx¶
      line = rgOPlsh.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.COMPne, "SL", "<<", m);
      });
      if (atLeastOneReplacement) continue;

      // [>>] == => `RSx¶
      line = rgOPrsh.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.COMPne, "SR", ">>", m);
      });
      if (atLeastOneReplacement) continue;

      // *
      // Replace OPmul => `MLx
      line = rgMul.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPmul, "ML", "multiplication", m);
      });
      if (atLeastOneReplacement) continue;

      // /
      // Replace OPdiv => `DVx
      line = rgDiv.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPdiv, "DV", "division", m);
      });
      if (atLeastOneReplacement) continue;

      // %
      // Replace OPmod => `MOx
      line = rgMod.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPmod, "MO", "modulo", m);
      });
      if (atLeastOneReplacement) continue;

      // -
      // Replace OPsub => `SUx
      line = rgSub.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPsub, "SB", "subtraction", m);
      });
      if (atLeastOneReplacement) continue;

      // +
      // Replace OPsum => `ADx
      line = rgSum.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPsum, "AD", "addition", m);
      });
      if (atLeastOneReplacement) continue;

      // _i => QI
      line = rgCastI.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTi, GenId("QI"), origExpForException, linenumber);
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _i => QB
      line = rgCastB.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTb, GenId("QB"), origExpForException, linenumber);
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _i => QI
      line = rgCastF.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTf, GenId("QF"), origExpForException, linenumber);
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _s => QS
      line = rgCastS.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTs, GenId("QS"), origExpForException, linenumber);
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;


      // < => `LTx¶
      line = rgCMPlt.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.COMPlt, "LT", "<", m);
      });
      if (atLeastOneReplacement) continue;

      // <= => `LEx¶
      line = rgCMPle.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.COMPle, "LE", "<=", m);
      });
      if (atLeastOneReplacement) continue;

      // < => `GTx¶
      line = rgCMPgt.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.COMPlt, "GT", ">", m);
      });
      if (atLeastOneReplacement) continue;

      // <= => `GEx¶
      line = rgCMPge.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.COMPge, "GE", "=>", m);
      });
      if (atLeastOneReplacement) continue;

      // == => `EQx¶
      line = rgCMPeq.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.COMPeq, "EQ", "==", m);
      });
      if (atLeastOneReplacement) continue;

      // != => `NEx¶
      line = rgCMPne.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.COMPne, "NE", "!=", m);
      });
      if (atLeastOneReplacement) continue;


      // &
      // Replace OPand => `ANx
      line = rgAnd.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPand, "AN", "AND", m);
      });
      if (atLeastOneReplacement) continue;

      // |
      // Replace OPor => `ORx
      line = rgOr.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPor, "OR", "OR", m);
      });
      if (atLeastOneReplacement) continue;

      // ^
      // Replace OPxor => `XOx
      line = rgXor.Replace(line, m => {
        atLeastOneReplacement = true;
        return HandleOperand(BNF.OPor, "XO", "XOR", m);
      });
      if (atLeastOneReplacement) continue;

    }

    line = line.Trim(' ', '\t', '\r');
    if (!nodes.ContainsKey(line)) {
      line = rgTag.Replace(line, "").Trim();
      throw new Exception("Invalid expression at " + (linenumber + 1) + "\n" + origExpForException + "\n" + line);
    }
    return nodes[line];
  }

  void ParseAssignment(string line, CodeNode parent, BNF bnf, string match) {
    string dest = line.Substring(0, line.IndexOf(match));
    string val = line.Substring(line.IndexOf(match) + 2);
    CodeNode node = new CodeNode(bnf, line, linenumber);
    expected.Set(Expected.Val.MemReg);
    ParseLine(node, dest, null);
    parent.Add(node);
    node.Add(ParseExpression(val));
  }

  private string ParseMem(BNF bnf, string id, string line, Match m) {
    CodeNode n = new CodeNode(bnf, GenId(id), line, linenumber);
    string child = m.Value.Trim(' ', '[', ']');
    // strip the @ at end
    if (child.IndexOf('@') != -1) child = child.Substring(0, child.IndexOf('@'));
    n.Add(nodes[child]);
    nodes[n.id] = n;
    return n.id;
  }

  private string HandleOperand(BNF bnf, string code, string name, Match m) {
    if (m.Groups.Count < 4) throw new Exception("Unhandled " + name + " case: " + m.Groups.Count + " Line:" + (linenumber + 1));
    CodeNode left = nodes[m.Groups[1].Value.Trim()];
    CodeNode right = nodes[m.Groups[3].Value.Trim()];
    if ((left.type == BNF.INT || left.type == BNF.FLT || left.type == BNF.OPpar) && (right.type == BNF.INT || right.type == BNF.FLT || right.type == BNF.OPpar)) {
      CodeNode s = SimplifyNode(left, right, bnf);
      if (s != null) return s.id;
    }

    CodeNode n = new CodeNode(bnf, GenId(code), code, linenumber);
    n.Add(left);
    n.Add(right);
    nodes[n.id] = n;
    return n.id;
  }

  CodeNode SimplifyNode(CodeNode l, CodeNode r, BNF op) {
    while (l.type == BNF.OPpar && (l.CN1.type == BNF.INT || l.CN1.type == BNF.FLT || l.CN1.type == BNF.OPpar)) l = l.CN1;
    while (r.type == BNF.OPpar && (r.CN1.type == BNF.INT || r.CN1.type == BNF.FLT || r.CN1.type == BNF.OPpar)) r = r.CN1;

    bool lf = l.type == BNF.FLT;
    bool rf = r.type == BNF.FLT;

    switch(op) {
      case BNF.OPsum: {
        if (lf && rf)   { l.type = BNF.FLT; l.fVal += r.fVal; }
        if (!lf && rf)  { l.type = BNF.FLT; l.fVal = l.iVal + r.fVal; }
        if (lf && !rf)  { l.type = BNF.FLT; l.fVal += r.iVal; }
        if (!lf && !rf) { l.type = BNF.INT; l.iVal += r.iVal; }
      }
      break;
      case BNF.OPsub: {
        if (lf && rf)   { l.type = BNF.FLT; l.fVal -= r.fVal; }
        if (!lf && rf)  { l.type = BNF.FLT; l.fVal = l.iVal - r.fVal; }
        if (lf && !rf)  { l.type = BNF.FLT; l.fVal -= r.iVal; }
        if (!lf && !rf) { l.type = BNF.INT; l.iVal -= r.iVal; }
      }
      break;
      case BNF.OPmul: {
        if (lf && rf)   { l.type = BNF.FLT; l.fVal *= r.fVal; }
        if (!lf && rf)  { l.type = BNF.FLT; l.fVal = l.iVal * r.fVal; }
        if (lf && !rf)  { l.type = BNF.FLT; l.fVal *= r.iVal; }
        if (!lf && !rf) { l.type = BNF.INT; l.iVal *= r.iVal; }
      }
      break;
      case BNF.OPdiv: {
        if (lf && rf)   { l.type = BNF.FLT; l.fVal *= r.fVal; }
        if (!lf && rf)  { l.type = BNF.FLT; l.fVal = l.iVal / r.fVal; }
        if (lf && !rf)  { l.type = BNF.FLT; l.fVal *= r.iVal; }
        if (!lf && !rf) { l.type = BNF.INT; l.iVal *= r.iVal; }
      }
      break;
      case BNF.OPmod: {
        if (lf && rf)   { l.type = BNF.FLT; l.fVal %= r.fVal; }
        if (!lf && rf)  { l.type = BNF.FLT; l.fVal = l.iVal % r.fVal; }
        if (lf && !rf)  { l.type = BNF.FLT; l.fVal %= r.iVal; }
        if (!lf && !rf) { l.type = BNF.INT; l.iVal %= r.iVal; }
      }
      break;
      default: return null;
    }
    return l;
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

  private void ParseDataBlock(string[] lines, int start, int end, CodeNode data) {
    CodeNode lastDataLabel = null;
    Dictionary<string, bool> labels = new Dictionary<string, bool>();

    // Find at what line this starts
    for (int linenum = start + 1; linenum < end; linenum++) {
      string clean = lines[linenum].Trim();
      // Remove the comments and some unwanted chars
      clean = rgSLComment.Replace(clean, m => {
        if (m.Groups.Count > 1) return m.Groups[1].Value;
        return m.Value;
      });
      clean = rgMLComment.Replace(clean, "");
      clean = rgMLBacktick.Replace(clean, "'");

      // Until the block is empty, get the possible parts
      while (clean.Length > 0) {
        int poss = clean.IndexOf(' ');
        int posn = clean.IndexOf('\n');

        int pos = poss < posn ? poss : posn;
        if (pos == -1) pos = clean.Length;
        if (pos == 0) return;
        string line = clean.Substring(0, pos).Trim(' ', '\n', ',').ToLowerInvariant();

        // config(w,h,t)
        if (line.IndexOf("screen") != -1) { // ScreenCfg ***************************************************************** ScreenCfg
          pos = clean.IndexOf(")");
          line = clean.Substring(0, pos + 1).Trim(' ', '\n').ToLowerInvariant();

          Match m = rgConfScreen.Match(line);
          int.TryParse(m.Groups[1].Value.Trim(), out int w);
          int.TryParse(m.Groups[2].Value.Trim(), out int h);

          bool filter = (!string.IsNullOrEmpty(m.Groups[3].Value) && m.Groups[3].Value.IndexOf('f') != -1);

          Debug.Log(w + "," + h + (filter ? " filter" : ""));
          CodeNode n = new CodeNode(BNF.ScrConfig, null, linenum) { fVal = w, iVal = h, sVal = (filter ? "*" : "") };
          data.Add(n);

          clean = clean.Substring(pos + 1).Trim(' ', '\n');
        }
        else if (line.IndexOf("ram") != -1) { // RAM ****************************************************************** RAM
          pos = clean.IndexOf(")");
          line = clean.Substring(0, pos + 1).Trim(' ', '\n').ToLowerInvariant();
          Match m = rgRam.Match(line);
          int.TryParse(m.Groups[1].Value.Trim(), out int size);
          char unit = (m.Groups[2].Value.Trim().ToLowerInvariant() + " ")[0];
          if (unit == 'k') size *= 1024;
          if (unit == 'm') size *= 1024 * 1024;
          CodeNode n = new CodeNode(BNF.Ram, null, linenum) { iVal = size };
          data.Add(n);
          clean = clean.Substring(pos + 1).Trim(' ', '\n');
        }
        else if (line.IndexOf(':') != -1) { // Label ****************************************************************** Label
          pos = clean.IndexOf(':');
          line = clean.Substring(0, pos + 1).Trim(' ', '\n').ToLowerInvariant();
          if (labels.ContainsKey(line)) throw new Exception("Label \"" + line + "\" already defined");
          labels.Add(line, true);
          lastDataLabel = new CodeNode(BNF.Label, line, linenum) { bVal = new byte[1024], iVal = 0, sVal = line };
          data.Add(lastDataLabel);
          clean = clean.Substring(pos + 1).Trim(' ', '\n');
        }
        else if (rgBin.IsMatch(line)) { // Bin ************************************************************************ Bin
          if (lastDataLabel == null) throw new Exception("Found data without a label defined: " + line);
          Match m = rgBin.Match(line);
          string bin = m.Value.Trim(' ', '\n');

          int b = Convert.ToInt32(bin.Substring(1), 2);
          if (bin.Length < 10) {
            if (lastDataLabel.iVal == lastDataLabel.bVal.Length) {
              byte[] bytes = new byte[1024 + lastDataLabel.bVal.Length];
              for (int i = 0; i < lastDataLabel.bVal.Length; i++)
                bytes[i] = lastDataLabel.bVal[i];
              lastDataLabel.bVal = bytes;
            }
            lastDataLabel.bVal[lastDataLabel.iVal++] = (byte)(b & 0xff);
          }
          else {
            if (lastDataLabel.iVal >= lastDataLabel.bVal.Length - 3) {
              byte[] bytes = new byte[1024 + lastDataLabel.bVal.Length];
              for (int i = 0; i < lastDataLabel.bVal.Length; i++)
                bytes[i] = lastDataLabel.bVal[i];
              lastDataLabel.bVal = bytes;
            }
            byte[] vals = BitConverter.GetBytes(b);
            for (int i = 0; i < vals.Length; i++) {
              lastDataLabel.bVal[lastDataLabel.iVal++] = vals[i];
            }
          }

          clean = clean.Substring(bin.Length).Trim(' ', '\n', ',');
        }
        else if (rgHex.IsMatch(line)) { // Hex ************************************************************************ Hex
          if (lastDataLabel == null) throw new Exception("Found data without a label defined: " + line);
          Match m = rgHex.Match(line);
          string hex = m.Value.Trim(' ', '\n');

          int hx = Convert.ToInt32(hex.Substring(2), 16);
          if (hex.Length < 5) {
            if (lastDataLabel.iVal == lastDataLabel.bVal.Length) {
              byte[] bytes = new byte[1024 + lastDataLabel.bVal.Length];
              for (int i = 0; i < lastDataLabel.bVal.Length; i++)
                bytes[i] = lastDataLabel.bVal[i];
              lastDataLabel.bVal = bytes;
            }
            lastDataLabel.bVal[lastDataLabel.iVal++] = (byte)(hx & 0xff);
          }
          else {
            if (lastDataLabel.iVal >= lastDataLabel.bVal.Length - 3) {
              byte[] bytes = new byte[1024 + lastDataLabel.bVal.Length];
              for (int i = 0; i < lastDataLabel.bVal.Length; i++)
                bytes[i] = lastDataLabel.bVal[i];
              lastDataLabel.bVal = bytes;
            }
            byte[] vals = BitConverter.GetBytes(hx);
            for (int i = 0; i < vals.Length; i++) {
              lastDataLabel.bVal[lastDataLabel.iVal++] = vals[i];
            }
          }

          clean = clean.Substring(hex.Length).Trim(' ', '\n', ',');
        }
        else if (rgInt.IsMatch(line)) { // Int ************************************************************************ Int
          if (lastDataLabel == null) throw new Exception("Found data without a label defined: " + line);
          Match m = rgInt.Match(line);
          string num = m.Value.Trim(' ', '\n');

          int.TryParse(num, out int val);
          if (val < 256 && num.Length < 4) {
            if (lastDataLabel.iVal == lastDataLabel.bVal.Length) {
              byte[] bytes = new byte[1024 + lastDataLabel.bVal.Length];
              for (int i = 0; i < lastDataLabel.bVal.Length; i++)
                bytes[i] = lastDataLabel.bVal[i];
              lastDataLabel.bVal = bytes;
            }
            lastDataLabel.bVal[lastDataLabel.iVal++] = (byte)(val & 0xff);
          }
          else {
            if (lastDataLabel.iVal >= lastDataLabel.bVal.Length - 3) {
              byte[] bytes = new byte[1024 + lastDataLabel.bVal.Length];
              for (int i = 0; i < lastDataLabel.bVal.Length; i++)
                bytes[i] = lastDataLabel.bVal[i];
              lastDataLabel.bVal = bytes;
            }
            byte[] vals = BitConverter.GetBytes(val);
            for (int i = 0; i < vals.Length; i++) {
              lastDataLabel.bVal[lastDataLabel.iVal++] = vals[i];
            }
          }

          clean = clean.Substring(num.Length).Trim(' ', '\n', ',');
        }
        else
          throw new Exception("Invalid DATA from line: " + 0 + "\n" + clean);
        // label:
        // num (1 or 4 bytes)
        // hex (1, 2, 4 bytes)
        // binary (1, 2, 4 bytes)
      }
    }
  }

}
