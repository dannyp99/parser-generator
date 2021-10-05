using System;
using System.Collections.Generic;
using System.Linq;

public class AbsynType {

}

public class GrammarSym<AbsynType> {
    public string Sym { get; set; }
    public string FsharpType { get; set; }
    public bool Terminal { get; set; }
    public string Label { get; set; }
    public AbsynType value { get; set; }
    public int Precedence { get; set; }

    public GrammarSym(string s, bool isTerminal)
    {
        Sym = s;
        Terminal = isTerminal;
        Label = string.Empty;
        FsharpType = "String";
        Precedence = 20;
    }    
}

public class GrammarRule {
    public GrammarSym<AbsynType> Lhs { get; set; }
    public List<GrammarSym<AbsynType>> Rhs { get; set; }
    public string Action { get; set; }
    public string Operation { get; set; }
    
    public void PrintRule()
    {
        Console.Write($"Production: {Lhs.Sym} --> ");
        foreach (var s in Rhs)
        {
            Console.Write($"{s.Sym}");
            if (s.Label.Length > 0)
            {
                Console.Write($":{s.Label} ");
            }
            Console.Write(" ");
        }
        Console.WriteLine();
    }
}

public class Grammar
{
    private bool TRACE = false;
    public Dictionary<string, GrammarSym<AbsynType>> Symbols { get; set; }
    public List<GrammarRule> Rules { get; set; }
    public string TopSym { get; set; }
    public int Linenum { get; set; } // only for meta parser
    public SortedSet<string> Nullable { get; set; }
    public Dictionary<string, SortedSet<string>> First { get; set; }

    public Grammar()
    {
        Symbols = new Dictionary<string, GrammarSym<AbsynType>>();
        Rules = new List<GrammarRule>();
        TopSym = string.Empty;
        Linenum = 0;
        Nullable = new SortedSet<string>();
        First = new Dictionary<string, SortedSet<string>>();
    }

    public bool NonTerminal(string s)
    {
        return !Symbols[s].Terminal;
    }

    public bool Terminal(string s)
    {
        return Symbols[s].Terminal;
    }

    public void PrintFirst()
    {
        foreach (var pair in First)
        {
            Console.Write($"First({pair.Key}) = ");
            foreach (var item in pair.Value)
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public void PrintNullable()
    {
        Console.Write("Nullable Set = ");
        foreach ( var str in Nullable)
        {
            Console.Write($"{str} ");
        }
        Console.WriteLine();
    }

    public void ParseStdin()
    {
        var line = "";
        var atEOF = false;
        while (!atEOF)
        {
            line = Console.ReadLine();
            Linenum += 1;

            if (line == null)
            {
                atEOF = true;
            }
            else if (line.Length > 1 && line[0] != '#')
            {
                int linelen = line.Length;
                string[] toks = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (TRACE) {
                    foreach (var tok in toks) {
                        Console.WriteLine("'" + tok + "'");
                    }
                }
                GrammarSym<AbsynType> newTerm;
                switch (toks[0]) {
                    case "EOF":
                        atEOF = true;
                        break;
                    case "terminal":          
                        for(int i = 1; i < toks.Length; i++)
                        {
                        newTerm = new GrammarSym<AbsynType>(toks[i],true);
                        Symbols.Add(toks[i], newTerm);
                        }
                        break;
                    case "typedterminal":
                        newTerm = new GrammarSym<AbsynType>(toks[1],true);
                        newTerm.FsharpType = toks[2];
                        Symbols.Add(toks[1],newTerm);
                        break;
                   case "nonterminal":
                        newTerm = new GrammarSym<AbsynType>(toks[1],false);
                        newTerm.FsharpType = toks[2];
                        Symbols.Add(toks[1],newTerm);
                        break;
                    case "topsym":
                        if (TRACE) {Console.WriteLine("topsym");}
                        TopSym = toks[1];
                        break;
                    case "left":
                    case "right":
                        int preclevel;
                        if(!int.TryParse(toks[2],out preclevel)) { preclevel = 20; }
                        if(toks[0] == "right") { preclevel = -1 * preclevel; }

                        GrammarSym<AbsynType> gsym;
                        if(Symbols.TryGetValue(toks[2], out gsym)) { gsym.Precedence = preclevel; }

                        if (TRACE) {Console.WriteLine("left/right {0} {1}",toks[1],preclevel);}
                        break;
                    default:
                        if (NonTerminal(toks[0]) && toks[1] == "-->") {
                            if (TRACE) {Console.WriteLine("Rule");}
                            GrammarSym<AbsynType> lhsSym = Symbols[toks[0]];
                            List<GrammarSym<AbsynType>> rhsSyms = new List<GrammarSym<AbsynType>>();

                            foreach (string tok in toks.Skip(2)) {
                                if (TRACE) {Console.WriteLine("  " + tok);}
                                if (tok == "{") {
                                    break;
                                }

                                string[] tokLab = tok.Split(':');
                                // TODO handle exception for unrecognized symbol
                                GrammarSym<AbsynType> newSym = Symbols[tokLab[0]];
                                if (tokLab.Length > 1) {
                                    newSym.Label = tokLab[1];
                                }
                                rhsSyms.Add(newSym);
                            }

                            GrammarRule rule = new GrammarRule {
                                Lhs = lhsSym,
                                Rhs = rhsSyms,
                                Operation = default(string)
                            };
                            Rules.Add(rule);
                        } else {
                            throw new Exception("line format unrecognized");
                        }
                        break;
                }
            }
        }

        
        var startnt = new GrammarSym<AbsynType>("START",false);
        GrammarSym<AbsynType> eofterm = new GrammarSym<AbsynType>("EOF",true);
        Symbols.Add("START", startnt);
        Symbols.Add("EOF", eofterm);
        var topgsym = Symbols[TopSym];
        var startRule = new GrammarRule();
        startRule.Lhs = startnt;
        GrammarSym<AbsynType>[] temp = {topgsym,eofterm};
        startRule.Rhs = temp.ToList();

        Rules.Add(startRule);
        

    }

    public void ComputeFirst()
    {
	ComputeNullable();
        var FIRST = new Dictionary<string, SortedSet<string>>();
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var rule in Rules)
            {
                var nt = rule.Lhs.Sym;
                if (!FIRST.ContainsKey(nt))
                {
                    changed = true;
                    FIRST.Add(nt, new SortedSet<string>());
                }
                var Firstnt = FIRST[nt];
                var i = 0;
                var isNullable = true;
                while (i < rule.Rhs.Count && isNullable)
                {
                    var gs = rule.Rhs[i];
                    if (gs.Terminal) {
                        changed = Firstnt.Add(gs.Sym) || changed;
                        if (TRACE) { Console.WriteLine($"{gs.Sym} added to first set of {nt}"); }
                        isNullable = false;
                    }
                    else if (gs.Sym != nt) { // non-terminal
                        var firstGs = new SortedSet<string>();
                        if (FIRST.TryGetValue(gs.Sym, out firstGs)) {
                            foreach (var sym in firstGs)
                            {
                                changed = Firstnt.Add(sym) || changed;
                            }
                        }
                    }
                    if (gs.Terminal || ! this.Nullable.Contains(gs.Sym))
                    {
                        isNullable = false;
                    }
                    i += 1;
                }// while Rhs and isNullable
            }// for each rule
        }// while changed
        foreach (var nt in FIRST.Keys)
        {
            var rcell = FIRST[nt];
            this.First.Add(nt, rcell);
        }
    }

    public void ComputeNullable()
    {
        var changed = true;
        while (changed)
        { 
            changed = false;
            foreach(var r in Rules)
            {
                var addOrNot = true; // add or not
                foreach(var g in r.Rhs)
                {
                    if (g.Terminal || !Nullable.Contains(g.Sym)) { addOrNot = false; }
                }
                if (addOrNot) { changed = Nullable.Add(r.Lhs.Sym) || changed; }
            }// for each rule
        }// while
    }

    // lookahead will always be a terminal
    // TODO verify: no need to iterate, prefer hash set
    public HashSet<string> FirstSeq(List<GrammarSym<AbsynType>> seq, string la) {
        bool nullable = true;
        var firsts = new HashSet<string>();
        foreach (GrammarSym<AbsynType> sym in seq) {
            if (sym.Terminal) {
                nullable = false;
                firsts.Add(sym.Sym);
            } else {
                foreach (string t in First[sym.Sym]) {
                    firsts.Add(t);
                }
                nullable = this.Nullable.Contains(sym.Sym);
            }
            if (!nullable) {
                break;
            }
        }
        if (nullable) {
            firsts.Add(la);
        }
        return firsts;
    }

    // generate slr closure
    public void StateClosure(SortedSet<Gitem> States)
    {
        // keeps track of indices of items already processed:
        var done = new List<Boolean>(new bool[States.Count]);
        int i = 0;
        int closed = 0; // number of processed items
        while (closed < States.Count)
        {
            foreach (var state in States)
            {
                if (!done[i])
                {
                    done[i] = true; closed++; // process one item
                    int ri = state.Ri; // ri is the index of the rule
                    int pi = state.Pi; // position of next symbol after dot
                    var rule = Rules[ri];
                    if (pi < rule.Rhs.Count && !rule.Rhs[pi].Terminal)
                    {
                        // add all initial items for this non-terminal.
                        //List<int> rs = rulesof.get(Ntind.get(rule.Rhs[pi].Sym));
                        var rs = rule.Rhs[pi].Sym;
                        //rs contains indices of all rules that has this nonterminal on the lhs

                        foreach (int j in rs)
                        {
                            Gitem newitem = new Gitem(j, 0);
                            States.Add(newitem);
                            int pgi = new List<Gitem>(States).BinarySearch(newitem); //created a sorted list and locate the state index;
                            // SortedInsert inserts using compareTo order
                            if (pgi >= 0) done[pgi] = false;
                        }// for each rule beginning with nonterminal
                    }//if
                }// !done.get(i)
                i++;
            }// for each item i in State
        }// while
    }//Stateclosure

    // for testing ParseStdin
    public static void Main(string[] argv) {
        Grammar g = new Grammar();
        if (argv.Length > 0) {
            g.TRACE = true;
        }
        g.ParseStdin();
        if (g.TRACE) {Console.Write("\n");}
        Console.WriteLine("info:");
        Console.WriteLine("topsym: " + g.TopSym);
        foreach (var rule in g.Rules) {
            rule.PrintRule();
        }
        g.ComputeFirst();
        g.PrintFirst();
        g.ComputeNullable();
        g.PrintNullable();

        var test_seq = new List<GrammarSym<AbsynType>>();
        test_seq.Add(new GrammarSym<AbsynType>("S", false));
        test_seq.Add(new GrammarSym<AbsynType>("N", false));
        test_seq.Add(new GrammarSym<AbsynType>("T", true));

        var fseq = g.FirstSeq(test_seq, "END");
        Console.WriteLine("Firstseq:");
        foreach (var i in fseq) {
            Console.WriteLine(i);
        }
    }
}

internal class GitemComparer : IComparer<Gitem> {
    public int Compare(Gitem x, Gitem y)
    {
        var expr = (x.Ri*65536/2 + x.Pi) - (y.Ri*65536/2 + y.Pi);
        if (expr == 0)
        {
            return x.La.CompareTo(y.La);
        }
        return expr;
    }
}

public class Gitem : IComparable {
    public short Ri { get; set; } // rule index into metaparser.Rules
    public short Pi { get; set; } // position of dot
    public string La { get; set; }

    public Gitem(int ri, int pi)
    {
        Ri = (Int16)ri;
        Pi = (Int16)Pi;
    }

     public override bool Equals(object b) // equals should be consistent with compare
    {
	    return CompareTo((Gitem)b) == 0;
    }

    public int CompareTo(object I)
    {
        if (I == null) return 1;

        Gitem other = I as Gitem;
        if (other != null) {
          var expr = (Ri*65536/2 + Pi) - (other.Ri*65536/2 + other.Pi);
          if (expr == 0) {
            return La.CompareTo(other.La);
          }
          return expr;
        }
        else
           throw new ArgumentException("Object is not a Gitem");
        
    }

    public override int GetHashCode() // Generates warning without this function
    {
        return Ri.GetHashCode() ^ Pi.GetHashCode() ^ La.GetHashCode();
    }
}
