# Mongoose Compiler

## Building

First, build your parser generator as described in ../README.md:

In `src/`:
```sh
./compile.sh mongoose
./build.sh ../mongoose_compiler/mg.grammar ../mongoose_compiler/setup/
```

Then, compile the compiler and move necessary files to the run directory:

In `mongoose_compiler/setup`:
```sh
./setup
```

The generated compiler will be in `mongoose_compiler/setup/run`.

## Running

Run the compiler with `mono` and pass your mongoose file to be compiled on the
command line.  It will produce a file in the current directory called `a.ll`
which can be compiled with `clang`.

In `mongoose_compiler/setup/run`:
```sh
mono MongooseCompiler.exe /path/to/program.ms
clang a.ll
./a.out
```
