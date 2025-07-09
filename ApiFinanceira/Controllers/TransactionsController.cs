using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiFinanceira.Controllers
{
    [ApiController]
    [Route("api/accounts/{accountId}/transactions")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransacaoService _transacaoService;

        public TransactionsController(ITransacaoService transacaoService)
        {
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
        public async Task<ActionResult<TransacaoResponse>> CreateTransaction(
            Guid accountId,
            [FromBody] CreateTransacaoRequest request)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var transacaoResponse = await _transacaoService.CreateTransactionAsync(pessoaId, accountId, request);

                if (transacaoResponse == null)
                {
                    return BadRequest("Não foi possível criar a transação. Verifique os dados.");
                }
                return Ok(transacaoResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao criar transação: {ex.Message}" });
            }
        }

        
        [HttpPost("internal")] 
        public async Task<ActionResult<TransacaoResponse>> CreateInternalTransfer(
            Guid accountId,
            [FromBody] CreateTransferenciaRequest request)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var transacaoResponse = await _transacaoService.CreateInternalTransferAsync(pessoaId, accountId, request);

                if (transacaoResponse == null)
                {
                    return BadRequest("Não foi possível criar a transferência interna. Verifique os dados.");
                }
                return Ok(transacaoResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao criar transferência: {ex.Message}" });
            }
        }
     
        [HttpGet]
        public async Task<ActionResult<PagedTransacaoResponse>> GetTransactions(
            Guid accountId,
            [FromQuery] int itemsPerPage = 10,
            [FromQuery] int currentPage = 1,
            [FromQuery] string? type = null)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var pagedResponse = await _transacaoService.GetTransactionsByContaIdAsync(
                    pessoaId, accountId, itemsPerPage, currentPage, type);

                if (pagedResponse == null || !pagedResponse.Transactions.Any())
                {
                    return Ok(new PagedTransacaoResponse
                    {
                        Transactions = new List<TransacaoResponse>(),
                        Pagination = new PaginationInfo
                        {
                            ItemsPerPage = itemsPerPage,
                            CurrentPage = currentPage,
                            TotalItems = 0
                        }
                    });
                }

                return Ok(pagedResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao listar transações: {ex.Message}" });
            }
        }
    }
}
