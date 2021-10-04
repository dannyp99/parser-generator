using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


public class Gitem // : IComparable<Gitem>??
{
  /* This is Java code and have to change it around to IComparer C# 
  public int compareTo(Gitem I)
  {
    return (ri*65536/2 + pi) - (I.ri*65536/2 + I.pi); //something
  }
  */ 

  public class GitemComparer : IComparer<Gitem>
  {
    int IComparer<Gitem>.Compare(Gitem x, Gitem y)
    {
      return (x.ri*65536/2 + x.pi + x.la) - (y.ri*65536/2 + y.pi + y.la); 
    }
  }

  public short ri; // rule index
  public short pi; // position of the dot
  public short la; // look ahead for LR(1)
  public bool processed = false; //used for other procedures

  public Gitem(short a, short b, short c) { ri=a; pi=b; la=c; }
  public Gitem(int a, int b, int c) { ri=(short) a; pi=(short) b; la=(short) c;}
    
  public bool equals(Object b)
  {
    // ????  should Gitem also implement IComparable? 
    // this was for sorint we can easily pass in the comparer 
    // but for direct comparison we can use compareTo? Is that redundant?
    GitemComparer gicomp = new GitemComparer(); 
    return (gicomp.Compare(this,(Gitem)b)) == 0; //compareTo((Gitem)b) == 0;
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