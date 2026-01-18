using System.Net.Http.Headers;

namespace API.Helpers
{
    public static class FileHelper
    {
        public static UploadedFile Upload(IFormFile file, string toFolderName)
        {
            if (file == null)
                return null;
            
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "uploads", toFolderName);
            
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }


            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            
            string fileExtension = file.FileName.Split('.').Last();
            
            string fileName = Guid.NewGuid().ToString() + "." + fileExtension;
            
            var fullPath = Path.Combine(pathToSave, fileName);
            
            var uploadedFile = new UploadedFile();
            
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);

                uploadedFile.FileName = fileName;
                uploadedFile.OriginalFileName = originalFileName;
                uploadedFile.FilePath = HttpRequestHelper.GetBaseAddress() + "/uploads/" + toFolderName + "/" + fileName;
            }

            return uploadedFile;
        }
    }
    public class UploadedFile
    {
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public string Extension { get; set; }
        public string FileDirectory { get; set; }
        public string FileRootPath => $"{FileDirectory}/{FileName}";
        
    }
}
