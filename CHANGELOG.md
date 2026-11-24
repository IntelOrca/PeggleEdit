PeggleEdit 5.2.0 (Flamingo) (TBC)

* [Feature] Add new pak extract / pack tool
* [Feature] #23: Hold ALT to move objects by 0.125 units
* [Feature] #34: Copy and paste objects between levels and between instances of PeggleEdit
* [Feature] Nightly builds, published as a rolling "nightly" pre-release on every change
* [Feature] Choose a Release or Nightly update ring in Options
* [Feature] PeggleEdit can now update itself when a new version is available
* [Feature] PeggleEdit is DPI aware: it is sharp on high DPI displays, with the ribbon, windows, dialogs and level scaled to match
* [Feature] Choose any of the game's brick types: normal, outer and inner curved, their 90 variants, and straight
* [Feature] Show more of the game's settings in the property grid: peg type, brick anchor and end angles, poly normal direction, rotation, scale, grow type, mirror and image tile, and hole score, out delay and circular
* [Feature] Export levels to XML and import them back, losslessly (Pack Explorer: level > Export (XML), Levels > Import Level)
* [Feature] peggleedit-cli command line tool for converting levels to and from XML
* [Feature] Generators no longer need to be applied: levels are saved with the pegs and bricks they make, so they are playable, and PeggleEdit keeps the generators in its own data after the level
* [Feature] Preview shows generators as the pegs and bricks they make, moving as they will in the game
* [Feature] A generator's movement is passed on to its pegs and bricks, so that they move together as one shape: a rotating ring of bricks stays a ring
* [Feature] Emitters are run and drawn as the game does: curves, colours, additive blending, rates, spirals, and drawn in the level's drawing order. They are shown settled while editing and run live in the preview
* [Feature] Show the free ball bucket, swinging in the preview, and the masters' devices: Kat Tut's pyramid, Claude's flippers, the lid and Marina's lightning rod (View > Power-ups)
* [Feature] Mark the objects Claude's flippers take away, and leave them out of the preview
* [Feature] 3D Mode view option to show emitters as the game does without 3D acceleration
* [Feature] peggleedit-cli render can run a level for a time, and show the bucket and the masters' devices
* [Fix] New emitters no longer show a debug box with a particle count in the game, and the preview shows the box where the game does
* [Fix] Level objects are drawn in the game's layers: background objects behind the pegs, foreground objects in front
* [Fix] The preview no longer draws invisible holes, or where holes send the ball
* [Fix] Hole images are drawn where the game draws them
* [Fix] peggleedit-cli render draws .jp2 images
* [Change] Rename Circle, Polygon, Rod and Teleport to Ball, Poly, Line and Hole, the names Peggle itself uses
* [Change] Use the names from PopCap's own level editor in the property grid, with clearer descriptions
* [Enhancement] Fall back to main.pak or Peggle Nights directory for images
* [Change] Remove the unimplemented script button
* [Fix] Ribbon drop-downs close when clicking elsewhere, pressing Escape or switching to another application
* [Fix] Ribbon panel labels no longer cut off the tails of letters, and check box labels line up with their check boxes
* [Fix] Fix #25: Warning about peg generators when saving pops up multiple times
* [Fix] Fix #32: right-click context menu not appearing on image nodes in Pack Explorer
* [Fix] Fix #33: copy+paste of PegCurveGenerator always producing BrickCurveGenerator
* [Fix] Fix reading of some deluxe levels
* [Fix] Fix movement rotation of polygon
* [Fix] Fix deletion of image in pack explorer
* [Fix] Fix saving of objects with a sound set producing a corrupt level
* [Fix] Fix curved bricks gaining 4 extra curve points every time the level is saved
* [Fix] Fix teleports without a destination gaining a polygon destination when saved
* [Fix] Fix emitter colour changes being lost when saved
* [Fix] Fix copy / undo losing emitter rotation settings and all generic properties of rods
* [Fix] Fix #35: Angles drift slightly every time a level is loaded and saved
* [Fix] Fix #22: Blank pack description prevents the first level of each stage from loading
* [Fix] Fix #28, #6: PeggleEdit can not be launched on Linux with Mono, and draws moving pegs incorrectly under Wine
* [Fix] Fix solid (non-peg) bricks becoming pegs, and objects without a shadow gaining one, when saved
* [Fix] Peg Pen and Brick Pen keep their position and interval when saved, and update straight away when the interval or path is changed
* [Fix] Peg and brick generators with a maximum of 0 or less no longer hang PeggleEdit
* [Fix] Preview of Swing Around Circle, Rotate Back And Forth and Rotate with a max angle matches the game when reversed, path rotated or with a phase, and the movement Offset is now shown
* [Fix] Copying a teleport keeps what its destination's movement follows
* [Fix] Fix unknown data, and values stored in the level that match the default, being lost from level objects when saved
* [Fix] Use the game's defaults for solid and outline colours: black could not be used as a colour, and colours from the level showed as transparent
* [Fix] Fix early Pego levels (before file version 0x0F) being corrupted when saved
* [Fix] Fix level entries that refer to another object being dropped when saved
* [Fix] Fix text containing non-ASCII characters producing a corrupt level

PeggleEdit 5.1.1 (Eagle) (2023-09-13)

* [Feature] Add import / export of level packs to folder
* [Feature] Basic preview of emitters
* [Feature] Control points for rods, polygons, and teleports can now be moved in the editor
* [Enhancement] Show a warning if a level contains a generator
* [Fix] #12: Duel challenges do not load as duels
* [Fix] #15: Undoing moves teleports to 0,0
* [Fix] Preserve image format when loading and saving pak files

PeggleEdit 5.1.0 (Dunnock) (2023-06-19)

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

PeggleEdit 5.0.0 (Crow) (2023-04-10)

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
