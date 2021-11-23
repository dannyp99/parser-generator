# Parser Generator

Build the FSM first if it doesn't exist or needs to be regenerated.

```bash
make fsm < ./path_to_file.grammar
```

Run the parser with a simple `make`

```bash
make
```

**NOTE** You will have to change the `srcfile` variable in the `StateMachine.cs` to the text file you want to test.

If the `.exe` file is not automatically deleted, please do so to enforce make to recompile.

*Or* run `make clean` to enforce makefile to rebuild dlls and exe.

## Parsers

In the case of running different grammars that need different values from the previous grammar you just need to modify the parser.dll target in the `makefile`.

```makefile
parser.dll: /path/to/fsharp/parser/file.fs
    fsharpc /path/to/fsharp/parser/file.fs -a -out:parser.dll
```

This file should have the following for the Parse Tree:

- A disecrete union handling the different types that will be used.
- Constructors for C# integrations
- Eval and Run functions are needed to **evaluate the Parse Tree.**

## Lexer

The only requirement here is that if needed you must write a `translate_token(lexToken t)` function to translate the token types to those of your discrete union. The constructors of this class can simply call the parent class constructor. Use `calcLexer.cs` in `./lexer` as an example (see [Token Types](#token-types)).

- The `makefile` will compile all C# code in the lexer folder. To swap out lexers simple `mv` the class that implements the `translate_token(lexToken t)` you want in the `./lexer` folder and move any other classes into `./oldLexer`.
- In rare instances you may need to change the `simpleLexer.cs` if the operator for your grammar is not included.
  - One example was that `<<` commonly used in C++ was not originally detectable.

### Token Types

- "Symbol"   (non-alphanumeric symbols such as *, +, ==, etc )
- "Keyword"  (while, if ,else, etc)
- "Alphanumeric"     (alphanumeric  x, x1, etc)
- "Integer"  (base-10 non-negative integers, - is a separate symbol)
- "Float"    (non-negative doubles 3.15)
- "StringLiteral" (double-quoted strings without nested ""'s)

## Debugging

The `launch.json` is configured with the [ms-vscode.mono-debug](https://marketplace.visualstudio.com/items?itemName=ms-vscode.mono-debug) extension for VSCode please be sure to install the extension before attempting to debug.

Build and Debug all with one make

First run the debugger and then run the following command

```bash
make debug
```

If the `.exe` file is not automatically deleted, please do so to enforce make to recompile.

*Or* run `make clean` to enforce makefile to rebuild dlls and exe.