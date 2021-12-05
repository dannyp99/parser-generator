#!/bin/bash

if [[ $# -gt 1 ]];then
    if [[ -f *.exe ]]; then
        rm *.exe
    fi
    mcs Grammar.cs Gitem.cs StateMachine.cs StateAction.cs -out:Generate.exe
    mcs Grammar.cs Gitem.cs StateMachine.cs StateAction.cs -target:library -out:Generate.dll
    if [[ ! -d "$2" ]]; then
        mkdir -p "$2"
    fi
    mono Generate.exe < "$1" "$2"
else
    echo "Please pass a directory for where you want to save the FSM";
    echo "Ex. ./build.sh ./path/to/file.grammar ./path/to/output/par.cs"
fi