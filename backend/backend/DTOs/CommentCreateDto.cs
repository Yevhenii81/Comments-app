using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CommentCreateDto
{
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Только латинские буквы и цифры")]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Url(ErrorMessage = "Неверный формат URL")]
    public string? HomePage { get; set; }

    [Required]
    public string CaptchaToken { get; set; } = string.Empty;

    [Required]
    public string CaptchaAnswer { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Text { get; set; } = string.Empty;

    public int? ParentId { get; set; }
}