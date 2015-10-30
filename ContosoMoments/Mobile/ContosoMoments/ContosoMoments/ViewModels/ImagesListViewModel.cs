using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments.ViewModels
{
    public class ImagesListViewModel : BaseViewModel
    {
        MobileServiceClient _client;

        public ImagesListViewModel(MobileServiceClient client)
        {
            _client = client;
        }

        // View model properties
        private MobileServiceCollection<Image, Image> _Images;
        public MobileServiceCollection<Image, Image> Images
        {
            get { return _Images; }
            set
            {
                _Images = value;
                OnPropertyChanged("Images");
            }
        }

        //private MobileServiceCollection<Album, Album> _Albums;
        //public MobileServiceCollection<Album, Album> Albums
        //{
        //    get { return _Albums; }
        //    set
        //    {
        //        _Albums = value;
        //        OnPropertyChanged("Albums");
        //    }
        //}

        //private MobileServiceCollection<User, User> _Users;
        //public MobileServiceCollection<User, User> Users
        //{
        //    get { return _Users; }
        //    set
        //    {
        //        _Users = value;
        //        OnPropertyChanged("Users");
        //    }
        //}

        private bool _IsPending;
        public bool IsPending
        {
            get { return _IsPending; }
            set
            {
                _IsPending = value;
                OnPropertyChanged("IsPending");
            }
        }

        private string _ErrorMessage = null;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                _ErrorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public async Task GetImagesAsync()
        {
            IsPending = true;
            ErrorMessage = null;

            try
            {
                IMobileServiceTable<Image> table = _client.GetTable<Image>();
                Images = await table.ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (HttpRequestException ex2)
            {
                ErrorMessage = ex2.Message;
            }
            finally
            {
                IsPending = false;
            }
        }
    }
}
