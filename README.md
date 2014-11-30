# Exor #
_Exor_ is two related libraries:

- **Exor.Core** is a simple, attribute-driven extension framework. Implemented as a PCL
  for cross-platform usage, Exor.Core allows you to define type mappings in a deferred
  manner, i.e. types defined in `BaseModule` can be subclassed in `AddinModule` and
  those subtypes loaded in response to a string-based attribute key.
- **Exor.Compiler** is a runtime compiler and ExtensionLoader factory. Intended for
  runtime compilation of game scripts written in C#, Exor.Compiler allows a game to
  pack "loose" .cs files and compile them for inclusion in the game's ExtensionLoader.
  Using Exor.Compiler on desktops, where `CSharpCodeProvider` is available, allows
  you to trivially extend your projects. At the same time, the same code used in the
  dynamic loader on desktops can be added statically for platforms such as iOS where
  dynamic code loading isn't allowed--making mods on desktops easy without making life
  harder when you need to port to iOS, XBox One, or the PlayStation 4.
  
## Installation ##
[Exor.Core](https://www.nuget.org/packages/EdCanHack.Exor.Core) and
[Exor.Compiler](https://www.nuget.org/packages/EdCanHack.Exor.Compiler) are both available
on NuGet. To install, run the following command in the Package Manager Console:

```
PM> Install-Package EdCanHack.Exor.Core
PM> Install-Package EdCanHack.Exor.Compiler
```

## Usage ##
Exor comes with a set of tests that demonstrate (along the happy paths, at least) how to
use it in both compile-time and dynamically-loading situations.

- [Compiled Example](https://github.com/eropple/Exor/blob/master/Exor.Compiler.Tests/CompilerTests.cs)
- [Dynamic Example](https://github.com/eropple/Exor/blob/master/Exor.Compiler.Tests/CompilerTests.cs)

## Future Work ##
- Exor.Core currently operates on the assumption that extension 

- Implement inclusion strategies for DynamicLoaders. (They're stubbed out, but currently
  just throw `NotImplementedException`.)
- Implement a .csproj ICodeSource. This isn't currently done because it's a bunch of XML
  parsing; using Microsoft.Build isn't cross-platform (xbuild isn't 100% compatible).