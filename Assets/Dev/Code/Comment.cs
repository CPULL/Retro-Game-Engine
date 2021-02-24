
public class Comment {
  public string comment;
  public CodeNode.CommentType type;
  public int indent;
  public Comment() {
    comment = null;
    type = CodeNode.CommentType.None;
    indent = 0;
  }
  public Comment(string c, CodeNode.CommentType t) {
    comment = c;
    type = t;
    indent = 0;
  }
  internal void Set(string value, CodeNode.CommentType t) {
    comment = value;
    type = t;
  }
  public void Zero() {
    comment = null;
    type = CodeNode.CommentType.None;
    indent = 0;
  }
}
