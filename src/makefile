LEX=./lexer
MONGOOSE=./mongoose

run: ParGen.exe
	-mono ParGen.exe run
	@rm ParGen.exe # Quietly remove the executable
.PHONY: run

fsm: ParGen.exe
	-mono ParGen.exe
	@rm ParGen.exe
.PHONY: fsm

debug: ParGen.exe
	-mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 ParGen.exe run
	@rm ParGen.exe # Quietly remove the executable
.PHONY: debug

ParGen.exe: lexer.dll
	mcs -debug *.cs -r:lexer.dll,parser.dll -out:ParGen.exe

lexer.dll: $(LEX)/*.cs parser.dll
	mcs -debug $(LEX)/*.cs -r:parser.dll -target:library -out:lexer.dll

parser.dll: $(MONGOOSE)/mongoose_base.fs
	fsharpc $(MONGOOSE)/mongoose_base.fs -a --out:parser.dll

clean:
	rm -rf *.exe *.dll
.PHONY: clean
