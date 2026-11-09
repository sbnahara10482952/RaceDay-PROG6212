
using System;
using System.Security.Cryptography;

Console.Write("Enter a temporary test password: ");
string password = Console.ReadLine() ?? string.Empty;

if (string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Password cannot be empty.");
    return;
}

const int saltSize = 16;
const int hashSize = 32;
const int iterations = 100000;

byte[] salt = RandomNumberGenerator.GetBytes(saltSize);

byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
    password,
    salt,
    iterations,
    HashAlgorithmName.SHA256,
    hashSize);

string passwordHash =
    $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

Console.WriteLine();
Console.WriteLine("Generated password hash:");
Console.WriteLine(passwordHash);