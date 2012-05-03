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

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using OrangeLib;

namespace OrangeShare {

	public class OrangeController : OrangeControllerBase {

        public override string PluginsPath {
            get {
                return Path.Combine (NSBundle.MainBundle.ResourcePath, "Plugins");
            }
        }

        // We have to use our own custom made folder watcher, as
        // System.IO.FileSystemWatcher fails watching subfolders on Mac
        private OrangeMacWatcher watcher;

        
        public OrangeController () : base ()
        {
            using (var a = new NSAutoreleasePool ())
            {
                string content_path =
                    Directory.GetParent (System.AppDomain.CurrentDomain.BaseDirectory).ToString ();
    
                string app_path   = Directory.GetParent (content_path).ToString ();
                string growl_path = Path.Combine (app_path, "Frameworks", "Growl.framework", "Growl");
    
                // Needed for Growl
                Dlfcn.dlopen (growl_path, 0);
                NSApplication.Init ();
            }


            // Let's use the bundled git first
            OrangeLib.Git.OrangeGit.Path =
                Path.Combine (NSBundle.MainBundle.ResourcePath,
                    "git", "libexec", "git-core", "git");

            OrangeLib.Git.OrangeGit.ExecPath =
                Path.Combine (NSBundle.MainBundle.ResourcePath,
                    "git", "libexec", "git-core");
        }


        public override void Initialize ()
        {
            base.Initialize ();

            this.watcher.Changed += delegate (object sender, OrangeMacWatcherEventArgs args) {
                string path = args.Path;

                // Don't even bother with paths in .git/
                if (path.Contains (".git"))
                    return;

                string repo_name;

                if (path.Contains ("/"))
                    repo_name = path.Substring (0, path.IndexOf ("/"));
                else
                    repo_name = path;

                // Ignore changes in the root of each subfolder, these
                // are already handled by the repository
                if (Path.GetFileNameWithoutExtension (path).Equals (repo_name))
                    return;

                repo_name = repo_name.Trim ("/".ToCharArray ());
                FileSystemEventArgs fse_args = new FileSystemEventArgs (
                    WatcherChangeTypes.Changed,
                    Path.Combine (OrangeConfig.DefaultConfig.FoldersPath, path),
                    Path.GetFileName (path)
                );

                foreach (OrangeRepoBase repo in Repositories) {
                    if (repo.Name.Equals (repo_name))
                        repo.OnFileActivity (fse_args);
                }
            };
        }


		public override void CreateStartupItem ()
		{
            // There aren't any bindings in MonoMac to support this yet, so
            // we call out to an applescript to do the job
            Process process = new Process ();
            process.EnableRaisingEvents              = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute        = false;
            process.StartInfo.FileName               = "osascript";
            process.StartInfo.CreateNoWindow         = true;

            string app_path = Path.GetDirectoryName (NSBundle.MainBundle.ResourcePath);
            app_path        = Path.GetDirectoryName (app_path);

            process.StartInfo.Arguments = "-e 'tell application \"System Events\" to " +
                "make login item at end with properties {path:\"" + app_path + "\", hidden:false}'";

            process.Exited += delegate {
                OrangeHelpers.DebugInfo ("Controller", "Added " + app_path + " to login items");
            };

            try {
                process.Start ();

            } catch (Exception e) {
                OrangeHelpers.DebugInfo ("Controller", "Failed adding " + app_path + " to login items: " + e.Message);
            }
		}


        public override void InstallProtocolHandler ()
        {
             // We ship OrangeShareInviteHandler.app in the bundle
        }

		
		// Adds the OrangeShare folder to the user's
		// list of bookmarked places
		public override void AddToBookmarks ()
		{
            // TODO: Waiting for NSMutableArray/Dictionary support
         /* NSDictionary sidebar_plist = NSUserDefaults.StandardUserDefaults.PersistentDomainForName ("com.apple.sidebarlists");

            foreach (object sidebar_item in sidebar_plist.Keys) {
                if (sidebar_item.ToString ().Equals ("useritems")) {
                    NSDictionary user_items = (NSDictionary) sidebar_plist.ValueForKey (new NSString (sidebar_item.ToString ()));

                    foreach (NSObject user_item in user_items.Keys) {
                        if (user_item.ToString ().Equals ("CustomListItems")) {
                            NSArray custom_items = (NSArray) user_items.ValueForKey (new NSString (user_item.ToString ()));

                            NSDictionary new_dictionary = new NSDictionary ();
                            new dictionary.SetValueForKey (new NSString ("Test"),  new NSString ("Name"));

                            custom_items.SetValueForKey (new_dictionary, new NSString ("Item 6"));
                            user_items.SetValueForKey (custom_items, new NSString (user_item.ToString ()));
                        }
                    }

                    sidebar_plist.SetValueForKey (user_items, new NSString (sidebar_item.ToString ()));
                }
            }

            NSUserDefaults.StandardUserDefaults.SetPersistentDomain (sidebar_plist, "com.apple.sidebarlists"); */
		}
		

		// Creates the OrangeShare folder in the user's home folder
		public override bool CreateOrangeShareFolder ()
		{
            this.watcher = new OrangeMacWatcher (OrangeConfig.DefaultConfig.FoldersPath);

            if (!Directory.Exists (OrangeConfig.DefaultConfig.FoldersPath)) {
                Directory.CreateDirectory (OrangeConfig.DefaultConfig.FoldersPath);
                return true;

            } else {
                return false;
            }
		}


		public override void OpenFolder (string path)
		{
			NSWorkspace.SharedWorkspace.OpenFile (path);
		}
		
		
		public override string EventLogHTML
		{
			get {
                using (var a = new NSAutoreleasePool ()) {
    				string resource_path = NSBundle.MainBundle.ResourcePath;
    				string html_path     = Path.Combine (resource_path, "HTML", "event-log.html");
    				string html          = File.ReadAllText (html_path);
    
                    string jquery_file_path = Path.Combine (NSBundle.MainBundle.ResourcePath,
                        "HTML", "jquery.js");
    
                    string jquery = File.ReadAllText (jquery_file_path);
                    html          = html.Replace ("<!-- $jquery -->", jquery);
    
                    return html;
                }
			}
		}

		
		public override string DayEntryHTML
		{
			get {
                using (var a = new NSAutoreleasePool ())
                {
    				string resource_path = NSBundle.MainBundle.ResourcePath;
    				string html_path     = Path.Combine (resource_path, "HTML", "day-entry.html");
    				
    				StreamReader reader = new StreamReader (html_path);
    				string html = reader.ReadToEnd ();
    				reader.Close ();
    				
    				return html;
                }
			}
		}
		
	
		public override string EventEntryHTML
		{
			get {
                using (var a = new NSAutoreleasePool ()) {
    				string resource_path = NSBundle.MainBundle.ResourcePath;
    				string html_path     = Path.Combine (resource_path, "HTML", "event-entry.html");
    				
    				StreamReader reader = new StreamReader (html_path);
    				string html = reader.ReadToEnd ();
    				reader.Close ();
    				
    				return html;
                }
			}
		}


        public override void OpenFile (string url)
        {
            url = url.Replace ("%20", " ");
            NSWorkspace.SharedWorkspace.OpenFile (url);
        }
	}
}
