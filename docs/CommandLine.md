# Command Line Tool

PeggleEdit comes with **peggleedit-cli.exe**, a command line tool for working with level files without opening the editor. Most people will not need it.

## Converting levels to and from XML
```
peggleedit-cli to-xml <level.dat>... [-o <output>] [--force]
peggleedit-cli to-dat <level.xml>... [-o <output>] [--force]
```
`to-xml` turns Pego, Peggle and Peggle Nights level files into XML, a text format you can read and edit. `to-dat` turns them back. Converting a level to XML and back gives exactly the same file.

## Drawing levels as pictures
```
peggleedit-cli render <level.dat|level.xml>... [-o <output>] [--force]
                      [--peggle <dir>] [--no-interface] [--time <seconds>]
                      [--no-particles] [--software] [--devices <list>]
```
`render` saves an 800 x 600 PNG of each level, as the game shows it at the start of play. The background is the image next to the level with the same name.

## Options

| Option | What it does |
|---|---|
| `-o`, `--output <path>` | Where to save. When converting several files, this is a folder. |
| `-f`, `--force` | Overwrite files that already exist. |
| `--peggle <dir>` | The game's folder, for finding images. Not needed if the level is in the game's `levels` folder. |
| `--no-interface` | Leave out the game's interface around the level. |
| `--time <seconds>` | Show the level this many seconds after it starts, with objects moved and emitters running. |
| `--no-particles` | Leave out emitter particles. |
| `--software` | Show emitters as the game does without 3D acceleration. |
| `--devices <list>` | Choose what to show instead of the bucket, separated by commas: bucket, pyramid, flippers, flippersraised, cover, lightningrod, or none. |
| `-h`, `--help` | Show the help. |

## The editor's own commands
* `peggleedit.exe <pack.pak>` starts the editor with that pack open.
* `peggleedit.exe unpack <file.pak>` extracts every file in a `.pak` into the folder the `.pak` is in.
