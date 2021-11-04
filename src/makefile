LEX=./lexer

run: Parser.exe
#	-mono Parser.exe < ../test_grammars/java14.grammar
	-mono Parser.exe < ../mg.grammar
	@rm Parser.exe # Quietly remove the executable
.PHONY: run

Parser.exe: simpleLexer.dll
	mcs *.cs -r:simpleLexer.dll -out:Parser.exe

simpleLexer.dll: $(LEX)/*.cs
	mcs $(LEX)/*.cs -target:library -out:simpleLexer.dll

clean:
	rm -rf *.exe simpleLexer.dll
.PHONY: clean
