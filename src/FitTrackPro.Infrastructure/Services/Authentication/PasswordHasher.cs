namespace FitTrackPro.Infrastructure.Services.Authentication;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using FitTrackPro.Application.Common.Interfaces;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        var hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var hashBytes = Convert.FromBase64String(passwordHash);
        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        for (int i = 0; i < KeySize; i++)
        {
            if (hashBytes[i + SaltSize] != hash[i])
                return false;
        }

        return true;
    }
}