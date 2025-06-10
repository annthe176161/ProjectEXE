using System;
using System.Linq;
using System.Security.Cryptography;

namespace ProjectEXE.DTO;
public static class ReferralCodeGenerator
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(int length = 8)
    {
        var randomBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var code = new string(randomBytes.Select(b => Chars[b % Chars.Length]).ToArray());
        return code;
    }
}
