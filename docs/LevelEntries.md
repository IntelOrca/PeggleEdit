# Level Objects

Everything you place in a level is an object. PeggleEdit uses the same names for them as Peggle does:

* [Pegs](Pegs.md): the round pegs you shoot at.
* [Bricks](Bricks.md): straight or curved pegs.
* [Balls](Balls.md): round objects, often used as bumpers or to show an image.
* [Polys](Polys.md): shapes made of points, often used as walls or to show an image.
* [Lines](Lines.md): thin walls between two points.
* [Holes](Holes.md): send the ball somewhere else in the level, or take it away.
* [Emitters](Emitters.md): particle effects, such as glows, bubbles and snow.
* [Generators](Generators.md): PeggleEdit's own objects that make rings of pegs or bricks for you.

## Settings all objects have
Select an object to see its settings in the **Properties** window. Click a setting to read what it does at the bottom of the window. The most useful ones are:

| Setting | What it does |
|---|---|
| Visible | Whether the object is drawn. |
| Collidable | Whether the ball bounces off it. Turn it off for decoration. |
| Locked | Whether the object is locked, so that it can not be selected or moved in the level. See the [Editor Guide](EditorGuide.md#home-tab). The game does not use it. |
| Image | The image drawn for the object. See below. |
| Background | Draws the object behind everything except the background image. |
| Foreground | Draws the object in front of everything except the game's interface. |
| Draw Shadow | Whether the object has a shadow. |
| Bouncy, Rolly | How bouncy the object is, and how much the ball rolls along it. 0 uses the game's normal value. |
| Solid Color, Outline Color | The colours used when the object has no image. |
| Mover | The object's movement. See [Advanced Movement](AdvancedMovement.md). |
| Flipper Flags | Whether the object is taken away while Claude's flippers are out. |
| Id, Logic | Used by the game's own logic. Most levels do not need them. |

A few settings say "The exact effect is not known". These are kept exactly as they are when you save, so you can leave them alone.

## Using images
Add your image to the pack first (see [Level Packs](LevelPacks.md#images)). Then type its name into the object's **Image** setting, starting with `images\` and without `.png`. For example, an image added as `images\bumper.png` is typed as `images\bumper`.

To use one of the game's own level images, type just its name, such as `flame4`.

**Image DX** and **Image DY** move the image from the object's position, and **Image Rotation** turns it (with that offset) anticlockwise around the position, in degrees. A poly's image turns with the poly too. Bricks only use Image Rotation while they turn.

## Drawing order
Objects are drawn in the order shown in the **Entry List**. Use **Bring to Front**, **Send to Back** and the other Z Order buttons on the **Home** tab to change it.
