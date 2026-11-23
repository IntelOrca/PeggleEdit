Generators are objects which generate multiple objects automatically. Normally used to generate objects where their locations and other properties are calculated.

Generators stay generators: you never have to apply them for a level to be playable. When PeggleEdit saves a level, it saves the pegs and bricks each generator makes, which is what Peggle loads and plays. The generators themselves are saved after the level's objects, where Peggle does not read them, so that PeggleEdit opens the level with its generators as you left them (see [Level Format](LevelFormat.md)).

Turn on **Preview** on the View tab to see the level as Peggle will show it: each generator is drawn as the pegs or bricks it makes, moving as they will in the game.

To turn a generator into ordinary pegs or bricks, for example to change one of them on its own, select it and click **Apply** on the Tools tab.

If another program changes a level that PeggleEdit saved, PeggleEdit can not tell which pegs and bricks came from which generator any more. It then opens the level with the generated pegs and bricks as ordinary objects, and tells you.

## Movement

A generator can be given a movement, like any other object. Its pegs or bricks then move together as one shape:

* **Rotate**: the shape spins around the generator's anchor. Each peg goes round a circle around it, starting at its own place; each brick also turns as it goes, so a ring of bricks stays a ring.
* **Rotate Back And Forth**: the shape swings back and forth around the generator's anchor in the same way. As in the game, the phase says where the swing is centred, and the path rotation turns the whole swing around the anchor.
* **Rotate Around Circle** and **Swing Around Circle**: the shape goes round (or swings around) the circle, turning as it goes, as one object would.
* **Any other movement** (cycles, circles, infinity, arcs, wraps): each peg or brick follows the generator's path, offset by its place in the shape, so the whole shape moves along the path.

The pause, phase, offset and parent movement settings of the generator's movement are passed on as well.

Other objects can follow a generator's movement by linking to it. In the game they get their own copy of the movement, which moves the same way; PeggleEdit keeps the link.

## Generators

**[Peg Generator](PegGenerators.md)** - Generates a circle of pegs by specifying such properties as **x-radius** and **y-radius**.

**[Brick Generator](BrickGenerators.md)** - Generates a circle of bricks by specifying the radius and the number of bricks etc.

**Peg Pen** and **Brick Pen** - Generate pegs or bricks along a path that you draw.
