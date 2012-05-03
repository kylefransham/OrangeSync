﻿
// <autogenerated>
// This code was generated by a tool. Any changes made manually will be lost
// the next time this code is regenerated.
// </autogenerated>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace OrangeShare
{
    public class Strings
    {
        private static Object resourceManLock = new Object();
        private static System.Resources.ResourceManager resourceMan;
        private static System.Globalization.CultureInfo resourceCulture;

        public const string ResourceName = "Strings";

        private static string resourcesDir = GetSetting("ResourcesDir", "po");
        private static string fileFormat = GetSetting("ResourcesFileFormat", "{{culture}}.po");
        
        private static string GetSetting(string setting, string defaultValue)
        {
            var section = (System.Collections.Specialized.NameValueCollection)System.Configuration.ConfigurationManager.GetSection("appSettings");
            if (section == null) return defaultValue;
            else return section[setting] ?? defaultValue;
        }

        
        /// <summary>
        /// Resources directory used to retrieve files from.
        /// </summary>
        public static string ResourcesDirectory
        {
            get { return resourcesDir; }
            set { resourcesDir = value; }
        }

        /// <summary>
        /// Format of the file based on culture and resource name.
        /// </summary>
        public static string FileFormat
        {
            get { return fileFormat; }
            set { fileFormat = value; }
        }


        /// <summary>
        /// Returns the cached ResourceManager instance used by this class.
        /// </summary>
        public static System.Resources.ResourceManager ResourceManager
        {
            get
            {
            
                if (object.ReferenceEquals(resourceMan, null))
                {
                    lock (resourceManLock) 
                    {
                        if (object.ReferenceEquals(resourceMan, null))
                        {
                            var directory = resourcesDir;
                                var mgr = new global::Gettext.Cs.GettextResourceManager(ResourceName, directory, fileFormat);
                                resourceMan = mgr;
                        }
                    }
                }
                
                return resourceMan;
            }
        }

        /// <summary>
        /// Overrides the current thread's CurrentUICulture property for all
        /// resource lookups using this strongly typed resource class.
        /// </summary>
        public static System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        /// <summary>
        /// Looks up a localized string; used to mark string for translation as well.
        /// </summary>
        public static string T(string t)
        {
            return T(null, t);
        }

        /// <summary>
        /// Looks up a localized string; used to mark string for translation as well.
        /// </summary>
        public static string T(CultureInfo info, string t)
        {
            if (String.IsNullOrEmpty(t)) return t;
            var translated = ResourceManager.GetString(t, info ?? resourceCulture);
            return String.IsNullOrEmpty(translated) ? t : translated;
        }

        /// <summary>
        /// Looks up a localized string and formats it with the parameters provided; used to mark string for translation as well.
        /// </summary>
        public static string T(string t, params object[] parameters)
        {
            return T(null, t, parameters);
        }

        /// <summary>
        /// Looks up a localized string and formats it with the parameters provided; used to mark string for translation as well.
        /// </summary>
        public static string T(CultureInfo info, string t, params object[] parameters)
        {
            if (String.IsNullOrEmpty(t)) return t;
            return String.Format(T(info, t), parameters);
        }

        /// <summary>
        /// Marks a string for future translation, does not translate it now.
        /// </summary>
        public static string M(string t)
        {
            return t;
        }
        
        /// <summary>
        /// Returns the resource set available for the specified culture.
        /// </summary>
        public static System.Resources.ResourceSet GetResourceSet(CultureInfo culture)
        {
            return ResourceManager.GetResourceSet(culture, true, true);
        }
    }
}
 

