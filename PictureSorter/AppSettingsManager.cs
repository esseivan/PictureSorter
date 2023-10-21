using ESNLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureSorter
{
    /// <summary>
    /// Management of settings for the app
    /// </summary>
    public class AppSettingsManager
    {
        /// <summary>
        /// Path to the setting file. Is null until <see cref="Load"/> has been called
        /// </summary>
        public static string SettingsPath = null;

        private static AppSettings _instance = null;

        /// <summary>
        /// Get the settings
        /// </summary>
        public static AppSettings Instance
        {
            get
            {
                if (null == _instance)
                {
                    Load();
                }
                return _instance;
            }
        }

        private AppSettingsManager() { }

        /// <summary>
        /// Generate the setting filepaths
        /// </summary>
        public static string GenerateSettingFilePath()
        {
            SettingsManager.MyPublisherName = "ESN";
            SettingsManager.MyAppName = "PictureSorter";

            SettingsPath = SettingsManager.GetDefaultSettingFilePath(false);

            return SettingsPath;
        }

        /// <summary>
        /// Save
        /// </summary>
        public static void Save()
        {
            if (_instance == null)
            {
                _instance = new AppSettings();
            }

            SettingsManager.SaveToDefault(
                _instance,
                SettingsManager.BackupMode.dotBak,
                true,
                false,
                false
            );
        }

        /// <summary>
        /// Load the settings
        /// </summary>
        public static AppSettings Load()
        {
            if (string.IsNullOrEmpty(SettingsPath))
                GenerateSettingFilePath();

            SettingsManager.LoadFromDefault(out AppSettings _settings, false);

            if (_settings == null)
            {
                // New settings
                _instance = new AppSettings();
                Save();
            }
            else
            {
                _instance = _settings;
            }

            return _instance;
        }

        /// <summary>
        /// Reset the settings
        /// </summary>
        public static void Reset()
        {
            _instance = new AppSettings();
            Save();
        }
    }
}
