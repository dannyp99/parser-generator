using System;
using System.Collections.Generic;

public class StackElement<Object>
{
    public int Si {get; set;}
    public object Value {get; set;}

    public StackElement(int si, object val)
    {
        Si = si;
        Value = val;
    }

    public override string ToString()
    {
        return String.Format("Index: {0} with value {1}", Si, (string) Value);
    }
}

public class Parser<Object>
{
    public List<Dictionary<string,IStateAction>> RSM;
    public List<RGrule> Rules;

    public Parser(int rlen, int slen) 
    {
        RSM = new List<Dictionary<string,IStateAction>>(slen);
        Rules = new List<RGrule>(rlen);

        for(int i = 0; i < slen; i++) {
            RSM.Add(new Dictionary<string,IStateAction>(1024));
        }
    }

    //TODO parse
    // line 1064 of Rust
    public object Parse(simpleLexer tokenizer)
    {
        bool TRACE = false;
        absLexer abstractLex = new concreteLexer();
        object result = default(object);
        Stack<StackElement<object>> stack = new Stack<StackElement<object>>(8*1024);

        stack.Push(new StackElement<object>(0,default(object)));
        //Error handling line
        IStateAction unexpected = new Error("Unexpected end of input");
        // action is error until it isnt
        IStateAction action = unexpected;
        bool stopparsing = false;
        var nextoken = tokenizer.next();
        if(TRACE) {
            Console.WriteLine("*** First token: "+nextoken);        
        }
        lexToken lookahead = abstractLex.translate_token(nextoken);
        if(TRACE) {
            Console.WriteLine("*** First translated token: "+lookahead);                
        }
        if(lookahead == null) { stopparsing = true; }
        abstractLex.translate_token(lookahead);
        if(TRACE) {
            Console.WriteLine("lookahead " + lookahead.token_type);
        }

        while(!stopparsing) {
            //Console.WriteLine(stack.Peek());
            int currentState = stack.Peek().Si;
            if(TRACE) {
                Console.Write("State "+currentState+", lookahead ("+lookahead.token_type+"): ");            
            }
            IStateAction actionopt = RSM[currentState][lookahead.token_type];
            action = actionopt;
            if(action is Shift) { // being "match"
                if(TRACE) {
                    Console.WriteLine("Shifting to state "+action.Next);
                }
                stack.Push(new StackElement<object>(action.Next,lookahead.token_value));
                if (lookahead.token_type!="EOF") {
                  nextoken = tokenizer.next();

                  if(TRACE) {
                    Console.WriteLine("***next token: "+nextoken);
                  }
                  if (nextoken!=null)
                    lookahead = abstractLex.translate_token(nextoken);
                  else  stopparsing=true;
                }// if not at EOF
            }
            else if(action is Reduce) {
                RGrule rulei = Rules[action.Next];
                object val = rulei.RuleAction(stack);
                int newtop = stack.Peek().Si; 

                if(TRACE) {
                    Console.Write("Reduce by rule "+action.Next+",  ");
                    Console.Write("   : RuleAction value " + val);
                    Console.Write("   : newtop " + newtop + " Lhs  :" + rulei.Lhs);
                }

                IStateAction goton = RSM[newtop][rulei.Lhs];
            
                if(goton is GotoState) {
                    if(TRACE) {
                        Console.WriteLine("    : Goto state "+goton.Next);
                    }
                    stack.Push(new StackElement<object>(goton.Next, val));
                }
                else { stopparsing = true;}
            }
            else if(action is Accept) {
                if(TRACE) {
                    Console.WriteLine("Accept");
                }
                // result  = stack.Pop().Value;
                result  = stack.Pop().Value;                
                stopparsing = true;
            }
            else if(action is Error) {
                Console.WriteLine("Error");
                stopparsing = true;
            }
            else if(action is GotoState) {
                if(TRACE) {
                    Console.WriteLine("GotoState");
                }

                StackElement<object> oldTop = stack.Pop();
                stack.Push(new StackElement<object>(action.Next,oldTop.Value));

                // stopparsing = true;
            } //end "match"            
        } // while loop

        if(action is Error) {
            Error err = (Error)action;
            throw new ArgumentException(String.Format("Parsing failed on line {0}, next symbol {1}: {2}\n", tokenizer.linenum(),lookahead.token_type,err.Message));
        }
        return result;
    } //parse
}