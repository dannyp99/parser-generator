module MongooseCompiler

open System
type environ = (string*expr ref) list 
  and
  expr = Val of int | Str of string | Binop of (string*expr*expr) | Uniop of (string*expr) | Var of string | Ifelse of expr*expr*expr | Seq of (expr list) | Letexp of string*expr*expr | App of string*expr | Lambda of string*expr | Assign of string*expr | Sym of string | Nothing with
    override this.ToString() = 
      match this with 
        | Val(i) -> "Val(" + i.ToString() + ")"
        | Str(s) | Var(s) | Sym(s) -> s
        | Binop(s,e1,e2) -> "Binop(" + s + ", " + e1.ToString() + ", " + e2.ToString() + " )"
        | Uniop(s,e) -> "Uniop(" + s + ", " + e.ToString() + " )"
        | Ifelse(e1,e2,e3) -> "Ifelse(" + e1.ToString() + ", " + e2.ToString() + ", " + e3.ToString() + " )"
        | Seq(car::cdr) -> "Sequence(" + car.ToString() + ", " + cdr.ToString() + " )"
        | Letexp(s,e1,e2) -> "Letexp(\"" + s + "\", " + e1.ToString() + ", " + e2.ToString() + " )"
        | App(s,e) -> "App(" + s + ", " + e.ToString() + ")"
        | Lambda(s,e) -> "Lambda(" + s + ", " + e.ToString() + ")" 
        | Assign(s,e) -> "Assign(" + s + ", " + e.ToString() + ")"
        | Nothing -> "Nothing"
        | expr -> "This expr is not supported by ToString() yet" // place holder to implement the rest of the expr ToStrings

let NewVal(v) = Val(v);
let NewStr(s) = Str(s);
let NewBinop(s,e1,e2) = Binop(s,e1,e2);
let NewUniop(s,e) = Uniop(s,e);
let NewVar(s) = Var(s);
let NewIfelse(e1,e2,e3) = Ifelse(e1,e2,e3);
let NewSeq(e,es) = Seq([e;es]); // I still have my concerns about this. but I believe is keeps the car::cdr functionality. It will end with [Nothing] and not Nothing though. 
let NewLetexp(s,e1,e2) = Letexp(s,e1,e2);
let NewApp(s,exp) = App(s,exp);
let NewLambda(s,exp) = Lambda(s,exp);
let NewAssign(s,exp) = Assign(s,exp);
let NewSym(s) = Sym(s);
let NewFSNothing = Nothing; 

(*
type expr =
    | Val of int
    | Str of string
    | Binop of (string*expr*expr)
    | Uniop of (string*expr)
    | Var of string
    | Ifelse of expr*expr*expr
    | Seq of (expr list)
    | Letexp of string*expr*expr
    | App of string*expr
    | Lambda of string*expr
    | Assign of string*expr
    | Sym of string
    | Nothing
*)

type bvar = (string*int)

type alphamap = (string * string) //what 1s th1s

let mutable counter = 0
let mutable lcx = 0  // alpha counter for register names

let defs: string list = []


let symtable  = []

let newreg () =
    lcx <- lcx + 1
    "%r" + string(lcx)

let newlabel () =
    lcx <- lcx + 1
    "l" + string(lcx)

type inst_type =
    | Cfunc
    | Comparison
    | Arith
    | Unknown

let transinst = function
  | "+" -> (Arith, "add i32")
  | "-" -> (Arith, "sub i32")
  | "*" -> (Arith, "mul i32")
  | "/" -> (Arith, "sdiv i32")
  | "%" -> (Arith, "srem i32")
  | "==" -> (Comparison, "icmp eq i32")
  | "<" -> (Comparison, "icmp slt i32")
  | "<=" -> (Comparison, "icmp sle i32")
  (*
  | "==" -> (Comparison, "call i32 @mongoose_eq")
  | "<" -> (Comparison, "call i32 @mongoose_lt")
  | "<=" -> (Comparison, "call i32 @mongoose_leq")
  *)
  | "^" ->  (Cfunc, "call i32 @mongoose_expt")
  | "=" -> (Cfunc, "call i32 @mongoose_assign")
  | "&&" -> (Cfunc, "call i32 @mongoose_and")
  | "||" -> (Cfunc, "call i32 @mongoose_or")
  | x -> (Unknown, x);;

let includes = "declare i32 @putchar(i32)
declare i32 @mongoose_cout_expr(i32)
declare i32 @mongoose_cout_str(i8*)
declare i32 @mongoose_cin()
declare i32 @mongoose_expt(i32, i32)
declare i32 @mongoose_assign(i32*, i32)
declare i32 @mongoose_or(i32, i32)
declare i32 @mongoose_and(i32, i32)
declare i32 @mongoose_not(i32)
declare i32 @mongoose_neg(i32)\n"

let mutable compile_binop = fun (op,x,y,bvar,alpha,label) -> ("","","")
let mutable compile_ifelse = fun (c,t,f,bvar,alpha,label) -> ("","","")

let rec comp_llvm  (exp,bvar,alpha,label) =
    match exp with
        | Val(n) ->
            ("",string(n),label)
        | Binop(op,x,y) ->
            compile_binop(op, x, y, bvar, alpha, label)
        | Ifelse(c,t,f) ->
            compile_ifelse(c,t,f,bvar,alpha,label)
        | _ ->
            (string(exp) + "%s\n ERROr ^not compile-able^","",label)

compile_ifelse <- fun (c,t,f,bvar,alpha,label) ->
            let start_true = newlabel()
            let start_false = newlabel()
            let (outc,destc,labelc) = comp_llvm(c,bvar,alpha,label)
            let (outt,destt,end_true) = comp_llvm(t,bvar,alpha,start_true)
            let (outf,destf,end_false) = comp_llvm(f,bvar,alpha,start_false)
            let comparereg = newreg()
            let result = newreg()
            let rlbl = newlabel()
            let mutable output = outc
            output <- output + sprintf "%s = icmp sgt i32 %s, 0\n" comparereg destc
            output <- output + sprintf "br i1 %s, label %%%s, label %%%s\n" comparereg start_true start_false
            output <- output + sprintf "\n%s:\n%s" start_true outt
            output <- output + sprintf "br label %%%s\n" rlbl
            output <- output + sprintf "\n%s:\n%s" start_false outf
            output <- output + sprintf "br label %%%s\n" rlbl
            output <- output + sprintf "\n%s:\n" rlbl
            let phi = sprintf " = phi i32 [%s, %%%s], [%s, %%%s]\n" destt end_true destf end_false
            output <- output + result + phi
            (output,result,rlbl)

compile_binop <- fun (op, x, y, bvar, alpha, label) ->
    let (outx,destx,xlabel) = comp_llvm(x,bvar,alpha,label)
    let (outy,desty,ylabel) = comp_llvm(y,bvar,alpha,label)
    let top = transinst(op)
    let mutable output = sprintf "%s%s" outx outy
    let mutable reg = newreg()
    match top with
        | (Arith, opstring) ->
            output <- output + sprintf "%s = %s %s, %s\n" reg opstring destx desty
        | (Cfunc, opstring) ->
            output <- output + sprintf "%s = %s(i32 %s, i32 %s)\n" reg opstring destx desty
        | (Comparison, opstring) ->
            let tmp_reg = reg
            reg <- newreg()
            output <- output + sprintf "%s = %s %s, %s\n" tmp_reg opstring destx desty
            output <- output + sprintf "%s = zext i1 %s to i32\n" reg tmp_reg
        | (Unknown, opstring) ->
            output <- "unrecognized binary operator"
    (output,reg,label)



let boilerplate = "target datalayout = \"e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128\"
target triple = \"x86_64-pc-linux-gnu\"\n\n"

let top = sprintf "%s\ndefine i32 @main() {\n" includes
let bot = "ret i32 0\n}"

(*
(ifelse (((5+6)-1)*1)
    (ifelse (0)
        7,
        8)
    9
)
*)
// let tree = Ifelse (Binop ("*", Binop ("-", Binop ("+", Val 5, Val 6), Val 1), Val 1), Ifelse (Val 0, Val 7, Val 8), Val 9)

let compile (tree:expr) =
    let res = comp_llvm (tree, "", "", "")
    match res with
        | (code,res,label) ->
            let print = sprintf "call i32 @mongoose_cout_expr(i32 %s)\ncall i32 @putchar(i32 10)\n" res
            let output = sprintf "%s%s%s\n%s%s" boilerplate top code print bot
            output
