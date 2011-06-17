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

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Size mode
   /// </summary>
   internal enum zSizeMode
   {
      /// <summary>
      /// No size 
      /// </summary>
      None     = 0,
      /// <summary>
      /// Resize from left edge
      /// </summary>
      Left     = 1,
      /// <summary>
      /// Resize from right edge
      /// </summary>
      Right    = 2,
      /// <summary>
      /// Resize from top edge
      /// </summary>
      Top      = 4,
      /// <summary>
      /// Resize from bottom edge
      /// </summary>
      Bottom   = 8,
      /// <summary>
      /// Move
      /// </summary>
      Move = 16,
   }
}
