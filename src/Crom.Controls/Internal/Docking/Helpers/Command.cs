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

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Command handler
   /// </summary>
   internal delegate void CommandHandler();

   /// <summary>
   /// Implementation of command
   /// </summary>
   internal class Command
   {
      #region Fields

      private CommandHandler _handler = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public Command()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor of the handler
      /// </summary>
      public CommandHandler Handler
      {
         get { return _handler; }
         set { _handler = value; }
      }

      #endregion Public section
   }
}
