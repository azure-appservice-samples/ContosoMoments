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
using System.Diagnostics;

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

        public async Task GetImagesAsync(string albumId)
        {
            IsPending = true;
            ErrorMessage = null;

            try
            {                
                var images = await (App.Current as App).imageTableSync.ToListAsync();
                var res = from image in images
                          where image.AlbumId == albumId
                          select image;

                _Images = res.ToList();
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

        public async Task<bool> DeleteImageAsync(Image selectedImage)
        {
            bool bRes = true; //Assume success

            try
            {
                //await imageTable.DeleteAsync(selectedImage);
                await (App.Current as App).imageTableSync.DeleteAsync(selectedImage);
            }
            catch (Exception ex)
            {
                bRes = false;
            }

            return bRes;
        }

        public async Task<bool> UploadImageAsync(Stream imageStream)
        {
            try {
                var app = App.Current as App;
                var image = new Models.Image 
                {
                    UserId = User.UserId.ToString(),
                    AlbumId = Album.AlbumId,
                    ImageFormat = "Mobile Image",
                    ContainerName = "https://donnamcontosomoments.blob.core.windows.net/" // TODO: fix hardcode
                };

                await app.imageTableSync.InsertAsync(image); // create a new image record
                await app.AddImage(image, imageStream); // add the image file to the record

                return true;
            }
            catch (Exception e) {
                Trace.WriteLine(e);
                return false;
            }
        }

        //public async Task<bool> UploadImageAsync(Stream imageStream)
        //{
        //    bool retVal = false;

        //    try
        //    {
        //        var sasToken = await App.MobileService.InvokeApiAsync<string>("GetSasUrl", HttpMethod.Get, null);
        //        string token = sasToken.Substring(sasToken.IndexOf("?")).TrimEnd('"');
        //        StorageCredentials credentials = new StorageCredentials(token);
        //        string blobUri = sasToken.Substring(0, sasToken.IndexOf("?"));
        //        CloudBlockBlob blobFromSASCredential = new CloudBlockBlob(new System.Uri(blobUri), credentials);
        //        blobFromSASCredential.Properties.ContentType = "image/jpeg";
        //        byte[] bytes = new byte[imageStream.Length];
        //        await imageStream.ReadAsync(bytes, 0, (int)imageStream.Length);
        //        await blobFromSASCredential.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
        //        string json = string.Format("{{\"UserId\":\"{1}\", \"IsMobile\":true, \"AlbumId\":\"{2}\", \"SasUrl\": \"{0}\", \"blobParts\":null }}", sasToken, User.UserId, Album.AlbumId);

        //        Newtonsoft.Json.Linq.JToken body = Newtonsoft.Json.Linq.JToken.Parse(json);

        //        var res = await App.MobileService.InvokeApiAsync<Newtonsoft.Json.Linq.JToken, Newtonsoft.Json.Linq.JObject>("CommitBlob", body, HttpMethod.Post, null);
        //        retVal = (bool)res["success"];
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    return retVal;
        //}
    }
}