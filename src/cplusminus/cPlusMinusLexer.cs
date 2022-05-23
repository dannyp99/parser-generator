using System;
public class CPlusMinusLexer : simpleLexer
{
    public CPlusMinusLexer() {}
    public CPlusMinusLexer(string s): base(s) {}
    public CPlusMinusLexer(string a, string b): base(a,b) {}

    public override lexToken next() {
        var tok = base.next();
        return translate_token(tok);
    }

    public override lexToken translate_token(lexToken t)
    {
        if (t.token_type == "Integer") { t.token_type = "string";} 
        else if (t.token_type == "Symbol") { 
            if((string)t.token_value ==":") {
                t.token_type = "COLON";
            }
            else {
                t.token_type = (string)t.token_value; 
            }
        }
        else if (t.token_type == "Alphanumeric") { 
            t.token_type = (string) t.token_value;
        } 
        else if (t.token_type =="Keyword") { t.token_type = (string)t.token_value;}
        else if (t.token_type =="StringLiteral") { t.token_type = "EXPR";}
        return t; 
    }
}