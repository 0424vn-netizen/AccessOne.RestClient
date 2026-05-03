using System;
using System.Security.Cryptography;
using System.Text;

namespace VW.Api.RestClient
{
    /// <summary>
    /// The crypto utilities
    /// </summary>
    public static class CryptoUtils
    {
        #region Variables

        /// <summary>
        /// Gets or sets the encrypt key.
        /// </summary>
        private const string DefaultEncryptKey = "1@ECHOa1";

        /// <summary>
        /// Gets or sets the encrypt iv.
        /// </summary>
        private const string DefaultEncryptIV = "29d##m12";

        #endregion

        #region Public methods

        #region Encrypt

        /// <summary>
        /// Encrypts the given text.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <returns></returns>
        public static string Encrypt(string value)
        {
            return Encrypt(value, DefaultEncryptKey, DefaultEncryptIV);
        }

        /// <summary>
        /// Encrypts the given text.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <returns></returns>
        public static string Encrypt(string value, string encryptKey)
        {
            return Encrypt(value, encryptKey, DefaultEncryptIV);
        }

        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <param name="encryptIV">The encrypt iv.</param>
        /// <returns></returns>
        public static string Encrypt(string value, string encryptKey, string encryptIV)
        {
            return EncryptText(value, encryptKey, encryptIV);
        }

        /// <summary>
        /// Encrypts the and ensure success.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <returns></returns>
        public static string TryEncrypt(string value)
        {
            return TryEncrypt(value, DefaultEncryptKey, DefaultEncryptIV);
        }

        /// <summary>
        /// Tries to encrypt.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <returns></returns>
        public static string TryEncrypt(string value, string encryptKey)
        {
            return TryEncrypt(value, encryptKey, DefaultEncryptIV);
        }

        /// <summary>
        /// Tries to encrypt.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <param name="encryptIV">The encrypt iv.</param>
        /// <returns></returns>
        public static string TryEncrypt(string value, string encryptKey, string encryptIV)
        {
            try
            {
                return EncryptText(value, encryptKey, encryptIV);
            }
            catch
            {
                return null;
            }
        }


        #endregion

        #region Decrypt

        /// <summary>
        /// Decrypts the given encrypted text.
        /// </summary>
        /// <param name="value">The encrypted text.</param>
        /// <returns></returns>
        public static string Decrypt(string value)
        {
            return Decrypt(value, DefaultEncryptKey, DefaultEncryptIV);
        }

        /// <summary>
        /// Decrypts the given encrypted text.
        /// </summary>
        /// <param name="value">The encrypted text.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <returns></returns>
        public static string Decrypt(string value, string encryptKey)
        {
            return Decrypt(value, encryptKey, DefaultEncryptIV);
        }

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <param name="encryptIV">The encrypt iv.</param>
        /// <returns></returns>
        public static string Decrypt(string value, string encryptKey, string encryptIV)
        {
            return DecryptText(value, encryptKey, encryptIV);
        }

        /// <summary>
        /// Tries to decrypt.
        /// </summary>
        /// <param name="value">The encrypted text.</param>
        /// <returns></returns>
        public static string TryDecrypt(string value)
        {
            return TryDecrypt(value, DefaultEncryptKey, DefaultEncryptIV);
        }

        /// <summary>
        /// Tries to decrypt.
        /// </summary>
        /// <param name="value">The encrypted text.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <returns></returns>
        public static string TryDecrypt(string value, string encryptKey)
        {
            return TryDecrypt(value, encryptKey, DefaultEncryptIV);
        }

        /// <summary>
        /// Tries to decrypt.
        /// </summary>
        /// <param name="value">The encrypted text.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <param name="encryptIV">The encrypt iv.</param>
        /// <returns></returns>
        public static string TryDecrypt(string value, string encryptKey, string encryptIV)
        {
            try
            {
                return DecryptText(value, encryptKey, encryptIV);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Create a SHA2 string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string SHA2(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            using (var hasher = SHA256.Create())
            {
                var bytes = hasher.ComputeHash(Encoding.ASCII.GetBytes(value));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Tries to create a SHA2 string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string TrySHA2(string value)
        {
            try
            {
                return SHA2(value);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Method private

        /// <summary>
        /// Encrypts the text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encryptKey">The encrypt key.</param>
        /// <param name="encryptIV">The encrypt iv.</param>
        /// <returns></returns>
        private static string EncryptText(string value, string encryptKey, string encryptIV)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var des = new DESCryptoServiceProvider
            {
                Mode = CipherMode.CBC,
                Key = Encoding.UTF8.GetBytes(encryptKey),
                IV = Encoding.UTF8.GetBytes(encryptIV)
            };

            var bytes = Encoding.UTF8.GetBytes(value);
            var transform = des.CreateEncryptor(des.Key, des.IV);

            return Convert.ToBase64String(transform.TransformFinalBlock(bytes, 0, bytes.Length));
        }

        /// <summary>
        /// Decrypts the text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string DecryptText(string value, string encryptKey, string encryptIV)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var des = new DESCryptoServiceProvider
            {
                Mode = CipherMode.CBC,
                Key = Encoding.UTF8.GetBytes(encryptKey),
                IV = Encoding.UTF8.GetBytes(encryptIV)
            };

            var bytes = Convert.FromBase64String(value);
            var transform = des.CreateDecryptor(des.Key, des.IV);

            return Encoding.UTF8.GetString(transform.TransformFinalBlock(bytes, 0, bytes.Length));
        }

        #endregion
    }
}