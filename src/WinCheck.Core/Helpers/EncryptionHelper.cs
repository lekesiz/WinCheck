using System;
using System.Security.Cryptography;
using System.Text;

namespace WinCheck.Core.Helpers;

/// <summary>
/// Provides encryption/decryption functionality using Windows Data Protection API (DPAPI)
/// </summary>
public static class EncryptionHelper
{
    /// <summary>
    /// Encrypts a string using Windows DPAPI (Data Protection API)
    /// Data is protected for the current user only
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>Base64 encoded encrypted string, or empty string if input is null/empty</returns>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        try
        {
            // Convert string to bytes
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            // Encrypt using DPAPI for current user
            byte[] encryptedBytes = ProtectedData.Protect(
                plainBytes,
                optionalEntropy: null, // No additional entropy
                scope: DataProtectionScope.CurrentUser // Current user only
            );

            // Convert to Base64 for storage
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (CryptographicException)
        {
            // If encryption fails, return empty (better than exposing plaintext)
            return string.Empty;
        }
    }

    /// <summary>
    /// Decrypts a DPAPI-encrypted string
    /// </summary>
    /// <param name="encryptedText">Base64 encoded encrypted string</param>
    /// <returns>Decrypted plaintext, or empty string if decryption fails</returns>
    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
        {
            return string.Empty;
        }

        try
        {
            // Convert from Base64
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            // Decrypt using DPAPI
            byte[] decryptedBytes = ProtectedData.Unprotect(
                encryptedBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser
            );

            // Convert back to string
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception) // Catches CryptographicException, FormatException, etc.
        {
            // If decryption fails, return empty
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if a string appears to be encrypted (Base64 format check)
    /// </summary>
    /// <param name="text">Text to check</param>
    /// <returns>True if text appears to be encrypted, false otherwise</returns>
    public static bool IsEncrypted(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        try
        {
            // Try to decode as Base64
            Convert.FromBase64String(text);

            // If successful and reasonably long, probably encrypted
            // Plaintext API keys are typically 20+ chars but not Base64
            return text.Length > 20 && !text.StartsWith("sk-") && !text.Contains(" ");
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
