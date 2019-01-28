using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using BankApi.Models;
using System;
using BankApi.Helpers;

namespace BankApi.Services
{

    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("BankDb"));
            var database = client.GetDatabase("BankDb");
            _users = database.GetCollection<User>("Users");
        }
        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _users.Find(u => u.Username == username).FirstOrDefault();

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public List<User> Get()
        {
            return _users.Find(user => true).ToList();
        }

        public User Get(Guid id)
        {

            return _users.Find<User>(_users => _users.Id == id).FirstOrDefault();
        }

        public User Create(User user,string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_users.Find<User>(x => x.Username == user.Username).ToList().Count > 1)
                throw new AppException("Username \"" + user.Username + "\" is already taken");
            byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            _users.InsertOne(user);
            return user;
        }

        // public void Update(Guid id, User userIn)
        // {
        //     _users.ReplaceOne(user => user.Id == id, userIn);
        // }

        public void Update(User userParam, string password = null)
        {
            var user = _users.Find<User>(u => u.Id == userParam.Id).FirstOrDefault();

            if (user == null)
                throw new AppException("User not found");

            if (userParam.Username != user.Username)
            {
                // username has changed so check if the new username is already taken
                if (_users.Find<User>(x => x.Username == user.Username).ToList().Count > 1)
                    throw new AppException("Username \"" + user.Username + "\" is already taken");
            }

            // update user properties
            user.Name = userParam.Name;
            user.Role = userParam.Role;
            user.Username = userParam.Username;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _users.ReplaceOne(u => u.Id == userParam.Id, user);
        }
        public void Remove(Guid id)
        {
            _users.DeleteOne(user => user.Id == id);
        }


        // private helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}