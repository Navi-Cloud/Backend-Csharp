using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson.Serialization.Attributes;

namespace Navi_Server.Models
{
    [ExcludeFromCodeCoverage]
    public class FileMetadata
    {
        /// <summary>
        /// Define 'who owned this file?' => By User Email
        /// </summary>
        [BsonElement("fileOwnerEmail")]
        public string FileOwnerEmail { get; set; }
        
        /// <summary>
        /// Define Virtual Directory Full Path, Including name
        /// </summary>
        [BsonElement("virtualDirectory")]
        public string VirtualDirectory { get; set; }
        
        /// <summary>
        /// Define Parent Virtual Directory Full Path.
        /// </summary>
        [BsonElement("virtualParentDirectory")]
        public string VirtualParentDirectory { get; set; }
        
        /// <summary>
        /// Whether this logical data defines folder or file.
        /// </summary>
        [BsonElement("isFolder")]
        public bool IsFolder { get; set; }
    }
}