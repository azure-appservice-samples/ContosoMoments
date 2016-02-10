using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace ContosoMoments.ViewModels
{
    public class AlbumsListViewModel : BaseViewModel
    {
        App _app;

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
                OnPropertyChanged("ErrorMessage");
            }
        }

        private List<Album> _Albums;
        public List<Album> Albums
        {
            get { return _Albums; }
            set
            {
                _Albums = value;
                OnPropertyChanged("Albums");
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

        public async Task GetUserAsync(Guid userId)
        {
            var user = await _app.userTableSync.LookupAsync(userId.ToString());

            if (user != null)
                this.User = user;
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

        public async Task GetAlbumsAsync()
        {
            _Albums =
                await _app.albumTableSync
                .Where(a => a.UserId == User.UserId.ToString() || a.IsDefault)
                .ToListAsync();
        }

        public async Task AddNewAlbumAsync(string albumName)
        {
            var album = new Album() { AlbumName = albumName, IsDefault = false, UserId = User.UserId.ToString() };
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
