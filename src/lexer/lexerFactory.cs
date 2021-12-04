// This is an attempt to streamline the need for a concrete lexer.
// The creature of this class is dependent on the simple lexer we have be given by Dr. Liang
// So this class is the bridge between a given lexical analyzer and the translation to a grammar tokens 
using static FSEvaluator; // for the Discriminated Union types

public class GrammarLexer : absLexer 
{
  public simpleLexer SLexer;

  public GrammarLexer(string srcfile) {
    SLexer = new simpleLexer(srcfile,"EOF");
  }  
  // Now a functional question is we can keep the seperation of next and translate OR we can do
    // translate_token(SLexer.next()) and all we do is call .next() in Parser.cs to make it cleaner looking
    // the two ways to do this are:
      // as is implemented
      // just change abslexer to not require translate_token() and do the translation in next() anyway

  public lexToken next(){ return translate_token(SLexer.next()); } 
 
  public int linenum(){ return SLexer.linenum(); }

  public lexToken translate_token(lexToken t) 
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
    return t; 
  }
}