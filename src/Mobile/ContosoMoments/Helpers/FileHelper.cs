using PCLStorage;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments
{
    public class FileHelper
    {
        public static async Task<string> SaveStreamAsync(string itemId, string filename, System.IO.Stream sourceStream)
        {
            IFolder localStorage = FileSystem.Current.LocalStorage;

            string targetPath = await GetLocalFilePathAsync(itemId, filename);
            var targetFile = await localStorage.CreateFileAsync(targetPath, CreationCollisionOption.ReplaceExisting);

            using (var targetStream = await targetFile.OpenAsync(FileAccess.ReadAndWrite)) {
                await sourceStream.CopyToAsync(targetStream);
            }

            return targetPath;
        }

        public static async Task<string> CopyFileAsync(string itemId, string filePath)
        {
            var sourceFile = await FileSystem.Current.LocalStorage.GetFileAsync(filePath);
            var sourceStream = await sourceFile.OpenAsync(FileAccess.Read);
            string fileName = System.IO.Path.GetFileName(filePath);

            return await SaveStreamAsync(itemId, fileName, sourceStream);
        }

        public static async Task<string> GetLocalFilePathAsync(string itemId, string fileName)
        {
            IPlatform platform = DependencyService.Get<IPlatform>();

            string recordFilesPath = System.IO.Path.Combine(await platform.GetDataFilesPath(), itemId);

            var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(recordFilesPath);
            if (checkExists == ExistenceCheckResult.NotFound) {
                await FileSystem.Current.LocalStorage.CreateFolderAsync(recordFilesPath, CreationCollisionOption.ReplaceExisting);
            }

            return System.IO.Path.Combine(recordFilesPath, fileName);
        }

        public static async Task DeleteLocalFileAsync(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile fileName)
        {
            string localPath = await GetLocalFilePathAsync(fileName.ParentId, fileName.Name);
            var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(localPath);

            if (checkExists == ExistenceCheckResult.FileExists) {
                var file = await FileSystem.Current.LocalStorage.GetFileAsync(localPath);
                await file.DeleteAsync();
            }
        }
    }
}
