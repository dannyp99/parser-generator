using System;
public interface Expr
{
  int Eval();
}


public class Val : Expr 
{
  int Value {get; set;}
  public Val(int a) 
  {
    Value = a;
  }
  public int Eval()
  {
    return Value;
  }
}

public class NegExpr : Expr
{
  Expr E;
  public NegExpr(Expr a)
  {
    E = a;
  }
  public int Eval()
  {
    return -1 * E.Eval();
  }
}
public class Binop
{
  public Expr E1, E2;
  public Binop() {}
  public Binop(Expr a, Expr b)
  {
    E1=a;
    E2=b;
  } 

}
public class BoolExpr
{
  bool Value;
  public BoolExpr(bool a)
  {
    Value = a;
  }

  public bool Eval()
  {
    return Value;
  }
}
public class SumExpr : Binop, Expr
{
  public SumExpr(Expr a, Expr b) : base(a,b) {}
  public int Eval()
  {
    return  E1.Eval() + E2.Eval();
  }
}

public class MultExpr : Binop, Expr
{
  public MultExpr(Expr a, Expr b) : base(a,b) {}
  public int Eval()
  {
    return E1.Eval() + E2.Eval();
  }
}

public class AndExpr : Binop, Expr
{
  public AndExpr(Expr a, Expr b) : base(a,b) {}
  public int Eval()
  {
    return 1;
  }
}

public class EqExpr : Binop, Expr
{
  public EqExpr(Expr a, Expr b) : base(a,b) {}
  public int Eval()
  {
    return 2;
  }
}

public class Driver
{
  public static void Main(string[] argv)
  {
    if(test)
    {
      Console.WriteLine("This wont matter");
    }
  }
}
// Encountered issue where logic Expr want to return bool
// and not int but the interface is int Eval(). Can
// this be cirumvented?
// Solutions are find the way to do what I want OR
// hand code those cases (which then nullify the use of)
// interface and Binop class