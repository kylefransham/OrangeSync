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
using System.Diagnostics;
using System.IO;

using OrangeLib;

namespace OrangeShare {

    public class OrangeController : OrangeControllerBase {


        public override string PluginsPath {
            get {
                return OrangeHelpers.CombineMore (Defines.DATAROOTDIR, "sparkleshare", "plugins");
            }
        }


        public OrangeController () : base ()
        {
        }


        // Creates a .desktop entry in autostart folder to
        // start OrangeShare automatically at login
        public override void CreateStartupItem ()
        {
            string autostart_path = Path.Combine (
                Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
                "autostart"
            );

            string desktopfile_path = Path.Combine (autostart_path, "sparkleshare.desktop");

            if (!Directory.Exists (autostart_path))
                Directory.CreateDirectory (autostart_path);

            if (!File.Exists (desktopfile_path)) {
                try {
                    File.WriteAllText (desktopfile_path,
                        "[Desktop Entry]\n" +
                        "Type=Application\n" +
                        "Name=OrangeShare\n" +
                        "Exec=sparkleshare start\n" +
                        "Icon=folder-sparkleshare\n" +
                        "Terminal=false\n" +
                        "X-GNOME-Autostart-enabled=true\n" +
                        "Categories=Network");

                    OrangeHelpers.DebugInfo ("Controller", "Added OrangeShare to login items");

                } catch (Exception e) {
                    OrangeHelpers.DebugInfo ("Controller", "Failed adding OrangeShare to login items: " + e.Message);
                }
            }
        }
        
        
        public override void InstallProtocolHandler ()
        {
            // sparkleshare-invite-opener.desktop launches the handler on newer
            // systems (like GNOME 3) that implement the last freedesktop.org specs.
            // For GNOME 2 however we need to tell gconf about the protocol manually

            try {
                // Add the handler to gconf...
                Process process = new Process ();
                process.StartInfo.FileName  = "gconftool-2";
                process.StartInfo.Arguments =
                    "-s /desktop/gnome/url-handlers/sparkleshare/command 'sparkleshare open %s' --type String";

                process.Start ();
                process.WaitForExit ();


                // ...and enable it
                process.StartInfo.Arguments =
                    "-s /desktop/gnome/url-handlers/sparkleshare/enabled --type Boolean true";

                process.Start ();
                process.WaitForExit ();

            } catch {
                // Pity...
            }
        }


        // Adds the OrangeShare folder to the user's
        // list of bookmarked places
        public override void AddToBookmarks ()
        {
            string bookmarks_file_path   = Path.Combine (OrangeConfig.DefaultConfig.HomePath, ".gtk-bookmarks");
            string sparkleshare_bookmark = "file://" + OrangeConfig.DefaultConfig.FoldersPath + " OrangeShare";

            if (File.Exists (bookmarks_file_path)) {
                StreamReader reader = new StreamReader (bookmarks_file_path);
                string bookmarks = reader.ReadToEnd ();
                reader.Close ();

                if (!bookmarks.Contains (sparkleshare_bookmark)) {
                    TextWriter writer = File.AppendText (bookmarks_file_path);
                    writer.WriteLine ("file://" + OrangeConfig.DefaultConfig.FoldersPath + " OrangeShare");
                    writer.Close ();
                }
            } else {
                StreamWriter writer = new StreamWriter (bookmarks_file_path);
                writer.WriteLine ("file://" + OrangeConfig.DefaultConfig.FoldersPath + " OrangeShare");
                writer.Close ();
            }
        }


        // Creates the OrangeShare folder in the user's home folder
        public override bool CreateOrangeShareFolder ()
        {
            if (!Directory.Exists (OrangeConfig.DefaultConfig.FoldersPath)) {
        
                Directory.CreateDirectory (OrangeConfig.DefaultConfig.FoldersPath);
                OrangeHelpers.DebugInfo ("Controller", "Created '" + OrangeConfig.DefaultConfig.FoldersPath + "'");

                string gvfs_command_path =
                    new string [] {Path.VolumeSeparatorChar.ToString (),
                        "usr", "bin", "gvfs-set-attribute"}.Combine ();

                // Add a special icon to the OrangeShare folder
                if (File.Exists (gvfs_command_path)) {
                    Process process = new Process ();

                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute        = false;
                    process.StartInfo.FileName               = "gvfs-set-attribute";

                    // Clear the custom (legacy) icon path
                    process.StartInfo.Arguments = "-t unset " + OrangeConfig.DefaultConfig.FoldersPath + " metadata::custom-icon";
                    process.Start ();
                    process.WaitForExit ();

                    // Give the OrangeShare folder an icon name, so that it scales
                    process.StartInfo.Arguments = OrangeConfig.DefaultConfig.FoldersPath + " metadata::custom-icon-name 'folder-sparkleshare'";
                    process.Start ();
                    process.WaitForExit ();
                }

                return true;
            }

            return false;
        }
        

        public override string EventLogHTML {
            get {
                string html_path = new string [] {Defines.PREFIX, "share",
                    "sparkleshare", "html", "event-log.html"}.Combine ();

                string html = File.ReadAllText (html_path);

                string jquery_file_path = new string [] {Defines.PREFIX, "share",
                    "sparkleshare", "html", "jquery.js"}.Combine ();

                string jquery = File.ReadAllText (jquery_file_path);
                html          = html.Replace ("<!-- $jquery -->", jquery);
            
                return html;
            }
        }

        
        public override string DayEntryHTML {
            get {
                string path = new string [] {Defines.PREFIX,
                    "share", "sparkleshare", "html", "day-entry.html"}.Combine ();
            
                return String.Join (Environment.NewLine, File.ReadAllLines (path));
            }
        }

        
        public override string EventEntryHTML {
            get {
                string path = new string [] {Defines.PREFIX,
                    "share", "sparkleshare", "html", "event-entry.html"}.Combine ();
            
                return String.Join (Environment.NewLine, File.ReadAllLines (path));
            }
        }

            
        public override void OpenFolder (string path)
        {
            OpenFile (path);
        }


        public override void OpenFile (string path)
        {
            Process process = new Process ();
            process.StartInfo.FileName = "xdg-open";
            process.StartInfo.Arguments = "\"" + path + "\"";
            process.Start ();
        }
    }
}
