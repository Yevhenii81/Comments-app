using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaService _captchaService;
    public CaptchaController(ICaptchaService captchaService) => _captchaService = captchaService;

    [HttpGet]
    public IActionResult GetCaptcha()
    {
        var (token, imageBytes) = _captchaService.GenerateCaptcha();
        return Ok(new
        {
            token,
            imageBase64 = Convert.ToBase64String(imageBytes)
        });
    }
}