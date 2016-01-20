using ContosoMoments.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ContosoMoments.Settings
{
    public class Settings : ISettings
    {
        private static ApplicationDataContainer AppSettings
        {
            get { return ApplicationData.Current.LocalSettings; }
        }

        private readonly object locker = new object();

        public bool AddOrUpdateValue<T>(string key, T value)
        {
            return InternalAddOrUpdateValue(key, value);
        }

        public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            object value;
            lock (locker)
            {
                if (typeof(T) == typeof(decimal))
                {
                    string savedDecimal;
                    // If the key exists, retrieve the value.
                    if (AppSettings.Values.ContainsKey(key))
                    {
                        savedDecimal = Convert.ToString(AppSettings.Values[key]);
                    }
                    // Otherwise, use the default value.
                    else
                    {
                        savedDecimal = defaultValue == null ? default(decimal).ToString() : defaultValue.ToString();
                    }

                    value = Convert.ToDecimal(savedDecimal, System.Globalization.CultureInfo.InvariantCulture);

                    return null != value ? (T)value : defaultValue;
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    string savedTime = null;
                    // If the key exists, retrieve the value.
                    if (AppSettings.Values.ContainsKey(key))
                    {
                        savedTime = Convert.ToString(AppSettings.Values[key]);
                    }

                    var ticks = string.IsNullOrWhiteSpace(savedTime) ? -1 : Convert.ToInt64(savedTime, System.Globalization.CultureInfo.InvariantCulture);
                    if (ticks == -1)
                        value = defaultValue;
                    else
                        value = new DateTime(ticks);

                    return null != value ? (T)value : defaultValue;
                }

                // If the key exists, retrieve the value.
                if (AppSettings.Values.ContainsKey(key))
                {
                    var tempValue = AppSettings.Values[key];
                    if (tempValue != null)
                        value = (T)tempValue;
                    else
                        value = defaultValue;
                }
                // Otherwise, use the default value.
                else
                {
                    value = defaultValue;
                }
            }

            return null != value ? (T)value : defaultValue;
        }

        public void Remove(string key)
        {
            lock (locker)
            {
                // If the key exists remove
                if (AppSettings.Values.ContainsKey(key))
                {
                    AppSettings.Values.Remove(key);
                }
            }
        }

        private bool InternalAddOrUpdateValue(string key, object value)
        {
            bool valueChanged = false;
            lock (locker)
            {

                if (value is decimal)
                {
                    return AddOrUpdateValue(key, Convert.ToString(Convert.ToDecimal(value), System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (value is DateTime)
                {
                    return AddOrUpdateValue(key, Convert.ToString((Convert.ToDateTime(value)).Ticks, System.Globalization.CultureInfo.InvariantCulture));
                }


                // If the key exists
                if (AppSettings.Values.ContainsKey(key))
                {

                    // If the value has changed
                    if (AppSettings.Values[key] != value)
                    {
                        // Store key new value
                        AppSettings.Values[key] = value;
                        valueChanged = true;
                    }
                }
                // Otherwise create the key.
                else
                {
                    AppSettings.CreateContainer(key, ApplicationDataCreateDisposition.Always);
                    AppSettings.Values[key] = value;
                    valueChanged = true;
                }
            }

            return valueChanged;
        }
    }
}
