using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace backend.Services;

public interface IFileService
{
    Task<(string? path, string? fileType, string? error)> SaveFileAsync(IFormFile file);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly string[] _allowedImageTypes = { ".jpg", ".jpeg", ".gif", ".png" };
    private const int MaxImageWidth = 320;
    private const int MaxImageHeight = 240;
    private const long MaxTextSize = 100 * 1024;

    public FileService(IWebHostEnvironment env) => _env = env;

    public async Task<(string? path, string? fileType, string? error)> SaveFileAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLower();
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsDir, uniqueName);

        if (ext == ".txt")
        {
            if (file.Length > MaxTextSize)
                return (null, null, "Текстовый файл не должен превышать 100 КБ");

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return ($"/uploads/{uniqueName}", "text", null);
        }

        if (_allowedImageTypes.Contains(ext))
        {
            using var inputStream = file.OpenReadStream();
            using var img = await Image.LoadAsync(inputStream);

            if (img.Width > MaxImageWidth || img.Height > MaxImageHeight)
                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(MaxImageWidth, MaxImageHeight)
                }));

            await img.SaveAsync(fullPath);
            return ($"/uploads/{uniqueName}", "image", null);
        }

        return (null, null, "Недопустимый тип файла. Разрешены: JPG, GIF, PNG, TXT");
    }
}