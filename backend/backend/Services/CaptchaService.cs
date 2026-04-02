using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;

namespace backend.Services;

public interface ICaptchaService
{
    (string token, byte[] imageBytes) GenerateCaptcha();
    bool ValidateCaptcha(string token, string answer);
}

public class CaptchaService : ICaptchaService
{
    private readonly ConcurrentDictionary<string, (string answer, DateTime expiry)> _store = new();
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly Random _rng = new();

    public (string token, byte[] imageBytes) GenerateCaptcha()
    {
        var code = new string(Enumerable.Range(0, 5)
            .Select(_ => Chars[_rng.Next(Chars.Length)]).ToArray());

        var token = Guid.NewGuid().ToString("N");
        _store[token] = (code, DateTime.UtcNow.AddMinutes(5));

        using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(150, 50);

        image.Mutate(ctx =>
        {
            ctx.Fill(SixLabors.ImageSharp.Color.White);

            for (int i = 0; i < 5; i++)
            {
                ctx.DrawLine(
                    SixLabors.ImageSharp.Color.LightGray,
                    1f,
                    new SixLabors.ImageSharp.PointF(_rng.Next(150), _rng.Next(50)),
                    new SixLabors.ImageSharp.PointF(_rng.Next(150), _rng.Next(50))
                );
            }
        });

        var font = SixLabors.Fonts.SystemFonts.CreateFont("Liberation Sans", 22, SixLabors.Fonts.FontStyle.Bold);
        image.Mutate(ctx => ctx.DrawText(code, font, SixLabors.ImageSharp.Color.DarkBlue,
            new SixLabors.ImageSharp.PointF(10, 8)));

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return (token, ms.ToArray());
    }

    public bool ValidateCaptcha(string token, string answer)
    {
        foreach (var key in _store.Keys)
            if (_store.TryGetValue(key, out var val) && val.expiry < DateTime.UtcNow)
                _store.TryRemove(key, out _);

        if (!_store.TryRemove(token, out var entry)) return false;
        if (entry.expiry < DateTime.UtcNow) return false;
        return string.Equals(entry.answer, answer.ToUpper().Trim(), StringComparison.Ordinal);
    }
}