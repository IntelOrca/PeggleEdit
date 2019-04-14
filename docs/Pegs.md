In PeggleEdit, pegs can be round or rectangular ([bricks](bricks.md)). They can be defined as normal (blue) or objective (orange). When playing the game Peggle will,  at random, change two blue pegs to green (powerup) and one to purple (bonus).

## Peg Info ##
Pegs are [circles](Circle.md) objects with additional information which defines the circle as a peg. This information is stored in a small set of properties called PegInfo.

You can place pegs by using the Peg tool found on the tools tab of the ribbon at the top of the main window.

There are two properties in the PegInfo.

**Can Be Orange** states whether the peg can be an orange peg. With this property set to false, the peg will never be orange. In PeggleEdit if the property is true, pegs are displayed orange, otherwise blue. Peggle will choose 25 of the **Can Be Orange** pegs (for a default challenge) to be orange when playing the game.

**Quick Disappear** states that the peg will disappear shortly after it is hit by the ball. Normally, if the ball does not hit any pegs for a few seconds, there is a delay before the pegs that have been hit will disappear, for instance if the ball is stuck in an arc of pegs. You can set this property on bricks or pegs (e.g. one at the bottom of an arc) to reduce the time that a ball will remain stuck.