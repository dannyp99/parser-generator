using System;
using System.Collections.Generic;

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
        SortedSet<Gitem> state = States[si];
        var newStates = new Dictionary<string, SortedSet<Gitem>>();
        var keyList = new List<string>();
        foreach (Gitem item in state)
        {
            var rule = Grammar.Rules[item.Ri];
            if (item.Pi < rule.Rhs.Count) // Can go to
            {
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
                    if (currentAction is Reduce r && r.Rulei < item.Ri)//simulated pattern matching
                    {   
                        change = false;
                        Console.WriteLine("Reduce-Reduce conflict!");
                        //PrintState()
                    }
                    else if (currentAction is Reduce r2 && r2.Rulei > item.Ri)
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
                    if (item.Ri == Grammar.Rules.Count - 1 && item.La == "EOF")
                    {
                        FSM[si].Add(item.La, new Accept());
                    }
                    else {
                        FSM[si].Add(item.La, new Reduce(item.Ri));
                    }
                }// add IStateAction
            }//set reduce action
        }// for each item
        foreach (var key in keyList)
        {
            if(newStates.ContainsKey(key))
            {
                Grammar.StateClosure(newStates[key]);//Fill state
                AddState(newStates[key],si,key);
            }
        }
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
            Console.WriteLine("Adding state:" );
            PrintSet(state);
            States.Add(state);
            FSM.Add(new Dictionary<string, IStateAction>());
        }

        GrammarSym gSymbol = Grammar.Symbols[nextSym];
        IStateAction newAction;
        if (gSymbol.Terminal)
        {
            newAction = new Shift(toAdd);
        }
        else {
            newAction = new GotoState(toAdd);
        }
        if (FSM[psi].ContainsKey(nextSym)) {
            FSM[psi][nextSym] =  newAction;
        }
        else {
            FSM[psi].Add(nextSym, newAction);
        }
    } // End AddState

    public void generatefsm()
    {
        SortedSet<Gitem> startState = new SortedSet<Gitem>();
        startState.Add( new Gitem(Grammar.Rules.Count-1,0,"EOF") );
        Grammar.StateClosure(startState); 

        States.Add(startState);
        FSM.Add(new Dictionary<string,IStateAction>());

        short closed = 0;
        while(closed < States.Count){ 
            makegotos(closed);
            closed +=1;
            //Console.WriteLine(closed + " : " + States.Count);
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
    }

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
        g.PrintNullable();

        

        var itemSet = new SortedSet<Gitem>(new GitemComparer());
        g.StateClosure(itemSet);
        StateMachine sm = new StateMachine(g);
        sm.generatefsm();
        sm.prettyPrintFSM(sm.States[0], g);
        Console.WriteLine(sm.States.Count);
    }
}
