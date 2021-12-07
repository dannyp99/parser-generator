#!/usr/bin/env sh

rm -rf run
mkdir run

( cd ../; fsharpc -a llvm_comp.fs )
mv ../llvm_comp.dll run
mv ../FSharp.Core.dll run

cp ../../src/lexer.dll run
cp ../../src/Generate.dll run
mcs *.cs -r:run/lexer.dll,run/Generate.dll,run/llvm_comp.dll -out:run/MongooseCompiler.exe
