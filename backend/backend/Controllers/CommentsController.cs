using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using System.Text.RegularExpressions;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICaptchaService _captchaService;
    private readonly IFileService _fileService;

    public CommentsController(AppDbContext db, ICaptchaService captchaService, IFileService fileService)
    {
        _db = db;
        _captchaService = captchaService;
        _fileService = fileService;
    }

    // GET /api/comments
    [HttpGet]
    public async Task<IActionResult> GetComments(
        [FromQuery] int page = 1,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] bool sortDesc = true)
    {
        const int pageSize = 25;
        var query = _db.Comments
            .Include(c => c.User)
            .Where(c => c.ParentId == null);

        query = (sortBy.ToLower(), sortDesc) switch
        {
            ("username", true) => query.OrderByDescending(c => c.User.UserName),
            ("username", false) => query.OrderBy(c => c.User.UserName),
            ("email", true) => query.OrderByDescending(c => c.User.Email),
            ("email", false) => query.OrderBy(c => c.User.Email),
            (_, true) => query.OrderByDescending(c => c.CreatedAt),
            (_, false) => query.OrderBy(c => c.CreatedAt),
        };

        var total = await query.CountAsync();
        var comments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResultDto<CommentResponseDto>
        {
            Items = comments.Select(c => MapToDto(c, loadReplies: true)).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    // POST /api/comments
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromForm] CommentCreateDto dto, IFormFile? file)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_captchaService.ValidateCaptcha(dto.CaptchaToken, dto.CaptchaAnswer))
            return BadRequest(new { error = "Неверная CAPTCHA" });

        var cleanText = SanitizeHtml(dto.Text);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                HomePage = dto.HomePage,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                BrowserAgent = Request.Headers.UserAgent.ToString()
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        string? filePath = null;
        string? fileType = null;
        if (file != null)
        {
            var (path, type, error) = await _fileService.SaveFileAsync(file);
            if (error != null) return BadRequest(new { error });
            filePath = path;
            fileType = type;
        }

        var comment = new Comment
        {
            Text = cleanText,
            UserId = user.Id,
            ParentId = dto.ParentId,
            FilePath = filePath,
            FileType = fileType
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        await _db.Entry(comment).Reference(c => c.User).LoadAsync();

        return Ok(MapToDto(comment, loadReplies: false));
    }

    private CommentResponseDto MapToDto(Comment comment, bool loadReplies)
    {
        if (loadReplies)
        {
            _db.Entry(comment)
               .Collection(c => c.Replies)
               .Query()
               .Include(r => r.User)
               .Load();
        }

        return new CommentResponseDto
        {
            Id = comment.Id,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            FilePath = comment.FilePath,
            FileType = comment.FileType,
            ParentId = comment.ParentId,
            User = new UserResponseDto
            {
                UserName = comment.User.UserName,
                Email = comment.User.Email,
                HomePage = comment.User.HomePage
            },
            Replies = loadReplies
                ? comment.Replies.Select(r => MapToDto(r, loadReplies: true)).ToList()
                : new List<CommentResponseDto>()
        };
    }

    private static string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var escaped = System.Net.WebUtility.HtmlEncode(input);

        escaped = Regex.Replace(escaped, @"&lt;strong&gt;", "<strong>", RegexOptions.IgnoreCase);
        escaped = Regex.Replace(escaped, @"&lt;/strong&gt;", "</strong>", RegexOptions.IgnoreCase);
        escaped = Regex.Replace(escaped, @"&lt;i&gt;", "<i>", RegexOptions.IgnoreCase);
        escaped = Regex.Replace(escaped, @"&lt;/i&gt;", "</i>", RegexOptions.IgnoreCase);
        escaped = Regex.Replace(escaped, @"&lt;code&gt;", "<code>", RegexOptions.IgnoreCase);
        escaped = Regex.Replace(escaped, @"&lt;/code&gt;", "</code>", RegexOptions.IgnoreCase);
        escaped = Regex.Replace(escaped,
            @"&lt;a\s+href=&quot;(https?://[^&""<>]+)&quot;(\s+title=&quot;([^&""<>]*)&quot;)?&gt;",
            m => {
                var href = m.Groups[1].Value;
                var title = m.Groups[3].Success ? $" title=\"{m.Groups[3].Value}\"" : "";
                return $"<a href=\"{href}\"{title}>";
            }, RegexOptions.IgnoreCase);
        escaped = Regex.Replace(escaped, @"&lt;/a&gt;", "</a>", RegexOptions.IgnoreCase);
        escaped = EnsureTagsClosed(escaped);

        return escaped;
    }

    private static string EnsureTagsClosed(string html)
    {
        var stack = new Stack<string>();
        var result = html;
        var tagPattern = new Regex(@"<(/?)(\w+)[^>]*>", RegexOptions.IgnoreCase);

        foreach (Match match in tagPattern.Matches(html))
        {
            var isClosing = match.Groups[1].Value == "/";
            var tagName = match.Groups[2].Value.ToLower();
            if (!isClosing) stack.Push(tagName);
            else if (stack.Count > 0 && stack.Peek() == tagName) stack.Pop();
        }

        while (stack.Count > 0)
            result += $"</{stack.Pop()}>";

        return result;
    }
}