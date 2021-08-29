using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Navi_Server.Exchange;
using Navi_Server.Models;
using Navi_Server.Repositories;

namespace Navi_Server.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;

        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }
        
        /// <summary>
        /// See <see cref="IFileService.ExploreFolder"/> for more details.
        /// </summary>
        /// <param name="folderTarget"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<ExecutionResult<List<FileMetadata>>> ExploreFolder(string folderTarget, string userEmail)
        {
            return new ExecutionResult<List<FileMetadata>>
            {
                ResultType = ExecutionResultType.SUCCESS,
                Value = await _fileRepository.ExploreFolder(folderTarget, userEmail)
            };
        }
        
        /// <summary>
        /// See <see cref="IFileService.CreateFolderAsync"/> for more details.
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="folderName"></param>
        /// <param name="userMail"></param>
        /// <returns></returns>
        public async Task<ExecutionResult<FileMetadata>> CreateFolderAsync(string parentFolder, string folderName, string userMail)
        {
            // Append Path
            var newFolderName = CreateNewPath(parentFolder, folderName);
            
            // Create New Folder Metadata
            var newFolderMetadata = new FileMetadata
            {
                FileOwnerEmail = userMail,
                IsFolder = true,
                VirtualParentDirectory = parentFolder,
                VirtualDirectory = newFolderName
            };

            return await TryGetOrElseAsync(async () => new ExecutionResult<FileMetadata>
            {
                ResultType = ExecutionResultType.SUCCESS,
                Value = await _fileRepository.CreateFolderAsync(newFolderMetadata)
            }, exception => HandleFileCreationError(exception, newFolderMetadata));
        }
        
        /// <summary>
        /// See <see cref="IFileService.UploadFileAsync"/> for more details.
        /// </summary>
        /// <param name="parentMetadata"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<ExecutionResult<FileMetadata>> UploadFileAsync(FileMetadata parentMetadata, string fileName, MemoryStream fileStream, string userEmail)
        {
            var newFileMetadata = new FileMetadata
            {
                FileOwnerEmail = userEmail,
                IsFolder = false,
                VirtualDirectory = CreateNewPath(parentMetadata.VirtualDirectory, fileName),
                VirtualParentDirectory = parentMetadata.VirtualDirectory
            };

            return await TryGetOrElseAsync(async () => new ExecutionResult<FileMetadata>
            {
                ResultType = ExecutionResultType.SUCCESS,
                Value = await _fileRepository.UploadFileAsync(newFileMetadata, fileStream)
            }, exception => HandleFileCreationError(exception, newFileMetadata));
        }
        
        /// <summary>
        /// Download File. See <see cref="IFileService.DownloadFileAsync"/> for more details.
        /// </summary>
        /// <param name="targetMetadata"></param>
        /// <returns></returns>
        public async Task<ExecutionResult<(string, MemoryStream)>> DownloadFileAsync(FileMetadata targetMetadata)
        {
            return await TryGetOrElseAsync( async () => new ExecutionResult<(string, MemoryStream)>
            {
                ResultType = ExecutionResultType.SUCCESS,
                Value = await _fileRepository.DownloadFileAsync(targetMetadata)
            }, exception => HandleDownloadError<(string, MemoryStream)>(exception, targetMetadata));
        }
        
        /// <summary>
        /// Remove File from db. See <see cref="IFileService.DeleteFileAsync"/> for more details.
        /// </summary>
        /// <param name="targetMetadata"></param>
        /// <returns></returns>
        public async Task<ExecutionResult<FileMetadata>> DeleteFileAsync(FileMetadata targetMetadata)
        {
            return await TryGetOrElseAsync(async () => new ExecutionResult<FileMetadata>
            {
                ResultType = ExecutionResultType.SUCCESS,
                Value = await _fileRepository.DeleteFileAsync(targetMetadata)
            }, exception => HandleDownloadError<FileMetadata>(exception, targetMetadata));
        }

        private string CreateNewPath(string parentFolder, string name)
        {
            return (parentFolder == "/") ? $"/{name}" : string.Join('/', parentFolder.Split('/'));
        }
        
        [ExcludeFromCodeCoverage]
        private async Task<TR> TryGetOrElseAsync<TR>(Func<Task<TR>> executionLambda, Func<Exception, TR> exceptionHandler)
        {
            try
            {
                return await executionLambda();
            }
            catch (Exception superException)
            {
                return exceptionHandler.Invoke(superException);
            }
        }

        [ExcludeFromCodeCoverage]
        private ExecutionResult<T> HandleDownloadError<T>(Exception superException,
            FileMetadata toDownload)
        {
            if (superException is InvalidOperationException)
            {
                return new ExecutionResult<T>
                {
                    ResultType = ExecutionResultType.NotFound,
                    Message = $"Requested File is not found: {toDownload.VirtualDirectory}"
                };
            }

            return new ExecutionResult<T>
            {
                ResultType = ExecutionResultType.Unknown,
                Message = $"Unknown Error Occurred: {superException.Message}"
            };
        }

        [ExcludeFromCodeCoverage]
        private ExecutionResult<FileMetadata> HandleFileCreationError(Exception superException, FileMetadata toCreate)
        {
            // When Error type is MongoWriteException
            if (superException is MongoWriteException mongoWriteException)
            {
                // When Error Type is 'Duplicate Key'
                if (mongoWriteException.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return new ExecutionResult<FileMetadata>
                    {
                        ResultType = ExecutionResultType.DuplicatedID,
                        Message = $"Duplicated Folder found: {toCreate.VirtualDirectory}"
                    };
                } // Else -> goto Unknown Error.
            }

            // Unknown if exception is not MongoWriteException.
            return new ExecutionResult<FileMetadata>
            {
                ResultType = ExecutionResultType.Unknown,
                Message = $"Unknown Error Occurred! : {superException.Message}"
            };
        }
    }
}