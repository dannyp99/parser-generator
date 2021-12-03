/*
    This simple lexical analyzer distinguishes between keywords, symbols,
    alphanumeric words, integer, floating-point and string literals.

    Compile with absLexer.cs

    New keywords can be added to the default with the addKeyword function.
    Multi-character symbols such as "==", "!=", etc can be added
    to the defaults with the addMultichar function.

    There are two constructors to the simpleLexer class:
    
    One takes a single string and splits the string using the regular
    expression operators_re.

    The other constructor takes a string representing a file name and a
    'end-of-file' symbol that is inserted into the string at the end of each
    line (unless it's set to null, which is the usual case).  It
    reads all lines from the file into an array of lines.  The next()
    function will go to the next line and update the linenum() attribute
    as needed.

    The next() function returns null if no more tokens are available.  
    Otherwise, it returns a lexToken object with the token_type set to one of:

    "Symbol"   (non-alphanumeric symbols such as *, +, ==, etc )
    "Keyword"  (while, if ,else, etc)
    "Alphanumeric"     (alphanumeric  x, x1, etc)
    "Integer"  (base-10 non-negative integers, - is a separate symbol)
    "Float"    (non-negative doubles 3.15)
    "StringLiteral" (double-quoted strings without nested ""'s)

    with the token_value set to the object of the appropriate type.
*/

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class simpleLexer : absLexer
{
  public static string operators_re =
    "([()\\+\\-\\*/:;%^<>!,]|\\s|\\[|\\]|{|}|=|\"[^\"]*\")";

  public string endofline = null; //"ENDL";
  // optional endofline marker
  // special string to mark new line
  // should not be confused with other classes of tokens in object-language.

  public char commentchar = '#';  // lines starting with this are ignored

  string[] keywords = {"if","else","while","let","for","lambda"};
  string[] multichar_syms = {"==","<=",">=","!=", "++","--","&&","||", "<<", ">>"};

  HashSet<string> kwhash = new HashSet<string>();
  Dictionary<char,string> mshash = new Dictionary<char,string>();
  
  public void addKeyword(string kw)
  {  kwhash.Add(kw);  }
  public void addMultichar(string ms)
  {
    if (ms.Length>1) mshash[ms[0]] = ms.Substring(1,ms.Length-1);
    //Console.WriteLine("adding ms "+ms[0]+" to "+ms.Substring(1,ms.Length-1));
  }

  protected string[] lines;
  protected string[] ssplit; 
  protected int ti=0; // token position in ssplit
  protected int linenumber=0;  // subtract 1 to use as index
  public simpleLexer() {}
  public simpleLexer(string s) // make lexer from string
  {
     foreach (string kw in keywords) addKeyword(kw);
     foreach (string ms in multichar_syms) addMultichar(ms);
     lines = new string[1]; lines[0] = s;  linenumber = 1;
     ssplit = Regex.Split(s,operators_re);
  }//constructor

  public simpleLexer(string filename, string eof) // read from file
  {
     bool TRACE = false;
     foreach (string kw in keywords) addKeyword(kw);
     foreach (string ms in multichar_syms) addMultichar(ms);  
     //endofline = eof;
     var tempLines = new List<string>(System.IO.File.ReadAllLines(filename));
     if(TRACE){
        Console.WriteLine("Raw File Lines from simplexLexer:");
        foreach(string s in tempLines){
           Console.WriteLine(s);
        }
     }
     tempLines.Add("EOF");
     lines = tempLines.ToArray();
     linenumber = 0;
     newline();
     //ssplit = Regex.Split(line[0],operators_re);     
  }//constructor from file

  bool newline()
  {
     string s = "";
     while (linenumber<lines.Length && (s==null || s.Length<1 || s[0]==commentchar))  {
         linenumber++;
         s = lines[linenumber-1];
         if (s!=null) s = s.Trim();
       }
     if (s!=null && s.Length>0 && s[0]!=commentchar) {
        if (endofline!=null && endofline.Length>1)
           s = s+ " " + endofline; // This line is cause endofline to appear after lines
        ssplit = Regex.Split(s,operators_re); // This line is causeing endofline to appear after lines
        ti = 0; // token index in ssplit
        return true;
     }
     else return false;
  }//newline

  static bool alphabetical(char x) => (x >=65 && x<=90) || (x>=97 && x<=122);

  public lexToken next()
  {
     string tok = "";
     if (ssplit==null) return null;
     while (tok==null || tok.Length<1)
     {
        if (ti>=ssplit.Length) {
           if (!newline()) return null;
        }
        else tok = ssplit[ti++].Trim();
     }
     lexToken ax = new lexToken("Symbol",tok);
     try {
      if (kwhash.Contains(tok)) {ax = new lexToken("Keyword",tok); }
      else if (tok[0]=='\"' && tok[tok.Length-1]=='\"')
       { ax = new lexToken("StringLiteral", tok); }
      else if (alphabetical(tok[0])) {ax = new lexToken("Alphanumeric",tok);}
      else if ((int)tok[0]==(int)'.' || ((int)tok[0]>=48 && (int)tok[0]<=57)) {
       try {
          int n = int.Parse(tok);
          ax = new lexToken("Integer",n);
       } catch (Exception) { ax= new lexToken("Float", double.Parse(tok)); }
      }
      else if (tok.Length==1 && ti<ssplit.Length) { // check for multichar sym
        string rest= mshash[tok[0]]; 
        string nexttok =ssplit[ti];
        while ((nexttok==null || nexttok.Length<1) && ti<ssplit.Length-1)
          { nexttok = ssplit[++ti]; } // won't look past current line
        //Console.WriteLine("see "+tok+" and "+rest+" and nexttok "+nexttok.Length+".");
        if (rest==nexttok) {ax= new lexToken("Symbol",tok+nexttok); ti++;}
      }
     }
     catch (Exception) {ax = new lexToken("Symbol",tok);}
     return ax;
  }//next

  public int linenum() {return linenumber;}
  public virtual lexToken translate_token(lexToken t) { return t; }


  /////// main for testing
  public static void Main(string[] argv)
  {
     string ax = "while (1) fork();";
     if (argv.Length>0) ax = argv[0];
     absLexer scanner = new simpleLexer(ax);
     lexToken token;
     do {
        token = scanner.next();
        if (token!=null) Console.WriteLine("lexToken: "+token);
     }
     while (token!=null);

     Console.WriteLine("\ntesting file input from lexertest.txt..");
     scanner = new simpleLexer("simpleTest.txt",null);
     do {
        token = scanner.next();
        if (token!=null) Console.WriteLine("Token from file: "+token);
     }
     while (token!=null);
     Console.WriteLine("line number at "+scanner.linenum());
     
  }//main
}//simpleLexer

/*  sample run:
$ mcs simpleLexer.cs absLexer.cs
$ mono simpleLexer.exe "let x2 = 10.0==30-20; print(\"hello world\");"
lexToken: Keyword(let)
lexToken: Alphanumeric(x2)
lexToken: Symbol(=)
lexToken: Float(10)
lexToken: Symbol(==)
lexToken: Integer(30)
lexToken: Symbol(-)
lexToken: Integer(20)
lexToken: Symbol(;)
lexToken: Alphanumeric(print)
lexToken: Symbol(()
lexToken: StringLiteral("hello world")
lexToken: Symbol())
lexToken: Symbol(;)
*/
