using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.Services;
using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ApiFinanceira.Tests.ApplicationTests
{
    public class ContaServiceTests
    {
        private readonly Mock<IPessoaRepository> _mockPessoaRepository;
        private readonly Mock<IContaRepository> _mockContaRepository;
        private readonly ContaService _contaService;

        public ContaServiceTests()
        {
            _mockContaRepository = new Mock<IContaRepository>();
            _mockPessoaRepository = new Mock<IPessoaRepository>();

            _contaService = new ContaService(
                _mockContaRepository.Object,
                _mockPessoaRepository.Object
            );
        }

        [Fact]
        public async Task CriarContaAsync_DeveRetornarContaResponse_QuandoPessoaExisteERequisicaoValida()
        {
            var pessoaId = Guid.NewGuid();
            var requisicao = new CreateContaRequest { Branch = "0001", Account = "12345-6" };

            var mockPessoa = new Pessoa { Id = pessoaId, Nome = "Pessoa Teste" };

            var expectedContaId = Guid.NewGuid();
            var expectedCreatedAt = DateTime.UtcNow;
            var expectedUpdatedAt = DateTime.UtcNow;

            _mockPessoaRepository.Setup(r => r.GetByIdAsync(pessoaId))
                                 .ReturnsAsync(mockPessoa);

            _mockContaRepository.Setup(r => r.AddAsync(It.IsAny<Conta>()))
                                 .Returns(Task.CompletedTask);

            _mockContaRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var resultado = await _contaService.CreateContaAsync(pessoaId, requisicao);

            Assert.NotNull(resultado);
            Assert.NotEqual(Guid.Empty, resultado.Id);
            Assert.Equal(requisicao.Branch, resultado.Branch);
            Assert.Equal(requisicao.Account, resultado.Account);
            Assert.True(resultado.CreatedAt > DateTime.MinValue);
            Assert.True(resultado.UpdatedAt > DateTime.MinValue);


            _mockPessoaRepository.Verify(r => r.GetByIdAsync(pessoaId), Times.Once);
            _mockContaRepository.Verify(r => r.AddAsync(It.IsAny<Conta>()), Times.Once);
            _mockContaRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarContaAsync_DeveRetornarNulo_QuandoPessoaNaoExiste()
        {
            var pessoaId = Guid.NewGuid();
            var requisicao = new CreateContaRequest { Branch = "0001", Account = "12345-6" };

            _mockPessoaRepository.Setup(r => r.GetByIdAsync(pessoaId))
                                 .ReturnsAsync((Pessoa?)null);

            var resultado = await _contaService.CreateContaAsync(pessoaId, requisicao);

            Assert.Null(resultado);

            _mockPessoaRepository.Verify(r => r.GetByIdAsync(pessoaId), Times.Once);
            _mockContaRepository.Verify(r => r.AddAsync(It.IsAny<Conta>()), Times.Never);
            _mockContaRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ObterContasPorPessoaIdAsync_DeveRetornarListaDeContaResponses_QuandoContasExistem()
        {
            var pessoaId = Guid.NewGuid();
            var listaContas = new List<Conta>
            {
                new Conta { Id = Guid.NewGuid(), PessoaId = pessoaId, Branch = "0001", Account = "11111-1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Conta { Id = Guid.NewGuid(), PessoaId = pessoaId, Branch = "0001", Account = "22222-2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            _mockContaRepository.Setup(r => r.GetByPessoaIdAsync(pessoaId))
                                 .ReturnsAsync(listaContas);

            var resultado = await _contaService.GetContasByPessoaIdAsync(pessoaId);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count());
            Assert.Contains(resultado, c => c.Account == "11111-1" && c.Branch == "0001");
            Assert.Contains(resultado, c => c.Account == "22222-2" && c.Branch == "0001");

            _mockContaRepository.Verify(r => r.GetByPessoaIdAsync(pessoaId), Times.Once);
        }

        [Fact]
        public async Task ObterContasPorPessoaIdAsync_DeveRetornarListaVazia_QuandoNenhumaContaExiste()
        {
            var pessoaId = Guid.NewGuid();
            _mockContaRepository.Setup(r => r.GetByPessoaIdAsync(pessoaId))
                                 .ReturnsAsync(new List<Conta>());

            var resultado = await _contaService.GetContasByPessoaIdAsync(pessoaId);

            Assert.NotNull(resultado);
            Assert.Empty(resultado);

            _mockContaRepository.Verify(r => r.GetByPessoaIdAsync(pessoaId), Times.Once);
        }

        [Fact]
        public async Task ObterSaldoContaAsync_DeveRetornarSaldoCorreto_QuandoContaExisteECorrespondeAPessoa()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var saldoEsperado = 500.50m;

            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId, Saldo = saldoEsperado };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId))
                                 .ReturnsAsync(mockConta);

            var resultado = await _contaService.GetAccountBalanceAsync(pessoaId, contaId);

            Assert.Equal(saldoEsperado, resultado);

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
        }

        [Fact]
        public async Task ObterSaldoContaAsync_DeveLancarUnauthorizedAccessException_QuandoContaNaoEncontrada()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId))
                                 .ReturnsAsync((Conta?)null);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _contaService.GetAccountBalanceAsync(pessoaId, contaId));

            Assert.Contains("Conta não encontrada ou você não tem permissão para acessá-la.", excecao.Message);

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
        }

        [Fact]
        public async Task ObterSaldoContaAsync_DeveLancarUnauthorizedAccessException_QuandoPessoaIdDaContaNaoCorresponde()
        {
            var pessoaId = Guid.NewGuid();
            var pessoaIdErrado = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var saldoEsperado = 1000m;

            var mockConta = new Conta { Id = contaId, PessoaId = pessoaIdErrado, Saldo = saldoEsperado };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId))
                                 .ReturnsAsync(mockConta);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _contaService.GetAccountBalanceAsync(pessoaId, contaId));

            Assert.Contains("Conta não encontrada ou você não tem permissão para acessá-la.", excecao.Message);

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
        }
    }
}