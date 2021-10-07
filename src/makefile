Grammar.exe: Grammar.cs State.dll Gitem.dll
	mcs -r:Gitem.dll,State.dll Grammar.cs
lib:
	mcs -target:library -out:Gitem.dll Gitem.cs
	mcs -r:Gitem.dll,Grammar.dll  -target:library -out:State.dll StateAction.cs StateMachine.cs
#Gitem.dll: Gitem.cs
#StateAction.dll StateMachine.dll: StateAction.cs StateMachine.cs
clean:
	rm ./*.exe
