# Parser Generator

## Table of Contents

[Building the Grammar FSM](#building-the-grammar-fsm)

[Your Responsibility](#your-responsibility)

- [F# Abstract Syntax Tree](#f-abstract-syntax-tree)

- [Lexical Analyzer](#lexical-analyzer)
  - [Token Types](#token-types)

- [Main Function](#main-function)

[Compile & Test File](#compile--test-file)

## Building the Grammar FSM

For building the Finite State Machine, simply use the `build.sh` script with the appropriate arguments.

```bash
  ./build.sh ./path/to/file.grammar ./path/to/output/par.cs
```

This will generate the FSM in the par.cs file in the directory argument passed. If the folder doesn't exist it will create it for you.

## Your Responsibility

- F# Abstract Syntax Tree
- Lexical Analyzer that implements the `absLexer` interface
- Main to run your code.

### F# Abstract Syntax Tree

For the sake of simplicity we allow users to make a discrete union of the types for their Abstract Syntax Tree. **However**, creating one is not always necessary for simple grammars. If your Grammar uses C# types and doesn't need a f# file you are only required to create a f# file with the following code:

```fsharp
module FSEvaluator
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

### Lexical Analyzer

For full freedom we only require your Lexical Analyzer implements the absLexer interface:

```csharp
public interface absLexer
{
   lexToken next(); // returns null at eof. Should return a token of the appropriate type
   int linenum();
}
```

If you wish to fully leverage our lexical analyzer we allow you to extend the `simpleLexer` class which handles the `next()` and `linenum()`. **However**, you must override:

```csharp
public virtual lexToken translate_token(lexToken t)
```

This is because the `simpleLexer.next()` only returns of the token types below. You also need to have matching constructors but you can simply invoke the inherited constructor.

#### Token Types

- "Symbol"   (non-alphanumeric symbols such as *, +, ==, etc )
- "Keyword"  (while, if ,else, etc)
- "Alphanumeric"     (alphanumeric  x, x1, etc)
- "Integer"  (base-10 non-negative integers, - is a separate symbol)
- "Float"    (non-negative doubles 3.15)
- "StringLiteral" (double-quoted strings without nested ""'s)

An example of how we do this with calculator below

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
            run(t);
            Console.WriteLine("Result: "+t); 
        }
      }
  }
}
```

## Compile & Test File

After meeting these prerequisites, you can now use the `compile.sh` script to run the code.

```bash
./compile.sh folder
# ex: ./compile.sh mongoose
# this command will grab and compile the necessary files mentioned above to compile the ParGen.exe
```

This will generate `ParGen.exe` which you can run and pass files to to test your language code files.

```bash
mono ParGen.exe ./calculator/test1.calc
```
