using System;
using System.Linq;
using System.Windows;
using Awesomium.Core;
using System.Collections.Generic;
using System.Windows.Input;

namespace AweShark {
    public partial class ChildWindow : Window {
        public ChildWindow() {
            InitializeComponent();
            // In this example, ShowCreatedWebView of all WebControls, 
            // is handled by a common handler.

            webControl.ShowCreatedWebView += App.OnShowNewView;
            // Temporary demonstration of custom handling of Keyboard events.
            // Uncomment these and check the handlers on the bottom of the 
            // Event Handlers region.
            //webControl.PreviewKeyDown += OnWebControlKeyDown;
            //webControl.PreviewTextInput += OnWebControlTextInput;
            //webControl.PreviewKeyUp += OnWebControlKeyUp;
            }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            // Destroy the WebControl and its underlying view.
            webControl.Dispose();
            }

        // This will be set to the target URL, when this ChildWindow does not
        // host a created child view. The WebControl, is bound to this property.
        public Uri Source {
            get { return this.GetValue(SourceProperty) as Uri; }
            set { SetValue(SourceProperty, value); }
            }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source",
            typeof(Uri), typeof(ChildWindow),
            new UIPropertyMetadata(null));

        // This will be set to the created child view that the WebControl will wrap,
        // when ShowCreatedWebView is the result of 'window.open'. The WebControl, 
        // is bound to this property.
        public IntPtr NativeView {
            get { return (IntPtr)this.GetValue(ChildWindow.NativeViewProperty); }
            internal set { this.SetValue(ChildWindow.NativeViewPropertyKey, value); }
            }

        private static readonly DependencyPropertyKey NativeViewPropertyKey =
            DependencyProperty.RegisterReadOnly("NativeView",
            typeof(IntPtr), typeof(ChildWindow),
            new FrameworkPropertyMetadata(IntPtr.Zero));

        public static readonly DependencyProperty NativeViewProperty =
            NativeViewPropertyKey.DependencyProperty;

        private void webControl_NativeViewInitialized(object sender, WebViewEventArgs e) {
            // This event is fired right when IsLive is set to true.
            if (!webControl.IsLive)
                return;

            // Acquire the NavigationInterceptor service. Navigation events (available through this interceptor)
            // are fired on the I/O thread.
            INavigationInterceptor navigationInterceptor = webControl.GetService(typeof(INavigationInterceptor)) as INavigationInterceptor;

            if (navigationInterceptor == null)
                return;

            // Add a handler for the BeginNavigation event. This will allow us
            // to explicitly cancel navigations.
            navigationInterceptor.BeginNavigation += OnBeginNavigation;
            // Declare a filtering rule. This accepts wildcards. In this example,
            // we deny navigations to any Google site. Note that this will not
            // prevent the initial loading of the WebControl; the value defined
            // to Source at design-time, will always be loaded. After the program
            // starts, try to navigate to any of the other Google services (Images, Mail etc.).
            navigationInterceptor.AddRule(@"http?://*.google.com/*", NavigationRule.Deny);
            }

        // This event is fired on the I/O thread. Do not attempt to access the WebControl 
        // or any other not thread-safe Awesomium API from inside this event handler.
        private void OnBeginNavigation(object sender, NavigationEventArgs e) {
            // We demonstrate canceling navigation to
            // specified URLs. This example will also
            // prevent firing a download for the ZIP file.
            if (e.Url.ToString().EndsWith(".zip"))
                e.Cancel = true;
            }

        // ======================================================================================

        // Temporary demonstration of providing custom handling for Keyboard events.
        // Uncomment assigning these handlers in the constructor.


        private void OnWebControlKeyDown(object sender, KeyEventArgs e) {
            if (!webControl.IsLive)
                return;

            // This will for example to enter any text in a contentEditable,
            // but not in a text input.
            if (webControl.FocusedElementType != FocusedElementType.TextInput)
                return;

            // We demonstrate disallowing 2, Tab and Backspace.
            switch (e.Key) {
                case Key.D2:
                case Key.Tab:
                case Key.Back:
                    e.Handled = true;
                    break;
                }
            }
        private void OnWebControlTextInput(object sender, TextCompositionEventArgs e) {
            if (!webControl.IsLive)
                return;

            if (webControl.FocusedElementType != FocusedElementType.TextInput)
                return;

            if (e.Text == "2")
                e.Handled = true;
            }
        private void OnWebControlKeyUp(object sender, KeyEventArgs e) {
            if (!webControl.IsLive)
                return;

            if (webControl.FocusedElementType != FocusedElementType.TextInput)
                return;

            switch (e.Key) {
                case Key.D2:
                case Key.Tab:
                case Key.Back:
                    e.Handled = true;
                    break;
                }
            }
        }
    }
