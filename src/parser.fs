// shift-reduce parser for online calculator

// ************** Requires simpleLexer.dll ******************

(*  Ambiguous context-free grammer:

    E := var of string  |  // not used at first
	 val of int	|
         E + E  	|
	 E * E		|
	 E - E		|
	 E / E          |
         E ^ E
         (E);;       
         - E;           // unary minus - reduce-reduce precedence
 	                //( will have highest precedence - reduce, don't shift

    negative values will be handled by the tokenizer

    input stream of tokens will be represented as an array, from C# program
    global index will point to the next token.
    
    parse stack will be a list of expressions, starting with empty stack.
    left-side is tos.
*)


open System;;
open Microsoft.FSharp.Math;;
open System.Text.RegularExpressions;;

///////// ABSTRACT SYNTAX

// expr folds in both expressions and tokens from the lexer
type expr = Val of int | Plus of (expr*expr) | Times of (expr*expr) | Subtract of (expr*expr) | Divide of (expr*expr) | Expt of (expr*expr) | Uminus of expr | Sym of String | EOF;;

// proper expression check (shallow)
let proper = function
  | EOF -> false
  | Sym(_) -> false
  | _ -> true;


let rec eval = function 
  | Val(v) -> v
  | Plus(a,b) -> eval a + eval b
  | Times(a,b) -> eval a * eval b
  | Subtract(a,b) -> eval a - eval b
  | Divide(a,b) -> eval(a) / eval(b)
  | Expt(a,b) -> int(Math.Pow(float(eval a),float(eval b)))
  | Uminus(a) -> -1 * eval(a)
  | x -> raise (Exception(string(x)+"can't be evaluated"));
//(printf "error in eval\n"; 0);;   // returns 0 after error
////////////////////////////////////////////////////

(*
         LEXICAL ANALYSER (LEXER) - use version developed in C#

         compile with -r:simpleLexer.dll
*)

// Take input string, hard-coded examples
//let inp = "7+(8-2)";;
//let TS = [Val(7);Sym("+");Sym("(");Val(8);Sym("-");Val(2);Sym(")");EOF];;

Console.Write("Enter expression to be evaluated: ");;
let inp = Console.ReadLine();  // get user input
let scanner = 5; //simplelexer(inp);  // create simplelexer
  
// function to convert C# lexToken structures to F# type expr
// This replaces the concrete Lexer // lextoken translate
type lexToken() = 
  token_type j
let convert_token (token:lexToken) =
  match token.token_type with
   | "Integer" -> Val(token.token_value :?> int) // :?> downcasts from obj
   |  "Symbol" | "Alphanumeric" | "Keyword" -> Sym(token.token_value :?> string)
   | _ -> EOF;;

// collect all tokens into a list, which will enable pattern matching
let rec tokenize cf =  // cf is a "continuation function"
   let next_token = scanner.next()
   if next_token=null then cf([EOF]) else
      tokenize (fun tail -> cf(convert_token(next_token)::tail));;

(* To understand how the tokenize functions places the symbols in the right
   order, assume that there are 2 tokens t1 and t2 to be read by the scanner.
   Initially, we call
   tokenize fun x->x, then we call (with cf1= fun x->x)
   tokenize fun y->cf1(convert_token(t1)::y), 
   tokenize fun z->cf2(convert_token(t2)::z), with cf2 the above fun
   Call this final continuation function (fun z->...) cf3.
   The next call to scanner.next() returns null, so tokenize will return
   cf3([EOF]).  This unravels to
     cf2(convert_token(t2)::[EOF])  -->
     cf1(convert_token(t1)::convert_token(t2)::[EOF])  -->
     convert_token(t1)::convert_token(t2)::[EOF] because cf1 is lambda x.x
*)

let TS = tokenize (fun x -> x);
printfn "token stream: %A" TS;;

let mutable TI = 0;; // global index for TS stream;;
///////////////////



///////////////////
////////////////////////// SHIFT-REDUCE PARSER ////////////////////////

let precedence = function
 | Val(_) -> 100
 | Sym("+") -> 200
 | Sym("-") -> 300
 | Sym("*") -> 400
 | Sym("/") -> 500
 | Sym("^") -> 600
 | Sym("(") -> 800
 | Sym(")") -> 20
 | EOF        -> 10
 | _ -> 0;;

let mutable leftassoc = fun x -> true;; // this assumes all ops are left-assoc

// check for precedence and proper expressions
let check(a,b,e1,e2) =
  match (a,b) with
    | (a,b) when a=b -> leftassoc(a) && proper(e1) && proper(e2)
    | (a,b) ->
       let (pa,pb) = (precedence(a),precedence(b));
       (pa > pb) && proper(e1) && proper(e2);;

// parse takes parse stack and lookahead; default is shift

let rec parse = function 
  | ([e],EOF) when proper(e) -> e   // base case, returns an expression
  | (Sym(")")::e1::Sym("(")::t, la) when check(Sym("("),la,e1,e1) -> 
            parse (e1::t,la)
  | (e2::Sym("+")::e1::t, la) when check(Sym("+"),la,e1,e2) ->  
            let e = Plus(e1,e2) 
            parse (e::t,la)
  | (e2::Sym("-")::e1::t, la) when check(Sym("-"),la,e1,e2) ->
            let e = Subtract(e1,e2) 
            parse (e::t,la)
  | (e2::Sym("*")::e1::t, la) when check(Sym("*"),la,e1,e2) ->
            let e = Times(e1,e2) in parse (e::t,la)
  | (e2::Sym("/")::e1::t, la) when check(Sym("/"),la,e1,e2) ->
            let e = Divide(e1,e2) in parse (e::t,la)
  | (e2::Sym("^")::e1::t, la) when check(Sym("^"),la,e1,e2) ->
            let e = Expt(e1,e2) in parse (e::t,la)
  | (e1::Sym("-")::t, la) when check(Sym("-"),la,e1,e1) ->  // "rrc"
            let e = Uminus(e1) in parse (e::t,la)
  | (st,la) when (TI < TS.Length-1) -> 
            (TI <- TI+1;         // shift!
             printf "shift: %A\n" st;   // trace
             let newla = TS.[TI] in parse (la::st,newla))
  | (st,la) -> (printf "parsing error: %A\n" (la::st); EOF);;


//////// RUN
let ee = parse([],TS.[0]);;
let v = eval ee;;
printf "value of %s = %d\n" inp v;;

