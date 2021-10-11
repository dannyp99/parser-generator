public interface IStateAction
{
    void DoIt(AbsParser absParser);
}

public abstract class AbsParser
{

}

public class Reduce : IStateAction
{
    public int Rulei {get; set; } // rule index (for an arraylist called Rules) to reduce to
    public Reduce(int ri)
    {
      Rulei = ri;
    }
    public void DoIt(AbsParser absParser) {}
}

public class Shift : IStateAction
{
  int Nextstate { get; set; } // the state index (for an arraylist of states) to go to.

  public Shift(int nextState)
  {
    Nextstate = nextState;
  }
  public void DoIt(AbsParser absParser) {}
}

public class GotoState : IStateAction
{
  public int Nsi { get; set; } // next state index
  public GotoState(int nsi)
  {
    Nsi = nsi;
  }
  public void DoIt(AbsParser absParser) {}
}
public class Accept : IStateAction
{
  public void DoIt(AbsParser absParser) {}
}