module MongooseCompiler

open System
type environ = (string*expr ref) list 
  and
  expr = Val of int | Str of string | Binop of (string*expr*expr) | Uniop of (string*expr) | Var of string | Ifelse of expr*expr*expr | Seq of (expr list) | Letexp of string*expr*expr | App of string*expr | Lambda of string*expr | Assign of string*expr | Sym of string | Nothing with
    override this.ToString() = 
      match this with 
        | Val(i) -> "Val(" + i.ToString() + ")"
        | Str(s) -> "Str(" + s + ")"
        | Var(s) -> "Var(" + s + ")"
        | Sym(s) -> "Sym(" + s + ")"
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


let mutable counter = 0
let mutable lcx = 0  // alpha counter for register names
let mutable strx = 0 // counter for global strings

let mutable defs: string list = []

let mutable symtable  = Map.empty<string, string list>

let newreg () =
  lcx <- lcx + 1
  "%r" + string(lcx)

let newlabel () =
  lcx <- lcx + 1
  "l" + string(lcx)

let newstring () =
  strx <- strx + 1
  "@str." + string(strx)
  
let aConvert (x:string) =
  counter <- counter + 1
  "%" + x + string(counter)

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
  | "^" ->  (Cfunc, "call i32 @mongoose_expt")
  | "=" -> (Cfunc, "call i32 @mongoose_assign")
  | "&&" -> (Cfunc, "call i32 @mongoose_and")
  | "||" -> (Cfunc, "call i32 @mongoose_or")
  | x -> (Unknown, x);;

let includes = "\
declare i32 @mongoose_cout_expr(i32)
declare i32 @mongoose_cout_str(i8*)
declare i32 @mongoose_cin()
declare i32 @mongoose_expt(i32, i32)
declare i32 @mongoose_assign(i32*, i32)
declare i32 @mongoose_or(i32, i32)
declare i32 @mongoose_and(i32, i32)
declare i32 @mongoose_not(i32)
declare i32 @mongoose_neg(i32)

declare i32 @putchar(i32)
declare i32 @printf(i8*,...)
declare i32 @puts(i8*)
@out_expr.s = constant [3 x i8] c\"%d\\00\"
@out_str.s = constant [3 x i8] c\"%s\\00\"\n"

let mutable compile_binop = fun (op,x,y,bvar,alpha,label) -> ("","","")
let mutable compile_uniop = fun (op,x,bvar,alpha,label) -> ("","","")
let mutable compile_ifelse = fun (c,t,f,bvar,alpha,label) -> ("","","")
let mutable compile_let = fun (c,t,f,bvar,alpha,label) -> ("","","")

let rec comp_llvm  (exp, bvar: string list, alpha:Map<string, string>,label) =
  match exp with
    | Val(n) ->
      ("",string(n),label)
    | Var(n) ->
      let mutable output = sprintf "Error, variable %s does not exist in this scope\n" n
      let reg = newreg()
      if alpha.ContainsKey(n) then
        let avar = alpha.[n]
        output <- sprintf "%s = load i32, i32* %s, align 4\n" reg avar
      (output,reg,label)
    | Binop(op,x,y) ->
      compile_binop(op,x,y,bvar,alpha,label)
    | Uniop(op,x) ->
      compile_uniop(op,x,bvar,alpha,label)
    | Ifelse(c,t,f) ->
      compile_ifelse(c,t,f,bvar,alpha,label)
    | Letexp(var,expr,next) -> //not lambda's
      compile_let(var, expr, next,bvar,alpha,label)
    | Assign(var, v) ->
      let (outv,destv,labelv) = comp_llvm(v,bvar,alpha,label)
      let mutable output = sprintf "Error, %s not defined" var
      if alpha.ContainsKey(var) then
        let avar = alpha.[var]
        output <- outv + sprintf "store i32 %s, i32* %%%s, align 4\n" destv var
      (output,destv,labelv)
    | Seq(head :: [Nothing]) ->
      comp_llvm(head,bvar,alpha,label)
    | Seq(head :: [tail]) ->
      let (outh,desth,labelh) = comp_llvm(head,bvar,alpha,label)
      let (outt,destt,labelt) = comp_llvm(tail,bvar,alpha,label)
      let output = outh + outt
      (output,destt,labelt)
    | _ ->
      (string(exp) + "\n ERROr ^not compile-able^\n","",label)

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

compile_uniop <- fun (op, x, bvar, alpha, label) ->
  let (outx,destx,xlabel) =
    match x with
      | Sym(s) ->
        ("","",label)
      | _ ->
        comp_llvm(x,bvar,alpha,label)
  let mutable output = outx
  let reg = newreg()
  match (op,x) with
    | ("COUT",Sym(s)) ->
      // FIXME proper string escaping
      let mutable str = s.[1..s.Length-2]
      let strname = newstring();
      // gross hack to get right length when string contains backslash escapes
      let strlen = str.Length - (Seq.length (Seq.filter ((=) '\\') str)) + 1
      str <- str.Replace("\\n","\\0A")
      defs <- (sprintf "%s = constant [%d x i8] c\"%s\00\"\n" strname strlen str) :: defs
      // can't use puts, appends newline
      //output <- output + sprintf "call i32 (i8*)\
      //  @puts(i8* getelementptr ([%d x i8], [%d x i8]* %s, i64 0, i64 0))\n" strlen strlen strname
      output <- output + sprintf "call i32 (i8*,...)\
        @printf(i8* getelementptr ([3 x i8], [3 x i8]* @out_str.s, i64 0, i64 0), i8* getelementptr ([%d x i8], [%d x i8]* %s, i64 0, i64 0))\n" strlen strlen strname
      (output,"0",label)
    | ("COUT",Val(n)) ->
      output <- output + sprintf "call i32 (i8*,...)\
        @printf(i8* getelementptr ([3 x i8], [3 x i8]* @out_expr.s, i64 0, i64 0), i32 %d)\n" n
      (output,"0",label)
    | _ ->
      (sprintf "uniop %s not implemented\n" op, "0", label)

compile_let <- fun (var, expr, next, bvar:string list, alpha, label) ->
  match expr with
      | Lambda(farg,body) ->
      (* totally unsure if works
        symtable <- symtable.Add(var,bvar) //add func name to symtable with its bvars (rec)
        let bvarnew = (bvar) @ [farg] //add local lamba term to bvars
        let (outb,destb,labelb) = comp_llvm(body,bvarnew,alpha,label) //compile body
        let mutable prms = ""
        for b in bvar do //cycle thru bvars and add them as params
          prms <- prms + ", i32* " + b
        let header = sprintf "define i32 @%s(i32 %%farg_%s%s) {{" var farg prms
        let mutable sfarg = sprintf "%%%s = alloca i32, align 4\n" farg
        sfarg <- sfarg + sprintf "store i32 %%farg_%s, i32 %s, align 4\n" farg farg //FORMAL PARAM‚add it as itself so that body recognizes it
        let func = header + sfarg + outb + sprintf "ret i32 %s\n" destb
        defs <- (defs) @ [func]
        comp_llvm(next,bvar,alpha,label) *)
        ("lambda not supported dude","","")
      | _ ->
        let mutable avar = "%" + var
        if alpha.ContainsKey(var) then 
          avar <- aConvert(var)
        //let (outc,destv,labelv) = comp_llvm(var,bvar,alpha,label)
        let (outexp,destexp,labelexp) = comp_llvm(expr,bvar,alpha,label) //let int
        let output = sprintf "%s = alloca i32, align 4\nstore i32 %s, i32* %s, align 4\n" avar destexp avar
        let alphanew = alpha.Add(var,avar)
        let bvarnew = (bvar) @ [avar]
        let (outn,destn,labeln) = comp_llvm(next,bvarnew,alphanew,labelexp)
        (output + outn,destn,labeln)

let boilerplate = "target datalayout = \"e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128\"
target triple = \"x86_64-pc-linux-gnu\"\n\n"

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
(* 
let x = 3: (x + x)
*)

//let tree = Letexp("x", Val 3, Binop("+", Var "x", Var "x"))
//let y = Var "y" //fsharp no likey tree
//let tree = Letexp("y", Val 0, Letexp("x", Val 2, Assign("y", Binop("+", y, Val 1)) ) )

let compile (tree:expr) =
  printfn "%s" (string tree)
  let alphamap = Map.empty<string, string>

  let res = comp_llvm (tree, [], alphamap, "")

  let top = sprintf "%s\n%s\ndefine i32 @main() {\n" includes (String.concat "" (List.rev defs))
  
  match res with
    | (code,res,label) ->
      let print = sprintf "call i32 (i8*,...)\
        @printf(i8* getelementptr ([3 x i8], [3 x i8]* @out_expr.s, i64 0, i64 0), i32 %s)\n\
        call i32 @putchar(i32 10)\n" res
      let output = sprintf "%s%s%s\n%s%s" boilerplate top code print bot
      output
