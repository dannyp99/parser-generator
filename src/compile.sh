#!/bin/bash

if [[ $# -gt 0 ]];then
    count=`ls -1 "$1"/*.fs 2>/dev/null | wc -l`
    if [[ count -gt 0 ]];then
    	fsharpc "$1"/*.fs -a -o AbstractSyntax.dll
    	mcs lexer/*.cs Parser.cs -r:AbstractSyntax.dll,Generate.dll -target:library -out:lexer.dll
    	mcs "$1"/*.cs -r:AbstractSyntax.dll,lexer.dll,Generate.dll -out:ASTGenerator.exe
    else
        mcs lexer/*.cs Parser.cs -r:Generate.dll -target:library -out:lexer.dll
	mcs "$1"/*.cs -r:lexer.dll,Generate.dll -out:ASTGenerator.exe
    fi
    echo "Executable ASTGenerator.exe created"
fi
