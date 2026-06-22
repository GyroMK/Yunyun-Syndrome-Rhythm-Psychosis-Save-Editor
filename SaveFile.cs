using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YunyunSaveEditor;

/// <summary>
/// Cifrado/descifrado de los guardados de Yunyun Syndrome.
/// Formato: [16 bytes IV][AES-256-CBC + PKCS7]. El JSON va en UTF-8 sin BOM.
/// </summary>
public static class SaveFile
{
    // Clave AES-256 extraída del juego (App.dll), en base64.
    private const string KeyBase64 = "Lg9miakf2T4OlLEV/2vgTLexyDP69IvpHdpxGFnIhhI=";
    private static readonly byte[] Key = Convert.FromBase64String(KeyBase64);

    public static string Decrypt(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        if (data.Length < 32)
            throw new InvalidDataException("El archivo es demasiado pequeño para ser un guardado válido.");

        byte[] iv = new byte[16];
        Array.Copy(data, 0, iv, 0, 16);

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var dec = aes.CreateDecryptor();
        byte[] plain = dec.TransformFinalBlock(data, 16, data.Length - 16);
        return Encoding.UTF8.GetString(plain);
    }

    public static void Encrypt(string json, string path)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();              // IV aleatorio en cada guardado (como hace el juego)
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        byte[] pt = Encoding.UTF8.GetBytes(json);
        using var enc = aes.CreateEncryptor();
        byte[] ct = enc.TransformFinalBlock(pt, 0, pt.Length);

        byte[] outBytes = new byte[16 + ct.Length];
        Array.Copy(aes.IV, 0, outBytes, 0, 16);
        Array.Copy(ct, 0, outBytes, 16, ct.Length);
        File.WriteAllBytes(path, outBytes);
    }
}
