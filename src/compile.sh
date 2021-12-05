#!/bin/bash

if [[ $# -gt 0 ]];then
    fsharpc "$1"/*.fs -a -o AbstractSyntax.dll
    mcs lexer/*.cs Parser.cs -r:AbstractSyntax.dll,Generate.dll -target:library -out:lexer.dll
    mcs "$1"/*.cs -r:AbstractSyntax.dll,lexer.dll,Generate.dll -out:ParGen.exe
fi