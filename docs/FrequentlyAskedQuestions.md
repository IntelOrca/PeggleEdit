# Frequently Asked Questions

### Which computers can run PeggleEdit?
Windows 7 or later. It also runs on Linux with Mono or Wine. Level packs made with PeggleEdit also work in the Mac version of Peggle Nights. Install them the same way as the official bonus levels.

### How do I play my levels?
Save your level pack in the `levelpacks` folder inside your Peggle Nights folder, then start the game. If there is no `levelpacks` folder, create one. The Peggle Nights folder is usually one of these:
* `C:\Program Files (x86)\PopCap Games\Peggle Nights`
* `C:\Program Files (x86)\Steam\steamapps\common\Peggle Nights`

### I get an error when saving: "file is being used by another process"
Peggle Nights is probably running and has the pack open. Close the game and save again. If the game is not running, another program has the file open.

### My level says LOCKED
The level needs more stages to be completed first. Set its minimum stage to -3 in the level's **Properties** (right-click it in the Pack Explorer) and it can always be played.

### Peggle says "invalid save game" when I play my level
The game could not read the level. Please report it on the [issues page](https://github.com/IntelOrca/PeggleEdit/issues) with your pack attached. To find what causes it, try deleting objects from a copy of the level until it works.

Levels with [generators](Generators.md) used to cause this error. They no longer do, so you do not need to apply generators before playing.

### Peggle says there is a duplicate challenge ID
Two challenges, in the game or in any of your level packs, have the same ID. Give each of your challenges its own ID. A number between 500 and 10000 makes a clash unlikely.

### Can I make levels for the first Peggle, Peggle Deluxe or Pego?
Partly. PeggleEdit can open the levels these games come with (open their `main.pak`), and can export a level for each of them (right-click the level, then **Export**). These games do not have level packs, so to play your level you need to put it in place of one of the game's own levels. Back up the game first.

### Can I play my levels in the Peggle Nights demo?
Yes. The demo allows level packs, as long as your levels do not need a stage higher than 2 to be unlocked.

### Is there a quicker way to test my level over and over?
The game does not let a level pack change while it is running, so you have to close and reopen it each time. A quicker way is to extract the game's `main.pak` into the Peggle Nights folder with **.pak Extract / Pack** in the orca menu, then rename `main.pak` so the game uses the extracted files instead. Then export your level over one of the game's Quick Play levels (such as `levels\bjorn1.dat`) and reload that level in the game. Back up the game first.

### How do I see how a level will look in the game?
Turn on **Preview** on the **View** tab. Objects move and emitters run as they will in the game. Use **Power-ups** on the same tab to see the bucket and the masters' powers.

### What size should my level art be?
800 x 600, the size the game runs at. To see where the game's interface will cover your art, put [this overlay](img/bg_template.png) on top of it as a layer in your image editor.

**Set Background** resizes and crops any image to 800 x 600 for you.

### Why do my images have white boxes around them?
The image is not transparent. Save it as a PNG with a transparent background. Free programs such as [GIMP](https://www.gimp.org/) and [Inkscape](https://inkscape.org/) can do this.
