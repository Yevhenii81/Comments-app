namespace backend.DTOs;

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public int? ParentId { get; set; }
    public UserResponseDto User { get; set; } = null!;
    public List<CommentResponseDto> Replies { get; set; } = new();
}

public class UserResponseDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? HomePage { get; set; }
}