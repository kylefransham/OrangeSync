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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Gtk;
using Mono.Unix;
using Mono.Unix.Native;
using OrangeLib;

namespace OrangeShare {

    public class OrangeUI {
        
        public static OrangeStatusIcon StatusIcon;
        public static OrangeEventLog EventLog;
        public static OrangeBubbles Bubbles;
        public static OrangeSetup Setup;
        public static OrangeAbout About;

        public static string AssetsPath =
            new string [] {Defines.PREFIX, "share", "sparkleshare"}.Combine ();


        // Short alias for the translations
        public static string _(string s)
        {
            return Program._ (s);
        }


        public OrangeUI ()
        {
            Application.Init ();

            // Use translations
            Catalog.Init (Defines.GETTEXT_PACKAGE, Defines.LOCALE_DIR);

            Setup      = new OrangeSetup ();
            EventLog   = new OrangeEventLog ();
            About      = new OrangeAbout ();
            Bubbles    = new OrangeBubbles ();
            StatusIcon = new OrangeStatusIcon ();
        
			Program.Controller.UIHasLoaded ();
        }


        // Runs the application
        public void Run ()
        {
            Application.Run ();
        }
    }
}