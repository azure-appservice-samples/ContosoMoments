using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoMoments.Models
{
    public interface ISettings
    {
        T GetValueOrDefault<T>(string key, T defaultValue = default(T));

        bool AddOrUpdateValue<T>(string key, T value);

        void Remove(string key);
    }
}
