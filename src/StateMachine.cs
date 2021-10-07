using System;
using System.Collections.Generic;
using State;

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
    public void makegotos(int si)
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
                var leftSym = Grammar.Rules[item.Ri].Lhs.Sym;
                symState.Add(newItem);
            }
            else {
                IStateAction currentAction = FSM[si][item.La];
                var change = true;
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
                }
                else if (currentAction is Shift)
                {
                    var ruleRiPrec = Grammar.Rules[item.Ri].Precedence;
                    var symRiPrec = Grammar.Symbols[item.La].Precedence;
                    if (ruleRiPrec == symRiPrec && ruleRiPrec < 0) {change = false;}// right associative
                    else if (Math.Abs(symRiPrec) > Math.Abs(ruleRiPrec)) {change = false;}// still shift
                }// pattern matching done
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
            }
        }
    }//makegotos
}