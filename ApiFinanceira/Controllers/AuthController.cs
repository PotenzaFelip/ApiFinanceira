using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiFinanceira.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Documento ou senha inválidos." });
            }

            response.Token = $"Bearer {response.Token}";

            return Ok(response);
        }
    }
}
