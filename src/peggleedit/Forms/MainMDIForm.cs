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
using System.Threading;
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
        public static MainMDIForm Instance { get; private set; }

        public MainMDIForm()
        {
            Instance = this;

            PlatformServices.Current.CentreSystemDialogs();
            InitIdentifierMap();
            InitForm();
            InitChangesTimer();

            DefaultStartupPack();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Program.JustUpdated)
                SetStatus("Updated to {0} {1}", Program.AppTitle, Program.AppVersionLabel);
            CheckForUpdates();
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
            UpdateTitle();

            //Set the form size (saved sizes are scaled if they were saved at another DPI)
            float sizesScale = Settings.Default.SizesScale > 0 ? Dpi.Factor / Settings.Default.SizesScale : 1;
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            var minimumSize = Dpi.Scale(new Size(700, 500));
            MinimumSize = new Size(Math.Min(minimumSize.Width, workingArea.Width - 8), Math.Min(minimumSize.Height, workingArea.Height - 8));
            ClientSize = new Size(
                (int)Math.Round(Settings.Default.MDIFormSize.Width * sizesScale),
                (int)Math.Round(Settings.Default.MDIFormSize.Height * sizesScale));
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
            // Sizes are saved in 96 DPI units, so that versions that are not DPI aware read them as they are
            mSerializer.LoadScale = Dpi.Factor / (Settings.Default.SizesScale > 0 ? Settings.Default.SizesScale : 1);
            mSerializer.SaveScale = Dpi.Factor;

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
            // Mono sizes an empty status strip to only a few pixels high
            mStatusStrip.MinimumSize = new Size(0, Dpi.Scale(22));

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
            if (!ConfirmClosePack())
            {
                e.Cancel = true;
                // An update that was to be installed now is installed when the editor is closed
                if (Updater.PendingInstall != null)
                    SetStatus("The update will be installed when {0} is closed.", Program.AppTitle);
                return;
            }

            // Saved in 96 DPI units (see InitDockContainer)
            Settings.Default.MDIFormSize = new Size(
                (int)Math.Round(ClientSize.Width / Dpi.Factor),
                (int)Math.Round(ClientSize.Height / Dpi.Factor));
            Settings.Default.SizesScale = 1;

            if (WindowState == FormWindowState.Maximized)
                Settings.Default.MDIMaximized = true;
            else
                Settings.Default.MDIMaximized = false;

            SaveLayout();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            mChangesTimer.Dispose();
            Application.RemoveMessageFilter(mInputFilter);
            base.OnClosed(e);
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
                    // Not with Alt, so that characters typed with AltGr (Ctrl + Alt) are not taken as Ctrl shortcuts
                    if (sa.Key == e.KeyCode && ((sa.Ctrl && e.Control && !e.Alt) || (!sa.Ctrl && !e.Control)))
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
            try
            {
                mSerializer.Save();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // Failing to write the layout file should not prevent the application from closing
                Debug.WriteLine(ex);
            }
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
            if (e.Form is LevelToolWindow window)
                LevelWindowClosed(window);
        }

        /// <summary>
        /// Once the level being edited is closed (or deleted), clears the entry list and properties, which show it, and
        /// stops the ribbon's commands acting on it. They are filled again by the next level edited. Closing another
        /// level leaves them as they are.
        /// </summary>
        private void LevelWindowClosed(LevelToolWindow window)
        {
            if (window != mLastFocusedLevel)
                return;

            mLastFocusedLevel = null;
            UpdateEntryList(null);
            UpdateProperties(new LevelEntry[0]);
            SetStatus("");
            mMenuToolPanel.UpdateViewPanel();
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
            mMenuToolPanel.UpdateViewPanel();

            mEditedLevels.Add(level);
            ltw.UpdateTitle(mPack.IsLevelModified(level));
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
            mMenuToolPanel.UpdateViewPanel();
        }

        /// <summary>
        /// Shows how a level is viewed (e.g. its zoom, or the kinds of object shown) on the ribbon, if it is the level
        /// being worked on.
        /// </summary>
        public void LevelZoomChanged(LevelEditor editor)
        {
            if (editor == GetFocusedLevelEditor())
                mMenuToolPanel.UpdateViewPanel();
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
            mMenuToolPanel.RefreshSelectedTool(tool);
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
                // A new pack is opened instead if it can not be
                if (File.Exists(openWithFilename) && Open(openWithFilename))
                    return;
            }

            NewPack();
            //OpenPack(@"C:\Program Files\PopCap Games\Peggle Nights\Test\levelpacks\peggle nights\pegglenights.pak");

            //OpenNewPackWithLevelImport(@"C:\Program Files\PopCap Games\Peggle Nights\Test\levels\bjorn2.dat");

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
                // Nothing to save, as the level is in its file
                mPack.MarkSaved();
                OpenLevel(level);
            }

            UpdatePackExplorerView();
            UpdateChanges();
        }

        // The game whose levels are open (from its main.pak), whose files are used rather than those of the game
        // in the settings, until another pack is opened
        private static (string Directory, string MainPak)? s_openedGame;

        /// <summary>
        /// Points the game file lookups at the game whose levels are open, or else the game directory in the settings.
        /// </summary>
        public static void RegisterGamePaths()
        {
            if (s_openedGame.HasValue)
                LevelPack.RegisterPegglePath(s_openedGame.Value.Directory, s_openedGame.Value.MainPak);
            else
                LevelPack.RegisterPegglePath(Settings.Default.PeggleNightsPath);
        }

        /// <summary>
        /// Points the game file lookups at the game directory in the settings again, for a pack that is not a game's.
        /// </summary>
        public static void UseSettingsGamePaths()
        {
            s_openedGame = null;
            RegisterGamePaths();
        }

        public void NewPack()
        {
            UseSettingsGamePaths();

            mPackFilename = String.Empty;

            LevelPack pack = new LevelPack();
            pack.Levels.Add(new Level());
            OpenPack(pack);
            //OpenLevel(pack.Levels[0]);
        }

        /// <summary>
        /// Opens a level pack, or a game's own levels from its main.pak (or another .pak of the game's, or its
        /// levels\stages.cfg), whichever the file is.
        /// </summary>
        /// <returns>Whether it was opened.</returns>
        public bool Open(string path)
        {
            return IsGameLevels(path) ? OpenGameLevels(path) : OpenPack(path);
        }

        /// <summary>
        /// Whether a file has a game's own levels, rather than being a level pack: a game's .pak, which has its
        /// levels\stages.cfg, or the stages.cfg of an extracted game.
        /// </summary>
        public static bool IsGameLevels(string path)
        {
            if (Path.GetFileName(path).Equals("stages.cfg", StringComparison.OrdinalIgnoreCase))
                return true;
            return path.EndsWith(".pak", StringComparison.OrdinalIgnoreCase) && LevelPack.IsGamePak(path);
        }

        /// <returns>Whether the pack was opened.</returns>
        public bool OpenPack(string filename)
        {
            SetStatus("Opening '{0}'...", Path.GetFileName(filename));

            LevelPack pack = new LevelPack();
            var previousGame = s_openedGame;
            UseSettingsGamePaths();
            if (pack.Open(filename))
            {
                mPackFilename = filename;
                SetStatus("'{0}' successfully loaded.", Path.GetFileName(filename));
            }
            else
            {
                // The pack that is still open keeps its game
                s_openedGame = previousGame;
                RegisterGamePaths();
                MessageBox.Show(DialogOwner, String.Format("'{0}' could not be opened.", filename), "Open",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            UpdateRecentList(filename);
            OpenPack(pack);
            return true;
        }

        /// <summary>
        /// Opens the levels a game comes with as a new, untitled pack (so that saving it makes a level pack, rather
        /// than changing the game's files), and points the game file lookups at that game.
        /// </summary>
        /// <param name="path">The game's directory, main.pak or levels\stages.cfg.</param>
        /// <returns>Whether the levels were opened.</returns>
        public bool OpenGameLevels(string path)
        {
            var gameDirectory = LevelPack.GetGameDirectory(path);
            SetStatus("Opening the levels of '{0}'...", Path.GetFileName(gameDirectory));

            var pack = new LevelPack();
            var previousGame = s_openedGame;
            IReadOnlyList<string> problems;
            try
            {
                // The game's own .pak, which may have been renamed, or else its main.pak
                var mainPak = path.EndsWith(".pak", StringComparison.OrdinalIgnoreCase) ? Path.GetFullPath(path) : null;
                s_openedGame = (gameDirectory, mainPak);
                RegisterGamePaths();
                problems = pack.OpenGame(path);
            }
            catch (Exception ex)
            {
                // The pack that is still open keeps its game
                s_openedGame = previousGame;
                RegisterGamePaths();
                SetStatus("");
                MessageBox.Show(DialogOwner, ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            mPackFilename = String.Empty;
            UpdateRecentList(path);
            OpenPack(pack);
            SetStatus("{0} levels of '{1}' loaded.", pack.Levels.Count, pack.Name);
            if (problems.Count > 0)
            {
                MessageBox.Show(DialogOwner, String.Join(Environment.NewLine, problems), "Open",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return true;
        }

        /// <summary>
        /// Opens a level pack that has been exported to a folder, as a pack with no file yet, so that saving it asks
        /// for one.
        /// </summary>
        /// <returns>Whether it was opened.</returns>
        public bool ImportPack(string folder)
        {
            var pack = new LevelPack();
            var previousGame = s_openedGame;
            UseSettingsGamePaths();
            if (!pack.Import(folder))
            {
                // The pack that is still open keeps its game
                s_openedGame = previousGame;
                RegisterGamePaths();
                return false;
            }

            mPackFilename = String.Empty;
            OpenPack(pack);
            return true;
        }

        public void OpenPack(LevelPack pack)
        {
            ClosePack();

            mPack = pack;
            LevelPack.Current = pack;
            // As it was opened, so that only changes from now on are asked to be saved
            pack.MarkSaved();
            mEditedLevels.Clear();

            UpdatePackExplorerView();
            UpdateChanges();
        }

        /// <summary>
        /// The window that owns a message box: this one, once it is shown (and not while the pack given on the
        /// command line is opened, when a message box it owned would have no button on the taskbar).
        /// </summary>
        private IWin32Window DialogOwner => Visible ? this : null;

        /// <summary>
        /// Whether a file is one of a game's own .pak files, which a level pack is not saved over.
        /// </summary>
        private static bool IsGamePakName(string filename) =>
            Path.GetFileName(filename).Equals("main.pak", StringComparison.OrdinalIgnoreCase) || LevelPack.IsGamePak(filename);

        public bool SavePack(string filename)
        {
            // The game's own levels, opened from the game's .pak, are saved as a level pack, not over the game's
            if (IsGamePakName(filename))
            {
                MessageBox.Show(this, "A level pack can not be saved as main.pak or over another of the game's own .pak files. Choose another name.",
                    "Save Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            UpdateRecentList(filename);
            if (!mPack.Save(filename))
                return false;

            mPack.MarkSaved();
            UpdateChanges();
            return true;
        }

        /// <summary>
        /// Saves the pack to its file, or asks for one if it has none.
        /// </summary>
        /// <returns>Whether the pack was saved.</returns>
        public bool Save()
        {
            // A pack whose file is named as a game's .pak is saved as another
            if (String.IsNullOrEmpty(mPackFilename) || IsGamePakName(mPackFilename))
                return SaveAs();
            return SavePack(mPackFilename);
        }

        /// <summary>
        /// Asks for a file to save the pack to, and saves it there.
        /// </summary>
        /// <returns>Whether the pack was saved.</returns>
        public bool SaveAs()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Peggle Level Packs (*.pak)|*.pak";
                if (dialog.ShowDialog(this) != DialogResult.OK || !SavePack(dialog.FileName))
                    return false;

                mPackFilename = dialog.FileName;
                return true;
            }
        }

        public void ClosePack()
        {
            // A copy, as removing a window can raise FormClosed, which removes it from the list
            foreach (DockableFormInfo dfi in mLevelToolWindowInfos.ToArray())
            {
                if (dfi.IsDisposed)
                    continue;

                // Taken before the window is removed, which disposes its info
                var window = dfi.DockableForm as LevelToolWindow;
                mDockContainer.Remove(dfi);
                if (window != null)
                    LevelWindowClosed(window);
            }
            RemoveDisposedLevelToolWindows();
            if (mLastFocusedLevel != null)
                LevelWindowClosed(mLastFocusedLevel);
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
                    // Removing the window does not close it, so the level's views are cleared here
                    mDockContainer.Remove(dfi);
                    LevelWindowClosed(window);
                    return;
                }
            }
        }

        /// <summary>
        /// Asks whether to save the pack before it is closed, if it has changed since it was opened or saved.
        /// </summary>
        /// <returns>Whether to go on and close the pack: it has not changed, it has been saved, or the changes are
        /// not wanted.</returns>
        public bool ConfirmClosePack()
        {
            if (mPack == null || !mPack.IsModified())
                return true;

            var changedLevels = mPack.GetModifiedLevels().Select(x => x.Info.Name).ToArray();
            switch (SaveChangesForm.Show(this, mPack.Name, changedLevels))
            {
                case DialogResult.Yes:
                    return Save();
                case DialogResult.No:
                    return true;
                default:
                    return false;
            }
        }

        #region Changes

        // The levels opened in an editor since the pack was opened, the only ones whose objects can have changed
        private readonly HashSet<Level> mEditedLevels = new HashSet<Level>();
        private System.Windows.Forms.Timer mChangesTimer;

        private void InitChangesTimer()
        {
            // Changes are found by comparing the pack with how it was saved, which sees every change and undo
            // however it was made. Only the user changes the pack, so it is compared again only after a key or
            // button, and not at all while the preview plays on its own.
            Application.AddMessageFilter(mInputFilter);
            mChangesTimer = new System.Windows.Forms.Timer { Interval = 500 };
            mChangesTimer.Tick += (s, e) =>
            {
                if (mInputFilter.TakeInput())
                    UpdateChanges();
            };
            mChangesTimer.Start();
        }

        private readonly InputFilter mInputFilter = new InputFilter();

        /// <summary>
        /// Notes whether a key or mouse button has been used (anywhere in the application) since it was last asked.
        /// </summary>
        private sealed class InputFilter : IMessageFilter
        {
            private const int WM_KEYFIRST = 0x0100;
            private const int WM_KEYLAST = 0x0109;
            private const int WM_LBUTTONDOWN = 0x0201;
            private const int WM_MOUSEWHEEL = 0x020A;
            private const int WM_MOUSEHWHEEL = 0x020E;

            private bool mInput = true;

            public void NoteInput() => mInput = true;

            public bool TakeInput()
            {
                var input = mInput;
                mInput = false;
                return input;
            }

            public bool PreFilterMessage(ref Message m)
            {
                // Keys, and mouse buttons (down, up or double clicked) and wheels, but not the mouse moving
                if ((m.Msg >= WM_KEYFIRST && m.Msg <= WM_KEYLAST) || (m.Msg >= WM_LBUTTONDOWN && m.Msg <= WM_MOUSEHWHEEL))
                    mInput = true;
                return false;
            }
        }

        /// <summary>
        /// Shows whether each open level, and the pack, has changed since the pack was opened or saved, with an *
        /// after its name.
        /// </summary>
        public void UpdateChanges()
        {
            if (mPack == null)
                return;

            foreach (var window in GetAllOpenLevelToolWindows())
                window.UpdateTitle(mPack.IsLevelModified(window.Level));
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var title = String.Format("{0} {1}", Program.AppTitle, Program.AppVersionLabel);
            if (mPack != null)
                title = String.Format("{0}{1} - {2}", mPack.Name, mPack.IsModified(mEditedLevels) ? "*" : "", title);
            if (Text != title)
                Text = title;
        }

        #endregion

        private void UpdateRecentList(string path)
        {
            PlatformServices.Current.AddRecentDocument(path);

            RecentFiles.Add(Settings.Default.RecentPackFiles, path);
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

        private bool mCheckingForUpdates;
        private bool mRecheckForUpdates;
        private UpdateInfo mPendingUpdate;
        private UpdateRing mPendingUpdateRing;

        /// <summary>
        /// Checks the selected update ring for a newer build and offers to install it. Failures (offline,
        /// rate limited, bad feed) are only reported in the status bar.
        /// </summary>
        public async void CheckForUpdates()
        {
            if (!BuildInfo.IsPublished)
                return;
            if (mCheckingForUpdates)
            {
                // The ring may have changed since the running check started.
                mRecheckForUpdates = true;
                return;
            }

            mCheckingForUpdates = true;
            try
            {
                var ring = Settings.Default.EffectiveUpdateRing;
                UpdateInfo update;
                try
                {
                    update = await Updater.CheckAsync(ring, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    if (!IsDisposed)
                    {
                        var message = ex is OperationCanceledException ? "timed out" : ex.Message;
                        SetStatus("Unable to check for updates: {0}", message);
                    }
                    return;
                }

                if (update == null || update.Key == Settings.Default.SkippedUpdate || IsDisposed)
                    return;
                if (ring != Settings.Default.EffectiveUpdateRing)
                    return;

                mPendingUpdate = update;
                mPendingUpdateRing = ring;
                OfferPendingUpdate();
            }
            finally
            {
                mCheckingForUpdates = false;
                if (mRecheckForUpdates && !IsDisposed)
                {
                    mRecheckForUpdates = false;
                    CheckForUpdates();
                }
            }
        }

        /// <summary>
        /// Shows the update prompt, unless another dialog is open, in which case it is shown when the
        /// main window is next activated.
        /// </summary>
        private void OfferPendingUpdate()
        {
            if (mPendingUpdate == null || IsDisposed || !PlatformServices.Current.AcceptsInput(this))
                return;

            var update = mPendingUpdate;
            mPendingUpdate = null;
            if (mPendingUpdateRing != Settings.Default.EffectiveUpdateRing)
                return;
            if (NewUpdateForm.Show(this, update, mPendingUpdateRing))
            {
                Close();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            // A message box or file dialog, after which the editor is active again, runs its own message loop, so the
            // click that made a change in it is not seen as input
            mInputFilter.NoteInput();
            if (mPendingUpdate != null)
                BeginInvoke((Action)OfferPendingUpdate);
        }

        #endregion
    }
}
