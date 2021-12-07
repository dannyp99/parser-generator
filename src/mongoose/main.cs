using System; //consoles
using static FSEvaluator; //needed for expr
class Driver {
  public static void Main(string[] argv){
    string srcfile = "./test1.ms";
    if(argv.Length >= 1) { srcfile = argv[0]; }
    var Par = Generator.make_parser(); 
    if(Par != null) {
      expr t = (expr)Par.Parse(new MongooseLexer(srcfile, "EOF"));
      if(t != null) {
        //FSPrint(t);
        // run(t);
        Console.WriteLine("Result: "+t); 
      }
    }
  }
}
