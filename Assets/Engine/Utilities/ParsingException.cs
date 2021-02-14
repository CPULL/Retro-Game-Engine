using System;

public class ParsingException : Exception {
  public string Code;

  public ParsingException(string code) {
    Code = code;
  }

  public ParsingException(string message, string code) : base(message) {
    Code = code;
  }

  public ParsingException(string message, Exception inner, string code) : base(message, inner) {
    Code = code;
  }
}
