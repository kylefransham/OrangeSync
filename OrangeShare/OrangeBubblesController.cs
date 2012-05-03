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
using OrangeLib;

namespace OrangeShare {

    public class OrangeBubblesController {

        public event ShowBubbleEventHandler ShowBubbleEvent;
        public delegate void ShowBubbleEventHandler (string title, string subtext, string image_path);


        public OrangeBubblesController ()
        {
            Program.Controller.AlertNotificationRaised += delegate (string title, string message) {
                ShowBubble (title, message, null);
            };

            Program.Controller.NotificationRaised += delegate (OrangeChangeSet change_set) {
                ShowBubble (change_set.User.Name, FormatMessage (change_set),
                    Program.Controller.GetAvatar (change_set.User.Email, 36));
            };
        }


        public void ShowBubble (string title, string subtext, string image_path)
        {
            if (ShowBubbleEvent != null && Program.Controller.NotificationsEnabled)
                ShowBubbleEvent (title, subtext, image_path);
        }


        public void BubbleClicked ()
        {
            Program.Controller.ShowEventLogWindow ();
        }


        private string FormatMessage (OrangeChangeSet change_set)
        {
            string file_name = "";
            string message   = "";

            if (change_set.Added.Count > 0) {
                file_name = change_set.Added [0];
                message = String.Format ("added ‘{0}’", file_name);
            }

            if (change_set.MovedFrom.Count > 0) {
                file_name = change_set.MovedFrom [0];
                message = String.Format ("moved ‘{0}’", file_name);
            }

            if (change_set.Edited.Count > 0) {
                file_name = change_set.Edited [0];
                message = String.Format ("edited ‘{0}’", file_name);
            }

            if (change_set.Deleted.Count > 0) {
                file_name = change_set.Deleted [0];
                message = String.Format ("deleted ‘{0}’", file_name);
            }

            int changes_count = (change_set.Added.Count +
                                 change_set.Edited.Count +
                                 change_set.Deleted.Count +
                                 change_set.MovedFrom.Count) - 1;

            if (changes_count > 0) {
                string msg = string.Format ("and {0} more", changes_count);
                message += " " + String.Format (msg, changes_count);

            } else if (changes_count < 0) {
                message += "did something magical";
            }

            return message;
        }
    }
}
