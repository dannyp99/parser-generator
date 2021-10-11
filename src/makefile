BUILD=./build

all: lib
	mcs -r:$(BUILD)/Gitem.dll,$(BUILD)/State.dll -out:Grammar.exe Grammar.cs

lib: $(BUILD)
	mcs -target:library -out:$(BUILD)/Gitem.dll Gitem.cs
	mcs -r:$(BUILD)/Gitem.dll -target:library -out:$(BUILD)/Grammar.dll Grammar.cs
	mcs -r:$(BUILD)/Gitem.dll,$(BUILD)/Grammar.dll  -target:library -out:$(BUILD)/State.dll StateAction.cs StateMachine.cs

$(BUILD):
	mkdir -p $@

clean:
	rm -rf Grammar.exe $(BUILD)
.PHONY: clean
