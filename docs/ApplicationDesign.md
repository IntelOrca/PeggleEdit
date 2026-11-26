# Application Design

PeggleEdit is written in C# with Windows Forms, and targets the .NET Framework 4.7.2. It runs as a 64-bit program on Windows, and on Linux with Mono or Wine. Build it with `dotnet build` using any recent .NET SDK.

## Projects
* **IntelOrca.PeggleEdit.Tools**: reads and saves level packs (`.pak`), levels (`.dat`) and level XML. It also draws levels and runs movement and emitters as the game does. It has no editor user interface.
* **peggleedit**: the editor itself, including the level editor control, the tool windows and the self-updater.
* **peggleedit-cli**: the [command line tool](CommandLine.md) for converting and drawing levels.
* **Crom.Controls**: the docking windows.
* **System.Windows.Forms.Ribbon**: the Office style ribbon.
* **tests/IntelOrca.PeggleEdit.Tools.Tests**: xUnit tests for the Tools project. Run them with `dotnet test`.

## Important classes
* **LevelPack**: a level pack, with its levels, images and challenges.
* **Level**: one level and its objects.
* **LevelEntry**: the base class of every level object (Ball, Brick, Poly, Line, Hole, Emitter and the generators).
* **Mover** and **MoverLink**: an object's movement, and links to other objects' movement.
* **LevelXml**: converts levels to and from XML, byte for byte.

## Saving levels exactly
PeggleEdit keeps everything it reads from a level, including values it does not understand. A level that is opened and saved without changes is written back exactly as it was. See [Level Format](LevelFormat.md) for how PeggleEdit stores its own data, such as generators, inside a level the game can still play.

## Images
JPEG 2000 (`.jp2`) images, which the game uses for backgrounds, are decoded with the CoreJ2K library, so the game does not need to be installed to open them.

## Builds and releases
Every push to `master` is built by GitHub Actions and published as the rolling `nightly` release. A `v*` tag publishes a full release. Each release has a `build.json` that the self-updater reads to find new versions.
