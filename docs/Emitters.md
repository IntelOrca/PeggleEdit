## What are emitters? ##
An emitter is an object that was introduced in Peggle Nights. They cannot be used in the first Peggle game. Emitter's create visual particles which add dynamic effects to the level. They do not change the gameplay in any way and generally just make the level look nicer.

## Examples ##
Some examples of where emitters are used are:
  * Glowing background animations
  * Light twinkles
  * Bubbles / Smoke / Fire
  * Snow

Emitters often have movement info associated with them.

## Seeing emitters in PeggleEdit ##
PeggleEdit runs emitters the way Peggle Nights does, so what you see is what the game shows:

  * **While editing**, each emitter is shown *settled*: as it looks once it has been running for a few seconds. It is
    worked out again whenever you change the emitter.
  * **With Preview on**, emitters run live, 100 times a second, starting from nothing as they do when a level starts.
    Moving emitters leave their particles behind them, unless they are *Attached*.
  * **Particles** (View tab) shows or hides the particles.
  * **3D Mode** (View tab) is on by default. Untick it to see the level as the game shows it without 3D acceleration:
    emitters with a *Software Rate* of 0 (most of PopCap's) are not shown at all.

Particles are drawn where the emitter is in the level's drawing order, as in the game: an emitter early in the level is
drawn under the pegs, and one with *Background* set is drawn under everything (even the peg shadows). Use Bring to
Front / Send to Back to change it. Invisible emitters show no particles.

## The debug box ##
Peggle Nights draws a box around an emitter, with the number of particles next to it, unless its **Outline Color** is
black. The game's default outline colour is white, so an emitter that does not set it shows the box. New emitters made in
PeggleEdit set it to black. If an older level shows boxes in the preview, set the emitters' Outline Color to black.

## Properties ##
There are a number of properties that can be used to change the behaviour of the emitters and their particles. Times are
in seconds unless they say otherwise, and *per tick* means every hundredth of a second.

| Property | What it does |
|---|---|
| Particle Image | The image of each particle, e.g. `images/levels/flame4`, or just its name for an image in `images/levels`. |
| Additive | Adds the particles to what is under them, so they glow and black is transparent. Most of PopCap's emitters are additive. |
| Width, Height | The area particles start in (with *Random Start*); otherwise they all start in the middle. |
| Random Start | Start each particle at a random place in the area. |
| Circular | Use an ellipse instead of a rectangle (and emit a third more). |
| Distrib | How far out over the area particles start, e.g. 0.5 for the middle half. |
| Rate | Particles per tick: 0.01 is one a second. Can be a curve, stepped through every tick (looping with *Rate Is Looping*). |
| Rand Rate | A random amount, from 0 to this, added to the rate every tick. |
| Max | The most particles at once. When there are this many, no more are emitted until some disappear. |
| Duration | How long each particle lasts. |
| Alpha Into | How long each particle takes to fade in. |
| Alpha Fade At | When each particle starts to fade out (it fades until the end of its life). |
| Fast Forward | Ticks to run ahead when the level starts, so that the emitter is already going. |
| Alpha, Color R/G/B, Scale, Rotation | How each particle looks over its life (curves go from its start to its end). A colour over 1 spills into the next colour, as in the game. |
| Scale Vel, Rotation Vel | How much the scale and rotation change every tick. Rotation is in degrees, anticlockwise. |
| Vel X/Y, Acc X, Gravity | Movement in pixels per tick, and its change every tick. The *Rand* values add a random amount from 0 to them. |
| Polar Vel, Vel Rot | Movement at an angle in degrees (0 is right, 90 is down). |
| Spiral Count | How many times particles go round on their way into the middle. |
| Orient To Velocity | Turn each particle to face the way it moves. |
| Attached | Particles move with the emitter instead of being left behind. |
| Starts Emitting | When off, the emitter waits until the ball hits the object whose id is the *Trigger Object*. |

Particles that are rotated (with any rotation, or *Orient To Velocity*) are drawn half their image up and to the left of
where they are, as the game does. PopCap's levels move these emitters to make up for it.
