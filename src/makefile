BUILD=./build

all: lib
	mcs -r:$(BUILD)/Gitem.dll,$(BUILD)/Grammar.dll -out:Parser.exe StateAction.cs StateMachine.cs

lib: $(BUILD)
	mcs -target:library -out:$(BUILD)/Gitem.dll Gitem.cs
	mcs -r:$(BUILD)/Gitem.dll -target:library -out:$(BUILD)/Grammar.dll Grammar.cs

$(BUILD):
	mkdir -p $@

clean:
	rm -rf Grammar.exe $(BUILD)
.PHONY: clean
