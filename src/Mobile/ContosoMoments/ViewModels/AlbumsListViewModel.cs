using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace ContosoMoments.ViewModels
{
    public class AlbumsListViewModel : BaseViewModel
    {
        public AlbumsListViewModel(MobileServiceClient client, App app)
        {
            this.app = app;
            _client = client;

            RenameCommand = new DelegateCommand(OnStartAlbumRename, IsRenameAndDeleteEnabled);
            DeleteCommand = new DelegateCommand(OnDeleteAlbum, IsRenameAndDeleteEnabled);
        }

        #region Properties
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

        public string ErrorMessageTitle { get; set; }

        App app;
        private Album currentAlbumEdit;

        private string editedName;

        public string EditedAlbumName
        {
            get { return editedName; }
            set
            {
                editedName = value;
                OnPropertyChanged(nameof(EditedAlbumName));
            }
        }

        private bool showCancelButton;

        public bool ShowCancelButton
        {
            get { return showCancelButton && showInputControl; }
            set
            {
                showCancelButton = value;
                OnPropertyChanged(nameof(ShowCancelButton));
            }
        }

        public string CreateOrUpdateButtonText
        {
            get { return isRename ? "Rename" : "Add"; }
        }

        private bool isRename;

        public bool IsRename
        {
            get { return isRename; }
            set
            {
                isRename = value;
                OnPropertyChanged(nameof(IsRename));
                OnPropertyChanged(nameof(CreateOrUpdateButtonText));
            }
        }

        private bool showInputControl;

        public bool ShowInputControl
        {
            get { return showInputControl; }
            set
            {
                showInputControl = value;
                OnPropertyChanged(nameof(ShowInputControl));
                OnPropertyChanged(nameof(ShowCancelButton));
            }
        }

        private List<Album> albums;
        public List<Album> Albums
        {
            get { return albums; }
            set
            {
                albums = value;
                OnPropertyChanged(nameof(Albums));
            }
        }

        public ICommand RenameCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        // called when the album delete button is clicked
        public Action<Album> DeleteAlbumViewAction { get; set; }

        #endregion

        public async Task CheckUpdateNotificationRegistrationAsync(string userId)
        {
#if !__WP__
            string installationId = App.Instance.MobileService.GetPush().InstallationId;
#elif (__WP__ && DEBUG)
            string installationId = "8a526c49-b824-4a81-8f27-dce0e383e850";
#endif

#if (!__WP__) || (__WP__ && DEBUG)
            var jsonRequest = new JObject();
            jsonRequest["InstallationId"] = installationId;
            jsonRequest["UserId"] = userId;

            await App.Instance.MobileService.InvokeApiAsync("PushRegistration", jsonRequest, HttpMethod.Post, null);
#endif
        }

        public async Task GetAlbumsAsync(string userId)
        {
            Albums =
                await app.albumTableSync
                .Where(a => a.UserId == userId || a.IsDefault)
                .ToListAsync();
        }

        #region UI Actions
        private void OnStartAlbumRename(object obj)
        {
            var selectedAlbum = obj as Album;
            Debug.WriteLine($"Selected album: {selectedAlbum?.AlbumName}");

            if (selectedAlbum != null) {
                currentAlbumEdit = selectedAlbum;
                IsRename = true;
                EditedAlbumName = selectedAlbum.AlbumName;
                ShowInputControl = true;
                ShowCancelButton = true;
            }
        }

        public async Task DeleteAlbumAsync(Album selectedAlbum)
        {
            await app.albumTableSync.DeleteAsync(selectedAlbum);
        }

        private void OnDeleteAlbum(object obj)
        {
            var selectedAlbum = obj as Album;
            DeleteAlbumViewAction?.Invoke(selectedAlbum);
        }

        public async Task<bool> CreateOrRenameAlbum()
        {
            if (currentAlbumEdit == null || EditedAlbumName.Length > 0) {
                ShowInputControl = false;

                if (IsRename) {
                    currentAlbumEdit.AlbumName = EditedAlbumName;
                    await app.albumTableSync.UpdateAsync(currentAlbumEdit);
                }
                else {
                    await CreateAlbumAsync();
                }

                return true;
            }

            return false;
        }

        private async Task CreateAlbumAsync()
        {
            var album = new Album() 
            {
                AlbumName = EditedAlbumName,
                IsDefault = false,
                UserId = App.Instance.CurrentUserId
            };

            await app.albumTableSync.InsertAsync(album);
        }

        public void OnAdd(object sender, EventArgs e)
        {
            ShowInputControl = !ShowInputControl;
            IsRename = false;
            ShowCancelButton = false;
        }

        #endregion

        #region Helpers

        // return true if the rename and delete commands are available
        internal static bool IsRenameAndDeleteEnabled(object input)
        {
            var album = input as Album;
            var isDefaultAlbum = album != null ? album.IsDefault : false;  // default album can't be renamed

            return !isDefaultAlbum && Settings.Current.AuthenticationType != Settings.AuthOption.GuestAccess;
        }

        #endregion
    }

}

