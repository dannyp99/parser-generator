module Compiler
open System;;
open System.Collections.Generic;;

type SymbolTable = Dictionary<string,SortedSet<string>>
type AlphaMap = Dictionary<string,string> //Alpha Conversion
type expr = Val of int | Plus of (expr*expr) | Times of (expr*expr) | Subtract of (expr*expr) | Divide of (expr*expr) | Expt of (expr*expr) | Uminus of expr | Sym of String | EOF;;

type LlvmCompiler =
    struct
        val mutable public counter: int32
        val mutable public defs: List<string>
        val mutable public symtable: SymbolTable

        new(c: int32) = {
            counter = c
            defs = new List<_>(1024)
            symtable = new Dictionary<_,_>(1024)
        }

        //don't need local for compile function
    end

type Compout = // Handle the end of function call 
    struct
        val mutable public out: string
        val mutable public dest: string
        val mutable public label: string

        member this.newRegister() =
            let x = 2;
            x
        //need val, binop and Ifelse
        member this.compile(exp:expr, closureVars: SortedSet<string>, alphaConvert: AlphaMap, bb: int32) : Tuple<string, string> =
            let x = 2; //Will need to clone alpha map since it's distructive and needs to branch and not effect things globally.
            ("Hello", "World")
        //Letexp(x,v,e) //x is the var, v is the expression e is the environment to close under. 
    end

// [<EntryPoint>]
// let main argv : int =
//     let srcfile = argv.[0]//"./mongoose/test1.ms";
//     let SLexer = simpleLexer(srcfile, "EOF")
//     //if(TRACE) { Console.WriteLine("SLexer is null? " + SLexer == null);}
//     let Par = Generator.make_parser()
//     //if(TRACE) { Console.WriteLine("Parser Generated"); } 
//     if Par != null then
//         let t = expr (Par.Parse(SLexer))
//         if t != null then
//             //FSPrint(t);
//             FSEvaluator.run t
//             //Console.WriteLine("Result: "+t) 
//     else 
//         Console.WriteLine("Error in Parser Generation. Parser Generator is null");
//     0;;