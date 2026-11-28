# Changelog

## PeggleEdit 5.2.0 (Flamingo) (2026-11-27)

The biggest PeggleEdit release yet. Levels are drawn and previewed as the game shows them, generators no longer need to be applied, levels are read and saved exactly as they were, and PeggleEdit can now keep itself up to date.

### Highlights

* Levels, emitters, movement and the masters' devices are drawn and previewed as Peggle draws them
* Generators are saved with the pegs and bricks they make, so levels with generators are playable
* Levels are read and saved losslessly, with many fixes for objects that saving changed or corrupted
* Zoom, rotate, group, lock and show or hide objects, and copy and paste between levels and instances
* Open the levels the games come with, export levels to XML, and set your own thumbnails and backgrounds
* PeggleEdit updates itself, from a Release or Nightly update ring
* Sharp on high DPI displays, 64-bit, and runs on Linux with Mono or Wine

### Updates and installation

* [Feature] PeggleEdit can now update itself when a new version is available
* [Feature] Choose a Release or Nightly update ring in Options
* [Feature] Nightly builds, published as a rolling "nightly" pre-release on every change
* [Change] PeggleEdit and peggleedit-cli run as 64-bit programs, and decode .jp2 images themselves, so no longer need the game's J2K library
* [Fix] #28, #6: PeggleEdit can not be launched on Linux with Mono, and draws moving pegs incorrectly under Wine

### Editing levels

* [Feature] #34: Copy and paste objects between levels and between instances of PeggleEdit
* [Feature] #37: Zoom the level in and out, from 25% to 800% (View > Zoom, Ctrl++, Ctrl+-, Ctrl+0, or Ctrl and the mouse wheel)
* [Feature] #39: Rotate the selected objects with the mouse, around their middle or a point you choose, by whole degrees or in steps of 22.5 or 45 degrees with Ctrl
* [Feature] #40: Group objects, so that they are selected together, and lock objects, so that they can not be selected or moved in the level (Home > Group, Ctrl+G, Ctrl+L)
* [Feature] #38: Show or hide each kind of object (View > Kinds). Hidden objects can not be selected in the level
* [Feature] #23: Hold ALT to move objects by 0.0625 (1/16) units
* [Feature] #14: Remove > Unreachable Pegs removes the pegs and bricks no shot from the cannon can reach without a bounce, worked out from how the game fires the ball
* [Feature] #42: The Entry List is a table, with columns for each object's index, type, position and group, and a padlock for locked objects
* [Feature] A level with unsaved changes has an * after its name on its tab, as does the pack in the title bar
* [Change] PeggleEdit asks to save only when something has changed, with Save, Don't Save and Cancel, and also when it is closed
* [Fix] #24: Drawing with the Peg Pen or Brick Pen while snapping to the grid is no longer away from the mouse
* [Fix] Pegs no longer move by a pixel and back as the mouse moves over them
* [Fix] The selection box is drawn on whole pixels, the same distance from the object wherever it is
* [Fix] The Entry List lists a level's objects as soon as the level is opened or switched to, rather than once it is clicked
* [Fix] The Entry List no longer flickers while objects are dragged
* [Fix] The Entry List and properties are cleared when the level they show is closed
* [Fix] The View tab shows the view options of the level that is focused, rather than those of the level before

### Level objects and properties

* [Feature] Choose any of the game's brick types: normal, outer and inner curved, their 90 variants, and straight
* [Feature] Show more of the game's settings in the property grid: peg type, brick anchor and end angles, poly normal direction, rotation, scale, grow type, mirror and image tile, and hole score, out delay and circular
* [Change] Rename Circle, Polygon, Rod and Teleport to Ball, Poly, Line and Hole, the names Peggle itself uses
* [Change] Use the names from PopCap's own level editor in the property grid, with clearer descriptions
* [Change] A peg's Variable, Crumble and Peg Type are shown with the object's other properties, rather than under Peg Properties
* [Fix] Use the game's defaults for solid and outline colours: black could not be used as a colour, and colours from the level showed as transparent
* [Fix] Spelling of the Vertical Radius, Spiral Count and Appearance labels

### Generators

* [Feature] Generators no longer need to be applied: levels are saved with the pegs and bricks they make, so they are playable, and PeggleEdit keeps the generators in its own data after the level
* [Feature] Preview shows generators as the pegs and bricks they make, moving as they will in the game
* [Feature] A generator's movement is passed on to its pegs and bricks, so that they move together as one shape: a rotating ring of bricks stays a ring
* [Feature] A generator that rotates or swings around an ellipse (a Y Radius different from its Radius) moves its pegs and bricks as one shape too
* [Feature] The Generator button is highlighted while a generator tool is selected
* [Fix] #16: Undo, copy and paste no longer turn a Peg Pen into a Brick Pen
* [Fix] #30, #33: Copy and paste of a peg curve generator always produced a brick curve generator
* [Fix] #25: The warning about peg generators when saving popped up once for each generator
* [Fix] Peg Pen and Brick Pen keep their position and interval when saved, and update straight away when the interval or path is changed
* [Fix] Peg and brick generators with a maximum of 0 or less no longer hang PeggleEdit

### Drawing and preview

* [Feature] #36: Draw each brick type with the shape the game gives it (outer and inner curved, their 90 variants, and straight bricks with tilted ends), and draw objects' image rotation and the rotation of polys that do not move, as the game does
* [Feature] Emitters are run and drawn as the game does: curves, colours, additive blending, rates, spirals, and drawn in the level's drawing order. They are shown settled while editing and run live in the preview
* [Feature] Show the free ball bucket, swinging in the preview, and the masters' devices: Kat Tut's pyramid, Claude's flippers, the lid and Marina's lightning rod (View > Power-ups)
* [Feature] Mark the objects Claude's flippers take away, and leave them out of the preview
* [Feature] Show the game's cannon, pointing at the mouse, to check it is clear of the level's objects (View > Cannon)
* [Feature] Collision mode shows the game's own collision: the walls, the bucket and the devices of the powers
* [Feature] 3D Mode view option to show emitters as the game does without 3D acceleration
* [Feature] Transparent view option to draw the level's objects half transparent over the background
* [Feature] Fast Forward view option to play the preview at the game's fast forward speed (8x)
* [Enhancement] Fall back to main.pak or the Peggle Nights directory for images
* [Change] The bucket, the devices of the powers, the cannon and the lightning rod are drawn from PeggleEdit's own copy of the game's images, so they are shown even when the game's files can not be found
* [Change] The interface no longer shows a master
* [Change] The preview plays only in the level that is focused, and pauses while another level or application is in front
* [Fix] The preview with peg textures is several times faster, especially zoomed on a high DPI display, so PeggleEdit stays responsive while it plays
* [Fix] Level objects are drawn in the game's layers: background objects behind the pegs, foreground objects in front
* [Fix] Pegs and images are drawn on whole pixels, so moving objects no longer blur or have their edges cut off or bleed
* [Fix] Hole images are drawn where the game draws them
* [Fix] The preview no longer draws invisible holes, or where holes send the ball
* [Fix] New emitters no longer show a debug box with a particle count in the game, and the preview shows the box where the game does

### Movement

* [Fix] Movement matches the game: arcs swing through their max angle, easing at each end (e.g. Death Defying), cycles and vertical infinity start where the game starts them, flower loop moves, and the phase is a whole tick as in the game
* [Fix] Preview of Swing Around Circle, Rotate Back And Forth and Rotate with a max angle matches the game when reversed, path rotated or with a phase, and the movement Offset is now shown
* [Fix] A movement's path is drawn through where the object goes, including a negative radius
* [Fix] Movement rotation of polys

### Level packs and the Pack Explorer

* [Feature] Open a game's main.pak to open the levels and challenges Peggle, Peggle Nights or Pego come with as a level pack, to look at, copy from, or save as a level pack of their own
* [Feature] Export levels to XML and import them back, losslessly (Pack Explorer: level > Export > XML, Levels > Import Level)
* [Feature] Reorder levels, challenges and challenge pages by dragging them in the Pack Explorer
* [Feature] New pak extract / pack tool
* [Fix] #31, #32: The right-click menu of images in the Pack Explorer did not appear, so images could not be deleted
* [Fix] Export in the menu of an image in the Pack Explorer did nothing
* [Fix] Pack Explorer shows a pack that is opened from the top, as the Entry List does another level's entries
* [Fix] New and imported levels are given a file name no other level in the pack has, and a pack is not saved while two of its levels share a file name, as one would be saved over the other
* [Fix] Close in the orb menu closes the level pack, and opening a recent level pack or importing one asks before closing the open pack
* [Fix] A file is only listed once in the recent files, however its path was written
* [Fix] Open and Save As start in the folder of the pack that is open
* [Fix] #22: A blank pack description stopped the first level of each stage from loading
* [Fix] #21: A pack's .cfg file links to PeggleEdit's GitHub page, rather than a website that no longer exists
* [Fix] Challenges keep the game's settings that PeggleEdit does not show (e.g. a score ceiling, a background override) when a pack is saved
* [Fix] Pack names and descriptions with a / in them are kept when a pack is saved

### Backgrounds and thumbnails

* [Feature] #43: Background > Import shows the image behind the game's frame, and keeps it at its own size or scales it to fill the screen (800 x 600) or the opening in the frame (646 x 543). Move and zoom it with the mouse to place it by hand, and save it as a PNG or a JPG of the quality you choose
* [Feature] #45: Background > Import can fit the image in its size with black around it, or show it at 100%, as well as cover it
* [Feature] #41: Set a level's thumbnail to an image of your own, kept when the level changes (Pack Explorer: level > Thumbnail > Import)
* [Feature] #44: Export a level's background and thumbnail (Pack Explorer: level > Background > Export, level > Thumbnail > Export)
* [Fix] A 646 x 543 background, the size of Peggle's own, is drawn where the game draws it, a few pixels from where it was

### Reading and saving levels

* [Fix] #35: Angles drifted slightly every time a level was loaded and saved
* [Fix] Unknown data, and values stored in the level that match the default, were lost from level objects when saved
* [Fix] Objects with a sound set produced a corrupt level when saved
* [Fix] Text containing non-ASCII characters produced a corrupt level
* [Fix] Early Pego levels (before file version 0x0F) were corrupted when saved
* [Fix] Level entries that refer to another object were dropped when saved
* [Fix] Solid (non-peg) bricks became pegs, and objects without a shadow gained one, when saved
* [Fix] Curved bricks gained 4 extra curve points every time the level was saved
* [Fix] Teleports without a destination gained a polygon destination when saved
* [Fix] Emitter colour changes were lost when saved
* [Fix] Copy and undo lost emitter rotation settings and all generic properties of rods
* [Fix] Copying a teleport keeps what its destination's movement follows
* [Fix] Some Peggle Deluxe levels could not be read
* [Fix] A damaged level with objects nested thousands deep is reported as unreadable, rather than crashing PeggleEdit

### Command line (peggleedit-cli)

* [Feature] Convert levels to and from XML
* [Feature] render draws a level as the game shows it, and can run the level for a time and show the bucket and the masters' devices
* [Fix] render draws .jp2 images

### Interface

* [Feature] PeggleEdit is DPI aware: it is sharp on high DPI displays, with the ribbon, windows, dialogs and level scaled to match
* [Change] Remove the unimplemented script button
* [Fix] Ribbon drop-downs close when clicking elsewhere, pressing Escape or switching to another application
* [Fix] Ribbon panel labels no longer cut off the tails of letters, and check box labels line up with their check boxes
* [Fix] Dialogs and message boxes open centred over PeggleEdit
* [Fix] A level that is opened is drawn once it is in place, rather than at each size and place it has on the way, which were left drawn over the other windows
* [Fix] A window dragged over the others no longer leaves a trail of itself over them
* [Fix] A window drag that another window takes the mouse from part way through (e.g. Print Screen) ends, rather than the window following the mouse until a click, which then failed

## PeggleEdit 5.1.1 (Eagle) (2023-09-13)

* [Feature] Add import / export of level packs to folder
* [Feature] Basic preview of emitters
* [Feature] Control points for rods, polygons, and teleports can now be moved in the editor
* [Enhancement] Show a warning if a level contains a generator
* [Fix] #12: Duel challenges do not load as duels
* [Fix] #15: Undoing moves teleports to 0,0
* [Fix] Preserve image format when loading and saving pak files

## PeggleEdit 5.1.0 (Dunnock) (2023-06-19)

* [Feature] #8: Automatically resize / crop background image
* [Feature] #13: Add a bezier pen tool for pegs and bricks
* [Feature] Add an eyedropper tool
* [Feature] Add separate tools for placing straight and curved bricks
* [Feature] Allow b, B and v keys to insert a brick to the currently selected brick (As seen in Pego editor)
* [Feature] Allow CTRL to always allow placement of a peg, SHIFT to prevent brick connection
* [Feature] Allow exporting to Pego and Peggle Deluxe
* [Feature] Connect brick to neighbour when placing a brick close to another
* [Feature] Show a ghost of the peg that is about to be placed
* [Feature] Use Peggle textures for pegs and bricks. Can be disabled in settings
* [Feature] Accept first command line argument as the .pak to open on launch
* [Feature] Add command line interface for unpacking .pak files
* [Enhancement] Improve hit testing of curve generators. Empty space no longer blocks placing of peg
* [Enhancement] Persist properties window scroll position when selection changes

## PeggleEdit 5.0.0 (Crow) (2023-04-10)

* [Feature] Add button to open GitHub page
* [Feature] Add new automatic update checking
* [Feature] Implement RetraceCircle movement type
* [Enhancement] Allow adding to selection with CTRL drag
* [Enhancement] Improve editing of movements
* [Enhancement] Improve error handling of launching Peggle Nights
* [Enhancement] Save and restore window layout
* [Change] Change config to be JSON file in AppData
* [Change] Rename IntelOrca.PeggleEdit.Designer.exe to just peggleedit.exe
* [Fix] .pak file cannot be opened if file is readonly
* [Fix] Challenge form, maximum for powers
* [Fix] Fix reading of bad CFG files
* [Fix] Nested movement logic
* [Fix] PAK file not added to recent files list when saving
* [Fix] Some level backgrounds did not open correctly (requires Peggle's J2K library)
