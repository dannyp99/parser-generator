#!/bin/bash
mcs *.cs -r:AbstractSyntax.dll,lexer.dll,all.dll -out:main.exe