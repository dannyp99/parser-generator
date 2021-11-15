LEX=./lexer

run: ParGen.exe
	-mono ParGen.exe
	@rm ParGen.exe # Quietly remove the executable
.PHONY: run

ParGen.exe: lexer.dll
	mcs *.cs -r:lexer.dll,parser.dll -out:ParGen.exe

lexer.dll: $(LEX)/*.cs parser.dll
	mcs $(LEX)/*.cs -r:parser.dll -target:library -out:lexer.dll

parser.dll: parser.fs
	fsharpc parser.fs -a --out:parser.dll

clean:
	rm -rf *.exe *.dll
.PHONY: clean
