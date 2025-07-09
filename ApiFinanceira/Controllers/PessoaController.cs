using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiFinanceira.Controllers
{
    [ApiController] 
    [Route("api/people")]
    public class PessoaController : ControllerBase
    {
        private readonly IPessoaService _pessoaService;

      
        public PessoaController(IPessoaService pessoaService)
        {
            _pessoaService = pessoaService;
        }


        [HttpPost()]
        [ProducesResponseType(typeof(PessoaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterPessoa([FromBody] CreatePessoaRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pessoaResponse = await _pessoaService.RegisterPessoaAsync(request);

            if (pessoaResponse == null)
            {
                return Conflict(new { message = "Pessoa com este documento já existe." });
            }
            
            return CreatedAtAction(nameof(GetPessoaById), new { id = pessoaResponse.Id }, pessoaResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PessoaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPessoaById(Guid id)
        {
            var pessoaResponse = await _pessoaService.GetPessoaByIdAsync(id);

            if (pessoaResponse == null)
            {
                return NotFound();
            }

            return Ok(pessoaResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("document/{document}")]
        [ProducesResponseType(typeof(PessoaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPessoaByDocument(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
            {
                return BadRequest("O documento não pode ser vazio.");
            }

            var pessoaResponse = await _pessoaService.GetPessoaByDocumentAsync(document);

            if (pessoaResponse == null)
            {
                return NotFound();
            }

            return Ok(pessoaResponse);
        }
    }
}
