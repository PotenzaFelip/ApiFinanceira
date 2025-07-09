using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiFinanceira.Controllers
{
    [ApiController]
    //[Route("api/accounts/{accountId}/cards")]
    [Authorize]
    public class CardsController : ControllerBase
    {
        private readonly ICartaoService _cartaoService;

        public CardsController(ICartaoService cartaoService)
        {
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

        [Route("api/accounts/{accountId}/cards")]
        [HttpPost]
        public async Task<ActionResult<CartaoResponse>> CreateCard(Guid accountId, [FromBody] CreateCartaoRequest request)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();

                var cartaoResponse = await _cartaoService.CreateCardAsync(pessoaId, accountId, request);

                if (cartaoResponse == null)
                {
                    
                    return BadRequest("Não foi possível criar o cartão. Verifique os dados e tente novamente.");
                }

               
                return CreatedAtAction(nameof(GetCardById), new { accountId = accountId, cardId = cartaoResponse.Id }, cartaoResponse);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao criar cartão: {ex.Message}" });
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("{cardId}")]
        public async Task<ActionResult<CartaoResponse>> GetCardById(Guid accountId, Guid cardId)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var cartao=0;

                return Ok(cartao);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao buscar cartão: {ex.Message}" });
            }
        }

        [HttpGet("api/accounts/{accountId}/cards")]
        public async Task<ActionResult<IEnumerable<CartaoResponse>>> GetCardsByAccount(Guid accountId)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var cartoes = await _cartaoService.GetCardsByContaIdAsync(pessoaId, accountId);

                if (cartoes == null || !cartoes.Any())
                {
                    return Ok(new List<CartaoResponse>());
                }

                return Ok(cartoes);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao listar cartões da conta: {ex.Message}" });
            }
        }

        [HttpGet("api/cards")]
        public async Task<ActionResult<PagedCardResponse>> GetCards(
           [FromQuery] int itemsPerPage = 10,
           [FromQuery] int currentPage = 1)
        {
            try
            {
                var pessoaId = GetPessoaIdFromClaims();
                var pagedResponse = await _cartaoService.GetCardsByPessoaIdAsync(pessoaId, itemsPerPage, currentPage);

                if (pagedResponse == null || !pagedResponse.Cards.Any())
                {                    
                    return Ok(new PagedCardResponse
                    {
                        Cards = new List<CartaoResponse>(),
                        Pagination = new PaginationInfo
                        {
                            ItemsPerPage = itemsPerPage,
                            CurrentPage = currentPage,
                            TotalItems = 0,                            
                        }
                    });
                }

                return Ok(pagedResponse);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao listar cartões: {ex.Message}" });
            }
        }
    }
}
