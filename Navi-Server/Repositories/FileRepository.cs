using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Navi_Server.Models;

namespace Navi_Server.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly IGridFSBucket _gridFsBucket;
        
        public FileRepository(MongoContext mongoContext)
        {
            _gridFsBucket = new GridFSBucket(mongoContext._MongoDatabase);
            
            // Create Unique Index on VirtualDirectory AND Owner
            // Meaning Same owner cannot have same virtual directory[The full path to file]
            // But, different user? that is not the case.
            var collection = _gridFsBucket.Database.GetCollection<BsonDocument>("fs.files");
            collection.Indexes.CreateOne(
                new CreateIndexModel<BsonDocument>(
                    new BsonDocument {{"metadata.virtualDirectory", 1}, {"metadata.fileOwnerEmail", 1}}, new CreateIndexOptions {Unique = true}));
        }
        
        /// <summary>
        /// See <see cref="IFileRepository.ExploreFolder"/> for more details.
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<List<FileMetadata>> ExploreFolder(string parentFolder, string userEmail)
        {
            // Setup Find Filter
            var findFilter = Builders<GridFSFileInfo>.Filter.And(
                Builders<GridFSFileInfo>.Filter.Eq("metadata.fileOwnerEmail", userEmail),
                Builders<GridFSFileInfo>.Filter.Eq("metadata.virtualParentDirectory", parentFolder)
            );
            
            // Find operation
            using var cursor = await _gridFsBucket.FindAsync(findFilter);
            return cursor.ToList().Select(a => new FileMetadata
            {
                FileOwnerEmail = a.Metadata["fileOwnerEmail"].AsString,
                VirtualDirectory = a.Metadata["virtualDirectory"].AsString,
                VirtualParentDirectory = a.Metadata["virtualParentDirectory"].AsString,
                IsFolder = a.Metadata["isFolder"].AsBoolean
            }).ToList();
        }
        
        /// <summary>
        /// See <see cref="IFileRepository.UploadFileAsync"/> for more details.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public async Task<FileMetadata> UploadFileAsync(FileMetadata metadata, Stream fileStream)
        {
            var uploadOption = new GridFSUploadOptions {Metadata = metadata.ToBsonDocument()};
            await _gridFsBucket.UploadFromStreamAsync("", fileStream, uploadOption);

            return metadata;
        }
        
        /// <summary>
        /// See <see cref="IFileRepository.DeleteFileAsync"/> for more details.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<FileMetadata> DeleteFileAsync(FileMetadata metadata)
        {
            var targetFile = await FindFileByMetadataAsync(metadata);
            await _gridFsBucket.DeleteAsync(targetFile.Id);

            return metadata;
        }
        
        /// <summary>
        /// See <see cref="IFileRepository.CreateFolderAsync"/> for more details.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<FileMetadata> CreateFolderAsync(FileMetadata metadata)
        {
            var uploadOption = new GridFSUploadOptions {Metadata = metadata.ToBsonDocument()};
            await _gridFsBucket.UploadFromBytesAsync("", Encoding.UTF8.GetBytes(""), uploadOption);

            return metadata;
        }
        
        /// <summary>
        /// See <see cref="IFileRepository.DownloadFileAsync"/> for more details.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<(string, MemoryStream)> DownloadFileAsync(FileMetadata metadata)
        {
            var fileInformation = await FindFileByMetadataAsync(metadata);
            var memoryStream = new MemoryStream();
            await _gridFsBucket.DownloadToStreamAsync(fileInformation.Id, memoryStream);

            return (fileInformation.Filename, memoryStream);
        }
        
        /// <summary>
        /// See <see cref="IFileRepository.CopyFileAsync"/> for more details.
        /// </summary>
        /// <param name="originalFile"></param>
        /// <param name="destinationFile"></param>
        /// <returns></returns>
        public async Task<FileMetadata> CopyFileAsync(FileMetadata originalFile, FileMetadata destinationFile)
        {
            // Get Original File Stream
            await using var originalStream = (await DownloadFileAsync(originalFile)).Item2;
            
            // Upload with copied-metadata
            return await UploadFileAsync(destinationFile, originalStream);
        }

        /// <summary>
        /// <para>Find File[GridFSFileInfo] by File Metadata.</para>
        /// <para>This will help us to find objectId of file information via metadata.</para>
        /// </summary>
        /// <param name="metadata">File Metadata to search.</param>
        /// <returns>GridFS File Information, containing objectId</returns>
        private async Task<GridFSFileInfo> FindFileByMetadataAsync(FileMetadata metadata)
        {
            // Setup Find Filter
            var findFilter = Builders<GridFSFileInfo>.Filter.And(
                Builders<GridFSFileInfo>.Filter.Eq("metadata.fileOwnerEmail", metadata.FileOwnerEmail),
                Builders<GridFSFileInfo>.Filter.Eq("metadata.virtualDirectory", metadata.VirtualDirectory),
                Builders<GridFSFileInfo>.Filter.Eq("metadata.isFolder", metadata.IsFolder),
                Builders<GridFSFileInfo>.Filter.Eq("metadata.virtualParentDirectory", metadata.VirtualParentDirectory)
            );
            
            // Find operation
            using var cursor = await _gridFsBucket.FindAsync(findFilter);
            return await cursor.FirstAsync(); // There should be one though.
        } 
    }
}