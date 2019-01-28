using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using BankApi.Models;
using System;

namespace BankApi.Services
{
    public class AccountService
    {
        private readonly IMongoCollection<Account> _accounts;

        public AccountService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("BankDb"));
            var database = client.GetDatabase("BankDb");
            _accounts = database.GetCollection<Account>("Accounts");
        }

        public List<Account> Get()
        {
            return _accounts.Find(account => true).ToList();
        }

        public Account Get(Guid id)
        {

            return _accounts.Find<Account>(_account => _account.Id == id).FirstOrDefault();
        }

        public Account Create(Account account)
        {
            _accounts.InsertOne(account);
            return account;
        }

        public void Update(Guid id, Account accIn)
        {
            _accounts.ReplaceOne(account => account.Id == id, accIn);
        }

        public void Remove(Guid id)
        {
            _accounts.DeleteOne(account => account.Id == id);
        }
    }
}