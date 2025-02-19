using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class QuoteTokenService
{
    private readonly string _secretKey;

    public QuoteTokenService(string secretKey)
    {
        _secretKey = secretKey;
    }

    public string GenerateQuoteToken(QuoteTokenPayload payload)
    {
        string jsonPayload = JsonSerializer.Serialize(payload);
        string signature = GenerateHmacSha256(jsonPayload, _secretKey);

        return $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonPayload))}.{signature}";
    }

    public bool ValidateQuoteToken(string token, out QuoteTokenPayload payload)
    {
        payload = null;

        var parts = token.Split('.');
        if (parts.Length != 2) return false;

        string jsonPayload;
        try
        {
            jsonPayload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
            payload = JsonSerializer.Deserialize<QuoteTokenPayload>(jsonPayload);
        }
        catch
        {
            return false;
        }

        string expectedSignature = GenerateHmacSha256(jsonPayload, _secretKey);
        if (expectedSignature != parts[1]) return false;

        return payload.ExpiresAt > DateTime.UtcNow;
    }

    private string GenerateHmacSha256(string data, string key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
}