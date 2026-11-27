# Level Packs

A level pack is a single `.pak` file that holds your levels, their backgrounds, any images they use, and your challenges. Peggle Nights loads every pack in its `levelpacks` folder.

You work with the pack in the **Pack Explorer**. Right-click anything in it to see what you can do.

## Opening and saving
* **Open** opens a level pack. It can also open a game's `main.pak` (or its `levels\stages.cfg` if it has been extracted). This opens the levels and challenges that Peggle, Peggle Nights or Pego come with, so you can look at them, copy from them, or save them as a pack of your own. PeggleEdit will not save over the game's own files.
* **Save** and **Save As** save the pack. PeggleEdit only asks you to save when something has changed, and a `*` in the title bar shows unsaved changes.
* Close Peggle Nights before saving, or saving will fail because the game has the pack open.

## The pack
Right-click the pack's name to:
* **Properties**: set the pack's name and description.
* **Export**: save the whole pack as ordinary files in a folder.
* **Import**: open a pack that was exported to a folder.

## Levels
* **Add Level** makes a new, empty level.
* **Import Level** adds a level file (`.dat`) or a level exported as XML (`.xml`).
* Double-click a level to open it.
* Drag levels to change their order. You can also right-click and choose **Move Up** or **Move Down**, or press **Ctrl+Up** or **Ctrl+Down**.

Right-click a level to:
* **Properties**: set its display name, file name, minimum stage and ace score. Every level in a pack needs its own file name. PeggleEdit gives new and imported levels one, and will not let two levels share a file name.
* **Background > Import**: pick an image. It is shown behind the game's frame, as the game will show it. Choose its size: the whole screen (800 x 600), just inside the frame (646 x 543), or its own size. Then, unless you keep its own size, choose whether it covers that size, cutting off what is left over, fits in it with black around it, or is at 100%, where the game would show it at its own size. Drag it to move it, or use the mouse wheel to zoom it; an image kept at its own size is then saved as 800 x 600. Save it as a PNG, which keeps every pixel, or a smaller JPG, whose loss of detail the preview shows.
* **Background > Export**: save the background as it is stored in the pack, or as a PNG.
* **Thumbnail > Import**: pick an image to show when choosing the level in the game, instead of a picture of the level. It is resized and cropped to the game's 200 x 177 for you, and kept when you change the level. **Thumbnail > Reset** goes back to a picture of the level.
* **Thumbnail > Export**: save the thumbnail the pack will be saved with, yours or the picture of the level.
* **Export**: save the level on its own for Pego, Peggle Deluxe or Peggle Nights, or as XML.
* **Delete** it. You can also press **Delete**.

The XML export is a readable text copy of the level. Importing it gives back exactly the same level, so it is handy for comparing changes or keeping levels in version control.

## Images
Images that your objects use go in the **Images** folder. Right-click it and choose **Add Image** to add a PNG. Double-click an image to open it, or right-click it to rename, export or delete it. To use an image on an object, see [Level Objects](LevelEntries.md#using-images).

Use PNG images with a transparent background. Otherwise the image is drawn as a solid rectangle.

## Challenges
Challenges are grouped into pages, like in the game.
* Right-click **Challenges** and choose **Add Challenge Page**.
* Right-click a page and choose **Add Challenge**.
* Right-click a page or challenge and choose **Properties** to change it.
* Drag challenges and pages to change their order, or move a challenge to another page. **Move Up**, **Move Down**, **Ctrl+Up** and **Ctrl+Down** work here too.

A challenge's **Properties** set its name and description, which levels it uses, how many balls you get, what you need to do to win (such as clearing all orange pegs or reaching a score), any opponents, which power-ups are given, changes to gravity and ball speed, and other rules such as covering the free ball bucket.

Every challenge needs an ID that no other challenge uses, in the game or in any other pack. A number between 500 and 10000 makes a clash unlikely.

## Extracting and packing .pak files
**.pak Extract / Pack** in the orca menu extracts every file in a `.pak` to a folder, or packs a folder back into a `.pak`. This works for the game's own `.pak` files too.
