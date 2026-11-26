# Generators

Generators are PeggleEdit's own objects that place lots of pegs or bricks for you, in a ring or along a path. Change a setting and all the pegs or bricks update at once.

Generators stay generators. You do not need to do anything before playing the level. When PeggleEdit saves, it saves the pegs and bricks each generator makes, which is what the game plays. It also keeps the generators themselves, so they are still there next time you open the level in PeggleEdit.

Turn on **Preview** on the **View** tab to see a level as the game will show it, with each generator drawn as its pegs or bricks.

To turn a generator into ordinary pegs or bricks, for example to change one of them on its own, select it and click **Apply** on the **Tools** tab.

If another program changes a level saved by PeggleEdit, PeggleEdit can no longer tell which pegs and bricks came from a generator. It opens them as ordinary pegs and bricks, and tells you.

## Peg Generator
Places pegs evenly around a circle or oval. Find it under **Generator** on the **Tools** tab.

| Setting | What it does |
|---|---|
| Horizontal Radius, Vertical Radius | The size of the circle. Make them different for an oval. |
| Max Number of Pegs | How many equal places the circle is split into. |
| Number of Pegs | How many of those places get a peg. Use fewer than the maximum for part of a circle. |
| Angular Offset | Where the first peg goes, in degrees. |

## Brick Generator
Places curved bricks around a circle.

| Setting | What it does |
|---|---|
| Inner Radius, Mid Radius, Outer Radius | The size of the ring of bricks. |
| Brick Width | How thick the bricks are. |
| Max Number of Bricks | How many equal places the circle is split into. |
| Number of Bricks | How many of those places get a brick. |
| Angle of Sectors | How much of the circle each brick covers, in degrees. |
| Angular Offset | Where the first brick goes, in degrees. |

## Peg Pen and Brick Pen
Draw a path, and pegs or bricks are placed along it.
1. Pick **Peg Pen** or **Brick Pen** on the **Tools** tab.
2. Click to add points. Click and drag to make a curve. Hold **Ctrl** to keep lines at neat angles.
3. Right-click to finish.

**Interval** sets the gap between each peg or brick. To change the path afterwards, select it with the **Select** tool and drag its points.

## Moving generators
You can give a generator a movement, like any other object. Its pegs or bricks then move together as one shape. For example, a spinning ring of bricks stays a ring as it turns.

Other objects can follow a generator's movement too. See [Advanced Movement](AdvancedMovement.md).
