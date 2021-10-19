public interface IStateAction
{
  int Next {get; set;}
  void DoIt(AbsParser absParser);
}

public abstract class AbsParser
{

}

public class Reduce : IStateAction
{
  public int Next {get; set;}
  private int _rulei;
  public int Rulei {get{return _rulei;} set { _rulei = value; Next = value; } } // rule index (for an arraylist called Rules) to reduce to
  public Reduce(int ri)
  {
    Rulei = ri;
  }
  public void DoIt(AbsParser absParser) {}
}

public class Shift : IStateAction
{
  public int Next {get; set;}
  private int _nextstate;
  int Nextstate { get{return _nextstate;} set { _nextstate=value; Next=value;} } // the state index (for an arraylist of states) to go to.

  public Shift(int nextState)
  {
    Nextstate = nextState;
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
  private int _nsi;
  public int Nsi { get{return _nsi;} set {_nsi=value; Next=value;} } // next state index
  public GotoState(int nsi)
  {
    Nsi = nsi;
  }
  public void DoIt(AbsParser absParser) {}

  public override string ToString() 
  {
    return "GotoState";
  }
}
public class Accept : IStateAction
{
  public int Next{get;set;}
  public void DoIt(AbsParser absParser) {}

  public override string ToString() 
  {
    return "Accept";
  }
}