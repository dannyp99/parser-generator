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

  public static int Plus(Expr e1, Expr e2)
  {
    return e1.Eval() + e2.Eval();
  }
  public static int Times(Expr e1, Expr e2)
  {
    return e1.Eval() * e2.Eval();
  }
 
  public static int Minus(Expr e1, Expr e2)
  {
    return e1.Eval() - e2.Eval();
  }
  public static int Negative(Expr e)
  {
    return -1*e.Eval();
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
}
