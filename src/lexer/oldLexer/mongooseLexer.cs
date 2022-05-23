using System;
using static FSEvaluator;
public class concreteLexer : simpleLexer {

    public concreteLexer() {}
    public concreteLexer(string s) : base(s) {}
    public concreteLexer(string a, string b): base(a,b) {}

    // an issue we have been having is due to invalid type casts of F# Discriminated Unions to C# primitive types.
    // Greg suspects the issue is do to having with changeing the token_value to F# DU and not keepin them as they are
    public override lexToken translate_token(lexToken t)
    {
        if (t.token_type == "Integer") { t.token_type = "Val"; t.token_value = NewVal((int)t.token_value); } 
        else if (t.token_type == "Symbol") { 
            if((string)t.token_value ==":"){
                t.token_type = "COLON";
            }
            else{
                t.token_type = (string)t.token_value; 
            } 
        }
        else if (t.token_type == "Alphanumeric") { 
            if((string)t.token_value == "EOF"){
                t.token_type = "EOF";
                t.token_value = NewFSNothing;
            }
            else if((string)t.token_value =="cout") {
              t.token_type = (string)t.token_value;
              t.token_value = NewStr((string)t.token_value);
            }
            else if((string)t.token_value =="cin") { 
              t.token_type = (string)t.token_value;
            }
            else{
                t.token_type = "Var"; 
            }
        } 
        else if (t.token_type =="Keyword") { t.token_type = (string)t.token_value;}
        else if (t.token_type =="StringLiteral") { t.token_type = "Str"; t.token_value = NewStr((string)t.token_value); }
        Console.WriteLine(t);
        return t;
    }
}
