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
    "atan2",
    "box",
    "circle",
    "clr",
    "cos",
    "data",
    "deltatime",
    "destroy",
    "else",
    "for",
    "frame",
    "getp",
    "if",
    "keya",
    "keyad",
    "keyau",
    "keyb",
    "keybd",
    "keybu",
    "keyc",
    "keycd",
    "keycu",
    "keyd",
    "keydd",
    "keydu",
    "keye",
    "keyed",
    "keyesc",
    "keyescd",
    "keyescu",
    "keyeu",
    "keyf",
    "keyfd",
    "keyfire",
    "keyfired",
    "keyfireu",
    "keyfu",
    "keyh",
    "keyhd",
    "keyhu",
    "keyl",
    "keyld",
    "keylu",
    "keyr",
    "keyrd",
    "keyru",
    "keyu",
    "keyud",
    "keyuu",
    "keyv",
    "keyvd",
    "keyvu",
    "keyx",
    "keyxd",
    "keyxu",
    "keyy",
    "keyyd",
    "keyyu",
    "line",
    "loadmusic",
    "musicpos",
    "musicvoices",
    "mute",
    "name",
    "pan",
    "pitch",
    "playmusic",
    "pow",
    "ram",
    "return",
    "screen",
    "screen",
    "setp",
    "sin",
    "sound",
    "spen",
    "spos",
    "sprite",
    "sqrt",
    "srot",
    "sscale",
    "start",
    "stint",
    "stopmusic",
    "tan",
    "tilemap",
    "tilepos",
    "update",
    "volume",
    "wait",
    "wave",
    "while",
    "write",
  };

  #region Regex

  readonly Regex rgMLBacktick = new Regex("`", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgCommentML = new Regex("/\\*[^(\\*/)]*\\*/", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCommentSL = new Regex("//(.*?)\r?\n", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgOpenBracket = new Regex("[\\s]*\\{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBlockOpen = new Regex(".*\\{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));
  readonly Regex rgBlockClose = new Regex("^[\\s]*\\}[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));
  readonly Regex rgTag = new Regex("([\\s]*`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgVar = new Regex("(?<=[^a-z0-9`@_]|^)([a-z][0-9a-z]{0,7})([^a-z0-9\\(¶]|$)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgArray = new Regex("(?<=[^a-z0-9`@_]|^)([a-z][0-9a-z]{0,7})\\[((?>\\[(?<c>)|[^\\[\\]]+|\\](?<-c>))*(?(c)(?!)))\\]", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgHex = new Regex("0x([0-9a-f]{8}|[0-9a-f]{4}|[0-9a-f]{2})", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgCol = new Regex("c([0-5])([0-5])([0-5])([0-4])?", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgQString = new Regex("\\\\\"", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgString = new Regex("(\")([^\"]*)(\")", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDeltat = new Regex("deltatime", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFloat = new Regex("[0-9]*\\.[0-9]+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgInt = new Regex("([^0-9]?\\s*)(\\d+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBin = new Regex("0b([0-1]{1,31})", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));

  readonly Regex rgPars = new Regex("\\((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!))\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMem = new Regex("\\[[\\s]*`[a-z]{3,}¶[\\s]*]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemL = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemB = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@b[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemI = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@i[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemF = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@f[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemS = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@s[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMemC = new Regex("\\[[\\s]*(`[a-z]{3,}¶)[\\s]*@c[\\s]*\\]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
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
  readonly Regex rgOPlsh = new Regex("\\<\\<", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgOPrsh = new Regex("\\>\\>", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCastB = new Regex("(`[a-z]{3,}¶)_b", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCastI = new Regex("(`[a-z]{3,}¶)_i", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCastF = new Regex("(`[a-z]{3,}¶)_f", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCastS = new Regex("(`[a-z]{3,}¶)_s", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgLen = new Regex("([\\s]*`[a-z]{3,}¶)\\.len[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgPLen = new Regex("([\\s]*`[a-z]{3,}¶)\\.plen[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgTrim = new Regex("([\\s]*`[a-z]{3,}¶)\\.trim[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSubstring = new Regex("([\\s]*`[a-z]{3,}¶)\\.substring\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgAssign = new Regex("[^=][\\s]*=[\\s]*[^=]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssSum = new Regex("[\\s]*\\+=[^(\\+=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssSub = new Regex("[\\s]*\\-=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssMul = new Regex("[\\s]*\\*=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssDiv = new Regex("[\\s]*/=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssMod = new Regex("[\\s]*%=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssAnd = new Regex("[\\s]*&=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssOr = new Regex("[\\s]*\\|=[^(\\-=)]=", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAssXor = new Regex("[\\s]*\\^=[^(\\-=)]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgClr = new Regex("[\\s]*clr\\((.+)\\)[\\s]*", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));
  readonly Regex rgFrame = new Regex("frame", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  
  
  readonly Regex rgWrite = new Regex("[\\s]*write[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgLine = new Regex("[\\s]*line[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgGetP = new Regex("[\\s]*getp[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSetP = new Regex("[\\s]*setp[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgBox = new Regex("[\\s]*box[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCircle = new Regex("[\\s]*circle[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgInc = new Regex("(.*)\\+\\+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDec = new Regex("(.*)\\-\\-", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgIf = new Regex("[\\s]*if[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*(.*)$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgElse = new Regex("[\\s]*else[\\s]*(.*)$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgWhile = new Regex("[\\s]*while[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*(.*)$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFor = new Regex("[\\s]*for[\\s]*\\(([^,]*=[^,]*)?,([^,]*)?,([^,]*)?\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgScreen = new Regex("[\\s]*screen[\\s]*\\(([^,]*),([^,]*)(,([^,]*)){0,1}(,([^,]*)){0,1}\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgWait = new Regex("[\\s]*wait[\\s]*\\(([^,]+)(,[\\s]*([fn]))?\\)[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgDestroy = new Regex("[\\s]*destroy[\\s]*\\(([^,]+)\\)[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgImage = new Regex("[\\s]*image[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgSin = new Regex("[\\s]*sin[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCos = new Regex("[\\s]*cos[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgTan = new Regex("[\\s]*tan[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgAtan2 = new Regex("[\\s]*atan2[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSqrt = new Regex("[\\s]*sqrt[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgPow = new Regex("[\\s]*pow[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgSprite = new Regex("[\\s]*sprite[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSpos = new Regex("[\\s]*spos[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSrot = new Regex("[\\s]*srot[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSPen = new Regex("[\\s]*spen[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSTint = new Regex("[\\s]*stint[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgSScale = new Regex("[\\s]*sscale[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  
  readonly Regex rgTilemap = new Regex("[\\s]*tilemap[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgTilepos = new Regex("[\\s]*tilepos[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgTileset = new Regex("[\\s]*tileset[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgTileget = new Regex("[\\s]*tileget[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgTilerot = new Regex("[\\s]*tilegetrot[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgSound = new Regex("[\\s]*sound[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgWave = new Regex("[\\s]*wave[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMute = new Regex("[\\s]*mute[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgVolume = new Regex("[\\s]*volume[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgPitch = new Regex("[\\s]*pitch[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgPan = new Regex("[\\s]*pan[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMusicLoad = new Regex("[\\s]*loadmusic[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMusicPlay = new Regex("[\\s]*playmusic[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMusicStop = new Regex("[\\s]*stopmusic[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMusicPos = new Regex("[\\s]*musicpos[\\s]*[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgMusicVoices = new Regex("[\\s]*musicvoices[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgCMPlt = new Regex("(`[a-z]{3,}¶)([\\s]*\\<[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPle = new Regex("(`[a-z]{3,}¶)([\\s]*\\<\\=[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPgt = new Regex("(`[a-z]{3,}¶)([\\s]*\\>[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPge = new Regex("(`[a-z]{3,}¶)([\\s]*\\>\\=[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPeq = new Regex("(`[a-z]{3,}¶)([\\s]*\\=\\=[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgCMPne = new Regex("(`[a-z]{3,}¶)([\\s]*\\!\\=[\\s]*)(`[a-z]{3,}¶)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgKey = new Regex("[\\s]*key(fire|esc|[udlrabcfexyhv])([ud]?)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  //   keys -> U, D, L, R, A, B, C, D, X, Y, H, V, Fire, Esc
  readonly Regex rgLabel = new Regex("[\\s]*[a-z][a-z0-9_]+:[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgLabelGet = new Regex("[\\s]*label[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)[\\s]*", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgConfScreen = new Regex("screen[\\s]*\\([\\s]*([0-9]+)[\\s]*,[\\s]*([0-9]+)[\\s]*(,[\\s]*[fn])?[\\s]*\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgRam = new Regex("ram[\\s]*\\([\\s]*([0-9]+)[\\s]*([bkm])?[\\s]*\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

  readonly Regex rgName = new Regex("^name:[\\s]*([a-z0-9_\\s]+)$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgStart = new Regex("^start[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgUpdate = new Regex("^update[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgConfig = new Regex("^config[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgData = new Regex("^data[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFunction = new Regex("^#([a-z][a-z0-9]{0,11})[\\s]*\\((.*)\\)[\\s]*{[\\s]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgFunctionCall = new Regex("([a-z][a-z0-9]{0,11})[\\s]*\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
  readonly Regex rgReturn = new Regex("[\\s]*return[\\s]*(.*)[\\s]*", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));

  #endregion Regex


  int linenumber = 0; // Current parsed line number
  string origForException = ""; // Used to keep the original text of the line to show erorrs
  string currentFunction = null; // Used to keep track of the current parsed functions to have the local variables
  CodeNode currentFunctionParameters = null; // Used to keep track of the current parsed functions to have the local variables


  public CodeNode Parse(string file, Variables variables, bool parseDataSection) {
    try {
      // Start by replacing all the problematic stuff
      file = file.Trim().Replace("\r", "").Replace("\t", " ");
      // [QuotedStrings]
      file = rgQString.Replace(file, "ˠ");
      // Replace single line comments
      file = rgCommentSL.Replace(file, "");
      // Remove multiline-comments, but keep the newlines
      file = rgCommentML.Replace(file, m => {
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
            string var = par.Trim(' ', ',').ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(var)) {
              if (reserverdKeywords.Contains(var)) throw new Exception("Parameter name \"" + var + "\" is invalid (reserverd keyword)\n" + (linenumber + 1) + ": " + line);
              // Function parameters have to be local to the function, we will add the function name before
              var = n.sVal + "." + var;
              CodeNode v = new CodeNode(BNF.REG, par, linenumber) { sVal = var, Reg = vars.Add(var) };
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

        m = rgConfig.Match(line);
        if (m.Success) {
          // find the end of the block, and parse the result
          int end = FindEndOfBlock(lines, linenumber);
          if (end == -1) throw new Exception("\"CONFIG\" section does not end");

          CodeNode config = new CodeNode(BNF.Config, line, linenumber);
          res.Add(config);
          ParseConfigBlock(lines, linenumber, end, config);
          continue;
        }

        if (parseDataSection) {
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

          // We need to handle the variables as local variables if they are parameters
          currentFunction = fname;
          currentFunctionParameters = f.CN1;
          ParseBlock(lines, linenumber + 1, end, b);
          continue;
        }
      }
      return res;
    } catch (Exception e) {
      Debug.Log(e.Message + "\nCurrent line = " + (linenumber + 1) + "\n" + e.StackTrace);
      throw e;
    }
  }

  int FindEndOfBlock(string[] lines, int start) {
    int num = 0;
    for (int i = start; i < lines.Length; i++) {
      string line = lines[i];
      int pos1 = line.IndexOf('}');
      int pos2 = line.IndexOf('{');
      if (pos1 == -1 && pos2 == -1) continue;
      int pos3 = line.IndexOf('"');
      if (pos3 == -1) {
        if (pos1 != -1) {
          num--;
          if (num == 0) return i;
        }
        if (pos2 != -1) num++;
      }
      else {
        line = rgString.Replace(line, "");
        pos1 = line.IndexOf('}');
        pos2 = line.IndexOf('{');
        if (pos1 != -1) {
          num--;
          if (num == 0) return i;
        }
        if (pos2 != -1) num++;
      }
    }
    return lines.Length - 1;
  }

  private void ParseBlock(string[] lines, int start, int end, CodeNode parent) {
    // Follow the BNF rules to get the elements, one line at time
    for (linenumber = start; linenumber < end; linenumber++) {
      string line = lines[linenumber].Trim();
      if (string.IsNullOrEmpty(line)) continue;
      lines[linenumber] = line;
      expected.Set(Expected.Val.Statement);
      ParseLine(parent, lines);
    }
  }

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
        sVal = str.Replace("ˠ", "\"").Replace("\\n", "\n")
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
        ParseLine(b, after, lines);
        ParseElseBlock(node, lines, true);
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
        ParseLine(node, m.Groups[1].Value.Trim(), lines);
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
        ParseLine(b, m.Groups[3].Value.Trim(), lines);
      }
      return;
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
      string fullorigline = origForException;
      string dests = line.Substring(0, line.IndexOf('='));
      string val = line.Substring(line.IndexOf('=') + 1);
      CodeNode node = new CodeNode(BNF.ASSIGN, line, linenumber);
      expected.Set(Expected.Val.MemReg);
      ParseLine(node, dests, null);
      parent.Add(node);
      origForException = fullorigline;
      node.Add(ParseExpression(val));
      expected.Set(Expected.Val.Statement);
      return;
    }

    // [RETURN] = return [EXPR]
    if (expected.IsGood(Expected.Val.Statement) && rgReturn.IsMatch(line)) {
      Match m = rgReturn.Match(line);
      if (m.Groups.Count < 2) throw new Exception("Invalid Return() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.RETURN, line, linenumber);
      string ret = m.Groups[1].Value.Trim();
      if (!string.IsNullOrEmpty(ret)) node.Add(ParseExpression(ret));

      bool outsideFunctionDef = true;
      CodeNode pn = parent;
      while (pn != null) {
        if (pn.type == BNF.FunctionDef) {
          outsideFunctionDef = false;
          break;
        }
        pn = pn.parent;
      }

      if (outsideFunctionDef) throw new Exception("RETURN can be used only inside functions\n" + (linenumber + 1) + ": " + origForException);
      parent.Add(node);
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

    // [WRITE] = write([EXPR], [EXPR], [EXPR], [EXPR], [EXPR]) ; text, x, y, col(front), [col(back), size]
    if (expected.IsGood(Expected.Val.Statement) && rgWrite.IsMatch(line)) {
      Match m = rgWrite.Match(line);
      if (m.Groups.Count < 2) throw new Exception("Invalid Write() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.WRITE, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num < 4) throw new Exception("Invalid Write(), not enough parameters. Line: " + (linenumber + 1));
      if (num > 6) throw new Exception("Invalid Write(), too many parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [SetP] = SetP([EXPR], [EXPR])
    if (expected.IsGood(Expected.Val.Statement) && rgSetP.IsMatch(line)) {
      Match m = rgSetP.Match(line);
      if (m.Groups.Count < 2) throw new Exception("Invalid SetP() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.SETP, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num < 3) throw new Exception("Invalid SetP(), not enough parameters. Line: " + (linenumber + 1));
      if (num > 3) throw new Exception("Invalid SetP(), too many parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [LINE] = line([EXPR], [EXPR], [EXPR], [EXPR], [EXPR])
    if (expected.IsGood(Expected.Val.Statement) && rgLine.IsMatch(line)) {
      Match m = rgLine.Match(line);
      if (m.Groups.Count < 2) throw new Exception("Invalid Line() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.LINE, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num < 5) throw new Exception("Invalid Line(), not enough parameters. Line: " + (linenumber + 1));
      if (num > 5) throw new Exception("Invalid Line(), too many parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [BOX] = box([EXP], [EXP], [EXP], [EXP], [EXP], [[EXP]])
    if (expected.IsGood(Expected.Val.Statement) && rgBox.IsMatch(line)) {
      Match m = rgBox.Match(line);
      if (m.Groups.Count < 2) throw new Exception("Invalid Box() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.BOX, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num < 5) throw new Exception("Invalid Box(), not enough parameters. Line: " + (linenumber + 1));
      if (num > 6) throw new Exception("Invalid Box(), too many parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [CIRCLE] = circle([EXP], [EXP], [EXP], [EXP], [EXP], [[EXP]])
    if (expected.IsGood(Expected.Val.Statement) && rgCircle.IsMatch(line)) {
      Match m = rgCircle.Match(line);
      if (m.Groups.Count < 8) throw new Exception("Invalid Circle() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.CIRCLE, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num < 5) throw new Exception("Invalid Circle(), not enough parameters. Line: " + (linenumber + 1));
      if (num > 6) throw new Exception("Invalid Circle(), too many parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [Image] = circle([EXP], [EXP], [EXP], [EXP], [EXP], [[EXP],[EXP]])
    // int pointer, int px, int py, int w, int h, int linestart = 0, int linesize = 0
    if (expected.IsGood(Expected.Val.Statement) && rgImage.IsMatch(line)) {
      Match m = rgImage.Match(line);
      if (m.Groups.Count < 8) throw new Exception("Invalid Image() command. Line: " + (linenumber + 1));
      CodeNode node = new CodeNode(BNF.IMAGE, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 5 && num != 7) throw new Exception("Invalid Image(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
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
      CodeNode node = new CodeNode(BNF.SPRITE, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 3 && num != 5)
        throw new Exception("Invalid Sprite(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [SPOS] num, x, y[, enble]
    if (expected.IsGood(Expected.Val.Statement) && rgSpos.IsMatch(line)) {
      Match m = rgSpos.Match(line);
      CodeNode node = new CodeNode(BNF.SPOS, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 3 && num != 4)
        throw new Exception("Invalid SPos(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [SROT] num, dir, flip
    if (expected.IsGood(Expected.Val.Statement) && rgSrot.IsMatch(line)) {
      Match m = rgSrot.Match(line);
      CodeNode node = new CodeNode(BNF.SROT, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 3)
        throw new Exception("Invalid SRot(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [STINT] num, enable
    if (expected.IsGood(Expected.Val.Statement) && rgSTint.IsMatch(line)) {
      Match m = rgSTint.Match(line);
      CodeNode node = new CodeNode(BNF.STINT, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 2)
        throw new Exception("Invalid STint(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [SScale] num, enable
    if (expected.IsGood(Expected.Val.Statement) && rgSScale.IsMatch(line)) {
      Match m = rgSScale.Match(line);
      CodeNode node = new CodeNode(BNF.SSCALE, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 3)
        throw new Exception("Invalid SScale(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [SPEN] num, enable
    if (expected.IsGood(Expected.Val.Statement) && rgSPen.IsMatch(line)) {
      Match m = rgSPen.Match(line);
      CodeNode node = new CodeNode(BNF.SPEN, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 2)
        throw new Exception("Invalid SPen(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [Tilemap] id, addressmap, w, h, address tiles, tw, th [, sourcewidth]
    if (expected.IsGood(Expected.Val.Statement) && rgTilemap.IsMatch(line)) {
      Match m = rgTilemap.Match(line);
      CodeNode node = new CodeNode(BNF.TILEMAP, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 3)
        throw new Exception("Invalid Tilemap(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [TilePos] id, scrollx, scrolly[, order [, enabled]]
    if (expected.IsGood(Expected.Val.Statement) && rgTilepos.IsMatch(line)) {
      Match m = rgTilepos.Match(line);
      CodeNode node = new CodeNode(BNF.TILEPOS, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num < 3 && num > 5)
        throw new Exception("Invalid TilePos(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [TileSet] id, x, y, tile, rot
    if (expected.IsGood(Expected.Val.Statement) && rgTileset.IsMatch(line)) {
      Match m = rgTileset.Match(line);
      CodeNode node = new CodeNode(BNF.TILESET, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 4 && num != 5)
        throw new Exception("Invalid TileSet(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [Sound] channel, frequency[, length]
    if (expected.IsGood(Expected.Val.Statement) && rgSound.IsMatch(line)) {
      Match m = rgSound.Match(line);
      CodeNode node = new CodeNode(BNF.SOUND, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num < 2) throw new Exception("Invalid Sound(), channel and frequency are required. Line: " + (linenumber + 1));
      if (num > 3) throw new Exception("Invalid Sound(), possible parameters are channel, frequency, and length. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [Wave] channel, wave, phase, a, d, s, r
    // [Wave] channel, address
    if (expected.IsGood(Expected.Val.Statement) && rgWave.IsMatch(line)) {
      Match m = rgWave.Match(line);
      CodeNode node = new CodeNode(BNF.WAVE, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 2 && num != 7)
        throw new Exception("Invalid Wave(), wrong number of parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [Mute] channel
    if (expected.IsGood(Expected.Val.Statement) && rgMute.IsMatch(line)) {
      Match m = rgMute.Match(line);
      CodeNode node = new CodeNode(BNF.MUTE, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 1)
        throw new Exception("Invalid Mute(), channel is required as parameter. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [Volume] channel, volume | volume
    if (expected.IsGood(Expected.Val.Statement) && rgVolume.IsMatch(line)) {
      Match m = rgVolume.Match(line);
      CodeNode node = new CodeNode(BNF.VOLUME, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num == 0) throw new Exception("Invalid Volume(), specify the global volume or the channel and volume. Line: " + (linenumber + 1));
      if (num > 2) throw new Exception("Invalid Volume(), specify the global volume or the channel and volume only. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [Pitch] channel, pitch
    if (expected.IsGood(Expected.Val.Statement) && rgPitch.IsMatch(line)) {
      Match m = rgPitch.Match(line);
      CodeNode node = new CodeNode(BNF.PITCH, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 2) throw new Exception("Invalid Pitch(), specify the channel and pitch parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [Pan] channel, pan
    if (expected.IsGood(Expected.Val.Statement) && rgPan.IsMatch(line)) {
      Match m = rgPan.Match(line);
      CodeNode node = new CodeNode(BNF.PAN, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 2) throw new Exception("Invalid Pan(), specify the channel and pan parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [MusicLoad] address
    if (expected.IsGood(Expected.Val.Statement) && rgMusicLoad.IsMatch(line)) {
      Match m = rgMusicLoad.Match(line);
      CodeNode node = new CodeNode(BNF.MUSICLOAD, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 1) throw new Exception("Invalid MusicLoad(), specify the address of the music. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [MusicPlay] [step]
    if (expected.IsGood(Expected.Val.Statement) && rgMusicPlay.IsMatch(line)) {
      Match m = rgMusicPlay.Match(line);
      CodeNode node = new CodeNode(BNF.MUSICPLAY, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num > 1) throw new Exception("Invalid MusicPlay() parameters, max one allowed. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [MusicStop]
    if (expected.IsGood(Expected.Val.Statement) && rgMusicStop.IsMatch(line)) {
      Match m = rgMusicStop.Match(line);
      CodeNode node = new CodeNode(BNF.MUSICSTOP, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num != 0) throw new Exception("Invalid MusicStop(), it does not support parameters. Line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [MusicVoices] num[, num]{1-7}
    if (expected.IsGood(Expected.Val.Statement) && rgMusicVoices.IsMatch(line)) {
      Match m = rgMusicVoices.Match(line);
      CodeNode node = new CodeNode(BNF.MUSICVOICES, line, linenumber);
      string pars = m.Groups[1].Value.Trim();
      int num = ParsePars(node, pars);
      if (num == 0) throw new Exception("Invalid MusicVoices(), specify the channel numbers to be used. Line: " + (linenumber + 1));
      if (num > 8) throw new Exception("Invalid MusicVoices(), too many channel numbers specified. Line: " + (linenumber + 1));
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
        ParseLine(b, after, lines);
        return;
      }

      throw new Exception("Invalid block after WHILE statement: " + (linenumber + 1));
    }

    // [ARRAY]
    if (expected.IsGood(Expected.Val.MemReg) && rgArray.IsMatch(line)) {
      Match m = rgArray.Match(line);
      string var = m.Groups[1].Value.ToLowerInvariant();
      if (!reserverdKeywords.Contains(var)) {
        // Are we parsing a function?
        if (currentFunction != null && currentFunctionParameters.children != null) {
          // Is it a parameter variable?
          string lp = currentFunction + "." + var;
          foreach (CodeNode p in currentFunctionParameters.children) {
            if (p.sVal == lp) { // Yes, it is local
              var = lp;
              break;
            }
          }
        }
        CodeNode node = new CodeNode(BNF.ARRAY, line, linenumber) { Reg = vars.Add(var+"[]") };
        string par = m.Groups[2].Value.Trim();
        if (!string.IsNullOrEmpty(par)) node.Add(ParseExpression(par));
        parent.Add(node);
        return;
      }
    }

    // [MEM]= \[<exp>\] | \[<exp>@<exp>\]
    if (expected.IsGood(Expected.Val.MemReg) && rgMemUnparsed.IsMatch(line)) {
      CodeNode node = ParseExpression(rgMemUnparsed.Match(line).Value);
      if (node.type != BNF.MEM && node.type != BNF.MEMlong && node.type != BNF.MEMlongb && node.type != BNF.MEMlongi && node.type != BNF.MEMlongf && node.type != BNF.MEMlongs && node.type != BNF.MEMchar)
        throw new Exception("Expected Memory,\nfound " + node.type + "  at line: " + (linenumber + 1));
      parent.Add(node);
      return;
    }

    // [REG]=a-z[a-z0-9]*
    if (expected.IsGood(Expected.Val.MemReg) && rgVar.IsMatch(line)) {
      string var = rgVar.Match(line).Groups[1].Value.ToLowerInvariant();
      if (!reserverdKeywords.Contains(var)) {
        // Are we parsing a function?
        if (currentFunction != null && currentFunctionParameters.children != null) {
          // Is it a parameter variable?
          string lp = currentFunction + "." + var;
          foreach(CodeNode p in currentFunctionParameters.children) {
            if (p.sVal == lp) { // Yes, it is local
              var = lp;
              break;
            }
          }
        }
        CodeNode node = new CodeNode(BNF.REG, line, linenumber) { Reg = vars.Add(var) };
        parent.Add(node);
        return;
      }
    }

    // {
    if (rgOpenBracket.IsMatch(line)) return;

    // [FUNCTION]()
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

        if (!functions.ContainsKey(fnc)) throw new Exception("The function \"" + fnc + "\"\nis not defined\n" + (linenumber + 1) + ": " + origForException);

        if ((functions[fnc].CN1 == null && ps.children != null) || (functions[fnc].CN1.children?.Count != ps.children?.Count))
          throw new Exception("Function " + fnc + " has\na wrong number of parameters\n" + (linenumber + 1) + ": " + origForException);
        return;
      }
    }



    throw new Exception("Invalid code at " + (linenumber + 1) + "\n" + origForException);
  }


  void ParseIfBlock(CodeNode ifNode, string after, string[] lines) {
    CodeNode b = new CodeNode(BNF.BLOCK, after, linenumber);
    ifNode.Add(b);
    if (rgBlockOpen.IsMatch(after)) {  // [IF] {
      int end = FindEndOfBlock(lines, linenumber);
      if (end < 0) throw new Exception("\"IF\" section does not end");
      ParseBlock(lines, linenumber + 1, end, b);
      linenumber = end;
    }
    else if (string.IsNullOrEmpty(after)) { // [IF] \n* ({ | [^{ ])
      for (int i = linenumber + 1; i < lines.Length; i++) {
        string l = lines[i].Trim();
        if (string.IsNullOrEmpty(l)) continue;
        if (rgOpenBracket.IsMatch(l)) { // [IF] \n* {
          int end = FindEndOfBlock(lines, i);
          if (end < 0) throw new Exception("\"IF\" section does not end");
          ParseBlock(lines, linenumber + 1, end, b);
          linenumber = end;
          break;
        }
        else { // [IF] \n* [^{ ]
          linenumber = i;
          ParseLine(b, lines);
          break;
        }
      }
    }
    ParseElseBlock(ifNode, lines, false);
  }


  void ParseElseBlock(CodeNode ifNode, string[] lines, bool nextLine) {
    // Is the next non-empty line an "else"?
    for (int pos = linenumber + (nextLine ? 1 : 0); pos < lines.Length; pos++) {
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
          ifNode.Add(nElse);
          ParseBlock(lines, linenumber + 1, end, nElse);
          linenumber = end;
          return;
        }
        if (string.IsNullOrEmpty(after)) { // [ELSE] \n* ({ | [^{ ])
          for (int i = pos + 1; i < lines.Length; i++) {
            l = lines[i].Trim();
            if (string.IsNullOrEmpty(l)) continue;
            if (rgOpenBracket.IsMatch(l)) { // [ELSE] \n* {
              int end = FindEndOfBlock(lines, i);
              if (end < 0) throw new Exception("\"ELSE\" section does not end");
              ifNode.Add(nElse);
              ParseBlock(lines, linenumber + 1, end, nElse);
              linenumber = end;
              return;
            }
            else { // [ELSE] \n* [^{ ]
              linenumber = i;
              ifNode.Add(nElse);
              ParseLine(nElse, lines);
              return;
            }
          }
        }
        else { // [ELSE] \n* [^{ ]
          ifNode.Add(nElse);
          ParseLine(nElse, after, null);
          linenumber++;
          return;
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
      ParseLine(b, after, lines);
      linenumber++;
    }
  }

  // [EXP] [OP] [EXP] | [PAR] | [REG] | [INT] | [FLT] | [MEM] | [UO] | [LEN] | deltaTime
  CodeNode ParseExpression(string line) {
    line = line.Trim(' ', '\t', '\r', ';');

    bool atLeastOneReplacement = true;
    while (atLeastOneReplacement) {
      atLeastOneReplacement = false;

      // - (unary)
      line = rgUOsub.Replace(line, m => {
        atLeastOneReplacement = true;
        string toReplace = m.Captures[0].Value.Trim();
        toReplace.Trim();
        if (toReplace[0] != '-') throw new Exception("Invalid negative value: " + toReplace);
        toReplace = toReplace.Substring(1).Trim();
        CodeNode n = new CodeNode(BNF.UOsub, GenId("US"), origForException, linenumber);
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
      if (atLeastOneReplacement) continue;

      // !
      line = rgUOneg.Replace(line, m => {
        atLeastOneReplacement = true;
        string toReplace = m.Captures[0].Value.Trim();
        toReplace.Trim();
        if (toReplace[0] != '!') throw new Exception("Invalid negation: " + toReplace);
        toReplace = toReplace.Substring(1).Trim();
        CodeNode n = new CodeNode(BNF.UOneg, GenId("US"), origForException, linenumber);
        CodeNode exp = ParseExpression(toReplace);
        n.Add(exp);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // ~
      line = rgUOinv.Replace(line, m => {
        atLeastOneReplacement = true;
        string toReplace = m.Captures[0].Value.Trim();
        toReplace.Trim();
        if (toReplace[0] != '~') throw new Exception("Invalid unary complement: " + toReplace);
        toReplace = toReplace.Substring(1).Trim();
        CodeNode n = new CodeNode(BNF.UOinv, GenId("US"), origForException, linenumber);
        CodeNode exp = ParseExpression(toReplace);
        n.Add(exp);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace DTIME => `DTx
      line = rgDeltat.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.DTIME, GenId("DT"), origForException, linenumber);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace DTIME => `DTx
      line = rgMusicPos.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.MUSICPOS, GenId("MP"), origForException, linenumber);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace FLT => `FTx
      line = rgFloat.Replace(line, m => {
        atLeastOneReplacement = true;
        float.TryParse(m.Value, System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.CultureInfo("en-US"), out float fVal);
        CodeNode n = new CodeNode(BNF.FLT, GenId("FT"), origForException, linenumber) {
          fVal = fVal
        };
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace HEX => `HXx
      line = rgHex.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.INT, GenId("HX"), origForException, linenumber) {
          iVal = Convert.ToInt32("0" + m.Value, 16)
        };
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace BIN => `BIx
      line = rgBin.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.INT, GenId("BI"), origForException, linenumber) {
          iVal = Convert.ToInt32("0" + m.Value, 2)
        };
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace COL => `CLx
      line = rgCol.Replace(line, m => {
        atLeastOneReplacement = true;
        int.TryParse(m.Groups[1].Value, out int r);
        int.TryParse(m.Groups[2].Value, out int g);
        int.TryParse(m.Groups[3].Value, out int b);
        int a = -1;
        if (m.Groups.Count > 4 && !string.IsNullOrEmpty(m.Groups[4].Value)) int.TryParse(m.Groups[4].Value, out a);
        if (r > 5) r = 5;
        if (g > 5) g = 5;
        if (b > 5) b = 5;
        if (a > 4) a = 4;
        CodeNode n = new CodeNode(BNF.COLOR, GenId("CL"), origForException, linenumber) {
          iVal = Col.GetByteFrom6(r, g, b, a)
        };
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // LAB
      line = rgLabel.Replace(line, m => {
        atLeastOneReplacement = true;
        string lab = m.Value.Trim().ToLowerInvariant();
        lab = lab.Substring(0, lab.Length - 1);
        CodeNode n = new CodeNode(BNF.Label, GenId("LB"), origForException, linenumber) {
          sVal = lab
        };
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace INT => `INx
      line = rgInt.Replace(line, m => {
        atLeastOneReplacement = true;
        string pre = m.Groups[1].Value;
        string val = m.Groups[2].Value;
        if (!string.IsNullOrEmpty(pre)) {
          // Check that we do not have letters, ], and )
          char c = pre[0];
          if (char.IsLetter(c)) return m.Value;
          if (c == ']') throw new Exception("Syntax error in expression: " + line + "\n" + origForException);
          if (c == ')') throw new Exception("Syntax error in expression: " + line + "\n" + origForException);
        }
        int.TryParse(val, out int iVal);
        CodeNode n = new CodeNode(BNF.INT, GenId("IN"), origForException, linenumber) {
          iVal = iVal
        };
        nodes[n.id] = n;
        if (!string.IsNullOrEmpty(pre)) return pre + n.id;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // [ARRAY] Replace REG[[EXPR]] => `ARx
      line = rgArray.Replace(line, m => {
        atLeastOneReplacement = true;
        string var = m.Groups[1].Value.ToLowerInvariant() + "[]";
        // Are we parsing a function?
        if (currentFunction != null && currentFunctionParameters.children != null) {
          // Is it a parameter variable?
          string lp = currentFunction + "." + var;
          foreach (CodeNode p in currentFunctionParameters.children) {
            if (p.sVal == lp) { // Yes, it is local
              var = lp;
              break;
            }
          }
        }
        CodeNode n = new CodeNode(BNF.REG, GenId("AR"), origForException, linenumber) {
          Reg = vars.Add(var)
        };
        n.Add(ParseExpression(m.Groups[2].Value));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace REG => `RGx
      line = rgVar.Replace(line, m => {
        string var = m.Groups[1].Value.ToLowerInvariant();
        if (!reserverdKeywords.Contains(var)) {
          atLeastOneReplacement = true;
          // Are we parsing a function?
          if (currentFunction != null && currentFunctionParameters.children != null) {
            // Is it a parameter variable?
            string lp = currentFunction + "." + var;
            foreach (CodeNode p in currentFunctionParameters.children) {
              if (p.sVal == lp) { // Yes, it is local
                var = lp;
                break;
              }
            }
          }
          CodeNode n = new CodeNode(BNF.REG, GenId("RG"), origForException, linenumber) {
            Reg = vars.Add(var)
          };
          nodes[n.id] = n;
          return n.id + m.Groups[2].Value;
        }
        return m.Value;
      });
      if (atLeastOneReplacement) continue;

      // [Sin] = Sin([EXPR])
      line = rgSin.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.SIN, GenId("SI"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 1) throw new Exception("Invalid Sin(), one and only one parameter is required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // [Cos] = Cos([EXPR])
      line = rgCos.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.COS, GenId("CO"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 1) throw new Exception("Invalid Cos(), one and only one parameter is required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // [Tan] = Tan([EXPR])
      line = rgTan.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.TAN, GenId("TA"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 1) throw new Exception("Invalid Tan(), one and only one parameter is required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // [aTan2] = aTan2([EXPR],[EXPR])
      line = rgAtan2.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.ATAN2, GenId("AT"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        ParsePars(n, pars);
        int num = ParsePars(n, pars);
        if (num != 2) throw new Exception("Invalid Atan2(), 2 parameters are required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // [Sqrt] = Sqrt([EXPR])
      line = rgSqrt.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.SQR, GenId("SQ"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 1) throw new Exception("Invalid Sqrt(), one and only one parameter is required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // [pow] = exp([EXPR],[EXPR])
      line = rgPow.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.POW, GenId("PW"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 2) throw new Exception("Invalid Pow(), 2 parameters are required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;


      // [LabelGet] = Label([EXPR])
      line = rgLabelGet.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.LABG, GenId("LB"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 1) throw new Exception("Invalid Label(), 1 and only 1 parameter is required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;


      // [TileGet] = TileGet([EXPR],[EXPR],[EXPR])
      line = rgTileget.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.TILEGET, GenId("TG"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 3) throw new Exception("Invalid TileGet(), 3 parameters are required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // [TileGetRot] = TileGetRot([EXPR],[EXPR],[EXPR])
      line = rgTilerot.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.TILEGETROT, GenId("TR"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 2) throw new Exception("Invalid TileGetRot(), 3 parameters are required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;


      // [GetP] = GetP([EXPR], [EXPR])
      // GETP
      // Replace GETP => `GPx
      line = rgGetP.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.GETP, GenId("GP"), origForException, linenumber);
        string pars = m.Groups[1].Value.Trim();
        int num = ParsePars(n, pars);
        if (num != 2) throw new Exception("Invalid GetP(), 2 parameters are required. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // STR.Substring(start, len)
      line = rgSubstring.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.SUBSTRING, GenId("SS"), origForException, linenumber);
        string left = m.Groups[1].Value.Trim();
        n.Add(nodes[left]);
        string pars = m.Groups[2].Value.Trim();
        int num = ParsePars(n, pars);
        if (num < 1) throw new Exception("Invalid Substring(), at least the start of th estring is required. Line: " + (linenumber + 1));
        if (num > 2) throw new Exception("Invalid Substring(), max two parameters are possible. Line: " + (linenumber + 1));
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // functions => `FNx
      line = rgFunctionCall.Replace(line, m => {
        // Check that the function is defined and it is not a reserved keywork: m.Groups[1]
        string fname = m.Groups[1].Value.Trim().ToLowerInvariant();
        if (!functions.ContainsKey(fname)) throw new Exception("A function named \"" + fname + "\"\nis not defined\n" + (linenumber + 1) + ": " + origForException);
        if (reserverdKeywords.Contains(fname)) throw new Exception("A reserved keyword is used as function:\n\"" + fname + "\"\n" + (linenumber + 1) + ": " + origForException);
        CodeNode n = new CodeNode(BNF.FunctionCall, GenId("FN"), origForException, linenumber) { sVal = fname };

        // Parse each parameter as expression: m.Groups[2]
        string pars = m.Groups[2].Value.Trim();
        CodeNode ps = new CodeNode(BNF.Params, line, linenumber);
        n.Add(ps);
        ParsePars(ps, pars);

        atLeastOneReplacement = true;
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // PAR
      // Replace PAR => `PRx
      line = rgPars.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.OPpar, GenId("PR"), origForException, linenumber);
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
        CodeNode n = new CodeNode(BNF.LEN, GenId("LN"), origForException, linenumber);
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
        CodeNode n = new CodeNode(BNF.PLEN, GenId("PL"), origForException, linenumber);
        if (m.Groups.Count < 2) throw new Exception("Unhandled PLEN case: " + m.Groups.Count + " Line:" + (linenumber + 1));
        string left = m.Groups[1].Value.Trim();
        n.Add(nodes[left]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // STR.trim
      line = rgTrim.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.TRIM, GenId("TR"), origForException, linenumber);
        if (m.Groups.Count < 2) throw new Exception("Unhandled TRIM case: " + m.Groups.Count + " Line:" + (linenumber + 1));
        string left = m.Groups[1].Value.Trim();
        n.Add(nodes[left]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@l => `MDx
      line = rgMemL.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlong, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@b => `MDx
      line = rgMemB.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongb, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@i => `MDx
      line = rgMemI.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongi, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@f => `MDx
      line = rgMemF.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongf, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@s => `MDx
      line = rgMemS.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMlongs, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // Replace MEM@c => `MDx
      line = rgMemC.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEMchar, "MD", line, m);
      });
      if (atLeastOneReplacement) continue;

      // MEM
      // Replace MEM => `MMx
      line = rgMem.Replace(line, m => {
        atLeastOneReplacement = true;
        return ParseMem(BNF.MEM, "MM", line, m);
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
            n = new CodeNode(BNF.KEYx, GenId("KX"), origForException, linenumber);
            nodes[n.id] = n;
            return n.id;
          case 'y':
            n = new CodeNode(BNF.KEYy, GenId("KY"), origForException, linenumber);
            nodes[n.id] = n;
            return n.id;
          default: throw new Exception("Invalid Key at " + (linenumber + 1) + "\n" + line);
        }
        n = new CodeNode(BNF.KEY, GenId("KK"), origForException, linenumber) { iVal = pos };
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
        CodeNode n = new CodeNode(BNF.CASTi, GenId("QI"), origForException, linenumber);
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _i => QB
      line = rgCastB.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTb, GenId("QB"), origForException, linenumber);
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _i => QI
      line = rgCastF.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTf, GenId("QF"), origForException, linenumber);
        string child = m.Groups[1].Value.Trim();
        n.Add(nodes[child]);
        nodes[n.id] = n;
        return n.id;
      });
      if (atLeastOneReplacement) continue;

      // _s => QS
      line = rgCastS.Replace(line, m => {
        atLeastOneReplacement = true;
        CodeNode n = new CodeNode(BNF.CASTs, GenId("QS"), origForException, linenumber);
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
        return HandleOperand(BNF.COMPgt, "GT", ">", m);
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
      throw new Exception("Invalid expression at " + (linenumber + 1) + "\n" + origForException + "\n" + line);
    }
    return nodes[line];
  }

  private int ParsePars(CodeNode ps, string pars) {
    // We need to grab each single parameter, they are separated by commas (,) but other functions can be nested
    int num = 0;
    int nump = 0;
    string parline = "";
    foreach (char c in pars) {
      if (c == '(') { parline += c; nump++; }
      else if (c == ')') { parline += c; nump--; }
      else if (c == ',' && nump == 0) {
        // Parse
        parline = parline.Trim(' ', ',');
        ps.Add(ParseExpression(parline));
        num++;
        parline = "";
      }
      else parline += c;
    }
    // parse the remaining part
    parline = parline.Trim(' ', ',');
    if (!string.IsNullOrEmpty(parline)) {
      ps.Add(ParseExpression(parline));
      num++;
    }
    return num;
  }

  void ParseAssignment(string line, CodeNode parent, BNF bnf, string match) {
    string fullorigline = origForException;
    string dest = line.Substring(0, line.IndexOf(match));
    string val = line.Substring(line.IndexOf(match) + 2);
    CodeNode node = new CodeNode(bnf, line, linenumber);
    expected.Set(Expected.Val.MemReg);
    ParseLine(node, dest, null);
    parent.Add(node);
    origForException = fullorigline;
    node.Add(ParseExpression(val));
    expected.Set(Expected.Val.Statement);
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

  private void ParseConfigBlock(string[] lines, int start, int end, CodeNode config) {
    // Find at what line this starts
    string remaining = "";
    for (int linenum = start + 1; linenum < end; linenum++) {
      string clean = lines[linenum].Trim();
      // Remove the comments and some unwanted chars
      clean = rgMLBacktick.Replace(clean, "'");
      // Find Screen and Ram
      if (clean.IndexOf("screen") != -1) { // ScreenCfg ***************************************************************** ScreenCfg
        int pos = clean.IndexOf(")");
        clean = clean.Substring(0, pos + 1).Trim(' ', '\n').ToLowerInvariant();
        Match m = rgConfScreen.Match(clean);
        int.TryParse(m.Groups[1].Value.Trim(), out int w);
        int.TryParse(m.Groups[2].Value.Trim(), out int h);
        bool filter = (!string.IsNullOrEmpty(m.Groups[3].Value) && m.Groups[3].Value.IndexOf('f') != -1);
        CodeNode n = new CodeNode(BNF.ScrConfig, null, linenum) { fVal = w, iVal = h, sVal = (filter ? "*" : "") };
        config.Add(n);
      }
      else if (clean.IndexOf("ram") != -1) { // RAM ****************************************************************** RAM
        int pos = clean.IndexOf(")");
        clean = clean.Substring(0, pos + 1).Trim(' ', '\n').ToLowerInvariant();
        Match m = rgRam.Match(clean);
        int.TryParse(m.Groups[1].Value.Trim(), out int size);
        char unit = (m.Groups[2].Value.Trim().ToLowerInvariant() + " ")[0];
        if (unit == 'k') size *= 1024;
        if (unit == 'm') size *= 1024 * 1024;
        CodeNode n = new CodeNode(BNF.Ram, null, linenum) { iVal = size };
        config.Add(n);
      }
      else
        remaining += clean + "\n";
    }
  }

  private void ParseDataBlock(string[] lines, int start, int end, CodeNode data) {
    // Find at what line this starts
    string clean = "";
    for (int linenum = start + 1; linenum < end; linenum++) {
      // Remove the comments and some unwanted chars
      clean += rgMLBacktick.Replace(lines[linenum].Trim(), "'") + "\n";
    }
    ByteReader.ReadBlock(clean, out List<CodeLabel> labels, out byte[] rom);
    // Labels ****************************************************************** Label
    foreach (CodeLabel l in labels) {
      data.Add(new CodeNode(BNF.Label, "", 0) { bVal = null, iVal = l.start, sVal = l.name });
    }
    data.Add(new CodeNode(BNF.Rom, "", 0) { bVal = rom });
  }

}
