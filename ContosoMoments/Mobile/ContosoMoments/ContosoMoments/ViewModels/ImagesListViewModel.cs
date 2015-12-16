using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using System.Linq;

namespace ContosoMoments.ViewModels
{
    public class ImagesListViewModel : BaseViewModel
    {
        public ImagesListViewModel(MobileServiceClient client)
        {
            _client = client;

            //_UserName = "Demo User";
            //_AlbumName = "Demo Album";
        }

        // View model properties
        private /*MobileServiceCollection<Image, Image>*/List<Image> _Images;
        public /*MobileServiceCollection<Image, Image>*/List<Image> Images
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

        private User _user;
        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private Album _album;
        public Album Album
        {
            get { return _album; }
            set
            {
                _album = value;
                OnPropertyChanged("Album");
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


        //public async Task GetUserAsync(Guid userId)
        //{
        //    try
        //    {
        //        var table = _client.GetTable<User>();
        //        var user = await table.LookupAsync(userId);

        //        if (null != user)
        //            User = user;
        //    }
        //    catch (MobileServiceInvalidOperationException ex)
        //    {
        //        ErrorMessage = ex.Message;
        //    }
        //    catch (HttpRequestException ex2)
        //    {
        //        ErrorMessage = ex2.Message;
        //    }
        //}

        //public async Task GetAlbumAsync(string albumId)
        //{
        //    try
        //    {
        //        var table = _client.GetTable<Album>();
        //        var album = await table.LookupAsync(albumId);

        //        if (null != album)
        //            Album = album;
        //    }
        //    catch (MobileServiceInvalidOperationException ex)
        //    {
        //        ErrorMessage = ex.Message;
        //    }
        //    catch (HttpRequestException ex2)
        //    {
        //        ErrorMessage = ex2.Message;
        //    }
        //}

        public async Task GetImagesAsync(string albumId)
        {
            IsPending = true;
            ErrorMessage = null;

            try
            {
                var json = await _client.GetTable<Image>().ReadAsync("$expand=Album");
                var images = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Image>>(json.ToString());
                var res = from image in images
                          where image.Album.AlbumId == albumId
                          select image;

                _Images = res.ToList();
                //IMobileServiceTable<Image> table = _client.GetTable<Image>();
                //Images = await table.ToCollectionAsync();

                var iii = await (App.Current as App).imageTableSync.ToCollectionAsync();
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

        public async Task<bool> UploadImageAsync(Stream imageStream)
        {
            bool retVal = false;

            try
            {
                var sasToken = await App.MobileService.InvokeApiAsync<string>("GetSasUrl", HttpMethod.Get, null);
                string token = sasToken.Substring(sasToken.IndexOf("?")).TrimEnd('"');
                StorageCredentials credentials = new StorageCredentials(token);
                string blobUri = sasToken.Substring(0, sasToken.IndexOf("?"));
                CloudBlockBlob blobFromSASCredential = new CloudBlockBlob(new System.Uri(blobUri), credentials);
                blobFromSASCredential.Properties.ContentType = "image/jpeg";
                byte[] bytes = new byte[imageStream.Length];
                await imageStream.ReadAsync(bytes, 0, (int)imageStream.Length);
                await blobFromSASCredential.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                string json = string.Format("{{\"UserId\":\"{1}\", \"IsMobile\":true, \"AlbumId\":\"{2}\", \"SasUrl\": \"{0}\", \"blobParts\":null }}", sasToken, User.UserId, Album.AlbumId);

                Newtonsoft.Json.Linq.JToken body = Newtonsoft.Json.Linq.JToken.Parse(json);

                var res = await App.MobileService.InvokeApiAsync<Newtonsoft.Json.Linq.JToken, Newtonsoft.Json.Linq.JObject>("CommitBlob", body, HttpMethod.Post, null);
                retVal = (bool)res["success"];
            }
            catch (Exception ex)
            {
            }

            return retVal;
        }
    }
}
