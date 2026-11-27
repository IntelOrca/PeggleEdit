# Polys

Polys are shapes made from a list of points. They were called Polygons in older versions of PeggleEdit.

Polys are used for:
* **Walls and odd shapes** that the ball bounces off.
* **Images**: set **Image** to show a picture, and turn off **Collidable** if the ball should pass through it. A moving poly makes a moving picture.
* **Moving other objects**, in the same way as [balls](Balls.md).

To draw the shape, click the **...** button on the poly's **Points** setting. This opens the Poly Editor, where you click to place each point, going round anticlockwise. Click **Finish** when you are done.

With the **Select** tool, you can also drag a selected poly's points to move them.

## Poly settings

| Setting | What it does |
|---|---|
| Normal Dir | Which side the ball is kept on: Keep Out, Keep In, or Both. |
| Rotation | Turns a poly that does not move, anticlockwise around its position, with its points. A moving poly turns as its movement turns it instead. |
| Scale | The size the poly was last scaled to in PopCap's level editor, which changed its points. The game does not use it, as the points are already that size. |
| Grow Type | How the poly appears and disappears when the game shows or hides it. |
| Mirror, Image Tile | Mirror the image, or repeat it across the poly. |

## Images in front of particles
[Emitters](Emitters.md) can draw particles over the background. To make them look like they go behind part of the background, place a poly with a cut-out of that part of the background in front of them.
