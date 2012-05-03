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

namespace OrangeLib {

    public static class OrangeListenerFactory {

        private static List<OrangeListenerBase> listeners = new List<OrangeListenerBase> ();


        public static OrangeListenerBase CreateListener (string folder_name, string folder_identifier)
        {
            // Check if the user wants to use a global custom notification service
            string uri = OrangeConfig.DefaultConfig.GetConfigOption ("announcements_url");

            // Check if the user wants a use a custom notification service for this folder
            if (string.IsNullOrEmpty (uri))
                uri = OrangeConfig.DefaultConfig.GetFolderOptionalAttribute (
                    folder_name, "announcements_url");

            // Fall back to the fallback service is neither is the case
            if (string.IsNullOrEmpty (uri)) {
                // This is OrangeShare's centralized notification service.
                // It communicates "It's time to sync!" signals between clients.
                //
                // Here's how it works: the client listens to a channel (the
                // folder identifier, a SHA-1 hash) for when it's time to sync.
                // Clients also send the current revision hash to the channel
                // for other clients to pick up when you've synced up any
                // changes.
                //
                // Please see the OrangeShare wiki if you wish to run
                // your own service instead

                uri = "tcp://notifications.sparkleshare.org:1986";
            }

            Uri announce_uri = new Uri (uri);

            // We use only one listener per notification service to keep
            // the number of connections as low as possible
            foreach (OrangeListenerBase listener in listeners) {
                if (listener.Server.Equals (announce_uri)) {
                    OrangeHelpers.DebugInfo ("ListenerFactory",
                        "Refered to existing " + announce_uri.Scheme +
                        " listener for " + announce_uri);

                    // We already seem to have a listener for this server,
                    // refer to the existing one instead
                    listener.AlsoListenTo (folder_identifier);
                    return (OrangeListenerBase) listener;
                }
            }

            // Create a new listener with the appropriate
            // type if one doesn't exist yet for that server
            switch (announce_uri.Scheme) {
            case "tcp":
                listeners.Add (new OrangeListenerTcp (announce_uri, folder_identifier));
                break;
            default:
                listeners.Add (new OrangeListenerTcp (announce_uri, folder_identifier));
                break;
            }

            OrangeHelpers.DebugInfo ("ListenerFactory",
                "Issued new " + announce_uri.Scheme + " listener for " + announce_uri);

            return (OrangeListenerBase) listeners [listeners.Count - 1];
        }
    }
}
