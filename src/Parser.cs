using System;
using System.Collections.Generic;
using System.IO; //For RawParse function


public class RGrule 
{ 
    public string Lhs;
    public Func<Stack<StackElement<object>>,object> RuleAction;

    public RGrule() {}
    public RGrule(string lh)
    {
        Lhs=lh;
        RuleAction = (p) => {return new object();};
    }
}

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
        return String.Format("Index: {0} with value {1}", Si, Value.ToString());
    }
}

public class Parser<Object>  
{
    public List<Dictionary<string,IStateAction>> RSM;
    public List<RGrule> Rules;
    public string ReSyncSymbol { get; set; }

    public Parser(int rlen, int slen) 
    {
        RSM = new List<Dictionary<string,IStateAction>>(slen);
        Rules = new List<RGrule>(rlen);

        for(int i = 0; i < slen; i++) {
            RSM.Add(new Dictionary<string,IStateAction>(1024));
        }
    }
    public void RawParse(simpleLexer tokenizer){
        bool TRACE = false;
        List<lexToken> ToTranslate = new List<lexToken>();
        using( StreamWriter sw = new StreamWriter("./RawParse.txt")) {
            var token = tokenizer.next();
            while(token != null) {
                if(TRACE){
                    Console.WriteLine("Token = " + token);
                }
                sw.Write(token + "\n");
                ToTranslate.Add(token);
                token = tokenizer.next();
            }
        }
    }
//absLexer
    public object Parse(absLexer tokenizer) //used to be simpleLexer, GrammarLexer conversion
    {
        bool TRACE = false;
        //1 absLexer abstractLex = new concreteLexer();
        object result = default(object);
        Stack<StackElement<object>> stack = new Stack<StackElement<object>>(8*1024);

        stack.Push(new StackElement<object>(0,default(object)));
        //Error handling line
        IStateAction unexpected = new Error("Unexpected end of input");
        // action is error until it isnt
        IStateAction action = unexpected;
        bool stopparsing = false;
        var lookahead = tokenizer.next();
        if(TRACE) {
            //Console.WriteLine("*** First token: "+nextoken);        
        }
        if(TRACE) {
            Console.WriteLine("*** First translated token: "+lookahead);                
        }
        if(lookahead == null) { stopparsing = true; }
        //abstractLex.translate_token(lookahead);
        if(TRACE) {
            Console.WriteLine("lookahead " + lookahead.token_type);
        }

        while(!stopparsing) {
            //Console.WriteLine(stack.Peek());
            int currentState = stack.Peek().Si;
            var stackEl = stack.Peek();
            if(TRACE) {
                Console.Write("State "+currentState+", lookahead ("+lookahead.token_type+"): ");            
            }
            IStateAction actionopt;
            if(TRACE){ Console.WriteLine("Retrieving actionopt");}
            RSM[currentState].TryGetValue(lookahead.token_type, out actionopt);
            if(actionopt == null) { // Enter Resync Recovery Mode
                Console.WriteLine("Unexpected token type" + lookahead.token_type + " with value " + lookahead.token_value + "\nOn line " + tokenizer.linenum() + "\nCurrent State: " + currentState  );
                while(lookahead.token_type != ReSyncSymbol && lookahead.token_type != "EOF") {
                    //1 nextoken = tokenizer.next(); 
                    //1 lookahead = abstractLex.translate_token(nextoken);
                    lookahead = tokenizer.next();
                }

                if(lookahead.token_type !="EOF"){
                    //1 nextoken = tokenizer.next();
                    //1 lookahead = abstractLex.translate_token(nextoken);
                    lookahead = tokenizer.next();
                }
                
                if(TRACE) { Console.WriteLine("Popping stack...");}
                while( actionopt == null && stack.Count > 0) {
                    stackEl = stack.Pop();
                    if(TRACE){
                        if(stackEl.Value != null){   
                          Console.WriteLine("StackElement:: " + stackEl);
                        }
                    }
                    currentState = stackEl.Si;
                    RSM[currentState].TryGetValue(lookahead.token_type, out actionopt);
                }
                if(TRACE) { Console.WriteLine("----BOTTOM OF STACK----"); }
                
                if(actionopt == null) {
                    Console.WriteLine("Parsing Failed");
                    return null;
                }

                stack.Push(stackEl);
            } // End Error Handling
            if (TRACE) {
                Console.WriteLine("Sematic Action is " + actionopt);
            }
            action = actionopt;
            if(action is Shift) { // being "match"
                if(TRACE) {
                    Console.WriteLine("Shifting to state "+action.Next);
                }
                stack.Push(new StackElement<object>(action.Next,lookahead.token_value));
                if (lookahead.token_type!="EOF") {
                  lookahead = tokenizer.next();
                  //1 nextoken = tokenizer.next();
                  //Console.WriteLine("Token before translation " + nextoken.token_type);
                  if(TRACE) {
                    //Console.WriteLine("***next token: "+nextoken);
                  }
                  if (lookahead!=null) {//1nextoken!=null) {
                    //1 lookahead = abstractLex.translate_token(nextoken);
                    Console.WriteLine("Token after translation " + lookahead.token_type);
                    if(TRACE) {
                        Console.WriteLine("*** translated nextoken: " + lookahead);
                    }
                  }
                  else  stopparsing=true;
                }// if not at EOF
            }
            else if(action is Reduce) {
                
                RGrule rulei = Rules[action.Next];
                if(TRACE) { Console.WriteLine("Got rulei"); Console.WriteLine(rulei.Lhs);}
                object val = rulei.RuleAction(stack);
                if(TRACE) { Console.WriteLine("Got val");}
                int newtop = stack.Peek().Si; 
                if(TRACE) { Console.WriteLine("Got newtop");}

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