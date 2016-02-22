using Android.App;
using Android.Content;
using Android.Preferences;
using ContosoMoments.Models;
using System;

namespace ContosoMoments.Settings
{
    public class Settings : ISettings
    {
        private readonly object locker = new object();

        public bool AddOrUpdateValue<T>(string key, T value)
        {
            Type typeOf = typeof(T);
            if (typeOf.IsGenericType && typeOf.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                typeOf = Nullable.GetUnderlyingType(typeOf);
            }
            var typeCode = Type.GetTypeCode(typeOf);
            return AddOrUpdateValue(key, value, typeCode);
        }

        public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            lock (locker)
            {
                using (var sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
                {
                    return GetValueOrDefaultCore(sharedPreferences, key, defaultValue);
                }
            }
        }

        public void Remove(string key)
        {
            lock (locker)
            {
                using (var sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
                {
                    using (var sharedPreferencesEditor = sharedPreferences.Edit())
                    {
                        sharedPreferencesEditor.Remove(key);
                        sharedPreferencesEditor.Commit();
                    }
                }
            }
        }

        private T GetValueOrDefaultCore<T>(ISharedPreferences sharedPreferences, string key, T defaultValue)
        {
            Type typeOf = typeof(T);
            if (typeOf.IsGenericType && typeOf.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                typeOf = Nullable.GetUnderlyingType(typeOf);
            }

            object value = null;
            var typeCode = Type.GetTypeCode(typeOf);
            bool resave = false;
            switch (typeCode)
            {
                case TypeCode.Decimal:
                    //Android doesn't have decimal in shared prefs so get string and convert
                    var savedDecimal = string.Empty;
                    try
                    {
                        savedDecimal = sharedPreferences.GetString(key, string.Empty);
                    }
                    catch (Java.Lang.ClassCastException cce)
                    {
                        Console.WriteLine("Settings 1.5 change, have to remove key.");

                        try
                        {
                            Console.WriteLine("Attempting to get old value.");
                            savedDecimal =
                                sharedPreferences.GetLong(key,
                                    (long)Convert.ToDecimal(defaultValue, System.Globalization.CultureInfo.InvariantCulture))
                                    .ToString();
                            Console.WriteLine("Old value has been parsed and will be updated and saved.");
                        }
                        catch (Java.Lang.ClassCastException cce2)
                        {
                            Console.WriteLine("Could not parse old value, will be lost.");
                        }
                        Remove("key");
                        resave = true;
                    }
                    if (string.IsNullOrWhiteSpace(savedDecimal))
                        value = Convert.ToDecimal(defaultValue, System.Globalization.CultureInfo.InvariantCulture);
                    else
                        value = Convert.ToDecimal(savedDecimal, System.Globalization.CultureInfo.InvariantCulture);

                    if (resave)
                        AddOrUpdateValue(key, value);

                    break;
                case TypeCode.Boolean:
                    value = sharedPreferences.GetBoolean(key, Convert.ToBoolean(defaultValue));
                    break;
                case TypeCode.Int64:
                    value =
                        (Int64)
                            sharedPreferences.GetLong(key,
                                (long)Convert.ToInt64(defaultValue, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case TypeCode.String:
                    var val = sharedPreferences.GetString(key, Convert.ToString(defaultValue));
                    if (val.Length == 0)
                        value = defaultValue;
                    else
                        value = val;
                    break;
                case TypeCode.Double:
                    //Android doesn't have double, so must get as string and parse.
                    var savedDouble = string.Empty;
                    try
                    {
                        savedDouble = sharedPreferences.GetString(key, string.Empty);
                    }
                    catch (Java.Lang.ClassCastException cce)
                    {
                        Console.WriteLine("Settings 1.5  change, have to remove key.");

                        try
                        {
                            Console.WriteLine("Attempting to get old value.");
                            savedDouble =
                                sharedPreferences.GetLong(key,
                                    (long)Convert.ToDouble(defaultValue, System.Globalization.CultureInfo.InvariantCulture))
                                    .ToString();
                            Console.WriteLine("Old value has been parsed and will be updated and saved.");
                        }
                        catch (Java.Lang.ClassCastException cce2)
                        {
                            Console.WriteLine("Could not parse old value, will be lost.");
                        }
                        Remove(key);
                        resave = true;
                    }
                    if (string.IsNullOrWhiteSpace(savedDouble))
                        value = Convert.ToDouble(defaultValue, System.Globalization.CultureInfo.InvariantCulture);
                    else
                        value = Convert.ToDouble(savedDouble, System.Globalization.CultureInfo.InvariantCulture);

                    if (resave)
                        AddOrUpdateValue(key, value);
                    break;
                case TypeCode.Int32:
                    value = sharedPreferences.GetInt(key,
                        Convert.ToInt32(defaultValue, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Single:
                    value = sharedPreferences.GetFloat(key,
                        Convert.ToSingle(defaultValue, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case TypeCode.DateTime:
                    var ticks = sharedPreferences.GetLong(key, -1);
                    if (ticks == -1)
                        value = defaultValue;
                    else
                        value = new DateTime(ticks);
                    break;
                default:

                    if (defaultValue is Guid)
                    {
                        var outGuid = Guid.Empty;
                        Guid.TryParse(sharedPreferences.GetString(key, Guid.Empty.ToString()), out outGuid);
                        value = outGuid;
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Value of type {0} is not supported.", value.GetType().Name));
                    }

                    break;
            }

            return null != value ? (T)value : defaultValue;
        }

        private bool AddOrUpdateValue(string key, object value, TypeCode typeCode)
        {
            lock (locker)
            {
                using (var sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
                {
                    using (var sharedPreferencesEditor = sharedPreferences.Edit())
                    {
                        switch (typeCode)
                        {
                            case TypeCode.Decimal:
                                sharedPreferencesEditor.PutString(key,
                                    Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                                break;
                            case TypeCode.Boolean:
                                sharedPreferencesEditor.PutBoolean(key, Convert.ToBoolean(value));
                                break;
                            case TypeCode.Int64:
                                sharedPreferencesEditor.PutLong(key,
                                    (long)Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture));
                                break;
                            case TypeCode.String:
                                sharedPreferencesEditor.PutString(key, Convert.ToString(value));
                                break;
                            case TypeCode.Double:
                                sharedPreferencesEditor.PutString(key,
                                    Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                                break;
                            case TypeCode.Int32:
                                sharedPreferencesEditor.PutInt(key,
                                    Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture));
                                break;
                            case TypeCode.Single:
                                sharedPreferencesEditor.PutFloat(key,
                                    Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture));
                                break;
                            case TypeCode.DateTime:
                                sharedPreferencesEditor.PutLong(key, (Convert.ToDateTime(value)).Ticks);
                                break;
                            default:
                                if (value is Guid)
                                {
                                    sharedPreferencesEditor.PutString(key, ((Guid)value).ToString());
                                }
                                else
                                {
                                    throw new ArgumentException(string.Format("Value of type {0} is not supported.",
                                        value.GetType().Name));
                                }
                                break;
                        }

                        sharedPreferencesEditor.Commit();
                    }
                }
            }

            return true;
        }
    }
}