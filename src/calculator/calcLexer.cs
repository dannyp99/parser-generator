using System;
using static FSEvaluator;
public class CalcLexer : simpleLexer {

    public CalcLexer() {}
    public CalcLexer(string s) : base(s) {}
    public CalcLexer(string a, string b): base(a,b) {}

     public override lexToken next() {
        var tok = base.next();
        return translate_token(tok);
    }

    public override lexToken translate_token(lexToken t)
    {
        if (t.token_type == "Integer") { t.token_type = "int"; t.token_value = NewVal((int)t.token_value); }
        else if (t.token_type == "Symbol") {
            t.token_type = (string) t.token_value;
        }
        else if (t.token_type == "Alphanumeric") {t.token_type = (string)t.token_value;}
        else if (t.token_type =="Keyword") { t.token_type = (string)t.token_value;}
        Console.WriteLine(t);
        return t;
    }
}
