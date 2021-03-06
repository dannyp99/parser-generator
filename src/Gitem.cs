using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

  public class GitemComparer : IComparer<Gitem>
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

  public class Gitem : IComparable
  {
    public short Ri; // rule index
    public short Pi; // position of the dot
    public string La; // look ahead for LR(1)
    public bool Processed = false; //used for other procedures

    public Gitem() { }
    public Gitem(short a, short b, string c) { Ri=a; Pi=b; La=c; Processed=false; }
    public Gitem(short a, short b) { Ri=a; Pi=b; La=string.Empty; Processed=false; }
    public Gitem(int a, int b, string c) { Ri=(Int16) a; Pi=(Int16) b; La=c; Processed=false; }
    public Gitem(int a, int b) { Ri=(Int16) a; Pi=(Int16) b; La=string.Empty; Processed=false; }
      
    public override bool Equals(Object b)
    {
      // ????  should Gitem also implement IComparable? 
      // this was for sorint we can easily pass in the comparer 
      // but for direct comparison we can use compareTo? Is that redundant?
      //return CompareTo(b) == 0; //compareTo((Gitem)b) == 0;
      if (b==null) return false;
      var other = (Gitem)b;
      return Ri==other.Ri && Pi==other.Pi && La.Equals(other.La);
    }

    public int CompareTo(object I)
    {
      if (I == null) return 1;

      Gitem other = (Gitem)I;
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
      //return Ri.GetHashCode() ^ Pi.GetHashCode() ^ La.GetHashCode();
      return (""+Ri+","+Pi+","+La).GetHashCode();
    }

    public override string ToString()
    {
      return "Gitem::  Rule Index: " + Ri + " :: Dot: " + Pi + " :: lookahead: " + La;
    }

    // may want to write prettyprint function
    // TODO: Print Item
  } //Gitem