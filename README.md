# Parser Generator

Build and Run all with one make

```bash
make < ./path_to_file.grammar
```

If the `.exe` file is not automatically deleted, please do so to enforce make to recompile.

*Or* run `make clean` to enforce makefile to rebuild dlls and exe.

## Debugging

The `launch.json` is configured with the [ms-vscode.mono-debug](https://marketplace.visualstudio.com/items?itemName=ms-vscode.mono-debug) extension for VSCode please be sure to install the extension before attempting to debug.

Build and Debug all with one make

First run the debugger and then run the following command

```bash
make debug < ./path_to_file.grammar
```

If the `.exe` file is not automatically deleted, please do so to enforce make to recompile.

*Or* run `make clean` to enforce makefile to rebuild dlls and exe.