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
   /// Template event args
   /// </summary>
   /// <typeparam name="T">args</typeparam>
   internal class TemplateEventArgs<T> : EventArgs
   {
      #region Fields

      private T _data = default(T);

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="data">data</param>
      public TemplateEventArgs(T data)
      {
         _data = data;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Data accessor
      /// </summary>
      public T Data
      {
         get { return _data; }
      }

      #endregion Public section
   }
}
