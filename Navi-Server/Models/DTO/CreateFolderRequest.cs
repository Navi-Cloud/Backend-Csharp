namespace Navi_Server.Models.DTO
{
    public class CreateFolderRequest
    {
        public string NewFolderName { get; set; }
        public FileMetadata ParentFolderMetadata { get; set; }
    }
}