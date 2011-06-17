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
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Serializer for dock state
   /// </summary>
   public class DockStateSerializer
   {
      #region Delegates

      /// <summary>
      /// Form factory handler
      /// </summary>
      /// <param name="identifier">identifier</param>
      /// <returns>form created</returns>
      public delegate Form FormFactoryHandler(Guid identifier);

      #endregion Delegates

      #region Embedded types

      /// <summary>
      /// Control size info
      /// </summary>
      private class ControlSizeInfo
      {
         #region Fields

         private Control   _control    = null;
         private int       _width      = 0;
         private int       _height     = 0;

         #endregion Fields

         #region Instance

         /// <summary>
         /// Constructor
         /// </summary>
         /// <param name="control">control</param>
         /// <param name="width">width</param>
         /// <param name="height">height</param>
         public ControlSizeInfo(Control control, int width, int height)
         {
            _control    = control;
            _width   = width;
            _height  = height;
         }

         #endregion Instance

         #region Public section

         /// <summary>
         /// Accessor of the control
         /// </summary>
         public Control Control
         {
            get { return _control; }
         }

         /// <summary>
         /// Accessor of the width
         /// </summary>
         public int Width
         {
            get { return _width; }
         }

         /// <summary>
         /// Accessor of the height
         /// </summary>
         public int Height
         {
            get { return _height; }
         }

         /// <summary>
         /// Text representation
         /// </summary>
         /// <returns>text</returns>
         public override string ToString()
         {
            return Control.ToString() + " W:" + Width.ToString() + " H:" + Height.ToString();
         }

         #endregion Public section
      }

      #endregion Embedded types

      #region Fields

      private string          _savePath         = null;
      private DockContainer   _container        = null;
      private FormWrapper     _host             = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="container">container</param>
      public DockStateSerializer(DockContainer container)
      {
         _container     = container;
         _host          = new FormWrapper(container, 0, 0);
         string appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
         _savePath      = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
         _savePath      = Path.Combine(_savePath, Resources.StateFileName);
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor of the path where to save the state serialization
      /// </summary>
      public string SavePath
      {
         get { return _savePath; }
         set { _savePath = value; }
      }

      /// <summary>
      /// Save the state of the container
      /// </summary>
      public void Save()
      {
         XmlDocument xmlDocument = new XmlDocument();

         XmlNode xmlForms = xmlDocument.AppendChild(xmlDocument.CreateElement(XmlTags.TagForms));

         for (int index = 0; index < _host.ControlsCount; index++)
         {
            DockableContainer container = _host.GetControlAt(index) as DockableContainer;
            if (container == null)
            {
               continue;
            }

            SaveContainer(container, Guid.Empty, xmlForms);
         }

         if (Directory.Exists(Path.GetDirectoryName(SavePath)) == false)
         {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
         }

         xmlDocument.Save(SavePath);
      }

      /// <summary>
      /// Save container
      /// </summary>
      /// <param name="container">container</param>
      /// <param name="containerOwnerId">container owner id</param>
      /// <param name="xmlForms">forms node</param>
      /// <returns>container id</returns>
      private Guid SaveContainer(DockableContainer container, Guid containerOwnerId, XmlNode xmlForms)
      {
         if (container.SingleChild != null)
         {
            return SaveView(container.SingleChild, containerOwnerId, xmlForms);
         }

         if (container.LinkedView != null)
         {
            return SaveView(container.LinkedView, containerOwnerId, xmlForms);
         }
         
         if (container.LeftPane != null)
         {
            Guid containerId = SaveContainer(container.LeftPane,  containerOwnerId, xmlForms);
            SaveContainer(container.RightPane, containerId, xmlForms);

            return containerId;
         }
         
         if (container.TopPane != null)
         {
            Guid containerId = SaveContainer(container.TopPane, containerOwnerId, xmlForms);
            SaveContainer(container.BottomPane, containerId, xmlForms);

            return containerId;
         }

         throw new NotSupportedException();
      }

      /// <summary>
      /// Load state
      /// </summary>
      /// <param name="clear">clear the existing forms</param>
      /// <param name="formsFactory">forms factory</param>
      public void Load(bool clear, FormFactoryHandler formsFactory)
      {
         if (clear)
         {
            _container.Clear();
         }

         XmlDocument xmlDocument = new XmlDocument();
         xmlDocument.Load(SavePath);

         _container.SuspendLayout();

         DockableFormInfo selected = null;
         List<DockableFormInfo> autoHide = new List<DockableFormInfo>();
         List<ControlSizeInfo> formsSize = LoadForms(xmlDocument, formsFactory, autoHide, out selected);

         RestoreSizes(formsSize);

         foreach (DockableFormInfo toAuto in autoHide)
         {
            _container.SetAutoHide(toAuto, true);
         }

         if (selected != null)
         {
            selected.IsSelected = true;
         }

         _container.ResumeLayout(true);
      }

      #endregion Public section

      #region Private section

      /// <summary>
      /// Save the view
      /// </summary>
      /// <param name="view">view to save</param>
      /// <param name="viewOwnerId">view owner identifier</param>
      /// <param name="xmlForms">forms node</param>
      /// <returns>identifier of the view</returns>
      private Guid SaveView(FormsTabbedView view, Guid viewOwnerId, XmlNode xmlForms)
      {
         Form firstDockedPage = null;
         for (int childIndex = 0; childIndex < view.Count; childIndex++)
         {
            DockableFormInfo childInfo = view.GetPageInfoAt(childIndex);
            if (childInfo.Dock != DockStyle.Fill)
            {
               firstDockedPage = childInfo.DockableForm;
               break;
            }
         }

         if (firstDockedPage == null)
         {
            firstDockedPage = view.GetPageAt(0);
         }

         DockableFormInfo info = _container.GetFormInfo(firstDockedPage);

         Save(info, viewOwnerId, xmlForms);

         for (int childIndex = 0; childIndex < view.Count; childIndex++)
         {
            DockableFormInfo siblingInfo = view.GetPageInfoAt(childIndex);
            if (siblingInfo == info)
            {
               continue;
            }

            Save(siblingInfo, info.Id, xmlForms);
         }

         return info.Id;
      }

      /// <summary>
      /// Save 
      /// </summary>
      /// <param name="info">info</param>
      /// <param name="parentId">parent id identifier</param>
      /// <param name="xmlForms">forms</param>
      private void Save(DockableFormInfo info, Guid parentId, XmlNode xmlForms)
      {
         XmlDocument xmlDocument = xmlForms.OwnerDocument;

         XmlNode xmlForm         = xmlForms.AppendChild(xmlDocument.CreateElement(XmlTags.TagForm));

         XmlNode xmlGuid         = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagGuid));
         XmlNode xmlAllowedDock  = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagAllowedDock));
         XmlNode xmlCurrentDock  = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagCurrentDock));
         XmlNode xmlCurrentMode  = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagCurrentMode));
         XmlNode xmlIsSelected   = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagIsSelected));
         XmlNode xmlIsAutoHide   = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagIsAutoHide));
         XmlNode xmlWidth        = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagWidth));
         XmlNode xmlHeight       = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagHeight));
         if (parentId != Guid.Empty)
         {
            XmlNode xmlParentGuid   = xmlForm.AppendChild(xmlDocument.CreateElement(XmlTags.TagParentGuid));
            xmlParentGuid.InnerText = parentId.ToString();
         }

         xmlGuid.InnerText          = info.Id.ToString();
         xmlAllowedDock.InnerText   = info.AllowedDock.ToString();
         xmlCurrentMode.InnerText   = info.DockMode.ToString();
         xmlIsSelected.InnerText    = info.IsSelected.ToString().ToLower();
         xmlIsAutoHide.InnerText    = info.IsAutoHideMode.ToString().ToLower();

         DockableContainer container = HierarchyUtility.GetClosestDockableContainer(info.DockableForm);

         xmlWidth.InnerText         = container.Width.ToString();
         xmlHeight.InnerText        = container.Height.ToString();

         if (parentId == Guid.Empty)
         {
            if (info.IsAutoHideMode == false)
            {
               xmlCurrentDock.InnerText = info.HostContainerDock.ToString();
            }
            else
            {
               xmlCurrentDock.InnerText = info.AutoHideSavedDock.ToString();
            }
         }
         else
         {
            xmlCurrentDock.InnerText = info.Dock.ToString();
         }
      }

      /// <summary>
      /// Load form
      /// </summary>
      /// <param name="xmlForm">xml form</param>
      /// <param name="formsFactory">forms factory</param>
      /// <param name="autoHide">list of autohide forms</param>
      /// <param name="selected">selected info</param>
      private ControlSizeInfo AddForm(XmlNode xmlForm, FormFactoryHandler formsFactory, List<DockableFormInfo> autoHide, out DockableFormInfo selected)
      {
         selected = null;

         XmlNode xmlGuid         = xmlForm.SelectSingleNode(XmlTags.TagGuid);
         XmlNode xmlParentGuid   = xmlForm.SelectSingleNode(XmlTags.TagParentGuid);
         XmlNode xmlAllowedDock  = xmlForm.SelectSingleNode(XmlTags.TagAllowedDock);
         XmlNode xmlCurrentDock  = xmlForm.SelectSingleNode(XmlTags.TagCurrentDock);
         XmlNode xmlCurrentMode  = xmlForm.SelectSingleNode(XmlTags.TagCurrentMode);
         XmlNode xmlIsSelected   = xmlForm.SelectSingleNode(XmlTags.TagIsSelected);
         XmlNode xmlIsAutoHide   = xmlForm.SelectSingleNode(XmlTags.TagIsAutoHide);
         XmlNode xmlWidth        = xmlForm.SelectSingleNode(XmlTags.TagWidth);
         XmlNode xmlHeight       = xmlForm.SelectSingleNode(XmlTags.TagHeight);

         Guid identifier            = new Guid(xmlGuid.InnerText);
         zAllowedDock allowedDock   = (zAllowedDock)Enum.Parse(typeof(zAllowedDock), xmlAllowedDock.InnerText);
         Form form                  = formsFactory(identifier);
         int width                  = Convert.ToInt32(xmlWidth.InnerText);
         int height                 = Convert.ToInt32(xmlHeight.InnerText);
         form.Width  = width;
         form.Height = height;

         DockableFormInfo info = _container.Add(form, allowedDock, identifier);

         if (Convert.ToBoolean(xmlIsSelected.InnerText))
         {
            selected = info;
         }

         if (Convert.ToBoolean(xmlIsAutoHide.InnerText))
         {
            autoHide.Add(info);
         }


         DockStyle currentDock = (DockStyle)Enum.Parse(typeof(DockStyle), xmlCurrentDock.InnerText);
         zDockMode currentMode = (zDockMode)Enum.Parse(typeof(zDockMode), xmlCurrentMode.InnerText);
         if (currentDock != DockStyle.None)
         {
            if (xmlParentGuid == null)
            {
               _container.DockForm(info, currentDock, currentMode);
            }
            else
            {
               Guid parentGuid = new Guid(xmlParentGuid.InnerText);
               DockableFormInfo parentInfo = _container.GetFormInfo(parentGuid);
               _container.DockForm(info, parentInfo, currentDock, currentMode);
            }
         }

         return new ControlSizeInfo(form, width, height);
      }

      /// <summary>
      /// Extract containers size info
      /// </summary>
      /// <param name="container">container</param>
      /// <param name="source">source</param>
      /// <param name="destination">destination</param>
      /// <returns>computed container size</returns>
      private static ControlSizeInfo ExtractContainersSizeInfo(DockableContainer container, List<ControlSizeInfo> source, List<ControlSizeInfo> destination)
      {
         DockableContainer currentContainer = container;
         if (currentContainer.SingleChild != null)
         {
            Size size = RemoveViewSizeInfo(currentContainer.SingleChild, source);
            ControlSizeInfo result = new ControlSizeInfo(container, size.Width, size.Height);
            destination.Add(result);
            return result;
         }
         
         if (currentContainer.TopPane != null)
         {
            ControlSizeInfo topSize    = ExtractContainersSizeInfo(container.TopPane, source, destination);
            ControlSizeInfo bottomSize = ExtractContainersSizeInfo(container.BottomPane, source, destination);

            Size size = new Size(Math.Max(topSize.Width, bottomSize.Width), topSize.Height + bottomSize.Height + container.TopPane.Splitter.Height);
            ControlSizeInfo result = new ControlSizeInfo(container, size.Width, size.Height);
            destination.Add(result);
            return result;
         }
         
         if (currentContainer.LeftPane != null)
         {
            ControlSizeInfo leftSize   = ExtractContainersSizeInfo(container.LeftPane, source, destination);
            ControlSizeInfo rightSize  = ExtractContainersSizeInfo(container.RightPane, source, destination);

            Size size = new Size(leftSize.Width + rightSize.Width + container.LeftPane.Splitter.Width, Math.Max(leftSize.Height, rightSize.Height));
            ControlSizeInfo result = new ControlSizeInfo(container, size.Width, size.Height);
            destination.Add(result);
            return result;
         }


         throw new NotSupportedException();
      }

      /// <summary>
      /// Move view size info
      /// </summary>
      /// <param name="view">view</param>
      /// <param name="source">source</param>
      /// <returns>view size</returns>
      private static Size RemoveViewSizeInfo(FormsTabbedView view, List<ControlSizeInfo> source)
      {
         Form form = view.GetPageAt(0);
         ControlSizeInfo firstInfo = GetInfo(form, source);

         for(int index = 0; index < view.Count; index++)
         {
            ControlSizeInfo info = GetInfo(view.GetPageAt(index), source);
            source.Remove(info);
         }

         return new Size(firstInfo.Width, firstInfo.Height);
      }

      /// <summary>
      /// Get form size info
      /// </summary>
      /// <param name="form">form </param>
      /// <param name="source">size infos</param>
      /// <returns>form size info</returns>
      private static ControlSizeInfo GetInfo(Form form, List<ControlSizeInfo> sizesInfo)
      {
         for (int index = 0; index < sizesInfo.Count; index++)
         {
            ControlSizeInfo info = sizesInfo[index];
            if (info.Control == form)
            {
               return info;
            }
         }

         return null;
      }

      /// <summary>
      /// Load forms 
      /// </summary>
      /// <param name="xmlDocument">document</param>
      /// <param name="formsFactory">forms factory</param>
      /// <returns>list of control size info</returns>
      /// <param name="autoHide">list of autohide forms</param>
      /// <param name="selected">selected info</param>
      private List<ControlSizeInfo> LoadForms(XmlDocument xmlDocument, FormFactoryHandler formsFactory, List<DockableFormInfo> autohide, out DockableFormInfo selected)
      {
         selected = null;

         List<ControlSizeInfo> formsSize = new List<ControlSizeInfo>();
         XmlNodeList xmlForms = xmlDocument.SelectNodes("//" + XmlTags.TagForms + "/" + XmlTags.TagForm);
         for (int index = 0; index < xmlForms.Count; index++)
         {
            XmlNode xmlForm = xmlForms[index];

            DockableFormInfo selectedForm = null;
            formsSize.Add(AddForm(xmlForm, formsFactory, autohide, out selectedForm));

            if (selectedForm != null)
            {
               selected = selectedForm;
            }
         }
         return formsSize;
      }

      /// <summary>
      /// Restore sizes
      /// </summary>
      /// <param name="formsSize">form sizes</param>
      private void RestoreSizes(List<ControlSizeInfo> formsSize)
      {
         while (formsSize.Count > 0)
         {
            ControlSizeInfo sizeInfo = formsSize[0];

            DockableContainer container = HierarchyUtility.GetClosestDockableContainer((Form)sizeInfo.Control);
            while (container.Parent.Handle != _container.Handle)
            {
               container = (DockableContainer)container.Parent;
            }

            List<ControlSizeInfo> containersSize = new List<ControlSizeInfo>();
            ExtractContainersSizeInfo(container, formsSize, containersSize);

            for (int index = containersSize.Count - 1; index >= 0; index--)
            {
               ControlSizeInfo info = containersSize[index];
               info.Control.Width   = info.Width;
               info.Control.Height  = info.Height;
            }
         }
      }

      #endregion Private section
   }
}
