using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Navi_Server.Models;

namespace Navi_Server.Repositories
{
    public interface IFileRepository
    {
        /// <summary>
        /// Get All file information under parentFolder.
        /// </summary>
        /// <param name="parentFolder">To explore.</param>
        /// <param name="userEmail">File Owner</param>
        /// <returns>File List. Might be empty.</returns>
        public Task<List<FileMetadata>> ExploreFolder(string parentFolder, string userEmail);
        
        /// <summary>
        /// Upload File to server.
        /// </summary>
        /// <param name="metadata">File metadata to upload.</param>
        /// <param name="fileStream">File contents as stream.</param>
        /// <returns>
        /// <para>Metadata itself when uploading succeed.</para>
        /// </returns>
        /// <exception cref="MongoWriteException">When user tries to upload duplicated file.</exception>
        public Task<FileMetadata> UploadFileAsync(FileMetadata metadata, Stream fileStream);
        
        /// <summary>
        /// <para>Download file based on metadata.[Will download file corresponding to metadata]</para>
        /// <para>NOTE THAT THIS FUNCTION WILL NOT CLOSE MEMORYSTREAM!!!</para>
        /// </summary>
        /// <param name="metadata">Metadata of file to download.</param>
        /// <returns>MemoryStream for holding file contents.</returns>
        /// <exception cref="InvalidOperationException">When given metdata entity is not found.</exception>
        public Task<(string, MemoryStream)> DownloadFileAsync(FileMetadata metadata);
        
        /// <summary>
        /// Create Logical Folder to server.
        /// </summary>
        /// <param name="metadata">Folder Metadata created from service.</param>
        /// <returns>Created Folder Metadata.</returns>
        /// <exception cref="MongoWriteExcption">When user tries to upload duplicated file.</exception>
        public Task<FileMetadata> CreateFolderAsync(FileMetadata metadata);
        
        /// <summary>
        /// Delete File from server.
        /// </summary>
        /// <param name="metadata">File to remove.</param>
        /// <returns>
        /// <para>Deleted File information if succeeds.</para>
        /// </returns>
        /// <exception cref="InvalidOperationException">When given metadata entity is not found.</exception>
        public Task<FileMetadata> DeleteFileAsync(FileMetadata metadata);
        
        /// <summary>
        /// Copy file from to another directory
        /// </summary>
        /// <param name="originalFile">Original File</param>
        /// <param name="destinationFile">Destination Metadata[should be built]</param>
        /// <returns>Copied File Metadata</returns>
        /// <exception cref="MongoWriteException">When user tries to copy 'duplicated' file. - meaning same name exists.</exception>
        /// <exception cref="InvalidOperationException">When given metadata entity is not found.</exception>
        public Task<FileMetadata> CopyFileAsync(FileMetadata originalFile, FileMetadata destinationFile);
    }
}