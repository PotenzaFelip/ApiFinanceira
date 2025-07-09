using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiFinanceira.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IContaService _contaService;
        private readonly ICartaoService _cartaoService;

        public AccountsController(IContaService contaService, ICartaoService cartaoService)
        {
            _contaService = contaService;
            _cartaoService = cartaoService;
        }

        private Guid GetPessoaIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid pessoaId))
            {
                throw new InvalidOperationException("ID da pessoa não encontrado ou inválido no token JWT.");
            }
            return pessoaId;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateContaRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var response = await _contaService.CreateContaAsync(pessoaId, request);

                if (response == null)
                {
                    return Conflict(new { message = "Não foi possível criar a conta. Verifique se o número da conta já existe ou se a pessoa é válida." });
                }

                return CreatedAtAction(nameof(GetAccounts), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContaResponse>>> GetAccounts()
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var contas = await _contaService.GetContasByPessoaIdAsync(pessoaId);

                return Ok(contas);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
