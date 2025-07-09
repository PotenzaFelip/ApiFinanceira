using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
        private readonly ITransacaoService _transacaoService;

        public AccountsController(IContaService contaService, ICartaoService cartaoService, ITransacaoService transacaoService)
        {
            _contaService = contaService;
            _cartaoService = cartaoService;
            _transacaoService = transacaoService;
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
        [HttpGet("{accountId}/balance")]
        [ProducesResponseType(typeof(BalanceResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAccountBalance(Guid accountId)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var balance = await _contaService.GetAccountBalanceAsync(pessoaId, accountId);
                return Ok(new BalanceResponse { Balance = balance });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao obter saldo: {ex.Message}" });
            }
        }

        public class BalanceResponse
        {
            public decimal Balance { get; set; }
        }
        [HttpPost("{accountId}/transactions/{transactionId}/revert")]
        [ProducesResponseType(typeof(TransacaoResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RevertTransaction(Guid accountId, Guid transactionId, [FromBody] RevertTransactionRequest request)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var revertedTransaction = await _transacaoService.RevertTransactionAsync(pessoaId, accountId, transactionId, request.Description);
                return Ok(revertedTransaction);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao reverter transação: {ex.Message}" });
            }
        }        
        public class RevertTransactionRequest
        {
            [Required(ErrorMessage = "A descrição da reversão é obrigatória.")]
            public string Description { get; set; } = string.Empty;
        }
    }

}  

