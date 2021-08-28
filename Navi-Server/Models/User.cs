using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Navi_Server.Models
{
    /// <summary>
    /// User model description. All about users!
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class User
    {
        /// <summary>
        /// Unique ID[Or Identifier] for Each User.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        /// <summary>
        /// User's Email Address
        /// </summary>
        [BsonElement("userEmail")]
        public string UserEmail { get; set; }
        
        /// <summary>
        /// User's Name Information, could be null though
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// User's Password Information. Note this should be encrypted.
        /// </summary>
        public string UserPassword { get; set; } // TODO: Need to be encrypted.

        public bool CheckPassword(string input)
        {
            return UserPassword == input;
        }
    }
}