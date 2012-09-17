using System;
using System.Windows;
using System.IO.IsolatedStorage;
using System.ComponentModel;

namespace BencinaChile.Utilities
{
    public class AppSettings
    {
        // Our isolated storage settings
        IsolatedStorageSettings settings;

        // The isolated storage key names of our settings
        const string LocationEnabledName = "LocationEnabledSetting";

        // The default value of our settings
        const bool LocationEnabledDefault = true;

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public AppSettings()
        {
            // Get the settings for this application.
            if (!DesignerProperties.IsInDesignTool)
                settings = IsolatedStorageSettings.ApplicationSettings;
        }

        #region general methods

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            settings.Save();
        }

        #endregion
        
        /// <summary>
        /// Property to get and set a Location Enabled setting.
        /// </summary>
        public bool LocationEnabledSetting
        {
            get
            {
                return GetValueOrDefault<bool>(LocationEnabledName, LocationEnabledDefault);
            }
            set
            {
                if (AddOrUpdateValue(LocationEnabledName, value))
                {
                    Save();
                }
            }
        }
    }
}