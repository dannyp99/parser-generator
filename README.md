# Parser Generator

Build the FSM first if it doesn't exist or needs to be regenerated.

```bash
make fsm < ./path_to_file.grammar
```

Run the parser with a simple `make`

```bash
make
```

If the `.exe` file is not automatically deleted, please do so to enforce make to recompile.

*Or* run `make clean` to enforce makefile to rebuild dlls and exe.

## Debugging

The `launch.json` is configured with the [ms-vscode.mono-debug](https://marketplace.visualstudio.com/items?itemName=ms-vscode.mono-debug) extension for VSCode please be sure to install the extension before attempting to debug.

Build and Debug all with one make

First run the debugger and then run the following command

```bash
make debug
```

If the `.exe` file is not automatically deleted, please do so to enforce make to recompile.

*Or* run `make clean` to enforce makefile to rebuild dlls and exe.