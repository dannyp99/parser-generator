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
  "%v_" + x + string(counter)

let fConvert (x:string) =
  counter <- counter + 1
  "@" + x + string(counter)

type inst_type =
  | Cfunc of string
  | Comparison of string
  | Arith of string
  | Pow
  | Unknown of string

let transinst = function
  | "+" -> Arith("add i32")
  | "-" -> Arith("sub i32")
  | "*" -> Arith("mul i32")
  | "/" -> Arith("sdiv i32")
  | "%" -> Arith("srem i32")
  | "==" -> Comparison("icmp eq i32")
  | "<" -> Comparison("icmp slt i32")
  | "<=" -> Comparison("icmp sle i32")
  | "^" ->  Pow
  | x -> Unknown(x);;

let includes = "\
declare i32 @putchar(i32)
declare i32 @printf(i8*,...)
@out_expr.s = constant [3 x i8] c\"%d\\00\"
@out_str.s = constant [3 x i8] c\"%s\\00\"\n"

let mutable compile_while = fun (c,b,bvar,alpha,label) -> ("","","")
let mutable compile_ifelse = fun (c,t,f,bvar,alpha,label) -> ("","","")
let mutable compile_binop = fun (op,x,y,bvar,alpha,label) -> ("","","")
let mutable compile_bool = fun (op,x,y,bvar,alpha,label) -> ("","","")
let mutable compile_uniop = fun (op,x,bvar,alpha,label) -> ("","","")
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
        output <- outv + sprintf "store i32 %s, i32* %s, align 4\n" destv avar
      (output,destv,labelv)
    | Seq(head :: [Nothing]) ->
      comp_llvm(head,bvar,alpha,label)
    | Seq(head :: [tail]) ->
      let (outh,desth,labelh) = comp_llvm(head,bvar,alpha,label)
      let (outt,destt,labelt) = comp_llvm(tail,bvar,alpha,labelh)
      let output = outh + outt
      (output,destt,labelt)
    | App(func, arg) ->
      let reg = newreg()
      let (outa,desta,labela) = comp_llvm(arg,bvar,alpha,label)
      let mutable output = sprintf "function %s does not exist in this scope" func
      if alpha.ContainsKey(func) then
        output <- outa
        let afunc = alpha.[func]
        let mutable type_string = ""
        let mutable arg_string = ""
        let func_vars = symtable.[afunc]
        for b in func_vars do
          type_string <- type_string + ",i32*"
          arg_string <- arg_string + sprintf ", i32* %s" b
        output <- output + sprintf "%s = call i32 (i32%s) %s(i32 %s%s)\n" reg type_string afunc desta arg_string
      (output,reg,labela)
    | _ ->
      (string(exp) + "\n ERROr ^not compile-able^\n","",label)

compile_while <- fun (c,b,bvar,alpha,label) ->
  let start_cond = newlabel()
  let start_body = newlabel()
  let break_label = newlabel()
  let (outc,destc,labelc) = comp_llvm(c,bvar,alpha,start_cond)
  let (outb,destb,labelb) = comp_llvm(b,bvar,alpha,start_body)
  let cmp_reg = newreg()
  let mutable output = sprintf "br label %%%s\n" start_cond
  output <- output + sprintf "\n%s:\n%s" start_cond outc
  output <- output + sprintf "%s = icmp ne i32 %s, 0\n" cmp_reg destc
  output <- output + sprintf "br i1 %s, label %%%s, label %%%s\n" cmp_reg start_body break_label
  output <- output + sprintf "\n%s:\n%s" start_body outb
  output <- output + sprintf "br label %%%s\n" start_cond
  output <- output + sprintf "\n%s:\n" break_label
  (output, destb, break_label)

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
  output <- output + sprintf "%s = icmp ne i32 %s, 0\n" comparereg destc
  output <- output + sprintf "br i1 %s, label %%%s, label %%%s\n" comparereg start_true start_false
  output <- output + sprintf "\n%s:\n%s" start_true outt
  output <- output + sprintf "br label %%%s\n" rlbl
  output <- output + sprintf "\n%s:\n%s" start_false outf
  output <- output + sprintf "br label %%%s\n" rlbl
  output <- output + sprintf "\n%s:\n" rlbl
  let phi = sprintf " = phi i32 [%s, %%%s], [%s, %%%s]\n" destt end_true destf end_false
  output <- output + result + phi
  (output,result,rlbl)

compile_bool <- fun (op,x,y,bvar,alpha,label) ->
  let ft_lab = newlabel() // fallthrough
  let sc_lab = newlabel() // short circuit
  let (outx,destx,labelx) = comp_llvm(x,bvar,alpha,label)
  let (outy,desty,labely) = comp_llvm(y,bvar,alpha,ft_lab)
  let start_cmp = newreg()
  let ft_cmp = newreg()
  let sc_reg = newreg()
  let sc_fin = newreg()
  let (cmp_type,sc_const) =
    match op with
      | "&&" -> ("eq",0)
      | "||" -> ("ne",1)
      | _ ->
        printfn "bad operator %s passed to compile_bool" op
        ("ERROR",0)
  let mutable output =  outx
  output <- output + sprintf "%s = icmp %s i32 %s, 0\n" start_cmp cmp_type destx
  output <- output + sprintf "br i1 %s, label %%%s, label %%%s\n" start_cmp sc_lab ft_lab
  output <- output + sprintf "\n%s:\n%s" ft_lab outy
  output <- output + sprintf "%s = icmp ne i32 %s, 0\n" ft_cmp desty
  output <- output + sprintf "br label %%%s\n" sc_lab
  output <- output + sprintf "\n%s:\n" sc_lab
  output <- output + sprintf "%s = phi i1 [%d, %%%s], [%s, %%%s]\n" sc_reg sc_const labelx ft_cmp labely
  output <- output + sprintf "%s = zext i1 %s to i32\n" sc_fin sc_reg
  (output,sc_fin,sc_lab)

compile_binop <- fun (op, x, y, bvar, alpha, label) ->
  match op with
    | "while" ->
      compile_while(x,y,bvar,alpha,label)
    | "&&"|"||" ->
      compile_bool(op,x,y,bvar,alpha,label)
    | _ ->
      let mutable blabel = label
      let (outx,destx,xlabel) = comp_llvm(x,bvar,alpha,label)
      let (outy,desty,ylabel) = comp_llvm(y,bvar,alpha,xlabel)
      let top = transinst(op)
      let mutable output = sprintf "%s%s" outx outy
      let mutable reg = newreg()
      match top with
        | Arith(opstring) ->
          output <- output + sprintf "%s = %s %s, %s\n" reg opstring destx desty
        | Cfunc(opstring) ->
          output <- output + sprintf "%s = %s(i32 %s, i32 %s)\n" reg opstring destx desty
        | Comparison(opstring) ->
          let tmp_reg = reg
          reg <- newreg()
          output <- output + sprintf "%s = %s %s, %s\n" tmp_reg opstring destx desty
          output <- output + sprintf "%s = zext i1 %s to i32\n" reg tmp_reg
        | Pow ->
          lcx <- lcx - 1 // ignore new reg
          let pow_tree = Letexp( "pow_ax", x, Letexp( "pow_i", y, Binop( "while", Binop( "<", Val(1), Var("pow_i") ), Seq([ Assign( "pow_i", Binop( "-", Var("pow_i"), Val(1) ) ); Seq([ Assign( "pow_ax", Binop( "*", Var("pow_ax"), x ) ); Nothing ]) ]) ) ) )
          let (outpow,destpow,labelpow) = comp_llvm(pow_tree,bvar,alpha,ylabel)
          reg <- destpow
          output <- output + outpow
          blabel <- labelpow
        | Unknown(opstring) ->
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
      (output,"0",xlabel)
    | ("COUT",n) ->
      output <- output + sprintf "call i32 (i8*,...)\
        @printf(i8* getelementptr ([3 x i8], [3 x i8]* @out_expr.s, i64 0, i64 0), i32 %s)\n" destx
      (output,"0",xlabel)
    | ("-", n) ->
      let reg = newreg()
      output <- output + sprintf "%s = sub i32 0, %s\n" reg destx
      (output,reg,xlabel)
    | ("!", n) ->
      let cmp_reg = newreg()
      let ret_reg = newreg()
      output <- output + sprintf "%s = icmp eq i32 0, %s\n" cmp_reg destx
      output <- output + sprintf "%s = zext i1 %s to i32\n" ret_reg cmp_reg
      (output,ret_reg,xlabel)
    | _ ->
      (sprintf "uniop %s not implemented\n" op, "0", xlabel)

compile_let <- fun (var, expr, next, bvar:string list, alpha, label) ->
  match expr with
    | Lambda(farg,body) ->
      let func = fConvert(var)
      let alpha_new = alpha.Add(var,func)
      symtable <- symtable.Add(func,bvar) //add func name to symtable with its bvars (rec)
      let mutable afarg = "%" + farg
      afarg <- aConvert(farg)
      let alpha_local = alpha_new.Add(farg,afarg)
      let bvarnew = (bvar) @ [afarg] //add local lamba term to bvars
      let reg_count = lcx
      lcx <- 0
      let (outb,destb,labelb) = comp_llvm(body,bvarnew,alpha_local,label)
      lcx <- reg_count
      let mutable prms = ""
      for b in bvar do
        prms <- prms + ", i32* " + b
      let mutable func_def = sprintf "\ndefine i32 %s(i32 %%lambda_arg%s) {\n" func prms
      func_def <- func_def + sprintf "%s = alloca i32\n" afarg
      func_def <- func_def + sprintf "store i32 %%lambda_arg, i32* %s\n" afarg
      func_def <- func_def + outb
      func_def <- func_def + sprintf "ret i32 %s\n}\n" destb
      defs <- (defs) @ [func_def]
      comp_llvm(next,bvar,alpha_new,label)
    | _ ->
      let avar =  aConvert(var)
      let alpha_new = alpha.Add(var,avar)
      let (outexp,destexp,labelexp) = comp_llvm(expr,bvar,alpha,label) //let int
      let mutable output = outexp
      output <- output + sprintf "%s = alloca i32, align 4\nstore i32 %s, i32* %s, align 4\n" avar destexp avar
      let bvarnew = (bvar) @ [avar]
      let (outn,destn,labeln) = comp_llvm(next,bvarnew,alpha_new,labelexp)
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

  let res = comp_llvm (tree, [], alphamap, "0")

  let top = sprintf "%s\n%s\ndefine i32 @main() {\n" includes (String.concat "" (List.rev defs))
  
  match res with
    | (code,res,label) ->
      let print = sprintf "call i32 (i8*,...)\
        @printf(i8* getelementptr ([3 x i8], [3 x i8]* @out_expr.s, i64 0, i64 0), i32 %s)\n\
        call i32 @putchar(i32 10)\n" res
      let output = sprintf "%s%s%s\n%s%s" boilerplate top code print bot
      output
