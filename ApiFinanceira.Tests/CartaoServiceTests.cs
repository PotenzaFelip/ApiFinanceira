using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.Services;
using ApiFinanceira.Application.Services.ApiFinanceira.Application.Services;
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
    public class CartaoServiceTests
    {
        private readonly Mock<ICartaoRepository> _mockCartaoRepository;
        private readonly Mock<IContaRepository> _mockContaRepository;
        private readonly Mock<IPessoaRepository> _mockPessoaRepository;
        private readonly CartaoService _cartaoService;

        public CartaoServiceTests()
        {
            _mockCartaoRepository = new Mock<ICartaoRepository>();
            _mockContaRepository = new Mock<IContaRepository>();
            _mockPessoaRepository = new Mock<IPessoaRepository>();

            _cartaoService = new CartaoService(
                _mockCartaoRepository.Object,
                _mockContaRepository.Object,
                _mockPessoaRepository.Object
            );
        }

        private string MascararUltimosQuatroDigitos(string numeroCompleto)
        {
            if (string.IsNullOrEmpty(numeroCompleto) || numeroCompleto.Length < 4)
            {
                return numeroCompleto;
            }
            return numeroCompleto.Substring(numeroCompleto.Length - 4);
        }

        #region Testes_CriarCartaoAsync

        [Fact]
        public async Task CriarCartaoAsync_DeveRetornarCartaoResponse_QuandoCartaoVirtualValido()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var requisicao = new CreateCartaoRequest
            {
                Type = "virtual",
                Number = "1111222233334444",
                Cvv = "123"
            };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);
            _mockCartaoRepository.Setup(r => r.GetPhysicalCardByContaIdAsync(contaId)).ReturnsAsync((Cartao?)null);
            _mockCartaoRepository.Setup(r => r.GetByCardNumberAsync(requisicao.Number)).ReturnsAsync((Cartao?)null);
            _mockCartaoRepository.Setup(r => r.AddAsync(It.IsAny<Cartao>())).Returns(Task.CompletedTask);
            _mockCartaoRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var resultado = await _cartaoService.CreateCardAsync(pessoaId, contaId, requisicao);

            Assert.NotNull(resultado);
            Assert.NotEqual(Guid.Empty, resultado.Id);
            Assert.Equal("virtual", resultado.Type);
            Assert.Equal(MascararUltimosQuatroDigitos(requisicao.Number), resultado.Number);
            Assert.Equal(requisicao.Cvv, resultado.Cvv);
            Assert.True(resultado.CreatedAt > DateTime.MinValue);
            Assert.True(resultado.UpdatedAt > DateTime.MinValue);

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetPhysicalCardByContaIdAsync(contaId), Times.Never);
            _mockCartaoRepository.Verify(r => r.GetByCardNumberAsync(requisicao.Number), Times.Once);
            _mockCartaoRepository.Verify(r => r.AddAsync(It.IsAny<Cartao>()), Times.Once);
            _mockCartaoRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarCartaoAsync_DeveRetornarCartaoResponse_QuandoCartaoFisicoValidoENaoExisteCartaoFisico()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var requisicao = new CreateCartaoRequest
            {
                Type = "physical",
                Number = "1111222233334444",
                Cvv = "123"
            };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);
            _mockCartaoRepository.Setup(r => r.GetPhysicalCardByContaIdAsync(contaId)).ReturnsAsync((Cartao?)null);
            _mockCartaoRepository.Setup(r => r.GetByCardNumberAsync(requisicao.Number)).ReturnsAsync((Cartao?)null);
            _mockCartaoRepository.Setup(r => r.AddAsync(It.IsAny<Cartao>())).Returns(Task.CompletedTask);
            _mockCartaoRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var resultado = await _cartaoService.CreateCardAsync(pessoaId, contaId, requisicao);

            Assert.NotNull(resultado);
            Assert.Equal("physical", resultado.Type);
            Assert.Equal(MascararUltimosQuatroDigitos(requisicao.Number), resultado.Number);

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetPhysicalCardByContaIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetByCardNumberAsync(requisicao.Number), Times.Once);
            _mockCartaoRepository.Verify(r => r.AddAsync(It.IsAny<Cartao>()), Times.Once);
            _mockCartaoRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarCartaoAsync_DeveRetornarNulo_QuandoContaNaoEncontrada()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var requisicao = new CreateCartaoRequest { Type = "virtual", Number = "1111222233334444", Cvv = "123" };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync((Conta?)null);

            var resultado = await _cartaoService.CreateCardAsync(pessoaId, contaId, requisicao);

            Assert.Null(resultado);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.AddAsync(It.IsAny<Cartao>()), Times.Never);
        }

        [Fact]
        public async Task CriarCartaoAsync_DeveRetornarNulo_QuandoPessoaIdNaoCorresponde()
        {
            var pessoaId = Guid.NewGuid();
            var pessoaIdErrado = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var requisicao = new CreateCartaoRequest { Type = "virtual", Number = "1111222233334444", Cvv = "123" };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaIdErrado };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);

            var resultado = await _cartaoService.CreateCardAsync(pessoaId, contaId, requisicao);

            Assert.Null(resultado);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.AddAsync(It.IsAny<Cartao>()), Times.Never);
        }

        [Theory]
        [InlineData("123456789012345")]
        [InlineData("12345678901234567")]
        [InlineData("123456789012345A")]
        public async Task CriarCartaoAsync_DeveLancarArgumentException_QuandoNumeroCartaoComprimentoInvalido(string numeroInvalido)
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var requisicao = new CreateCartaoRequest
            {
                Type = "virtual",
                Number = numeroInvalido,
                Cvv = "123"
            };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);

            var excecao = await Assert.ThrowsAsync<ArgumentException>(() =>
                _cartaoService.CreateCardAsync(pessoaId, contaId, requisicao));

            Assert.Contains("O número do cartão deve conter 16 dígitos.", excecao.Message);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.AddAsync(It.IsAny<Cartao>()), Times.Never);
        }

        [Theory]
        [InlineData("12")]
        [InlineData("1234")]
        [InlineData("12A")]
        public async Task CriarCartaoAsync_DeveLancarArgumentException_QuandoCvvInvalido(string cvvInvalido)
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var requisicao = new CreateCartaoRequest
            {
                Type = "virtual",
                Number = "1111222233334444",
                Cvv = cvvInvalido
            };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);

            var excecao = await Assert.ThrowsAsync<ArgumentException>(() =>
                _cartaoService.CreateCardAsync(pessoaId, contaId, requisicao));

            Assert.Contains("O CVV deve conter exatos 3 dígitos numéricos.", excecao.Message);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.AddAsync(It.IsAny<Cartao>()), Times.Never);
        }

        [Fact]
        public async Task CriarCartaoAsync_DeveLancarInvalidOperationException_QuandoExisteCartaoFisico()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var requisicao = new CreateCartaoRequest
            {
                Type = "physical",
                Number = "1111222233334444",
                Cvv = "123"
            };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };
            var cartaoFisicoExistente = new Cartao { ContaId = contaId, Type = "physical" };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);
            _mockCartaoRepository.Setup(r => r.GetPhysicalCardByContaIdAsync(contaId)).ReturnsAsync(cartaoFisicoExistente);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _cartaoService.CreateCardAsync(pessoaId, contaId, requisicao));

            Assert.Contains("Uma conta só pode ter um cartão físico.", excecao.Message);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetPhysicalCardByContaIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.AddAsync(It.IsAny<Cartao>()), Times.Never);
        }

        [Fact]
        public async Task CriarCartaoAsync_DeveLancarInvalidOperationException_QuandoNumeroCartaoJaEmUso()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var requisicao = new CreateCartaoRequest
            {
                Type = "virtual",
                Number = "1111222233334444",
                Cvv = "123"
            };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };
            var cartaoExistenteComMesmoNumero = new Cartao { ContaId = Guid.NewGuid(), Number = requisicao.Number };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);
            _mockCartaoRepository.Setup(r => r.GetPhysicalCardByContaIdAsync(contaId)).ReturnsAsync((Cartao?)null);
            _mockCartaoRepository.Setup(r => r.GetByCardNumberAsync(requisicao.Number)).ReturnsAsync(cartaoExistenteComMesmoNumero);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _cartaoService.CreateCardAsync(pessoaId, contaId, requisicao));

            Assert.Contains("Este número de cartão já está em uso.", excecao.Message);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetPhysicalCardByContaIdAsync(contaId), Times.Never);
            _mockCartaoRepository.Verify(r => r.GetByCardNumberAsync(requisicao.Number), Times.Once);
            _mockCartaoRepository.Verify(r => r.AddAsync(It.IsAny<Cartao>()), Times.Never);
        }

        #endregion

        #region Testes_ObterCartoesPorContaIdAsync

        [Fact]
        public async Task ObterCartoesPorContaIdAsync_DeveRetornarListaDeCartaoResponses_QuandoCartoesExistem()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };
            var cartoes = new List<Cartao>
            {
                new Cartao { Id = Guid.NewGuid(), ContaId = contaId, Type = "virtual", Number = "1111222233330001", Cvv = "001" },
                new Cartao { Id = Guid.NewGuid(), ContaId = contaId, Type = "physical", Number = "1111222233330002", Cvv = "002" }
            };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);
            _mockCartaoRepository.Setup(r => r.GetByContaIdAsync(contaId)).ReturnsAsync(cartoes);

            var resultado = await _cartaoService.GetCardsByContaIdAsync(pessoaId, contaId);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count());
            Assert.Contains(resultado, c => c.Type == "virtual" && c.Number == MascararUltimosQuatroDigitos("1111222233330001"));
            Assert.Contains(resultado, c => c.Type == "physical" && c.Number == MascararUltimosQuatroDigitos("1111222233330002"));

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetByContaIdAsync(contaId), Times.Once);
        }

        [Fact]
        public async Task ObterCartoesPorContaIdAsync_DeveRetornarListaVazia_QuandoNenhumCartaoExiste()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);
            _mockCartaoRepository.Setup(r => r.GetByContaIdAsync(contaId)).ReturnsAsync(new List<Cartao>());

            var resultado = await _cartaoService.GetCardsByContaIdAsync(pessoaId, contaId);

            Assert.NotNull(resultado);
            Assert.Empty(resultado);

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetByContaIdAsync(contaId), Times.Once);
        }

        [Fact]
        public async Task ObterCartoesPorContaIdAsync_DeveRetornarListaVazia_QuandoContaNaoEncontrada()
        {
            var pessoaId = Guid.NewGuid();
            var contaId = Guid.NewGuid();

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync((Conta?)null);

            var resultado = await _cartaoService.GetCardsByContaIdAsync(pessoaId, contaId);

            Assert.NotNull(resultado);
            Assert.Empty(resultado);

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetByContaIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ObterCartoesPorContaIdAsync_DeveRetornarListaVazia_QuandoContaPessoaIdNaoCorresponde()
        {
            var pessoaId = Guid.NewGuid();
            var pessoaIdErrado = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaIdErrado };

            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);

            var resultado = await _cartaoService.GetCardsByContaIdAsync(pessoaId, contaId);

            Assert.NotNull(resultado);
            Assert.Empty(resultado);

            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetByContaIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region Testes_ObterCartoesPorPessoaIdAsync

        [Fact]
        public async Task ObterCartoesPorPessoaIdAsync_DeveRetornarCartaoResponsePaginado_QuandoCartoesExistem()
        {
            var pessoaId = Guid.NewGuid();
            var itensPorPagina = 2;
            var paginaAtual = 1;
            var totalItens = 3;
            var cartoes = new List<Cartao>
            {
                new Cartao { Id = Guid.NewGuid(), ContaId = Guid.NewGuid(), Type = "virtual", Number = "1111222233330001", Cvv = "001" },
                new Cartao { Id = Guid.NewGuid(), ContaId = Guid.NewGuid(), Type = "physical", Number = "1111222233330002", Cvv = "002" }
            };

            _mockCartaoRepository.Setup(r => r.CountCardsByPessoaIdAsync(pessoaId)).ReturnsAsync(totalItens);
            _mockCartaoRepository.Setup(r => r.GetPagedCardsByPessoaIdAsync(pessoaId, 0, itensPorPagina)).ReturnsAsync(cartoes);

            var resultado = await _cartaoService.GetCardsByPessoaIdAsync(pessoaId, itensPorPagina, paginaAtual);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Cards.Count());
            Assert.Equal(totalItens, resultado.Pagination.TotalItems);
            Assert.Equal(paginaAtual, resultado.Pagination.CurrentPage);
            Assert.Equal(itensPorPagina, resultado.Pagination.ItemsPerPage);
            Assert.Equal(2, resultado.Pagination.TotalPages);

            _mockCartaoRepository.Verify(r => r.CountCardsByPessoaIdAsync(pessoaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetPagedCardsByPessoaIdAsync(pessoaId, 0, itensPorPagina), Times.Once);
        }

        [Fact]
        public async Task ObterCartoesPorPessoaIdAsync_DeveRetornarCartaoResponsePaginadoVazio_QuandoNenhumCartaoExiste()
        {
            var pessoaId = Guid.NewGuid();
            var itensPorPagina = 10;
            var paginaAtual = 1;
            var totalItens = 0;

            _mockCartaoRepository.Setup(r => r.CountCardsByPessoaIdAsync(pessoaId)).ReturnsAsync(totalItens);
            _mockCartaoRepository.Setup(r => r.GetPagedCardsByPessoaIdAsync(pessoaId, 0, itensPorPagina)).ReturnsAsync(new List<Cartao>());

            var resultado = await _cartaoService.GetCardsByPessoaIdAsync(pessoaId, itensPorPagina, paginaAtual);

            Assert.NotNull(resultado);
            Assert.Empty(resultado.Cards);
            Assert.Equal(0, resultado.Pagination.TotalItems);
            Assert.Equal(1, resultado.Pagination.TotalPages);

            _mockCartaoRepository.Verify(r => r.CountCardsByPessoaIdAsync(pessoaId), Times.Once);
            _mockCartaoRepository.Verify(r => r.GetPagedCardsByPessoaIdAsync(pessoaId, 0, itensPorPagina), Times.Once);
        }

        [Theory]
        [InlineData(0, 1, 10, 1)]
        [InlineData(10, 0, 10, 1)]
        [InlineData(-5, -5, 10, 1)]
        public async Task ObterCartoesPorPessoaIdAsync_DeveLidarComParametrosDePaginacaoInvalidos(int itensPorPaginaInvalidos, int paginaAtualInvalida, int itensPorPaginaEsperados, int paginaAtualEsperada)
        {
            var pessoaId = Guid.NewGuid();
            var totalItens = 5;
            var cartoes = new List<Cartao> { new Cartao(), new Cartao(), new Cartao(), new Cartao(), new Cartao() };

            _mockCartaoRepository.Setup(r => r.CountCardsByPessoaIdAsync(pessoaId)).ReturnsAsync(totalItens);
            _mockCartaoRepository.Setup(r => r.GetPagedCardsByPessoaIdAsync(pessoaId, (paginaAtualEsperada - 1) * itensPorPaginaEsperados, itensPorPaginaEsperados)).ReturnsAsync(cartoes.Take(itensPorPaginaEsperados));

            var resultado = await _cartaoService.GetCardsByPessoaIdAsync(pessoaId, itensPorPaginaInvalidos, paginaAtualInvalida);

            Assert.NotNull(resultado);
            Assert.Equal(itensPorPaginaEsperados, resultado.Pagination.ItemsPerPage);
            Assert.Equal(paginaAtualEsperada, resultado.Pagination.CurrentPage);
            Assert.Equal(totalItens, resultado.Pagination.TotalItems);
            _mockCartaoRepository.Verify(r => r.GetPagedCardsByPessoaIdAsync(pessoaId, (paginaAtualEsperada - 1) * itensPorPaginaEsperados, itensPorPaginaEsperados), Times.Once);
        }

        #endregion

        #region Testes_ObterCartaoPorIdEContaIdEPessoaIdAsync

        [Fact]
        public async Task ObterCartaoPorIdEContaIdEPessoaIdAsync_DeveRetornarCartaoResponse_QuandoTodosOsIdsCorrespondem()
        {
            var cartaoId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var pessoaId = Guid.NewGuid();
            var mockCartao = new Cartao { Id = cartaoId, ContaId = contaId, Type = "virtual", Number = "1111222233335555", Cvv = "555" };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaId };

            _mockCartaoRepository.Setup(r => r.GetByIdAsync(cartaoId)).ReturnsAsync(mockCartao);
            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);

            var resultado = await _cartaoService.GetCardByIdAndContaIdAndPessoaIdAsync(cartaoId, contaId, pessoaId);

            Assert.NotNull(resultado);
            Assert.Equal(cartaoId, resultado.Id);
            Assert.Equal(mockCartao.Type, resultado.Type);
            Assert.Equal(MascararUltimosQuatroDigitos(mockCartao.Number), resultado.Number);
            Assert.Equal(mockCartao.Cvv, resultado.Cvv);

            _mockCartaoRepository.Verify(r => r.GetByIdAsync(cartaoId), Times.Once);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
        }

        [Fact]
        public async Task ObterCartaoPorIdEContaIdEPessoaIdAsync_DeveRetornarNulo_QuandoCartaoNaoEncontrado()
        {
            var cartaoId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var pessoaId = Guid.NewGuid();

            _mockCartaoRepository.Setup(r => r.GetByIdAsync(cartaoId)).ReturnsAsync((Cartao?)null);

            var resultado = await _cartaoService.GetCardByIdAndContaIdAndPessoaIdAsync(cartaoId, contaId, pessoaId);

            Assert.Null(resultado);

            _mockCartaoRepository.Verify(r => r.GetByIdAsync(cartaoId), Times.Once);
            _mockContaRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ObterCartaoPorIdEContaIdEPessoaIdAsync_DeveRetornarNulo_QuandoContaIdDoCartaoNaoCorresponde()
        {
            var cartaoId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var contaIdErrada = Guid.NewGuid();
            var pessoaId = Guid.NewGuid();
            var mockCartao = new Cartao { Id = cartaoId, ContaId = contaIdErrada };

            _mockCartaoRepository.Setup(r => r.GetByIdAsync(cartaoId)).ReturnsAsync(mockCartao);

            var resultado = await _cartaoService.GetCardByIdAndContaIdAndPessoaIdAsync(cartaoId, contaId, pessoaId);

            Assert.Null(resultado);

            _mockCartaoRepository.Verify(r => r.GetByIdAsync(cartaoId), Times.Once);
            _mockContaRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ObterCartaoPorIdEContaIdEPessoaIdAsync_DeveRetornarNulo_QuandoContaNaoEncontrada()
        {
            var cartaoId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var pessoaId = Guid.NewGuid();
            var mockCartao = new Cartao { Id = cartaoId, ContaId = contaId };

            _mockCartaoRepository.Setup(r => r.GetByIdAsync(cartaoId)).ReturnsAsync(mockCartao);
            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync((Conta?)null);
            var resultado = await _cartaoService.GetCardByIdAndContaIdAndPessoaIdAsync(cartaoId, contaId, pessoaId);

            Assert.Null(resultado);

            _mockCartaoRepository.Verify(r => r.GetByIdAsync(cartaoId), Times.Once);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
        }

        [Fact]
        public async Task ObterCartaoPorIdEContaIdEPessoaIdAsync_DeveRetornarNulo_QuandoContaPessoaIdNaoCorresponde()
        {
            var cartaoId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var pessoaId = Guid.NewGuid();
            var pessoaIdErrada = Guid.NewGuid();
            var mockCartao = new Cartao { Id = cartaoId, ContaId = contaId };
            var mockConta = new Conta { Id = contaId, PessoaId = pessoaIdErrada };

            _mockCartaoRepository.Setup(r => r.GetByIdAsync(cartaoId)).ReturnsAsync(mockCartao);
            _mockContaRepository.Setup(r => r.GetByIdAsync(contaId)).ReturnsAsync(mockConta);

            var resultado = await _cartaoService.GetCardByIdAndContaIdAndPessoaIdAsync(cartaoId, contaId, pessoaId);

            Assert.Null(resultado);

            _mockCartaoRepository.Verify(r => r.GetByIdAsync(cartaoId), Times.Once);
            _mockContaRepository.Verify(r => r.GetByIdAsync(contaId), Times.Once);
        }

        #endregion
    }
}