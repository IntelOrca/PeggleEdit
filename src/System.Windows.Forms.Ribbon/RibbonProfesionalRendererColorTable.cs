/*
 
 2008 José Manuel Menéndez Poo
 * 
 * Please give me credit if you use this code. It's all I ask.
 * 
 * Contact me for more info: menendezpoo@gmail.com
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace System.Windows.Forms
{
    public class RibbonProfesionalRendererColorTable
    {
        #region Pendent for black

        public Color FormBorder = FromHex("#3B5A82");

        public Color OrbDropDownDarkBorder = Color.FromArgb(0x9b, 0xaf, 0xca);
        public Color OrbDropDownLightBorder = Color.FromArgb(0xff, 0xff, 0xff);
        public Color OrbDropDownBack = Color.FromArgb(0xbf, 0xd3, 0xeb);
        public Color OrbDropDownNorthA = Color.FromArgb(0xd7, 0xe5, 0xf7);
        public Color OrbDropDownNorthB = Color.FromArgb(0xd4, 0xe1, 0xf3);
        public Color OrbDropDownNorthC = Color.FromArgb(0xc6, 0xd8, 0xee);
        public Color OrbDropDownNorthD = Color.FromArgb(0xb7, 0xca, 0xe6);
        public Color OrbDropDownSouthC = Color.FromArgb(0xb0, 0xc9, 0xea);
        public Color OrbDropDownSouthD = Color.FromArgb(0xcf, 0xe0, 0xf5);
        public Color OrbDropDownContentbg = Color.FromArgb(0xE9, 0xEA, 0xEE);
        public Color OrbDropDownContentbglight = Color.FromArgb(0xFA, 0xFA, 0xFA);
        public Color OrbDropDownSeparatorlight = Color.FromArgb(0xF5, 0xF5, 0xF5);
        public Color OrbDropDownSeparatordark = Color.FromArgb(0xC5, 0xC5, 0xC5);

        /// <summary>
        /// Caption bar is made of 4 rectangles height of each is indicated below
        /// </summary>
        public Color Caption1 = FromHex("#E3EBF6"); //4
        public Color Caption2 = FromHex("#DAE9FD");
        public Color Caption3 = FromHex("#D5E5FA"); //4
        public Color Caption4 = FromHex("#D9E7F9");
        public Color Caption5 = FromHex("#CADEF7"); //23
        public Color Caption6 = FromHex("#E4EFFD");
        public Color Caption7 = FromHex("#B0CFF7"); //1

        public Color QuickAccessBorderDark = FromHex("#B6CAE2");
        public Color QuickAccessBorderLight = FromHex("#F2F6FB");
        public Color QuickAccessUpper = FromHex("#E0EBF9");
        public Color QuickAccessLower = FromHex("#C9D9EE");

        public Color OrbOptionBorder = FromHex("#7793B9");
        public Color OrbOptionBackground = FromHex("#E8F1FC");
        public Color OrbOptionShine = FromHex("#D2E1F4");

        #endregion

        #region Fields

        public Color Arrow = FromHex("#678CBD");
        public Color ArrowLight = Color.FromArgb(200, Color.White);
        public Color ArrowDisabled = FromHex("#B7B7B7");
        public Color Text = FromHex("#15428B");

        /// <summary>
        /// Orb colors in normal state
        /// </summary>
        public Color OrbBackgroundDark = FromHex("#7C8CA4");
        public Color OrbBackgroundMedium = FromHex("#99ABC6");
        public Color OrbBackgroundLight = Color.White;
        public Color OrbLight = Color.White;

        /// <summary>
        /// Orb colors in selected state
        /// </summary>
        public Color OrbSelectedBackgroundDark = FromHex("#DFAA1A");
        public Color OrbSelectedBackgroundMedium = FromHex("#F9D12E");
        public Color OrbSelectedBackgroundLight = FromHex("#FFEF36");
        public Color OrbSelectedLight = FromHex("#FFF52B");

        /// <summary>
        /// Orb colors in pressed state
        /// </summary>
        public Color OrbPressedBackgroundDark = FromHex("#CE8410");
        public Color OrbPressedBackgroundMedium = FromHex("#CE8410");
        public Color OrbPressedBackgroundLight = FromHex("#F57603");
        public Color OrbPressedLight = FromHex("#F08500");
        public Color OrbBorderAero = FromHex("#99A1AD");

        public Color RibbonBackground = FromHex("#BFDBFF");
        public Color TabBorder = FromHex("#8DB2E3");
        public Color TabNorth = FromHex("#EBF3FE");
        public Color TabSouth = FromHex("#E1EAF6");
        public Color TabGlow = FromHex("#D1FBFF");
        public Color TabText = FromHex("#15428B");
        public Color TabActiveText = FromHex("#15428B");
        public Color TabContentNorth = FromHex("#C8D9ED");
        public Color TabContentSouth = FromHex("#E7F2FF");
        public Color TabSelectedGlow = FromHex("#E1D2A5");
        public Color PanelDarkBorder = Color.FromArgb(51, FromHex("#15428B"));
        public Color PanelLightBorder = Color.FromArgb(102, Color.White);
        public Color PanelTextBackground = FromHex("#C2D9F0");
        public Color PanelTextBackgroundSelected = FromHex("#C2D9F0");
        public Color PanelText = FromHex("#15428B");
        public Color PanelBackgroundSelected = Color.FromArgb(102, FromHex("#E8FFFD"));
        public Color PanelOverflowBackground = FromHex("#B9D1F0");
        public Color PanelOverflowBackgroundPressed = FromHex("#7699C8");
        public Color PanelOverflowBackgroundSelectedNorth = Color.FromArgb(100, Color.White);
        public Color PanelOverflowBackgroundSelectedSouth = Color.FromArgb(102, FromHex("#B8D7FD"));

        public Color ButtonBgOut = FromHex("#C1D5F1");
        public Color ButtonBgCenter = FromHex("#CFE0F7");
        public Color ButtonBorderOut = FromHex("#B9D0ED");
        public Color ButtonBorderIn = FromHex("#E3EDFB");
        public Color ButtonGlossyNorth = FromHex("#DEEBFE");
        public Color ButtonGlossySouth = FromHex("#CBDEF6");

        public Color ButtonDisabledBgOut = FromHex("#E0E4E8");
        public Color ButtonDisabledBgCenter = FromHex("#E8EBEF");
        public Color ButtonDisabledBorderOut = FromHex("#C5D1DE");
        public Color ButtonDisabledBorderIn = FromHex("#F1F3F5");
        public Color ButtonDisabledGlossyNorth = FromHex("#F0F3F6");
        public Color ButtonDisabledGlossySouth = FromHex("#EAEDF1");

        public Color ButtonSelectedBgOut = FromHex("#FFD646");
        public Color ButtonSelectedBgCenter = FromHex("#FFEAAC");
        public Color ButtonSelectedBorderOut = FromHex("#C2A978");
        public Color ButtonSelectedBorderIn = FromHex("#FFF2C7");
        public Color ButtonSelectedGlossyNorth = FromHex("#FFFDDB");
        public Color ButtonSelectedGlossySouth = FromHex("#FFE793");

        public Color ButtonPressedBgOut = FromHex("#F88F2C");
        public Color ButtonPressedBgCenter = FromHex("#FDF1B0");
        public Color ButtonPressedBorderOut = FromHex("#8E8165");
        public Color ButtonPressedBorderIn = FromHex("#F9C65A");
        public Color ButtonPressedGlossyNorth = FromHex("#FDD5A8");
        public Color ButtonPressedGlossySouth = FromHex("#FBB062");

        public Color ButtonCheckedBgOut = FromHex("#F9AA45");
        public Color ButtonCheckedBgCenter = FromHex("#FDEA9D");
        public Color ButtonCheckedBorderOut = FromHex("#8E8165");
        public Color ButtonCheckedBorderIn = FromHex("#F9C65A");
        public Color ButtonCheckedGlossyNorth = FromHex("#F8DBB7");
        public Color ButtonCheckedGlossySouth = FromHex("#FED18E");

        public Color ItemGroupOuterBorder = FromHex("#9EBAE1");
        public Color ItemGroupInnerBorder = Color.FromArgb(51, Color.White);
        public Color ItemGroupSeparatorLight = Color.FromArgb(64, Color.White);
        public Color ItemGroupSeparatorDark = Color.FromArgb(38, FromHex("#9EBAE1"));
        public Color ItemGroupBgNorth = FromHex("#CADCF0");
        public Color ItemGroupBgSouth = FromHex("#D0E1F7");
        public Color ItemGroupBgGlossy = FromHex("#BCD0E9");

        public Color ButtonListBorder = FromHex("#B9D0ED");
        public Color ButtonListBg = FromHex("#D4E6F8");
        public Color ButtonListBgSelected = FromHex("#ECF3FB");

        public Color DropDownBg = FromHex("#FAFAFA");
        public Color DropDownImageBg = FromHex("#E9EEEE");
        public Color DropDownImageSeparator = FromHex("#C5C5C5");
        public Color DropDownBorder = FromHex("#868686");
        public Color DropDownGripNorth = FromHex("#FFFFFF");
        public Color DropDownGripSouth = FromHex("#DFE9EF");
        public Color DropDownGripBorder = FromHex("#DDE7EE");
        public Color DropDownGripDark = FromHex("#5574A7");
        public Color DropDownGripLight = FromHex("#FFFFFF");

        public Color SeparatorLight = FromHex("#FAFBFD");
        public Color SeparatorDark = FromHex("#96B4DA");
        public Color SeparatorBg = FromHex("#DAE6EE");
        public Color SeparatorLine = FromHex("#C5C5C5");

        public Color TextBoxUnselectedBg = FromHex("#EAF2FB");
        public Color TextBoxBorder = FromHex("#ABC1DE");

        #endregion

        #region Methods

        internal static Color FromHex(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length != 6) throw new Exception("Color not valid");

            return Color.FromArgb(
                int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
        }

        internal static Color ToGray(Color c)
        {
            int m = (c.R + c.G + c.B ) / 3;
            return Color.FromArgb(m, m, m);
        }

        #endregion
    }
}