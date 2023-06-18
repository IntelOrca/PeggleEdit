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
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crom.Controls.Docking;
using IntelOrca.PeggleEdit.Designer.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Designer
{
    [Guid("1A01E050-F11A-47C5-B62B-25678916F209")]
    partial class MainMDIForm : Form
    {
        public MainMDIForm()
        {
            InitIdentifierMap();
            InitForm();

            DefaultStartupPack();

#if !DEBUG
            CheckForNewVersion();
#endif
        }

        #region MDI Form

        private Dictionary<Guid, Type> mWindowIdentifierMap = new Dictionary<Guid, Type>();
        private DockStateSerializer mSerializer;
        private MenuToolPanel mMenuToolPanel;
        private DockContainer mDockContainer;
        private StatusStrip mStatusStrip;
        private ToolStripStatusLabel mStatusLabel;
        private ToolStripStatusLabel mLocationLabel;

        private void InitForm()
        {
            //Set the form icon
            this.Icon = Icon.FromHandle(Resources.orca_32.GetHicon());

            //Set the form title
            this.Text = String.Format("{0} {1} ({2})", Program.AppTitle, Program.AppVersion, Program.AppVersionName);

            //Set the form size
            MinimumSize = new Size(700, 500);
            ClientSize = Settings.Default.MDIFormSize;
            StartPosition = FormStartPosition.CenterScreen;

            Width = Math.Min(Width, Screen.PrimaryScreen.WorkingArea.Width - 8);
            Height = Math.Min(Height, Screen.PrimaryScreen.WorkingArea.Height - 8);

            if (Settings.Default.MDIMaximized)
                WindowState = FormWindowState.Maximized;

            //Other
            this.KeyPreview = true;

            //Initialise the controls
            InitMenu();
            InitDockContainer();
            InitStatusBar();

            //Create the panels
            Panel mTopPanel = new Panel();
            Panel mMiddlePanel = new Panel();
            Panel mBottomPanel = new Panel();

            //Add the controls to the panels
            mTopPanel.Controls.Add(mMenuToolPanel);
            mMiddlePanel.Controls.Add(mDockContainer);
            mBottomPanel.Controls.Add(mStatusStrip);

            //Set relevent heights
            mTopPanel.Height = mMenuToolPanel.RightHeight;
            mBottomPanel.Height = mStatusStrip.Height;

            //Apply docking
            mTopPanel.Location = new Point(0, 0);
            mTopPanel.Size = new Size(ClientSize.Width, mMenuToolPanel.RightHeight);
            mTopPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            mBottomPanel.Location = new Point(0, ClientSize.Height - mStatusStrip.Height);
            mBottomPanel.Size = new Size(ClientSize.Width, mStatusStrip.Height);
            mBottomPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            mMiddlePanel.Location = new Point(0, mTopPanel.Bottom);
            mMiddlePanel.Size = new Size(ClientSize.Width, mBottomPanel.Top - mTopPanel.Bottom);
            mMiddlePanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            //Add the panels to the form
            Controls.Add(mTopPanel);
            Controls.Add(mMiddlePanel);
            Controls.Add(mBottomPanel);

            //Load the docking layout
            LoadDefaultDockLayout();
            LoadLayout();
        }

        private void InitIdentifierMap()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.DefinedTypes)
            {
                if (!type.IsSubclassOf(typeof(Form)))
                    continue;

                var identifier = GetIdentifier(type);
                if (identifier != Guid.Empty)
                {
                    mWindowIdentifierMap.Add(identifier, type);
                }
            }
        }

        private void InitMenu()
        {
            mMenuToolPanel = new MenuToolPanel(this);
            mMenuToolPanel.Dock = DockStyle.Fill;
        }

        private void InitDockContainer()
        {
            mDockContainer = new DockContainer();
            mSerializer = new DockStateSerializer(mDockContainer);
            mSerializer.SavePath = Settings.GetLayoutPath();

            mDockContainer.TitleBarGradientColor1 = Color.FromArgb(225, 237, 252);
            mDockContainer.TitleBarGradientColor2 = Color.FromArgb(191, 219, 255);
            mDockContainer.TitleBarGradientSelectedColor1 = Color.FromArgb(225, 237, 255);
            mDockContainer.TitleBarGradientSelectedColor2 = Color.FromArgb(150, 200, 255);

            mDockContainer.Dock = DockStyle.Fill;

            mDockContainer.FormClosed += new EventHandler<FormEventArgs>(mDockContainer_FormClosed);
        }

        private void InitStatusBar()
        {
            mStatusStrip = new StatusStrip();

            mStatusLabel = new ToolStripStatusLabel();
            mStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            mStatusLabel.Spring = true;

            mLocationLabel = new ToolStripStatusLabel();
            mLocationLabel.Alignment = ToolStripItemAlignment.Right;

            mStatusStrip.Items.Add(mStatusLabel);
            mStatusStrip.Items.Add(mLocationLabel);

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.MDIFormSize = ClientSize;

            if (WindowState == FormWindowState.Maximized)
                Settings.Default.MDIMaximized = true;
            else
                Settings.Default.MDIMaximized = false;

            SaveLayout();
            base.OnClosing(e);
        }

        #endregion

        #region Shortcuts

        List<IShortcutContainer> mShortcutContainers = new List<IShortcutContainer>();

        public void RegisterShortcutContainer(IShortcutContainer container)
        {
            mShortcutContainers.Add(container);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            foreach (IShortcutContainer container in mShortcutContainers)
            {
                foreach (ShortcutAction sa in container.GetShortcuts())
                {
                    if (sa.Key == e.KeyCode && ((sa.Ctrl && e.Control) || (!sa.Ctrl && !e.Control)))
                    {
                        if (sa.EditorMustBeFocused && !IsLevelEditorFocused())
                            break;

                        sa.CallMethod();
                        break;
                    }
                }
            }
        }

        private bool IsLevelEditorFocused()
        {
            LevelEditor editor = GetFocusedLevelEditor();
            if (editor == null)
                return false;

            if (editor.ContainsFocus || editor.Focused)
                return true;

            return false;
        }

        #endregion

        #region Dock Container

        DockableFormInfo mPackExplorerToolWindowInfo;
        DockableFormInfo mPropertiesToolWindowInfo;
        DockableFormInfo mEntryListToolWindowInfo;
        List<DockableFormInfo> mLevelToolWindowInfos = new List<DockableFormInfo>();
        LevelToolWindow mLastFocusedLevel;

        private void LoadDefaultDockLayout()
        {
            ShowPackExplorerWindow();
            ShowPropertiesWindow();
            //ShowEntryListWindow();
        }

        private void LoadLayout()
        {
            try
            {
                var dummyForms = new List<Form>();

                if (File.Exists(mSerializer.SavePath))
                {
                    mSerializer.Load(true, guid =>
                    {
                        if (mWindowIdentifierMap.TryGetValue(guid, out var type))
                        {
                            if (type == typeof(LevelToolWindow))
                            {
                                // We can't restore level windows (might not have level pack open)
                                // Crom.Controls does not let us return null, so create a dummy form and close it
                                // at the end of the load sequence
                                var form = new Form();
                                dummyForms.Add(form);
                                return form;
                            }
                            return (Form)Activator.CreateInstance(type, this);
                        }
                        return null;
                    });
                }

                mPackExplorerToolWindowInfo = GetWindowInfo(typeof(PackExplorerToolWindow));
                mPropertiesToolWindowInfo = GetWindowInfo(typeof(PropertiesToolWindow));
                mEntryListToolWindowInfo = GetWindowInfo(typeof(EntryListToolWindow));

                foreach (var form in dummyForms)
                {
                    form.Close();
                }
            }
            catch
            {
                LoadDefaultDockLayout();
            }
        }

        private void SaveLayout()
        {
            mSerializer.Save();
        }

        private DockableFormInfo GetWindowInfo(Type formType) => mDockContainer.GetFormInfo(GetIdentifier(formType));

        private void RemoveDisposedLevelToolWindows()
        {
            mLevelToolWindowInfos.RemoveAll(new Predicate<DockableFormInfo>(IsDFIDisposed));
        }

        private bool IsDFIDisposed(DockableFormInfo dfi)
        {
            return (dfi.IsDisposed);
        }

        private void mDockContainer_FormClosed(object sender, FormEventArgs e)
        {
            RemoveDisposedLevelToolWindows();
        }

        public void RefreshDockContainer()
        {
            //TODO: Try and get dock container to refresh window texts
        }

        #endregion

        #region Windows

        public void ShowPackExplorerWindow()
        {
            if (mPackExplorerToolWindowInfo != null)
            {
                if (!mPackExplorerToolWindowInfo.IsDisposed)
                    return;
            }

            PackExplorerToolWindow window = new PackExplorerToolWindow(this);
            window.UpdateView();

            mPackExplorerToolWindowInfo = AddToolWindow(window);
            mPackExplorerToolWindowInfo.ShowContextMenuButton = false;

            if (mPropertiesToolWindowInfo != null)
            {
                if (!mPropertiesToolWindowInfo.IsDisposed)
                {
                    mDockContainer.DockForm(mPackExplorerToolWindowInfo, mPropertiesToolWindowInfo, DockStyle.Top, zDockMode.Inner);
                    return;
                }
            }

            mDockContainer.DockForm(mPackExplorerToolWindowInfo, DockStyle.Right, zDockMode.Inner);
        }

        public void ShowPropertiesWindow()
        {
            if (mPropertiesToolWindowInfo != null)
            {
                if (!mPropertiesToolWindowInfo.IsDisposed)
                    return;
            }

            PropertiesToolWindow window = new PropertiesToolWindow(this);
            mPropertiesToolWindowInfo = AddToolWindow(window);
            mPropertiesToolWindowInfo.ShowContextMenuButton = false;

            if (mPackExplorerToolWindowInfo != null)
            {
                if (!mPackExplorerToolWindowInfo.IsDisposed)
                {
                    mDockContainer.DockForm(mPropertiesToolWindowInfo, mPackExplorerToolWindowInfo, DockStyle.Bottom, zDockMode.Inner);
                    return;
                }
            }

            mDockContainer.DockForm(mPropertiesToolWindowInfo, DockStyle.Right, zDockMode.Inner);
        }

        public void ShowEntryListWindow()
        {
            if (mEntryListToolWindowInfo != null)
            {
                if (!mEntryListToolWindowInfo.IsDisposed)
                    return;
            }

            EntryListToolWindow window = new EntryListToolWindow(this);
            mEntryListToolWindowInfo = AddToolWindow(window);
            mEntryListToolWindowInfo.ShowContextMenuButton = false;

            //if (mEntryListToolWindowInfo != null) {
            //    if (!mEntryListToolWindowInfo.IsDisposed) {
            //        mDockContainer.DockForm(mEntryListToolWindowInfo, DockStyle.Left, zDockMode.Outer);
            //    }
            //}

            mDockContainer.DockForm(mEntryListToolWindowInfo, DockStyle.Left, zDockMode.Inner);
        }

        private void AddLevelWindow(Level level)
        {
            LevelToolWindow ltw = new LevelToolWindow(this, level);
            DockableFormInfo dfi = AddToolWindow(ltw);

            DockableFormInfo oldfi = mDockContainer.GetFormInfo(mLastFocusedLevel);
            if (oldfi == null)
            {
                mDockContainer.DockForm(dfi, DockStyle.Fill, zDockMode.Inner);
            }
            else
            {
                mDockContainer.DockForm(dfi, oldfi, DockStyle.Fill, zDockMode.Inner);
            }

            mLevelToolWindowInfos.Add(dfi);
            mLastFocusedLevel = ltw;
        }

        private DockableFormInfo AddToolWindow(Form form)
        {
            return mDockContainer.Add(form, zAllowedDock.All, GetIdentifier(form));
        }

        private Guid GetIdentifier(Form form) => GetIdentifier(form.GetType());

        private Guid GetIdentifier(Type formType)
        {
            var info = formType;
            var attributes = info.GetCustomAttributes(typeof(GuidAttribute), false);
            return attributes.Length > 0 ?
                new Guid(((GuidAttribute)attributes[0]).Value) :
                Guid.Empty;
        }

        public void LevelWindowHasFocus(LevelToolWindow window)
        {
            mLastFocusedLevel = window;
        }

        public bool IsEditorAvailable()
        {
            return (mLastFocusedLevel != null);
        }

        public LevelEditor GetFocusedLevelEditor()
        {
            if (mLastFocusedLevel == null)
                return null;

            return mLastFocusedLevel.LevelEditor;
        }

        public LevelToolWindow GetLevelToolWindow(Level level)
        {
            foreach (LevelToolWindow ltw in GetAllOpenLevelToolWindows())
            {
                if (ltw.Level == level)
                    return ltw;
            }

            return null;
        }

        public DockableFormInfo[] GetAllToolWindows()
        {
            List<DockableFormInfo> dfis = new List<DockableFormInfo>();
            for (int i = 0; i < mDockContainer.Count; i++)
            {
                DockableFormInfo dfi = mDockContainer.GetFormInfoAt(i);
                if (dfi.IsDisposed)
                    continue;

                dfis.Add(dfi);
            }

            return dfis.ToArray();
        }

        public LevelToolWindow[] GetAllOpenLevelToolWindows()
        {
            List<LevelToolWindow> ltws = new List<LevelToolWindow>();

            foreach (DockableFormInfo dfi in GetAllToolWindows())
            {
                LevelToolWindow ltw = dfi.DockableForm as LevelToolWindow;
                if (ltw != null)
                    ltws.Add(ltw);
            }

            return ltws.ToArray();
        }

        private void UpdatePackExplorerView()
        {
            if (mPackExplorerToolWindowInfo == null)
                return;

            if (mPackExplorerToolWindowInfo.IsDisposed)
                return;

            PackExplorerToolWindow form = mPackExplorerToolWindowInfo.DockableForm as PackExplorerToolWindow;
            form.UpdateView();
        }

        public void UpdateProperties(LevelEntry[] objects)
        {
            if (mPropertiesToolWindowInfo == null)
                return;

            if (mPropertiesToolWindowInfo.IsDisposed)
                return;

            PropertiesToolWindow form = mPropertiesToolWindowInfo.DockableForm as PropertiesToolWindow;
            form.UpdatePropertyGrid(objects);
        }

        public void UpdateEntryList(LevelEditor editor)
        {
            if (mEntryListToolWindowInfo == null)
                return;

            if (mEntryListToolWindowInfo.IsDisposed)
                return;

            EntryListToolWindow form = mEntryListToolWindowInfo.DockableForm as EntryListToolWindow;
            form.UpdateView(editor);
        }

        #endregion

        #region Environment

        public void SetEditorTool(EditorTool tool)
        {
            foreach (LevelToolWindow ltw in GetAllOpenLevelToolWindows())
            {
                EditorTool newtool = (EditorTool)tool.Clone();
                newtool.Editor = ltw.LevelEditor;
                newtool.FinishCallback = new CallbackMethod(FinishTool);

                //Deactivate current tool
                ltw.LevelEditor.SelectedTool.Deactivate();

                //Set new tool
                ltw.LevelEditor.SelectedTool = newtool;

                //Activate new tool
                newtool.Activate();
            }
        }

        private void FinishTool()
        {
            mMenuToolPanel.SelectDefaultTool();
        }

        public void SetStatus(string sz)
        {
            mStatusLabel.Text = sz;
            mStatusStrip.Refresh();
        }

        public void SetStatus(string format, params object[] args)
        {
            SetStatus(String.Format(format, args));
        }

        public void SetStatusLocation(Point location)
        {
            mLocationLabel.Text = String.Format("({0}, {1})", location.X, location.Y);
        }

        #endregion

        #region Level Pack

        LevelPack mPack;
        string mPackFilename;

        private void DefaultStartupPack()
        {
            //Check command line arguments
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                string openWithFilename = args[1];
                if (File.Exists(openWithFilename))
                {
                    OpenPack(openWithFilename);
                    return;
                }
            }


            //PeggleNightsStoryExtractor pnse = new PeggleNightsStoryExtractor(@"C:\Program Files\PopCap Games\Peggle Nights\main.pak");
            //LevelPack pack = pnse.Extract();
            //OpenPack(pack);

            NewPack();
            //OpenPack(@"C:\Program Files\PopCap Games\Peggle Nights\Test\levelpacks\peggle nights\pegglenights.pak");

            //OpenNewPackWithLevelImport(@"C:\Program Files\PopCap Games\Peggle Nights\Test\levels\bjorn2.dat");



            //LevelXMLWriter lw = new LevelXMLWriter(level);
            //File.WriteAllText(@"C:\Users\Ted\Desktop\bjorn3.xml", lw.GetXML());
        }

        public void OpenNewPackWithLevelImport(string path)
        {
            LevelReader reader = new LevelReader(path);
            Level level = reader.Read();
            reader.Dispose();

            NewPack();
            if (level != null)
            {
                LevelInfo info = LevelInfo.DefaultInfo;
                info.Name = Path.GetFileNameWithoutExtension(path);
                level.Info = info;

                mPack.Levels.Clear();
                mPack.Levels.Add(level);
                OpenLevel(level);
            }

            UpdatePackExplorerView();
        }

        public void NewPack()
        {
            mPackFilename = String.Empty;

            LevelPack pack = new LevelPack();
            pack.Levels.Add(new Level());
            OpenPack(pack);
            //OpenLevel(pack.Levels[0]);
        }

        public void OpenPack(string filename)
        {
            SetStatus("Opening '{0}'...", Path.GetFileName(filename));

            LevelPack pack = new LevelPack();
            J2K.RegisterPegglePath(Settings.Default.PeggleNightsExePath);
            if (pack.Open(filename))
            {
                mPackFilename = filename;
                SetStatus("'{0}' successfully loaded.", Path.GetFileName(filename));
            }
            else
            {
                MessageBox.Show(String.Format("'{0}' could not be opened.", filename));
                return;
            }

            UpdateRecentList(filename);
            OpenPack(pack);
        }

        public void OpenPack(LevelPack pack)
        {
            ClosePack();

            mPack = pack;
            LevelPack.Current = pack;

            UpdatePackExplorerView();
        }

        public bool SavePack(string filename)
        {
            UpdateRecentList(filename);
            return mPack.Save(filename);
        }

        public void ClosePack()
        {
            foreach (DockableFormInfo dfi in mLevelToolWindowInfos)
            {
                if (dfi.IsDisposed)
                    continue;

                mDockContainer.Remove(dfi);
            }
        }

        public void OpenLevel(Level level)
        {
            //Search to see if it's already open
            foreach (DockableFormInfo dfi in mLevelToolWindowInfos)
            {
                if (dfi.IsDisposed)
                    continue;

                LevelToolWindow window = dfi.DockableForm as LevelToolWindow;
                if (window == null)
                    continue;

                if (window.Level == level)
                {
                    dfi.IsSelected = true;
                    return;
                }
            }

            //Open it as it's not open currently
            AddLevelWindow(level);

            FinishTool();
        }

        public void CloseLevel(Level level)
        {
            //Search to see if it's open
            foreach (DockableFormInfo dfi in mLevelToolWindowInfos)
            {
                if (dfi.IsDisposed)
                    continue;

                LevelToolWindow window = dfi.DockableForm as LevelToolWindow;
                if (window == null)
                    continue;

                if (window.Level == level)
                {
                    mDockContainer.Remove(dfi);
                    return;
                }
            }
        }

        public bool ShowClosePackWarning()
        {
            DialogResult result = MessageBox.Show("Close this project without saving?", "Close Project", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            return (result == DialogResult.OK);
        }

        private void UpdateRecentList(string path)
        {
            WinAPI.AddRecentDocument(path);

            var recentPackFiles = Settings.Default.RecentPackFiles;
            var index = recentPackFiles.IndexOf(path);
            if (index != -1)
            {
                recentPackFiles.RemoveAt(index);
            }

            recentPackFiles.Insert(0, path);
            if (recentPackFiles.Count > 10)
            {
                recentPackFiles.RemoveRange(10, recentPackFiles.Count - 10);
            }

            Settings.Save();

            mMenuToolPanel.UpdateRecentPackFiles();
        }

        public LevelPack LevelPack
        {
            get
            {
                return mPack;
            }
            set
            {
                mPack = value;
            }
        }

        public string PackFilename
        {
            get
            {
                return mPackFilename;
            }
            set
            {
                mPackFilename = value;
            }
        }

        #endregion

        #region Update check

        private void CheckForNewVersion()
        {
            CheckForNewVersionAsync();
        }

        private async void CheckForNewVersionAsync()
        {
            // Update current version in settings file
            var currentVersionString = Settings.Default.CurrentVersion;
            if (!Version.TryParse(currentVersionString, out var currentVersion) || currentVersion != Program.CurrentVersion)
            {
                Settings.Default.CurrentVersion = Program.CurrentVersion.ToString();
                Settings.Default.LatestVersionAvailable = null;
                Settings.Default.HideVersionNotification = false;
                Settings.Save();
            }

            // Check GitHub for latest version if necessary
            var latestVersionString = Settings.Default.LatestVersionAvailable;
            if (!Version.TryParse(latestVersionString, out var latestVersion) || latestVersion <= Program.CurrentVersion)
            {
                latestVersion = await GetLatestVersionAsync();
                Settings.Default.LatestVersionAvailable = latestVersion.ToString();
                Settings.Save();
            }

            if (latestVersion > Program.CurrentVersion)
            {
                if (!Settings.Default.HideVersionNotification)
                {
                    NewUpdateForm.Show(this);
                }
            }
        }

        private async Task<Version> GetLatestVersionAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Program.AppTitle, Program.AppVersion.ToString()));
            var response = await client.GetAsync("https://api.github.com/repos/IntelOrca/PeggleEdit/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var body = JsonSerializer.Deserialize<VersionCheckBody>(jsonResponse);
                var tagName = body.tag_name;
                return Version.Parse(tagName.Substring(1));
            }
            throw new Exception("Unable to get latest version");
        }

        #endregion
    }

    public class VersionCheckBody
    {
        public string tag_name { get; set; }
    }
}
