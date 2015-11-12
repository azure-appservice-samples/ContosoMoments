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

        private string _UserName;
        public string UserName
        {
            get { return _UserName; }
            set
            {
                _UserName = value;
                OnPropertyChanged("UserName");
            }
        }

        private string _AlbumName;
        public string AlbumName
        {
            get { return _AlbumName; }
            set
            {
                _AlbumName = value;
                OnPropertyChanged("AlbumName");
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


        public async Task GetUserAsync(Guid userId)
        {
            try
            {
                var table = _client.GetTable<User>();
                var user = await table.LookupAsync(userId);

                if (null != user)
                    UserName = user.UserName;
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (HttpRequestException ex2)
            {
                ErrorMessage = ex2.Message;
            }
        }

        public async Task GetAlbumAsync(Guid albumId)
        {
            try
            {
                var table = _client.GetTable<Album>();
                var album = await table.LookupAsync(albumId);

                if (null != album)
                    AlbumName = album.AlbumName;
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (HttpRequestException ex2)
            {
                ErrorMessage = ex2.Message;
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

        public async Task<bool> UploadImageAsync(Stream imageStream)
        {
            bool retVal = false;

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(Constants.ApplicationURL + "api/GetSasUrl"));
                request.Method = "GET";

                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        byte[] bytes = new byte[stream.Length];
                        await stream.ReadAsync(bytes, 0, (int)stream.Length);

                        var sasToken = System.Text.Encoding.UTF8.GetString(bytes, 0, (int)stream.Length);
                        string token = sasToken.Substring(sasToken.IndexOf("?")).TrimEnd('"');
                        StorageCredentials credentials = new StorageCredentials(token);
                        string blobUri = sasToken.Substring(1, sasToken.IndexOf("?"));
                        CloudBlockBlob blobFromSASCredential = new CloudBlockBlob(new System.Uri(blobUri), credentials);
                        bytes = new byte[imageStream.Length];
                        await imageStream.ReadAsync(bytes, 0, (int)imageStream.Length);
                        await blobFromSASCredential.UploadFromByteArrayAsync(bytes, 0, bytes.Length);

                        HttpWebRequest postRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(Constants.ApplicationURL + "api/CommitBlob"));
                        postRequest.ContentType = "application/json";
                        postRequest.Method = "POST";

                        string json = string.Format("{{\"UserId\":null, \"IsMobile\":true, \"AlbumId\":null, \"SasUrl\": {0}, \"blobParts\":null }}", sasToken);

                        using (var streamWriter = new StreamWriter(await postRequest.GetRequestStreamAsync()))
                        {
                            streamWriter.Write(json);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }

                        var httpResponse = (HttpWebResponse)await postRequest.GetResponseAsync();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            bool b;
                            if (!bool.TryParse(result, out b))
                                retVal = b;
                            else
                                retVal = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }


            return retVal;
        }
    }
}
