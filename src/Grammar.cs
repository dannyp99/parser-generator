using System;
using System.Collections.Generic;
using System.Linq;
using State;

public class GrammarSym {
    public string Sym { get; set; }
    public string FsharpType { get; set; }
    public bool Terminal { get; set; }
    public string Label { get; set; }
    public object value { get; set; }
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
    public GrammarSym Lhs { get; set; }
    public List<GrammarSym> Rhs { get; set; }
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
    public Dictionary<string, GrammarSym> Symbols { get; set; }
    public List<GrammarRule> Rules { get; set; }
    public string TopSym { get; set; }
    public int Linenum { get; set; } // only for meta parser
    public SortedSet<string> Nullable { get; set; }
    public Dictionary<string, SortedSet<string>> First { get; set; }
    public Dictionary<string, HashSet<int>> Rulesfor { get; set; }

    public Grammar()
    {
        Symbols = new Dictionary<string, GrammarSym>();
        Rules = new List<GrammarRule>();
        TopSym = string.Empty;
        Linenum = 0;
        Nullable = new SortedSet<string>();
        First = new Dictionary<string, SortedSet<string>>();
        Rulesfor = new Dictionary<string, HashSet<int>>();
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
                GrammarSym newTerm;
                switch (toks[0]) {
                    case "EOF":
                        atEOF = true;
                        break;
                    case "terminal":          
                        for(int i = 1; i < toks.Length; i++)
                        {
                        newTerm = new GrammarSym(toks[i],true);
                        Symbols.Add(toks[i], newTerm);
                        }
                        break;
                    case "typedterminal":
                        newTerm = new GrammarSym(toks[1],true);
                        newTerm.FsharpType = toks[2];
                        Symbols.Add(toks[1],newTerm);
                        break;
                   case "nonterminal":
                        newTerm = new GrammarSym(toks[1],false);
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

                        GrammarSym gsym;
                        if(Symbols.TryGetValue(toks[2], out gsym)) { gsym.Precedence = preclevel; }

                        if (TRACE) {Console.WriteLine("left/right {0} {1}",toks[1],preclevel);}
                        break;
                    default:
                        if (NonTerminal(toks[0]) && toks[1] == "-->") {
                            if (TRACE) {Console.WriteLine("Rule");}
                            GrammarSym lhsSym = Symbols[toks[0]];
                            List<GrammarSym> rhsSyms = new List<GrammarSym>();

                            foreach (string tok in toks.Skip(2)) {
                                if (TRACE) {Console.WriteLine("  " + tok);}
                                if (tok == "{") {
                                    break;
                                }

                                string[] tokLab = tok.Split(':');
                                // TODO handle exception for unrecognized symbol
                                GrammarSym newSym = Symbols[tokLab[0]];
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

        
        var startnt = new GrammarSym("START",false);
        GrammarSym eofterm = new GrammarSym("EOF",true);
        Symbols.Add("START", startnt);
        Symbols.Add("EOF", eofterm);
        var topgsym = Symbols[TopSym];
        var startRule = new GrammarRule();
        startRule.Lhs = startnt;
        GrammarSym[] temp = {topgsym,eofterm};
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
            int rulei = 0;
            changed = false;
            foreach(var r in Rules)
            {

                var addOrNot = true; // add or not
                foreach(var g in r.Rhs)
                {
                    if (g.Terminal || !Nullable.Contains(g.Sym)) { addOrNot = false; }
                }
                if (addOrNot) { changed = Nullable.Add(r.Lhs.Sym) || changed; }
                HashSet<int> None;
                if (Rulesfor.TryGetValue(r.Lhs.Sym, out None))
                {
                    Rulesfor.Add(r.Lhs.Sym, new HashSet<int>());
                }
                var ruleSet = Rulesfor[r.Lhs.Sym];
                ruleSet.Add(rulei);
                rulei += 1;
            }// for each rule
        }// while
    }

    // lookahead will always be a terminal
    // TODO verify: no need to iterate, prefer hash set
    public HashSet<string> FirstSeq(List<GrammarSym> seq, string la) {
        bool nullable = true;
        var firsts = new HashSet<string>();
        foreach (GrammarSym sym in seq) {
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
    public void StateClosure(SortedSet<Gitem> States) //change paramter to Grammar if moved to LR1State class
    {
        while (States.Any(x => !x.Processed)) //while any states is not processed.
        {
            var state = States.FirstOrDefault(x => !x.Processed); //returns first state that is not processed or null otherwise.
            if (state != null)
            {
                state.Processed = true; // process one item
                int ri = state.Ri; // ri is the index of the rule
                int pi = state.Pi; // position of next symbol after dot
                string la = state.La;
                var rule = Rules[ri];
                var lhs = rule.Lhs.Sym;
                //insert into state or this (aka self)?
                if (pi < rule.Rhs.Count && !rule.Rhs[pi].Terminal)
                {
                    var nti = rule.Rhs[pi];
                    var lookaheads = FirstSeq(rule.Rhs.Skip(pi+1).ToList(), la);
                    foreach (var rulent in Rulesfor[nti.Sym])
                    {
                        foreach (var lafollow in lookaheads)
                        {
                            var newItem = new Gitem(rulent, 0, lafollow);
                            if (!States.Contains(newItem)) 
                            {
                                States.Add(newItem);
                                //Ask how if the check is needed and if this should change to State addition
                            }
                        }//foreach lookahead
                    }// foreach Rule of nonterminals
                }//if
            }// not processed
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

        var test_seq = new List<GrammarSym>();
        test_seq.Add(new GrammarSym("S", false));
        test_seq.Add(new GrammarSym("N", false));
        test_seq.Add(new GrammarSym("T", true));

        var fseq = g.FirstSeq(test_seq, "END");
        Console.WriteLine("Firstseq:");
        foreach (var i in fseq) {
            Console.WriteLine(i);
        }
        var itemSet = new SortedSet<Gitem>(new GitemComparer());
        itemSet.Add(new Gitem(0,0, "S"));
        g.StateClosure(itemSet);
        
    }
}// Grammar class

// internal class GitemComparer : IComparer<Gitem> {
//     public int Compare(Gitem x, Gitem y)
//     {
//         var expr = (x.Ri*65536/2 + x.Pi) - (y.Ri*65536/2 + y.Pi);
//         if (expr == 0)
//         {
//             return x.La.CompareTo(y.La);
//         }
//         return expr;
//     }
// }

// public class Gitem : IComparable {
//     public short Ri { get; set; } // rule index into metaparser.Rules
//     public short Pi { get; set; } // position of dot
//     public string La { get; set; }

//     public Gitem(int ri, int pi)
//     {
//         Ri = (Int16)ri;
//         Pi = (Int16)Pi;
//     }

//      public override bool Equals(object b) // equals should be consistent with compare
//     {
// 	    return CompareTo((Gitem)b) == 0;
//     }

//     public int CompareTo(object I)
//     {
//         if (I == null) return 1;

//         Gitem other = I as Gitem;
//         if (other != null) {
//           var expr = (Ri*65536/2 + Pi) - (other.Ri*65536/2 + other.Pi);
//           if (expr == 0) {
//             return La.CompareTo(other.La);
//           }
//           return expr;
//         }
//         else
//            throw new ArgumentException("Object is not a Gitem");
        
//     }

//     public override int GetHashCode() // Generates warning without this function
//     {
//         return Ri.GetHashCode() ^ Pi.GetHashCode() ^ La.GetHashCode();
//     }
//}
