using System;
using System.Windows;
using Awesomium.Core;
using System.Diagnostics;

using System.Windows.Input;

using OpenShark;

namespace AweShark {
    public partial class MainWindow : Window {
        public MainWindow() {
            WebCore.Initialize(new WebConfig() {
                HomeURL = new Uri("asset://local/web/index.html"),
                LogLevel = LogLevel.Verbose,
            });
            InitializeComponent();

            /// Tray icon Load ____________________________________________________________
            System.Windows.Forms.NotifyIcon ni =
                new System.Windows.Forms.NotifyIcon();
            System.Windows.Forms.ContextMenuStrip cmi =
                new System.Windows.Forms.ContextMenuStrip();
            ni.Icon = AweShark.Properties.Resources.OpenShark;
            ni.Visible = true;
            ni.DoubleClick +=
                delegate(object sender, EventArgs args) {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    };
            /// __________________________________________________________________________

            webControl.ShowCreatedWebView += App.OnShowNewView;
            }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
            }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            // Destroy the WebControl and its underlying view.
            webControl.Dispose();

            if (Application.Current.Windows.Count <= 1)
                WebCore.Shutdown();
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
