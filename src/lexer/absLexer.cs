public interface absLexer
{
   lexToken next(); // returns null at eof. Should return a token of the appropriate type
   int linenum();
   //lexToken translate_token(lexToken t);
}

public class lexToken
{
   public string token_type;
   public object token_value;
   public lexToken(string t, object v) {token_type=t; token_value=v;}
   public override string ToString() {return token_type+"("+token_value+")";}
}
