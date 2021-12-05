using System; //consoles
using static FSEvaluator; //needed for expr
class Driver {
  public static void Main(string[] argv){
    string srcfile = "./cplusminus/cmpError.txt";
    if(argv.Length >= 1) { srcfile = argv[0]; }
      var Par = Generator.make_parser(); 
      if(Par != null) {
        var t = Par.Parse(new CPlusMinusLexer(srcfile, "EOF"));
        if(t != null) {
            //FSPrint(t);
            // run(t);
            Console.WriteLine("Result: "+t); 
        }
      }
  }
}