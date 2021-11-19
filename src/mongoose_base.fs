(*    CSC 123/252 Programming Assignment and Base Program:

Completing the Interpreter for the Programming Language MONGOOSE
("Mongoose Eat Snakes")

Main characteristics of the language:

Monotyped. Everything must eventually evaluate to type int.  
Booleans: 0 for false and non-zero (1 preferred) for true
Binary operators +, -, *, /, %, ||, &&,
Unary operators: - (unary minus) and ! (boolean negation)
booleans operators: == (equality), < (less than), <= 

if-else expressions (no standalone ifs).
while loops
sequence expressions separated by ;
The only delimiters are () - use liberally
Comment lines starting with #  (must be for the entire line)
destructive assignment: x = x+1
let binds statically. ***
let can bind variables to lambda expressions
lambda expressions can only take int and return int  (type int -> int)
lambda expressions can contain free variables
lambda expressions can be recursive (in terms of pure lambda calclus, such
lambdas are actually fixed points).
I/O: cin and cout (but without << or >>).

Sample mongoose programs:

(test1.ms):
if (3 == 3-1) 100 else (if (1 < 2) 200 else 300)
#this nested if-else should evaluate to 200

(test2.ms):
# sequences should be in ()s use () everywhere because of hacked parser
let y=0:
let x=2:
(
 while (y < 4)
 (
  x=x*x;
  y=y+1;
  cout "the value of y is now ";
  cout y;
  cout "\n";
 );
 cout "the final value of x is ";
 cout x; 
 cout "\n";
)
# mongoose eat snakes and c++ too.

(accumulator.ms)
let x=1:
let f=(lambda y.(x=x+y; x;)):
(
  (f 2);
  (f 3);
  (f (f (f 4)));
)
#this program  should return 40

(area51.ms)
let pi = 314159: 
let area = (lambda r.(pi*r*r)/100000):
let pi = 1: (area 51)
# Save your planet by implementing the right kind of scoping rule.

(log2.ms)
# testing recursion  (implicitly this is a let rec...)
let log2 = (lambda n. if (n == 1) 0 else 1+(log2 (n/2))): (log2 1024)

//////////////////////////////////////////////////////////////////////

The base program contains the definition of abstract syntax, the
lexical analyzer (imported from C#), parser, and incomplete
interpreter (eval function).  You must complete the interpreter for
the full language.  This assignment will give you a taste of how to
design and implement your own programming language.

************************ YOUR ASSIGNMENT ********************************

YOU ONLY NEED TO WORRY ABOUT THE 'eval' FUNCTION.  Part of the
function has already been written: but right now the program can
barely recognize anything beyond simple arithmetic expressions.  You
need to "extend" the definition of the eval function to interpret as
much of mongoose as possible.  First, study the definition of the
abstract syntax carefully.  If you're going to attempt some of the
more difficult parts of the assignment, pay special attention to
Letexp, Lambda and App.  Closures are not created by the parser but by
the evaluator when there's a need to.  Secondly, study carefully the
base implementation of eval, especially the case for Var

Extend the eval function by assigning it to a new lambda term like
the example to interpret the entire language by creating a .fs file with
the following skeleton:

module CSC7B  // at the very top
open CSC7B;

let base_eval = eval
eval <- fun env exp ->  // eval takes a environ structure 'env' and and expr 'exp'
  match exp with   //if you match against (env,exp), your pattern must be a pair
   | ... // your new clauses implementing new features.
   ...   // more clauses
   | _ -> base_eval env exp;; // this links your extentions with the original

runit(); // at the very bottom

compile your program: (instructions are for the linux vm):
  fsharpc yourprogram.fs -r mongoose_base.dll -r iosetup.dll -r simpleLexer.dll
run: mono yourprogram.exe interpret prompt   (prompts user for expression)
     mono yourprogram.exe interpret < test1.ms (reads from file and run it)

  This Assignment contains several levels.  A passing grade is earned by
  completing the basic level, with progressively better grades for each
  subsequent level completed.

=Level 1 (Grade: "Nerd").  To earn the minimal grade of "nerd" you must
be able to handle both boolean expressions and if-else expressions,
including nested if's.  Since Mongoose only have the int type, it uses
0 for false and anything non-zero for true (but 1 is preferred).  You
must be able to properly interpret the boolean operators &&, || and !
(negation), You must also be able to interpret the relational
operators ==, <, and <= (i.e. 3<4-2 should eval to 0). Then, evaluate
the Ifelse construct (eval the "then" case or the "else" case.  Your
program must work properly on test1.ms and test2.ms as well as similar programs.

==Level 2 (Grade: "Meganerd"). To earn the better grade of "meganerd", you
must complete all the requirements for being "nerd" as well as the
following: let expressions, expressions with variables, 'Assign'
expressions (destructive assignment to declared variables), sequence
expression (case Seq in the abstract syntax) and while-loops. As in C,
an assignment x=value should also return value.  You must be able to
correctly run test3.ms, test4.ms and test5.ms as well as similar programs.

Look through mongoose_base.fs and you will see some pre-defined functions
that will help you in your effort, including lookup, change and changeval
(on environments) change/changeval can be used to implement destructive 
assignment. 

===Level 3 (Grade: "Giganerd").  To be giganerd you must complete all the
requirements of meganerd plus be able to interpret the entire language,
namely lambda-functions including recursive functions and function
calls.  All test programs up to test10.ms, plus the ALL-IMPORTANT
area51.ms.

To correctly implement static scoping (the fate of your planet depends on it),
you must wrap all lambda terms inside Closures.  The closure locks the environment
under which a lambda was defined.  To implement applying a function to an argument,
lookup the closure-enclosed lambda term from the environment, insert a new
binding (non-destructively) into the environment, then eval the body of the
lambda.  Refer to class notes if this sounds confusing.


#### Extra Special Grades, for the "Teranerd", "Petanerd", "Exanerd", and the
#### the one "zetanerd" (there can be only one), TBA.  


*************************************************************************
- This program is part of module CSC7B.  
*)

module CSC7B
open System;
open Microsoft.FSharp.Math;
open System.Text.RegularExpressions;
open System.Collections.Generic;
open CSC7B;;  
// requires iosetup.dll and simpleLexer.dll

////////////  Abstract Syntax (expressions and enviornments) ////

///// the abstract syntax for expression trees contains a case for closures,
// and so is defined simultaneously for both expressions and environments.
// The type of expressions also include Sym and EOF, which are used during
// the parsing stage.  The expression trees (type expr) produced by the parser
// do not include closure expressions.  Closures are only introduced during
// the interpreter stage (eval).  eval produces environments in which variables
// names are bound to not just numbers but closures (unevaluated expressions).

//// Note: because it's difficult to extend a discriminated union modularly,
// we are using strings to represent different kinds of expressions, so there
// is a cost to be paid in terms of static type safety.  Although pure F# is
// statically "type safe", there is not much type information available when
// use strings to represent data.  Instead, part of "type checking" has to
// be done at runtime with code like the following:


type environ = (string*expr ref) list
     and 
     expr = Val of int | Binop of (string*expr*expr) | Uniop of (string*expr) | Var of string | Ifelse of expr*expr*expr | Seq of (expr list) | Letexp of string*expr*expr | Closure of (environ*expr) | App of string*expr | Lambda of string*expr | Assign of string*expr | Sym of string | EOF ;;

// the following are 'Constructors' for easier integration into C#
// We never make environ outside of the fsharp code.
let NewVal(val) = Val(val);
let NewBinop(s,e1,e2) = Binop(s,e1,e2);
let NewUniop(s,exp) = Uniop(s,exp);
let NewVar(s) = Var(s);
let NewIfelse(e1,e2,e3) = Ifelse(e1,e2,e3);
let NewSeq(elist) = Seq(elist);
let NewLetexp(s,e1,e2) = Letexp(s,e1,e2)
let NewClosure(envt,exp) = Closure(env,exp)
let NewApp(s,exp) = App(s,exp)
let NewLambda(s,exp) = Lambda(s,exp);
let NewAssign(s,exp) = Assign(s,exp);
let NewSym(s) = Sym(s);

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
    | Uniop("-",a) -> -1 * (eval env a)
    | Var(s) -> eval env (lookup s env)
    | Uniop("cin",_) ->
        writenoln("<< ")  // writenoln defaults to Console.Write
        let sinp = getln()   // getln defaults to Console.ReadLine
        try (int(sinp))
          with
          | _ -> raise(Exception("input is not an integer"))
    | Uniop("cout",Sym(s)) ->
        let s2=s.Replace("\\n","\n")
        writenoln(s2)
        0  // void ops should always return 0
    | Uniop("cout",e1) ->
        let ev1 = eval env e1
        writenoln(string(ev1))
        0
    | x -> raise (Exception("not supported eval case: "+string(x)));;
////////////////////////////////////////////////////


////////////////       LEXICAL ANALYZER (LEXER, TOKENIZER)
//// must adopt basic lexer written in C# (in simpleLexer.dll)
let mutable TS =[];; // global list of tokens
let mutable TI = 0;; // global index for TS stream;;
let mutable input_string = "";; // default input string, GLOBAL!

// function to convert C# lexToken structures to F# type expr
let convert_token (token:lexToken) =
  match token.token_type with
   | "Integer" -> Val(token.token_value :?> int) // :?> downcasts from obj
   |  "Symbol" | "Keyword" -> Sym(token.token_value :?> string)
   |  "StringLiteral" ->
      let s = token.token_value :?> string
      Sym(s.Substring(1,s.Length-2))
   |  "Alphanumeric" -> Var(token.token_value :?> string)
   | _ -> EOF;;
// all alphanumerics that are not keywords become Variables   


///// The following function takes an input string and sets global
// variable TS, which is a stream of tokens (see commented example above
// for (7+3*2)).  It also sets TI, which is a global index into TS.
let mutable lexer = fun (inp:string) ->  // main lexical analysis function
  let scanner = simpleLexer(inp);  // create .net object
  for kw in ["if";"else";"while";"let";"lambda";"cin";"cout"] do
     scanner.addKeyword(kw)
  let rec tokenize ax =
     let token = scanner.next()
     if token=null then ax else tokenize (convert_token(token)::ax);
  let rec reverse stack = function  // shorthand for match arg with ...
    | [] -> stack
    | a::b -> reverse (a::stack) b;
  let tokens = reverse [EOF] (tokenize [])
  TS <- tokens  // assign to globar for convenience
  TI <- 0;;  // reset if needed
//  printfn "\ntoken stream: %A\n" TS;;



///////////////////////////
////////////////////////// SHIFT-REDUCE PARSER ////////////////////////
let mutable binops=    ["+";"*";"/";"-";"%";"==";"^";"<";"<=";"while";"&&";"||";"assign"];
let mutable unaryops=["-"; "!"; "~";"cin";"cout"];

// use hash table (Dictionary) to associate each operator with precedence
let prectable = Dictionary<string,int>();;
prectable.["+"] <- 200;
prectable.["-"] <- 300;
prectable.["*"] <- 400;
prectable.["/"] <- 500;
prectable.["%"] <- 500;
prectable.["^"] <- 550;
prectable.["!"] <- 550;
prectable.["&&"] <- 540;
prectable.["||"] <- 530;
prectable.["("] <- 990;
prectable.[")"] <- 20;
prectable.[":"] <- 42;   // trial by error... got to be careful
prectable.["="] <- 30;
prectable.["."] <- 20;
prectable.["_"] <- 600;  // fictional symbol for function app
prectable.["=="] <- 35;
prectable.["<="] <- 35;
prectable.["<"] <- 35;
prectable.["if"] <- 20;
prectable.["let"] <- 42;
prectable.["while"] <- 40
prectable.["else"] <- 18; //20
prectable.["cin"] <- 100 //  same as Val
prectable.["cout"] <- 22;
prectable.[";"] <- 20;
prectable.["begin"] <- 20;
prectable.["end"] <- 20;

let mutable proper = fun f ->
  match f with
    | Sym(_) -> false
    | EOF -> false
    | _ -> true;; // everything else is considered a proper expression
// check if a list of expressions are all proper
let rec all_proper n =
  match n with
    | [] -> true
    | (car::cdr) -> proper(car) && all_proper(cdr);;
// note : short-circuited boolean makes this tail-recursive.


// function defines precedence of symbol, which includes more than just Syms
let mutable precedence = fun s ->
  match s with
   | Val(_) -> 100
   | Var(_) -> 100
   | Sym(s) when prectable.ContainsKey(s) -> prectable.[s]
   | EOF    -> 10
   | _ -> 11;;

   
// Function defines associativity: true if left associative, false if right...
// Not all operators are left-associative: the assignment operator is
// right associative:  a = b = c; means first assign c to b, then b to a,
// as is the F# type operator ->: a->b->c means a->(b->c).

let mutable leftassoc = fun e ->
  match e with
   | _ -> true;  // most operators are left associative.

// check for precedence, associativity, and proper expressions to determine
// if a reduce rule is applicable.
let mutable checkreducible = fun (a,b,elist) ->
  let (pa,pb) = (precedence(a),precedence(b))
  ((a=b && leftassoc(a)) || pa>=pb) && all_proper(elist);
// parse takes parse stack and lookahead; default is shift

////////////////// HERE IS THE HEART OF THE SHIFT-REDUCE PARSER ////////
let mutable parse = fun (x:expr list,expr) -> EOF // dummy for recursion
parse <- fun (stack,lookahead) ->
  match (stack,lookahead) with
   | ([e],EOF) when proper(e) -> e   // base case, returns an expression
   | (Sym(")")::e1::Var(f)::Sym("(")::t,la) when int(f.[0])>96 && int(f.[0])<123 && checkreducible(Sym("_"),la,[e1]) ->
        let e = App(f,e1)
        parse(e::t,la)
   | (Sym(")")::e1::Sym("(")::t, la) when checkreducible(Sym("("),la,[e1]) ->
        parse (e1::t,la)
   | (Sym("cin")::t,la) when (precedence (Sym "cin"))>=(precedence la) ->
        let e= Uniop("cin",Val(0))       // Val(0) is just filler
        parse(e::t,la)
   | (e2::Sym("cout")::t,la) when checkreducible(Sym("cout"),la,[e2]) ->
        let e = Uniop("cout",e2)
        parse(e::t,la)
   | (Sym(s)::Sym("cout")::t,la) when checkreducible(Sym("cout"),la,[Var(s)]) ->
        let e = Uniop("cout",Sym(s))
        parse(e::t,la)
   | (e2::Sym(op)::e1::cdr,la)   // generic case for binary operators
     when (List.exists (fun x->x=op) binops) && checkreducible(Sym(op),la,[e1;e2]) ->
        let e = Binop(op,e1,e2)
        parse(e::cdr,la)
   | (e1::Sym("-")::t, la) when checkreducible(Sym("-"),la,[e1]) ->  // "rrc"
        let e = Uniop("-",e1)
        parse (e::t,la)
   | (e1::Sym("!")::t, la) when checkreducible(Sym("-"),la,[e1]) ->  // "rrc"
        let e = Uniop("!",e1)
        parse (e::t,la)
   | (e2::Sym(":")::e1::Sym("=")::Var(x)::Sym("let")::t,la) when checkreducible(Sym(":"),la,[e1;e2]) -> 
        let e = Letexp(x,e1,e2)  //let expressions
        parse(e::t,la)
   | (e1::Sym("=")::Var(x)::t,la) when checkreducible(Sym("="),la,[e1]) ->
        let e = Assign(x,e1)
        parse(e::t,la)
   | (e1::Sym(".")::Var(x)::Sym("lambda")::t,la) when checkreducible(Sym("."),la,[e1]) ->
        let e = Lambda(x,e1)
        parse(e::t,la)
   | (e3::Sym("else")::e2::e1::Sym("if")::t,la) when checkreducible(Sym("if"),la,[e1;e2;e3]) -> 
        let e = Ifelse(e1,e2,e3)
        parse (e::t,la)
   | (body::be::Sym("while")::t,la) when checkreducible(Sym("while"),la,[be;body]) ->
        let e = Binop("while",be,body) in parse(e::t,la)   
   | (Sym(";")::e1::t,la) when checkreducible(Sym("end"),la,[e1]) ->
        let e = Seq([e1]) in parse(e::t,la)
   | (Seq(s2)::Sym(";")::e1::t,la) when checkreducible(Sym("end"),la,[e1]) ->
        let e = Seq(e1::s2) in parse(e::t,la)
   | (st,la) when (TI < TS.Length-1) ->  // shift case
        TI <- TI+1;         
        let newla = TS.[TI]
        parse (la::st,newla)
   | (st,la) ->
        let ms = sprintf "PARSER ERROR: lookahead=%A, stack = %A" la st
        raise (Exception(ms));;
/////////////////////////////////(parser)


////////////////////////////////////////////////////////////////////
////// AOP-style "advice" to trace parse, eval functions

// advice to trace before/after parse, eval
let mutable traceopt = fun (before,after,target:unit->unit) ->
  let proceed_eval = eval
  let proceed_parse = parse
  eval <- fun env e ->
     if before then printfn "evaluating %A under env %A" e env
     let v = proceed_eval env e
     if after then printfn "  eval returned %d" v
     v  // return
  parse <- fun(st,la) ->
     if before then printfn "parsing %A with lookahead %A" st la
     let e = proceed_parse(st,la)
     if after then printfn "  parse returned expression %A" e
     e //return
  target()  // execute target operation
  eval <- proceed_eval      
  parse <- proceed_parse;;


////// Advice to handle exceptions gracefully
let mutable advice_errhandle = fun (target:unit->unit) ->
  let proceed_parse = parse
  let proceed_eval = eval
  parse <- fun(st,la) ->
     try proceed_parse(st,la) with
       | exc ->
           writeln("parse failed with exception "+string(exc))
           exit(1)
  eval <- fun env e ->
     try (proceed_eval env e) with
       | exc ->
           writeln("eval failed with exception "+string(exc)+", returning 0 by default")
           0
  target()  // execute target
  eval <- proceed_eval      
  parse <- proceed_parse;; // restore originals before exit
// advice_errhandle

/// Note that these advice functions group together code that "crosscut" 
// the conventional function-oriented design to be oriented instead towards
// certain "aspects" of the program (tracing, error handling).

///// Advice to read from stdin with prompt:
let mutable advice_io = fun (prompt, target:unit->unit) ->
    if prompt then 
       writenoln("Enter expression: ");
       input_string <- getln()
    else  // take multi-line input
       let mutable inp = "x"
       while inp<>null do
         inp <- getln()
         if inp<>null && inp.Length>0 && inp.[0]<>'#' then 
            input_string <- input_string + inp + " "
    lexer(input_string)
    target();;
//advice_io doesn't need to modify any functions, just inject before target

//// main execution function
// call this function from our C# Program
let mutable run = fun () ->
  let ee = parse([],TS.[0])
  let mutable Bindings:environ = []
  let v = eval Bindings ee
  let ps=(sprintf "\nValue of %s = %d\n" input_string v) in writeln(ps)
let mutable interpret = run;

//////// RUN UNDER ADVICE, innermost advice will have precedence:
//advice_io(true, fun ()-> advice_errhandle( fun () -> advice_trace( run ) ));; 

//lexer(Console.ReadLine()); // tokenize without prompt
// run();; // runs without any advice, must call lexer on some input string.


// default:  (commented out when generating .dll)
//advice_io(true,run);; // run with just input advice and console prompt option

///////////////
//// main execution function
let runit() = 
  let argv = Environment.GetCommandLineArgs();
  if argv.Length>1 && argv.[1] = "CBN" then
     call_by_value <- false;
     argv.[1] <- "interpret"
  else if argv.Length>1 && (argv.[1]="interpret" || argv.[1]="CBV") then
     if argv.Length>3 then serverio_advice(fun () -> advice_io(false,interpret))
     else if argv.Length>2 && argv.[2] = "prompt" then
       advice_io(true,interpret)
     else
       advice_io(false,interpret);

// run with before-trace option (trace call but not return):
//     else serverio_advice(fun ()->memdump_advice(16,run) );

//traceopt(true,true,runit);;
runit();                          /////// comment out to produce .dll

//// .dll was compiled with above line commented out.
// please put both module CSC7B and open CSC7B at the top of your program
// like here.  If you want all definitions to be visible, then compile with
// multiple .dlls:  fsharpc program.fs -r first.dll -r second.dll ...
////////// run (mono) hsbase.exe compile/interpret or mono hsbase.exe for a prompt

// This program was compiled into a .dll with "runit()" commented out.
// Uncomment it to produce an executable:
//   fsharpc mongoose_base.fs -r iosetup.dll -r simpleLexer.dll
// You can run this program and see how it works with
// mono mongoose_base.exe interpret prompt
// You can enter simple arithmetic expressions such as 3+2*4

