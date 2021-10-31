public interface IStateAction
{
  int Next {get; set;}
  void DoIt(AbsParser absParser);
}

public abstract class AbsParser {
  
} 

public class Reduce : IStateAction
{
  public int Next {get; set;}
  public Reduce(int next)
  {
    Next = next;
  }
  public void DoIt(AbsParser absParser) {}
}

public class Shift : IStateAction
{
  public int Next {get; set;}

  public Shift(int next)
  {
    Next = next;
  }
  public void DoIt(AbsParser absParser) {}

  public override string ToString() 
  {
    return "Shift";
  }
}

public class GotoState : IStateAction
{
  public int Next { get; set;}
  public GotoState(int next)
  {
    Next = next;
  }
  public void DoIt(AbsParser absParser) {}

  public override string ToString() 
  {
    return "GotoState";
  }
}
public class Accept : IStateAction
{
  public int Next{ get; set; }
  public void DoIt(AbsParser absParser) {}

  public override string ToString() 
  {
    return "Accept";
  }
}

public class Error : IStateAction
{
  public int Next {get; set; }
  public string Message { get; set; }

  public Error(string message)
  {
    Message = message;
  }

  public void DoIt(AbsParser absParser) {}

    public override string ToString()
    {
      return "Error" + Message;
    }
}