LEX=./lexer

run: ParGen.exe
	-mono ParGen.exe
	@rm ParGen.exe # Quietly remove the executable
.PHONY: run

debug: ParGen.exe
	-mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 ParGen.exe
	@rm ParGen.exe # Quietly remove the executable
.PHONY: debug

ParGen.exe: lexer.dll
	csc -debug *.cs -r:lexer.dll,parser.dll -out:ParGen.exe

lexer.dll: $(LEX)/*.cs parser.dll
	csc -debug $(LEX)/*.cs -r:parser.dll -target:library -out:lexer.dll

parser.dll: parser.fs
	fsharpc parser.fs -a --out:parser.dll

clean:
	rm -rf *.exe *.dll
.PHONY: clean
