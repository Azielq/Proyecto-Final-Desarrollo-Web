using System;
using System.Security.Cryptography;
using Microsoft.AspNet.Identity;

public class PBKDF2PasswordHasher : IPasswordHasher
{
    // Tamaño en bytes para la sal y el hash
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 20; // 160 bits
    // Número de iteraciones recomendadas (puedes ajustar este valor)
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        // Genera la sal de forma segura
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            // Calcula el hash usando PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Combina la sal y el hash en un solo arreglo
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                // Convierte a Base64 y guarda el número de iteraciones junto al hash
                string base64Hash = Convert.ToBase64String(hashBytes);
                return $"{Iterations}.{base64Hash}";
            }
        }
    }

    public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        // Se espera que el hashedPassword tenga el formato "iteraciones.hashEnBase64"
        var splitted = hashedPassword.Split('.');
        if (splitted.Length != 2)
        {
            return PasswordVerificationResult.Failed;
        }

        if (!int.TryParse(splitted[0], out int iterations))
        {
            return PasswordVerificationResult.Failed;
        }

        byte[] hashBytes = Convert.FromBase64String(splitted[1]);
        if (hashBytes.Length != SaltSize + HashSize)
        {
            return PasswordVerificationResult.Failed;
        }

        // Extrae la sal del hash
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Calcula el hash para la contraseña proporcionada
        using (var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, iterations))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Compara byte a byte
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return PasswordVerificationResult.Failed;
                }
            }
            return PasswordVerificationResult.Success;
        }
    }
}
