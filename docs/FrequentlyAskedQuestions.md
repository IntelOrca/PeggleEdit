
### What operating systems are supported? ###
  * Currently only Windows is supported. PeggleEdit is written in CSharp and .NET framework. It is possible to port it to other operating systems but a developer that has experience porting .NET applications would be required.
  * Level packs created with PeggleEdit should however still work on the Mac version of Peggle. Install them the same way your would install the official bonus levels.

### Will this work with Peggle 1? ###
Currently the editor does not make it easy to create levels for the original Peggle game. The game does not have levelpacks which means the only way of playing your own levels is to overwrite an existing quickplay level. This would involve extracting the files from the .pak file. PeggleEdit will hopefully add an easy way of doing this.

### Can I play my levels in the demo version of Peggle Nights? ###
Yes, the demo version of Peggle Nights does not restrict levelpacks. As long as the levels do not set their minimum stage above 2, it should be playable.

### I get an error when saving, "file is being used by another process" ###
This might be because Peggle Nights is running, if not something else has the .pak file open. Turn Peggle Nights off to overcome this error.

### How do I play my levels? ###
You can play your levels in PeggleNights by saving your level pack in the levelpacks folder in your Peggle Nights folder. By default this would be "C:\Program Files\PopCap Games\Peggle Nights\levelpacks\" ("Program Files (x86)" on a 64-bit version of Windows). If levelpacks does not exist, create it.

### I see my level but it says LOCKED ###
One reason for this may be because you have not completed the required number of stages set by the level. Any level should be playable if the minimum stage on the level properties is set to -3.

### When I play my level, I get an error saying invalid save game? ###
  * This error occurs when Peggle cannot read the level file. This may be because of a number of reasons. Due to the complexity of the file structure, it is easy for a small error to cause the whole level to be unreadable. The best way to tackle this is to delete objects that are mostly likely causing the error in the level and try a process of elimination until you find out what is causing it.
  * You will get this error if you have a [generator](Generators.md) in your level which hasn't been applied. [Generators](Generators.md) are PeggleEdit objects for designing levels, they are not understood by Peggle. If you have saved a level with an unapplied generator then the level will not be able to be loaded into Peggle.
  * If you still think there is a problem due to PeggleEdit, please submit a bug report with your level as an attachment on the issues page.

### When I start Peggle, I get an error about duplicate challenge ID ###
This means that out of all the challenges in the game and in all the level packs, two of them have the same ID. Make sure that all your challenges have a unique ID. Picking an ID between 500 and 10000 should lower the chances of a clash.

### Is there any faster way to repeatedly test my level? ###
Because Peggle Nights stops its level packs from being overwritten, you have to keep closing and re-opening the game. Another way to test your level is to extract main.pak found in the PeggleNights folder and delete the .pak file so that Peggle Nights reads the individual files in the folder rather than in main.pak. Then you can export your level and save it over the top of an existing quick-play level such as "bjorn1.dat". Then you simply reload the level in the game.

### Why do the images I import for objects have white squares behind them? ###
Your images need to be transparent. If they are not transparent then you will get this effect. The preferred image type is PNG which supports an alpha channel. Applications such as GIMP or Inkscape allow you to save transparent PNG files. See [GIMP Transparency](http://docs.gimp.org/en/gimp-using-web-transparency.html) for a guide to saving transparent images in GIMP.