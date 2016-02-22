using Microsoft.WindowsAzure.MobileServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContosoMoments.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected MobileServiceClient _client;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var temp = PropertyChanged;
            if (temp != null)
            {
                temp(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
