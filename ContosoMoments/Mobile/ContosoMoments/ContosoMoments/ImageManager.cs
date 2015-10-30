using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments
{
    public class EntitiesManager
    {
        // Azure
        IMobileServiceSyncTable<Image> imageTable;
        IMobileServiceSyncTable<Album> albumTable;
        IMobileServiceSyncTable<User> userTable;

        MobileServiceClient client;
        IDisposable eventSubscription;

        public EntitiesManager()
        {
            client = new MobileServiceClient(Constants.ApplicationURL, Constants.GatewayURL, Constants.ApplicationKey, new LoggingHandler(false));

            var store = new MobileServiceSQLiteStore("localstore.db");
            store.DefineTable<Album>();
            store.DefineTable<User>();
            store.DefineTable<Image>();

            this.client.SyncContext.InitializeAsync(store);

            imageTable = client.GetSyncTable<Image>();
            albumTable = client.GetSyncTable<Album>();
            userTable = client.GetSyncTable<User>();

            eventSubscription = this.client.EventManager.Subscribe<IMobileServiceEvent>(GeneralEventHandler);
        }

        private void GeneralEventHandler(IMobileServiceEvent mobileServiceEvent)
        {
            Debug.WriteLine("Event handled: " + mobileServiceEvent.Name);
        }

        public async Task SyncImagesAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                // FILES: Push file changes
                //await this.imageTable.PushFileChangesAsync();

                // FILES: Automatic pull
                // A normal pull will automatically process new/modified/deleted files, engaging the file sync handler
                await this.imageTable.PullAsync("Image", this.imageTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }
                }
            }
        }

        public async Task SyncAlbumsAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                // FILES: Push file changes
                await this.albumTable.PushFileChangesAsync();

                // FILES: Automatic pull
                // A normal pull will automatically process new/modified/deleted files, engaging the file sync handler
                await this.albumTable.PullAsync("Album", this.albumTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }
                }
            }
        }

        public async Task SyncUsersAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                // FILES: Push file changes
                await this.userTable.PushFileChangesAsync();

                // FILES: Automatic pull
                // A normal pull will automatically process new/modified/deleted files, engaging the file sync handler
                await this.userTable.PullAsync("User", this.userTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }
                }
            }
        }

        public async Task<IEnumerable<Image>> GetImagesAsync()
        {
            try
            {
                return await imageTable.ReadAsync();
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            try
            {
                return await userTable.ReadAsync();
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync()
        {
            try
            {
                return await albumTable.ReadAsync();
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }

        public async Task SaveImageAsync(Image image)
        {
            if (image.ImageId == null)
            {
                image.ImageId = new Guid();

                await imageTable.InsertAsync(image);
            }
            else
                await imageTable.UpdateAsync(image);
        }

        public async Task SaveUserAsync(User user)
        {
            if (user.UserId == null)
            {
                user.UserId = new Guid();
                await userTable.InsertAsync(user);
            }
            else
                await userTable.UpdateAsync(user);
        }

        public async Task SaveAlbumAsync(Album album)
        {
            if (album.AlbumId == null)
            {
                album.AlbumId = new Guid();
                await albumTable.InsertAsync(album);
            }
            else
                await albumTable.UpdateAsync(album);
        }

        public async Task DeleteImageAsync(Image image)
        {
            try
            {
                await imageTable.DeleteAsync(image);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
        }

        public async Task DeleteAlbumAsync(Album album)
        {
            try
            {
                await albumTable.DeleteAsync(album);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
        }

        public async Task DeleteUserAsync(User user)
        {
            try
            {
                await userTable.DeleteAsync(user);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
        }
    }
}
