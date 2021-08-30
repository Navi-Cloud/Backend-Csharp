using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Navi_Server.Exchange;
using Navi_Server.Middleware;
using Navi_Server.Models.DTO;
using Navi_Server.Services;

namespace Navi_Server.Controllers
{
    [ApiController]
    [Route("/api/folder")]
    public class FolderController: CustomControllerBase
    {
        private readonly IFileService _fileService;
        private string UserEmail => (string)HttpContext.Items["userId"];

        public FolderController(IFileService fileService)
        {
            _fileService = fileService;
        }
        
        /// <summary>
        /// Explore Folder with given toExploreFolderData.
        /// </summary>
        /// <param name="targetFolder">The folder you want to explore.</param>
        /// <returns>200 OK with List of FileMetadata inside toExploreFolderData.</returns>
        [CustomUserValidator]
        [HttpGet]
        public async Task<IActionResult> ExploreFolder(string targetFolder)
        {
            var result = await _fileService.ExploreFolder(targetFolder, UserEmail);
            
            // Handled Case
            var handledCase = new Dictionary<ExecutionResultType, LazyLoadAction>
            {
                {ExecutionResultType.SUCCESS, () => Ok(result.Value)},
            };

            return ResultCaseHandler(handledCase, result.ResultType);
        }
        
        /// <summary>
        /// Create Folder on server.
        /// </summary>
        /// <param name="createFolderRequest">Containing New Folder Name and Parent Information.</param>
        /// <returns>
        /// <para>200 OK with created folder entity</para>
        /// <para>409 CONFLICT with error message if same folder already exists.</para>
        /// </returns>
        [CustomUserValidator]
        [HttpPost]
        public async Task<IActionResult> CreateFolder(CreateFolderRequest createFolderRequest)
        {
            var result = 
                await _fileService.CreateFolderAsync(createFolderRequest.ParentFolderMetadata.VirtualDirectory, createFolderRequest.NewFolderName, UserEmail);
            
            // Handled Case
            var handledCase = new Dictionary<ExecutionResultType, LazyLoadAction>
            {
                {ExecutionResultType.SUCCESS, () => Ok(result.Value)},
                {ExecutionResultType.DuplicatedID, () => Conflict(GetErrorResponseModel(result.Message))}
            };

            return ResultCaseHandler(handledCase, result.ResultType);
        }
    }
}