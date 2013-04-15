// http://html5.grooveshark.com/

using System;
using System.Windows;
using Awesomium.Core;
using System.Diagnostics;

using System.Windows.Input;

using OpenShark;

namespace AweShark {
    public partial class MainWindow : Window {

        public const string project = "OpenShark 2.0";

        private string fn;
        private OpenShark.ApiHook.IniFile ini;

        private void ShowHide() {
            if (this.WindowState == WindowState.Minimized) this.Show();
            else {
                this.WindowState = WindowState.Minimized;
                if (OpenShark.Properties.trayMinimize) this.Hide();
                }
            }

        private bool IsVisibleOnAnyScreen(System.Drawing.Rectangle rect) {
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens) {
                if (screen.WorkingArea.IntersectsWith(rect)) {
                    return true;
                    }
                }
            return false;
            }

        private void htmlClick(string action) {
            if (this.webControl.IsDocumentReady) {
                this.webControl.ExecuteJavascript(
                    String.Format(
                        @"$(""#GroovePlayer"").contents().find(""a #{0}"").click();", action));
                }
            }

        private void js(string selector) {
            if (this.webControl.IsDocumentReady) {
                this.webControl.ExecuteJavascript(selector);
                }
            }

        void hook_KeyPressed(object sender, Tuple<OpenShark.ApiHook.KeyboardHook, OpenShark.Hotkeys.KeyPressedEventArgs> et) {
            var e = et.Item2;
            switch (e.Key.ToString()) {
                case "MediaPlayPause":
                htmlClick("play-pause");
                break;
                case "MediaNextTrack":
                htmlClick("play-next");
                break;
                case "MediaPreviousTrack":
                htmlClick("play-prev");
                break;
                }

            uint KeyAsInt = (uint)(e.Key | OpenShark.Core.hook.keyToModifierKey(e.Modifier));
            if (KeyAsInt == OpenShark.Properties.hotkeyPlay) {
                htmlClick("play-pause");
                }
            else if (KeyAsInt == OpenShark.Properties.hotkeyNext) {
                htmlClick("play-next");
                }
            else if (KeyAsInt == OpenShark.Properties.hotkeyPrevious) {
                htmlClick("play-prev");
                }
            else if (KeyAsInt == OpenShark.Properties.hotkeyLike) {
                htmlClick("player-wrapper .queue-item-active .smile");
                }
            else if (KeyAsInt == OpenShark.Properties.hotkeyDislike) {
                htmlClick("#player-wrapper .queue-item-active .frown");
                }
            else if (KeyAsInt == OpenShark.Properties.hotkeyFavorite) {
                htmlClick("np-fav");
                }
            else if (KeyAsInt == OpenShark.Properties.hotkeyMute) {
                htmlClick("volume");
                }
            else if (KeyAsInt == OpenShark.Properties.hotkeyShowHide) {
                ShowHide();
                }
            else if (KeyAsInt == OpenShark.Properties.hotkeyShuffle) {
                htmlClick("shuffle");
                }
            }

        public MainWindow() {

            bool ok;
            object m = new System.Threading.Mutex(true, "OpenShark", out ok);
            if (!ok) {
                MessageBox.Show("OpenShark is already running.");
                return;
                }
            GC.KeepAlive(m);

            fn = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\OpenShark.ini";
            ini = new OpenShark.ApiHook.IniFile(fn);
            if (System.IO.File.Exists(fn)) OpenShark.Properties.Load(ini);
            else OpenShark.Properties.Save(ini);

            string[] ops = { 
                           "--allow-file-access-from-files",
                           "--allow-file-access",
                           "--allow-http-background-page",
                           "--disable-web-security" 
                           };
            WebCore.Initialize(new WebConfig() {
                HomeURL = new Uri("asset://local/web/index.html"),
                LogLevel = LogLevel.Verbose,
                AdditionalOptions = ops
            });
            InitializeComponent();

            /// Tray icon Load ____________________________________________________________
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            System.Windows.Forms.ContextMenuStrip cmi = new System.Windows.Forms.ContextMenuStrip();

            var current = new System.Windows.Forms.ToolStripMenuItem("Current: "); current.Enabled = false;
            var about = new System.Windows.Forms.ToolStripMenuItem(project); about.Enabled = false;
            var emo = new System.Windows.Forms.ToolStripMenuItem("Emotions:"); emo.Enabled = false;
            var options = new System.Windows.Forms.ToolStripMenuItem("Options");

            options.Click += ((obj, send) => {
                System.Windows.MessageBox.Show("Not Supported Yet");
            });

            var exit = new System.Windows.Forms.ToolStripMenuItem("Exit");

            exit.Click += ((obj, send) => {
                this.Close();
            });

            var previous = new System.Windows.Forms.ToolStripMenuItem("Previous");

            previous.Click += ((obj, send) => {
                htmlClick("play-prev");
            });

            var next = new System.Windows.Forms.ToolStripMenuItem("Next");

            next.Click += ((obj, send) => {
                htmlClick("play-next");
            });

            var play = new System.Windows.Forms.ToolStripMenuItem("Play / Stop");

            play.Click += ((obj, send) => {
                htmlClick("play-pause");
            });

            var favorite = new System.Windows.Forms.ToolStripMenuItem("Favorite");

            favorite.Click += ((obj, send) => {
                htmlClick("np-fav");
            });

            var like = new System.Windows.Forms.ToolStripMenuItem("Like");

            like.Click += ((obj, send) => {
                htmlClick("player-wrapper .queue-item-active .smile");
            });

            var dislike = new System.Windows.Forms.ToolStripMenuItem("Dislike");

            dislike.Click += ((obj, send) => {
                htmlClick("#player-wrapper .queue-item-active .frown");
            });

            cmi.Items.Add(current);
            cmi.Items.Add(previous);
            cmi.Items.Add(next);
            cmi.Items.Add(play);

            cmi.Items.Add(emo);
            cmi.Items.Add(favorite);
            cmi.Items.Add(like);
            cmi.Items.Add(dislike);

            cmi.Items.Add(about);
            cmi.Items.Add(options);
            cmi.Items.Add(exit);

            ni.Icon = AweShark.Properties.Resources.OpenShark;
            ni.Text = project;
            ni.ContextMenuStrip = cmi;
            ni.Visible = true;
            ni.DoubleClick += ((obj, send) => { this.ShowHide(); });
            OpenShark.Core.hook.KeyPressed +=
                new Microsoft.FSharp.Control.FSharpHandler
                    <Tuple<OpenShark.ApiHook.KeyboardHook, OpenShark.Hotkeys.KeyPressedEventArgs>>(hook_KeyPressed);
            /// __________________________________________________________________________

            webControl.ShowCreatedWebView += App.OnShowNewView;

            OpenShark.Core.SetupGlobalHotkeys();
            }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
            }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            switch (this.WindowState) {
                case WindowState.Normal:
                case WindowState.Maximized: OpenShark.Properties.WindowState = this.WindowState; break;
                default: OpenShark.Properties.WindowState = WindowState.Normal; break;
                }
            OpenShark.Properties.Save(ini);

            // Destroy the WebControl and its underlying view.
            webControl.Dispose();

            if (System.Windows.Application.Current.Windows.Count <= 1) WebCore.Shutdown();
            }

        private void webControl_WindowClose(object sender, WindowCloseEventArgs e) {
            if (!e.IsCalledFromFrame)
                this.Close();
            }

        private void webControl_ConsoleMessage(object sender, ConsoleMessageEventArgs e) {
            // Display JavaScript console messages.
            Debug.Print(String.Format("{0} - Line: {1}", e.Message, e.LineNumber));
            }
        }
    }
