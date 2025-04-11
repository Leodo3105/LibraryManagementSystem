﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Services
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string providedPassword, string storedHash);
    }

    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool VerifyPassword(string providedPassword, string storedHash)
        {
            string computedHash = HashPassword(providedPassword);
            return computedHash == storedHash;
        }
    }
}