# Changelog

## PeggleEdit 5.2.0 (Flamingo) (2026-11-27)

The biggest PeggleEdit release yet: levels are drawn as the game shows them, generators no longer need applying, levels are saved exactly as they were read, and PeggleEdit updates itself.

### Highlights

* Levels, emitters, movement and the masters' devices drawn as Peggle draws them
* Generators saved with their pegs and bricks, so levels with them are playable
* Levels read and saved losslessly
* Zoom, rotate, group, lock, and show or hide objects
* Copy and paste between levels and instances
* Open the games' own levels, export levels to XML, and set your own thumbnails and backgrounds
* Self-updating, from a Release or Nightly ring
* Sharp on high DPI displays, 64-bit, and runs on Linux with Mono or Wine

### Updates and installation

* [Feature] PeggleEdit updates itself
* [Feature] Choose a Release or Nightly update ring (Options)
* [Feature] Nightly builds
* [Change] 64-bit, and no longer needs the game's J2K library
* [Fix] #28, #6: PeggleEdit could not launch on Linux with Mono, and drew moving pegs wrongly under Wine

### Editing levels

* [Feature] #34: Copy and paste objects between levels and instances
* [Feature] #37: Zoom from 25% to 800% (View > Zoom, Ctrl++, Ctrl+-, Ctrl+0, Ctrl+mouse wheel)
* [Feature] #39: Rotate the selection with the mouse, with Ctrl for steps of 22.5 or 45 degrees
* [Feature] #40: Group and lock objects (Home > Group, Ctrl+G, Ctrl+L, add Shift to ungroup or unlock all)
* [Feature] #38: Show or hide each kind of object (View > Kinds)
* [Feature] #23: Hold Alt with the arrow keys to move objects in steps of 1/16
* [Feature] #14: Remove pegs no shot can reach (Tools > Remove > Unreachable Pegs)
* [Feature] #42: The Entry List is a table of each object's index, type, position, group and lock
* [Feature] Levels and packs with unsaved changes are marked with *
* [Change] Only ask to save when something has changed, including on close
* [Fix] #24: Peg Pen and Brick Pen drew away from the mouse when snapping to the grid
* [Fix] Pegs moved by a pixel as the mouse moved over them
* [Fix] The selection box was uneven around objects
* [Fix] The Entry List did not list a level's objects until it was clicked, and flickered while dragging
* [Fix] The Entry List and properties were not cleared when their level was closed
* [Fix] The View tab showed the previous level's view options

### Level objects and properties

* [Feature] All of the game's brick types
* [Feature] More of the game's settings in the property grid
* [Change] Rename Circle, Polygon, Rod and Teleport to Peggle's names: Ball, Poly, Line and Hole
* [Change] Property names and descriptions from PopCap's own level editor
* [Change] Variable, Crumble and Peg Type are with the object's other properties
* [Fix] Black could not be used as a colour, and level colours showed as transparent
* [Fix] Spelling of Vertical Radius, Spiral Count and Appearance

### Generators

* [Feature] Generators no longer need to be applied: levels are saved with the pegs and bricks they make
* [Feature] Preview generators as their pegs and bricks, moving
* [Feature] Generated pegs and bricks move together as one shape, including around an ellipse
* [Fix] #9: The Generator button was not highlighted while a generator tool was selected
* [Fix] #16: Undo, copy and paste turned a Peg Pen into a Brick Pen
* [Fix] #30, #33: Pasting a peg curve generator made a brick curve generator
* [Fix] #25: The peg generator warning on save was shown once per generator
* [Fix] Peg Pen and Brick Pen lost their position and interval when saved
* [Fix] Generators with a maximum of 0 or less hung PeggleEdit

### Drawing and preview

* [Feature] #36: Draw brick types, image rotation and poly rotation as the game does
* [Feature] Emitters run and drawn as the game does
* [Feature] Show the free ball bucket and the masters' devices (View > Power-ups)
* [Feature] Mark the objects Claude's flippers remove, and leave them out of the preview
* [Feature] Show the game's cannon, pointing at the mouse (View > Cannon)
* [Feature] Collision view shows the game's collision, including the walls, bucket and devices
* [Feature] 3D Mode, Transparent and Fast Forward (8x) view options
* [Change] Images not in the pack come from PeggleEdit's own copy, or the game's main.pak or folder
* [Change] The interface no longer shows the Super Guide portrait
* [Change] The preview plays only in the focused level, and pauses when PeggleEdit is in the background
* [Fix] The preview was slow with peg textures, especially zoomed on high DPI displays
* [Fix] Background and foreground objects were drawn in the wrong layer
* [Fix] Moving objects were blurred or had their edges cut off
* [Fix] Hole images were drawn in the wrong place
* [Fix] The preview drew invisible holes, and where holes send the ball
* [Fix] New emitters showed a debug box in the game

### Movement

* [Feature] Show a movement's Offset
* [Fix] Movement did not match the game, including arcs, cycles, vertical infinity, flower loop and phase
* [Fix] Swing Around Circle, Rotate Back And Forth and Rotate with a max angle were wrong when reversed, rotated or phased
* [Fix] Movement paths were drawn away from where objects go
* [Fix] Movement rotation of polys

### Level packs and the Pack Explorer

* [Feature] Open a game's main.pak to open its levels and challenges as a pack
* [Feature] Export levels to XML and import them back
* [Feature] Reorder levels, challenges and challenge pages by dragging
* [Feature] Extract a .pak to a folder, or pack a folder into one (orb menu > .pak Extract / Pack)
* [Fix] #31, #32: Images in the Pack Explorer could not be deleted
* [Fix] Export in an image's menu did nothing
* [Fix] The Pack Explorer did not show an opened pack from the top
* [Fix] Levels with the same file name were saved over each other
* [Fix] Close in the orb menu did not close the pack, and opening another pack did not ask first
* [Fix] Recent files listed the same file more than once
* [Fix] Open and Save As did not start in the open pack's folder
* [Fix] #22: A blank pack description stopped the first level of each stage from loading
* [Fix] #21: A pack's .cfg linked to a website that no longer exists
* [Fix] Challenge settings PeggleEdit does not show were lost when saved
* [Fix] Pack names and descriptions with a / were lost when saved

### Backgrounds and thumbnails

* [Feature] #43, #45: Import a background in the game's frame, then cover, fit or show it at 100%, place it with the mouse, and save it as PNG or JPG
* [Feature] #41: Import your own level thumbnails
* [Feature] #44: Export a level's background and thumbnail
* [Fix] 646 x 543 backgrounds were drawn a few pixels out

### Reading and saving levels

* [Fix] #35: Angles drifted slightly every load and save
* [Fix] Unknown data, and values matching the default, were lost when saved
* [Fix] Objects with a sound made a corrupt level
* [Fix] Non-ASCII text made a corrupt level
* [Fix] Early Pego levels (before file version 0x0F) were corrupted when saved
* [Fix] Entries that refer to another object were dropped when saved
* [Fix] Solid bricks became pegs, and objects without a shadow gained one, when saved
* [Fix] Curved bricks gained 4 curve points every save
* [Fix] Holes without a destination gained one when saved
* [Fix] Emitter colour changes were lost when saved
* [Fix] Copy and undo lost emitter rotation and the properties of lines
* [Fix] Copying a hole lost what its destination's movement follows
* [Fix] Some Peggle Deluxe levels could not be read
* [Fix] Damaged levels with deeply nested objects crashed PeggleEdit

### Command line (peggleedit-cli)

* [Feature] Convert levels to and from XML
* [Feature] render draws a level as the game shows it, optionally after a time and with the masters' devices
* [Fix] render could not draw .jp2 images

### Interface

* [Feature] Sharp on high DPI displays
* [Feature] Launch Peggle as well as Peggle Nights, and set where each is (Options > Games)
* [Change] Remove the unimplemented script button
* [Fix] Ribbon drop-downs did not close on a click elsewhere, Escape or switching application
* [Fix] Ribbon labels cut off letters, and check box labels were out of line
* [Fix] Dialogs and message boxes did not open centred over PeggleEdit
* [Fix] Opening a level left copies of it drawn over other windows
* [Fix] Dragged windows left a trail
* [Fix] A window drag interrupted by another window (e.g. Print Screen) stuck to the mouse

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
