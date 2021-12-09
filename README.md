# Parser Generator

## Table of Contents

[Grammar Notes](#grammar-notes)

[Building the Grammar FSM](#building-the-grammar-fsm)

[Your Responsibility](#your-responsibility)

- [F# Abstract Syntax Tree](#f-abstract-syntax-tree)

- [Grammar Semantic Action](grammar-semantic-action)

- [Lexical Analyzer](#lexical-analyzer)
  - [Token Types](#token-types)

- [Main Function](#main-function)

[Compile & Test File](#compile--test-file)

## Grammar Notes

You have the ability to inject code to the top of your FSM file. This is to allow the following:

- Inject additional using statements you may need.
- Inject a `using static FSharpModule` that contains you Abstract Syntax Tree.
- Declare additional classes that you may want or need.
  - i.e. a global counter so you need a global class with a static counter.

An example of this for the calculator grammar is shown below.

```txt
\# ambiguous grammar for online calculator
!using static FSEvaluator; //Fsharp Module name
!
!public static class Counter {
!   public static int counter = 0;
!}
nonterminal E expr
terminal + - * ( )
typedterminal int expr
```

## Building the Grammar FSM

For building the Finite State Machine, simply use the `build.sh` script with the appropriate arguments.

```bash
  ./build.sh ./path/to/file.grammar ./path/to/output/par.cs
```

This will generate the FSM in the par.cs file in the directory argument passed. If the folder doesn't exist it will create it for you.

## Your Responsibility

- F# Abstract Syntax Tree
- Grammar Semantic Action
- Lexical Analyzer that implements the `absLexer` interface
- Main to run your code.

### F# Abstract Syntax Tree

For the sake of simplicity we allow users to make a discrete union of the types for their Abstract Syntax Tree. **However**, creating one is not always necessary for simple grammars. If your Grammar uses C# types and doesn't need a f# file simply don't inject one from the grammar. If you do use one name it the same as the one manual injected from the grammar, i.e.

```txt
!using static FSModule
```

```fsharp
module FSModule
```

To simplify the importing of F# code we recommend translator functions and for minimal changes an example of calculator is shown below:

```fsharp
module FSEvaluator
open System;;
open Microsoft.FSharp.Math;;
open System.Text.RegularExpressions;;

type expr = Val of int | Plus of (expr*expr) | Times of (expr*expr) | Subtract of (expr*expr) | Divide of (expr*expr) | Expt of (expr*expr) | Uminus of expr | Sym of String | EOF;;

let NewVal(a) = Val(a);
let NewPlus(a,b) = Plus(a,b);
let NewTimes(a,b) = Times(a,b);
let NewSubtract(a,b) = Subtract(a,b);
let NewDivide(a,b) = Divide(a,b);
let NewExpt(a,b) = Expt(a,b);
let NewUminus(a) = Uminus(a);;
```

### Grammar Semantic Action

As you work to create your own grammar for your language you will need to give the parser actions to perform when it encounters your reductions rules. To formulate your actions, refer back to your F# code for your abstract sytax tree and see what code your productions will use to create its parse trees. Below are two productions from the basic calculator grammar:

```txt
E --> int:n { return n; }
E --> E:e1 + E:e2 { return NewPlus(e1,e2); }
```

The code within the curly braces will be added to the reduction rules performed by the generated parser. These actions are critical as they are the building blocks for the parse tree. Think about what the code should be creating to reflect the reduction rules at each step. The reductions for the above grammar productions are represented as code in the parser as:

```csharp
rule.RuleAction = (pstack) => {expr n = (expr)pstack.Pop().Value; return n; };
```

```csharp
rule.RuleAction = (pstack) => {expr e2 = (expr)pstack.Pop().Value; pstack.Pop();  expr e1 = (expr)pstack.Pop().Value; return NewPlus(e1,e2); };
```

Note: per the grammar, 'int' in the first rule refers to a typed terminal of type expr and is not to be confused with the c# primitive int. Notice also that the given semantic actions are the last statements for each of these rules.

### Lexical Analyzer

For full freedom we only require your Lexical Analyzer implement the absLexer interface:

```csharp
public interface absLexer
{
   lexToken next(); // returns null at eof. Should return a token of the appropriate type
   int linenum();
}
```

If you wish to fully leverage our lexical analyzer we allow you to extend the `simpleLexer` class which handles the `next()` and `linenum()`. **However**, you must override:

```csharp
//Code from simpleLexer.cs
public virtual lexToken translate_token(lexToken t) { return t; }
```

This is because the `simpleLexer.next()` only returns of the token types below. You also need to have matching constructors but you can simply invoke the inherited constructor.

#### Token Types

- "Symbol"   (non-alphanumeric symbols such as *, +, ==, etc )
- "Keyword"  (while, if ,else, etc)
- "Alphanumeric"     (alphanumeric  x, x1, etc)
- "Integer"  (base-10 non-negative integers, - is a separate symbol)
- "Float"    (non-negative doubles 3.15)
- "StringLiteral" (double-quoted strings without nested ""'s)

An example of how we do this with calculator can be found below:

```csharp
using System;
using static FSEvaluator;
public class CalcLexer : simpleLexer {

    public CalcLexer() {}
    public CalcLexer(string s) : base(s) {}
    public CalcLexer(string a, string b): base(a,b) {}

    public override lexToken next() {
      var tok = base.next();
      return translate_token(tok);
    }

    public override lexToken translate_token(lexToken t)
    {
      if (t.token_type == "Integer") { t.token_type = "int"; t.token_value = NewVal((int)t.token_value); }
      else if (t.token_type == "Symbol") {
        t.token_type = (string) t.token_value;
      }
      else if (t.token_type == "Alphanumeric") {t.token_type = (string)t.token_value;}
      else if (t.token_type =="Keyword") { t.token_type = (string)t.token_value;}
      Console.WriteLine(t);
      return t;
    }
```

### Main Function

The final requirement is a class with a `Main` function to run the fsm and call the parser and pass an instance of your lexical analyzer. An example of the Calculator Main is shown below:

```csharp
using System; //consoles
using static FSEvaluator; //needed for expr
class Driver {
  public static void Main(string[] argv){
    string srcfile = "./test1.ms";
    if(argv.Length >= 1) { srcfile = argv[0]; }
      var Par = Generator.make_parser(); 
      if(Par != null) {
        expr t = (expr)Par.Parse(new CalcLexer(srcfile, "EOF"));
        if(t != null) {
          //FSPrint(t);
          run(t); //Not required, but recommended to
          //try interpreting simple expressions.
          Console.WriteLine("Result: "+t); 
        }
      }
  }
}
```

The `run(t)` will call the eval function we have for interpreting the Abstract Syntax Tree returned.

```fsharp
let rec eval = function 
  | Val(v) -> v
  | Plus(a,b) -> eval a + eval b
  | Times(a,b) -> eval a * eval b
  | Subtract(a,b) -> eval a - eval b
  | Divide(a,b) -> eval(a) / eval(b)
  | Expt(a,b) -> int(Math.Pow(float(eval a),float(eval b)))
  | Uminus(a) -> -1 * eval(a)
  | x -> raise (Exception(string(x)+"can't be evaluated"));

let run(e) = 
  printfn "%A" (eval(e));;
```

## Compile & Test File

After meeting these prerequisites, you can now use the `compile.sh` script to run the code.

```bash
./compile.sh folder
# ex: ./compile.sh calculator
# this command will grab and compile the necessary files mentioned above to compile the ASTGenerator.exe
```

This will generate `ASTGenerator.exe` which you can run and pass files to to test your language code files.

```bash
mono ASTGenerator.exe ./calculator/test1.calc
```
