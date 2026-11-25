// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2011. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Forms;
using IntelOrca.PeggleEdit.Designer.Level_Editor;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    [GuidAttribute("1A01E050-F11A-47C5-B62B-000000000003")]
    class MenuToolPanel : Control, IShortcutContainer
    {
        MainMDIForm mParent;
        Ribbon mRibbon;

        public MenuToolPanel(MainMDIForm parent)
        {
            mParent = parent;

            Text = "Menu";

            mRibbon = new Ribbon();
            mRibbon.Location = new Point(0, 0);
            mRibbon.Width = this.Width;
            mRibbon.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            Controls.Add(mRibbon);

            InitRibbon();

            InitShortcuts();

            UpdateRecentPackFiles();

            Height = mRibbon.Height;
        }

        public int RightHeight
        {
            get
            {
                return mRibbon.Height;
            }
        }

        private LevelEditor LevelEditor
        {
            get
            {
                return mParent.GetFocusedLevelEditor();
            }
        }

        #region Ribbon

        RibbonButton btnSelectTool;
        RibbonButton btnEyedropperTool;
        RibbonButton btnPegTool;
        RibbonButton btnStraightBrickTool;
        RibbonButton btnCurvedBrickTool;
        RibbonButton btnPegPenTool;
        RibbonButton btnBrickPenTool;
        RibbonButton btnCircle;
        RibbonButton btnPolygon;
        RibbonButton btnRod;
        RibbonButton btnTeleport;
        RibbonButton btnEmitter;
        RibbonButton btnGenerator;
        RibbonButton btnPegGenerator;
        RibbonButton btnBrickGenerator;

        RibbonPanel panelClipboard;
        RibbonPanel panelAlignment;
        RibbonPanel panelZOrder;

        RibbonPanel panelInsert;

        RibbonPanel panelRemove;

        RibbonCheckBox chkBackground;
        RibbonCheckBox chkInterface;
        RibbonCheckBox chkCannon;
        RibbonCheckBox chkObjects;
        RibbonCheckBox chkCollision;
        RibbonCheckBox chkTransparent;
        RibbonCheckBox chkPreview;
        RibbonCheckBox chkFastForward;
        RibbonCheckBox chkParticles;
        RibbonCheckBox chk3DAcceleration;

        RibbonCheckBox chkBucket;
        RibbonCheckBox chkPyramid;
        RibbonCheckBox chkFlippers;
        RibbonCheckBox chkFlippersUp;
        RibbonCheckBox chkCover;
        RibbonCheckBox chkLightningRod;

        private void InitOrb()
        {
            mRibbon.OrbImage = Resources.orca_orb;

            RibbonOrbMenuItem orbNew = new RibbonOrbMenuItem("New");
            orbNew.Image = Resources.new_32;
            orbNew.Click += new EventHandler(newRibbonButton_Click);

            RibbonOrbMenuItem orbOpen = new RibbonOrbMenuItem("Open");
            orbOpen.Image = Resources.open_32;
            orbOpen.Click += new EventHandler(openRibbonButton_Click);

            RibbonOrbMenuItem orbOpenGameLevels = new RibbonOrbMenuItem("Open Game Levels");
            orbOpenGameLevels.Image = Resources.open_32;
            orbOpenGameLevels.Click += new EventHandler(openGameLevelsRibbonButton_Click);

            RibbonOrbMenuItem orbSave = new RibbonOrbMenuItem("Save");
            orbSave.Image = Resources.save_32;
            orbSave.Click += new EventHandler(saveRibbonButton_Click);

            RibbonOrbMenuItem orbSaveAs = new RibbonOrbMenuItem("Save As");
            orbSaveAs.Image = Resources.saveas_32;
            orbSaveAs.Click += new EventHandler(saveAsRibbonButton_Click);

            RibbonOrbMenuItem orbPakTool = new RibbonOrbMenuItem(".pak Extract / Pack");
            orbPakTool.Click += new EventHandler(pakToolRibbonButton_Click);

            RibbonOrbMenuItem orbClose = new RibbonOrbMenuItem("Close");

            RibbonOrbMenuItem orbExit = new RibbonOrbMenuItem("Exit");
            orbExit.SmallImage = Resources.exit_16;
            orbExit.Click += new EventHandler(exitRibbonButton_Click);

            RibbonOrbMenuItem orbOptions = new RibbonOrbMenuItem("PeggleEdit Options");
            orbOptions.SmallImage = Resources.settings_16;
            orbOptions.Click += new EventHandler(optionsRibbonButton_Click);

            mRibbon.OrbDropDown.MenuItems.Add(orbNew);
            mRibbon.OrbDropDown.MenuItems.Add(orbOpen);
            mRibbon.OrbDropDown.MenuItems.Add(orbOpenGameLevels);
            mRibbon.OrbDropDown.MenuItems.Add(orbSave);
            mRibbon.OrbDropDown.MenuItems.Add(orbSaveAs);
            mRibbon.OrbDropDown.MenuItems.Add(new RibbonSeparator());
            mRibbon.OrbDropDown.MenuItems.Add(orbPakTool);
            mRibbon.OrbDropDown.MenuItems.Add(new RibbonSeparator());
            mRibbon.OrbDropDown.MenuItems.Add(orbClose);
            mRibbon.OrbDropDown.OptionItems.Add(orbExit);
            mRibbon.OrbDropDown.OptionItems.Add(orbOptions);
        }

        private void InitQuickAccess()
        {
            RibbonButton btnNew = new RibbonButton();
            btnNew.SmallImage = Resources.new_16;
            btnNew.Click += new EventHandler(newRibbonButton_Click);

            RibbonButton btnOpen = new RibbonButton();
            btnOpen.SmallImage = Resources.open_16;
            btnOpen.Click += new EventHandler(openRibbonButton_Click);

            RibbonButton btnSave = new RibbonButton();
            btnSave.SmallImage = Resources.save_16;
            btnSave.Click += new EventHandler(saveRibbonButton_Click);

            RibbonButton btnSaveAs = new RibbonButton();
            btnSaveAs.SmallImage = Resources.saveas_16;
            btnSaveAs.Click += new EventHandler(saveAsRibbonButton_Click);

            RibbonButton btnUndo = new RibbonButton();
            btnUndo.SmallImage = Resources.undo_16;
            btnUndo.Click += new EventHandler(undoRibbonButton_Click);

            mRibbon.QuickAcessToolbar.Items.Add(btnNew);
            mRibbon.QuickAcessToolbar.Items.Add(btnOpen);
            mRibbon.QuickAcessToolbar.Items.Add(btnSave);
            mRibbon.QuickAcessToolbar.Items.Add(btnSaveAs);
            mRibbon.QuickAcessToolbar.Items.Add(btnUndo);
        }

        private void InitHomeTab()
        {
            //Clipboard panel
            RibbonButton btnPaste = new RibbonButton("Paste");
            btnPaste.Image = Resources.paste_32;
            btnPaste.Click += new EventHandler(pasteRibbonButton_Click);

            RibbonButton btnCut = new RibbonButton("Cut");
            btnCut.MaxSizeMode = RibbonElementSizeMode.Medium;
            btnCut.SmallImage = Resources.cut_16;
            btnCut.Click += new EventHandler(cutRibbonButton_Click);

            RibbonButton btnCopy = new RibbonButton("Copy");
            btnCopy.MaxSizeMode = RibbonElementSizeMode.Medium;
            btnCopy.SmallImage = Resources.copy_16;
            btnCopy.Click += new EventHandler(copyRibbonButton_Click);

            RibbonButton btnDelete = new RibbonButton("Delete");
            btnDelete.MaxSizeMode = RibbonElementSizeMode.Medium;
            btnDelete.SmallImage = Resources.delete_16;
            btnDelete.Click += new EventHandler(deleteRibbonButton_Click);

            panelClipboard = new RibbonPanel("Clipboard");
            panelClipboard.Items.Add(btnPaste);
            panelClipboard.Items.Add(btnCut);
            panelClipboard.Items.Add(btnCopy);
            panelClipboard.Items.Add(btnDelete);

            //Arrange
            RibbonButton btnAlignHorizontally = new RibbonButton();
            btnAlignHorizontally.SmallImage = Resources.align_horizontally_16;
            btnAlignHorizontally.MaxSizeMode = RibbonElementSizeMode.Compact;
            btnAlignHorizontally.Click += new EventHandler(alignHorizontallyRibbonButton_Click);

            RibbonButton btnAlignVertically = new RibbonButton();
            btnAlignVertically.SmallImage = Resources.align_vertically_16;
            btnAlignVertically.MaxSizeMode = RibbonElementSizeMode.Compact;
            btnAlignVertically.Click += new EventHandler(alignVerticallyRibbonButton_Click);

            RibbonButton btnSpaceHorizontally = new RibbonButton();
            btnSpaceHorizontally.SmallImage = Resources.space_horizontally_16;
            btnSpaceHorizontally.MaxSizeMode = RibbonElementSizeMode.Compact;
            btnSpaceHorizontally.Click += new EventHandler(spaceHorizontallyRibbonButton_Click);

            RibbonButton btnSpaceVertically = new RibbonButton();
            btnSpaceVertically.SmallImage = Resources.space_vertically_16;
            btnSpaceVertically.MaxSizeMode = RibbonElementSizeMode.Compact;
            btnSpaceVertically.Click += new EventHandler(spaceVerticallyRibbonButton_Click);

            RibbonItemGroup itemgroupAlignment = new RibbonItemGroup();
            itemgroupAlignment.Items.Add(btnAlignHorizontally);
            itemgroupAlignment.Items.Add(btnAlignVertically);
            itemgroupAlignment.Items.Add(btnSpaceHorizontally);
            itemgroupAlignment.Items.Add(btnSpaceVertically);

            RibbonButton btnFlipHorizontally = new RibbonButton();
            btnFlipHorizontally.SmallImage = Resources.flip_horizontally_16;
            btnFlipHorizontally.MaxSizeMode = RibbonElementSizeMode.Compact;
            btnFlipHorizontally.Click += new EventHandler(flipHorizontallyRibbonButton_Click);

            RibbonButton btnFlipVertically = new RibbonButton();
            btnFlipVertically.SmallImage = Resources.flip_vertically_16;
            btnFlipVertically.MaxSizeMode = RibbonElementSizeMode.Compact;
            btnFlipVertically.Click += new EventHandler(flipVerticallyRibbonButton_Click);

            RibbonItemGroup itemgroupTransform = new RibbonItemGroup();
            itemgroupTransform.Items.Add(btnFlipHorizontally);
            itemgroupTransform.Items.Add(btnFlipVertically);

            RibbonButton btnRotateAntiClockwise = new RibbonButton();
            btnRotateAntiClockwise.SmallImage = Resources.rotate_acw_16;
            btnRotateAntiClockwise.MaxSizeMode = RibbonElementSizeMode.Compact;
            btnRotateAntiClockwise.Click += new EventHandler(rotateACwRibbonButton_Click);

            RibbonButton btnRotateClockwise = new RibbonButton();
            btnRotateClockwise.SmallImage = Resources.rotate_cw_16;
            btnRotateClockwise.MaxSizeMode = RibbonElementSizeMode.Compact;
            btnRotateClockwise.Click += new EventHandler(rotateCwRibbonButton_Click);

            RibbonItemGroup itemgroupRotatation = new RibbonItemGroup();
            itemgroupRotatation.Items.Add(btnRotateAntiClockwise);
            itemgroupRotatation.Items.Add(btnRotateClockwise);

            panelAlignment = new RibbonPanel("Alignment");
            panelAlignment.FlowsTo = RibbonPanelFlowDirection.Right;
            panelAlignment.Items.Add(itemgroupAlignment);
            panelAlignment.Items.Add(itemgroupTransform);
            panelAlignment.Items.Add(itemgroupRotatation);

            RibbonButton btnBringForward = new RibbonButton("Bring Forwards");
            btnBringForward.Image = Resources.bring_forward_32;
            btnBringForward.Click += new EventHandler(bringForwardRibbonButton_Click);

            RibbonButton btnSendBackward = new RibbonButton("Send Backwards");
            btnSendBackward.Image = Resources.send_backward_32;
            btnSendBackward.Click += new EventHandler(sendBackwardRibbonButton_Click);

            RibbonButton btnBringToFront = new RibbonButton("Bring to Front");
            btnBringToFront.Image = Resources.bring_to_front_32;
            btnBringToFront.Click += new EventHandler(bringToFrontRibbonButton_Click);

            RibbonButton btnSendToBack = new RibbonButton("Send to Back");
            btnSendToBack.Image = Resources.send_to_back_32;
            btnSendToBack.Click += new EventHandler(sendToBackRibbonButton_Click);

            panelZOrder = new RibbonPanel("Z Order");
            panelZOrder.Items.Add(btnBringForward);
            panelZOrder.Items.Add(btnSendBackward);
            panelZOrder.Items.Add(btnBringToFront);
            panelZOrder.Items.Add(btnSendToBack);

            RibbonButton btnLaunchPeggleNights = new RibbonButton("Nights");
            btnLaunchPeggleNights.Image = Resources.PeggleNights_32;
            btnLaunchPeggleNights.Click += new EventHandler(peggleNightsRibbonButton_Click);

            RibbonPanel panelPeggle = new RibbonPanel("Launch Peggle");
            panelPeggle.Items.Add(btnLaunchPeggleNights);

            RibbonTab tabHome = new RibbonTab(mRibbon, "Home");
            tabHome.Panels.Add(panelClipboard);
            tabHome.Panels.Add(panelAlignment);
            tabHome.Panels.Add(panelZOrder);
            tabHome.Panels.Add(panelPeggle);

            mRibbon.Tabs.Add(tabHome);
        }

        private void InitToolTab()
        {
            //Tool panel
            btnSelectTool = new RibbonButton("Select");
            btnSelectTool.Image = Resources.pointer_32;
            btnSelectTool.Click += new EventHandler(selectRibbonButton_Click);

            btnEyedropperTool = new RibbonButton("Eyedropper");
            btnEyedropperTool.Image = Resources.eyedropper_32;
            btnEyedropperTool.Click += new EventHandler(eyedropperRibbonButton_Click);

            btnPegTool = new RibbonButton("Peg");
            btnPegTool.Image = Resources.peg_32;
            btnPegTool.Click += new EventHandler(pegRibbonButton_Click);

            btnStraightBrickTool = new RibbonButton("Brick");
            btnStraightBrickTool.Image = Resources.brick_32;
            btnStraightBrickTool.Click += new EventHandler(straightBrickRibbonButton_Click);

            btnCurvedBrickTool = new RibbonButton("Curved Brick");
            btnCurvedBrickTool.Image = Resources.brick_curved_32;
            btnCurvedBrickTool.Click += new EventHandler(curvedBrickRibbonButton_Click);

            btnPegPenTool = new RibbonButton("Peg Pen");
            btnPegPenTool.Image = Resources.peg_pen_32;
            btnPegPenTool.Click += new EventHandler(pegPenRibbonButton_Click);

            btnBrickPenTool = new RibbonButton("Brick Pen");
            btnBrickPenTool.Image = Resources.brick_pen_32;
            btnBrickPenTool.Click += new EventHandler(brickPenRibbonButton_Click);

            btnCircle = new RibbonButton("Ball");
            btnCircle.Image = Resources.material_32;
            btnCircle.Click += new EventHandler(circleRibbonButton_Click);

            btnPolygon = new RibbonButton("Poly");
            btnPolygon.Image = Resources.polygon_32;
            btnPolygon.Click += new EventHandler(polygonRibbonButton_Click);

            btnRod = new RibbonButton("Line");
            btnRod.Image = Resources.rod_32;
            btnRod.Click += new EventHandler(rodRibbonButton_Click);

            btnTeleport = new RibbonButton("Hole");
            btnTeleport.Image = Resources.teleport_32;
            btnTeleport.Click += new EventHandler(teleportRibbonButton_Click);

            btnEmitter = new RibbonButton("Emitter");
            btnEmitter.Image = Resources.emitter_32;
            btnEmitter.Click += new EventHandler(emitterRibbonButton_Click);

            btnGenerator = new RibbonButton("Generator");
            btnGenerator.Style = RibbonButtonStyle.DropDown;
            btnGenerator.Image = Resources.peg_circle_32;

            btnPegGenerator = new RibbonButton("Peg Generator");
            btnPegGenerator.SmallImage = Resources.peg_16;
            btnPegGenerator.Click += new EventHandler(pegGeneratorRibbonButton_Click);

            btnBrickGenerator = new RibbonButton("Brick Generator");
            btnBrickGenerator.SmallImage = Resources.brick_16;
            btnBrickGenerator.Click += new EventHandler(brickGeneratorRibbonButton_Click);

            btnGenerator.DropDownItems.Add(btnPegGenerator);
            btnGenerator.DropDownItems.Add(btnBrickGenerator);

            panelInsert = new RibbonPanel("Tools");
            panelInsert.Items.Add(btnSelectTool);
            panelInsert.Items.Add(btnEyedropperTool);
            panelInsert.Items.Add(btnPegTool);
            panelInsert.Items.Add(btnStraightBrickTool);
            panelInsert.Items.Add(btnCurvedBrickTool);
            panelInsert.Items.Add(btnPegPenTool);
            panelInsert.Items.Add(btnBrickPenTool);
            panelInsert.Items.Add(btnCircle);
            panelInsert.Items.Add(btnPolygon);
            panelInsert.Items.Add(btnRod);
            panelInsert.Items.Add(btnTeleport);
            panelInsert.Items.Add(btnEmitter);
            panelInsert.Items.Add(btnGenerator);

            RibbonPanel panelFunctions = new RibbonPanel("Functions");

            RibbonButton btnApplyFunction = new RibbonButton("Apply");
            btnApplyFunction.Image = Resources.execute_32;
            btnApplyFunction.Click += new EventHandler(applyFunctionRibbonButton_Click);

            panelFunctions.Items.Add(btnApplyFunction);

            RibbonButton btnRemove = new RibbonButton("Remove");
            btnRemove.Image = Resources.remove_peg_32;
            btnRemove.Style = RibbonButtonStyle.DropDown;

            RibbonButton btnDuplicatePegs = new RibbonButton("Duplicate Pegs");
            btnDuplicatePegs.Click += new EventHandler(removeDuplicatePegsRibbonButton_Click);

            RibbonButton btnOffscreenPegs = new RibbonButton("Offscreen Pegs");
            btnOffscreenPegs.Click += new EventHandler(removeOffscreenPegsRibbonButton_Click);

            btnRemove.DropDownItems.Add(btnDuplicatePegs);
            btnRemove.DropDownItems.Add(btnOffscreenPegs);

            panelRemove = new RibbonPanel("Remove");
            panelRemove.Items.Add(btnRemove);

            RibbonTab tabTools = new RibbonTab(mRibbon, "Tools");
            tabTools.Panels.Add(panelInsert);
            tabTools.Panels.Add(panelFunctions);
            tabTools.Panels.Add(panelRemove);

            mRibbon.Tabs.Add(tabTools);
        }

        private void InitObjectTab()
        {
            RibbonButton btnSameLocation = new RibbonButton("Relocate");
            btnSameLocation.Image = Resources.properties_32;
            btnSameLocation.Click += new EventHandler(sameLocationRibbonButton_Click);

            RibbonPanel panelProperties = new RibbonPanel("Properties");
            panelProperties.Items.Add(btnSameLocation);

            RibbonButton btnMovementType = new RibbonButton("Type");
            btnMovementType.Image = Resources.peg_move_type_32;
            btnMovementType.Style = RibbonButtonStyle.DropDown;

            for (int i = 0; i <= (int)MoverType.FlowerLoop; i++)
            {
                RibbonButton ritem = new RibbonButton();
                ritem.Text = ((MoverType)i).ToString();
                ritem.Tag = i;
                ritem.Click += new EventHandler(movementTypeRibbonButton_Click);

                btnMovementType.DropDownItems.Add(ritem);
            }

            RibbonButton btnSpreadPhase = new RibbonButton("Phase");
            btnSpreadPhase.Image = Resources.spread_phase_32;
            btnSpreadPhase.Click += new EventHandler(spreadPhaseRibbonButton_Click);

            RibbonButton btnDuplicateAndPhase = new RibbonButton("Duplicate & Phase");
            btnDuplicateAndPhase.Image = Resources.duplicate_and_phase_32;
            btnDuplicateAndPhase.Click += new EventHandler(duplicateAndPhaseRibbonButton_Click);

            RibbonButton btnLinkSubMovements = new RibbonButton("Link Sub-movements");
            btnLinkSubMovements.Image = Resources.link_sub_movements_32;
            btnLinkSubMovements.Click += new EventHandler(linkSubMovementsRibbonButton_Click);

            RibbonPanel panelMovement = new RibbonPanel("Movement");
            panelMovement.Items.Add(btnMovementType);
            panelMovement.Items.Add(btnSpreadPhase);
            panelMovement.Items.Add(btnDuplicateAndPhase);
            panelMovement.Items.Add(btnLinkSubMovements);

            RibbonTab tabObject = new RibbonTab(mRibbon, "Object");
            tabObject.Panels.Add(panelProperties);
            tabObject.Panels.Add(panelMovement);
            mRibbon.Tabs.Add(tabObject);
        }

        private void InitViewTab()
        {
            chkBackground = new RibbonCheckBox("Background");
            chkBackground.Checked = true;
            chkBackground.CheckedChanged += new EventHandler(showBackgroundRibbonButton_Click);

            chkInterface = new RibbonCheckBox("Interface");
            chkInterface.Checked = true;
            chkInterface.CheckedChanged += new EventHandler(showInterfaceRibbonButton_Click);

            chkCannon = new RibbonCheckBox("Cannon");
            chkCannon.Checked = true;
            chkCannon.ToolTipTitle = "Cannon";
            chkCannon.ToolTip = "Show the game's cannon, pointing at the mouse, to see whether it is too close to the level's objects.";
            chkCannon.CheckedChanged += new EventHandler(showCannonRibbonCheckBox_CheckedChanged);

            chkObjects = new RibbonCheckBox("Objects");
            chkObjects.Checked = true;
            chkObjects.CheckedChanged += new EventHandler(showPegsRibbonButton_Click);

            chkCollision = new RibbonCheckBox("Collision");
            chkCollision.CheckedChanged += new EventHandler(showCollisionRibbonButton_Click);

            chkTransparent = new RibbonCheckBox("Transparent");
            chkTransparent.ToolTipTitle = "Transparent";
            chkTransparent.ToolTip = "Draw the level's objects half transparent, so that the background under them can be seen.";
            chkTransparent.CheckedChanged += new EventHandler(transparentRibbonCheckBox_CheckedChanged);

            chkPreview = new RibbonCheckBox("Preview");
            chkPreview.CheckedChanged += new EventHandler(showPreviewRibbonButton_Click);

            chkFastForward = new RibbonCheckBox("Fast Forward");
            chkFastForward.ToolTipTitle = "Fast Forward";
            chkFastForward.ToolTip = "Play the preview as fast as the game's fast forward.";
            chkFastForward.CheckedChanged += new EventHandler(fastForwardRibbonCheckBox_CheckedChanged);

            chkParticles = new RibbonCheckBox("Particles");
            chkParticles.Checked = true;
            chkParticles.CheckedChanged += new EventHandler(showParticlesRibbonButton_Click);

            chk3DAcceleration = new RibbonCheckBox("3D Mode");
            chk3DAcceleration.Checked = Settings.Default.Acceleration3D;
            chk3DAcceleration.ToolTipTitle = "3D Mode";
            chk3DAcceleration.ToolTip = "Show emitters as the game does with 3D acceleration, as nearly everyone plays. Without it, the game leaves out emitters with no Software Rate, which is most of them, and emits less from the rest.";
            chk3DAcceleration.CheckedChanged += new EventHandler(acceleration3DRibbonCheckBox_CheckedChanged);

            RibbonPanel panelShowHide = new RibbonPanel("Show / Hide");
            panelShowHide.Items.Add(chkBackground);
            panelShowHide.Items.Add(chkInterface);
            panelShowHide.Items.Add(chkCannon);
            panelShowHide.Items.Add(chkObjects);
            panelShowHide.Items.Add(chkCollision);
            panelShowHide.Items.Add(chkTransparent);
            panelShowHide.Items.Add(chkPreview);
            panelShowHide.Items.Add(chkFastForward);
            panelShowHide.Items.Add(chkParticles);
            panelShowHide.Items.Add(chk3DAcceleration);

            RibbonPanel panelPowerUps = InitPowerUpsPanel();

            RibbonButton btnPackExplorer = new RibbonButton("Pack Explorer");
            btnPackExplorer.Image = Resources.pack_explorer_32;
            btnPackExplorer.Click += new EventHandler(packExplorerRibbonButton_Click);

            RibbonButton btnProperties = new RibbonButton("Properties");
            btnProperties.Image = Resources.properties_32;
            btnProperties.Click += new EventHandler(propertiesRibbonButton_Click);

            RibbonButton btnEntryList = new RibbonButton("Entry List");
            btnEntryList.Image = Resources.properties_32;
            btnEntryList.Click += new EventHandler(entryListRibbonButton_Click);

            RibbonPanel panelWindow = new RibbonPanel("Window");
            panelWindow.Items.Add(btnPackExplorer);
            panelWindow.Items.Add(btnProperties);
            panelWindow.Items.Add(btnEntryList);

            RibbonTab tabView = new RibbonTab(mRibbon, "View");
            tabView.Panels.Add(panelShowHide);
            tabView.Panels.Add(panelPowerUps);
            tabView.Panels.Add(panelWindow);
            mRibbon.Tabs.Add(tabView);
        }

        /// <summary>
        /// The panel that shows what the game adds to every level: the free ball bucket, and the devices of the masters'
        /// powers. More than one power can be on at once, so each device is shown or hidden on its own.
        /// </summary>
        private RibbonPanel InitPowerUpsPanel()
        {
            chkBucket = CreateDeviceCheckBox("Bucket", GameDevices.Bucket, "The free ball bucket. It swings from side to side while the preview plays.");
            chkPyramid = CreateDeviceCheckBox("Pyramid", GameDevices.Pyramid, "Kat Tut's Pyramid Power. The bucket swings less far.");
            chkFlippers = CreateDeviceCheckBox("Flippers", GameDevices.Flippers, "Claude's Flippers. The objects the game takes away while they are out are marked in red, and left out of the preview. The bucket swings much less far.");
            chkFlippersUp = CreateDeviceCheckBox("Flippers Up", GameDevices.FlippersRaised, "Show the flippers flipped up.");
            chkCover = CreateDeviceCheckBox("Lid", GameDevices.Cover, "The lid over the bucket: Renfield's Spooky Ball, Lord Cinderbottom's Fireball, or a challenge with the free ball bucket covered.");
            chkLightningRod = CreateDeviceCheckBox("Lightning Rod", GameDevices.LightningRod, "Marina's Electrobolt: the lightning rod raised over the bucket.");

            var panel = new RibbonPanel("Power-ups");
            panel.Items.Add(chkBucket);
            panel.Items.Add(chkPyramid);
            panel.Items.Add(chkFlippers);
            panel.Items.Add(chkFlippersUp);
            panel.Items.Add(chkCover);
            panel.Items.Add(chkLightningRod);

            UpdatePowerUpsPanel();
            return panel;
        }

        private RibbonCheckBox CreateDeviceCheckBox(string text, GameDevices device, string toolTip)
        {
            var checkBox = new RibbonCheckBox(text);
            checkBox.Tag = device;
            checkBox.ToolTipTitle = text;
            checkBox.ToolTip = toolTip;
            checkBox.CheckedChanged += deviceRibbonCheckBox_CheckedChanged;
            return checkBox;
        }

        /// <summary>
        /// Shows the devices in the settings on the power-ups panel.
        /// </summary>
        private void UpdatePowerUpsPanel()
        {
            var devices = Settings.Default.Devices;
            foreach (var checkBox in new[] { chkBucket, chkPyramid, chkFlippers, chkFlippersUp, chkCover, chkLightningRod })
                checkBox.Checked = (devices & (GameDevices)checkBox.Tag) != 0;
            mRibbon.Refresh();
        }

        private void deviceRibbonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is RibbonCheckBox checkBox) || !(checkBox.Tag is GameDevices device))
                return;

            var devices = Settings.Default.Devices;
            devices = checkBox.Checked ? devices | device : devices & ~device;
            // Flipping up shows the flippers; taking them away puts them down
            if (device == GameDevices.FlippersRaised && checkBox.Checked)
                devices |= GameDevices.Flippers;
            if ((devices & GameDevices.Flippers) == 0)
                devices &= ~GameDevices.FlippersRaised;
            Settings.Default.Devices = devices;
            UpdatePowerUpsPanel();
            DoSettingUpdate();
        }

        private void InitHelpTab()
        {
            RibbonButton btnReadme = new RibbonButton("README.md");
            btnReadme.Image = Resources.readme_32;
            btnReadme.Click += new EventHandler(readmeRibbonButton_Click);

            RibbonButton btnGitHub = new RibbonButton("GitHub");
            btnGitHub.Image = Resources.github_32;
            btnGitHub.Click += new EventHandler(githubRibbonButton_Click);

            RibbonButton btnAbout = new RibbonButton("About");
            btnAbout.Image = Resources.orca_32;
            btnAbout.Click += new EventHandler(aboutRibbonButton_Click);

            RibbonPanel panelHelp = new RibbonPanel();
            panelHelp.Items.Add(btnReadme);
            panelHelp.Items.Add(btnGitHub);
            panelHelp.Items.Add(btnAbout);

            RibbonTab tabHelp = new RibbonTab(mRibbon, "Help");
            tabHelp.Panels.Add(panelHelp);
            mRibbon.Tabs.Add(tabHelp);
        }

        private void InitRibbon()
        {
            InitOrb();
            InitQuickAccess();

            InitHomeTab();
            InitToolTab();
            InitObjectTab();
            InitViewTab();
            InitHelpTab();
        }

        #endregion

        #region Ribbon Button Events

        #region File

        private void newRibbonButton_Click(object sender, EventArgs e)
        {
            if (!mParent.ShowClosePackWarning())
                return;

            mParent.NewPack();
        }

        private void openRibbonButton_Click(object sender, EventArgs e)
        {
            if (!mParent.ShowClosePackWarning())
                return;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Peggle Level Packs (*.pak)|*.pak";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                mParent.OpenPack(dialog.FileName);
            }
        }

        private void openGameLevelsRibbonButton_Click(object sender, EventArgs e)
        {
            if (!mParent.ShowClosePackWarning())
                return;

            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Open Game Levels (choose the game's main.pak, or levels\\stages.cfg if it has been extracted)";
                dialog.Filter = "Game levels (*.pak, stages.cfg)|*.pak;stages.cfg|All files (*.*)|*.*";
                var gameDirectory = Settings.Default.PeggleNightsPath;
                if (!String.IsNullOrEmpty(gameDirectory) && Directory.Exists(gameDirectory))
                    dialog.InitialDirectory = gameDirectory;
                if (dialog.ShowDialog() == DialogResult.OK)
                    mParent.OpenGameLevels(dialog.FileName);
            }
        }

        private void saveRibbonButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(mParent.PackFilename))
                saveAsRibbonButton_Click(sender, e);
            else
                mParent.SavePack(mParent.PackFilename);
        }

        private void saveAsRibbonButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Peggle Level Packs (*.pak)|*.pak";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (mParent.SavePack(dialog.FileName))
                    mParent.PackFilename = dialog.FileName;
            }
        }

        private void pakToolRibbonButton_Click(object sender, EventArgs e)
        {
            using (var pakToolForm = new PakToolForm())
            {
                pakToolForm.ShowDialog();
            }
        }

        private void packDetailsRibbonButton_Click(object sender, EventArgs e)
        {
            PackDetailsForm pack = new PackDetailsForm(mParent.LevelPack);
            pack.ShowDialog();
        }

        private void challengesRibbonButton_Click(object sender, EventArgs e)
        {
            //ChallengePageForm challengeForm = new ChallengePageForm(mParent.LevelPack);
            //challengeForm.ShowDialog();

            MessageBox.Show("Not implemented yet!");
        }

        private void exitRibbonButton_Click(object sender, EventArgs e)
        {
            // Close the main window rather than calling Application.Exit, so that the layout is saved
            // before any of the docked windows are closed.
            mParent.Close();
        }

        private void optionsRibbonButton_Click(object sender, EventArgs e)
        {
            OptionsForm form = new OptionsForm();
            form.ShowDialog();

            DoSettingUpdate();
        }

        #endregion

        #region Home

        private void undoRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.Undo();
        }

        private void cutRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.CutObjects();
        }

        private void copyRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.CopyObjects();
        }

        private void pasteRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.PasteObjects();
        }

        private void deleteRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.DeleteObjects();
        }

        private void selectAllRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.SelectAllObjects();
        }

        private void alignHorizontallyRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.AlignObjectXs();
            UpdatePropertyGrid();
        }

        private void alignVerticallyRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.AlignObjectYs();
            UpdatePropertyGrid();
        }

        private void spaceHorizontallyRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.SpaceObjectXsEqually();
            UpdatePropertyGrid();
        }

        private void spaceVerticallyRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.SpaceObjectYsEqually();
            UpdatePropertyGrid();
        }

        private void flipHorizontallyRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.FlipObjectsHorizontally();
            UpdatePropertyGrid();
        }

        private void flipVerticallyRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.FlipPegsVertically();
            UpdatePropertyGrid();
        }

        private void rotateCwRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.RotateObjects(45.0f);
            UpdatePropertyGrid();
        }

        private void rotateACwRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.RotateObjects(-45.0f);
            UpdatePropertyGrid();
        }

        private void bringForwardRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.BringObjectsForward();
        }

        private void sendBackwardRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.SendObjectsBackward();
        }

        private void bringToFrontRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.BringObjectsToFront();
        }

        private void sendToBackRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.SendObjectsToBack();
        }

        private void peggleNightsRibbonButton_Click(object sender, EventArgs e)
        {
            var errorMessage = null as string;
            var path = Settings.Default.PeggleNightsExePath;
            var psi = new ProcessStartInfo(Settings.Default.PeggleNightsExePath)
            {
                WorkingDirectory = Path.GetDirectoryName(Settings.Default.PeggleNightsExePath)
            };
            if (File.Exists(path))
            {
                try
                {
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }
            else
            {
                errorMessage = $"Unable to find '{path}'.\nIf this path is wrong, you can change it in settings.";
            }
            if (errorMessage != null)
            {
                MessageBox.Show(errorMessage, "Unable To Launch Peggle Nights", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Tools

        private void selectRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.SetEditorTool(new SelectEditorTool());
        }

        private void eyedropperRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.SetEditorTool(new EyedropperEditorTool());
        }

        private void pegRibbonButton_Click(object sender, EventArgs e)
        {
            var circle = new Ball(null);
            circle.PegInfo = new PegInfo(circle, true, false);
            mParent.SetEditorTool(new DrawEditorTool(circle, true, 30, 30));
        }

        private void straightBrickRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.SetEditorTool(new BrickEditorTool(false));
        }

        private void curvedBrickRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.SetEditorTool(new BrickEditorTool(true));
        }

        private void pegPenRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.SetEditorTool(new PenEditorTool(PegKind.Ball));
        }

        private void brickPenRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.SetEditorTool(new PenEditorTool(PegKind.Brick));
        }

        private void circleRibbonButton_Click(object sender, EventArgs e)
        {
            var circle = new Ball(null);
            circle.Radius = 20.0f;
            mParent.SetEditorTool(new DrawEditorTool(circle, false));
        }

        private void polygonRibbonButton_Click(object sender, EventArgs e)
        {
            var p = new Poly(null);
            p.SetPoints(new PointF[] { new PointF(0, 0), new PointF(0, 40), new PointF(40, 40), new PointF(40, 0), new PointF(0, 0) });
            mParent.SetEditorTool(new DrawEditorTool(p, false));
        }

        private void rodRibbonButton_Click(object sender, EventArgs e)
        {
            var rod = new Line(null);
            rod.PointA = new PointF(300, 200);
            rod.PointB = new PointF(400, 250);
            mParent.SetEditorTool(new DrawEditorTool(rod, false));
        }

        private void teleportRibbonButton_Click(object sender, EventArgs e)
        {
            var teleport = new Hole(null);
            teleport.DestinationX = 50.0f;
            teleport.DestinationY = 50.0f;
            mParent.SetEditorTool(new DrawEditorTool(teleport, false));
        }

        private void emitterRibbonButton_Click(object sender, EventArgs e)
        {
            var emitter = new Emitter(null);
            emitter.DefaultValues();
            emitter.Width = 100;
            emitter.Height = 100;
            // Without an outline colour the game draws a debug box around the emitter (its default is white)
            emitter.OutlineColor = Color.Black;
            mParent.SetEditorTool(new DrawEditorTool(emitter, false));
        }

        private void pegGeneratorRibbonButton_Click(object sender, EventArgs e)
        {
            var pg = new PegGenerator(null);
            mParent.SetEditorTool(new DrawEditorTool(pg, false));
        }

        private void brickGeneratorRibbonButton_Click(object sender, EventArgs e)
        {
            var bg = new BrickGenerator(null);
            mParent.SetEditorTool(new DrawEditorTool(bg, false));
        }

        private void applyFunctionRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            var entries = LevelEditor.GetSelectedObjects()
                .Cast<LevelEntry>()
                .Where(x => x is IEntryFunction)
                .Cast<IEntryFunction>()
                .ToArray();
            if (entries.Length != 0)
            {
                LevelEditor.CreateUndoPoint();
                foreach (var entry in entries)
                    entry.Execute();
            }

            LevelEditor.ClearSelection();
            LevelEditor.UpdateRedraw();
        }

        private void removeDuplicatePegsRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.RemoveDuplicateObjects();
        }

        private void removeOffscreenPegsRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.RemoveOffscreenObjects();
        }



        #endregion

        #region Object

        private void sameLocationRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            bool firstItem = true;
            float x = 0.0f, y = 0.0f;
            foreach (LevelEntry o in LevelEditor.GetSelectedObjects())
            {
                if (firstItem)
                {
                    x = o.X;
                    y = o.Y;
                    firstItem = false;
                }
                else
                {
                    o.X = x;
                    o.Y = y;
                }
            }

            LevelEditor.UpdateRedraw();
            UpdatePropertyGrid();
        }

        private void spreadPhaseRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            List<Mover> movements = new List<Mover>();
            foreach (LevelEntry m in LevelEditor.GetSelectedObjects())
            {
                if (m.MoverLink.OwnsMover)
                {
                    movements.Add(m.MoverLink.Mover);
                }
            }

            for (int i = 0; i < movements.Count; i++)
            {
                movements[i].Phase = (float)i / (float)movements.Count;
            }

            LevelEditor.UpdateRedraw();
            UpdatePropertyGrid();
        }

        private void duplicateAndPhaseRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            //Check if there is only one peg selected
            LevelEntryCollection objs = LevelEditor.GetSelectedObjects();
            if (objs.Count != 1)
            {
                MessageBox.Show("You must have only one movement peg selected.");
                return;
            }

            //Check if its a moving peg
            LevelEntry movementPeg = objs[0];
            if (movementPeg == null)
            {
                MessageBox.Show("You must have only one movement peg selected.");
                return;
            }

            //Check if the peg has moving details
            if (movementPeg.MoverLink?.OwnsMover != true)
            {
                MessageBox.Show("The peg must have unshared movement properties.");
                return;
            }

            //Find out how many pegs to duplicate
            string ans = InputForm.Show("How many pegs would you like in this movement cycle, including the selected one?", "Duplicate and Phase", "8");
            int num_pegs;
            if (!Int32.TryParse(ans, out num_pegs) || num_pegs < 0 || num_pegs > 100)
            {
                MessageBox.Show("Invalid number of pegs.");
                return;
            }

            LevelEntryCollection entries = new LevelEntryCollection();
            entries.Add((LevelEntry)objs[0]);

            //Duplicate the peg
            for (int i = 0; i < num_pegs - 1; i++)
            {
                LevelEntry entry = (LevelEntry)objs[0].Clone();
                LevelEditor.Level.Entries.Add(entry);
                entries.Add(entry);
            }

            LevelEditor.ClearSelection();
            LevelEditor.SelectedEntries = entries;

            spreadPhaseRibbonButton_Click(sender, e);

            LevelEditor.UpdateRedraw();
            UpdatePropertyGrid();
        }

        private void linkSubMovementsRibbonButton_Click(object sender, EventArgs e)
        {
            var selectedObjects = LevelEditor.GetSelectedObjects();
            if (selectedObjects.Count < 2)
            {
                MessageBox.Show("First select the origin object that moves, then all the others you want to set their sub-movement of.");
                return;
            }

            var primary = selectedObjects[0];
            if (!(primary.MoverLink?.Mover is Mover primaryMovement))
            {
                MessageBox.Show("First selected object must have movement information.");
                return;
            }
            var muid = primaryMovement.MUID;

            for (int i = 1; i < selectedObjects.Count; i++)
            {
                var secondary = selectedObjects[i];
                if (secondary.MoverLink?.Mover is Mover movement)
                {
                    if (movement.MoverLink == null)
                        movement.MoverLink = new MoverLink(LevelEditor.Level);
                    movement.MoverLink.InternalLinkId = muid;
                    movement.MoverLink.InternalMover = primaryMovement;
                    movement.AnchorPointX = 0;
                    movement.AnchorPointY = 0;
                }
            }

            LevelEditor.UpdateRedraw();
        }

        private void movementTypeRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            MoverType movementType;
            if (sender is RibbonButton)
            {
                RibbonButton ribbonButton = (RibbonButton)sender;
                movementType = (MoverType)ribbonButton.Tag;
            }
            else
            {
                return;
            }

            foreach (LevelEntry m in LevelEditor.GetSelectedObjects())
            {
                if (m.MoverLink?.Mover is Mover movement)
                {
                    movement.Type = movementType;
                }
                else
                {
                    m.MoverLink = new MoverLink(LevelEditor.Level)
                    {
                        Mover = new Mover(LevelEditor.Level)
                        {
                            Location = m.Location,
                            Type = movementType,
                            Time = 400,
                            Radius = 90
                        }
                    };
                }
            }

            LevelEditor.UpdateRedraw();
            UpdatePropertyGrid();
        }

        #endregion

        #region View

        /// <summary>
        /// Shows the view state of the focused level on the view tab, as each level has its own.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="RibbonCheckBox.Checked"/> does not raise CheckedChanged (only clicking does), so this does
        /// not write the state back to the level or restart its preview. 3D Mode and the power-ups are settings, which
        /// are the same for every level, so they are left alone.
        /// </remarks>
        public void UpdateViewPanel()
        {
            var level = LevelEditor?.Level;
            if (level == null)
                return;

            bool changed = false;
            changed |= SetChecked(chkBackground, level.ShowingBackground);
            changed |= SetChecked(chkInterface, level.ShowingInterface);
            changed |= SetChecked(chkCannon, level.ShowCannon);
            changed |= SetChecked(chkObjects, level.ShowingPegs);
            changed |= SetChecked(chkCollision, level.ShowCollision);
            changed |= SetChecked(chkTransparent, level.TransparentObjects);
            changed |= SetChecked(chkPreview, level.ShowPreview);
            changed |= SetChecked(chkFastForward, level.PreviewSpeed != 1.0);
            changed |= SetChecked(chkParticles, level.ShowParticles);
            if (changed)
                mRibbon.Refresh();
        }

        private static bool SetChecked(RibbonCheckBox checkBox, bool value)
        {
            if (checkBox.Checked == value)
                return false;

            checkBox.Checked = value;
            return true;
        }

        private void showBackgroundRibbonButton_Click(object sender, EventArgs e)
        {
            LevelEditor.Level.ShowingBackground = chkBackground.Checked;
            LevelEditor.UpdateRedraw();
        }

        private void showInterfaceRibbonButton_Click(object sender, EventArgs e)
        {
            LevelEditor.Level.ShowingInterface = chkInterface.Checked;
            LevelEditor.UpdateRedraw();
        }

        private void showPegsRibbonButton_Click(object sender, EventArgs e)
        {
            LevelEditor.Level.ShowingPegs = chkObjects.Checked;
            LevelEditor.UpdateRedraw();
        }

        private void showCollisionRibbonButton_Click(object sender, EventArgs e)
        {
            LevelEditor.Level.ShowCollision = chkCollision.Checked;
            LevelEditor.UpdateRedraw();
        }

        private void showPreviewRibbonButton_Click(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            if (!chkPreview.Checked)
            {
                LevelEditor.Level.ShowPreview = false;
            }
            else
            {
                LevelEditor.Level.ResetPreview();
                LevelEditor.Level.ShowPreview = true;
            }

            LevelEditor.UpdateRedraw();
        }

        private void showCannonRibbonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.Level.ShowCannon = chkCannon.Checked;
            LevelEditor.UpdateRedraw();
        }

        private void transparentRibbonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.Level.TransparentObjects = chkTransparent.Checked;
            LevelEditor.UpdateRedraw();
        }

        private void fastForwardRibbonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsEditorAvailable())
                return;

            LevelEditor.Level.PreviewSpeed = chkFastForward.Checked ? Level.FastForwardSpeed : 1.0;
        }

        private void showParticlesRibbonButton_Click(object sender, EventArgs e)
        {
            LevelEditor.Level.ShowParticles = chkParticles.Checked;
            LevelEditor.UpdateRedraw();
        }

        private void acceleration3DRibbonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.Acceleration3D = chk3DAcceleration.Checked;
            DoSettingUpdate();
        }

        private void showGridRibbonButton_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowGrid = !Settings.Default.ShowGrid;
            DoSettingUpdate();
        }


        private void packExplorerRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.ShowPackExplorerWindow();
        }

        private void propertiesRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.ShowPropertiesWindow();
        }

        private void entryListRibbonButton_Click(object sender, EventArgs e)
        {
            mParent.ShowEntryListWindow();
        }

        #endregion

        #region Help

        private void githubRibbonButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/IntelOrca/PeggleEdit");
        }

        private void readmeRibbonButton_Click(object sender, EventArgs e)
        {
            var peggleEditPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var readmePath = Path.Combine(peggleEditPath, "README.md");
            if (File.Exists(readmePath))
            {
                Process.Start(readmePath);
            }
        }

        private void aboutRibbonButton_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        #endregion

        #endregion

        #region Methods

        private bool IsEditorAvailable()
        {
            return mParent.IsEditorAvailable();
        }

        public void UpdateRedraw()
        {
            LevelEditor.UpdateRedraw();
        }

        private void UnselectAllTools()
        {
            btnSelectTool.Checked = false;
            btnEyedropperTool.Checked = false;
            btnPegTool.Checked = false;
            btnStraightBrickTool.Checked = false;
            btnCurvedBrickTool.Checked = false;
            btnPegPenTool.Checked = false;
            btnBrickPenTool.Checked = false;
            btnCircle.Checked = false;
            btnPolygon.Checked = false;
            btnRod.Checked = false;
            btnTeleport.Checked = false;
            btnEmitter.Checked = false;
            btnGenerator.Checked = false;
            btnPegGenerator.Checked = false;
            btnBrickGenerator.Checked = false;
        }

        public void RefreshSelectedTool(EditorTool tool)
        {
            UnselectAllTools();
            switch (tool)
            {
                case SelectEditorTool _:
                    btnSelectTool.Checked = true;
                    break;
                case EyedropperEditorTool _:
                    btnEyedropperTool.Checked = true;
                    break;
                case BrickEditorTool brickEditorTool:
                    if (brickEditorTool.Curved)
                        btnCurvedBrickTool.Checked = true;
                    else
                        btnStraightBrickTool.Checked = true;
                    break;
                case PenEditorTool penEditorTool:
                    if (penEditorTool.PegKind == PegKind.Ball)
                        btnPegPenTool.Checked = true;
                    else
                        btnBrickPenTool.Checked = true;
                    break;
                case DrawEditorTool drawEditorTool:
                    switch (drawEditorTool.Template)
                    {
                        case Ball circle when circle.HasPegInfo:
                            btnPegTool.Checked = true;
                            break;
                        case Ball _:
                            btnCircle.Checked = true;
                            break;
                        case Poly _:
                            btnPolygon.Checked = true;
                            break;
                        case Line _:
                            btnRod.Checked = true;
                            break;
                        case Hole _:
                            btnTeleport.Checked = true;
                            break;
                        case Emitter _:
                            btnEmitter.Checked = true;
                            break;
                        case PegGenerator _:
                            btnGenerator.Checked = true;
                            btnPegGenerator.Checked = true;
                            break;
                        case BrickGenerator _:
                            btnGenerator.Checked = true;
                            btnBrickGenerator.Checked = true;
                            break;
                    }
                    break;
            }
        }

        public void SelectDefaultTool()
        {
            selectRibbonButton_Click(this, EventArgs.Empty);
        }

        private void DoSettingUpdate()
        {
            foreach (LevelToolWindow ltw in mParent.GetAllOpenLevelToolWindows())
            {
                LevelEditor editor = ltw.LevelEditor;

                //editor.GridShowing = Settings.ShowGrid;
                //editor.GridSnap = (Settings.SnapToGrid & Settings.ShowGrid);
                //editor.GridSize = Settings.GridSize;
                //editor.GridSnapThreshold = Settings.SnapThreshold;

                editor.UpdateRedraw();
            }
        }



        private void UpdatePropertyGrid()
        {
            mParent.UpdateProperties(LevelEditor.GetSelectedObjects().ToArray());
        }

        public void UpdateRecentPackFiles()
        {
            RibbonItemCollection collection = mRibbon.OrbDropDown.RecentItems;
            collection.Clear();
            for (int i = 0; i < Settings.Default.RecentPackFiles.Count; i++)
            {
                RibbonButton item = new RibbonButton();
                item.Text = PathShortener(Settings.Default.RecentPackFiles[i], 40);
                item.Tag = Settings.Default.RecentPackFiles[i];

                item.Click += new EventHandler(recentItem_Click);

                collection.Add(item);
            }
        }

        private void recentItem_Click(object sender, EventArgs e)
        {
            RibbonItem item = (RibbonItem)sender;
            string path = (string)item.Tag;
            if (!File.Exists(path))
            {
                MessageBox.Show(String.Format("'{0}' does not exist.", path), "Open Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                mParent.OpenPack(path);
            }
        }

        private static string PathShortener(string path, int length)
        {
            return PlatformServices.Current.CompactPath(path, length);
        }

        #endregion

        #region Shortcuts

        private ShortcutAction[] mShortcuts;

        private void InitShortcuts()
        {
            mShortcuts = new ShortcutAction[] {
                new ShortcutAction(Keys.F1, false, new EventHandler(readmeRibbonButton_Click)),

                new ShortcutAction((Keys)49, false, true, new EventHandler(selectRibbonButton_Click)),
                new ShortcutAction((Keys)50, false, true, new EventHandler(eyedropperRibbonButton_Click)),
                new ShortcutAction((Keys)51, false, true, new EventHandler(pegRibbonButton_Click)),
                new ShortcutAction((Keys)52, false, true, new EventHandler(straightBrickRibbonButton_Click)),
                new ShortcutAction((Keys)53, false, true, new EventHandler(curvedBrickRibbonButton_Click)),
                new ShortcutAction((Keys)54, false, true, new EventHandler(pegPenRibbonButton_Click)),
                new ShortcutAction((Keys)55, false, true, new EventHandler(brickPenRibbonButton_Click)),

                new ShortcutAction(Keys.N, true, new EventHandler(newRibbonButton_Click)),
                new ShortcutAction(Keys.O, true, new EventHandler(openRibbonButton_Click)),
                new ShortcutAction(Keys.S, true, new EventHandler(saveRibbonButton_Click)),
                new ShortcutAction(Keys.Z, true, true, new EventHandler(undoRibbonButton_Click)),
                new ShortcutAction(Keys.X, true, true, new EventHandler(cutRibbonButton_Click)),
                new ShortcutAction(Keys.C, true, true, new EventHandler(copyRibbonButton_Click)),
                new ShortcutAction(Keys.V, true, true, new EventHandler(pasteRibbonButton_Click)),
                new ShortcutAction(Keys.Delete, false, true, new EventHandler(deleteRibbonButton_Click)),
                new ShortcutAction(Keys.A, true, true, new EventHandler(selectAllRibbonButton_Click)),
                new ShortcutAction(Keys.Home, false, true, new EventHandler(bringToFrontRibbonButton_Click)),
                new ShortcutAction(Keys.End, false, true, new EventHandler(sendToBackRibbonButton_Click)),
                new ShortcutAction(Keys.PageUp, false, true, new EventHandler(bringForwardRibbonButton_Click)),
                new ShortcutAction(Keys.PageDown, false, true, new EventHandler(sendBackwardRibbonButton_Click)),
                new ShortcutAction((Keys)222, false, true, new EventHandler(showGridRibbonButton_Click)),
            };

            mParent.RegisterShortcutContainer(this);
        }

        public ShortcutAction[] GetShortcuts()
        {
            return mShortcuts;
        }

        #endregion
    }
}
