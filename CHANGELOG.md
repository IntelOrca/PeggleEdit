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
