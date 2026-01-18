using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using API.Domain.Enums;

namespace API.Shared.Helpers;

public class SecurityHelper
{

    private string allChars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890@#$";
    private const string _alg = "HmacSHA256";
    private const string _salt = "rBzkgxfOrMsMzwhjEWuLWgCax2hG0cw2"; // Generated at https://www.random.org/strings

    private static byte[] _key
    {
        get
        {
            if (_keyStore is null || _keyStore.Length != 32)
            {
                _keyStore = Convert.FromBase64String("MLql0jCo5JfbNJAAHaUpVVyNkk8dZo/xoklaur2cdKI=");
            }
            return _keyStore;
        }
    }
    private static byte[]? _keyStore = null;
    private static readonly Regex Base64Strict = new Regex(
        @"^(?:[A-Za-z0-9\+/]{4})*(?:[A-Za-z0-9\+/]{2}==|[A-Za-z0-9\+/]{3}=)?$",
        RegexOptions.Compiled);

    public static string EncryptValue(string plain)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, iv);
        var plainBytes = Encoding.UTF8.GetBytes(plain);

        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var combined = new byte[iv.Length + cipherBytes.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, iv.Length, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public static string DecryptValue(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText) 
            || !IsLikelyCipherText(cipherText))
        {
            return cipherText;
        }

        byte[] combined;
        try
        {
            combined = Convert.FromBase64String(cipherText);
        }
        catch (FormatException)
        {
            return cipherText;
        }
        
        const int blockSizeBytes = 16; 
        if (combined.Length < blockSizeBytes * 2)
        {
            return cipherText;
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[aes.BlockSize / 8];
            Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);

            var cipherBytes = new byte[combined.Length - iv.Length];
            Buffer.BlockCopy(combined, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var decryptor = aes.CreateDecryptor(aes.Key, iv);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException)
        {
            return cipherText;
        }
        catch (ArgumentException)
        {
            // guard against any BlockCopy or other argument issues
            return cipherText;
        }
    }
    
    private static bool IsLikelyCipherText(string s)
    {
        // Rough heuristic: length is multiple of 4, only base64 chars
        return s.Length % 4 == 0 
               && Base64Strict.IsMatch(s);
    }

    //MAS2213
    #region Tokens
   
    public static string GenerateSalt()
    {
        int saltLength = 16;
        byte[] salt = new byte[saltLength];
        using (var random = new RNGCryptoServiceProvider())
        {
            random.GetNonZeroBytes(salt);
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < salt.Length; i++)
        {
            builder.Append(salt[i].ToString("x2"));
        }
        return builder.ToString();
    }
    public static bool ValidatePassword(string password, string salt, string hashedPassword)
    {
        return GetHashedPassword(password + salt) == hashedPassword;
    }
    public static string GetHashedPassword(string password, string salt = "")
    {
        if (string.IsNullOrEmpty(salt))
            salt = _salt;
        string key = string.Join(":", new string[] { password.Trim(), salt });
        using (HMAC hmac = HMACSHA256.Create(_alg))
        {
            // Hash the key.
            hmac.Key = Encoding.UTF8.GetBytes(_salt);
            hmac.ComputeHash(Encoding.UTF8.GetBytes(key));
            return Convert.ToBase64String(hmac.Hash);
        }
    }
  
    #endregion

    #region Encryption & Decryption


    public static string Encrypt(string text, string salt = "")
    {
        if (!string.IsNullOrEmpty(salt))
            salt = _salt;
        string result;
        if (text == "")
        {
            result = "";
        }
        else
        {
            UTF8Encoding uTF8Encoding = new UTF8Encoding();
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] key = mD5CryptoServiceProvider.ComputeHash(uTF8Encoding.GetBytes(salt));
            TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
            tripleDESCryptoServiceProvider.Key = key;
            tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
            tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
            byte[] bytes = uTF8Encoding.GetBytes(text);
            byte[] inArray;
            try
            {
                ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateEncryptor();
                inArray = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
            }
            finally
            {
                tripleDESCryptoServiceProvider.Clear();
                mD5CryptoServiceProvider.Clear();
            }
            result = Convert.ToBase64String(inArray);
        }
        return result;
    }

    public static string Decrypt(string text, string salt = "")
    {
        if (!string.IsNullOrEmpty(salt))
            salt = _salt;
        string result;
        if (string.IsNullOrEmpty(text))
        {
            result = "";
        }
        else
        {
            UTF8Encoding uTF8Encoding = new UTF8Encoding();
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] key = mD5CryptoServiceProvider.ComputeHash(uTF8Encoding.GetBytes(salt));
            TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
            tripleDESCryptoServiceProvider.Key = key;
            tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
            tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
            byte[] array = Convert.FromBase64String(text);
            byte[] bytes;
            try
            {
                ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor();
                bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
            }
            catch (Exception ex)
            {
                bytes = null;
            }
            finally
            {
                tripleDESCryptoServiceProvider.Clear();
                mD5CryptoServiceProvider.Clear();
            }
            result = uTF8Encoding.GetString(bytes);
        }
        return result;
    }
   
    public static string PasswordGenerator(int passwordLength = 10, bool strongPassword = true)
    {
        Random Random = new Random();
        int seed = Random.Next(1, int.MaxValue);
        //const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
        const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
        const string specialCharacters = @"!#$%&'()*+,-./:;<=>?@[\]_";

        var chars = new char[passwordLength];
        var rd = new Random(seed);

        for (var i = 0; i < passwordLength; i++)
        {
            // If we are to use special characters
            if (strongPassword && i % Random.Next(3, passwordLength) == 0)
            {
                chars[i] = specialCharacters[rd.Next(0, specialCharacters.Length)];
            }
            else
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
        }

        return new string(chars);
    }
    public static bool IsStrongPassword(string password)
    {
        string pattern = "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,15}$";
        bool result;
        try
        {
            Regex.Match("", pattern);
        }
        catch (ArgumentException)
        {
            result = false;
            return result;
        }
        result = true;
        return result;
    }
    #endregion
    
    public static TokenPayloadViewModel? GetPayload(string token) {
        var payload = new TokenPayloadViewModel();
        if (string.IsNullOrEmpty(token))
            return null;
        try
        {
            var key = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = key.Split([':']);
            
            if (parts.Length < 4) return null;
            
            var ticks = long.Parse(parts[2]);
            payload.ExpirationDate = new DateTime(ticks);
                
            if (parts.Length >= 6)
                payload.RoleID = Enum.TryParse<ApplicationRole>(parts[5], out var roleID) ? roleID : 0;

            if (parts.Length >= 7)
                payload.UserID = long.TryParse(parts[6], out var userID) ? userID : 0;
                
            if (parts.Length >= 8)
                payload.CompanyID = long.TryParse(parts[7], out var companyID) ? companyID : 0;
                
            if (parts.Length >= 9)
                payload.UserName = parts[8];
                
            return payload;

        }
        catch (Exception ex)
        {
            return null;
        }
    }
}

public class TokenPayloadViewModel
{
    public DateTime ExpirationDate { get; set; }
    public ApplicationRole RoleID { get; set; }
    public long UserID { get; set; }
    public long CompanyID { get; set; }
    public string? UserName { get; set; }
}
