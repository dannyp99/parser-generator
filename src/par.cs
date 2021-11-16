using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static FSEvaluator;

class Generator{
public static Parser<string> make_parser()
{
Parser<string> parser1 = new Parser<string>(11,32);
RGrule rule = new RGrule("start");
rule = new RGrule("STATLIST");
rule.RuleAction = (pstack) => { pstack.Pop(); return default(object);};
parser1.Rules.Add(rule);
rule = new RGrule("STATLIST");
rule.RuleAction = (pstack) => { pstack.Pop(); pstack.Pop(); return default(object);};
parser1.Rules.Add(rule);
rule = new RGrule("STAT");
rule.RuleAction = (pstack) => { pstack.Pop(); pstack.Pop(); pstack.Pop(); return Console.ReadLine(); };
parser1.Rules.Add(rule);
rule = new RGrule("STAT");
rule.RuleAction = (pstack) => {  string s = (string)pstack.Pop().Value; pstack.Pop(); pstack.Pop(); Console.WriteLine(": " + s); return ""; };
parser1.Rules.Add(rule);
rule = new RGrule("EXPR");
rule.RuleAction = (pstack) => { pstack.Pop(); return "x"; };
parser1.Rules.Add(rule);
rule = new RGrule("EXPR");
rule.RuleAction = (pstack) => { pstack.Pop(); return "y"; };
parser1.Rules.Add(rule);
rule = new RGrule("EXPR");
rule.RuleAction = (pstack) => { pstack.Pop(); return "z"; };
parser1.Rules.Add(rule);
rule = new RGrule("EXPR");
rule.RuleAction = (pstack) => { pstack.Pop();  string s = (string)pstack.Pop().Value; pstack.Pop(); return s; };
parser1.Rules.Add(rule);
rule = new RGrule("EXPRLIST");
rule.RuleAction = (pstack) => {  string s = (string)pstack.Pop().Value; return s; };
parser1.Rules.Add(rule);
rule = new RGrule("EXPRLIST");
rule.RuleAction = (pstack) => {  string s = (string)pstack.Pop().Value; pstack.Pop();  string sl = (string)pstack.Pop().Value; return String.Format("{0} {1}",sl,s); };
parser1.Rules.Add(rule);
rule = new RGrule("START");
rule.RuleAction = (pstack) => { pstack.Pop(); pstack.Pop(); return default(object);};
parser1.Rules.Add(rule);
parser1.RSM[0].Add("STATLIST",new GotoState(1));
parser1.RSM[0].Add("STAT",new GotoState(2));
parser1.RSM[0].Add("cin",new Shift(3));
parser1.RSM[0].Add("cout",new Shift(4));
parser1.RSM[1].Add("EOF",new GotoState(5));
parser1.RSM[1].Add("STAT",new GotoState(6));
parser1.RSM[1].Add("cin",new Shift(3));
parser1.RSM[1].Add("cout",new Shift(4));
parser1.RSM[2].Add("EOF",new Reduce(0));
parser1.RSM[2].Add("cin",new Reduce(0));
parser1.RSM[2].Add("cout",new Reduce(0));
parser1.RSM[3].Add(">>",new Shift(7));
parser1.RSM[4].Add("<<",new Shift(8));
parser1.RSM[5].Add("EOF",new Accept());
parser1.RSM[6].Add("EOF",new Reduce(1));
parser1.RSM[6].Add("cin",new Reduce(1));
parser1.RSM[6].Add("cout",new Reduce(1));
parser1.RSM[7].Add("EXPR",new GotoState(9));
parser1.RSM[7].Add("x",new Shift(10));
parser1.RSM[7].Add("y",new Shift(11));
parser1.RSM[7].Add("z",new Shift(12));
parser1.RSM[7].Add("(",new Shift(13));
parser1.RSM[8].Add("EXPRLIST",new GotoState(14));
parser1.RSM[8].Add("EXPR",new GotoState(15));
parser1.RSM[8].Add("x",new Shift(16));
parser1.RSM[8].Add("y",new Shift(17));
parser1.RSM[8].Add("z",new Shift(18));
parser1.RSM[8].Add("(",new Shift(19));
parser1.RSM[9].Add("cout",new Reduce(2));
parser1.RSM[9].Add("cin",new Reduce(2));
parser1.RSM[9].Add("EOF",new Reduce(2));
parser1.RSM[10].Add("EOF",new Reduce(4));
parser1.RSM[10].Add("cin",new Reduce(4));
parser1.RSM[10].Add("cout",new Reduce(4));
parser1.RSM[11].Add("EOF",new Reduce(5));
parser1.RSM[11].Add("cin",new Reduce(5));
parser1.RSM[11].Add("cout",new Reduce(5));
parser1.RSM[12].Add("EOF",new Reduce(6));
parser1.RSM[12].Add("cin",new Reduce(6));
parser1.RSM[12].Add("cout",new Reduce(6));
parser1.RSM[13].Add("EXPR",new GotoState(20));
parser1.RSM[13].Add("x",new Shift(21));
parser1.RSM[13].Add("y",new Shift(22));
parser1.RSM[13].Add("z",new Shift(23));
parser1.RSM[13].Add("(",new Shift(24));
parser1.RSM[14].Add("cout",new Reduce(3));
parser1.RSM[14].Add("cin",new Reduce(3));
parser1.RSM[14].Add("EOF",new Reduce(3));
parser1.RSM[14].Add("<<",new Shift(25));
parser1.RSM[15].Add("EOF",new Reduce(8));
parser1.RSM[15].Add("<<",new Reduce(8));
parser1.RSM[15].Add("cin",new Reduce(8));
parser1.RSM[15].Add("cout",new Reduce(8));
parser1.RSM[16].Add("<<",new Reduce(4));
parser1.RSM[16].Add("EOF",new Reduce(4));
parser1.RSM[16].Add("cin",new Reduce(4));
parser1.RSM[16].Add("cout",new Reduce(4));
parser1.RSM[17].Add("<<",new Reduce(5));
parser1.RSM[17].Add("EOF",new Reduce(5));
parser1.RSM[17].Add("cin",new Reduce(5));
parser1.RSM[17].Add("cout",new Reduce(5));
parser1.RSM[18].Add("<<",new Reduce(6));
parser1.RSM[18].Add("EOF",new Reduce(6));
parser1.RSM[18].Add("cin",new Reduce(6));
parser1.RSM[18].Add("cout",new Reduce(6));
parser1.RSM[19].Add("EXPR",new GotoState(26));
parser1.RSM[19].Add("x",new Shift(21));
parser1.RSM[19].Add("y",new Shift(22));
parser1.RSM[19].Add("z",new Shift(23));
parser1.RSM[19].Add("(",new Shift(24));
parser1.RSM[20].Add(")",new Shift(27));
parser1.RSM[21].Add(")",new Reduce(4));
parser1.RSM[22].Add(")",new Reduce(5));
parser1.RSM[23].Add(")",new Reduce(6));
parser1.RSM[24].Add("EXPR",new GotoState(28));
parser1.RSM[24].Add("x",new Shift(21));
parser1.RSM[24].Add("y",new Shift(22));
parser1.RSM[24].Add("z",new Shift(23));
parser1.RSM[24].Add("(",new Shift(24));
parser1.RSM[25].Add("EXPR",new GotoState(29));
parser1.RSM[25].Add("x",new Shift(16));
parser1.RSM[25].Add("y",new Shift(17));
parser1.RSM[25].Add("z",new Shift(18));
parser1.RSM[25].Add("(",new Shift(19));
parser1.RSM[26].Add(")",new Shift(30));
parser1.RSM[27].Add("EOF",new Reduce(7));
parser1.RSM[27].Add("cin",new Reduce(7));
parser1.RSM[27].Add("cout",new Reduce(7));
parser1.RSM[28].Add(")",new Shift(31));
parser1.RSM[29].Add("EOF",new Reduce(9));
parser1.RSM[29].Add("<<",new Reduce(9));
parser1.RSM[29].Add("cin",new Reduce(9));
parser1.RSM[29].Add("cout",new Reduce(9));
parser1.RSM[30].Add("<<",new Reduce(7));
parser1.RSM[30].Add("EOF",new Reduce(7));
parser1.RSM[30].Add("cin",new Reduce(7));
parser1.RSM[30].Add("cout",new Reduce(7));
parser1.RSM[31].Add(")",new Reduce(7));
parser1.ReSyncSymbol = ";";
return parser1;
}//make_parser

// Needs to be compiled with both .dlls (all .dlls?)
//using System;
// Do we even need this? I don't know if it fits into how we are doing things
//public static void Main(string[] argv]) {
   //Console.WriteLine("Write something in C+- : "); // Changed
   //string input = Console.ReadLine();  // Changed
   //simpleLexer lexer1 =  new simpleLexer(srcfile, "EOF"); // Changed
   //Parser<object> parser = Generator.make_parser(); // Changed
   //parser1.parse(lexer1); // Changed
//}//main

} // Generator Class