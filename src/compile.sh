#!/bin/bash

if [[ $# -gt 0 ]];then
    key="$1"
    case $key in
        mongoose)
            fsharpc mongoose/mongoose_base.fs -a -o Absyntax.dll
            mcs lexer/*.cs -r:AbstractSyntax.dll -target:library -out:lexer.dll
            mcs *.cs -r:AbstractSyntax.dll,lexer.dll -out:ParGen.exe
            ;;
    esac
fi