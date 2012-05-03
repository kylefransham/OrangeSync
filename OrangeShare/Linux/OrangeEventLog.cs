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
using System.Collections.Generic;
using System.Threading;

using Gtk;
using Mono.Unix;
using WebKit;

using IO = System.IO;

namespace OrangeShare {

    public class OrangeEventLog : Window {

        public OrangeEventLogController Controller = new OrangeEventLogController ();

        private Label size_label;
        private Label history_label;
        private HBox layout_horizontal;
        private ComboBox combo_box;
        private HBox combo_box_wrapper;
        private EventBox content_wrapper;
        private ScrolledWindow scrolled_window;
        private WebView web_view;
        private OrangeSpinner spinner;
        private string link_status;


        // Short alias for the translations
        public static string _ (string s)
        {
            return Catalog.GetString (s);
        }


        public OrangeEventLog () : base ("")
        {
            SetSizeRequest (480, 640);

            // TODO: right side of screen like on Mac
            SetPosition (WindowPosition.Center);

            Resizable   = true;
            BorderWidth = 0;

            Title = _("Recent Changes");
            IconName = "folder-sparkleshare";

            DeleteEvent += delegate (object o, DeleteEventArgs args) {
                Controller.WindowClosed ();
                args.RetVal = true;
            };

            this.size_label = new Label () {
                Markup = "<b>Size:</b> …",
                Xalign = 0
            };
            
            this.history_label = new Label () {
                Markup = "<b>History:</b> …",
                Xalign = 0
            };
            
            HBox layout_sizes = new HBox (false, 12);
            layout_sizes.Add (this.size_label);
            layout_sizes.Add (this.history_label);

            VBox layout_vertical = new VBox (false, 0);
            this.spinner         = new OrangeSpinner (22);
            this.content_wrapper = new EventBox ();
            this.scrolled_window = new ScrolledWindow ();

            Gdk.Color white = new Gdk.Color();
            Gdk.Color.Parse ("white", ref white);

            this.content_wrapper.ModifyBg (StateType.Normal, white);

            this.web_view = new WebView () {
                Editable = false
            };

            this.web_view.HoveringOverLink += delegate (object o, WebKit.HoveringOverLinkArgs args) {
                this.link_status = args.Link;
            };

            this.web_view.NavigationRequested += delegate (object o, WebKit.NavigationRequestedArgs args) {
                if (args.Request.Uri == this.link_status)
                    Controller.LinkClicked (args.Request.Uri);

                // Don't follow HREFs (as this would cause a page refresh)
                if (!args.Request.Uri.Equals ("file:"))
                    args.RetVal = 1;
            };

            this.scrolled_window.Add (this.web_view);
            this.content_wrapper.Add (this.spinner);

            this.spinner.Start ();

            this.layout_horizontal = new HBox (true, 0);
            this.layout_horizontal.PackStart (layout_sizes, true, true, 12);

            layout_vertical.PackStart (this.layout_horizontal, false, false, 0);
            layout_vertical.PackStart (CreateShortcutsBar (), false, false, 0);
            layout_vertical.PackStart (this.content_wrapper, true, true, 0);

            Add (layout_vertical);


            Controller.HideWindowEvent += delegate {
                Application.Invoke (delegate {
                    HideAll ();
					
					if (this.content_wrapper.Child != null)
                        this.content_wrapper.Remove (this.content_wrapper.Child);
                });
            };

            Controller.ShowWindowEvent += delegate {
                Application.Invoke (delegate {
                    ShowAll ();
                    Present ();
                });
            };

            Controller.UpdateChooserEvent += delegate (string [] folders) {
                Application.Invoke (delegate {
                    UpdateChooser (folders);
                });
            };

            Controller.UpdateContentEvent += delegate (string html) {
                 Application.Invoke (delegate {
                    UpdateContent (html);
                });
            };

            Controller.ContentLoadingEvent += delegate {
                Application.Invoke (delegate {
                    if (this.content_wrapper.Child != null)
                        this.content_wrapper.Remove (this.content_wrapper.Child);

                    this.content_wrapper.Add (this.spinner);
                    this.spinner.Start ();
                    this.content_wrapper.ShowAll ();
                });
            };

            Controller.UpdateSizeInfoEvent += delegate (string size, string history_size) {
                Application.Invoke (delegate {
                    this.size_label.Markup    = "<b>Size:</b> " + size;
                    this.history_label.Markup = "<b>History:</b> " + history_size;

                    this.size_label.ShowAll ();
                    this.history_label.ShowAll ();
                });
            };
        }


        public void UpdateChooser (string [] folders)
        {
            if (folders == null)
                folders = Controller.Folders;

            if (this.combo_box_wrapper != null && this.combo_box_wrapper.Parent != null) {
                this.layout_horizontal.Remove (this.combo_box_wrapper);
                this.combo_box_wrapper.Remove (this.combo_box);
            }

            this.combo_box_wrapper = new HBox (false, 0);
            this.combo_box = new ComboBox ();

            CellRendererText cell = new CellRendererText();
            this.combo_box.PackStart (cell, false);
            this.combo_box.AddAttribute (cell, "text", 0);

            ListStore store = new ListStore (typeof (string));

            store.AppendValues (_("All Projects"));
            store.AppendValues ("---");
			
			this.combo_box.Model  = store;
			this.combo_box.Active = 0;
			
			int row = 2;
       		foreach (string folder in folders) {
				store.AppendValues (folder);
				
				if (folder.Equals (Controller.SelectedFolder))
					this.combo_box.Active = row;
				
				row++;
        	}

            this.combo_box.RowSeparatorFunc = delegate (TreeModel model, TreeIter iter) {
                string item = (string) this.combo_box.Model.GetValue (iter, 0);
                return (item == "---");
            };

            this.combo_box.Changed += delegate {
                TreeIter iter;
                this.combo_box.GetActiveIter (out iter);
                string selection = (string) this.combo_box.Model.GetValue (iter, 0);
                TreePath path    = this.combo_box.Model.GetPath (iter);

                if (path.Indices [0] == 0)
                    Controller.SelectedFolder = null;
                else
                    Controller.SelectedFolder = selection;
            };

            this.combo_box_wrapper.PackStart (new Label (" "), false, false, 9);
            this.combo_box_wrapper.PackStart (this.combo_box, true, true, 0);

            this.layout_horizontal.BorderWidth = 9;
            this.layout_horizontal.PackStart (this.combo_box_wrapper, true, true, 0);
            this.layout_horizontal.ShowAll ();
        }


        public void UpdateContent (string html)
        {
            Thread thread = new Thread (new ThreadStart (delegate {
                if (html == null)
                    html = Controller.HTML;

                if (html == null)
                    return;
				
				string pixmaps_path = IO.Path.Combine (OrangeUI.AssetsPath, "pixmaps");
				string icons_path  = new string [] {OrangeUI.AssetsPath, "icons",
                	"hicolor", "12x12", "status"}.Combine ();

                html = html.Replace ("<!-- $body-font-size -->", (double) (Style.FontDescription.Size / 1024 + 3) + "px");
                html = html.Replace ("<!-- $day-entry-header-font-size -->", (Style.FontDescription.Size / 1024 + 3) + "px");
                html = html.Replace ("<!-- $a-color -->", "#0085cf");
                html = html.Replace ("<!-- $a-hover-color -->", "#009ff8");
                html = html.Replace ("<!-- $body-font-family -->", "\"" + Style.FontDescription.Family + "\"");
                html = html.Replace ("<!-- $body-color -->", OrangeUIHelpers.GdkColorToHex (Style.Foreground (StateType.Normal)));
                html = html.Replace ("<!-- $body-background-color -->", OrangeUIHelpers.GdkColorToHex (new TreeView ().Style.Base (StateType.Normal)));
                html = html.Replace ("<!-- $day-entry-header-background-color -->", OrangeUIHelpers.GdkColorToHex (Style.Background (StateType.Normal)));
                html = html.Replace ("<!-- $secondary-font-color -->", OrangeUIHelpers.GdkColorToHex (Style.Foreground (StateType.Insensitive)));
                html = html.Replace ("<!-- $small-color -->", OrangeUIHelpers.GdkColorToHex (Style.Foreground (StateType.Insensitive)));
             
				html = html.Replace ("<!-- $pixmaps-path -->", pixmaps_path);
                
				html = html.Replace ("<!-- $document-added-background-image -->", 
					"file://" + IO.Path.Combine (icons_path + "document-added.png"));
				
				html = html.Replace ("<!-- $document-edited-background-image -->", 
					"file://" + IO.Path.Combine (icons_path + "document-edited.png"));
				
				html = html.Replace ("<!-- $document-deleted-background-image -->", 
					"file://" + IO.Path.Combine (icons_path + "document-deleted.png"));
				
				html = html.Replace ("<!-- $document-moved-background-image -->", 
					"file://" + IO.Path.Combine (icons_path + "document-moved.png"));
                        
                
                Application.Invoke (delegate {
                    this.spinner.Stop ();
                    this.web_view.LoadString (html, null, null, "file://");
                    this.content_wrapper.Remove (this.content_wrapper.Child);
                    this.content_wrapper.Add (this.scrolled_window);
                    this.content_wrapper.ShowAll ();
                });
            }));

            thread.Start ();
        }


        private MenuBar CreateShortcutsBar ()
        {
            // Adds a hidden menubar that contains to enable keyboard
            // shortcuts to close the log
            MenuBar menu_bar = new MenuBar ();

                MenuItem file_item = new MenuItem ("File");

                    Menu file_menu = new Menu ();

                        MenuItem close_1 = new MenuItem ("Close1");
                        MenuItem close_2 = new MenuItem ("Close2");
        
                        // adds specific Ctrl+W and Esc key accelerators to Log Window
                        AccelGroup accel_group = new AccelGroup ();
                        AddAccelGroup (accel_group);

                        // Close on Esc
                        close_1.AddAccelerator ("activate", accel_group, new AccelKey (Gdk.Key.W,
                            Gdk.ModifierType.ControlMask, AccelFlags.Visible));

                        close_1.Activated += delegate { Controller.WindowClosed (); };

                        // Close on Ctrl+W
                        close_2.AddAccelerator ("activate", accel_group, new AccelKey (Gdk.Key.Escape,
                            Gdk.ModifierType.None, AccelFlags.Visible));
                        close_2.Activated += delegate { Controller.WindowClosed (); };

                    file_menu.Append (close_1);
                    file_menu.Append (close_2);

                file_item.Submenu = file_menu;

            menu_bar.Append (file_item);

            // Hacky way to hide the menubar, but the accellerators
            // will simply be disabled when using Hide ()
            menu_bar.HeightRequest = 1;
            menu_bar.ModifyBg (StateType.Normal, Style.Background (StateType.Normal));

            return menu_bar;
        }
    }
}
