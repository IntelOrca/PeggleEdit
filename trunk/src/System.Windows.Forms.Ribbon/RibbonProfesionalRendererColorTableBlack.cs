using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace System.Windows.Forms
{
    public class RibbonProfesionalRendererColorTableBlack
        : RibbonProfesionalRendererColorTable
    {
        public RibbonProfesionalRendererColorTableBlack()
        {
            #region Fields

            OrbDropDownDarkBorder = ToGray(OrbDropDownDarkBorder);
            OrbDropDownLightBorder = ToGray(OrbDropDownLightBorder);
            OrbDropDownBack = ToGray(OrbDropDownBack);
            OrbDropDownNorthA = ToGray(OrbDropDownNorthA);
            OrbDropDownNorthB = ToGray(OrbDropDownNorthB);
            OrbDropDownNorthC = ToGray(OrbDropDownNorthC);
            OrbDropDownNorthD = ToGray(OrbDropDownNorthD);
            OrbDropDownSouthC = ToGray(OrbDropDownSouthC);
            OrbDropDownSouthD = ToGray(OrbDropDownSouthD);
            OrbDropDownContentbg = ToGray(OrbDropDownContentbg);
            OrbDropDownContentbglight = ToGray(OrbDropDownContentbglight);
            OrbDropDownSeparatorlight = ToGray(OrbDropDownSeparatorlight);
            OrbDropDownSeparatordark = ToGray(OrbDropDownSeparatordark);

            Caption1 = ToGray(Caption1); //4
            Caption2 = ToGray(Caption2);
            Caption3 = ToGray(Caption3); //4
            Caption4 = ToGray(Caption4);
            Caption5 = ToGray(Caption5); //23
            Caption6 = ToGray(Caption6);
            Caption7 = ToGray(Caption7); //1

            QuickAccessBorderDark = ToGray(QuickAccessBorderDark);
            QuickAccessBorderLight = ToGray(QuickAccessBorderLight);
            QuickAccessUpper = ToGray(QuickAccessUpper);
            QuickAccessLower = ToGray(QuickAccessLower);

            OrbOptionBorder = ToGray(OrbOptionBorder);
            OrbOptionBackground = ToGray(OrbOptionBackground);
            OrbOptionShine = ToGray(OrbOptionShine);

            Arrow = FromHex("#7C7C7C");
            ArrowLight = FromHex("#EAF2F9");
            ArrowDisabled = FromHex("#7C7C7C");
            Text = FromHex("#000000");

            RibbonBackground = FromHex("#535353");
            TabBorder = FromHex("#BEBEBE");
            TabNorth = FromHex("#F1F2F2");
            TabSouth = FromHex("#D6D9DF");
            TabGlow = FromHex("#D1FBFF");
            TabText = Color.White;
            TabActiveText = Color.Black;
            TabContentNorth = FromHex("#B6BCC6");
            TabContentSouth = FromHex("#E6F0F1");
            TabSelectedGlow = FromHex("#E1D2A5");
            PanelDarkBorder = FromHex("#AEB0B4"); //Color.FromArgb(51, FromHex("#FF0000"));
            PanelLightBorder = FromHex("#E7E9ED"); //Color.FromArgb(102, Color.White);
            PanelTextBackground = FromHex("#ABAEAE");
            PanelTextBackgroundSelected = FromHex("#949495");
            PanelText = Color.White;
            PanelBackgroundSelected = FromHex("#F3F5F5"); // Color.FromArgb(102, FromHex("#E8FFFD"));
            PanelOverflowBackground = FromHex("#B9D1F0");
            PanelOverflowBackgroundPressed = FromHex("#AAAEB3");
            PanelOverflowBackgroundSelectedNorth = Color.FromArgb(100, Color.White);
            PanelOverflowBackgroundSelectedSouth = Color.FromArgb(102, FromHex("#EBEBEB"));

            ButtonBgOut = FromHex("#B4B9C2"); // FromHex("#C1D5F1");
            ButtonBgCenter = FromHex("#CDD2D8");
            ButtonBorderOut = FromHex("#A9B1B8");
            ButtonBorderIn = FromHex("#DFE2E6");
            ButtonGlossyNorth = FromHex("#DBDFE4");
            ButtonGlossySouth = FromHex("#DFE2E8");

            ButtonDisabledBgOut = FromHex("#E0E4E8");
            ButtonDisabledBgCenter = FromHex("#E8EBEF");
            ButtonDisabledBorderOut = FromHex("#C5D1DE");
            ButtonDisabledBorderIn = FromHex("#F1F3F5");
            ButtonDisabledGlossyNorth = FromHex("#F0F3F6");
            ButtonDisabledGlossySouth = FromHex("#EAEDF1");

            ButtonSelectedBgOut = FromHex("#FFD646");
            ButtonSelectedBgCenter = FromHex("#FFEAAC");
            ButtonSelectedBorderOut = FromHex("#C2A978");
            ButtonSelectedBorderIn = FromHex("#FFF2C7");
            ButtonSelectedGlossyNorth = FromHex("#FFFDDB");
            ButtonSelectedGlossySouth = FromHex("#FFE793");

            ButtonPressedBgOut = FromHex("#F88F2C");
            ButtonPressedBgCenter = FromHex("#FDF1B0");
            ButtonPressedBorderOut = FromHex("#8E8165");
            ButtonPressedBorderIn = FromHex("#F9C65A");
            ButtonPressedGlossyNorth = FromHex("#FDD5A8");
            ButtonPressedGlossySouth = FromHex("#FBB062");

            ButtonCheckedBgOut = FromHex("#F9AA45");
            ButtonCheckedBgCenter = FromHex("#FDEA9D");
            ButtonCheckedBorderOut = FromHex("#8E8165");
            ButtonCheckedBorderIn = FromHex("#F9C65A");
            ButtonCheckedGlossyNorth = FromHex("#F8DBB7");
            ButtonCheckedGlossySouth = FromHex("#FED18E");

            ItemGroupOuterBorder = FromHex("#ADB7BB");
            ItemGroupInnerBorder = Color.FromArgb(51, Color.White);
            ItemGroupSeparatorLight = Color.FromArgb(64, Color.White);
            ItemGroupSeparatorDark = Color.FromArgb(38, FromHex("#ADB7BB"));
            ItemGroupBgNorth = FromHex("#D9E0E1");
            ItemGroupBgSouth = FromHex("#EDF0F1");
            ItemGroupBgGlossy = FromHex("#D2D9DB");

            ButtonListBorder = FromHex("#ACACAC");
            ButtonListBg = FromHex("#DAE2E2");
            ButtonListBgSelected = FromHex("#F7F7F7");

            DropDownBg = FromHex("#FAFAFA");
            DropDownImageBg = FromHex("#E9EEEE");
            DropDownImageSeparator = FromHex("#C5C5C5");
            DropDownBorder = FromHex("#868686");
            DropDownGripNorth = FromHex("#FFFFFF");
            DropDownGripSouth = FromHex("#DFE9EF");
            DropDownGripBorder = FromHex("#DDE7EE");
            DropDownGripDark = FromHex("#5574A7");
            DropDownGripLight = FromHex("#FFFFFF");

            SeparatorLight = FromHex("#E6E8EB");
            SeparatorDark = FromHex("#C5C5C5");
            SeparatorBg = FromHex("#EBEBEB");
            SeparatorLine = FromHex("#C5C5C5");

            TextBoxUnselectedBg = FromHex("#E8E8E8");
            TextBoxBorder = FromHex("#898989");

            #endregion
        }

        
    }
}
