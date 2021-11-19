module FSEvaluator

open System;;
open Microsoft.FSharp.Math;;
open System.Text.RegularExpressions;;

type expr = Val of int | Plus of (expr*expr) | Times of (expr*expr) | Subtract of (expr*expr) | Divide of (expr*expr) | Expt of (expr*expr) | Uminus of expr | Sym of String | EOF;;

let rec eval = function  
  | Val(v) -> v
  | Plus(a,b) -> eval a + eval b
  | Times(a,b) -> eval a * eval b
  | Subtract(a,b) -> eval a - eval b
  | Divide(a,b) -> eval(a) / eval(b)
  | Expt(a,b) -> int(Math.Pow(float(eval a),float(eval b)))
  | Uminus(a) -> -1 * eval(a)
  | x -> raise (Exception(string(x)+"can't be evaluated"));


// FSEvaluator.expr.NewVal(a)

let NewVal(a) = Val(a);
let NewPlus(a,b) = Plus(a,b);
let NewTimes(a,b) = Times(a,b);
let NewSubtract(a,b) = Subtract(a,b);
let NewDivide(a,b) = Divide(a,b);
let NewExpt(a,b) = Expt(a,b);
let NewUminus(a) = Uminus(a);;

let run(e) = 
  printfn "%A" (eval(e));;







