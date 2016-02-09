namespace ContosoMoments.Models
{
    public class Image
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string UploadFormat { get; set; }
        public Album Album { get; set; }
        public string AlbumId { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }
    }
}
