using System;

public class ParsingException : Exception {
  public string Code;
  public int LineNum;

  public ParsingException(string code) : base(code) {
    Code = code;
    LineNum = 0;
  }

  public ParsingException(string message, string code) : base(message) {
    Code = code;
    LineNum = 0;
  }

  public ParsingException(string message, int line) : base(message) {
    Code = "";
    LineNum = line;
  }

  public ParsingException(string message, string code, int line) : base(message) {
    Code = code;
    LineNum = line;
  }

  public ParsingException(string message, Exception inner, string code) : base(message, inner) {
    Code = code;
    LineNum = 0;
  }
}
