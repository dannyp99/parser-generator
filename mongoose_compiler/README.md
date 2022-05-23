# Mongoose Compiler

## Building

First, build your parser generator as described in `../README.md`:

```sh
# In `src/`:
$ ./compile.sh mongoose
$ ./build.sh ../mongoose_compiler/mg.grammar ../mongoose_compiler/setup/
```

Then, compile the compiler and move necessary files to the run directory:

```sh
# In `mongoose_compiler/setup`:
$ ./setup
```

The generated compiler will be in `mongoose_compiler/setup/run`.

## Running

Run the compiler with `mono` and pass your mongoose file to be compiled on the
command line.  It will produce a file in the current directory called `a.ll`
which can be compiled with `clang`.

```sh
# In `mongoose_compiler/setup/run`:
$ mono MongooseCompiler.exe /path/to/program.ms
$ clang a.ll
$ ./a.out
```
