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
using System.IO;
using System.Collections.Generic;

namespace OrangeLib {

    public class OrangeChangeSet {

        public OrangeUser User = new OrangeUser ("Unknown", "Unknown");

        public OrangeFolder Folder;
        public string Revision;
        public DateTime Timestamp;
        public DateTime FirstTimestamp;
        public bool IsMagical = false;
        public Uri RemoteUrl;

        public List<string> Added     = new List<string> ();
        public List<string> Deleted   = new List<string> ();
        public List<string> Edited    = new List<string> ();
        public List<string> MovedFrom = new List<string> ();
        public List<string> MovedTo   = new List<string> ();
		

        public string RelativeTimestamp {
            get {
                TimeSpan time_span = DateTime.Now - Timestamp;

                if (time_span <= TimeSpan.FromSeconds (60))
                    return "just now";

                if (time_span <= TimeSpan.FromMinutes (60))
                    return time_span.Minutes > 1
                        ? time_span.Minutes + " minutes ago"
                        : "a minute ago";

                if (time_span <= TimeSpan.FromHours (24))
                    return time_span.Hours > 1
                        ? time_span.Hours + " hours ago"
                        : "an hour ago";

                if (time_span <= TimeSpan.FromDays (30))
                    return time_span.Days > 1
                        ? time_span.Days + " days ago"
                        : "a day ago";

                if (time_span <= TimeSpan.FromDays (365))
                    return time_span.Days > 30
                    ? (time_span.Days / 30) + " months ago"
                    : "a month ago";

                return time_span.Days > 365
                    ? (time_span.Days / 365) + " years ago"
                    : "a year ago";
            }   
        }
    }


    // TODO: Move to its own file (and update all .csproj files)
    public class OrangeFolder {

        public string Name;
        public Uri RemoteAddress;

        public string FullPath {
            get {
                string custom_path = OrangeConfig.DefaultConfig.GetFolderOptionalAttribute (Name, "path");

                if (custom_path != null)
                    return Path.Combine (custom_path, Name);
                else
                    return Path.Combine (OrangeConfig.DefaultConfig.FoldersPath, Name);
            }
        }


        public OrangeFolder (string name)
        {
            Name = name;
        }
    }
}
