// File: Dinduction.Application/Services/PasswordService.cs
using BCrypt.Net;
using Dinduction.Application.Interfaces;

namespace Dinduction.Application.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}