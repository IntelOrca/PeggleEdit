-----------------------------------------------------------------
-- TED'S PEGGLE LEVEL EDITOR (PeggleEdit 0.3 Beta [Albatross]) --
-----------------------------------------------------------------

BETA NOTICE
-----------------
The Peggle Level Editor allows you to create user-designed levels for the game Peggle Nights (available from PopCap Games http://www.popcap.com). Please note that the editor is still in beta stage which means the editor is not fully functional and may still contain bugs. Also take note of the following things:

- There is no function for animated backgrounds, teleporters or polygon solid objects.
- Curved bricks are not drawn curved in the editor but will be in Peggle.
- Currently it only creates level packs that work with Peggle Nights and not single levels for Peggle or Peggle Nights adventure.
- Opening levels created by PopCap may not be read correctly and is not recommended. This is due to the missing features and objects that still need to be implemented.
- The author has no affiliation with PopCap Games, please leave comments or report any bugs to ted@brambles.org.

Other than that you should be able to create some good levels. Save regularly and enjoy!

Creating Levels
-----------------
Level packs are files containing the levels and configuration for them. This includes the details, the background images, thumbnails and challenges. Each level has a filename which must be unique to all other levels and level packs. If not then problems will occur with the level working. To avoid this I recommend prefixing your name or level pack name before each level. (e.g. "ted_fireworks")

The filename of the level can be set in Level->Properties. You can also change the display name of the level, the minimum stage and the ace score. The minimum stage relates to the stages in adventure mode which need to be completed before the level can be played. If this is 0, then the first 5 levels in adventure mode must be complete before the level is unlocked. -1 would allow you to play the level without playing adventure mode.

Backgrounds will be saved as .png files to preserve quality. If your background image is already in this format, you do not need to change it when importing it via Level->Set Background. If your background is a jpeg, then PeggleEdit will resave it with no reduction in quality.

If you want to export a single level you can do that via Level->Export. The reverse can be done using Level->Import. If you choose Level->Import, it will overwrite your current level. The equivalent background image will be saved and loaded if one exists.

You have several tools at your disposal when you create levels. One is to remove offscreen pegs. You should also check for pegs hidden by the interface by turning the interface on and off via View->Show Interface. The remove duplicate pegs tool also removes any pegs that have the same x and y coordinates, as you may find you have unknowingly created pegs on top of one another using paste or the circle tool. This will not, however, remove partically overlapping pegs which may be deliberate. It will also not remove moving pegs that are on top of each other as that is normally deliberate as well.

The circle tool is used for creating either a circle of pegs, or a circle of connected bricks. This will ask for several properties such as those used by Peggle to curve the individual bricks (the editor does not draw curved bricks yet so they will be shown straight). Keep checking the level in Peggle Nights to ensure that the bricks are correctly aligned. The properties are:

Number of pegs: This is only used for peg circles not bricks, and is self-explanatory. The lower the radius, the fewer pegs you will need.

Radius: The radius of the circle.

Alter angle: This is a rotation adjustment angle of the circle. For example, this can be used to adjust the circle to have either a peg or a gap at top dead centre.

Sector angle: How big the angle of each sector in the circle is. The number of bricks in a brick circle will be 360 divided by this angle.

Solid circular objects are objects that will act as an obstacle to the ball. When placing one, you can select an image to be used. The radius of the circle is automatically set to the highest out of the width or height of the image selected. You can modify this in the object properties window. Use Show Collision to see the actual collision radius.

Moving Objects - As far as I'm aware, all objects can have the moving properties. You can apply moving properties by going to Object->Moving->Type and selecting the move cycle you want. You can then change the properties of the movement in the object properties window. The properties are:

Time period: The time it takes for the object to complete one movement cycle in game turns (deciseconds). So if the type was a circle and the time period was 200, it would take 2 seconds for the object to complete one circle.

X Radius and Y Radius: The horizontal and vertical radius of the movement shape. If they are both the same for a circle type, it would be a perfect circle. If the Y Radius is half the X Radius, you would get an elipse.

Phase: The phase is a percentage of how far around the cycle it is. When you have multiple objects in a movement cycle, you want the phase of each one equally spread between 0 and 100%. For example if you wanted 4 pegs equally spaced around a circle. Your phases for each peg would be 0%, 25%, 50% and 75% providing the other movement details and the location is the same. This can be done automatically using the spread and phase button in the object tab.

Angle: Specifies the angle of the whole movement path.

Reverse: As it says, reverses the direction of the movement.

The editor allows you to preview how the pegs will move and how fast they move by pressing preview on the view tab.

Updates
-----------------
0.3 (Albatross)
  - Improved interface
  - Simple movement
  - Movement preview
  - Collision view
  - Solid circular materials

0.2
  - Original features

Hotkeys
-----------------
Ctrl-N			- New level pack
Ctrl-O			- Open level pack
Ctrl-S			- Save level pack

Ctrl-A			- Select all
Ctrl-Z			- Undo
Ctrl-X			- Cut
Ctrl-C			- Copy
Ctrl-V			- Paste
Delete			- Delete selected pegs

Arrows			- Move pegs by 1.0
Ctrl-Arrows		- Move pegs by 5.0

F5				- Goto previous level
F6				- Goto next level
F7				- Shift level up
F8				- Shift level down

S				- Selection Tool
P				- Peg Tool
B				- Brick Tool

F1				- Open readme.txt

Upcoming / Planned features
-----------------------------
 - Custom level thumbnail
 - Challenges
 - Advanced moving properties
 - Polygon solids
 - Teleporters
 - Animations / particle effects
 
 - Editing main adventure of Peggle and Peggle Nights
 
More Information
-----------------
If you have any questions, comments or bug reports please contact me via:

E-mail:  ted@brambles.org
Website: http://tedtycoon.co.uk

Licence information
-----------------
PeggleEdit uses a modified CromControls originally created by Cristinel Mazarine (http://www.osec.ro)
PeggleEdit uses a modified RibbonControl originally created by Jose Manuel Menéndez Poó (http://www.techniks.com.mx)
PeggleEdit uses a number of icons from the Oxygen Icon library. (http://www.oxygen-icons.org/)
PeggleEdit uses the interface image from Peggle Nights. Copyright PopCap Games (http://www.popcap.com) 

PeggleEdit uses OpenJPEG for decompression of JP2 files. The Licence information for this can be found below:
-------------------------------------------------------------------------------------------------------------
Copyright (c) 2002-2007, Communications and Remote Sensing Laboratory, Universite catholique de Louvain (UCL), Belgium
Copyright (c) 2002-2007, Professor Benoit Macq
Copyright (c) 2001-2003, David Janssens
Copyright (c) 2002-2003, Yannick Verschueren
Copyright (c) 2003-2007, Francois-Olivier Devaux and Antonin Descampe
Copyright (c) 2005, Herve Drolon, FreeImage Team
All rights reserved.
 *
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:
1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
 *
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS `AS IS'
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNERS OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.