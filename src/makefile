all:
	mcs -r:./build/Gitem.dll,./build/State.dll -out:Grammar.exe Grammar.cs

lib:
	mcs -target:library -out:build/Gitem.dll Gitem.cs
	mcs -r:./build/Gitem.dll -target:library -out:build/Grammar.dll Grammar.cs
	mcs -r:./build/Gitem.dll,./build/Grammar.dll  -target:library -out:build/State.dll StateAction.cs StateMachine.cs

clean:
	rm Grammar.exe build/*.dll
.PHONY: clean
