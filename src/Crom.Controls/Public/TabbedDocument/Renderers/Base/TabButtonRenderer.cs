/***************************************************************************
 *   CopyRight (C) 2009 by Cristinel Mazarine                              *
 *   Author:   Cristinel Mazarine                                          *
 *   Contact:  cristinel@osec.ro                                           *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the Crom Free License as published by           *
 *   the SC Crom-Osec SRL; version 1 of the License                        *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   Crom Free License for more details.                                   *
 *                                                                         *
 *   You should have received a copy of the Crom Free License along with   *
 *   this program; if not, write to the contact@osec.ro                    *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Crom.Controls.TabbedDocument
{
   /// <summary>
   /// Tab button renderer
   /// </summary>
   public abstract class TabButtonRenderer
   {
      #region Fields.

      private Color              _textColor           = Color.Black;
      private Color              _backGradient1       = Color.FromArgb(240, 240, 240);
      private Color              _backGradient2       = Color.White;
      private Color              _border1             = Color.White;
      private Color              _border2             = Color.DarkGray;
      private Color              _sbackGradient1      = Color.White;
      private Color              _sbackGradient2      = Color.FromArgb(224, 223, 227);
      private Color              _sborder1            = Color.White;
      private Color              _sborder2            = Color.DarkGray;
      private LinearGradientMode _backGradientMode    = LinearGradientMode.Vertical;

      #endregion Fields.

      #region Instance.

      /// <summary>
      /// Default constructor
      /// </summary>
      protected TabButtonRenderer()
      {
      }

      #endregion Instance.

      #region Public section.

      /// <summary>
      /// Text color
      /// </summary>
      public Color TextColor
      {
         get { return _textColor; }
         set { _textColor = value; }
      }

      /// <summary>
      /// Border color 1
      /// </summary>
      public Color Border1
      {
         get { return _border1; }
         set { _border1 = value; }
      }

      /// <summary>
      /// Border color 2
      /// </summary>
      public Color Border2
      {
         get { return _border2; }
         set { _border2 = value; }
      }

      /// <summary>
      /// Back gradient color 1
      /// </summary>
      public Color BackGradient1
      {
         get { return _backGradient1; }
         set { _backGradient1 = value; }
      }

      /// <summary>
      /// Back gradient color 2
      /// </summary>
      public Color BackGradient2
      {
         get { return _backGradient2; }
         set { _backGradient2 = value; }
      }

      /// <summary>
      /// Selected Border color 1
      /// </summary>
      public Color SelectedBorder1
      {
         get { return _sborder1; }
         set { _sborder1 = value; }
      }

      /// <summary>
      /// Selected Border color 2
      /// </summary>
      public Color SelectedBorder2
      {
         get { return _sborder2; }
         set { _sborder2 = value; }
      }

      /// <summary>
      /// Selected Back gradient color 1
      /// </summary>
      public Color SelectedBackGradient1
      {
         get { return _sbackGradient1; }
         set { _sbackGradient1 = value; }
      }

      /// <summary>
      /// Selected Back gradient color 2
      /// </summary>
      public Color SelectedBackGradient2
      {
         get { return _sbackGradient2; }
         set { _sbackGradient2 = value; }
      }


      /// <summary>
      /// Back gradient mode
      /// </summary>
      public LinearGradientMode BackGradientMode
      {
         get { return _backGradientMode; }
         set { _backGradientMode = value; }
      }

      /// <summary>
      /// Draw the button
      /// </summary>
      /// <param name="bounds">button bounds</param>
      /// <param name="text">button text</param>
      /// <param name="selected">flag indicating that should use selected colors</param>
      /// <param name="font">font used to draw the text</param>
      /// <param name="icon">icon associated with the button</param>
      /// <param name="graphics">graphics</param>
      public abstract void Draw(Rectangle bounds, string text, bool selected, Font font, Icon icon, Graphics graphics);

      /// <summary>
      /// Draw buttons line
      /// </summary>
      /// <param name="graphics">graphics object</param>
      /// <param name="panelBounds">panel bounds</param>
      /// <param name="buttonsPanelBounds">buttons panel bounds</param>
      public abstract void DrawButtonsLine(Graphics graphics, Rectangle panelBounds, Rectangle buttonsPanelBounds);

      /// <summary>
      /// Draw the scroll back button
      /// </summary>
      /// <param name="bounds">button bounds</param>
      /// <param name="state">flag indicating the state of the button</param>
      /// <param name="graphics">graphics</param>
      public abstract void DrawScrollBackButton(Rectangle bounds, zButtonState state, Graphics graphics);

      /// <summary>
      /// Draw the scroll next button
      /// </summary>
      /// <param name="bounds">button bounds</param>
      /// <param name="state">flag indicating the state of the button</param>
      /// <param name="graphics">graphics</param>
      public abstract void DrawScrollNextButton(Rectangle bounds, zButtonState state, Graphics graphics);

      /// <summary>
      /// Checks if the panel has scrolls
      /// </summary>
      /// <param name="panelBounds">panel bounds</param>
      /// <param name="lastButtonBounds">last button bounds</param>
      /// <returns>true if the panel has scrolls</returns>
      public abstract bool HasScroll(Rectangle panelBounds, Rectangle lastButtonBounds);

      /// <summary>
      /// Get the bounds of the scroll back button
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <returns>bounds of the scroll back button</returns>
      public abstract Rectangle GetScrollBackButtonBounds(Rectangle panelBounds);

      /// <summary>
      /// Get the bounds of the scroll next button
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <returns>bounds of the scroll next button</returns>
      public abstract Rectangle GetScrollNextButtonBounds(Rectangle panelBounds);

      /// <summary>
      /// Get first button bounds
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <param name="hasScroll">flag indicating that panel has scrolls</param>
      /// <param name="scrolPos">scroll position</param>
      /// <param name="text">text of the button</param>
      /// <param name="font">font used to draw the button</param>
      /// <param name="icon">icon associated with the button</param>
      /// <param name="graphics">graphics object</param>
      /// <returns>bounds of the first button</returns>
      public abstract Rectangle GetFirstButtonBounds(Rectangle panelBounds, bool hasScroll, int scrolPos, string text, Font font, Icon icon, Graphics graphics);

      /// <summary>
      /// Get next button bounds
      /// </summary>
      /// <param name="previousBounds">bounds of the previous button</param>
      /// <param name="text">text of the button</param>
      /// <param name="font">font used to draw the button</param>
      /// <param name="icon">icon associated with the button</param>
      /// <param name="graphics">graphics object</param>
      /// <returns>bounds of the next button</returns>
      public abstract Rectangle GetNextButtonBounds(Rectangle previousBounds, string text, Font font, Icon icon, Graphics graphics);

      /// <summary>
      /// Get client rectangle for the panel bounds
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <returns>client rectangle for the panel</returns>
      public abstract Rectangle GetClientRectangle(Rectangle panelBounds);

      /// <summary>
      /// Get buttons clip rectangle
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <param name="hasScroll">true if the panel has scroll</param>
      /// <param name="captionButtonsCount">number of caption buttons that may be drawn on buttons panel</param>
      /// <returns>clip rectangle for the buttons</returns>
      public abstract Rectangle GetButtonsClipRectangle(Rectangle panelBounds, bool hasScroll, int captionButtonsCount);

      /// <summary>
      /// Get the rectangle which will contain caption buttons drawn on panel buttons
      /// </summary>
      /// <param name="panelBounds">panel bounds</param>
      /// <param name="captionButtonsCount">number of caption buttons</param>
      /// <returns>rectangle containing caption buttons</returns>
      public abstract Rectangle GetCaptionButtonsRectangle(Rectangle panelBounds, int captionButtonsCount);

      /// <summary>
      /// Get the caption button index
      /// </summary>
      /// <param name="captionButtonsBounds">bounds of the caption buttons block</param>
      /// <param name="point">point where to search the button</param>
      /// <returns>zero based caption button index</returns>
      public abstract int GetCaptionButtonIndex(Rectangle captionButtonsBounds, Point point);

      /// <summary>
      /// Can undo the displacement of the next button (the next button was displaced before)
      /// </summary>
      /// <param name="displacedButton">displaced button</param>
      /// <param name="selectedButton">selected button</param>
      /// <param name="mouseLocation">current mouse location</param>
      /// <returns>true if can undo</returns>
      internal abstract bool CanUndoDisplaceNext(TabButton displacedButton, TabButton selectedButton, Point mouseLocation);

      /// <summary>
      /// Can undo the displacement of the previous button (the previous button was displaced before)
      /// </summary>
      /// <param name="displacedButton">displaced button</param>
      /// <param name="selectedButton">selected button</param>
      /// <param name="mouseLocation">current mouse location</param>
      /// <returns>true if can undo</returns>
      internal abstract bool CanUndoDisplaceBack(TabButton displacedButton, TabButton selectedButton, Point mouseLocation);

      /// <summary>
      /// Checks if can scroll next
      /// </summary>
      /// <param name="lastButton">last button</param>
      /// <param name="scrollNextBounds">scroll-next bounds</param>
      /// <returns>true if can scroll next</returns>
      internal abstract bool CanScrollNext(TabButton lastButton, Rectangle scrollNextBounds);

      /// <summary>
      /// Get scroll position
      /// </summary>
      /// <param name="buttons">buttons collection</param>
      /// <param name="firstShownButtonIndex">zero based first shown button index</param>
      /// <returns>scroll position</returns>
      internal abstract int GetScrollPos(IList<TabButton> buttons, int firstShownButtonIndex);

      #endregion Public section

      #region Protected section.

      /// <summary>
      /// Gets border color 1
      /// </summary>
      /// <param name="selected">flag indicating that is requested the selected color</param>
      protected Color GetBorder1Color(bool selected)
      {
         if (selected)
         {
            return SelectedBorder1;
         }

         return Border1;
      }

      /// <summary>
      /// Gets border color 2
      /// </summary>
      /// <param name="selected">flag indicating that is requested the selected color</param>
      protected Color GetBorder2Color(bool selected)
      {
         if (selected)
         {
            return SelectedBorder2;
         }

         return Border2;
      }

      /// <summary>
      /// Gets back gradient color 1
      /// </summary>
      /// <param name="selected">flag indicating that is requested the selected color</param>
      protected Color GetBackGradient1Color(bool selected)
      {
         if (selected)
         {
            return SelectedBackGradient1;
         }

         return BackGradient1;
      }

      /// <summary>
      /// Gets back gradient color 2
      /// </summary>
      /// <param name="selected">flag indicating that is requested the selected color</param>
      protected Color GetBackGradient2Color(bool selected)
      {
         if (selected)
         {
            return SelectedBackGradient2;
         }

         return BackGradient2;
      }

      #endregion Protected section.
   }
}
