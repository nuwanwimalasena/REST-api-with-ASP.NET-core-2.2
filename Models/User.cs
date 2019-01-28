using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BankApi.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class User
    {
        [BsonId]
        public Guid Id {get;set;}

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("PasswordHash")]
        public byte[] PasswordHash { get; set; }
        
        [BsonElement("PasswordSalt")]
        public byte[] PasswordSalt { get; set; }
    }
}