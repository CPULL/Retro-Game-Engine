using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Arcade : MonoBehaviour {
  const float updateTime = .5f;
  public bool DevMode = false;
  public RawImage Screen;
  public RawImage UI;
  public RectTransform rt;
  public TextMeshProUGUI FPS;
  public Audio audioManager;
  public Texture2D LogoTexture;
  Texture2D texture, textureUI;
  byte[] rawPixels, rawUI, rawTarget;
  readonly CodeParser cp = new CodeParser();
  int sw = 256;
  int sh = 160;
  int wm1 = 255;
  int hm1 = 159;
  float scaleW = 1920f / 256;
  float scaleH = 1080f / 160;
  bool useFilter = false;
  int memsize = 256 * 1024;
  int romsize = 0;
  byte[] mem;
  Variables variables = new Variables();
  readonly Dictionary<string, int> labels = new Dictionary<string, int>();
  readonly Dictionary<int, Texture2D> labelTextures = new Dictionary<int, Texture2D>();
  readonly Grob[] sprites = new Grob[256];

  float updateDelay = -1;
  float toWait = 0;
  CodeNode startCode;
  CodeNode updateCode;
  readonly ExecStacks stacks = new ExecStacks();
  readonly Dictionary<string, CodeNode> functions = new Dictionary<string, CodeNode>();
  readonly bool[] inputs = new bool[27];
  readonly System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
  public Material RGEPalette;
  readonly Color[] palette = new Color[256];
  public bool running = false;
  bool uiUpdated = false;

  public enum Keys {
    L = 0,  Lu = 1,  Ld = 2,
    R = 3,  Ru = 4,  Rd = 5,
    U = 6,  Uu = 7,  Ud = 8,
    D = 9,  Du = 10, Dd = 11,
    A = 12, Au = 13, Ad = 14,
    B = 15, Bu = 16, Bd = 17,
    C = 18, Cu = 19, Cd = 20,
    F = 21, Fu = 22, Fd = 23,
    E = 24, Eu = 25, Ed = 26
  }


  int FpsFrames = 0;
  float FpsTime = 0;
  int lastScreenW;
  int lastScreenH;
  CodeNode nodeToRun = null;


  private void Update() {
    running = false;
    if (updateDelay < 0) return;
    if (updateDelay > 0) {
      updateDelay -= Time.deltaTime;

      if (updateDelay > 0) {
        int num = (int)(sw * updateDelay / updateTime);
        for (int i = 0; i < num; i++) {
          SetPixel(i, hm1 - 1, 0b111111);
          SetPixel(i, hm1, 0b111111);
        }
        for (int i = num; i < sw; i++) {
          SetPixel(i, hm1 - 1, 0);
          SetPixel(i, hm1, 0);
        }
        CompleteFrame();
        return;
      }
      sprites[0].Pos(0, 0, scaleW, scaleH, false);
      updateDelay = 0;
      Col.UsePalette(false);
      RGEPalette.SetInt("_UsePalette", Col.UsePalette() ? 1 : 0);
    }

    int nowScreenW = (int)rt.rect.width;
    int nowScreenH = (int)rt.rect.height;
    if (nowScreenW != lastScreenW || nowScreenH != lastScreenH) {
      lastScreenW = (int)(nowScreenW * (Minimized ? .333333f : 1));
      lastScreenH = (int)(nowScreenH * (Minimized ? .333333f : 1));
      scaleW = lastScreenW / (float)sw;
      scaleH = lastScreenH / (float)sh;

      // update the height according to the aspect ratio
      float heightAccordingToWidth = UnityEngine.Screen.width / 16.0f * 9.0f;
      UnityEngine.Screen.SetResolution(UnityEngine.Screen.width, Mathf.RoundToInt(heightAccordingToWidth), false, 0);

      foreach (Grob s in sprites) {
        if (s != null && !s.notDefined) s.ResetScale(scaleW, scaleH);
      }
    }

    if (Input.GetKeyDown(KeyCode.F11)) {
      UnityEngine.Screen.fullScreen = !UnityEngine.Screen.fullScreen;
    }
    if (Input.GetKeyUp(KeyCode.Escape) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) {
      SceneManager.LoadScene("Loader");
    }

    running = true;
    FpsTime += Time.deltaTime;
    if (FpsTime > 1f) {
      FpsTime -= 1f;
      if (FPS != null) FPS.text = FpsFrames.ToString();
      FpsFrames = 0;
    }

    if (toWait > 0) {
      toWait -= Time.deltaTime;
      return;
    }

    if (stacks.Invalid) {
      runStatus = RunStatus.Stopped;
      return;
    }

    #region Key input
    for (int i = 0; i < inputs.Length; i++) inputs[i] = false;
    inputs[(int)Keys.U] = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow));
    inputs[(int)Keys.Uu] = (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.UpArrow));
    inputs[(int)Keys.Ud] = (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow));

    inputs[(int)Keys.D] = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
    inputs[(int)Keys.Du] = (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow));
    inputs[(int)Keys.Dd] = (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow));

    inputs[(int)Keys.L] = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow));
    inputs[(int)Keys.Lu] = (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.LeftArrow));
    inputs[(int)Keys.Ld] = (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow));

    inputs[(int)Keys.R] = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow));
    inputs[(int)Keys.Ru] = (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow));
    inputs[(int)Keys.Rd] = (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow));

    inputs[(int)Keys.F] = (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return));
    inputs[(int)Keys.Fu] = (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return));
    inputs[(int)Keys.Fd] = (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return));

    inputs[(int)Keys.A] = (Input.GetKey(KeyCode.I));
    inputs[(int)Keys.Au] = (Input.GetKeyUp(KeyCode.I));
    inputs[(int)Keys.Ad] = (Input.GetKeyDown(KeyCode.I));

    inputs[(int)Keys.B] = (Input.GetKey(KeyCode.O));
    inputs[(int)Keys.Bu] = (Input.GetKeyUp(KeyCode.O));
    inputs[(int)Keys.Bd] = (Input.GetKeyDown(KeyCode.O));

    inputs[(int)Keys.C] = (Input.GetKey(KeyCode.P));
    inputs[(int)Keys.Cu] = (Input.GetKeyUp(KeyCode.P));
    inputs[(int)Keys.Cd] = (Input.GetKeyDown(KeyCode.P));

    inputs[(int)Keys.E] = (Input.GetKey(KeyCode.Escape));
    inputs[(int)Keys.Eu] = (Input.GetKeyUp(KeyCode.Escape));
    inputs[(int)Keys.Ed] = (Input.GetKeyDown(KeyCode.Escape));

    #endregion

    // Are we paused?
    if (runStatus == RunStatus.Paused || runStatus == RunStatus.Error || runStatus == RunStatus.Stopped) return; // Yes, wait until we receive a go
    // No, run and at the end of the step check if we should pause (step or frame)
    if (runStatus == RunStatus.GoPause) {
      runStatus = RunStatus.Paused;
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
      return;
    }

    bool something = false;
    int numruns = 0;

    while (nodeToRun != null) {
      something = true;

      if (Execute(nodeToRun)) {
        CompleteFrame();
        nodeToRun = stacks.GetExecutionNode(this);
        StepExecutedCheck();
        return; // Skip the execution for now so Unity can actually draw the frame
      }
      if (runStatus == RunStatus.Stopped || runStatus == RunStatus.Error) {
        CompleteFrame();
        execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
        varsCallback?.Invoke(variables);
        return;
      }
      numruns++;
      if (numruns > 1024 * 256) {
        Write("Possible infinite loop at: " + nodeToRun.parent.origLineNum + "\n" + nodeToRun.parent.origLine, 4, 4, Col.C(5, 4, 0), 0);
        CompleteFrame();
        nodeToRun = stacks.GetExecutionNode(this);
        StepExecutedCheck();
        return;
      }
      nodeToRun = stacks.GetExecutionNode(this);
      if (StepExecutedCheck()) return;
    }
    nodeToRun = stacks.GetExecutionNode(this);
    if (nodeToRun == null) {
      stacks.Destroy();
      if (updateCode != null)
        stacks.AddStack(updateCode, null, updateCode.origLine, updateCode.origLineNum);
      nodeToRun = stacks.GetExecutionNode(this);
    }
    if (codeUpdated) {
      CompleteFrame();
      stacks.Destroy();
      stacks.AddStack(updateCodeUpdated, null, updateCodeUpdated.origLine, updateCodeUpdated.origLineNum);
      updateCode = updateCodeUpdated;
      variables = updatedVars;
      breakPoints = updatedBreaks;
      updateCodeUpdated = null;
      updatedVars = null;
      updatedBreaks = null;
      codeUpdated = false;
      CompleteFrame();
      StepExecutedCheck();
      return;
    }
    StepExecutedCheck();

    if (something) CompleteFrame();
    else if (FPS != null) FPS.text = "MAX";
  }

  bool StepExecutedCheck() {
    if (breakPoints != null && nodeToRun != null && breakPoints.Contains(nodeToRun.origLineNum)) {
      LastErrorMessage = "Breakpoint on " + nodeToRun.Format(variables, false);
      texture.Apply();
      runStatus = RunStatus.Paused;
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
      runStatus = RunStatus.Paused;
      return true;
    }
    if (nodeToRun != null && runStatus == RunStatus.RunAStep) {
      texture.Apply();
      runStatus = RunStatus.Paused;
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
      return true;
    }
    return false;
  }

  void CompleteFrame() {
    FpsFrames++;
    texture.LoadRawTextureData(rawPixels);
    texture.Apply();
    if (uiUpdated) {
      textureUI.LoadRawTextureData(rawUI);
      textureUI.Apply();
      uiUpdated = false;
    }
    varsCallback?.Invoke(variables);
    if (runStatus == RunStatus.RunAFrame || runStatus == RunStatus.RunAStep) {
      runStatus = RunStatus.Paused;
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
    }
  }


  private void Start() {
    Col.InitPalette(RGEPalette);
    texture = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    Screen.texture = texture;
    rawPixels = texture.GetRawTextureData();
    rawTarget = rawPixels;
    textureUI = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    UI.texture = textureUI;
    rawUI = textureUI.GetRawTextureData();
    Clear(0);
    ClearUI(255);
    uiUpdated = true;
    CompleteFrame();

    Write("--- MMM Arcade RGE ---", (sw - 22 * 8) / 2, 8, Col.C(5, 5, 0));
    Write("virtual machine", (sw - 15 * 8) / 2, 14 + 4, Col.C(1, 2, 3));
    Write("Retro Game Engine", (sw - 17 * 8) / 2, 14 + 9, Col.C(1, 5, 2));

    lastScreenW = (int)rt.rect.width;
    lastScreenH = (int)rt.rect.height;
    scaleW = lastScreenW / 256f;
    scaleH = lastScreenW / 160f;

    sprites[0] = Instantiate(SpriteTemplate, Layers[0]).GetComponent<Grob>();
    sprites[0].gameObject.name = "Sprite 0";
    sprites[0].gameObject.SetActive(true);
    sprites[0].Set(16, 16, LogoTexture, false);
    sprites[0].Pos(0, 8, scaleW, scaleH, true);
    audioManager.Init();
    NoiseS3D.Octaves = 2;

    if (DevMode) {
      DevStart();
    }
    else {
      RealStart();
    }
  }

  private void RealStart() {
    if (SceneManager.GetActiveScene().name == "ArcadePlus") {
      FileBrowser.SetLocation(Application.dataPath + "\\..\\Cartridges\\");
      FileBrowser.Load(SelectCartridge, FileBrowser.FileType.Cartridges);
    }
    else {
      // Load Game.Cartridge
      SelectCartridge(Application.dataPath + "/../Cartridges/Game.cartridge");
    }
    texture.Apply();
    for (int i = 0; i < 256; i++)
      palette[i] = Col.GetColor((byte)i);
    RGEPalette.SetColorArray("_Colors", palette);
    Col.UsePalette(false);
    RGEPalette.SetInt("_UsePalette", 0);
    RGEPalette.SetFloat("_Luma", 0);
    RGEPalette.SetFloat("_Contrast", 0);
    runStatus = RunStatus.Running;
    CurrentLineNumber = 0;
  }

  private void DevStart() {
    sw = 320;
    sh = 180;
    wm1 = sw - 1;
    hm1 = sh - 1;
    scaleW = rt.rect.width / sw;
    scaleH = rt.rect.height / sh;
    useFilter = false;
    texture = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = useFilter ? FilterMode.Bilinear : FilterMode.Point };
    Screen.texture = texture;
    rawPixels = texture.GetRawTextureData();
    rawTarget = rawPixels;
    textureUI = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = useFilter ? FilterMode.Bilinear : FilterMode.Point };
    UI.texture = textureUI;
    rawUI = textureUI.GetRawTextureData();
    sprites[0].Pos(0, 8, scaleW, scaleH, true);
    Clear(0);
    ClearUI(255);
    Write("--- MMM Arcade RGE ---", (sw - 22 * 8) / 2, 8, Col.C(5, 5, 0));
    Write("virtual machine", (sw - 15 * 8) / 2, 14 + 4, Col.C(1, 2, 3));
    Write("Retro Game Engine", (sw - 17 * 8) / 2, 14 + 9, Col.C(1, 5, 2));
    Write("Run your code or Debug", 8, 48, Col.C(5, 4, 0));
    for (int i = 0; i < 256; i++)
      palette[i] = Col.GetColor((byte)i);
    RGEPalette.SetColorArray("_Colors", palette);
    Col.UsePalette(false);
    RGEPalette.SetInt("_UsePalette", 0);
    RGEPalette.SetFloat("_Luma", 0);
    RGEPalette.SetFloat("_Contrast", 0);
    runStatus = RunStatus.Stopped;
    CurrentLineNumber = 0;
    uiUpdated = true;
    CompleteFrame();
  }

  public void SelectCartridge(string path) {
    string codefile;
    try { codefile = File.ReadAllText(path); } catch (Exception) {
      LastErrorMessage = "No cardridge found!\nPath: " + path;
      runStatus = RunStatus.Stopped;
      Write("No cardridge found!", 4, 40, Col.C(5, 1, 0));
      Write("Path: " + path, 4, 50, Col.C(5, 1, 0), 0, 2);
      texture.Apply();
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
      return;
    }

    // Check if we have a rom file
    string rom = null;
    string name = path;
    int dot = name.LastIndexOf('.');
    if (dot != -1) 
      name = name.Substring(0, dot) + ".rom";
    else
      name += ".rom";
    if (File.Exists(name)) { // Yes, read the file as binary
      rom = name;
    }

    LoadCartridge(codefile, rom);
  }

  public void LoadCartridge(string codefile, string rompath) {
    ResetArcade();
    Col.UsePalette(false);
    Col.SetDefaultPalette();
    RGEPalette.SetInt("_UsePalette", 0);
    if (string.IsNullOrEmpty(codefile)) {
      Write("No cardridge found!", 4, 40, Col.C(5, 1, 0));
      texture.Apply();
      return;
    }
    try {
      cp.ParseSingleBlock = false;
      CodeNode res = cp.Parse(codefile, variables, rompath == null);
      Write("Cartridge:", 4, 39, Col.C(1, 3, 4));
      if (res.sVal == null)
        Write("<no name>", 88, 39, Col.C(5, 3, 1));
      else
        Write(res.sVal, 88, 39, Col.C(5, 3, 1));

      // Config ************************************************************************************************************** Config
      CodeNode config = res.Get(BNF.Config);
      if (config != null) {
        // Screen ************************************************************************************************************** Screen
        CodeNode scrconf = config.Get(BNF.SCREEN);
        if (scrconf != null) {
          sw = Evaluate(scrconf.CN1).ToInt(culture);
          sh = Evaluate(scrconf.CN2).ToInt(culture);
          if (sw < 160) sw = 160;
          if (sw > 320) sw = 320;
          if (sh < 100) sh = 100;
          if (sh > 256) sh = 256;
          wm1 = sw - 1;
          hm1 = sh - 1;
          scaleW = rt.rect.width / sw;
          scaleH = rt.rect.height / sh;
          useFilter = Evaluate(scrconf.CN3).ToBool(culture);
          texture = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = useFilter ? FilterMode.Bilinear : FilterMode.Point };
          Screen.texture = texture;
          rawPixels = texture.GetRawTextureData();
          rawTarget = rawPixels;
          textureUI = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = useFilter ? FilterMode.Bilinear : FilterMode.Point };
          UI.texture = textureUI;
          rawUI = textureUI.GetRawTextureData();
        }
        sprites[0].Pos(0, 8, scaleW, scaleH, true);

        // Memory ************************************************************************************************************** Memory
        CodeNode memdef = config.Get(BNF.Ram);
        if (memdef != null) {
          if (memdef.iVal < 1024) memdef.iVal = 1024;
          if (memdef.iVal > 4096 * 1024) memdef.iVal = 4096 * 1024;
          memsize = memdef.iVal;
        }
        else {
          memsize = 1 * 1024;
        }
      }

      // Redraw
      Clear(0);
      ClearUI(255);
      Write("--- MMM Arcade RGE ---", (sw - 22 * 8) / 2, 8, Col.C(5, 5, 0));
      Write("virtual machine", (sw - 15 * 8) / 2, 14 + 4, Col.C(1, 2, 3));
      Write("Retro Game Engine", (sw - 17 * 8) / 2, 14 + 9, Col.C(1, 5, 2));
      Write("Cartridge:", 4, 39, Col.C(1, 3, 4));
      if (res.sVal == null)
        Write("<no name>", 88, 39, Col.C(5, 3, 1));
      else
        Write(res.sVal, 88, 39, Col.C(5, 3, 1));

      if (rompath != null) {
        ByteChunk data = new ByteChunk();
        ByteReader.ReadBinBlock(rompath, data);
        romsize = data.block.Length;
        Write("Data:   ROM " + MemSize(romsize), 4, 48 + 18, Col.C(1, 3, 4));
        mem = new byte[memsize + romsize];
        int pos = memsize;
        for (int i = 0; i < romsize; i++)
          mem[pos++] = data.block[i];
        foreach(CodeLabel l in data.labels) {
          labels.Add(l.name.Trim().ToLowerInvariant(), memsize + l.start);
        }
      }
      else if (res.HasNode(BNF.Data)) {
        CodeNode data = res.Get(BNF.Data);
        Write("Data:   Yes, source", 4, 48 + 18, Col.C(1, 3, 4));

        // ROM ****************************************************************************************************************** ROM
        CodeNode romdef = data.Get(BNF.Rom);
        if (romdef != null) {
          romsize = romdef.bVal.Length;
          mem = new byte[memsize + romsize];
          int pos = memsize;
          for (int i = 0; i < romsize; i++)
            mem[pos++] = romdef.bVal[i];
        }

        // PALETTE ****************************************************************************************************************** PALETTE
        CodeNode paldef = data.Get(BNF.PaletteConfig);
        if (paldef != null) {
          Col.UsePalette(paldef.iVal != 0);
          RGEPalette.SetInt("_UsePalette", paldef.iVal != 0 ? 1 : 0);
        }

        // LABELS *************************************************************************************************************** LABELS
        foreach (CodeNode n in data.children) {
          if (n.type == BNF.Label) {
            labels.Add(n.sVal, memsize + n.iVal);
          }
        }
      }
      else {
        Write("Data:   ", 4, 48 + 18, Col.C(1, 3, 4));
        Write("<missing>", 68, 48 + 18, Col.C(5, 3, 1));
        mem = new byte[256 * 1024];
      }

      startCode = res.Get(BNF.Start);
      Write("Start:  ", 4, 48, Col.C(1, 3, 4));
      if (startCode != null && startCode.children != null && startCode.children.Count > 0) {
        Write("Yes", 68, 48, Col.C(1, 3, 4));
      }
      else {
        Write("<missing>", 68, 48, Col.C(5, 3, 1));
        startCode = null;
      }

      Write("Update: ", 4, 48 + 9, Col.C(1, 3, 4));
      updateCode = res.Get(BNF.Update);
      if (updateCode != null && updateCode.children != null && updateCode.children.Count > 0) {
        Write("Yes", 68, 48 + 9, Col.C(1, 3, 4));
      }
      else {
        Write("<missing>", 68, 48 + 9, Col.C(5, 3, 1));
        updateCode = null;
      }

      Write("Screen: " + sw + " x " + sh, 10, 100, Col.C(1, 3, 4));
      Write("Memory: " + MemSize(memsize), 10, 110, Col.C(1, 3, 4));

      updateDelay = updateTime;

      CodeNode funcs = res.Get(BNF.Functions);
      if (funcs != null && funcs.children != null) {
        foreach(CodeNode f in funcs.children) {
          functions[f.sVal] = f;
        }
      }

      texture.Apply();

      if (startCode != null)
        stacks.AddStack(startCode, null, startCode.origLine, startCode.origLineNum);
      else if (updateCode != null)
        stacks.AddStack(updateCode, null, updateCode.origLine, updateCode.origLineNum);
    } catch (ParsingException e) {
      string msg = "";
      for (int i = 0, l = 0; i < e.Message.Length; i++) {
        char c = e.Message[i];
        if (c == '\n') l = 0;
        msg += c;
        l++;
        if (l == sw / 8 - 1) {
          msg += "\n";
          l = 0;
        }
      }
      LastErrorMessage = "Error in loading! " + e.Message + "\n" + e.Code + "\nLine: " + e.LineNum;
      Write("Error in loading!\n" + msg + "\n" + e.Code + "\nLine: " + e.LineNum, 4, 48, Col.C(5, 1, 0));
      texture.Apply();
      Debug.Log("Error in loading! " + e.Message + "\n" + e.Code + "\nLine: " + e.LineNum + "\n" + e.StackTrace);
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
    } catch (Exception e) {
      runStatus = RunStatus.Error;
      string msg = "";
      for (int i = 0, l = 0; i < e.Message.Length; i++) {
        char c = e.Message[i];
        if (c == '\n') l = 0;
        msg += c;
        l++;
        if (l == sw / 8 - 1) {
          msg += "\n";
          l = 0;
        }
      }
      LastErrorMessage = "Error in loading! " + e.Message;
      Write("Error in loading!\n" + msg, 4, 48, Col.C(5, 1, 0));
      texture.Apply();
      Debug.Log("Error in loading! " + e.Message + "\n" + e.StackTrace);
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
    }
  }

  Action<Variables> varsCallback = null;
  Action<int> execCallback = null;
  public int CurrentLineNumber { get; private set; } = 0;
  public enum RunStatus { Stopped=0, Running=1, Paused = 2, RunAStep=3, RunAFrame=4, GoPause=10, Error=99 };
  public RunStatus runStatus = RunStatus.Stopped;
  public string LastErrorMessage = null;
  HashSet<int> breakPoints = null;
  public void SetBreakpoints(HashSet<int> breaks) {
    breakPoints = breaks;
  }

  public void LoadCode(CodeNode code, Variables vars, ByteChunk romdata, Action<Variables> varsCB, Action<int> execCB, HashSet<int> breaks) { // Labels too
    ResetArcade();
    Col.UsePalette(false);
    Col.SetDefaultPalette();
    RGEPalette.SetInt("_UsePalette", 0);
    updateDelay = -1;
    Clear(0);
    ClearUI(255);
    CompleteFrame();
    variables = vars;
    labels.Clear();
    varsCallback = varsCB;
    execCallback = execCB;
    breakPoints = breaks;

    try {
      Write("Cartridge:", 4, 39, Col.C(1, 3, 4));
      if (code.sVal == null)
        Write("<no name>", 88, 39, Col.C(5, 3, 1));
      else
        Write(code.sVal, 88, 39, Col.C(5, 3, 1));

      // Config ************************************************************************************************************** Config
      CodeNode config = code.Get(BNF.Config);
      if (config != null) {
        // Screen ************************************************************************************************************** Screen
        CodeNode scrconf = config.Get(BNF.SCREEN);
        if (scrconf != null) {
          sw = Evaluate(scrconf.CN1).ToInt(culture);
          sh = Evaluate(scrconf.CN2).ToInt(culture);
          if (sw < 160) sw = 160;
          if (sw > 320) sw = 320;
          if (sh < 100) sh = 100;
          if (sh > 256) sh = 256;
          wm1 = sw - 1;
          hm1 = sh - 1;
          scaleW = rt.rect.width / sw;
          scaleH = rt.rect.height / sh;
          useFilter = Evaluate(scrconf.CN3).ToBool(culture);
          texture = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = useFilter ? FilterMode.Bilinear : FilterMode.Point };
          Screen.texture = texture;
          rawPixels = texture.GetRawTextureData();
          rawTarget = rawPixels;
          textureUI = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = useFilter ? FilterMode.Bilinear : FilterMode.Point };
          UI.texture = textureUI;
          rawUI = textureUI.GetRawTextureData();
        }
        sprites[0].Pos(0, 8, scaleW, scaleH, true);

        // Memory ************************************************************************************************************** Memory
        CodeNode memdef = config.Get(BNF.Ram);
        if (memdef != null) {
          if (memdef.iVal < 1024) memdef.iVal = 1024;
          if (memdef.iVal > 4096 * 1024) memdef.iVal = 4096 * 1024;
          memsize = memdef.iVal;
        }
        else {
          memsize = 256 * 1024;
        }
      }

      // Redraw
      Clear(0);
      ClearUI(255);
      Write("--- MMM Arcade RGE ---", (sw - 22 * 8) / 2, 8, Col.C(5, 5, 0));
      Write("virtual machine", (sw - 15 * 8) / 2, 14 + 4, Col.C(1, 2, 3));
      Write("Retro Game Engine", (sw - 17 * 8) / 2, 14 + 9, Col.C(1, 5, 2));
      Write("Cartridge:", 4, 39, Col.C(1, 3, 4));
      if (code.sVal == null)
        Write("<no name>", 88, 39, Col.C(5, 3, 1));
      else
        Write(code.sVal, 88, 39, Col.C(5, 3, 1));

      if (romdata != null) {
        romsize = romdata.block.Length;
        Write("Data:   ROM " + MemSize(romsize), 4, 48 + 18, Col.C(1, 3, 4));
        mem = new byte[memsize + romsize];
        int pos = memsize;
        for (int i = 0; i < romsize; i++)
          mem[pos++] = romdata.block[i];
        foreach (CodeLabel l in romdata.labels) {
          labels.Add(l.name.Trim().ToLowerInvariant(), memsize + l.start);
        }
      }
      else if (code.HasNode(BNF.Data)) {
        CodeNode data = code.Get(BNF.Data);
        Write("Data:   Yes, source", 4, 48 + 18, Col.C(1, 3, 4));

        // ROM ****************************************************************************************************************** ROM
        CodeNode romdef = data.Get(BNF.Rom);
        if (romdef != null) {
          romsize = romdef.bVal.Length;
          mem = new byte[memsize + romsize];
          int pos = memsize;
          for (int i = 0; i < romsize; i++)
            mem[pos++] = romdef.bVal[i];
        }

        // PALETTE ****************************************************************************************************************** PALETTE
        CodeNode paldef = data.Get(BNF.PaletteConfig);
        if (paldef != null) {
          Col.UsePalette(paldef.iVal != 0);
          RGEPalette.SetInt("_UsePalette", paldef.iVal != 0 ? 1 : 0);
        }

        // LABELS *************************************************************************************************************** LABELS
        foreach (CodeNode n in data.children) {
          if (n.type == BNF.Label) {
            labels.Add(n.sVal, memsize + n.iVal);
          }
        }
      }
      else {
        Write("Data:   ", 4, 48 + 18, Col.C(1, 3, 4));
        Write("<missing>", 68, 48 + 18, Col.C(5, 3, 1));
        mem = new byte[256 * 1024];
      }

      startCode = code.Get(BNF.Start);
      Write("Start:  ", 4, 48, Col.C(1, 3, 4));
      if (startCode != null && startCode.children != null && startCode.children.Count > 0) {
        Write("Yes", 68, 48, Col.C(1, 3, 4));
      }
      else {
        Write("<missing>", 68, 48, Col.C(5, 3, 1));
        startCode = null;
      }

      Write("Update: ", 4, 48 + 9, Col.C(1, 3, 4));
      updateCode = code.Get(BNF.Update);
      if (updateCode != null && updateCode.children != null && updateCode.children.Count > 0) {
        Write("Yes", 68, 48 + 9, Col.C(1, 3, 4));
      }
      else {
        Write("<missing>", 68, 48 + 9, Col.C(5, 3, 1));
        updateCode = null;
      }

      Write("Screen: " + sw + " x " + sh, 10, 100, Col.C(1, 3, 4));
      Write("Memory: " + MemSize(memsize), 10, 110, Col.C(1, 3, 4));

      updateDelay = updateTime;

      CodeNode funcs = code.Get(BNF.Functions);
      if (funcs != null && funcs.children != null) {
        foreach (CodeNode f in funcs.children) {
          functions[f.sVal] = f;
        }
      }

      texture.Apply();

      if (startCode != null)
        stacks.AddStack(startCode, null, startCode.origLine, startCode.origLineNum);
      else if (updateCode != null)
        stacks.AddStack(updateCode, null, updateCode.origLine, updateCode.origLineNum);
      runStatus = RunStatus.Running;

    } catch (ParsingException e) {
      runStatus = RunStatus.Error;
      string msg = "";
      for (int i = 0, l = 0; i < e.Message.Length; i++) {
        char c = e.Message[i];
        if (c == '\n') l = 0;
        msg += c;
        l++;
        if (l == sw / 8 - 1) {
          msg += "\n";
          l = 0;
        }
      }
      LastErrorMessage = "Error in loading! " + e.Message + "\n" + e.Code + "\nLine: " + e.LineNum;
      Write("Error in loading!\n" + msg + "\n" + e.Code + "\nLine: " + e.LineNum, 4, 48, Col.C(5, 1, 0));
      texture.Apply();
      Debug.Log("Error in loading! " + e.Message + "\n" + e.Code + "\nLine: " + e.LineNum + "\n" + e.StackTrace);
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
    } catch (Exception e) {
      runStatus = RunStatus.Error;
      string msg = "";
      for (int i = 0, l = 0; i < e.Message.Length; i++) {
        char c = e.Message[i];
        if (c == '\n') l = 0;
        msg += c;
        l++;
        if (l == sw / 8 - 1) {
          msg += "\n";
          l = 0;
        }
      }
      LastErrorMessage = "Error in loading! " + e.Message;
      Write("Error in loading!\n" + msg, 4, 48, Col.C(5, 1, 0));
      texture.Apply();
      Debug.Log("Error in loading! " + e.Message + "\n" + e.StackTrace);
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      varsCallback?.Invoke(variables);
    }
  }

  CodeNode updateCodeUpdated = null;
  Variables updatedVars = null;
  HashSet<int> updatedBreaks = null;
  bool codeUpdated = false;
  public void UpdateCode(CodeNode newcode, Variables newvars, HashSet<int> newbreaks) {
    updateCodeUpdated = newcode.Get(BNF.Update);
    if (updateCodeUpdated != null) {
      updatedVars = newvars;
      updatedBreaks = newbreaks;
      codeUpdated = true;
    }
  }

  public void ReadVariables() {
    varsCallback?.Invoke(variables);
  }

  private string MemSize(int size) {
    string m;
    if (size < 1024)
      m = "<1k (" + size + ")";
    else if (size < 1024 * 1024)
      m = (size / 1024) + "k (" + size + ")";
    else {
      float mb = size / (1024 * 1024.0f);
      if (mb != Mathf.Floor(mb))
        m = ((int)(mb * 10) / 10.0) + "m (" + size + ")";
      else
        m = (int)mb + "m (" + size + ")";
    }
    return m;
  }

  #region Drawing functions ****************************************************************************************************************************************************************************************************

  void SetPixel(int x, int y, byte col) {
    if (x < 0 || x > wm1 || y < 0 || y > hm1) return;
    Color32 c = Col.GetColor(col);
    rawTarget[(x + sw * (hm1 - y)) * 4] = c.r;
    rawTarget[(x + sw * (hm1 - y)) * 4 + 1] = c.g;
    rawTarget[(x + sw * (hm1 - y)) * 4 + 2] = c.b;
    rawTarget[(x + sw * (hm1 - y)) * 4 + 3] = c.a;
  }

  void SetPixel(int x, int y, Color32 c) {
    if (x < 0 || x > wm1 || y < 0 || y > hm1) return;
    rawTarget[(x + sw * (hm1 - y)) * 4] = c.r;
    rawTarget[(x + sw * (hm1 - y)) * 4 + 1] = c.g;
    rawTarget[(x + sw * (hm1 - y)) * 4 + 2] = c.b;
    rawTarget[(x + sw * (hm1 - y)) * 4 + 3] = c.a;
  }

  void SetPixel(int x, int y, byte r, byte g, byte b) {
    if (x < 0 || x > wm1 || y < 0 || y > hm1) return;
    rawTarget[(x + sw * (hm1 - y)) * 4] = r;
    rawTarget[(x + sw * (hm1 - y)) * 4 + 1] = g;
    rawTarget[(x + sw * (hm1 - y)) * 4 + 2] = b;
  }

  void Luma(float v) {
    if (v < -1) v = -1;
    if (v > 1) v = 1;
    RGEPalette.SetFloat("_Luma", v);
  }
  void Contrast(float v) {
    if (v < -1) v = -1;
    if (v > 1) v = 1;
    RGEPalette.SetFloat("_Contrast", v);
  }

  int GetPixel(int x, int y) {
    if (x < 0 || x > wm1 || y < 0 || y > hm1) return 255;
    Color32 pixel = texture.GetPixel(x, hm1 - y);
    return Col.GetColorByte(pixel);
  }

  void Console(string txt, byte color = 0) {
    int line = sw * 32;
    int size = sw * sh * 4;
    for (int i = size - 4; i >= line; i -= 4) {
      rawUI[i + 0] = rawUI[i + 0 - line];
      rawUI[i + 1] = rawUI[i + 1 - line];
      rawUI[i + 2] = rawUI[i + 2 - line];
      rawUI[i + 3] = rawUI[i + 3 - line];
    }
    for (int i = 0; i < line; i += 4) {
      rawUI[i + 0] = 0;
      rawUI[i + 1] = 0;
      rawUI[i + 2] = 0;
      rawUI[i + 3] = 0;
    }
    rawTarget = rawUI;
    Write(txt, 0, sh - 8, color);
    rawTarget = rawPixels;
    uiUpdated = true;
  }

  void Write(string txt, int x, int y, byte col, byte back = 255, byte mode = 0) {
    if (mode == 1) Write6(txt, x, y, col, back);
    else if (mode == 2) WriteC(txt, x, y, col, back);
    else if (mode == 3) WriteC6(txt, x, y, col, back);
    else Write8(txt, x, y, col, back);
  }

  void Write8(string txt, int x, int y, byte col, byte back) {
    int pos = x;
    Color32 frontc = Col.GetColor(col);
    Color32 backc = Col.GetColor(back);
    foreach (char c in txt) {
      if (c == '\n' || c == '\r') {
        y += 8;
        pos = x;
        if (y > 127) return;
        continue;
      }
      byte[] gliph;
      if (!font.ContainsKey(c))
        gliph = font['*'];
      else
        gliph = font[c];
      for (int h = 0; h < 8; h++) {
        for (int w = 0; w < 8; w++) {
          if ((gliph[h] & (1 << (7 - w))) != 0)
            SetPixel(pos + w, y + h, frontc);
          else if (back != 255 || rawTarget == rawUI)
            SetPixel(pos + w, y + h, backc);
        }
      }
      pos += 8;
      if (pos > wm1) return;
    }
  }

  void Write6(string txt, int x, int y, byte col, byte back) {
    int pos = x;
    Color32 frontc = Col.GetColor(col);
    Color32 backc = Col.GetColor(back);
    foreach (char c in txt) {
      if (c == '\n' || c == '\r') {
        y += 8;
        pos = x;
        if (y > 127) return;
        continue;
      }
      byte[] gliph;
      if (!font6.ContainsKey(c))
        gliph = font6['*'];
      else
        gliph = font6[c];
      for (int h = 0; h < 8; h++) {
        for (int w = 0; w < 6; w++) {
          if ((gliph[h] & (1 << (7 - w))) != 0)
            SetPixel(pos + w, y + h, frontc);
          else if (back != 255 || rawTarget == rawUI)
            SetPixel(pos + w, y + h, backc);
        }
      }
      pos += 6;
      if (pos > wm1) return;
    }
  }

  void WriteC(string txt, int x, int y, byte col, byte back) {
    if (back == 255) back = 0;
    int pos = x;
    Color32 frontc = Col.GetColor(col);
    Color32 backc = Col.GetColor(back);
    foreach (char c in txt) {
      if (c == '\n' || c == '\r') {
        y += 8;
        pos = x;
        if (y > 127) return;
        continue;
      }
      byte[] gliph;
      if (!font.ContainsKey(c))
        gliph = font['*'];
      else
        gliph = font[c];
      for (int h = 0; h < 8; h++) {
        for (int w = 0; w < 8; w += 2) {
          int mr = (gliph[h] & (1 << (7 - w))) != 0 ? frontc.r : backc.r;
          int mg = (gliph[h] & (1 << (7 - w))) != 0 ? frontc.g : backc.g;
          int mb = (gliph[h] & (1 << (7 - w))) != 0 ? frontc.b : backc.b;
          mr += (gliph[h] & (1 << (6 - w))) != 0 ? frontc.r : backc.r;
          mg += (gliph[h] & (1 << (6 - w))) != 0 ? frontc.g : backc.g;
          mb += (gliph[h] & (1 << (6 - w))) != 0 ? frontc.b : backc.b;
          SetPixel(pos + w / 2, y + h, (byte)(mr >> 1), (byte)(mg >> 1), (byte)(mb >> 1));
        }
      }
      pos += 4;
      if (pos > wm1) return;
    }
  }

  void WriteC6(string txt, int x, int y, byte col, byte back) {
    if (back == 255) back = 0;
    int pos = x;
    Color32 frontc = Col.GetColor(col);
    Color32 backc = Col.GetColor(back);
    foreach (char c in txt) {
      if (c == '\n' || c == '\r') {
        y += 8;
        pos = x;
        if (y > 127) return;
        continue;
      }
      byte[] gliph;
      if (!font6.ContainsKey(c))
        gliph = font6['*'];
      else
        gliph = font6[c];
      for (int h = 0; h < 8; h++) {
        for (int w = 0; w < 6; w+=2) {
          int mr = (gliph[h] & (1 << (7 - w))) != 0 ? frontc.r : backc.r;
          int mg = (gliph[h] & (1 << (7 - w))) != 0 ? frontc.g : backc.g;
          int mb = (gliph[h] & (1 << (7 - w))) != 0 ? frontc.b : backc.b;
          mr += (gliph[h] & (1 << (6 - w))) != 0 ? frontc.r : backc.r;
          mg += (gliph[h] & (1 << (6 - w))) != 0 ? frontc.g : backc.g;
          mb += (gliph[h] & (1 << (6 - w))) != 0 ? frontc.b : backc.b;
          SetPixel(pos + w/2, y + h, (byte)(mr >> 1), (byte)(mg >> 1), (byte)(mb >> 1));
        }
      }
      pos += 3;
      if (pos > wm1) return;
    }
  }

  void Clear(byte col) {
    Color32 pixel = Col.GetColor(col);
    int size = sw * sh * 4;
    for (int i = 0; i < size; i+=4) {
      rawPixels[i + 0] = pixel.r;
      rawPixels[i + 1] = pixel.g;
      rawPixels[i + 2] = pixel.b;
      rawPixels[i + 3] = 255;
    }
    texture.LoadRawTextureData(rawPixels);
  }

  void ClearUI(byte col) {
    uiUpdated = true;
    Color32 pixel = Col.GetColor(col);
    int size = sw * sh * 4;
    for (int i = 0; i < size; i+=4) {
      rawUI[i + 0] = pixel.r;
      rawUI[i + 1] = pixel.g;
      rawUI[i + 2] = pixel.b;
      rawUI[i + 3] = pixel.a;
    }
  }

  void Line(int x1, int y1, int x2, int y2, byte col) {
    int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
    dx = x2 - x1; dy = y2 - y1;

    if (dx == 0) { // Vertical
      if (y2 < y1) { int tmp = y1; y1 = y2; y2 = tmp; }
      for (y = y1; y <= y2; y++) SetPixel(x1, y, col);
      return;
    }

    if (dy == 0) { // Horizontal
      if (x2 < x1) { int tmp = x1; x1 = x2; x2 = tmp; }
      for (x = x1; x <= x2; x++) SetPixel(x, y1, col);
      return;
    }

    // Diagonal
    dx1 = dx;
    if (dx1 < 0) dx1 = -dx1;
    dy1 = dy;
    if (dy1 < 0) dy1 = -dy1;
    px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
    if (dy1 <= dx1) {
      if (dx >= 0) {
        x = x1; y = y1; xe = x2;
      }
      else {
        x = x2; y = y2; xe = x1;
      }

      SetPixel(x, y, col);

      for (i = 0; x < xe; i++) {
        x += 1;
        if (px < 0)
          px += 2 * dy1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y += 1; else y -= 1;
          px += 2 * (dy1 - dx1);
        }
        SetPixel(x, y, col);
      }
    }
    else {
      if (dy >= 0) {
        x = x1; y = y1; ye = y2;
      }
      else {
        x = x2; y = y2; ye = y1;
      }

      SetPixel(x, y, col);

      for (i = 0; y < ye; i++) {
        y += 1;
        if (py <= 0)
          py += 2 * dx1;
        else {
          if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x += 1; else x -= 1;
          py += 2 * (dx1 - dy1);
        }
        SetPixel(x, y, col);
      }
    }
  }

  void Box(int x1, int y1, int x2, int y2, byte col, byte back = 255) {
    if (x1 > x2) { int tmp = x1; x1 = x2; x2 = tmp; }
    if (y1 > y2) { int tmp = y1; y1 = y2; y2 = tmp; }
    for (int x = x1; x <= x2; x++) {
      SetPixel(x, y1, col);
      SetPixel(x, y2, col);
    }
    for (int y = y1; y <= y2; y++) {
      SetPixel(x1, y, col);
      SetPixel(x2, y, col);
    }
    if (back != 255) {
      for (int x = x1 + 1; x < x2; x++) {
        for (int y = y1 + 1; y < y2; y++) {
          SetPixel(x, y, back);
        }
      }
    }
  }

  void Circle(float cx, float cy, float rx, float ry, byte col, byte back = 255) {
    if (rx <= 0 || ry <= 0) return;
    int minx = (int)(cx - rx);
    int maxx = (int)(cx + rx + 1);
    int miny = (int)(cy - ry);
    int maxy = (int)(cy + ry + 1);

    float invrx = 1 / (rx * rx);
    float invry = 1 / (ry * ry);

    for (int x = minx; x < maxx; x++) {
      for (int y = miny; y < maxy; y++) {
        float px = x - cx;
        float py = y - cy;
        float p = (px * px * invrx) + (py * py * invry);

        if (p < 1.02f) {
          if (p > .98f) SetPixel(x, y, col);
          else if (back != 255) SetPixel(x, y, back);
        }

      }
    }
  }

  void Image(int pointer, int px, int py, int w = 0, int h = 0, int startx = 0, int starty = 0) {
    int pos;
    int imw = (mem[pointer] << 8) + mem[pointer + 1];
    int imh = (mem[pointer + 2] << 8) + mem[pointer + 3];
    if (imw < 8 || imh < 8) throw new Exception("Invalid image");
    if (w == 0) w = imw;
    if (h == 0) h = imh;
    for (int y = 0; y < h; y++)
      for (int x = 0; x < w; x++) {
        int dx = x + px;
        int dy = y + py;
        if (dx < 0 || dx >= sw || dy < 0 || dy > sh) continue;

        int sx = startx + x;
        int sy = starty + y;
        if (sx < 0 || sy < 0 || sx >= imw || sy >= imh) continue;

        pos = pointer + 4 + sx + imw * sy;
        byte col = mem[pos];
        if (col != 255) {
          Color32 pixel = Col.GetColor(col);
          pos = (dx + sw * (hm1 - dy)) * 4;
          if (pos < 0 || pos + 4 >= rawTarget.Length) continue;
          rawTarget[pos + 0] = pixel.r;
          rawTarget[pos + 1] = pixel.g;
          rawTarget[pos + 2] = pixel.b;
          rawTarget[pos + 3] = pixel.a;
        }
      }
  }


  #endregion Drawing functions

  #region Sprites ****************************************************************************************************************************************************************************************************

  public GameObject SpriteTemplate;
  public GameObject TilemapTemplate;
  public Transform SpritesFrontLayer;
  public Transform[] Layers;

  void Sprite(int num, int pointer, bool filter = false) {
    if (num < 0 || num > 255) throw new Exception("Invalid sprite number: " + num);
    int sx = (mem[pointer] << 8) + mem[pointer + 1];
    int sy = (mem[pointer + 2] << 8) + mem[pointer + 3];
    if (sprites[num] == null) {
      sprites[num] = Instantiate(SpriteTemplate, Layers[0]).GetComponent<Grob>();
      sprites[num].gameObject.SetActive(true);
      sprites[num].GetComponent<RectTransform>().sizeDelta = Minimized ? new Vector2(.3333333f, .3333333f) : new Vector2(1, 1);
      sprites[num].gameObject.name = "Sprite " + num;
    }
    if (labelTextures.ContainsKey(pointer)) {
      sprites[num].Set(sx, sy, labelTextures[pointer], filter);
    }
    else {
      labelTextures.Add(pointer, sprites[num].Set(sx, sy, mem, pointer + 4, filter));
    }
  }

  void SpritePos(int num, int x, int y, bool enable = true) {
    if (num < 0 || num > 255) throw new Exception("Invalid sprite number: " + num);
    if (sprites[num] == null || sprites[num].notDefined) throw new Exception("Sprite #" + num + " is not defined"); 
    sprites[num].Pos(x, y, scaleW, scaleH, enable);
  }
  
  void SpriteRot(int num, int rot, bool flip) {
    if (num < 0 || num > 255) throw new Exception("Invalid sprite number: " + num);
    if (sprites[num] == null || sprites[num].notDefined) throw new Exception("Sprite #" + num + " is not defined"); 
    rot &= 3;
    sprites[num].Rot(rot, flip);
  }
  
  void SpriteEnable(int num, bool enable) {
    if (num < 0 || num > 255) throw new Exception("Invalid sprite number: " + num);
    if (sprites[num] == null || sprites[num].notDefined) throw new Exception("Sprite #" + num + " is not defined");
    sprites[num].Enable(enable);
  }

  void SpriteTint(int num, byte color) {
    if (num < 0 || num > 255) throw new Exception("Invalid sprite number: " + num);
    if (sprites[num] == null || sprites[num].notDefined) throw new Exception("Sprite #" + num + " is not defined");
    sprites[num].Tint(color);
  }

  void SpriteScale(int num, byte sx, byte sy) {
    if (num < 0 || num > 255) throw new Exception("Invalid sprite number: " + num);
    if (sprites[num] == null || sprites[num].notDefined) throw new Exception("Sprite #" + num + " is not defined");
    sprites[num].Scale(sx, sy);
  }

  void SpritePri(int num, int order) {
    if (order < -1) order = -1;
    if (order > 10) order = 10;
    if (num < 0 || num > 255) throw new Exception("Invalid sprite number: " + num);
    if (sprites[num] == null || sprites[num].notDefined) throw new Exception("Sprite #" + num + " is not defined");

    if (order == -1)
      sprites[num].Parent(SpritesFrontLayer);
    else
      sprites[num].Parent(Layers[order]);
  }

  void ResetArcade() {
    // Remove sprites
    for (int i = 0; i < sprites.Length; i++) {
      Grob g = sprites[i];
      if (g != null) {
        Destroy(g.gameObject);
        sprites[i] = null;
      }
    }
    // Recreate first sprite
    sprites[0] = Instantiate(SpriteTemplate, Layers[0]).GetComponent<Grob>();
    sprites[0].gameObject.name = "Sprite 0";
    sprites[0].gameObject.SetActive(true);
    sprites[0].Set(16, 16, LogoTexture, false);
    sprites[0].Pos(0, 8, scaleW, scaleH, true);
    // Remove tilemaps
    foreach (TMap tm in tilemaps.Values) {
      Destroy(tm.gameObject);
    }
    tilemaps.Clear();
    // Clean up textures
    labelTextures.Clear();
    // Reset audio
    audioManager.Init();
    // Garbage collection
    GC.Collect(4);
  }

  #endregion Sprites

  #region Tilemap ****************************************************************************************************************************************************************************************************
  readonly Dictionary<byte, TMap> tilemaps = new Dictionary<byte, TMap>();

  void Tilemap(byte id, byte order, int start) {
    if (order < 0) order = 0;
    if (order > 12) order = 12;

    // check if we have the tilemap with this ID
    TMap t;
    if (tilemaps.ContainsKey(id)) {
      t = tilemaps[id];
      t.Destroy();
    }
    else {
      t = Instantiate(TilemapTemplate, Layers[order]).GetComponent<TMap>();
      t.gameObject.SetActive(true);
      t.gameObject.name = "Tilemap" + id;
      tilemaps.Add(id, t);
    }
    t.order = order;
    t.Set(mem, start);
    t.rt.localScale = new Vector3(scaleW, scaleH, 1);
  }

  void TilePos(byte id, int scrollx, int scrolly, byte order = 255, bool enabled = true) {
    if (!tilemaps.ContainsKey(id)) throw new Exception("Undefined Tilemap with ID = " + id);
    TMap t = tilemaps[id];
    t.transform.localPosition = new Vector3(scaleW * scrollx, -scaleH * scrolly, 0);
    if (order != 255) {
      if (order < 0) order = 0;
      if (order > 10) order = 10;
      t.transform.SetParent(Layers[order]);
    }
    t.enabled = enabled;
  }

  void TileSet(byte id, int x, int y, byte tile, byte rot = 255) {
    if (!tilemaps.ContainsKey(id)) throw new Exception("Undefined Tilemap with ID = " + id);
    TMap t = tilemaps[id];
    if (x < 0 || y < 0 || x >= t.w || y >= t.h) throw new Exception("Invalid tile (" + x + ", " + y + ") for Tilemap ID = " + id);
    if (rot > 8 && rot != 255) rot = 0;
    t.SetTile(x, y, tile, rot);
  }

  Value TileGet(byte id, byte x, byte y) {
    if (!tilemaps.ContainsKey(id)) throw new Exception("Undefined Tilemap with ID = " + id);
    TMap t = tilemaps[id];
    return new Value(t.GetTile(x, y));
  }

  Value TileGetRot(byte id, byte x, byte y) {
    if (!tilemaps.ContainsKey(id)) throw new Exception("Undefined Tilemap with ID = " + id);
    TMap t = tilemaps[id];
    return new Value(t.GetTileRotation(x, y));
  }

  #endregion Tilemap

  bool Execute(CodeNode n) {
// Debug.Log(n.Format(variables, false));
    CurrentLineNumber = n.origLineNum;
    try {
      switch (n.type) {
        case BNF.CLR: {
          Value tmp = Evaluate(n.CN1);
          Clear(tmp.ToByte(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.UIClr: {
          uiUpdated = true;
          Value tmp = Evaluate(n.CN1);
          ClearUI(tmp.ToByte(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.FRAME: {
          CompleteFrame();
          return true; // We will skip to the next frame
        }

        case BNF.Console: {
          if (n.CN2 == null)
            Console(Evaluate(n.CN1).ToStr());
          else
            Console(Evaluate(n.CN1).ToStr(), Evaluate(n.CN2).ToByte(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.WRITE: {
          Value a = Evaluate(n.CN1);
          Value b = Evaluate(n.CN2);
          Value c = Evaluate(n.CN3);
          Value d = Evaluate(n.CN4);
          if (n.children.Count > 5) {
            Value e = Evaluate(n.CN5);
            Value f = Evaluate(n.CN6);
            Write(a.ToStr(), b.ToInt(culture), c.ToInt(culture), d.ToByte(culture), e.ToByte(culture), f.ToByte(culture));
          }
          else if (n.children.Count > 4) {
            Value e = Evaluate(n.CN5);
            Write(a.ToStr(), b.ToInt(culture), c.ToInt(culture), d.ToByte(culture), e.ToByte(culture));
          }
          else
            Write(a.ToStr(), b.ToInt(culture), c.ToInt(culture), d.ToByte(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.UIWrite: {
          Value a = Evaluate(n.CN1);
          Value b = Evaluate(n.CN2);
          Value c = Evaluate(n.CN3);
          Value d = Evaluate(n.CN4);
          rawTarget = rawUI;
          uiUpdated = true;
          if (n.children.Count > 5) {
            Value e = Evaluate(n.CN5);
            Value f = Evaluate(n.CN6);
            Write(a.ToStr(), b.ToInt(culture), c.ToInt(culture), d.ToByte(culture), e.ToByte(culture), f.ToByte(culture));
          }
          else if (n.children.Count > 4) {
            Value e = Evaluate(n.CN5);
            Write(a.ToStr(), b.ToInt(culture), c.ToInt(culture), d.ToByte(culture), e.ToByte(culture));
          }
          else
            Write(a.ToStr(), b.ToInt(culture), c.ToInt(culture), d.ToByte(culture));
          rawTarget = rawPixels;
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.IncCmd: {
          Value a = Evaluate(n.CN1);
          if (a.IsReg()) variables.Incr(a.idx);
          if (a.IsMem()) mem[a.ToInt(culture)]++;
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.DecCmd: {
          Value a = Evaluate(n.CN1);
          if (a.IsReg()) variables.Decr(a.idx);
          if (a.IsMem()) mem[a.ToInt(culture)]--;
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.ASSIGN:
        case BNF.ASSIGNsum:
        case BNF.ASSIGNsub:
        case BNF.ASSIGNmul:
        case BNF.ASSIGNdiv:
        case BNF.ASSIGNmod:
        case BNF.ASSIGNand:
        case BNF.ASSIGNor:
        case BNF.ASSIGNxor: {
          Value r = Evaluate(n.CN2);
          // Calculate the actual operation on r
          Value l = Evaluate(n.CN1);
          switch(n.type) {
            case BNF.ASSIGNsum: r = l.Sum(r); break;
            case BNF.ASSIGNsub: r = l.Sub(r, culture); break;
            case BNF.ASSIGNmul: r = l.Mul(r, culture); break;
            case BNF.ASSIGNdiv: r = l.Div(r, culture); break;
            case BNF.ASSIGNmod: r = l.Mod(r, culture); break;
            case BNF.ASSIGNand: r = l.And(r, culture); break;
            case BNF.ASSIGNor: r = l.Or(r, culture); break;
            case BNF.ASSIGNxor: r = l.Xor(r, culture); break;
          }

          if (n.CN1.type == BNF.REG) {
            variables.Set(n.CN1.Reg, r);
          }
          else if (n.CN1.type == BNF.ARRAY) {
            variables.Set(l.idx, r);
          }
          else if (n.CN1.type == BNF.MEM) {
            int pos = Evaluate(n.CN1.CN1).ToInt(culture);
            if (pos < 0 || pos > memsize) throw new Exception("Memory violation:" + pos + "\nfrom:" + n);
            switch (n.type) {
              case BNF.ASSIGN: mem[pos] = r.ToByte(culture); break;
              case BNF.ASSIGNsum: mem[pos] += r.ToByte(culture); break;
              case BNF.ASSIGNsub: mem[pos] -= r.ToByte(culture); break;
              case BNF.ASSIGNmul: mem[pos] *= r.ToByte(culture); break;
              case BNF.ASSIGNdiv: mem[pos] /= r.ToByte(culture); break;
              case BNF.ASSIGNmod: mem[pos] %= r.ToByte(culture); break;
              case BNF.ASSIGNand: mem[pos] &= r.ToByte(culture); break;
              case BNF.ASSIGNor: mem[pos] |= r.ToByte(culture); break;
              case BNF.ASSIGNxor: mem[pos] ^= r.ToByte(culture); break;
            }
          }
          else if (n.CN1.type == BNF.MEMlong) {
            int pos = Evaluate(n.CN1.CN1).ToInt(culture);
            if (pos < 0 || pos > memsize) throw new Exception("Memory violation:" + pos + "\nfrom:" + n);

            // Get the value from memory, get the value from registry, do the operation, store it as sequence of bytes
            if (r.type == VT.None) {
              switch (n.type) {
                case BNF.ASSIGN: mem[pos] = 0; break;
                case BNF.ASSIGNmul: mem[pos] = 0; break;
                case BNF.ASSIGNdiv: throw new Exception("Division by zero: " + n);
                case BNF.ASSIGNmod: throw new Exception("Division by zero: " + n);
                case BNF.ASSIGNand: mem[pos] = 0; break;
              }
            }
            else if (r.type == VT.Int) {
              int val = BitConverter.ToInt32(mem, pos);
              switch (n.type) {
                case BNF.ASSIGN: val = r.ToInt(culture); break;
                case BNF.ASSIGNsum: val += r.ToInt(culture); break;
                case BNF.ASSIGNsub: val -= r.ToInt(culture); break;
                case BNF.ASSIGNmul: val *= r.ToInt(culture); break;
                case BNF.ASSIGNdiv: val /= r.ToInt(culture); break;
                case BNF.ASSIGNmod: val %= r.ToInt(culture); break;
                case BNF.ASSIGNand: val &= r.ToInt(culture); break;
                case BNF.ASSIGNor: val |= r.ToInt(culture); break;
                case BNF.ASSIGNxor: val ^= r.ToInt(culture); break;
              }
              byte[] vals = BitConverter.GetBytes(val);
              for (int i = 0; i < vals.Length; i++)
                mem[pos + i] = vals[i];
            }
            else if (r.type == VT.Float) {
              float val = BitConverter.ToSingle(mem, pos);
              switch (n.type) {
                case BNF.ASSIGN: val = r.ToFlt(culture); break;
                case BNF.ASSIGNsum: val += r.ToFlt(culture); break;
                case BNF.ASSIGNsub: val -= r.ToFlt(culture); break;
                case BNF.ASSIGNmul: val *= r.ToFlt(culture); break;
                case BNF.ASSIGNdiv: val /= r.ToFlt(culture); break;
                case BNF.ASSIGNmod: val %= r.ToFlt(culture); break;
              }
              byte[] vals = BitConverter.GetBytes(val);
              for (int i = 0; i < vals.Length; i++)
                mem[pos + i] = vals[i];
            }
            else if (r.type == VT.String) {
              byte[] vals = System.Text.Encoding.UTF8.GetBytes(r.ToStr());
              for (int i = 0; i < vals.Length; i++) {
                mem[pos + i + 2] = vals[i];
              }
              mem[pos] = (byte)(vals.Length >> 8);
              mem[pos + 1] = (byte)(vals.Length & 0xFF);
            }
          }
          else if (n.CN1.type == BNF.MEMlongb) {
            int pos = Evaluate(n.CN1.CN1).ToInt(culture);
            if (pos < 0 || pos > memsize) throw new Exception("Memory violation:" + pos + "\nfrom:" + n);
            switch (n.type) {
              case BNF.ASSIGN: mem[pos] = r.ToByte(culture); break;
              case BNF.ASSIGNsum: mem[pos] += r.ToByte(culture); break;
              case BNF.ASSIGNsub: mem[pos] -= r.ToByte(culture); break;
              case BNF.ASSIGNmul: mem[pos] *= r.ToByte(culture); break;
              case BNF.ASSIGNdiv: mem[pos] /= r.ToByte(culture); break;
              case BNF.ASSIGNmod: mem[pos] %= r.ToByte(culture); break;
              case BNF.ASSIGNand: mem[pos] &= r.ToByte(culture); break;
              case BNF.ASSIGNor: mem[pos] |= r.ToByte(culture); break;
              case BNF.ASSIGNxor: mem[pos] ^= r.ToByte(culture); break;
            }
          }
          else if (n.CN1.type == BNF.MEMlongi) {
            int pos = Evaluate(n.CN1.CN1).ToInt(culture);
            if (pos < 0 || pos > memsize) throw new Exception("Memory violation:" + pos + "\nfrom:" + n);
            int val = BitConverter.ToInt32(mem, pos);
            switch (n.type) {
              case BNF.ASSIGN: val = r.ToInt(culture); break;
              case BNF.ASSIGNsum: val += r.ToInt(culture); break;
              case BNF.ASSIGNsub: val -= r.ToInt(culture); break;
              case BNF.ASSIGNmul: val *= r.ToInt(culture); break;
              case BNF.ASSIGNdiv: val /= r.ToInt(culture); break;
              case BNF.ASSIGNmod: val %= r.ToInt(culture); break;
              case BNF.ASSIGNand: val &= r.ToInt(culture); break;
              case BNF.ASSIGNor: val |= r.ToInt(culture); break;
              case BNF.ASSIGNxor: val ^= r.ToInt(culture); break;
            }
            byte[] vals = BitConverter.GetBytes(val);
            for (int i = 0; i < vals.Length; i++)
              mem[pos + i] = vals[i];
          }
          else if (n.CN1.type == BNF.MEMlongf) {
            int pos = Evaluate(n.CN1.CN1).ToInt(culture);
            if (pos < 0 || pos > memsize) throw new Exception("Memory violation:" + pos + "\nfrom:" + n);
            float val = BitConverter.ToSingle(mem, pos);
            switch (n.type) {
              case BNF.ASSIGN: val = r.ToFlt(culture); break;
              case BNF.ASSIGNsum: val += r.ToFlt(culture); break;
              case BNF.ASSIGNsub: val -= r.ToFlt(culture); break;
              case BNF.ASSIGNmul: val *= r.ToFlt(culture); break;
              case BNF.ASSIGNdiv: val /= r.ToFlt(culture); break;
              case BNF.ASSIGNmod: val %= r.ToFlt(culture); break;
            }
            byte[] vals = BitConverter.GetBytes(val);
            for (int i = 0; i < vals.Length; i++)
              mem[pos + i] = vals[i];
          }
          else if (n.CN1.type == BNF.MEMlongs) {
            int pos = Evaluate(n.CN1.CN1).ToInt(culture);
            if (pos < 0 || pos > memsize) throw new Exception("Memory violation:" + pos + "\nfrom:" + n);
            byte[] vals = System.Text.Encoding.UTF8.GetBytes(r.ToStr());
            for (int i = 0; i < vals.Length; i++) {
              mem[pos + i + 2] = vals[i];
            }
            mem[pos] = (byte)(vals.Length >> 8);
            mem[pos + 1] = (byte)(vals.Length & 0xFF);
          }
          else if (n.CN1.type == BNF.MEMchar) {
            int pos = Evaluate(n.CN1.CN1).ToInt(culture);
            if (pos < 0 || pos > memsize) throw new Exception("Memory violation:" + pos + "\nfrom:" + n);
            byte[] vals = System.Text.Encoding.UTF8.GetBytes(r.ToStr());
            mem[pos] = vals[0];
          }
        
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.SETP: {
          Value x = Evaluate(n.CN1);
          Value y = Evaluate(n.CN2);
          Value c = Evaluate(n.CN3);
          SetPixel(x.ToInt(culture), y.ToInt(culture), c.ToByte(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.LINE: {
          Value x1 = Evaluate(n.CN1);
          Value y1 = Evaluate(n.CN2);
          Value x2 = Evaluate(n.CN3);
          Value y2 = Evaluate(n.CN4);
          Value col = Evaluate(n.CN5);
          Line(x1.ToInt(culture), y1.ToInt(culture), x2.ToInt(culture), y2.ToInt(culture), col.ToByte(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.UILine: {
          Value x1 = Evaluate(n.CN1);
          Value y1 = Evaluate(n.CN2);
          Value x2 = Evaluate(n.CN3);
          Value y2 = Evaluate(n.CN4);
          Value col = Evaluate(n.CN5);
          rawTarget = rawUI;
          uiUpdated = true;
          Line(x1.ToInt(culture), y1.ToInt(culture), x2.ToInt(culture), y2.ToInt(culture), col.ToByte(culture));
          rawTarget = rawPixels;
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.BOX: {
          Value x1 = Evaluate(n.CN1);
          Value y1 = Evaluate(n.CN2);
          Value x2 = Evaluate(n.CN3);
          Value y2 = Evaluate(n.CN4);
          Value col = Evaluate(n.CN5);
          if (n.children.Count > 5) {
            Value back = Evaluate(n.CN6);
            Box(x1.ToInt(culture), y1.ToInt(culture), x2.ToInt(culture), y2.ToInt(culture), col.ToByte(culture), back.ToByte(culture));
          }
          else
            Box(x1.ToInt(culture), y1.ToInt(culture), x2.ToInt(culture), y2.ToInt(culture), col.ToByte(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.UIBox: {
          Value x1 = Evaluate(n.CN1);
          Value y1 = Evaluate(n.CN2);
          Value x2 = Evaluate(n.CN3);
          Value y2 = Evaluate(n.CN4);
          Value col = Evaluate(n.CN5);
          rawTarget = rawUI;
          uiUpdated = true;
          if (n.children.Count > 5) {
            Value back = Evaluate(n.CN6);
            Box(x1.ToInt(culture), y1.ToInt(culture), x2.ToInt(culture), y2.ToInt(culture), col.ToByte(culture), back.ToByte(culture));
          }
          else
            Box(x1.ToInt(culture), y1.ToInt(culture), x2.ToInt(culture), y2.ToInt(culture), col.ToByte(culture));
          rawTarget = rawPixels;
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.CIRCLE: {
          Value cx = Evaluate(n.CN1);
          Value cy = Evaluate(n.CN2);
          Value rx = Evaluate(n.CN3);
          Value ry = Evaluate(n.CN4);
          Value col = Evaluate(n.CN5);
          if (n.children.Count > 5) {
            Value back = Evaluate(n.CN6);
            Circle(cx.ToFlt(culture), cy.ToFlt(culture), rx.ToFlt(culture), ry.ToFlt(culture), col.ToByte(culture), back.ToByte(culture));
          }
          else
            Circle(cx.ToFlt(culture), cy.ToFlt(culture), rx.ToFlt(culture), ry.ToFlt(culture), col.ToByte(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.IMAGE: {
          Value addr = Evaluate(n.CN1);
          Value px = Evaluate(n.CN2);
          Value py = Evaluate(n.CN3);
          if (n.children.Count == 7) {
            Value w = Evaluate(n.CN4);
            Value h = Evaluate(n.CN5);
            Value startx = Evaluate(n.CN6);
            Value starty = Evaluate(n.CN7);
            Image(addr.ToInt(culture), px.ToInt(culture), py.ToInt(culture), w.ToInt(culture), h.ToInt(culture), startx.ToInt(culture), starty.ToInt(culture));
          }
          else
            Image(addr.ToInt(culture), px.ToInt(culture), py.ToInt(culture));
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.UIImage: {
          Value addr = Evaluate(n.CN1);
          Value px = Evaluate(n.CN2);
          Value py = Evaluate(n.CN3);
          rawTarget = rawUI;
          uiUpdated = true;
          if (n.children.Count == 7) {
            Value w = Evaluate(n.CN4);
            Value h = Evaluate(n.CN5);
            Value startx = Evaluate(n.CN6);
            Value starty = Evaluate(n.CN7);
            Image(addr.ToInt(culture), px.ToInt(culture), py.ToInt(culture), w.ToInt(culture), h.ToInt(culture), startx.ToInt(culture), starty.ToInt(culture));
          }
          else
            Image(addr.ToInt(culture), px.ToInt(culture), py.ToInt(culture));
          rawTarget = rawPixels;
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.IF: {
          if (Evaluate(n.CN1).ToBool(culture)) {
            if (n.CN2.type != BNF.BLOCK || (n.CN2.children != null && n.CN2.children.Count > 0))
              stacks.AddStack(n.CN2, null, n.origLine, n.origLineNum);
            HandlePostIncrementDecrement();
            return false;
          }
          else if (n.children.Count > 2) {
            if (n.CN3.type != BNF.BLOCK || (n.CN3.children != null && n.CN3.children.Count > 0))
              stacks.AddStack(n.CN3, null, n.origLine, n.origLineNum);
            HandlePostIncrementDecrement();
            return false;
          }
        }
        break;

        case BNF.WHILE: {
          Value cond = Evaluate(n.CN1);
          if (cond.ToInt(culture) != 0) {
            stacks.AddStack(n.CN2, n.CN1, n.origLine, n.origLineNum);
            HandlePostIncrementDecrement();
            return false;
          }
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.FOR: {
          Execute(n.CN1);
          Value cond = Evaluate(n.CN2);
          if (cond.ToInt(culture) == 0) {
            HandlePostIncrementDecrement();
            return false;
          }
          CodeNode forBlock = new CodeNode(n.CN4, n.CN3);
          stacks.AddStack(forBlock, n.CN2, n.origLine, n.origLineNum);
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.WAIT: {
          toWait = Evaluate(n.CN1).ToFlt(culture);
          if (toWait > 0 && n.sVal == "*") CompleteFrame();
          HandlePostIncrementDecrement();
          return toWait > 0;
        }

        case BNF.SCREEN: {
          sw = Evaluate(n.CN1).ToInt(culture);
          if (sw < 128) sw = 128;
          if (sw > 320) sw = 320;
          sh = Evaluate(n.CN2).ToInt(culture);
          if (sh < 100) sh = 100;
          if (sh > 256) sh = 256;
          wm1 = sw - 1;
          hm1 = sh - 1;
          scaleW = rt.rect.width / sw;
          scaleH = rt.rect.height / sh;
          texture = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = n.CN3 == null || Evaluate(n.CN3).ToInt(culture) == 0 ? FilterMode.Point : FilterMode.Bilinear };
          Screen.texture = texture;
          rawPixels = texture.GetRawTextureData();
          rawTarget = rawPixels;
          textureUI = new Texture2D(sw, sh, TextureFormat.RGBA32, false) { filterMode = n.CN3 == null || Evaluate(n.CN3).ToInt(culture) == 0 ? FilterMode.Point : FilterMode.Bilinear };
          UI.texture = textureUI;
          rawUI = textureUI.GetRawTextureData();
          HandlePostIncrementDecrement();
        }
        break;

        case BNF.SPRITE: {
          Sprite(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN3).ToBool(culture));
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.DESTROY: {
          int pointer = Evaluate(n.CN1).ToInt(culture);
          if (labelTextures.ContainsKey(pointer)) labelTextures.Remove(pointer);
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.SPOS: SpritePos(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN3).ToInt(culture), n.CN4 == null || Evaluate(n.CN4).ToBool(culture)); HandlePostIncrementDecrement();  return false;

        case BNF.SROT: SpriteRot(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN3).ToBool(culture)); HandlePostIncrementDecrement(); return false;

        case BNF.SPEN: SpriteEnable(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToBool(culture)); HandlePostIncrementDecrement(); return false;

        case BNF.SPRI: SpritePri(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToByte(culture)); HandlePostIncrementDecrement(); return false;

        case BNF.STINT: SpriteTint(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToByte(culture)); HandlePostIncrementDecrement(); return false;

        case BNF.SSCALE: SpriteScale(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture)); HandlePostIncrementDecrement(); return false;

        case BNF.RETURN: return stacks.PopUp(); // Return is not called as expression, just end the stack

        case BNF.FunctionCall: {
          // Evaluate all parameters and set them
          // Get the function code, run it as stack, no need to collect the final result (it is not called as expression)
          CodeNode fDef = functions[n.sVal];
          // Set to NULL all the local variables
          variables.NullifyFunction(fDef.sVal);
          if (fDef.CN1?.children != null) {
            // Evaluate the parameters
            for (int i = 0; i < fDef.CN1.children.Count; i++) {
              CodeNode par = fDef.CN1.children[i];
              CodeNode val = n.CN1.children[i];
              Value v = Evaluate(val);
              variables.Set(par.Reg, v);
            }
          }
          HandlePostIncrementDecrement(); 
          stacks.AddStack(fDef.CN2, null, n.origLine, n.origLineNum);
          return false;
        }

        case BNF.SOUND: {
          if (n.CN3 == null)
            audioManager.Play(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToInt(culture));
          else
            audioManager.Play(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN3).ToFlt(culture));
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.WAVE: {
          if (n.children.Count == 2) {
            audioManager.Wave(Evaluate(n.CN1).ToInt(culture), mem, Evaluate(n.CN2).ToInt(culture));
          }
          else {
            int c = Evaluate(n.CN1).ToInt(culture);
            audioManager.Wave(c, (Waveform)Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN3).ToFlt(culture));
            audioManager.ADSR(c,
                                  Evaluate(n.CN3).ToByte(culture),
                                  Evaluate(n.CN4).ToByte(culture),
                                  Evaluate(n.CN5).ToByte(culture),
                                  Evaluate(n.CN6).ToByte(culture));
          }
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.MUTE: {
          audioManager.Stop(Evaluate(n.CN1).ToInt(culture));
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.VOLUME: {
          if (n.CN2 == null) {
            float vol = Evaluate(n.CN1).ToFlt(culture);
            if (vol < 0) vol = 0;
            if (vol > 1) vol = 1;
            for (int i = 0; i < 8; i++)
              audioManager.Volume(i, vol);
          }
          else {
            float vol = Evaluate(n.CN2).ToFlt(culture);
            if (vol < 0) vol = 0;
            if (vol > 1) vol = 1;
            audioManager.Volume(Evaluate(n.CN1).ToInt(culture), vol);
          }
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.PITCH: {
          float pitch = Evaluate(n.CN2).ToFlt(culture);
          if (pitch < 0) pitch = 0;
          if (pitch > 100) pitch = 100;
          audioManager.Pitch(Evaluate(n.CN1).ToInt(culture), pitch);
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.PAN: {
          float pan = Evaluate(n.CN2).ToFlt(culture);
          if (pan < -1) pan = -1;
          if (pan > 1) pan = 1;
          audioManager.Pan(Evaluate(n.CN1).ToInt(culture), pan);
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.MUSICLOAD: {
          audioManager.LoadMusic(mem, Evaluate(n.CN1).ToInt(culture));
          HandlePostIncrementDecrement();
          return false;
        }
        case BNF.MUSICVOICES: {
          if (n.children.Count == 8) audioManager.MusicVoices(
            Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture),
            Evaluate(n.CN4).ToByte(culture), Evaluate(n.CN5).ToByte(culture), Evaluate(n.CN6).ToByte(culture),
            Evaluate(n.CN7).ToByte(culture), Evaluate(n.CN8).ToByte(culture));
          if (n.children.Count == 7) audioManager.MusicVoices(
            Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture),
            Evaluate(n.CN4).ToByte(culture), Evaluate(n.CN5).ToByte(culture), Evaluate(n.CN6).ToByte(culture),
            Evaluate(n.CN7).ToByte(culture));
          if (n.children.Count == 6) audioManager.MusicVoices(
            Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture),
            Evaluate(n.CN4).ToByte(culture), Evaluate(n.CN5).ToByte(culture), Evaluate(n.CN6).ToByte(culture));
          if (n.children.Count == 5) audioManager.MusicVoices(
            Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture),
            Evaluate(n.CN4).ToByte(culture), Evaluate(n.CN5).ToByte(culture));
          if (n.children.Count == 4) audioManager.MusicVoices(
            Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture),
            Evaluate(n.CN4).ToByte(culture));
          if (n.children.Count == 3) audioManager.MusicVoices(
            Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture));
          if (n.children.Count == 2) audioManager.MusicVoices(
            Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture));
          if (n.children.Count == 1) audioManager.MusicVoices(Evaluate(n.CN1).ToByte(culture));
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.MUSICPLAY: {
          if (n.CN1 == null) audioManager.PlayMusic();
          else audioManager.PlayMusic(Evaluate(n.CN1).ToInt(culture));
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.MUSICSTOP: {
          audioManager.StopMusic();
          return false;
        }

        case BNF.TILEMAP: {
          Tilemap(Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToInt(culture));
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.TILEPOS: {
          if (n.CN4 == null)
            TilePos(Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN3).ToInt(culture));
          else if (n.CN5 == null)
            TilePos(Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN4).ToByte(culture));
          else
            TilePos(Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN2).ToInt(culture), Evaluate(n.CN4).ToByte(culture), Evaluate(n.CN5).ToBool(culture));
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.TILESET: {
          if (n.CN5 == null)
            TileSet(Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture), Evaluate(n.CN4).ToByte(culture));
          else
            TilePos(Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture), Evaluate(n.CN4).ToByte(culture), Evaluate(n.CN5).ToBool(culture));
          HandlePostIncrementDecrement();
          return false;
        }

        case BNF.USEPALETTE: {
          bool use = Evaluate(n.CN1).ToBool(culture);
          Col.UsePalette(use);
          RGEPalette.SetInt("_UsePalette", use ? 1 : 0);
          HandlePostIncrementDecrement();
          return false; 
        }
        case BNF.SETPALETTECOLOR: {
          if (n.children.Count == 1) Col.SetPalette(mem, Evaluate(n.CN1).ToInt(culture), 0);
          if (n.children.Count == 2) Col.SetPalette(mem, Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToInt(culture));
          if (n.children.Count == 4)
            Col.SetPalette(Evaluate(n.CN1).ToByte(culture),
                           Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture),
                           Evaluate(n.CN4).ToByte(culture), 255);
          else
            Col.SetPalette(Evaluate(n.CN1).ToByte(culture), 
                          Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture), 
                          Evaluate(n.CN4).ToByte(culture), Evaluate(n.CN5).ToByte(culture));
          HandlePostIncrementDecrement();
          return false; 
        }

        case BNF.LUMA: { Luma(Evaluate(n.CN1).ToFlt(culture)); HandlePostIncrementDecrement(); return false; }
        case BNF.CONTRAST: { Contrast(Evaluate(n.CN1).ToFlt(culture)); HandlePostIncrementDecrement(); return false; }

        case BNF.MEMCPY: {
          int dst = Evaluate(n.CN1).ToInt(culture);
          int src = Evaluate(n.CN2).ToInt(culture);
          int len = Evaluate(n.CN3).ToInt(culture);
          for (int i = 0; i < len; i++) {
            int sp = src + i;
            int dp = dst + i;
            if (sp < 0 || sp >= mem.Length || dp < 0 || dp >= mem.Length) continue;
            mem[dp] = mem[sp];
          }
          HandlePostIncrementDecrement(); 
          return false;
        }

        case BNF.NOP: return false;

        default: {
          runStatus = RunStatus.Error;
          Clear(Col.C(1, 0, 0));
          Write("Not handled code:\n " + n.type + "\n" + n, 2, 2, Col.C(5, 5, 0));
          LastErrorMessage = "Not handled code:\n " + n.type;
          updateDelay = -1;
          stacks.Destroy();
          CompleteFrame();
        }
        break;
      }
    } catch (Exception e) {
      runStatus = RunStatus.Error;
      LastErrorMessage = e.Message + "\nLine: " + CurrentLineNumber;
      Clear(Col.C(5, 0, 0));
      Debug.Log(e.Message + "\n" + e.StackTrace);
      string msg = "";
      for (int i = 0, l = 0; i < e.Message.Length; i++) {
        char c = e.Message[i];
        if (c == '\n') l = 0;
        msg += c;
        l++;
        if (l == sw / 8 - 1) {
          msg += "\n";
          l = 0;
        }
      }
      Write(msg, 2, 2, 0);
      updateDelay = -1;
      stacks.Destroy();
      execCallback?.Invoke(nodeToRun == null ? CurrentLineNumber : nodeToRun.origLineNum);
      CompleteFrame();
    }
    return false;
  }
  
  readonly Value[] valsToPostIncrement = new Value[32];
  int numValsToPostIncrement = 0;
  readonly Value[] valsToPostDecrement = new Value[32];
  int numValsToPostDecrement = 0;

  void HandlePostIncrementDecrement() {
    for (int i = 0; i < numValsToPostIncrement; i++) {
      Value v = valsToPostIncrement[i];
      if (v.IsReg()) variables.Incr(v.idx);
      if (v.IsMem()) mem[v.ToInt(culture)]++;
    }
    numValsToPostIncrement = 0;
    for (int i = 0; i < numValsToPostDecrement; i++) {
      Value v = valsToPostDecrement[i];
      if (v.IsReg()) variables.Incr(v.idx);
      if (v.IsMem()) mem[v.ToInt(culture)]++;
    }
    numValsToPostDecrement = 0;
  }

  internal Value Evaluate(CodeNode n) {
    if (n == null) return new Value();
    if (!n.Evaluable()) throw new Exception("Not evaluable node: " + n);

    switch (n.type) {
      case BNF.REG: {
        Value r = variables.Get(n.Reg);
        if (r.type == VT.Array) {
          int pos = Evaluate(n.CN1).ToInt(culture);
          return r.GetArrayValue(variables, pos);
        }
        return r;
      }

      case BNF.MEM:
      case BNF.MEMlongb: {
        int pos = Evaluate(n.CN1).ToInt(culture);
        if (pos < 0 || pos > mem.Length) throw new Exception("Memory violation:" + pos + "\nfrom:" + n);
        return new Value((int)mem[pos]);
      }

      case BNF.MEMlong:
      case BNF.MEMlongi: {
        int pos = Evaluate(n.CN1).ToInt(culture);
        return new Value(BitConverter.ToInt32(mem, pos));
      }

      case BNF.MEMlongf: {
        int pos = Evaluate(n.CN1).ToInt(culture);
        return new Value(BitConverter.ToSingle(mem, pos));
      }

      case BNF.MEMlongs: {
        int pos = Evaluate(n.CN1).ToInt(culture);
        int len = (mem[pos] << 80) + mem[pos + 1];
        return new Value(System.Text.Encoding.UTF8.GetString(mem, pos+2, len));
      }

      case BNF.MEMchar: {
        int pos = Evaluate(n.CN1).ToInt(culture);
        return new Value(System.Text.Encoding.UTF8.GetString(mem, pos, 1));
      }

      case BNF.INT: return new Value(n.iVal);
      case BNF.FLT: return new Value(n.fVal);
      case BNF.COLOR: return new Value(n.iVal);
      case BNF.STR: return new Value(n.sVal);

      case BNF.ARRAY: {
        Value a = variables.Get(n.Reg);
        if (a.type != VT.Array) {
          // Not an array, make it as array with at least the number of values specified, and return the value
          a.ConvertToArray(variables, n.Reg, Evaluate(n.CN1).ToInt(culture));
        }
        return a.GetArrayValue(variables, Evaluate(n.CN1).ToInt(culture));
      }


      case BNF.MUSICPOS: return new Value(audioManager.GetMusicPos());
      case BNF.DTIME: return new Value(Time.deltaTime);
      case BNF.OPpar: return Evaluate(n.CN1);
      case BNF.OPsum: {
        Value v = Evaluate(n.CN1).Sum(Evaluate(n.CN2));
        return v;
      }
      case BNF.OPsub: return Evaluate(n.CN1).Sub(Evaluate(n.CN2), culture);
      case BNF.OPmul: return Evaluate(n.CN1).Mul(Evaluate(n.CN2), culture);
      case BNF.OPdiv: return Evaluate(n.CN1).Div(Evaluate(n.CN2), culture);
      case BNF.OPmod: return Evaluate(n.CN1).Mod(Evaluate(n.CN2), culture);
      case BNF.OPland: return Evaluate(n.CN1).And(Evaluate(n.CN2), culture);
      case BNF.OPlor: return Evaluate(n.CN1).Or(Evaluate(n.CN2), culture);
      case BNF.OPand: return Evaluate(n.CN1).And(Evaluate(n.CN2), culture);
      case BNF.OPor: return Evaluate(n.CN1).Or(Evaluate(n.CN2), culture);
      case BNF.OPxor: return Evaluate(n.CN1).Xor(Evaluate(n.CN2), culture);
      case BNF.OPlsh: return Evaluate(n.CN1).Lsh(Evaluate(n.CN2), culture);
      case BNF.OPrsh: return Evaluate(n.CN1).Rsh(Evaluate(n.CN2), culture);

      case BNF.GETP: return new Value(GetPixel(Evaluate(n.CN1).ToInt(culture), Evaluate(n.CN2).ToInt(culture)));

      case BNF.LEN: 
        return new Value(Evaluate(n.CN1).ToStr().Length);

      case BNF.PLEN: 
        return new Value(System.Text.Encoding.UTF8.GetByteCount(Evaluate(n.CN1).ToStr()) + 2);

      case BNF.UOsub: return Evaluate(n.CN1).Sub(culture);
      case BNF.UOinv: return Evaluate(n.CN1).Inv();
      case BNF.UOneg: return Evaluate(n.CN1).Neg();

      case BNF.COMPeq:
      case BNF.COMPne:
      case BNF.COMPgt:
      case BNF.COMPge:
      case BNF.COMPlt:
      case BNF.COMPle:
        return new Value(Evaluate(n.CN1).Compare(Evaluate(n.CN2), n.type, culture));

      case BNF.CASTb: return new Value(Evaluate(n.CN1).ToByte(culture));
      case BNF.CASTi: return new Value(Evaluate(n.CN1).ToInt(culture));
      case BNF.CASTf: return new Value(Evaluate(n.CN1).ToFlt(culture));
      case BNF.CASTs: return new Value(Evaluate(n.CN1).ToStr());

      case BNF.KEY: return new Value(inputs[n.iVal] ? -1 : 0);
      case BNF.KEYx: return new Value(Input.GetAxis("Horixontal"));
      case BNF.KEYy: return new Value(Input.GetAxis("Vertical"));

      case BNF.Label: {
        if (!labels.ContainsKey(n.sVal)) throw new Exception("Undefined Label: " + n.sVal);
        return new Value(labels[n.sVal]);
      }

      case BNF.LABG: {
        string lab = Evaluate(n.CN1).ToStr().ToLowerInvariant();
        if (!labels.ContainsKey(lab)) throw new Exception("Undefined Label: " + lab);
        return new Value(labels[lab]);
      }

      case BNF.SIN: return new Value(Mathf.Sin(Evaluate(n.CN1).ToFlt(culture)));
      case BNF.COS: return new Value(Mathf.Cos(Evaluate(n.CN1).ToFlt(culture)));
      case BNF.TAN: return new Value(Mathf.Tan(Evaluate(n.CN1).ToFlt(culture)));
      case BNF.ATAN2: return new Value(Mathf.Atan2(Evaluate(n.CN1).ToFlt(culture), Evaluate(n.CN2).ToFlt(culture)));
      case BNF.SQR: return new Value(Mathf.Sqrt(Evaluate(n.CN1).ToFlt(culture)));
      case BNF.POW: return new Value(Mathf.Pow(Evaluate(n.CN1).ToFlt(culture), Evaluate(n.CN2).ToFlt(culture)));

      case BNF.PERLIN: {
        if (n.CN2 == null) {
          float x = Evaluate(n.CN1).ToFlt(culture);
          return new Value((float)(1 + NoiseS3D.NoiseCombinedOctaves(x)) * .5f);
        }
        else if (n.CN3 == null) {
          float x = Evaluate(n.CN1).ToFlt(culture);
          float y = Evaluate(n.CN2).ToFlt(culture);
          return new Value((float)(1 + NoiseS3D.NoiseCombinedOctaves(x, y)) * .5f);
        }
        else {
          float x = Evaluate(n.CN1).ToFlt(culture);
          float y = Evaluate(n.CN2).ToFlt(culture);
          float z = Evaluate(n.CN3).ToFlt(culture);
          return new Value((float)(1 + NoiseS3D.NoiseCombinedOctaves(x, y, z)) * .5f);
        }
      }

      case BNF.SUBSTRING: {
        string s = Evaluate(n.CN1).ToStr();
        int start = Evaluate(n.CN2).ToInt(culture);
        if (start < 0) start = 0;
        if (start >= s.Length) start = s.Length - 1;
        if (n.CN3 == null)
          return new Value(s.Substring(start));
        else {
          int end = Evaluate(n.CN3).ToInt(culture);
          if (start + end > s.Length) end = s.Length - start;
          if (end < 0) end = 0;
          return new Value(s.Substring(start, end));
        }
      }

      case BNF.TILEGET: return TileGet(Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture));
      case BNF.TILEGETROT: return TileGetRot(Evaluate(n.CN1).ToByte(culture), Evaluate(n.CN2).ToByte(culture), Evaluate(n.CN3).ToByte(culture));

      case BNF.TRIM: return new Value(Evaluate(n.CN1).ToStr().Trim());

      case BNF.IncExp: {
        Value v = new Value(Evaluate(n.CN1).ToStr().Trim());
        valsToPostIncrement[numValsToPostIncrement] = v;
        numValsToPostIncrement++;
        return v;
      }
      case BNF.DecExp: {
        Value v = new Value(Evaluate(n.CN1).ToStr().Trim());
        valsToPostDecrement[numValsToPostDecrement] = v;
        numValsToPostDecrement++;
        return v;
      }

      case BNF.FunctionCall: {
        // Evaluate all parameters, assign all values to the registers, run the statements like a stack, return the value from a "return" (or 0 if there is no return)
        CodeNode fDef = functions[n.sVal];
        if (fDef.CN1?.children != null) {
          // Evaluate the parameters
          for (int i = 0; i < fDef.CN1.children.Count; i++) {
            CodeNode par = fDef.CN1.children[i];
            CodeNode val = n.CN1.children[i];
            Value v = Evaluate(val);
            variables.Set(par.Reg, v);
          }
        }

        stacks.AddStack(fDef.CN2, null, fDef.origLine, fDef.origLineNum);

        int numruns = 0;
        CodeNode sn = stacks.GetExecutionNode(this);
        while (sn != null) {
          if (sn.type == BNF.RETURN) return Evaluate(sn);
          Execute(sn);
          numruns++;
          if (numruns > 1024 * 256) {
            Write("Possible infinite loop at: " + sn.parent.origLineNum + "\n" + sn.parent.origLine, 4, 4, 48, 0);
            CompleteFrame();
            return new Value();
          }
          sn = stacks.GetExecutionNode(this);
        }
        return new Value(0);
      }

      case BNF.RETURN: {
        if (n.CN1 == null) return new Value(0);
        Debug.Log("returning: " + n.CN1 + " = " + Evaluate(n.CN1).ToStr());
        return Evaluate(n.CN1);
      }
    }
    throw new Exception("Invalid node to evaluate: " + n.type);
  }

  readonly Dictionary<char, byte[]> font6 = new Dictionary<char, byte[]>() {
{' ', new byte[]{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00} },
{'a', new byte[]{0x00,0x00,0x38,0x04,0x3c,0x44,0x3c,0x00} },
{'b', new byte[]{0x20,0x20,0x38,0x24,0x24,0x24,0x38,0x00} },
{'c', new byte[]{0x00,0x00,0x38,0x44,0x40,0x44,0x38,0x00} },
{'d', new byte[]{0x04,0x04,0x3c,0x44,0x44,0x44,0x3c,0x00} },
{'e', new byte[]{0x00,0x00,0x38,0x44,0x7c,0x40,0x3c,0x00} },
{'f', new byte[]{0x18,0x24,0x20,0x70,0x20,0x20,0x20,0x00} },
{'g', new byte[]{0x00,0x00,0x3c,0x44,0x44,0x3c,0x04,0x78} },
{'h', new byte[]{0x40,0x40,0x78,0x44,0x44,0x44,0x44,0x00} },
{'i', new byte[]{0x10,0x00,0x10,0x10,0x10,0x10,0x10,0x00} },
{'j', new byte[]{0x04,0x00,0x04,0x04,0x04,0x44,0x44,0x38} },
{'k', new byte[]{0x40,0x40,0x4c,0x50,0x60,0x70,0x4c,0x00} },
{'l', new byte[]{0x70,0x10,0x10,0x10,0x10,0x10,0x78,0x00} },
{'m', new byte[]{0x00,0x00,0x44,0x6c,0x7c,0x54,0x44,0x00} },
{'n', new byte[]{0x00,0x00,0x78,0x44,0x44,0x44,0x44,0x00} },
{'o', new byte[]{0x00,0x00,0x38,0x44,0x44,0x44,0x38,0x00} },
{'p', new byte[]{0x00,0x00,0x78,0x44,0x44,0x78,0x40,0x40} },
{'q', new byte[]{0x00,0x00,0x3c,0x44,0x44,0x3c,0x04,0x04} },
{'r', new byte[]{0x00,0x00,0x78,0x44,0x40,0x40,0x40,0x00} },
{'s', new byte[]{0x00,0x00,0x3c,0x40,0x38,0x04,0x78,0x00} },
{'t', new byte[]{0x20,0x20,0x7c,0x20,0x20,0x24,0x18,0x00} },
{'u', new byte[]{0x00,0x00,0x44,0x44,0x44,0x44,0x38,0x00} },
{'v', new byte[]{0x00,0x00,0x44,0x44,0x6c,0x38,0x10,0x00} },
{'w', new byte[]{0x00,0x00,0x44,0x54,0x7c,0x6c,0x44,0x00} },
{'x', new byte[]{0x00,0x00,0x6c,0x38,0x10,0x38,0x6c,0x00} },
{'y', new byte[]{0x00,0x00,0x44,0x44,0x44,0x3c,0x04,0x78} },
{'z', new byte[]{0x00,0x00,0x7c,0x08,0x10,0x20,0x7c,0x00} },
{'A', new byte[]{0x10,0x38,0x44,0x7c,0x44,0x44,0x44,0x00} },
{'B', new byte[]{0x78,0x44,0x44,0x78,0x44,0x44,0x78,0x00} },
{'C', new byte[]{0x38,0x44,0x40,0x40,0x40,0x44,0x38,0x00} },
{'D', new byte[]{0x70,0x48,0x44,0x44,0x44,0x48,0x70,0x00} },
{'E', new byte[]{0x7c,0x40,0x40,0x70,0x40,0x40,0x7c,0x00} },
{'F', new byte[]{0x7c,0x40,0x40,0x78,0x40,0x40,0x40,0x00} },
{'G', new byte[]{0x38,0x44,0x40,0x5c,0x44,0x44,0x38,0x00} },
{'H', new byte[]{0x44,0x44,0x44,0x7c,0x44,0x44,0x44,0x00} },
{'I', new byte[]{0x38,0x10,0x10,0x10,0x10,0x10,0x38,0x00} },
{'J', new byte[]{0x1c,0x08,0x08,0x08,0x08,0x48,0x30,0x00} },
{'K', new byte[]{0x4c,0x58,0x70,0x60,0x70,0x58,0x4c,0x00} },
{'L', new byte[]{0x40,0x40,0x40,0x40,0x40,0x40,0x7c,0x00} },
{'M', new byte[]{0x44,0x6c,0x7c,0x54,0x44,0x44,0x44,0x00} },
{'N', new byte[]{0x44,0x64,0x74,0x7c,0x5c,0x4c,0x44,0x00} },
{'O', new byte[]{0x38,0x44,0x44,0x44,0x44,0x44,0x38,0x00} },
{'P', new byte[]{0x78,0x44,0x44,0x78,0x40,0x40,0x40,0x00} },
{'Q', new byte[]{0x38,0x44,0x44,0x44,0x44,0x38,0x1c,0x00} },
{'R', new byte[]{0x78,0x44,0x44,0x78,0x70,0x58,0x4c,0x00} },
{'S', new byte[]{0x38,0x4c,0x40,0x38,0x04,0x64,0x38,0x00} },
{'T', new byte[]{0x7c,0x10,0x10,0x10,0x10,0x10,0x10,0x00} },
{'U', new byte[]{0x44,0x44,0x44,0x44,0x44,0x44,0x38,0x00} },
{'V', new byte[]{0x44,0x44,0x44,0x44,0x28,0x38,0x10,0x00} },
{'W', new byte[]{0x44,0x44,0x44,0x54,0x7c,0x6c,0x44,0x00} },
{'X', new byte[]{0x44,0x6c,0x38,0x10,0x38,0x6c,0x44,0x00} },
{'Y', new byte[]{0x44,0x44,0x6c,0x38,0x10,0x10,0x10,0x00} },
{'Z', new byte[]{0x7c,0x04,0x08,0x10,0x20,0x40,0x7c,0x00} },
{'0', new byte[]{0x38,0x44,0x4c,0x54,0x64,0x44,0x38,0x00} },
{'1', new byte[]{0x10,0x30,0x70,0x10,0x10,0x10,0x7c,0x00} },
{'2', new byte[]{0x38,0x44,0x04,0x08,0x30,0x40,0x7c,0x00} },
{'3', new byte[]{0x38,0x44,0x04,0x18,0x04,0x44,0x38,0x00} },
{'4', new byte[]{0x08,0x18,0x28,0x48,0x7c,0x08,0x08,0x00} },
{'5', new byte[]{0x7c,0x40,0x70,0x08,0x04,0x44,0x38,0x00} },
{'6', new byte[]{0x38,0x4c,0x40,0x78,0x44,0x44,0x38,0x00} },
{'7', new byte[]{0x7c,0x44,0x08,0x10,0x10,0x20,0x20,0x00} },
{'8', new byte[]{0x38,0x6c,0x6c,0x38,0x6c,0x6c,0x38,0x00} },
{'9', new byte[]{0x38,0x44,0x44,0x38,0x04,0x64,0x38,0x00} },
{'~', new byte[]{0x34,0x58,0x00,0x00,0x00,0x00,0x00,0x00} },
{'`', new byte[]{0x60,0x30,0x18,0x00,0x00,0x00,0x00,0x00} },
{'!', new byte[]{0x30,0x30,0x30,0x30,0x00,0x00,0x30,0x00} },
{'@', new byte[]{0x38,0x44,0x54,0x58,0x40,0x44,0x38,0x00} },
{'#', new byte[]{0x28,0x28,0x7c,0x28,0x7c,0x28,0x28,0x00} },
{'$', new byte[]{0x38,0x54,0x50,0x38,0x14,0x54,0x38,0x00} },
{'%', new byte[]{0x64,0x68,0x08,0x10,0x20,0x2c,0x4c,0x00} },
{'^', new byte[]{0x00,0x10,0x28,0x44,0x00,0x00,0x00,0x00} },
{'&', new byte[]{0x30,0x48,0x48,0x30,0x4c,0x48,0x3c,0x00} },
{'*', new byte[]{0x00,0x48,0x30,0xfc,0x30,0x48,0x00,0x00} },
{'(', new byte[]{0x18,0x30,0x60,0x60,0x60,0x30,0x18,0x00} },
{')', new byte[]{0x60,0x30,0x18,0x18,0x18,0x30,0x60,0x00} },
{'_', new byte[]{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xfc} },
{'+', new byte[]{0x00,0x10,0x10,0x7c,0x10,0x10,0x00,0x00} },
{'|', new byte[]{0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x30} },
{'-',  new byte[]{0x00,0x00,0x00,0x7c,0x00,0x00,0x00,0x00} },
{'=', new byte[]{0x00,0x00,0x7c,0x00,0x7c,0x00,0x00,0x00} },
{'\\',new byte[]{0x00,0x40,0x60,0x30,0x18,0x0c,0x04,0x00} },
{'[', new byte[]{0x78,0x40,0x40,0x40,0x40,0x40,0x78,0x00} },
{']', new byte[]{0x78,0x08,0x08,0x08,0x08,0x08,0x78,0x00} },
{'{', new byte[]{0x1c,0x30,0x30,0x60,0x30,0x30,0x1c,0x00} },
{'}', new byte[]{0x70,0x18,0x18,0x0c,0x18,0x18,0x70,0x00} },
{';',  new byte[]{0x00,0x00,0x18,0x00,0x00,0x18,0x18,0x30} },
{':', new byte[]{0x00,0x00,0x18,0x00,0x00,0x18,0x00,0x00} },
{'\'',new byte[]{0x18,0x30,0x60,0x00,0x00,0x00,0x00,0x00} },
{'"', new byte[]{0x6c,0x6c,0x28,0x00,0x00,0x00,0x00,0x00} },
{',', new byte[]{0x00,0x00,0x00,0x00,0x00,0x18,0x18,0x30} },
{'.', new byte[]{0x00,0x00,0x00,0x00,0x00,0x30,0x30,0x00} },
{'/', new byte[]{0x00,0x04,0x0c,0x18,0x30,0x60,0x40,0x00} },
{'<', new byte[]{0x0c,0x18,0x30,0x60,0x30,0x18,0x0c,0x00} },
{'>', new byte[]{0x60,0x30,0x18,0x0c,0x18,0x30,0x60,0x00} },
{'?', new byte[]{0x38,0x6c,0x44,0x0c,0x18,0x00,0x18,0x00} },
{'£', new byte[]{0x18,0x24,0x60,0xf8,0x60,0xc4,0xf8,0x00} },
{'¥', new byte[]{0x6c,0x6c,0x6c,0x38,0x10,0x7c,0x10,0x00} },
{'§', new byte[]{0x78,0xc0,0x78,0xcc,0x78,0x0c,0x78,0x00} },
{'©', new byte[]{0x30,0x48,0x94,0xa4,0x94,0x48,0x30,0x00} },
{'«', new byte[]{0x00,0x14,0x28,0x50,0x28,0x14,0x00,0x00} },
{'¬', new byte[]{0x00,0x00,0x7c,0x0c,0x0c,0x00,0x00,0x00} },
{'°', new byte[]{0x38,0x6c,0x38,0x00,0x00,0x00,0x00,0x00} },
{'µ', new byte[]{0x00,0x00,0x24,0x24,0x24,0x24,0x78,0xc0} },
{'»', new byte[]{0x00,0x50,0x28,0x14,0x28,0x50,0x00,0x00} },
{'¿', new byte[]{0x00,0x30,0x00,0x30,0x60,0x44,0x6c,0x38} },
{'ß', new byte[]{0x38,0x64,0x64,0x68,0x64,0x64,0x68,0x00} },
{'à', new byte[]{0x60,0x30,0x38,0x04,0x3c,0x44,0x3c,0x00} },
{'á', new byte[]{0x0c,0x18,0x38,0x04,0x3c,0x44,0x3c,0x00} },
{'â', new byte[]{0x30,0x48,0x38,0x04,0x3c,0x44,0x3c,0x00} },
{'ä', new byte[]{0x28,0x00,0x38,0x04,0x3c,0x44,0x3c,0x00} },
{'æ', new byte[]{0x00,0x00,0x78,0x14,0x7c,0xd0,0x7c,0x00} },
{'ç', new byte[]{0x00,0x00,0x38,0x44,0x40,0x3c,0x10,0x60} },
{'è', new byte[]{0x60,0x30,0x38,0x44,0x7c,0x40,0x3c,0x00} },
{'é', new byte[]{0x0c,0x18,0x38,0x44,0x7c,0x40,0x3c,0x00} },
{'ê', new byte[]{0x30,0x48,0x38,0x44,0x7c,0x40,0x3c,0x00} },
{'ë', new byte[]{0x28,0x00,0x38,0x44,0x7c,0x40,0x3c,0x00} },
{'ì', new byte[]{0x60,0x30,0x00,0x10,0x10,0x10,0x10,0x00} },
{'í', new byte[]{0x0c,0x18,0x00,0x10,0x10,0x10,0x10,0x00} },
{'î', new byte[]{0x30,0x48,0x00,0x10,0x10,0x10,0x10,0x00} },
{'ï', new byte[]{0x28,0x00,0x10,0x10,0x10,0x10,0x10,0x00} },
{'ñ', new byte[]{0x64,0x98,0x00,0x78,0x44,0x44,0x44,0x00} },
{'ò', new byte[]{0x60,0x30,0x38,0x44,0x44,0x44,0x38,0x00} },
{'ó', new byte[]{0x0c,0x18,0x38,0x44,0x44,0x44,0x38,0x00} },
{'ô', new byte[]{0x30,0x48,0x38,0x44,0x44,0x44,0x38,0x00} },
{'ö', new byte[]{0x28,0x00,0x38,0x44,0x44,0x44,0x38,0x00} },
{'ù', new byte[]{0x60,0x30,0x44,0x44,0x44,0x44,0x38,0x00} },
{'ú', new byte[]{0x0c,0x18,0x44,0x44,0x44,0x44,0x38,0x00} },
{'û', new byte[]{0x30,0x48,0x44,0x44,0x44,0x44,0x38,0x00} },
{'ü', new byte[]{0x28,0x00,0x44,0x44,0x44,0x44,0x38,0x00} },
{'¡', new byte[]{0x30,0x00,0x00,0x30,0x30,0x30,0x30,0x00} }
  };

  readonly Dictionary<char, byte[]> font = new Dictionary<char, byte[]>() {
{' ', new byte[]{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00} },
{'a', new byte[]{0x00,0x00,0x3c,0x06,0x3e,0x66,0x3e,0x00} },
{'b', new byte[]{0x60,0x60,0x7c,0x66,0x66,0x66,0x7c,0x00} },
{'c', new byte[]{0x00,0x00,0x3c,0x66,0x60,0x66,0x3c,0x00} },
{'d', new byte[]{0x06,0x06,0x3e,0x66,0x66,0x66,0x3e,0x00} },
{'e', new byte[]{0x00,0x00,0x3c,0x66,0x7e,0x60,0x3e,0x00} },
{'f', new byte[]{0x1c,0x36,0x30,0x78,0x30,0x30,0x30,0x00} },
{'g', new byte[]{0x00,0x00,0x3e,0x66,0x66,0x3e,0x06,0x7c} },
{'h', new byte[]{0x60,0x60,0x7c,0x66,0x66,0x66,0x66,0x00} },
{'i', new byte[]{0x18,0x00,0x18,0x18,0x18,0x18,0x18,0x00} },
{'j', new byte[]{0x06,0x00,0x06,0x06,0x06,0x06,0x66,0x3c} },
{'k', new byte[]{0x60,0x60,0x66,0x6c,0x78,0x7c,0x66,0x00} },
{'l', new byte[]{0x38,0x18,0x18,0x18,0x18,0x18,0x3c,0x00} },
{'m', new byte[]{0x00,0x00,0x63,0x77,0x7f,0x6b,0x63,0x00} },
{'n', new byte[]{0x00,0x00,0x7c,0x66,0x66,0x66,0x66,0x00} },
{'o', new byte[]{0x00,0x00,0x3c,0x66,0x66,0x66,0x3c,0x00} },
{'p', new byte[]{0x00,0x00,0x7c,0x66,0x66,0x7c,0x60,0x60} },
{'q', new byte[]{0x00,0x00,0x3e,0x66,0x66,0x3e,0x06,0x06} },
{'r', new byte[]{0x00,0x00,0x7c,0x66,0x60,0x60,0x60,0x00} },
{'s', new byte[]{0x00,0x00,0x3c,0x60,0x3c,0x06,0x7c,0x00} },
{'t', new byte[]{0x30,0x30,0xfc,0x30,0x30,0x36,0x1c,0x00} },
{'u', new byte[]{0x00,0x00,0x66,0x66,0x66,0x66,0x3c,0x00} },
{'v', new byte[]{0x00,0x00,0x66,0x66,0x66,0x3c,0x18,0x00} },
{'w', new byte[]{0x00,0x00,0x63,0x6b,0x7f,0x36,0x22,0x00} },
{'x', new byte[]{0x00,0x00,0x66,0x3c,0x18,0x3c,0x66,0x00} },
{'y', new byte[]{0x00,0x00,0x66,0x66,0x66,0x3e,0x06,0x7c} },
{'z', new byte[]{0x00,0x00,0x7e,0x0c,0x18,0x30,0x7e,0x00} },
{'A', new byte[]{0x18,0x3c,0x66,0x7e,0x66,0x66,0x66,0x00} },
{'B', new byte[]{0x7c,0x66,0x66,0x7c,0x66,0x66,0x7c,0x00} },
{'C', new byte[]{0x3c,0x66,0x60,0x60,0x60,0x66,0x3c,0x00} },
{'D', new byte[]{0x78,0x6c,0x66,0x66,0x66,0x6c,0x78,0x00} },
{'E', new byte[]{0x7e,0x60,0x60,0x78,0x60,0x60,0x7e,0x00} },
{'F', new byte[]{0x7e,0x60,0x60,0x78,0x60,0x60,0x60,0x00} },
{'G', new byte[]{0x3c,0x66,0x60,0x6e,0x66,0x66,0x3c,0x00} },
{'H', new byte[]{0x66,0x66,0x66,0x7e,0x66,0x66,0x66,0x00} },
{'I', new byte[]{0x3c,0x18,0x18,0x18,0x18,0x18,0x3c,0x00} },
{'J', new byte[]{0x1e,0x0c,0x0c,0x0c,0x0c,0x6c,0x38,0x00} },
{'K', new byte[]{0x66,0x6c,0x78,0x70,0x78,0x6c,0x66,0x00} },
{'L', new byte[]{0x60,0x60,0x60,0x60,0x60,0x60,0x7e,0x00} },
{'M', new byte[]{0x63,0x77,0x7f,0x6b,0x63,0x63,0x63,0x00} },
{'N', new byte[]{0x66,0x76,0x7e,0x7e,0x6e,0x66,0x66,0x00} },
{'O', new byte[]{0x3c,0x66,0x66,0x66,0x66,0x66,0x3c,0x00} },
{'P', new byte[]{0x7c,0x66,0x66,0x7c,0x60,0x60,0x60,0x00} },
{'Q', new byte[]{0x3c,0x66,0x66,0x66,0x66,0x3c,0x0e,0x00} },
{'R', new byte[]{0x7c,0x66,0x66,0x7c,0x78,0x6c,0x66,0x00} },
{'S', new byte[]{0x3c,0x66,0x60,0x3c,0x06,0x66,0x3c,0x00} },
{'T', new byte[]{0x7e,0x18,0x18,0x18,0x18,0x18,0x18,0x00} },
{'U', new byte[]{0x66,0x66,0x66,0x66,0x66,0x66,0x3c,0x00} },
{'V', new byte[]{0x66,0x66,0x66,0x66,0x66,0x3c,0x18,0x00} },
{'W', new byte[]{0x63,0x63,0x63,0x6b,0x7f,0x77,0x63,0x00} },
{'X', new byte[]{0x66,0x66,0x3c,0x18,0x3c,0x66,0x66,0x00} },
{'Y', new byte[]{0x66,0x66,0x66,0x3c,0x18,0x18,0x18,0x00} },
{'Z', new byte[]{0x7e,0x06,0x0c,0x18,0x30,0x60,0x7e,0x00} },
{'0', new byte[]{0x3c,0x66,0x6e,0x76,0x66,0x66,0x3c,0x00} },
{'1', new byte[]{0x18,0x18,0x38,0x18,0x18,0x18,0x7e,0x00} },
{'2', new byte[]{0x3c,0x66,0x06,0x0c,0x30,0x60,0x7e,0x00} },
{'3', new byte[]{0x3c,0x66,0x06,0x1c,0x06,0x66,0x3c,0x00} },
{'4', new byte[]{0x06,0x0e,0x1e,0x66,0x7f,0x06,0x06,0x00} },
{'5', new byte[]{0x7e,0x60,0x7c,0x06,0x06,0x66,0x3c,0x00} },
{'6', new byte[]{0x3c,0x66,0x60,0x7c,0x66,0x66,0x3c,0x00} },
{'7', new byte[]{0x7e,0x66,0x0c,0x18,0x18,0x18,0x18,0x00} },
{'8', new byte[]{0x3c,0x66,0x66,0x3c,0x66,0x66,0x3c,0x00} },
{'9', new byte[]{0x3c,0x66,0x66,0x3e,0x06,0x66,0x3c,0x00} },
{'~', new byte[]{0x3b,0x6e,0x00,0x00,0x00,0x00,0x00,0x00} },
{'`', new byte[]{0x30,0x18,0x0c,0x00,0x00,0x00,0x00,0x00} },
{'!', new byte[]{0x18,0x18,0x18,0x18,0x00,0x00,0x18,0x00} },
{'@', new byte[]{0x3c,0x66,0x6e,0x6e,0x60,0x62,0x3c,0x00} },
{'#', new byte[]{0x66,0x66,0xff,0x66,0xff,0x66,0x66,0x00} },
{'$', new byte[]{0x18,0x3e,0x60,0x3c,0x06,0x7c,0x18,0x00} },
{'%', new byte[]{0x62,0x66,0x0c,0x18,0x30,0x66,0x46,0x00} },
{'^', new byte[]{0x00,0x18,0x3c,0x66,0x00,0x00,0x00,0x00} },
{'&', new byte[]{0x3c,0x66,0x3c,0x38,0x67,0x66,0x3f,0x00} },
{'*', new byte[]{0x00,0x66,0x3c,0xff,0x3c,0x66,0x00,0x00} },
{'(', new byte[]{0x0c,0x18,0x30,0x30,0x30,0x18,0x0c,0x00} },
{')', new byte[]{0x30,0x18,0x0c,0x0c,0x0c,0x18,0x30,0x00} },
{'_', new byte[]{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xff} },
{'+', new byte[]{0x00,0x18,0x18,0x7e,0x18,0x18,0x00,0x00} },
{'|', new byte[]{0x18,0x18,0x18,0x18,0x18,0x18,0x18,0x18} },
{'-', new byte[]{0x00,0x00,0x00,0x7e,0x00,0x00,0x00,0x00} },
{'=', new byte[]{0x00,0x00,0x7e,0x00,0x7e,0x00,0x00,0x00} },
{'\\', new byte[]{0x00,0x60,0x30,0x18,0x0c,0x06,0x03,0x00} },
{'[', new byte[]{0x3c,0x30,0x30,0x30,0x30,0x30,0x3c,0x00} },
{']', new byte[]{0x3c,0x0c,0x0c,0x0c,0x0c,0x0c,0x3c,0x00} },
{'{', new byte[]{0x0e,0x18,0x18,0x70,0x18,0x18,0x0e,0x00} },
{'}', new byte[]{0x70,0x18,0x18,0x0e,0x18,0x18,0x70,0x00} },
{';', new byte[]{0x00,0x00,0x18,0x00,0x00,0x18,0x18,0x30} },
{':', new byte[]{0x00,0x00,0x18,0x00,0x00,0x18,0x00,0x00} },
{'\'', new byte[]{0x06,0x0c,0x18,0x00,0x00,0x00,0x00,0x00} },
{'"', new byte[]{0x66,0x66,0x66,0x00,0x00,0x00,0x00,0x00} },
{',', new byte[]{0x00,0x00,0x00,0x00,0x00,0x18,0x18,0x30} },
{'.', new byte[]{0x00,0x00,0x00,0x00,0x00,0x18,0x18,0x00} },
{'/', new byte[]{0x00,0x03,0x06,0x0c,0x18,0x30,0x60,0x00} },
{'<', new byte[]{0x0e,0x18,0x30,0x60,0x30,0x18,0x0e,0x00} },
{'>', new byte[]{0x70,0x18,0x0c,0x06,0x0c,0x18,0x70,0x00} },
{'?', new byte[]{0x3c,0x66,0x06,0x0c,0x18,0x00,0x18,0x00} },
{'£', new byte[]{0x0c,0x12,0x30,0x7c,0x30,0x62,0xfc,0x00} },
{'¥', new byte[]{0x66,0x66,0x66,0x3c,0x18,0x7e,0x18,0x00} },
{'§', new byte[]{0x3c,0x60,0x3c,0x66,0x3c,0x06,0x3c,0x00} },
{'©', new byte[]{0x1c,0x22,0x4d,0x51,0x4d,0x22,0x1c,0x00} },
{'«', new byte[]{0x00,0x1b,0x36,0x6c,0x36,0x1b,0x00,0x00} },
{'¬', new byte[]{0x00,0x00,0x7e,0x06,0x06,0x00,0x00,0x00} },
{'°', new byte[]{0x3c,0x66,0x3c,0x00,0x00,0x00,0x00,0x00} },
{'µ', new byte[]{0x00,0x00,0x66,0x66,0x66,0x66,0xfe,0xc0} },
{'»', new byte[]{0x00,0x6c,0x36,0x1b,0x36,0x6c,0x00,0x00} },
{'¿', new byte[]{0x00,0x18,0x00,0x18,0x30,0x60,0x66,0x3c} },
{'ß', new byte[]{0x3c,0x66,0x66,0x6c,0x66,0x66,0x6c,0x00} },
{'à', new byte[]{0x30,0x18,0x3c,0x06,0x3e,0x66,0x3e,0x00} },
{'á', new byte[]{0x0c,0x18,0x3c,0x06,0x3e,0x66,0x3e,0x00} },
{'â', new byte[]{0x18,0x24,0x3c,0x06,0x3e,0x66,0x3e,0x00} },
{'ä', new byte[]{0x24,0x00,0x3c,0x06,0x3e,0x66,0x3e,0x00} },
{'æ', new byte[]{0x00,0x00,0x7e,0x1b,0x7f,0xd8,0x7f,0x00} },
{'ç', new byte[]{0x00,0x00,0x3c,0x66,0x60,0x3c,0x18,0x70} },
{'è', new byte[]{0x30,0x18,0x3c,0x66,0x7e,0x60,0x3e,0x00} },
{'é', new byte[]{0x0c,0x18,0x3c,0x66,0x7e,0x60,0x3e,0x00} },
{'ê', new byte[]{0x18,0x24,0x3c,0x66,0x7e,0x60,0x3e,0x00} },
{'ë', new byte[]{0x24,0x00,0x3c,0x66,0x7e,0x60,0x3e,0x00} },
{'ì', new byte[]{0x30,0x18,0x00,0x18,0x18,0x18,0x18,0x00} },
{'í', new byte[]{0x0c,0x18,0x00,0x18,0x18,0x18,0x18,0x00} },
{'î', new byte[]{0x18,0x24,0x18,0x18,0x18,0x18,0x18,0x00} },
{'ï', new byte[]{0x24,0x00,0x18,0x18,0x18,0x18,0x18,0x00} },
{'ñ', new byte[]{0x3b,0x6e,0x7c,0x66,0x66,0x66,0x66,0x00} },
{'ò', new byte[]{0x30,0x18,0x3c,0x66,0x66,0x66,0x3c,0x00} },
{'ó', new byte[]{0x0c,0x18,0x3c,0x66,0x66,0x66,0x3c,0x00} },
{'ô', new byte[]{0x18,0x24,0x3c,0x66,0x66,0x66,0x3c,0x00} },
{'ö', new byte[]{0x24,0x00,0x3c,0x66,0x66,0x66,0x3c,0x00} },
{'ù', new byte[]{0x30,0x18,0x66,0x66,0x66,0x66,0x3c,0x00} },
{'ú', new byte[]{0x0c,0x18,0x66,0x66,0x66,0x66,0x3c,0x00} },
{'û', new byte[]{0x18,0x24,0x66,0x66,0x66,0x66,0x3c,0x00} },
{'ü', new byte[]{0x24,0x00,0x66,0x66,0x66,0x66,0x3c,0x00} },
{'¡', new byte[]{0x00,0x18,0x00,0x00,0x18,0x18,0x18,0x18} }
  };

  public bool Minimized = false;
}


