using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace ContosoMoments.ViewModels
{
    public class AlbumsListViewModel : BaseViewModel
    {
        App _app;
        string _userEmail;       

        public AlbumsListViewModel(MobileServiceClient client, App app)
        {
            _app = app;
            _client = client;
        }

        private string _ErrorMessage = null;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                _ErrorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private List<Album> _Albums;
        public List<Album> Albums
        {
            get { return _Albums; }
            set
            {
                _Albums = value;
                OnPropertyChanged(nameof(Albums));
            }
        }

        public string UserEmail
        {
            get { return _userEmail; }
            set
            {
                _userEmail = value;
                OnPropertyChanged(nameof(UserEmail));
            }
        }

        public async Task CheckUpdateNotificationRegistrationAsync(string userId)
        {
#if !__WP__
            string installationId = App.MobileService.GetPush().InstallationId;
#elif (__WP__ && DEBUG)
            string installationId = "8a526c49-b824-4a81-8f27-dce0e383e850";
#endif

#if (!__WP__) || (__WP__ && DEBUG)
            string json = string.Format("{{\"InstallationId\":\"{0}\", \"UserId\":\"{1}\"}}", installationId, userId);
            Newtonsoft.Json.Linq.JToken body = Newtonsoft.Json.Linq.JToken.Parse(json);

            await App.MobileService.InvokeApiAsync("PushRegistration", body, HttpMethod.Post, null);
#endif
        }

        public async Task GetAlbumsAsync(string userId)
        {
            _Albums =
                await _app.albumTableSync
                .Where(a => a.UserId == userId || a.IsDefault)
                .ToListAsync();
        }

        public async Task AddNewAlbumAsync(string albumName)
            {
            var album = new Album() { AlbumName = albumName, IsDefault = false, UserId = App.Instance.CurrentUserId };
            await _app.albumTableSync.InsertAsync(album);
        }

        public async Task DeleteAlbumAsync(Album selectedAlbum)
            {
            await _app.albumTableSync.DeleteAsync(selectedAlbum);
        }

        public async Task UpdateAlbumAsync(Album selectedAlbum)
            {
            await _app.albumTableSync.UpdateAsync(selectedAlbum);
        }
    }
}
