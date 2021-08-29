using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using MongoDB.Driver;
using Moq;
using Navi_Server.Exchange;
using Navi_Server.Models;
using Navi_Server.Repositories;
using Navi_Server.Services;
using Xunit;

namespace Navi_Server_Test.Services
{
    public class FileServiceTest
    {
        private readonly IFileService _fileService;
        private readonly Mock<IFileRepository> _mockFileRepository;

        public FileServiceTest()
        {
            _mockFileRepository = new Mock<IFileRepository>();
            _fileService = new FileService(_mockFileRepository.Object);
        }
        
        private MongoWriteException CreateMongoException(ServerErrorCategory category)
        {
            var writeError = (WriteError) FormatterServices.GetUninitializedObject(typeof(WriteError));
            var writeErrorCategory =
                typeof(WriteError).GetField("_category", BindingFlags.NonPublic | BindingFlags.Instance);
            writeErrorCategory?.SetValue(writeError, category);
            
            var exceptionInfo = (MongoWriteException)FormatterServices.GetUninitializedObject(typeof(MongoWriteException));
            var toSet = typeof(MongoWriteException).GetField("_writeError", BindingFlags.NonPublic | BindingFlags.Instance);
            toSet.SetValue(exceptionInfo, writeError);

            return exceptionInfo;
        }

        [Fact(DisplayName = "ExploreFolder: ExploreFolder should return file list under folderTarget.")]
        public async void Is_ExploreFolder_Works_Well()
        {
            // Let
            _mockFileRepository.Setup(a => a.ExploreFolder(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<FileMetadata>());
            
            // Do
            var fileList = await _fileService.ExploreFolder("/", "testemail");
            
            // Check
            _mockFileRepository.Verify(a => a.ExploreFolder(It.IsAny<string>(), It.IsAny<string>()));
            Assert.Equal(ExecutionResultType.SUCCESS, fileList.ResultType);
            Assert.Empty(fileList.Value);
        }

        [Fact(DisplayName = "CreateFolderAsync: CreateFolderAsync should return created file metadata if succeeds.")]
        public async void Is_CreateFolderAsync_Works_Well()
        {
            // Let
            var testParentFolder = "/";
            var testFolderName = "testFolderName";
            var testEmail = "testEmail";
            var expectedOutput = new FileMetadata
            {
                FileOwnerEmail = testEmail,
                IsFolder = true,
                VirtualParentDirectory = testParentFolder,
                VirtualDirectory = "/testFolderName"
            };
            _mockFileRepository.Setup(a => a.CreateFolderAsync(It.IsAny<FileMetadata>()))
                .ReturnsAsync(expectedOutput);

            // Do
            var result = await _fileService.CreateFolderAsync(testParentFolder, testFolderName, testEmail);
            
            // Check
            Assert.Equal(ExecutionResultType.SUCCESS, result.ResultType);
            Assert.Equal(expectedOutput, result.Value);
        }

        [Fact(DisplayName =
            "CreateFolderAsync: CreateFolderAsync should return DuplicatedId when MongoWriteException Occurred.")]
        public async void Is_CreateFolderAsync_Returns_DuplicatedId_When_DuplicatedFolderName()
        {
            // Let
            _mockFileRepository.Setup(a => a.CreateFolderAsync(It.IsAny<FileMetadata>()))
                .ThrowsAsync(CreateMongoException(ServerErrorCategory.DuplicateKey));
            
            // Do
            var result = await _fileService.CreateFolderAsync("", "", "");
            
            // Check
            Assert.Equal(ExecutionResultType.DuplicatedID, result.ResultType);
        }

        [Fact(DisplayName = "CreateFolderAsync: CreateFolderAsync should return UNKNOWN when OtherException Occurred.")]
        public async void Is_CreateFolderAsync_Returns_Unknown_When_OtherException_Occurred()
        {
            // Let
            _mockFileRepository.Setup(a => a.CreateFolderAsync(It.IsAny<FileMetadata>()))
                .ThrowsAsync(new Exception());
            
            // Do
            var result = await _fileService.CreateFolderAsync("", "", "");
            
            // Check
            Assert.Equal(ExecutionResultType.Unknown, result.ResultType);
        }

        [Fact(DisplayName =
            "UploadFileAsync: UploadFileAsync should return Created Metadata if it succeeds to upload.")]
        public async void Is_UploadFileAsync_Works_Well()
        {
            // Let
            _mockFileRepository.Setup(a => a.UploadFileAsync(It.IsAny<FileMetadata>(), It.IsAny<MemoryStream>()))
                .ReturnsAsync(value: null);
            
            // Do
            await using var memoryStream = new MemoryStream();
            var result = await _fileService.UploadFileAsync(new FileMetadata {VirtualDirectory = "/"}, "", memoryStream, "");
            
            // Check
            _mockFileRepository.VerifyAll();
            Assert.Equal(ExecutionResultType.SUCCESS, result.ResultType);
            Assert.Null(result.Value);
        }

        [Fact(DisplayName = "UploadFileAsync: UploadFileAsync should return DuplicatedId type if file already exists.")]
        public async void Is_UploadFileAsync_Returns_DuplicatedId_When_File_Already_Exists()
        {
            // Let
            _mockFileRepository.Setup(a => a.UploadFileAsync(It.IsAny<FileMetadata>(), It.IsAny<MemoryStream>()))
                .ThrowsAsync(CreateMongoException(ServerErrorCategory.DuplicateKey));
            
            // Do
            await using var memoryStream = new MemoryStream();
            var result = await _fileService.UploadFileAsync(new FileMetadata {VirtualDirectory = "/"}, "", memoryStream, "");
            
            // Check
            _mockFileRepository.VerifyAll();
            Assert.Equal(ExecutionResultType.DuplicatedID, result.ResultType);
        }

        [Fact(DisplayName =
            "DownloadFileAsync: DownloadFileAsync should return filename and un-closed memorystream well.")]
        public async void Is_DownloadFileAsync_Works_Well()
        {
            // Let
            _mockFileRepository.Setup(a => a.DownloadFileAsync(It.IsAny<FileMetadata>()))
                .ReturnsAsync(value: ("", null));
            
            // Do
            var result = await _fileService.DownloadFileAsync(new FileMetadata());
            
            // Check
            Assert.Equal(ExecutionResultType.SUCCESS, result.ResultType);
            Assert.Equal("", result.Value.Item1);
            Assert.Null(result.Value.Item2);
        }

        [Fact(DisplayName =
            "DownloadFileAsync: DownloadFileAsync should return NotFound when requested file does not exists.")]
        public async void Is_DownloadFileAsync_Returns_NotFound_When_Not_Exists()
        {
            // Let
            _mockFileRepository.Setup(a => a.DownloadFileAsync(It.IsAny<FileMetadata>()))
                .ThrowsAsync(new InvalidOperationException());
            
            // Do
            var result = await _fileService.DownloadFileAsync(new FileMetadata());
            
            // Check
            Assert.Equal(ExecutionResultType.NotFound, result.ResultType);
        }

        [Fact(DisplayName = "DeleteFileAsync: DeleteFileAsync should return Deleted Metadata when succeeds.")]
        public async void Is_DeleteFileAsync_Works_Well()
        {
            // Let
            _mockFileRepository.Setup(a => a.DeleteFileAsync(It.IsAny<FileMetadata>()))
                .ReturnsAsync(new FileMetadata {VirtualDirectory = "/"});
            
            // Do
            var result = await _fileService.DeleteFileAsync(new FileMetadata());
            
            // Check
            Assert.Equal(ExecutionResultType.SUCCESS, result.ResultType);
            Assert.Equal("/", result.Value.VirtualDirectory);
        }

        [Fact(DisplayName = "DeleteFileAsync: DeleteFileAsync should return NotFound when target file is not found.")]
        public async void Is_DeleteFileAsync_Return_NotFound_When_Target_Not_Found()
        {
            // Let
            _mockFileRepository.Setup(a => a.DeleteFileAsync(It.IsAny<FileMetadata>()))
                .ThrowsAsync(new InvalidOperationException());
            
            // Do
            var result = await _fileService.DeleteFileAsync(new FileMetadata());
            
            // Check
            Assert.Equal(ExecutionResultType.NotFound, result.ResultType);
        }
    }
}