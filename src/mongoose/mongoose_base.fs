module FSEvaluator
open System;
open Microsoft.FSharp.Math;
open System.Text.RegularExpressions;
open System.Collections.Generic;

type environ = (string*expr ref) list 
  and
  expr = Val of int | Str of string | Binop of (string*expr*expr) | Uniop of (string*expr) | Var of string | Ifelse of expr*expr*expr | Seq of (expr list) | Letexp of string*expr*expr | Closure of (environ*expr) | App of string*expr | Lambda of string*expr | Assign of string*expr | Sym of string | EOF ;;
// the following are 'Constructors' for easier integration into C#
// We never make environ outside of the fsharp code.
let NewVal(v) = Val(v);
let NewBinop(s,e1,e2) = Binop(s,e1,e2);
let NewUniop(s,exp) = Uniop(s,exp);
let NewVar(s) = Var(s);
let NewIfelse(e1,e2,e3) = Ifelse(e1,e2,e3);
let NewSeq(e,elist) = Seq([e;elist]);
let NewLetexp(s,e1,e2) = Letexp(s,e1,e2)
let NewClosure(env,exp) = Closure(env,exp)
let NewApp(s,exp) = App(s,exp)
let NewLambda(s,exp) = Lambda(s,exp);
let NewAssign(s,exp) = Assign(s,exp);
let NewSym(s) = Sym(s);
let NewEOF = EOF;

let rec lookup x (env:environ) =   // returns expr
   match env with
    | [] -> raise(Exception(x+" not found in environment/table"))
    | ((y,rv)::cdr) when x=y -> !rv
    | ((y,rv)::cdr) -> lookup x cdr
let rec change x n (env:environ) =   
   match env with
    | [] -> raise(Exception(x+" not declared in this scope"))
    | ((y,rv)::cdr) when x=y -> rv := n
    | ((y,rv)::cdr) -> change x n cdr;;
let appended x e env = (x,ref e)::env; // to add ref binding to environment
// appended is non-destructive: (appended x e env) is a expression that represents
// a larger environment.  An environment's bindings, however, can change
// because it contains expr ref.  But the structure of the environment, in
// terms of what symbols (strings) it contains bindings for, may not change.

let lookupval x env =    // returns int
   let v = lookup x env
   match v with
     | Val(n) -> n
     | _ -> raise(Exception(string(v)+" is not an int"));;
let changeval x n env = change x (Val(n)) env;;   // n is int

let rec bindingexistsfor x (env:environ) =
   match env with
    | [] -> false
    | ((y,rv)::cdr) when x=y -> true
    | ((y,rv)::cdr) -> bindingexistsfor x cdr;;
  

// Eval an application (lambda x.e)v as let x=v in e.  each lambda constrained
// to abstract over an int expression.  need syntax for application.  where
// do closure come in?  - translate into let x=v in closure(envoflambda+x,e)

// use environments of the form [("x",ref Val(0));("y",ref Val(0))] ...


//////////////////////////////////// INTERPRETER

let mutable call_by_value = true;  // global var for reduction strategy

// writeln, getln, writenoln defined in iosetup.dll

// Because of Var expressions, env represents bindings for variables.
// eval is mutable so we can inject behaviors later...
// mutable funs can't be recursive calls, unless we declare eval first:
let mutable eval = fun (env:environ) (exp:expr) -> 0;;
eval <- fun (env:environ) exp ->
  match exp with
    | Val(v) -> v
    | Binop("+",a,b) -> (eval env a) + (eval env b)  // not Plus(a,b)
    | Binop("*",a,b) -> (eval env a) * (eval env b)  // lose some static safety
    | Binop("-",a,b) -> (eval env a) - (eval env b)
    | Binop("/",a,b) -> (eval env a) / (eval env b)
    | Binop("%",a,b) -> (eval env a) % (eval env b)
    | Binop("==",a,b) -> if (eval env a) = (eval env b) then 1 else 0
    | Binop("<",a,b) -> if (eval env a) < (eval env b) then 1 else 0
    | Binop("<=",a,b) -> if (eval env a) <= (eval env b) then 1 else 0
    //| Binop("^",a,b) -> (eval env a) ** (eval env b)
    //| Binop("while",a,b) -> 
    //| Binop("&&",a,b) ->
    //| Binop("||",a,b) ->
    | Uniop("-",a) -> -1 * (eval env a)
    | Var(s) -> eval env (lookup s env)
    | Uniop("cin",_) ->
        Console.WriteLine("<< ")  // writenoln defaults to Console.Write
        let sinp = Console.ReadLine()   // getln defaults to Console.ReadLine
        try (int(sinp))
          with
          | _ -> raise(Exception("input is not an integer"))
    | Uniop("cout",Sym(s)) ->
        let s2=s.Replace("\\n","\n")
        Console.WriteLine(s2)
        0  // void ops should always return 0
    | Uniop("cout",e1) ->
        let ev1 = eval env e1
        Console.WriteLine(string(ev1))
        0
    | Ifelse(c,a,b) ->
        match (eval env c) with
          | 0 -> (eval env b) * 1
          | _ -> (eval env a) * 1
    | x -> raise (Exception("not supported eval case: "+string(x)));;
////////////////////////////////////////////////////
// I do not think you can call certain variables depending on mutable/rec. Need
// to make an "entry" function
// let mutable printTree = fun result (parseTree:expr) -> "";;
// printTree <- fun result (parseTree:expr) ->
//   match parseTree with
//     | Val(x) -> 
//         result = result + "Val("+string(x)+")"; 
//         //endString <- endString + "Val(" + string(x) + ")"
//     | Binop(a,b,c) -> 
//         result = result + "Binop(" + string(a) + "," + (printTree b) + "," + (printTree c) + ")"; 
//         //endString <- endString + string(a) + "("+ (printTree b) +","+ (printTree c) +")"
//     | Ifelse(a,b,c) -> 
//         result = result + "if(" + string(a) + "," + (printTree b) + "," + (printTree c) + ")"; 
//         result
//         //endString <- endString + string(a) + "(" + (printTree b) +","+ (printTree c) + ")"
//     | _ ->  
//         result = "Unrecognized expression"; 
//         result;
//   endString;;


// let FSPrint(e:expr) = 
//   Console.WriteLine("Parse Tree:")
//   Console.WriteLine((printTree "" e)
let run(e:expr) = 
  printfn "%A" (eval [] e);


////////////////       LEXICAL ANALYZER (LEXER, TOKENIZER)
//// must adopt basic lexer written in C# (in simpleLexer.dll)
// let mutable TS =[];; // global list of tokens
// let mutable TI = 0;; // global index for TS stream;;
// let mutable input_string = "";; // default input string, GLOBAL!

// // function to convert C# lexToken structures to F# type expr
// let convert_token (token:lexToken) =
//   match token.token_type with
//    | "Integer" -> Val(token.token_value :?> int) // :?> downcasts from obj
//    |  "Symbol" | "Keyword" -> Sym(token.token_value :?> string)
//    |  "StringLiteral" ->
//       let s = token.token_value :?> string
//       Sym(s.Substring(1,s.Length-2))
//    |  "Alphanumeric" -> Var(token.token_value :?> string)
//    | _ -> EOF;;
// // all alphanumerics that are not keywords become Variables   


// ///// The following function takes an input string and sets global
// // variable TS, which is a stream of tokens (see commented example above
// // for (7+3*2)).  It also sets TI, which is a global index into TS.
// let mutable lexer = fun (inp:string) ->  // main lexical analysis function
//   let scanner = simpleLexer(inp);  // create .net object
//   for kw in ["if";"else";"while";"let";"lambda";"cin";"cout"] do
//      scanner.addKeyword(kw)
//   let rec tokenize ax =
//      let token = scanner.next()
//      if token=null then ax else tokenize (convert_token(token)::ax);
//   let rec reverse stack = function  // shorthand for match arg with ...
//     | [] -> stack
//     | a::b -> reverse (a::stack) b;
//   let tokens = reverse [EOF] (tokenize [])
//   TS <- tokens  // assign to globar for convenience
//   TI <- 0;;  // reset if needed
// //  printfn "\ntoken stream: %A\n" TS;;



// ///////////////////////////
// ////////////////////////// SHIFT-REDUCE PARSER ////////////////////////
// let mutable binops=    ["+";"*";"/";"-";"%";"==";"^";"<";"<=";"while";"&&";"||";"assign"];
// let mutable unaryops=["-"; "!"; "~";"cin";"cout"];

// // use hash table (Dictionary) to associate each operator with precedence
// let prectable = Dictionary<string,int>();;
// prectable.["+"] <- 200;
// prectable.["-"] <- 300;
// prectable.["*"] <- 400;
// prectable.["/"] <- 500;
// prectable.["%"] <- 500;
// prectable.["^"] <- 550;
// prectable.["!"] <- 550;
// prectable.["&&"] <- 540;
// prectable.["||"] <- 530;
// prectable.["("] <- 990;
// prectable.[")"] <- 20;
// prectable.[":"] <- 42;   // trial by error... got to be careful
// prectable.["="] <- 30;
// prectable.["."] <- 20;
// prectable.["_"] <- 600;  // fictional symbol for function app
// prectable.["=="] <- 35;
// prectable.["<="] <- 35;
// prectable.["<"] <- 35;
// prectable.["if"] <- 20;
// prectable.["let"] <- 42;
// prectable.["while"] <- 40
// prectable.["else"] <- 18; //20
// prectable.["cin"] <- 100 //  same as Val
// prectable.["cout"] <- 22;
// prectable.[";"] <- 20;
// prectable.["begin"] <- 20;
// prectable.["end"] <- 20;

// let mutable proper = fun f ->
//   match f with
//     | Sym(_) -> false
//     | EOF -> false
//     | _ -> true;; // everything else is considered a proper expression
// // check if a list of expressions are all proper
// let rec all_proper n =
//   match n with
//     | [] -> true
//     | (car::cdr) -> proper(car) && all_proper(cdr);;
// // note : short-circuited boolean makes this tail-recursive.


// // function defines precedence of symbol, which includes more than just Syms
// let mutable precedence = fun s ->
//   match s with
//    | Val(_) -> 100
//    | Var(_) -> 100
//    | Sym(s) when prectable.ContainsKey(s) -> prectable.[s]
//    | EOF    -> 10
//    | _ -> 11;;










