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
using System.Threading;

#if __MonoCS__
using Mono.Unix;
#endif
using OrangeLib;

namespace OrangeShare {

    // This is OrangeShare!
    public class Program {

        public static OrangeController Controller;
        public static OrangeUI UI;
		
		public static Mutex ProgramMutex = new Mutex (false, "OrangeShare");
		
		
        // Short alias for the translations
        public static string _ (string s)
        {
        #if __MonoCS__
            return Catalog.GetString (s);
         #else
            return Strings.T (s);
         #endif
        }
        
     
        #if !__MonoCS__
        [STAThread]
        #endif
        public static void Main (string [] args)
        {
            // Parse the command line options
            bool show_help       = false;
            OptionSet option_set = new OptionSet () {
                { "v|version", _("Print version information"), v => { PrintVersion (); } },
                { "h|help", _("Show this help text"), v => show_help = v != null }
            };

            try {
                option_set.Parse (args);

            } catch (OptionException e) {
                Console.Write ("OrangeShare: ");
                Console.WriteLine (e.Message);
                Console.WriteLine ("Try `sparkleshare --help' for more information.");
            }

            if (show_help)
                ShowHelp (option_set);

			
			// Only allow one instance of OrangeShare
			if (!ProgramMutex.WaitOne (0, false)) {
				Console.WriteLine ("OrangeShare is already running.");
				Environment.Exit (-1);
			}
				
			
            // Initialize the controller this way so that
            // there aren't any exceptions in the OS specific UI's
            Controller = new OrangeController ();
            Controller.Initialize ();
        
            if (Controller != null) {
                UI = new OrangeUI ();
                UI.Run ();
            }
         
            #if !__MonoCS__
            // Suppress assertion messages in debug mode
            GC.Collect (GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers ();
            #endif
        }


        // Prints the help output
        public static void ShowHelp (OptionSet option_set)
        {
            Console.WriteLine (" ");
            Console.WriteLine (_("OrangeShare, a collaboration and sharing tool."));
            Console.WriteLine (_("Copyright (C) 2010 Hylke Bons"));
            Console.WriteLine (" ");
            Console.WriteLine (_("This program comes with ABSOLUTELY NO WARRANTY."));
            Console.WriteLine (" ");
            Console.WriteLine (_("This is free software, and you are welcome to redistribute it "));
            Console.WriteLine (_("under certain conditions. Please read the GNU GPLv3 for details."));
            Console.WriteLine (" ");
            Console.WriteLine (_("OrangeShare is a collaboration and sharing tool that is "));
            Console.WriteLine (_("designed to keep things simple and to stay out of your way."));
            Console.WriteLine (" ");
            Console.WriteLine (_("Usage: sparkleshare [start|stop|restart|version] [OPTION]..."));
            Console.WriteLine (_("Sync OrangeShare folder with remote repositories."));
            Console.WriteLine (" ");
            Console.WriteLine (_("Arguments:"));

            option_set.WriteOptionDescriptions (Console.Out);
            Environment.Exit (0);
        }


        public static void PrintVersion ()
        {
            Console.WriteLine (_("OrangeShare " + Defines.VERSION));
            Environment.Exit (0);
        }
    }
}
