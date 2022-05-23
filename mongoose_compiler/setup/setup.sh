#!/usr/bin/env sh

rm -f run/*
mkdir -p run

fsharpc -a ../llvm_comp.fs -o run/llvm_comp.dll

cp ../../src/lexer.dll run
cp ../../src/Generate.dll run
mcs *.cs -r:run/lexer.dll,run/Generate.dll,run/llvm_comp.dll -out:run/MongooseCompiler.exe
