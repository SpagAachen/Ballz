![Ballz](https://spagaachen.github.io/static/img/ballz-logo.png)

Ballz is a 2D game similar to Worms. It is written in C# and MonoGame. It is tested on Windows and Linux. Other platforms might work. See below for build instructions.

The game is currently under development. Basic gameplay and multiplayer are working.

## Build Status:
| Linux / OSX | Windows |
| ----- | ------- |
| [![Build Status](https://travis-ci.org/SpagAachen/Ballz.svg?branch=master)](https://travis-ci.org/SpagAachen/Ballz) | [![Build status](https://ci.appveyor.com/api/projects/status/exmgex28ay9v20k8/branch/master?svg=true)](https://ci.appveyor.com/project/LukasBoersma/ballz/branch/master) |

## Before building:

On Windows, you need Visual Studio 2015.

On Linux, you need:
- Mono 4.6 or newer
- Nuget
- OpenAL
- SDL2

Make sure the git submodules are updated.

## Compiling the game:

Apart from the requirements listed above you should not need anything special to run the game. The build steps follow the usual procedure with .NET projects. In Visual Studio or MonoDevelop, you should be able to build and run everything without any further steps.

On Linux, you can also build from the command line:
```
nuget restore && xbuild
```

The binaries will be written to `Ballz.Core/bin/Debug/`. Use `mono Ballz.exe` in that directory to run the game.
