using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantCompanionApp.Models
{
    public class Settings
    {
        public IsolatedStorageSettings settings;
        const string CustomerEmailKeyName = "UsernameSetting";
        const string PasswordSettingKeyName = "PasswordSetting";

        const string CustomerEmailSettingDefault = "";
        const string PasswordSettingDefault = "";
        public Settings()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
        }

        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;
            if (settings.Contains(Key))
            {
                if (settings[Key] != value)
                {
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            else
            {
                settings.Add(Key, value);
                value = true;
            }
            return valueChanged;
        }

        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        public void Save()
        {
            settings.Save();
        }

        public string CustomerEmail
        {
            get
            {
                return GetValueOrDefault<string>(CustomerEmailKeyName, CustomerEmail);
            }
            set
            {
                if (AddOrUpdateValue(CustomerEmailKeyName, value))
                {
                    Save();
                }
            }
        }
        public string Password
        {
            get
            {
                return GetValueOrDefault<string>(PasswordSettingKeyName, Password);
            }
            set
            {
                if (AddOrUpdateValue(PasswordSettingKeyName, value))
                {
                    Save();
                }
            }
        }
    }
}
