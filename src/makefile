BUILD=./build

all: lib
	mcs -r:$(BUILD)/Gitem.dll  -out:Grammar.exe Grammar.cs StateAction.cs StateMachine.cs

lib: $(BUILD)
	mcs -target:library -out:$(BUILD)/Gitem.dll Gitem.cs

$(BUILD):
	mkdir -p $@

clean:
	rm -rf Grammar.exe $(BUILD)
.PHONY: clean
