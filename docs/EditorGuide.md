# Editor Guide

## The main window
* **Orca menu**: the round button at the top left. It has New, Open, Save, Save As, recent packs, **.pak Extract / Pack**, Close, **PeggleEdit Options** and Exit.
* **Quick access buttons**: the small buttons next to it, for New, Open, Save, Save As and Undo.
* **Ribbon**: the tabs along the top (Home, Tools, Object, View and Help).
* **Level**: the level you are working on, in the middle. You can open several levels at once, each in its own tab. A level with unsaved changes has a `*` after its name.
* **Pack Explorer**: the levels, images and challenges in your pack. See [Level Packs](LevelPacks.md).
* **Properties**: the settings of the selected objects. Click a setting to see what it does at the bottom of the window.
* **Entry List**: every object in the level, in the order the game draws them.

If you close a window, open it again from the **View** tab. PeggleEdit remembers where you put each window.

## Home tab
* **Clipboard**: cut, copy, paste and delete objects. You can copy objects from one level to another, or even between two copies of PeggleEdit.
* **Alignment**: line objects up, space them out evenly, flip them and rotate them.
* **Z Order**: change which objects are drawn on top of others.
* **Group**: group objects, and lock them.
  * **Group** puts the selected objects in a group. Clicking or boxing any object in a group selects the whole group, with a yellow box around it; hold **Alt** as you click or box to select just the objects themselves. **Ungroup** takes the selected objects out of their groups.
  * **Lock** locks the selected objects, so that they can not be selected or moved in the level, for example a background of bricks you are building on. You can still select a locked object in the Entry List, which shows it with a red box, and change it in the Properties window (**Locked** turns it off for just that object). **Unlock All** unlocks every object in the level.
  * The Entry List marks objects that are in a group or locked. Groups and locks are kept in PeggleEdit's own data in the level, which the game ignores. Pasted objects are put in new groups of their own and are not locked.
* **Nights**: start Peggle Nights. If it does not start, set where the game is under **PeggleEdit Options > Peggle Nights**.

## Tools tab

| Tool | What it does |
|---|---|
| Select | Select and move objects. Drag a box to select several. Hold **Ctrl** and click to add or remove an object from the selection. Right-click for more options. |
| Eyedropper | Click an object to start placing more objects like it. |
| Peg | Click to place a peg, or drag to paint lots of them. Pegs are not placed on top of other objects unless you hold **Ctrl**. |
| Brick | Click to place a straight brick, or drag to paint several. A brick placed near the end of another joins on to it. Hold **Shift** to stop it joining, and **Ctrl** to place it on top of other objects. |
| Curved Brick | The same as Brick, but for curved bricks. |
| Peg Pen, Brick Pen | Draw a path, and pegs or bricks are placed along it. Click to add points, drag to make a curve, and right-click to finish. Hold **Ctrl** to keep lines at neat angles. See [Generators](Generators.md). |
| Ball, Poly, Line, Hole, Emitter | Click to place one of these objects. Hold **Ctrl** to place more than one. |
| Generator | Place a peg generator or brick generator. See [Generators](Generators.md). |

The **Functions** panel has **Apply**, which turns the selected generators into ordinary pegs and bricks.

The **Remove** button can remove:
* **Duplicate Pegs**: objects that sit exactly on top of each other. Moving objects are left alone. Only the selected objects are checked, or the whole level if nothing is selected.
* **Offscreen Pegs**: every object outside the level area. Take care, as this includes hidden objects that you may have put there on purpose.
* **Unreachable Pegs**: pegs and bricks that no shot from the cannon can reach without first bouncing off something. It takes into account any challenges in your pack that change the ball's speed or gravity. Moving pegs are left alone. A bounced ball, or some of the masters' powers, could still reach the pegs it removes, so check the result.

## Object tab
* **Relocate**: moves all the selected objects to where the first one is.
* **Type**: gives the selected objects a movement, such as `Circle` or `HorizontalCycle`.
* **Phase**: spreads the selected moving objects evenly along their path.
* **Duplicate & Phase**: makes copies of one moving object, spread evenly along its path.
* **Link Sub-movements**: makes objects follow another object's movement. See [Advanced Movement](AdvancedMovement.md).

## View tab
**Show / Hide**

| Option | What it does |
|---|---|
| Background | Shows the level's background image. |
| Interface | Shows the game's interface around the level. |
| Cannon | Shows the game's cannon, pointing at the mouse, so you can check it is not too close to anything. |
| Objects | Shows the level's objects. |
| Collision | Shows what the ball bounces off, including the walls, the bucket and any power-ups. |
| Transparent | Makes objects see-through, so you can see the background under them. |
| Preview | Plays the level as the game would: objects move and emitters run. |
| Fast Forward | Plays the preview at the game's fast forward speed. |
| Particles | Shows the particles of [emitters](Emitters.md). |
| 3D Mode | Shows emitters as the game does with 3D acceleration, which is how nearly everyone plays. |

**Kinds** shows or hides each kind of object: **Pegs** (with the generators and pens that make them), **Bricks** (likewise), **Balls** that are not pegs, **Lines**, **Polys**, **Holes** and **Emitters**. **Invisible** chooses how objects whose **Visible** setting is off are shown while editing: **Shown** like any other object, **Faded** (drawn faint, over the other objects), or **Hidden**. Objects that are hidden can not be selected in the level, so that you can not move or delete them by mistake, but they are still saved and you can still select them in the Entry List. Objects you paste or place are always shown, so their kind is shown again if it was hidden. The game's view and thumbnails always show everything.

**Zoom** shows the level larger or smaller, from 25% to 800%. The large button shows the zoom; click it to pick one. **Zoom In** and **Zoom Out** go up or down a step, and **100%** goes back to the usual size. You can also hold **Ctrl** and turn the mouse wheel to zoom in and out around the mouse, and hold **Shift** and turn it to scroll sideways. Each level window has its own zoom.

**Power-ups** shows the free ball bucket and the things some masters' powers add to the level:
* **Bucket**: the free ball bucket. It swings from side to side in the preview.
* **Pyramid**: Kat Tut's Pyramid Power.
* **Flippers** and **Flippers Up**: Claude's flippers. Objects the game takes away while the flippers are out are marked in red.
* **Lid**: the lid over the bucket, for Renfield's Spooky Ball, Lord Cinderbottom's Fireball, or a challenge that covers the bucket.
* **Lightning Rod**: Marina's Electrobolt.

**Window** opens the Pack Explorer, Properties or Entry List again.

## Help tab
Open the README, the GitHub page, or see which version you have.

## Keyboard shortcuts

| Keys | What it does |
|---|---|
| Ctrl+N, Ctrl+O, Ctrl+S | New, open and save a pack |
| Ctrl+Z | Undo |
| Ctrl+X, Ctrl+C, Ctrl+V | Cut, copy and paste |
| Ctrl+A | Select everything |
| Delete | Delete the selected objects |
| 1 to 7 | Select, Eyedropper, Peg, Brick, Curved Brick, Peg Pen, Brick Pen |
| Arrow keys | Move the selected objects by 1 (Select tool) |
| Ctrl+Arrow keys | Move them by 5 |
| Alt+Arrow keys | Move them by a tiny amount (1/16) |
| Ctrl+G, Ctrl+Shift+G | Group and ungroup the selected objects |
| Ctrl+L, Ctrl+Shift+L | Lock the selected objects, and unlock everything |
| Alt+click | Select an object without the rest of its group |
| Home, End | Bring to front, send to back |
| Page Up, Page Down | Bring forwards, send backwards |
| B, V | Add a brick joined on to the selected brick. See [Bricks](Bricks.md) |
| ' (# on a UK keyboard) | Show or hide the grid |
| Ctrl++, Ctrl+- | Zoom in and out |
| Ctrl+0 | Zoom to 100% |
| Ctrl+mouse wheel | Zoom in and out around the mouse |
| Shift+mouse wheel | Scroll sideways |
| F1 | Open the README |

## Options
Open **PeggleEdit Options** from the orca menu.
* **Designer**: show the grid, snap objects to it, set its size, always show the anchors of moving objects, and draw pegs and bricks with the game's own textures or as plain shapes.
* **Peggle Nights**: where the game is, for the **Nights** button.
* **Application**: open .pak files with PeggleEdit when you double-click them, and choose your update ring.
