using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Navi_Server_Test.Helper;
using Navi_Server.Models;
using Navi_Server.Repositories;
using Xunit;

namespace Navi_Server_Test.Repositories
{
    public class FileRepositoryTest : MongoHelper
    {
        private readonly IFileRepository _fileRepository;
        private readonly IGridFSBucket _gridFsConnection;

        public FileRepositoryTest()
        {
            _gridFsConnection = new GridFSBucket(_mongoContext._MongoDatabase);
            _fileRepository = new FileRepository(_mongoContext);
            var collection = _gridFsConnection.Database.GetCollection<BsonDocument>("fs.files");
            collection.Indexes.CreateOne(
                new CreateIndexModel<BsonDocument>(
                    new BsonDocument {{"metadata.virtualDirectory", 1}, {"metadata.fileOwnerEmail", 1}}, new CreateIndexOptions {Unique = true}));
        }

        private void UploadFile(FileMetadata metadata, string contents)
        {
            // Let - Save Folder
            var options = new GridFSUploadOptions
            {
                Metadata = metadata.ToBsonDocument()
            };
            _gridFsConnection.UploadFromBytes("/",Encoding.UTF8.GetBytes(contents), options);
        }

        [Fact(DisplayName = "ExploreFolder: ExploreFolder should return list of file metadata if exists.")]
        public async void Is_ExploreFolder_Works_Well()
        {
            // Let
            UploadFile(new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            }, "test");

            // Do
            var fileList = await _fileRepository.ExploreFolder("/", "kangdroid@testemail.com");
            
            // Check
            Assert.Single(fileList);
        }

        [Fact(DisplayName = "UploadFile: UploadFile should return its metadata when uploading succeeds.")]
        public async void Is_UploadFile_Works_Well()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            };
            await using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("testcontent"));
            
            // Do
            var uploadResult = await _fileRepository.UploadFileAsync(metadata, memoryStream);
            
            // Check
            Assert.Equal(metadata, uploadResult);
        }

        [Fact(DisplayName = "UploadFile: UploadFile should throw MongoWriteException when duplicated index found.")]
        public async void Is_UploadFile_Throws_MongoWriteException_Duplicated_Index()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            };
            await using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("testcontent"));
            
            // Do
            await _fileRepository.UploadFileAsync(metadata, memoryStream);
            await Assert.ThrowsAsync<MongoWriteException>(() =>
                _fileRepository.UploadFileAsync(metadata, memoryStream));
        }

        [Fact(DisplayName =
            "DeleteFileAsync: DeleteFileAsync should return file metadata when removing action succeeds.")]
        public async void Is_DeleteFileAsync_Works_Well()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            };
            UploadFile(metadata, "test");
            
            // Do
            var result = await _fileRepository.DeleteFileAsync(metadata);
            
            // Check
            Assert.Equal(metadata, result);
        }

        [Fact(DisplayName =
            "DeleteFileAsync: DeleteFileAsync should throw InvalidOperationException when target file is not found.")]
        public async void Is_DeleteFileAsync_Throws_InvalidOperationException_Not_Found()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            };

            // Do
            await Assert.ThrowsAsync<InvalidOperationException>(() => _fileRepository.DeleteFileAsync(metadata));
        }

        [Fact(DisplayName = "CreateFolderAsync: CreateFolderAsync should return folder metadata")]
        public async void Is_CreateFolderAsync_Works_Well()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = true,
                VirtualDirectory = "/testFolder",
                VirtualParentDirectory = "/"
            };
            
            // Do
            var result = await _fileRepository.CreateFolderAsync(metadata);
            
            // Check
            Assert.Equal(metadata, result);
        }

        [Fact(DisplayName =
            "CreateFolderAsync: CreateFolderAsync should throw MongoWriteException when Duplicated folder found.")]
        public async void Is_CreateFolderAsync_Throws_MongoWriteException()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = true,
                VirtualDirectory = "/testFolder",
                VirtualParentDirectory = "/"
            };
            
            // Do
            await _fileRepository.CreateFolderAsync(metadata);
            
            // Check
            await Assert.ThrowsAsync<MongoWriteException>(() => _fileRepository.CreateFolderAsync(metadata));
        }

        [Fact(DisplayName =
            "DownloadFileAsync: DownloadFileAsync should return correct MemoryStream when everything is ok.")]
        public async void Is_DownloadFileAsync_Works_Well()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            };
            UploadFile(metadata, "testContents");
            
            // Do
            var downloadStream = await _fileRepository.DownloadFileAsync(metadata);
            await using var stream = downloadStream.Item2;
            
            // Check
            var outputString = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Equal("testContents", outputString);
            Assert.Equal("/", downloadStream.Item1);
        }
        
        [Fact(DisplayName =
            "DownloadFileAsync: DownloadFileAsync should throw InvalidOperationException when given metadata is not found on server.")]
        public async void Is_DownloadFileAsync_Throws_InvalidOperationException_When_Metadata_Not_Exists()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            };

            // Do
            await Assert.ThrowsAsync<InvalidOperationException>(() => _fileRepository.DownloadFileAsync(metadata));
        }

        [Fact(DisplayName = "CopyFileAsync: CopyFileAsync should return copied metadata well.")]
        public async void Is_CopyFileAsync_Works_Well()
        {
            // Let
            var metadata = new FileMetadata
            {
                FileOwnerEmail = "kangdroid@testemail.com",
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            };
            UploadFile(metadata, "testContents");
            var copyFileMetadata = new FileMetadata
            {
                FileOwnerEmail = metadata.FileOwnerEmail,
                IsFolder = metadata.IsFolder,
                VirtualDirectory = "/test2.txt",
                VirtualParentDirectory = "/"
            };

            // Do
            var result = await _fileRepository.CopyFileAsync(metadata, copyFileMetadata);
            
            // Check
            var emptyFilter = Builders<GridFSFileInfo>.Filter.Empty;
            using var cursor = await _gridFsConnection.FindAsync(emptyFilter);
            Assert.Equal(2, cursor.ToList().Count);
            Assert.Equal(copyFileMetadata, result);
        }
    }
}