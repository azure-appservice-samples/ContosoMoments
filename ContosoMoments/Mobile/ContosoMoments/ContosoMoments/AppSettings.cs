using ContosoMoments.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoMoments.Settings
{
    public static class AppSettings
    {
        static Lazy<ISettings> settings = new Lazy<ISettings>(() => CreateSettings(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static ISettings Current
        {
            get
            {
                ISettings ret = settings.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        static ISettings CreateSettings()
        {
            return new Settings();
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the plat-specific version of this assembly.");
        }
    }
}
