using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


public class Gitem : IComparable
{
  /* This is Java code and have to change it around to IComparer C# 
  public int compareTo(Gitem I)
  {
    return (ri*65536/2 + pi) - (I.ri*65536/2 + I.pi); //something
  }
  */ 

  internal class GitemComparer : IComparer<Gitem>
  {
    public int Compare(Gitem x, Gitem y)
    {
      var expr = (x.ri*65536/2 + x.pi) - (y.ri*65536/2 + y.pi); 
      if (expr == 0){
        return x.la.CompareTo(y.la);
      }
      return expr;
    }
  }

  public short ri; // rule index
  public short pi; // position of the dot
  public short la; // look ahead for LR(1)
  public bool processed = false; //used for other procedures

  public Gitem(short a, short b, short c) { ri=a; pi=b; la=c; }
  public Gitem(int a, int b, int c) { ri=(short) a; pi=(short) b; la=(short) c;}
    
  public override bool Equals(Object b)
  {
    // ????  should Gitem also implement IComparable? 
    // this was for sorint we can easily pass in the comparer 
    // but for direct comparison we can use compareTo? Is that redundant?
    return CompareTo((Gitem)b) == 0; //compareTo((Gitem)b) == 0;
  }

  public int CompareTo(object I)
  {
    if (I == null) return 1;

    Gitem other = I as Gitem;
    if (other != null){
      var expr = (ri*65536/2 + pi) - (other.ri*65536/2 + other.pi);
      if ( expr == 0){
        return la.CompareTo(other.la);
      }
      return expr;
    }
    else
      throw new ArgumentException("Object is not gitem");
  }

  public override int GetHashCode()
  {
    return ri.GetHashCode() ^ pi.GetHashCode() ^ la.GetHashCode();
  }

  public string toString()
  {
    return ri+" : "+pi+" : "+la;
  }

  // may want to write prettyprint function
  // TODO: Print Item


  public static void Main(string[] argv){
    Gitem gi = new Gitem(1,2,3);
    Console.WriteLine(gi.toString());
  }

} //Gitem