public class ExecStacks {
  readonly System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
  readonly ExecStack[] stacks;
  int level;

  public bool Invalid { get { return level == 0 && !stacks[0].valid; } }

  public ExecStacks() {
    stacks = new ExecStack[64];
    level = 0;
    for (int i = 0; i < 64; i++)
      stacks[i] = new ExecStack();
  }

  public CodeNode GetExecutionNode(Arcade vm) {
    if (!stacks[level].valid) return null;
    ExecStack stack = stacks[level];
    stack.step++;
    // Do we have a step here?
    if (stack.node.type != BNF.BLOCK || stack.node.children == null || stack.node.children.Count == 0) { // No, single step
      if (stack.step == 0) return stack.node;
    }
    else { // Yes, sequence
      if (stack.step < stack.node.children.Count) return stack.node.children[stack.step];
    }

    // If we are here we completed the sequence
    if (stack.cond != null && vm.Evaluate(stack.cond).ToBool(culture)) {
      // Restart
      stack.step = 0;
      if (stack.node.type != BNF.BLOCK || stack.node.children == null || stack.node.children.Count == 0) {
        return stack.node; // Single step
      }
      else {
        return stack.node.children[0]; // Sequence
      }
    }

    // Go up one level
    if (level == 0) return null; // Nothing to do
    stack.valid = false;
    level--;
    return GetExecutionNode(vm);
  }

  public void AddStack(CodeNode node, CodeNode cond, string origline, int origlinenum) {
    if (node == null) return;
    if (level == 63) throw new System.Exception("Too many levels of recursion\nstack overflow!\n" + stacks[63].origLine + ": " + stacks[63].origLineNum);
    level = Invalid ? 0 : level + 1;
    ExecStack stack = stacks[level];
    stack.valid = true;
    stack.node = node;
    if (node.type == BNF.Start || node.type == BNF.Update) node.type = BNF.BLOCK;
    stack.cond = cond;
    stack.origLine = origline;
    stack.origLineNum = origlinenum;
    stack.step = -1;
  }

  public bool PopUp() {
    if (level == 0) return true;
    stacks[level].valid = false;
    level--;
    return false;
  }

  public void Destroy() {
    level = 0;
    stacks[0].valid = false;
  }
}


public class ExecStack {
  public bool valid = false;
  public CodeNode node; // Node being executed (its children will be executed in case of a block)
  public CodeNode cond; // Possible condition (used in case of `for` and `while`)
  public string origLine; // in case of errors
  public int origLineNum; // in case of errors
  public int step = -1; // Current step of the node to be executed
}
