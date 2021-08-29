using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Navi_Server.Exchange;
using Navi_Server.Models;
using Navi_Server.Repositories;

namespace Navi_Server.Services
{
    public interface IFileService
    {
        /// <summary>
        /// <para>Get All File Information under folderTarget.</para>
        /// <para>See <see cref="IFileRepository.ExploreFolder"/> for more details.</para>
        /// </summary>
        /// <param name="folderTarget">Folder to explore</param>
        /// <param name="userEmail">Owner</param>
        /// <returns>File List, might be empty.</returns>
        public Task<ExecutionResult<List<FileMetadata>>> ExploreFolder(string folderTarget, string userEmail);
        
        /// <summary>
        /// Create Folder on parentFolder
        /// </summary>
        /// <param name="parentFolder">Parent folder of newly-created folder.</param>
        /// <param name="folderName">New Folder Name</param>
        /// <param name="userMail">Owner</param>
        /// <returns>
        /// <para>Execution Result of 'Creation of Folder'</para>
        /// <para>Returns Created Metadata if succeeds.</para>
        /// <para>Returns DuplicatedId Type if folder already exists.</para>
        /// </returns>
        public Task<ExecutionResult<FileMetadata>> CreateFolderAsync(string parentFolder, string folderName,
            string userMail);
        
        /// <summary>
        /// Upload File to Server. It will create new metadata for new file.
        /// </summary>
        /// <param name="parentMetadata">Parent[Folder] Metadata.</param>
        /// <param name="fileName">A File name[upload]</param>
        /// <param name="fileStream">A Non-Disposed File Stream</param>
        /// <param name="userEmail">User Email[Owner]</param>
        /// <returns>
        /// <para>Execution Result of 'Creation of File' => New-Created Metadata.</para>
        /// <para>Returns Created Metadata if succeeds.</para>
        /// <para>Returns DuplicatedId type if folder already exists.</para>
        /// </returns>
        public Task<ExecutionResult<FileMetadata>> UploadFileAsync(FileMetadata parentMetadata, string fileName, MemoryStream fileStream, string userEmail);
        
        /// <summary>
        /// Get Filename and MemoryStream from DB Server[Download]
        /// </summary>
        /// <param name="targetMetadata">Target File to download</param>
        /// <returns>
        /// <para>Success with filename and un-closed memory stream when succeeds.</para>
        /// <para>Not-Found when given metadata is not found.</para>
        /// </returns>
        public Task<ExecutionResult<(string, MemoryStream)>> DownloadFileAsync(FileMetadata targetMetadata);
        
        /// <summary>
        /// Remove File from db.
        /// </summary>
        /// <param name="targetMetadata">Target File to delete</param>
        /// <returns>Deleted File Metadata, or Not-Found when metadata is not found.</returns>
        public Task<ExecutionResult<FileMetadata>> DeleteFileAsync(FileMetadata targetMetadata);
    }
}