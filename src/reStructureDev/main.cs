using System; //consoles
using static FSEvaluator; //needed for expr
class Driver {
  public static void Main(string[] argv){
    string srcfile = "./test1.ms";
    string lexType = "mongoose";
    if(argv.Length >= 1) { srcfile = argv[0]; }
    LexerFactory factory = null; //new MongooseFactory(srcfile, "EOF");
      switch (lexType.ToLower())
      {
        case "calc":
          factory = new CalcFactory(srcfile, "EOF");
          break;
        case "cplusminus":
          factory = new CPlusMinusFactory(srcfile, "EOF");
          break;
        case "mongoose":
          factory = new MongooseFactory(srcfile, "EOF");            
          break;
        default:
          break;
      }
      simpleLexer SLexer = factory.GetSimpleLexer();
      var Par = Generator.make_parser(); 
      if(Par != null) {
        expr t = (expr)Par.Parse(SLexer);
        if(t != null) {
            //FSPrint(t);
            // run(t);
            Console.WriteLine("Result: "+t); 
        }
      }
  }
}