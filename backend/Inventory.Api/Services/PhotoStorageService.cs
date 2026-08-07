namespace Inventory.Api.Services;

public class InvalidPhotoException(string message) : Exception(message);

public class PhotoStorageService(IWebHostEnvironment env)
{
    // The Content-Type sent by the browser is just a client-side claim — a script can label
    // any file "image/jpeg". That's why real validation checks the file's binary signature
    // (magic bytes) instead of trusting the header the client sends.
    private static readonly (byte[] Signature, string Extension)[] AllowedSignatures =
    [
        ([0xFF, 0xD8, 0xFF], ".jpg"),
        ([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], ".png"),
    ];

    private const long MaxSizeBytes = 10 * 1024 * 1024;

    public async Task<string> SaveAsync(IFormFile photo)
    {
        if (photo.Length == 0)
        {
            throw new InvalidPhotoException("The photo is empty.");
        }

        if (photo.Length > MaxSizeBytes)
        {
            throw new InvalidPhotoException("The photo exceeds the 10MB limit.");
        }

        var extension = await DetectExtensionAsync(photo);
        if (extension is null)
        {
            throw new InvalidPhotoException("The photo must be a JPG or PNG file.");
        }

        // WebRootPath is null if the wwwroot folder doesn't exist on disk yet.
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var destinationFolder = Path.Combine(webRoot, "uploads", "itens");
        Directory.CreateDirectory(destinationFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(destinationFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await photo.CopyToAsync(stream);

        return $"/uploads/itens/{fileName}";
    }

    private static async Task<string?> DetectExtensionAsync(IFormFile photo)
    {
        var maxSignatureLength = AllowedSignatures.Max(a => a.Signature.Length);
        var header = new byte[maxSignatureLength];

        await using var stream = photo.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header);

        foreach (var (signature, extension) in AllowedSignatures)
        {
            if (bytesRead >= signature.Length && header.AsSpan(0, signature.Length).SequenceEqual(signature))
            {
                return extension;
            }
        }

        return null;
    }
}
