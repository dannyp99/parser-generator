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
      var expr = (x.Ri*65536/2 + x.Pi) - (y.Ri*65536/2 + y.Pi); 
      if (expr == 0){
        return x.La.CompareTo(y.La);
      }
      return expr;
    }
  }

  public short Ri; // rule index
  public short Pi; // position of the dot
  public string La; // look ahead for LR(1)
  public bool Processed = false; //used for other procedures

  public Gitem(short a, short b, string c) { Ri=a; Pi=b; La=c; }
  public Gitem(short a, short b) { Ri=a; Pi=b; La=string.Empty; }
  public Gitem(int a, int b, string c) { Ri=(Int16) a; Pi=(Int16) b; La = c; }
  public Gitem(int a, int b) { Ri=(Int16) a; Pi=(Int16) b; La=string.Empty; }
    
  public override bool Equals(Object b)
  {
    // ????  should Gitem also implement IComparable? 
    // this was for sorint we can easily pass in the comparer 
    // but for direct comparison we can use compareTo? Is that redundant?
    return CompareTo(b) == 0; //compareTo((Gitem)b) == 0;
  }

  public int CompareTo(object I)
  {
    if (I == null) return 1;

    Gitem other = I as Gitem;
    if (other != null){
      var expr = (Ri*65536/2 + Pi) - (other.Ri*65536/2 + other.Pi);
      if ( expr == 0){
        return La.CompareTo(other.La);
      }
      return expr;
    }
    else
      throw new ArgumentException("Object is not gitem");
  }

  public override int GetHashCode()
  {
    return Ri.GetHashCode() ^ Pi.GetHashCode() ^ La.GetHashCode();
  }

  public override string ToString()
  {
    return Ri + " : " + Pi + " : " + La;
  }

  // may want to write prettyprint function
  // TODO: Print Item
} //Gitem