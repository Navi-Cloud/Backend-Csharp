using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Navi_Server.Exchange;
using Navi_Server.Middleware;
using Navi_Server.Models;
using Navi_Server.Models.DTO;
using Navi_Server.Services;

namespace Navi_Server.Controllers
{
    [ApiController]
    [Route("/api/file")]
    public class FileController: CustomControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        private string UserEmail => (string)HttpContext.Items["userId"];
        
        /// <summary>
        /// Download file with specific parameter.
        /// </summary>
        /// <param name="vd">Virtual Directory[Absolute path to file]</param>
        /// <param name="vpd">Virtual Parent Directory[Absolute Path where file contains]</param>
        /// <returns>
        /// <para>200 Ok with File</para>
        /// <para>404 Not-Found if given vd and vpd are not found.</para>
        /// </returns>
        [CustomUserValidator]
        [HttpGet]
        public async Task<IActionResult> DownloadFile(string vd, string vpd)
        {
            // Make sure we are using requested - User ID
            var targetMetadata = new FileMetadata
            {
                FileOwnerEmail = UserEmail,
                IsFolder = false,
                VirtualDirectory = vd,
                VirtualParentDirectory = vpd
            };
            
            targetMetadata.FileOwnerEmail = UserEmail;
            var result = await _fileService.DownloadFileAsync(targetMetadata);
            
            // Handled Case
            var handledCase = new Dictionary<ExecutionResultType, LazyLoadAction>
            {
                {ExecutionResultType.SUCCESS, () =>
                {
                    var fileName = result.Value.Item1.Split('/').Last();
                    var content = result.Value.Item2;
                    return File(content, "application/octet-stream", fileName);
                }},
                {ExecutionResultType.NotFound, () => NotFound(GetErrorResponseModel(result.Message))}
            };

            return ResultCaseHandler(handledCase, result.ResultType);
        }

        /// <summary>
        /// Upload file to DB - NEEDS AUTHENTICATION!!
        /// </summary>
        /// <param name="fileRequest">A File and metadata.</param>
        /// <returns>
        /// <para>200 OK response with Uploaded Entity</para>
        /// <para>409 CONFLICT response if uploaded file already exists.</para>
        /// </returns>
        [CustomUserValidator]
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm]UploadFileRequest fileRequest)
        {
            // Get Filename
            var fileName = fileRequest.File.FileName;
            
            // Copy File Stream
            await using var memoryStream = new MemoryStream();
            await fileRequest.File.CopyToAsync(memoryStream);
            
            // Get Result
            var result = await _fileService.UploadFileAsync(new FileMetadata { VirtualDirectory = fileRequest.ParentFolder}, fileName, memoryStream, UserEmail);
            
            // Handled Case
            var handledCase = new Dictionary<ExecutionResultType, LazyLoadAction>
            {
                {ExecutionResultType.SUCCESS, () => Ok(result.Value)},
                {ExecutionResultType.DuplicatedID, () => Conflict(GetErrorResponseModel(result.Message))}
            };
            
            return ResultCaseHandler(handledCase, result.ResultType);
        }

        [CustomUserValidator]
        [HttpDelete]
        public async Task<IActionResult> DeleteFile(FileMetadata toDelete)
        {
            var result = await _fileService.DeleteFileAsync(toDelete);
            
            // Handled Case
            var handledCase = new Dictionary<ExecutionResultType, LazyLoadAction>
            {
                {ExecutionResultType.SUCCESS, () => Ok(result.Value)},
                {ExecutionResultType.NotFound, () => NotFound(GetErrorResponseModel(result.Message))}
            };
            
            return ResultCaseHandler(handledCase, result.ResultType);
        }
    }
}