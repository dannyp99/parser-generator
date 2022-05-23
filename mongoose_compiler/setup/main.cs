using System; // consoles
using System.IO; // file writing
using static MongooseCompiler;
class Driver {
  public static void Main(string[] argv){
    if(argv.Length < 1) {
      Console.WriteLine("Error, requires input file"); 
      return;
    }
    string srcfile = argv[0];
    var Par = Generator.make_parser(); 
    if(Par != null) {
      expr t = (expr)Par.Parse(new MongooseLexer(srcfile, "EOF"));
      if(t != null) {
        string output = compile(t);
        string path = "./a.ll";
        using (StreamWriter sw = File.CreateText(path)) {
          sw.Write(output);
        }
      }
    }
  }
}
