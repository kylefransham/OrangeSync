//   OrangeShare, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Drawing;
using System.IO;
using System.Threading;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using MonoMac.WebKit;

namespace OrangeShare {

    public class OrangeEventLog : NSWindow {

        public OrangeEventLogController Controller = new OrangeEventLogController ();

        private WebView web_view;
        private NSBox separator;
        private NSBox background;
        private NSPopUpButton popup_button;
        private NSProgressIndicator progress_indicator;
        private NSTextField size_label;
        private NSTextField size_label_value;
        private NSTextField history_label;
        private NSTextField history_label_value;
        private NSButton hidden_close_button;
        

        public OrangeEventLog (IntPtr handle) : base (handle)
        {
        }


        public OrangeEventLog () : base ()
        {
            using (var a = new NSAutoreleasePool ())
            {
                Title    = "Recent Changes";
                Delegate = new OrangeEventsDelegate ();
                // TODO: Make window resizable

                int width  = 480;
                int height = 640;
                float x    = (float) (NSScreen.MainScreen.Frame.Width * 0.61);
                float y    = (float) (NSScreen.MainScreen.Frame.Height * 0.5 - (height * 0.5));

                SetFrame (new RectangleF (x, y, width, height), true);


                StyleMask = (NSWindowStyle.Closable |
                             NSWindowStyle.Miniaturizable |
                             NSWindowStyle.Titled);

                MaxSize     = new SizeF (480, 640);
                MinSize     = new SizeF (480, 640);
                HasShadow   = true;
                BackingType = NSBackingStore.Buffered;


                this.web_view = new WebView (new RectangleF (0, 0, 481, 579), "", "") {
                    PolicyDelegate = new OrangeWebPolicyDelegate ()
                };


                this.hidden_close_button = new NSButton () {
                    Frame                     = new RectangleF (0, 0, 0, 0),
                    KeyEquivalentModifierMask = NSEventModifierMask.CommandKeyMask,
                    KeyEquivalent             = "w"
                };

                this.hidden_close_button.Activated += delegate {
                    Controller.WindowClosed ();
                };


                this.size_label = new NSTextField () {
                    Alignment       = NSTextAlignment.Right,
                    BackgroundColor = NSColor.WindowBackground,
                    Bordered        = false,
                    Editable        = false,
                    Frame           = new RectangleF (0, 588, 60, 20),
                    StringValue     = "Size:",
                    Font            = OrangeUI.BoldFont
                };

                this.size_label_value = new NSTextField () {
                    Alignment       = NSTextAlignment.Left,
                    BackgroundColor = NSColor.WindowBackground,
                    Bordered        = false,
                    Editable        = false,
                    Frame           = new RectangleF (60, 588, 75, 20),
                    StringValue     = "…",
                    Font            = OrangeUI.Font
                };


                this.history_label = new NSTextField () {
                    Alignment       = NSTextAlignment.Right,
                    BackgroundColor = NSColor.WindowBackground,
                    Bordered        = false,
                    Editable        = false,
                    Frame           = new RectangleF (130, 588, 60, 20),
                    StringValue     = "History:",
                    Font            = OrangeUI.BoldFont
                };

                this.history_label_value = new NSTextField () {
                    Alignment       = NSTextAlignment.Left,
                    BackgroundColor = NSColor.WindowBackground,
                    Bordered        = false,
                    Editable        = false,
                    Frame           = new RectangleF (190, 588, 75, 20),
                    StringValue     = "…",
                    Font            = OrangeUI.Font
                };


                this.separator = new NSBox (new RectangleF (0, 579, 480, 1)) {
                    BorderColor = NSColor.LightGray,
                    BoxType = NSBoxType.NSBoxCustom
                };


                this.background = new NSBox (new RectangleF (0, -1, 481, 581)) {
                    FillColor = NSColor.White,
                    BoxType = NSBoxType.NSBoxCustom
                };


                this.progress_indicator = new NSProgressIndicator () {
                    Style = NSProgressIndicatorStyle.Spinning,
                    Frame = new RectangleF (this.web_view.Frame.Width / 2 - 10,
                        this.web_view.Frame.Height / 2 + 10, 20, 20)
                };

                this.progress_indicator.StartAnimation (this);


                ContentView.AddSubview (this.size_label);
                ContentView.AddSubview (this.size_label_value);
                ContentView.AddSubview (this.history_label);
                ContentView.AddSubview (this.history_label_value);
                ContentView.AddSubview (this.separator);
                ContentView.AddSubview (this.progress_indicator);
                ContentView.AddSubview (this.background);
                ContentView.AddSubview (this.hidden_close_button);


                (this.web_view.PolicyDelegate as OrangeWebPolicyDelegate).LinkClicked += delegate (string href) {
                    Controller.LinkClicked (href);
                };
            }


            // Hook up the controller events
            Controller.HideWindowEvent += delegate {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        PerformClose (this);
						
						if (this.web_view.Superview == ContentView)
                            this.web_view.RemoveFromSuperview ();
                    });
                }
            };

            Controller.ShowWindowEvent += delegate {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        OrderFrontRegardless ();
                    });
                }
            };

            Controller.UpdateChooserEvent += delegate (string [] folders) {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        UpdateChooser (folders);
                    });
                }
            };

            Controller.UpdateContentEvent += delegate (string html) {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        UpdateContent (html);
                    });
                }
            };

            Controller.ContentLoadingEvent += delegate {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        if (this.web_view.Superview == ContentView)
                            this.web_view.RemoveFromSuperview ();

                        ContentView.AddSubview (this.progress_indicator);
                    });
                }
            };

            Controller.UpdateSizeInfoEvent += delegate (string size, string history_size) {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        this.size_label_value.StringValue    = size;
                        this.history_label_value.StringValue = history_size;
                    });
                }
            };
        }


        public void UpdateChooser (string [] folders)
        {
            using (var a = new NSAutoreleasePool ())
            {
                if (folders == null)
                    folders = Controller.Folders;
    
                if (this.popup_button != null)
                    this.popup_button.RemoveFromSuperview ();
    
                this.popup_button = new NSPopUpButton () {
                    Frame     = new RectangleF (480 - 156 - 8, 640 - 31 - 24, 156, 26),
                    PullsDown = false
                };
    
                this.popup_button.Cell.ControlSize = NSControlSize.Small;
                this.popup_button.Font = NSFontManager.SharedFontManager.FontWithFamily
                    ("Lucida Grande", NSFontTraitMask.Condensed, 0, NSFont.SmallSystemFontSize);
    
                this.popup_button.AddItem ("All Projects");
                this.popup_button.Menu.AddItem (NSMenuItem.SeparatorItem);
				
				int row = 2;
           		foreach (string folder in folders) {
	                this.popup_button.AddItem (folder);
					
					if (folder.Equals (Controller.SelectedFolder))
						this.popup_button.SelectItem (row);
					
					row++;
            	}
				
                this.popup_button.AddItems (folders);
    
                this.popup_button.Activated += delegate {
                    using (var b = new NSAutoreleasePool ())
                    {
                        InvokeOnMainThread (delegate {
                            if (this.popup_button.IndexOfSelectedItem == 0)
                                Controller.SelectedFolder = null;
                            else
                                Controller.SelectedFolder = this.popup_button.SelectedItem.Title;
                        });
                    }
                };

                ContentView.AddSubview (this.popup_button);
            }
        }


        public void UpdateContent (string html)
        {
            Thread thread = new Thread (new ThreadStart (delegate {
                using (var a = new NSAutoreleasePool ())
                {
                    if (html == null)
                        html = Controller.HTML;
    
					string pixmaps_path = "file://" + Path.Combine (
						NSBundle.MainBundle.ResourcePath, "Pixmaps");
					
                    html = html.Replace ("<!-- $body-font-family -->", "Lucida Grande");
                    html = html.Replace ("<!-- $day-entry-header-font-size -->", "13.6px");
                    html = html.Replace ("<!-- $body-font-size -->", "13.4px");
                    html = html.Replace ("<!-- $secondary-font-color -->", "#bbb");
                    html = html.Replace ("<!-- $small-color -->", "#ddd");
                    html = html.Replace ("<!-- $day-entry-header-background-color -->", "#f5f5f5");
                    html = html.Replace ("<!-- $a-color -->", "#0085cf");
                    html = html.Replace ("<!-- $a-hover-color -->", "#009ff8");
					
                    html = html.Replace ("<!-- $pixmaps-path -->", pixmaps_path);
    
                    html = html.Replace ("<!-- $document-added-background-image -->",
                        pixmaps_path + "/document-added-12.png");
    
                    html = html.Replace ("<!-- $document-deleted-background-image -->",
                        pixmaps_path + "/document-deleted-12.png");
    
                    html = html.Replace ("<!-- $document-edited-background-image -->",
                        pixmaps_path + "/document-edited-12.png");
    
                    html = html.Replace ("<!-- $document-moved-background-image -->",
                        pixmaps_path + "/document-moved-12.png");
					
                    InvokeOnMainThread (delegate {
                        if (this.progress_indicator.Superview == ContentView)
                            this.progress_indicator.RemoveFromSuperview ();
    
                        this.web_view.MainFrame.LoadHtmlString (html, new NSUrl (""));
                        ContentView.AddSubview (this.web_view);
                    });
                }
            }));

            thread.Start ();
        }


        public override void OrderFrontRegardless ()
        {
            NSApplication.SharedApplication.ActivateIgnoringOtherApps (true);
            MakeKeyAndOrderFront (this);

            if (Program.UI != null)
                Program.UI.UpdateDockIconVisibility ();

            base.OrderFrontRegardless ();
        }


        public override void PerformClose (NSObject sender)
        {
            base.OrderOut (this);

            if (Program.UI != null)
                Program.UI.UpdateDockIconVisibility ();

            return;
        }
    }


    public class OrangeEventsDelegate : NSWindowDelegate {

        public override bool WindowShouldClose (NSObject sender)
        {
            (sender as OrangeEventLog).Controller.WindowClosed ();
            return false;
        }
    }
    
    
    public class OrangeWebPolicyDelegate : WebPolicyDelegate {

        public event LinkClickedHandler LinkClicked;
        public delegate void LinkClickedHandler (string href);


        public override void DecidePolicyForNavigation (WebView web_view, NSDictionary action_info,
            NSUrlRequest request, WebFrame frame, NSObject decision_token)
        {
            if (LinkClicked != null)
                LinkClicked (request.Url.ToString ());
        }
    }
}
