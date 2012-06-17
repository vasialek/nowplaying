using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Av.Utils;
using System.Text;

namespace NowPlaying
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// Systray icon
        /// </summary>
        protected NotifyIcon _icon = null;

        /// <summary>
        /// Song downloader and parser thread (for all radio stations)
        /// </summary>
        private PageDownloaderThread _radioThread = null;

        #region " Get/set text methods (invoked) "

        delegate void SetTextDelegate(string name, string value);
        delegate string GetTextDelegate(string name);

        /// <summary>
        /// Returns text of control.
        /// </summary>
        /// <param name="name">Name of control</param>
        /// <returns>Text of control. Returns "checked"/"" or "" for CheckBox</returns>
        private string GetText(string name)
        {
            string s = "";

            try
            {
                if(this.InvokeRequired)
                {
                    this.Invoke(new GetTextDelegate(GetText), name);
                } else
                {
                    Control[] ar = this.Controls.Find(name, false);
                    if((ar != null) && (ar.Length > 0))
                    {
                        if(ar[0].GetType() == typeof(CheckBox))
                        {
                            s = ((CheckBox)ar[0]).Checked ? "checked" : "";
                        } else
                        {
                            s = ar[0].Text;
                        }
                    }
                }
            } catch(Exception ex)
            {
            }

            return s;
        }

        /// <summary>
        /// Thread safe AddText
        /// </summary>
        /// <param name="name">Name of control to add text</param>
        /// <param name="value">Text to add</param>
        private void AddText(string name, string value)
        {
            try
            {
                if(this.InvokeRequired)
                {
                    this.Invoke(new SetTextDelegate(AddText), name, value);
                } else
                {
                    Control[] ar = this.Controls.Find(name, false);
                    if((ar != null) && (ar.Length > 0))
                    {
                        ar[0].Text += value;
                        if(ar[0].GetType() == typeof(TextBox))
                        {
                            ((TextBox)ar[0]).SelectionStart = Int32.MaxValue;
                        }
                    }
                }
            } catch(Exception)
            {
            }
        }

        /// <summary>
        /// Thread safe SetText
        /// </summary>
        /// <param name="name">Name of control to set text</param>
        /// <param name="value">Text to set</param>
        private void SetText(string name, string value)
        {
            try
            {
                if(this.InvokeRequired)
                {
                    this.Invoke(new SetTextDelegate(SetText), name, value);
                } else
                {
                    Control[] ar = this.Controls.Find(name, false);
                    if((ar != null) && (ar.Length > 0))
                    {
                        ar[0].Text = value;
                    }
                }

            } catch(Exception ex)
            {
            }
        }

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //base.OnFormClosing(e);

                // Hide on close
                if(e.CloseReason == CloseReason.UserClosing)
                {
                    // TODO: Hide debug console instead of closing application
                    //HideApplication();
                    //e.Cancel = true;
                }

            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error OnFormClosing!");
                Log4cs.Log(Importance.Debug, ex.ToString());
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Log4cs.Log(Importance.Debug, "Firing Quit event to stop radio thread!");
            AEvents.arEvents[(int)AEvents.EventsId.Quit].Set();
            //if( _radioThread != null )
            //{
            //    _radioThread.Stop();
            //}

            if(_icon != null)
            {
                _icon.Dispose();
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            try
            {
                Log4cs.Log("Starting {0} (v{1})...", Settings.Name, Settings.Version);
                Debug("Starting {0}...", Settings.NameVersion);

                // In case we need only one instance of program
                EnsureSingleApplication();

                this.Text = Settings.NameVersion;

                // Load configuration
                Settings.Load();

                if((Settings.RadioStations == null) || (Settings.RadioStations.Length < 1))
                {
                    Log4cs.Log(Importance.Error, "Radio stations are not configured properly - no stations in XML!");
                    MessageBox.Show("Radio stations are not configured properly, check stations.xml file!");
                    throw new Exception("Bad stations.xml configuration!");
                } else
                {
                    Debug("Got {0} configured stations to watch", Settings.RadioStations.Length);
                }

                // Creates icons using emebedded icon
                CreateFormIcons();

                // Create context menu for tray icon
                MenuItem[] arMenu = this.CreateMenuItems();
                if(arMenu != null)
                {
                    _icon.ContextMenu = new ContextMenu(arMenu);
                    _icon.DoubleClick += new EventHandler(OnContextMenuDoubleClicked);
                }

                // Hides application (form) if necessary
                //HideApplication();

            } catch(ApplicationException ex)
            {
                Debug(Importance.Error, "Application already run, check logs!");
                MessageBox.Show(ex.ToString(), string.Format("{0} (v{1})", Settings.Name, Settings.Version), MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error loading main form!");
                Log4cs.Log(Importance.Debug, ex.ToString());
                MessageBox.Show("Error loading application!", string.Format("{0} (v{1})", Settings.Name, Settings.Version), MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        void OnContextMenuDoubleClicked(object sender, EventArgs e)
        {
            Log4cs.Log(Importance.Debug, "Context menu icon doubleclicked...");
            if(this.Visible)
            {
                HideApplication();
            } else
            {
                this.Visible = true;
            }
        }

        /// <summary>
        /// Hides application (form) and from taskbar
        /// </summary>
        protected void HideApplication()
        {
            this.ShowInTaskbar = false;
            this.Visible = false;
        }

        /// <summary>
        /// Hides from Alt-TAB
        /// </summary>
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;

        //        // Turns on WS_EX_TOOLWINDOW style bit to hide from Alt-TAB list
        //        cp.ExStyle |= 0x80;

        //        return cp;
        //    }
        //}

        /// <summary>
        /// Creates tray icon from embedded to project icon. Throws execptions
        /// </summary>
        private void CreateFormIcons()
        {
            // Create tray icon
            Assembly asm = Assembly.GetExecutingAssembly();
            FileInfo fi = new FileInfo(asm.GetName().Name);
            using(Stream s = asm.GetManifestResourceStream(string.Format("{0}.av1.ico", fi.Name)))
            {
                // Create icon to be used in Form and Tray
                Icon icon = new Icon(s);

                Icon = new Icon(icon, icon.Size);

                _icon = new NotifyIcon();
                _icon.Visible = true;
                _icon.Icon = new Icon(icon, icon.Size);

                icon.Dispose();
            }
        }

        /// <summary>
        /// Ensures that application is the only. Throws ApplicationException if there is such application
        /// </summary>
        private void EnsureSingleApplication()
        {
            bool createdNew = false;
            Mutex mx = new Mutex(false, Settings.Name, out createdNew);
            Log4cs.Log(Importance.Debug, "Is mutex created: {0}", createdNew);

            // If application is already running
            if(createdNew == false)
            {
                throw new ApplicationException(Settings.Name + " application is already running!");
            }
        }


        #region " Debug to UI console "

        /// <summary>
        /// Outputs message to UI output console
        /// </summary>
        /// <param name="msg">Message to input, could be used like string.Format()</param>
        /// <param name="args"></param>
        protected void Debug(string msg, params object[] args)
        {
            Debug(Importance.Info, msg, args);
        }

        /// <summary>
        /// Outputs message to UI output console
        /// </summary>
        /// <param name="level">Level of imortance - Error, Info, etc...</param>
        /// <param name="msg">Message to input, could be used like string.Format()</param>
        /// <param name="args"></param>
        protected void Debug(Importance level, string msg, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            if(level != Importance.No)
            {
                sb.AppendFormat("[{0}] ", level.ToString().ToUpper());
            }

            sb.AppendFormat(msg, args);
            sb.AppendLine();
            this.AddText("txtOutput", sb.ToString());
            txtOutput.SelectionStart = int.MaxValue;
            txtOutput.ScrollToCaret();
        }

        #endregion


        #region " Context menu methods "

        /// <summary>
        /// Creates context menu for tray icon
        /// </summary>
        /// <returns></returns>
        private MenuItem[] CreateMenuItems()
        {
            MenuItem[] arMenu = null;

            try
            {
                // Check if we have some radio stations
                int radios = Settings.RadioStations == null ? 0 : Settings.RadioStations.Length;

                // Got quantity of menus (+ radios if any + title)
                arMenu = new MenuItem[MyMenu.Size + radios + 1];

                for(int i = 0; i < MyMenu.Size; i++)
                {
                    arMenu[i] = new MenuItem(MyMenu.ToName(i), OnContextMenuClicked);
                }

                arMenu[MyMenu.Size] = new MenuItem("Radio stations");
                arMenu[MyMenu.Size].Enabled = false;
                // Add radio stations to menu, if any
                for(int i = 0; i < radios; i++)
                {
                    arMenu[MyMenu.Size + i + 1] = new MenuItem(Settings.RadioStations[i].RadioName, OnRadioStationClicked);
                    arMenu[MyMenu.Size + i + 1].Name = Settings.RadioStations[i].RadioName;
                }

                // By default status thread is stopped, so disable "Stop" command
                arMenu[MyMenu.Position.Stop].Enabled = false;

                // Format "Version"
                arMenu[MyMenu.Position.Version].Text = string.Format("{0} (v{1})", Settings.Name, Settings.Version);

            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error creating menu items!");
                Log4cs.Log(Importance.Debug, ex.ToString());
            }

            return arMenu;
        }

        /// <summary>
        /// Handles clicks on systray icon menu
        /// </summary>
        void OnContextMenuClicked(object sender, EventArgs e)
        {
            try
            {
                switch(((MenuItem)sender).Index)
                {
                    case MyMenu.Position.Start:
                        _icon.ContextMenu.MenuItems[MyMenu.Position.Start].Enabled = false;
                        _icon.ContextMenu.MenuItems[MyMenu.Position.Stop].Enabled = true;
                        OnStartStopClicked(null, null);
                        break;
                    case MyMenu.Position.Stop:
                        _icon.ContextMenu.MenuItems[MyMenu.Position.Start].Enabled = true;
                        _icon.ContextMenu.MenuItems[MyMenu.Position.Stop].Enabled = false;
                        OnStartStopClicked(null, null);
                        break;
                    case MyMenu.Position.Version:
                        MessageBox.Show("iPlayNow by mr. Aleksej Vasinov", Settings.NameVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case MyMenu.Position.Reload:
                        ReloadSettings();
                        break;
                    default:
                        this.Close();
                        break;
                }

            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error handling context menu click!");
                Log4cs.Log(Importance.Debug, ex.ToString());
            }
        }

        private void OnRadioStationClicked(object sender, EventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            //Debug("Radio station #{0} is clicked...", menu.Name);
            Song[] songs = _playlist.GetListOfLast(menu.Name, 5);
            if( songs != null )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Last songs at {0}:", menu.Name).AppendLine();
                foreach(Song s in songs)
                {
                    sb.AppendFormat("  At {0} song: {1}", s.PlayedAt.ToString("HH:mm:ss"), s.FullTitle).AppendLine();
                }

                Debug(sb.ToString());
                MessageBox.Show(sb.ToString());
            }
        }


        /// <summary>
        /// Reloads setting. Stops thread if necessary
        /// </summary>
        private void ReloadSettings()
        {
            Log4cs.Log("Reloading settings...");
            Settings.Load();

            // Need to reload radio station thread
            if( (_radioThread != null) && _radioThread.IsRunning )
            {
                _radioThread.Stop();
                _radioThread.Start(Settings.RadioStations);
            }
        }

        /// <summary>
        /// Starts/stops application if needed :)
        /// </summary>
        private void OnStartStopClicked(object sender, object e)
        {
            Log4cs.Log("Start/stop is clicked...");
            if( _radioThread == null )
            {
                _radioThread = new PageDownloaderThread();
                _radioThread.SongParsed += new SongParsedDelegate(OnSongParsed);
            }

            // Stop thread if running
            if(_radioThread.IsRunning)
            {
                _radioThread.Stop();
            } else
            {
                // Need to run thread
                _radioThread.Start(Settings.RadioStations);
            }
        }

        private Playlist _playlist = new Playlist();
        void OnSongParsed(RadioStationItem radio, Song song)
        {
            // Playlist returns true if need to display song
            if(_playlist.Add(song))
            {
                Debug("New song on radio {0}: {1}-{2}", radio.RadioName, song.Artist, song.Title);
            }
        }

        #endregion

    }  // END CLASS Form1

}
