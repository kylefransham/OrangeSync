//   OrangeShare, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons (hylkebons@gmail.com)
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
//   along with this program. If not, see (http://www.gnu.org/licenses/).


using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using OrangeLib;

namespace OrangeShare {

    public class OrangeInvite {

        public string Address { get; private set; }
        public string RemotePath { get; private set; }
        public string Fingerprint { get; private set; }
        public string AcceptUrl { get; private set; }
        public string AnnouncementsUrl { get; private set; }


        public bool IsValid {
            get {
                return (!string.IsNullOrEmpty (Address) &&
                        !string.IsNullOrEmpty (RemotePath));
            }
        }


        public OrangeInvite (string xml_file_path)
        {
            XmlDocument xml_document = new XmlDocument ();
            XmlNode node;

            string address           = "";
            string remote_path       = "";
            string accept_url        = "";
            string announcements_url = "";
            string fingerprint       = "";

            try {
                xml_document.Load (xml_file_path);

                node = xml_document.SelectSingleNode ("/sparkleshare/invite/address/text()");
                if (node != null) { address = node.Value; }

                node = xml_document.SelectSingleNode ("/sparkleshare/invite/remote_path/text()");
                if (node != null) { remote_path = node.Value; }

                node = xml_document.SelectSingleNode ("/sparkleshare/invite/accept_url/text()");
                if (node != null) { accept_url = node.Value; }

                node = xml_document.SelectSingleNode ("/sparkleshare/invite/announcements_url/text()");
                if (node != null) { announcements_url = node.Value; }

                node = xml_document.SelectSingleNode ("/sparkleshare/invite/fingerprint/text()");
                if (node != null) { fingerprint = node.Value; }

                Initialize (address, remote_path, accept_url, announcements_url, fingerprint);

            } catch (XmlException e) {
                OrangeHelpers.DebugInfo ("Invite", "Invalid XML: " + e.Message);
                return;
            }
        }


        public bool Accept ()
        {
            if (string.IsNullOrEmpty (AcceptUrl))
                return true;

            try {
                WebRequest request  = WebRequest.Create (AcceptUrl);

                request.Method        = "POST";
                request.ContentType   = "application/x-www-form-urlencoded";
                string post_data      = "pubkey=" + OrangeConfig.DefaultConfig.User.PublicKey;
                byte [] post_bytes    = Encoding.UTF8.GetBytes (post_data);
                request.ContentLength = post_bytes.Length;

                Stream data_stream = request.GetRequestStream ();
                data_stream.Write (post_bytes, 0, post_bytes.Length);
                data_stream.Close ();

                WebResponse response = request.GetResponse ();
                response.Close ();


                if ((response as HttpWebResponse).StatusCode == HttpStatusCode.OK) {
                    OrangeHelpers.DebugInfo ("Invite", "Uploaded public key to " + AcceptUrl);
                    return true;

                } else {
                    OrangeHelpers.DebugInfo ("Invite", "Failed uploading public key to " + AcceptUrl);
                    return false;
                }

            } catch (WebException e) {
                OrangeHelpers.DebugInfo ("Invite", "Failed uploading public key to " + AcceptUrl + ": " + e.Message);

                return false;
            }
        }


        private void Initialize (string address, string remote_path,
            string accept_url, string announcements_url, string fingerprint)
        {
            Address          = address;
            RemotePath       = remote_path;
            AcceptUrl        = accept_url;
            AnnouncementsUrl = announcements_url;
            Fingerprint      = fingerprint;
        }
    }
}
