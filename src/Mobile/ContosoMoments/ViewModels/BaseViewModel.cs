using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

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
