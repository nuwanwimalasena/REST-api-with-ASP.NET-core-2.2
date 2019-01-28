using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BankApi.Models
{
    public class Account
    {
        [BsonId]
        public Guid Id {get;set;}

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Balance")]
        public decimal Balance { get; set; }

        [BsonElement("Branch")]
        public string Branch { get; set; }

        [BsonElement("Owner")]
        public string Owner { get; set; }
    }
}