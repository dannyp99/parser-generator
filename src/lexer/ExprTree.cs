using System; // FOR TESTING ONLY
public interface Expr
{
  int Eval();
}
public class Val : Expr
{
  public int Value {get; set;}
  public Val(int a)
  {
    Value=a;
  }
  public int Eval()
  {
    return Value;
  }  

}

public class Plus : Expr
{
  Expr E1 {get;set;}
  Expr E2 {get;set;}
  public Plus(Expr e1, Expr e2)
  {
    E1 = e1;
    E2 = e2;
  }
  public int Eval()
  {
    return E1.Eval() + E2.Eval();
  }
}

public class Times : Expr
{
  Expr E1 {get;set;}
  Expr E2 {get;set;}

  public Times(Expr e1, Expr e2)
  {
    E1=e1;
    E2=e2;
  }
  public int Eval()
  {
    return E1.Eval() * E2.Eval();
  }
}

public class Minus : Expr
{
  Expr E1 {get; set;}
  Expr E2 {get; set;}
  public Minus(Expr e1, Expr e2)
  {
    E1=e1;
    E2=e2;
  }
  public int Eval()
  {
    return E1.Eval() - E2.Eval();
  }
}
public class Negative : Expr
{
  Expr E {get; set;}
  public Negative(Expr e)
  {
    E=e;
  }
  public int Eval()
  {
    return -1*E.Eval();
  } 
}


/* public static void Main(string[] argv)
 {
   Expr a = new Val(4);
   Expr b = new Val(5);

   Console.WriteLine(Plus(a,b));
   Console.WriteLine(Times(a,b));
   Console.WriteLine(Minus(a,b));
   Console.WriteLine(Negative(a));
 } */

