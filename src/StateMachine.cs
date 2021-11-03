using System;
using System.Collections.Generic;
using System.IO;

//used in write_fsm becasue we need state indices and their semantic action combined.

public class StateMachine
{
    public Grammar Grammar { get; set; }
    public List<SortedSet<Gitem>> States { get; set; }
    public Dictionary<string, HashSet<int>> StateLookup { get; set; }
    public List<Dictionary<string,IStateAction>> FSM { get; set; }

    public StateMachine(Grammar g)
    {
        Grammar = g;
        States = new List<SortedSet<Gitem>>(8*1024);
        StateLookup = new Dictionary<string, HashSet<int>>(1024);
        FSM = new List<Dictionary<string, IStateAction>>(8*1024);     
    }

    public void makegotos(short si)
    {
        bool TRACE = true;
        SortedSet<Gitem> state = States[si];
        var newStates = new Dictionary<string, SortedSet<Gitem>>();
        var keyList = new List<string>();
        foreach (Gitem item in state)
        {
            var rule = Grammar.Rules[item.Ri];
            if (item.Pi < rule.Rhs.Count) // Can go to
            {
                if (TRACE) {
                    Console.WriteLine("Item.Pi < rule.Rhs.Count");
                }
                var nextSym = rule.Rhs[item.Pi].Sym;
                if (!newStates.ContainsKey(nextSym))
                {
                    newStates.Add(nextSym, new SortedSet<Gitem>());
                    keyList.Add(nextSym);
                }
                SortedSet<Gitem> symState = newStates[nextSym];
                var newItem = new Gitem(item.Ri, item.Pi+1, item.La);
                symState.Add(newItem);
            }
            else {
                IStateAction currentAction;
                FSM[si].TryGetValue(item.La, out currentAction);
                var change = true;
                if(currentAction != null) {
                    if(TRACE) {
                        Console.Write("Current Action: " + currentAction);  
                    }
                    if (currentAction is Reduce r && r.Next < item.Ri)//simulated pattern matching
                    {   
                        change = false;
                        Console.WriteLine("Reduce-Reduce conflict!");
                        //PrintState()
                    }
                    else if (currentAction is Reduce r2 && r2.Next > item.Ri)
                    {
                        change = false;
                        Console.WriteLine("Reduce-Reduce conflict!");
                        //PrintState()
                    }
                    else if (currentAction is Accept)
                    {
                        change = false;
                        Console.WriteLine("Accept");
                    }
                    else if (currentAction is Shift)
                    {
                        var ruleRiPrec = Grammar.Rules[item.Ri].Precedence;
                        var symRiPrec = Grammar.Symbols[item.La].Precedence;
                        if (ruleRiPrec == symRiPrec && ruleRiPrec < 0) {change = false;}// right associative
                        else if (Math.Abs(symRiPrec) > Math.Abs(ruleRiPrec)) {change = false;}// still shift
                    }
                    else {
                        Console.WriteLine("Pattern Match Nothing");
                    }// pattern matching done
                }
                if (change)
                {   
                    if (item.Ri == Grammar.Rules.Count - 1 )
                    {
                        FSM[si].Add(item.La, new Accept());
                    }
                    else {
                        FSM[si].Add(item.La, new Reduce(item.Ri));
                        //Console.WriteLine("***FSM["+si+"]["+item.La+"]=Reduce by "+item.Ri);                        
                    }
                }// add IStateAction
            }//set reduce action
        }// for each item
        // Pretty print FSM
        foreach (var key in keyList)
        {
            if(newStates.ContainsKey(key))
            {
                Grammar.StateClosure(newStates[key]);//Fill state
                AddState(newStates[key],si,key);
            }
        }
        // pretty print fsm
    }//makegotos

    private bool stateeq(SortedSet<Gitem> s1, SortedSet<Gitem> s2)
    {
        if (s1.Count != s2.Count) return false;
        foreach (Gitem x in s1) {
            if (!s2.Contains(x)) 
                return false;
        }
        return true;
    }
    public void AddState(SortedSet<Gitem> state, short psi, string nextSym)
    {
        bool TRACE = false;
        var indexsave = States.Count; // getting the index of the state
        int toAdd = indexsave;
        for (int i = 0; i < States.Count; i++)
        {
            if (stateeq(state,States[i])) {
                toAdd=i; break;
            }
        }
        if (toAdd == indexsave)
        {
            if(TRACE){
                PrintSet(state);
            }
            States.Add(state);
            FSM.Add(new Dictionary<string, IStateAction>());
        }

        GrammarSym gSymbol = Grammar.Symbols[nextSym];
        IStateAction newAction;
        string actionstr = "Shift("+toAdd+")";
        if (gSymbol.Terminal && gSymbol.Sym !="EOF")  // && gSymbol.Sym!="EOF")
        {
            newAction = new Shift(toAdd);
        }
        else  {
            newAction = new GotoState(toAdd);
            actionstr = "GotoState("+toAdd+")";            
        }
        if (FSM[psi].ContainsKey(nextSym)) {
            FSM[psi][nextSym] =  newAction;
        }
        else {
            FSM[psi].Add(nextSym, newAction);
        }
        if(TRACE){ 
            Console.Write("Adding to FSM ::   ");
            Console.WriteLine("***FSM["+psi+"]["+nextSym+"]="+actionstr);
        }
    } // End AddState

    public void generatefsm()
    {
        bool TRACE = false;
        SortedSet<Gitem> startState = new SortedSet<Gitem>();
        if(TRACE) {
            Console.WriteLine("*** Rules ***");
            foreach(GrammarRule gr in Grammar.Rules) {
                gr.PrintRule();
            }
        }
        startState.Add( new Gitem(Grammar.Rules.Count-1,0,"EOF") );

        if(TRACE) {
            Console.WriteLine("Initial State::  " + startState);
            foreach(Gitem gi in startState) {
                Console.WriteLine(gi);
            }
        }

        Grammar.StateClosure(startState); 

        if(TRACE) {
            Console.WriteLine("Initial State after Closure");
            foreach(Gitem gi2 in startState) {
                Console.WriteLine(gi2);
            }
        }

        States.Add(startState);
        FSM.Add(new Dictionary<string,IStateAction>());

        short closed = 0;
        while(closed < States.Count){ 
            makegotos(closed);
            closed +=1;
            // Console.WriteLine(closed + " : " + States.Count);
        }

    }

    public void PrintSet(SortedSet<Gitem> set)
    {
        foreach (var item in set)
        {
         Console.WriteLine("    " + item);   
        }
    }

    public void prettyPrintFSM(SortedSet<Gitem> states, Grammar grammar)    
    {   
        Gitem state = new Gitem();
        foreach (var item in states) { state = item; break; }
        Console.WriteLine($"state {state}:");
        foreach (Gitem item in states)
        {
            var lhsSym = grammar.Rules[item.Ri].Lhs.Sym;
            var rhs = grammar.Rules[item.Ri].Rhs;
            Console.Write($"    ({item.Ri}) {lhsSym} --> ");
            int i = 0;
            foreach (var gsym in rhs)
            {
                if (i == item.Pi) { Console.Write("."); }
                Console.Write($"{gsym.Sym} ");
                i++;
            }
            if (i == item.Pi) { Console.Write(". "); }
            Console.WriteLine($", {item.La}");
        }
        Console.WriteLine(states.Count);
    }

    public void writefsm(string filename)
    {
        bool TRACE = false;
        
        using (StreamWriter sw = new StreamWriter(filename)) {
            sw.Write("using System;\n");
            sw.Write("using System.Collections;\n");
            sw.Write("using System.Collections.Generic;\n");
            sw.Write("using System.Linq;\n");
            sw.Write("class Generator{\n");
            string TAT = "object"; // can be replaced w/ object

            sw.Write(String.Format("public static Parser<{0}> make_parser()",TAT));
            sw.Write("\n{\n");
            sw.Write(String.Format("Parser<{0}> parser1 = new Parser<{0}>({1},{2});\n",TAT,Grammar.Rules.Count,States.Count));

            sw.Write("RGrule rule = new RGrule(\"start\");\n");
            for(int i = 0; i < Grammar.Rules.Count; i++) {
                if(TRACE){
                    Console.WriteLine(Grammar.Rules.Count);
                }
                sw.Write("rule = new RGrule(\"{0}\");\n",Grammar.Rules[i].Lhs.Sym);
                sw.Write("rule.RuleAction = (pstack) => { ");
                int k = Grammar.Rules[i].Rhs.Count;
                while(k>0) {
                    GrammarSym gsym = Grammar.Rules[i].Rhs[k-1];
                    if(gsym.Label.Length > 0) {
                        sw.Write(" {0} {1} = ({0})pstack.Pop().Value; ",  gsym.FsharpType, gsym.Label);
                    }
                    else {
                        sw.Write("pstack.Pop(); ");
                    }
                    k--;
                } // end Rhs while
                if(TRACE){
                    Console.WriteLine("Exit While");
                    Grammar.Rules[i].PrintRule();
                    Console.WriteLine("Action: [" + Grammar.Rules[i].Action + "]"); //never put in Action to the grammars??
                }
                string semaction = Grammar.Rules[i].Action ?? ""; 
                
                if(TRACE){
                    Console.WriteLine("Begin SemanticAction ifs");
                    Console.WriteLine(semaction);
                }
                if(semaction.Length>1) { 
                    if(TRACE) {
                        Console.WriteLine("semaction.Length > 1");
                    }
                    sw.Write(String.Format("{0};\n",semaction));
                }
                else {
                    sw.Write("return default(object);};\n");   
                }
                sw.Write("parser1.Rules.Add(rule);\n");
            } // end for i in Rules.Count
            
            //int linecs = 0;
            // int cxmax = 512; // number of line before creating a new function??
            for(int i = 0; i < FSM.Count; i++) {
                var row = FSM[i];
                foreach(var key in row.Keys) {
                    if(row[key] is Accept) {
                        sw.Write(String.Format("parser1.RSM[{0}].Add(\"{1}\",new {2}());\n",i,key,row[key]));
                    }
                    else { 
                        sw.Write(String.Format("parser1.RSM[{0}].Add(\"{1}\",new {2}({3}));\n",i,key,row[key],row[key].Next));
                    }
                }
            }
            sw.Write("return parser1;\n}//make_parser\n");
            sw.Write("} // Generator Class");
        } // Using StreamWriter  
    }//writefsm
    
        //bool TRACE = false;
    public static void Main(string[] argv) {
        Grammar g = new Grammar();
        if (argv.Length > 0) {
            g.TRACE = false;
        }
        g.ParseStdin();
        
        if (g.TRACE) {Console.Write("\n");}
        // Console.WriteLine("info:");
        // Console.WriteLine("topsym: " + g.TopSym);
        // foreach (var rule in g.Rules) {
        //     rule.PrintRule();
        // }
        
        g.ComputeFirst();
        g.PrintFirst();
        g.PrintNullable();
        // Console.WriteLine("GrammarSym:" + g.Rules[0].Rhs[0]);
        
        var itemSet = new SortedSet<Gitem>(new GitemComparer());
        g.StateClosure(itemSet);
        StateMachine sm = new StateMachine(g);
        sm.generatefsm();
        
        //for(int i=0;i<sm.States.Count;i++)
          //{sm.prettyPrintFSM(sm.States[i], g);  Console.WriteLine("---State "+i+" above-------"); }
        string testpath = "./writefsmTests/par.cs";
        sm.writefsm(testpath);

        if(argv.Length == 1) { 
            string srcfile = "./" + argv[0];
            simpleLexer SLexer = new simpleLexer(srcfile, "EOF");
            Parser<object> Par = Generator.make_parser();
            var t = Par.Parse(SLexer);
            Console.WriteLine("Result: "+t); 
        }
        else {
            Console.WriteLine("There is no given test file to parse. the Parser has been generated in ./writefsm");
        } 
    }//main
}
