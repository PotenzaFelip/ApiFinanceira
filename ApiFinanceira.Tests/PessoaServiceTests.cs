using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.ExternalServices;
using ApiFinanceira.Application.Services;
using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ApiFinanceira.Tests.ApplicationTests
{
    public class PessoaServiceTests
    {
        private readonly Mock<IPessoaRepository> _mockPessoaRepository;
        private readonly Mock<IComplianceService> _mockComplianceService;
        private readonly PessoaService _pessoaService;

        public PessoaServiceTests()
        {
            _mockPessoaRepository = new Mock<IPessoaRepository>();
            _mockComplianceService = new Mock<IComplianceService>();
            _pessoaService = new PessoaService(_mockPessoaRepository.Object, _mockComplianceService.Object);
        }

        private string SimularHashSenha(string senha)
        {
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(senha)));
        }

        #region Testes_RegistrarPessoaAsync

        [Fact]
        public async Task RegistrarPessoaAsync_DeveRetornarPessoaResponse_QuandoCpfValidoEComplianceSucesso()
        {
            var requisicao = new CreatePessoaRequest
            {
                Name = "João Silva",
                Document = "123.456.789-00",
                Password = "senhaSegura123"
            };
            var documentoLimpo = "12345678900";
            var tipoDocumento = "cpf";

            _mockPessoaRepository.Setup(r => r.GetByDocumentAsync(documentoLimpo)).ReturnsAsync((Pessoa?)null);
            _mockComplianceService.Setup(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento))
                .ReturnsAsync(new ComplianceResponse { Success = true });
            _mockPessoaRepository.Setup(r => r.AddAsync(It.IsAny<Pessoa>())).Returns(Task.CompletedTask);
            _mockPessoaRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var resultado = await _pessoaService.RegisterPessoaAsync(requisicao);

            Assert.NotNull(resultado);
            Assert.NotEqual(Guid.Empty, resultado.Id);
            Assert.Equal(requisicao.Name, resultado.Name);
            Assert.Equal(documentoLimpo, resultado.Document);
            Assert.True(resultado.CreatedAt > DateTime.MinValue);
            Assert.True(resultado.UpdatedAt > DateTime.MinValue);

            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpo), Times.Once);
            _mockComplianceService.Verify(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento), Times.Once);
            _mockPessoaRepository.Verify(r => r.AddAsync(It.Is<Pessoa>(p =>
                p.Nome == requisicao.Name &&
                p.Documento == documentoLimpo &&
                p.SenhaHash != null && p.SenhaHash != string.Empty)), Times.Once);
            _mockPessoaRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegistrarPessoaAsync_DeveRetornarPessoaResponse_QuandoCnpjValidoEComplianceSucesso()
        {
            var requisicao = new CreatePessoaRequest
            {
                Name = "Empresa Teste LTDA",
                Document = "11.222.333/0001-44",
                Password = "senhaDaEmpresa"
            };
            var documentoLimpo = "11222333000144";
            var tipoDocumento = "cnpj";

            _mockPessoaRepository.Setup(r => r.GetByDocumentAsync(documentoLimpo)).ReturnsAsync((Pessoa?)null);
            _mockComplianceService.Setup(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento))
                .ReturnsAsync(new ComplianceResponse { Success = true });
            _mockPessoaRepository.Setup(r => r.AddAsync(It.IsAny<Pessoa>())).Returns(Task.CompletedTask);
            _mockPessoaRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var resultado = await _pessoaService.RegisterPessoaAsync(requisicao);

            Assert.NotNull(resultado);
            Assert.NotEqual(Guid.Empty, resultado.Id);
            Assert.Equal(requisicao.Name, resultado.Name);
            Assert.Equal(documentoLimpo, resultado.Document);

            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpo), Times.Once);
            _mockComplianceService.Verify(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento), Times.Once);
            _mockPessoaRepository.Verify(r => r.AddAsync(It.IsAny<Pessoa>()), Times.Once);
            _mockPessoaRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegistrarPessoaAsync_DeveRetornarNulo_QuandoDocumentoJaExiste()
        {
            var requisicao = new CreatePessoaRequest
            {
                Name = "João Silva",
                Document = "12345678900",
                Password = "senha"
            };
            var documentoLimpo = "12345678900";
            var pessoaExistente = new Pessoa { Id = Guid.NewGuid(), Nome = "João Existente", Documento = documentoLimpo };

            _mockPessoaRepository.Setup(r => r.GetByDocumentAsync(documentoLimpo)).ReturnsAsync(pessoaExistente);

            var resultado = await _pessoaService.RegisterPessoaAsync(requisicao);

            Assert.Null(resultado);
            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpo), Times.Once);
            _mockComplianceService.Verify(s => s.ValidaDocumentComplianceAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockPessoaRepository.Verify(r => r.AddAsync(It.IsAny<Pessoa>()), Times.Never);
            _mockPessoaRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("1234567890123")]
        [InlineData("123456789012345")]
        public async Task RegistrarPessoaAsync_DeveLancarArgumentException_QuandoTamanhoDocumentoInvalido(string documentoInvalido)
        {
            var requisicao = new CreatePessoaRequest
            {
                Name = "Teste",
                Document = documentoInvalido,
                Password = "senha"
            };
            var documentoLimpo = documentoInvalido.Replace(".", "").Replace("-", "").Replace("/", "");

            var excecao = await Assert.ThrowsAsync<ArgumentException>(() =>
                _pessoaService.RegisterPessoaAsync(requisicao));

            Assert.Contains("Documento inválido. Deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos).", excecao.Message);
            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpo), Times.Once);
            _mockComplianceService.Verify(s => s.ValidaDocumentComplianceAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockPessoaRepository.Verify(r => r.AddAsync(It.IsAny<Pessoa>()), Times.Never);
            _mockPessoaRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegistrarPessoaAsync_DeveLancarInvalidOperationException_QuandoComplianceResponseENulo()
        {
            var requisicao = new CreatePessoaRequest
            {
                Name = "Teste Compliance",
                Document = "11122233344",
                Password = "senha"
            };
            var documentoLimpo = "11122233344";
            var tipoDocumento = "cpf";

            _mockPessoaRepository.Setup(r => r.GetByDocumentAsync(documentoLimpo)).ReturnsAsync((Pessoa?)null);
            _mockComplianceService.Setup(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento))
                .ReturnsAsync((ComplianceResponse?)null);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _pessoaService.RegisterPessoaAsync(requisicao));

            Assert.Contains("Documento não aprovado pela API de Compliance.", excecao.Message);
            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpo), Times.Once);
            _mockComplianceService.Verify(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento), Times.Once);
            _mockPessoaRepository.Verify(r => r.AddAsync(It.IsAny<Pessoa>()), Times.Never);
            _mockPessoaRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegistrarPessoaAsync_DeveLancarInvalidOperationException_QuandoComplianceNaoSucesso()
        {
            var requisicao = new CreatePessoaRequest
            {
                Name = "Teste Compliance",
                Document = "11122233344",
                Password = "senha"
            };
            var documentoLimpo = "11122233344";
            var tipoDocumento = "cpf";

            _mockPessoaRepository.Setup(r => r.GetByDocumentAsync(documentoLimpo)).ReturnsAsync((Pessoa?)null);

            _mockComplianceService.Setup(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento))
                .ReturnsAsync(new ComplianceResponse { Success = false, Status = 400, Message = "Documento em lista negra" });

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _pessoaService.RegisterPessoaAsync(requisicao));

            var mensagemCompletaEsperada = $"Documento não aprovado pela API de Compliance. Status retornado: 400. Mensagem: Documento em lista negra";
            Assert.Equal(mensagemCompletaEsperada, excecao.Message);

            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpo), Times.Once);
            _mockComplianceService.Verify(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento), Times.Once);
            _mockPessoaRepository.Verify(r => r.AddAsync(It.IsAny<Pessoa>()), Times.Never);
            _mockPessoaRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegistrarPessoaAsync_DeveLancarInvalidOperationException_QuandoComplianceLancaExcecao()
        {
            var requisicao = new CreatePessoaRequest
            {
                Name = "Teste Compliance",
                Document = "11122233344",
                Password = "senha"
            };
            var documentoLimpo = "11122233344";
            var tipoDocumento = "cpf";

            _mockPessoaRepository.Setup(r => r.GetByDocumentAsync(documentoLimpo)).ReturnsAsync((Pessoa?)null);
            _mockComplianceService.Setup(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento))
                .ThrowsAsync(new HttpRequestException("Erro de conexão com compliance API"));

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _pessoaService.RegisterPessoaAsync(requisicao));

            Assert.Contains("Falha na validação do documento na API de Compliance: Erro de conexão com compliance API", excecao.Message);
            Assert.IsType<HttpRequestException>(excecao.InnerException);
            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpo), Times.Once);
            _mockComplianceService.Verify(s => s.ValidaDocumentComplianceAsync(documentoLimpo, tipoDocumento), Times.Once);
            _mockPessoaRepository.Verify(r => r.AddAsync(It.IsAny<Pessoa>()), Times.Never);
            _mockPessoaRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region Testes_ObterPessoaPorIdAsync

        [Fact]
        public async Task ObterPessoaPorIdAsync_DeveRetornarPessoaResponse_QuandoPessoaExiste()
        {
            var pessoaId = Guid.NewGuid();
            var pessoaMock = new Pessoa
            {
                Id = pessoaId,
                Nome = "Alice",
                Documento = "98765432100",
                SenhaHash = "hash"
            };

            _mockPessoaRepository.Setup(r => r.GetByIdAsync(pessoaId)).ReturnsAsync(pessoaMock);

            var resultado = await _pessoaService.GetPessoaByIdAsync(pessoaId);

            Assert.NotNull(resultado);
            Assert.Equal(pessoaId, resultado.Id);
            Assert.Equal(pessoaMock.Nome, resultado.Name);
            Assert.Equal(pessoaMock.Documento, resultado.Document);
            Assert.Equal(pessoaMock.CreatedAt, resultado.CreatedAt);
            Assert.Equal(pessoaMock.UpdatedAt, resultado.UpdatedAt);

            _mockPessoaRepository.Verify(r => r.GetByIdAsync(pessoaId), Times.Once);
        }

        [Fact]
        public async Task ObterPessoaPorIdAsync_DeveRetornarNulo_QuandoPessoaNaoExiste()
        {
            var pessoaId = Guid.NewGuid();
            _mockPessoaRepository.Setup(r => r.GetByIdAsync(pessoaId)).ReturnsAsync((Pessoa?)null);

            var resultado = await _pessoaService.GetPessoaByIdAsync(pessoaId);

            Assert.Null(resultado);
            _mockPessoaRepository.Verify(r => r.GetByIdAsync(pessoaId), Times.Once);
        }

        #endregion

        #region Testes_ObterPessoaPorDocumentoAsync

        [Theory]
        [InlineData("123.456.789-00", "12345678900")]
        [InlineData("12345678900", "12345678900")]
        [InlineData("11.222.333/0001-44", "11222333000144")]
        [InlineData("11222333000144", "11222333000144")]
        public async Task ObterPessoaPorDocumentoAsync_DeveRetornarPessoaResponse_QuandoPessoaExisteEDocumentoELimpo(string documentoEntrada, string documentoLimpoEsperado)
        {
            var pessoaId = Guid.NewGuid();
            var pessoaMock = new Pessoa
            {
                Id = pessoaId,
                Nome = "Bob",
                Documento = documentoLimpoEsperado,
                SenhaHash = "hash"
            };

            _mockPessoaRepository.Setup(r => r.GetByDocumentAsync(documentoLimpoEsperado)).ReturnsAsync(pessoaMock);

            var resultado = await _pessoaService.GetPessoaByDocumentAsync(documentoEntrada);

            Assert.NotNull(resultado);
            Assert.Equal(pessoaId, resultado.Id);
            Assert.Equal(pessoaMock.Nome, resultado.Name);
            Assert.Equal(documentoLimpoEsperado, resultado.Document);

            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpoEsperado), Times.Once);
        }

        [Fact]
        public async Task ObterPessoaPorDocumentoAsync_DeveRetornarNulo_QuandoPessoaNaoExiste()
        {
            var documento = "99988877766";
            var documentoLimpo = "99988877766";

            _mockPessoaRepository.Setup(r => r.GetByDocumentAsync(documentoLimpo)).ReturnsAsync((Pessoa?)null);

            var resultado = await _pessoaService.GetPessoaByDocumentAsync(documento);
            Assert.Null(resultado);
            _mockPessoaRepository.Verify(r => r.GetByDocumentAsync(documentoLimpo), Times.Once);
        }

        #endregion
    }
}