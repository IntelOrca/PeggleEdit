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

namespace Crom.Controls
{
   /// <summary>
   /// Implementation of disposable objects
   /// </summary>
   public abstract class Disposable : IDisposable
   {
      #region Fields

      private bool         _isDisposing            = false;
      private bool         _isDisposed             = false;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      protected Disposable()
      {
      }

      /// <summary>
      /// Destructor
      /// </summary>
      ~Disposable()
      {
         DisposeInstance(false);
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Event raised when disposing was initiated for current instance by call to IDisposable.Dispose
      /// </summary>
      public event EventHandler ExplicitDisposing;

      /// <summary>
      /// Event raised when disposing was initiated for current instance by GC
      /// </summary>
      public event EventHandler GCDisposing;

      /// <summary>
      /// Event raised after current instance is disposed
      /// </summary>
      public event EventHandler Disposed;

      /// <summary>
      /// Flag indicating that current instance is disposed
      /// </summary>
      public bool IsDisposed
      {
         get { return _isDisposed; }
         private set { _isDisposed = value; }
      }

      /// <summary>
      /// Dispose current instance
      /// </summary>
      public void Dispose()
      {
         DisposeInstance(true);
         GC.SuppressFinalize(this);
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Dispose the current instance
      /// </summary>
      /// <param name="fromIDisposableDispose">dispose was called from IDisposable.Dispose</param>
      protected abstract void Dispose(bool fromIDisposableDispose);

      /// <summary>
      /// Validate that current instance is not disposed
      /// </summary>
      /// <exception cref="ObjectDisposedException">exception thrown if the current instance is disposed</exception>
      protected void ValidateNotDisposed()
      {
         if (IsDisposed)
         {
            throw new ObjectDisposedException(GetType().Name);
         }
      }

      #endregion Protected section

      #region Private section

      /// <summary>
      /// Dispose current instance
      /// </summary>
      /// <param name="fromIDisposableDispose">dispose was called from IDisposable.Dispose</param>
      private void DisposeInstance(bool fromIDisposableDispose)
      {
         if (_isDisposing)
         {
            return;
         }

         if (IsDisposed == false)
         {
            _isDisposing = true;

            EventHandler disposingHandler = ExplicitDisposing;
            if (fromIDisposableDispose == false)
            {
               disposingHandler = GCDisposing;
            }

            try
            {
               if (disposingHandler != null)
               {
                  disposingHandler(this, EventArgs.Empty);
               }

               Dispose(fromIDisposableDispose);
            }
            finally
            {
               IsDisposed   = true;
               _isDisposing = false;

               EventHandler disposedHandler = Disposed;
               if (disposedHandler != null)
               {
                  disposedHandler(this, EventArgs.Empty);
               }
            }
         }
      }

      #endregion Private section
   }
}
