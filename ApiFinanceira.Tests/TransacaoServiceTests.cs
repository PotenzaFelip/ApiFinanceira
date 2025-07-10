using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.Services;
using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Interfaces;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFinanceira.Tests.ApplicationTests
{
    public class TransacaoServiceTests
    {
        private readonly Mock<ITransacaoRepository> _mockRepositorioTransacao;
        private readonly Mock<IContaRepository> _mockRepositorioConta;
        private readonly TransacaoService _servicoTransacao;

        private readonly Guid _id_pessoa = Guid.NewGuid();
        private readonly Guid _id_conta = Guid.NewGuid();
        private readonly Guid _id_conta_recebedora = Guid.NewGuid();
        private readonly Guid _id_outra_pessoa = Guid.NewGuid();

        public TransacaoServiceTests()
        {
            _mockRepositorioTransacao = new Mock<ITransacaoRepository>();
            _mockRepositorioConta = new Mock<IContaRepository>();
            _servicoTransacao = new TransacaoService(
                _mockRepositorioTransacao.Object,
                _mockRepositorioConta.Object
            );
        }

        [Fact]
        public async Task CriarTransacaoAsync_DeveCriarTransacaoDeCreditoComSucesso()
        {
            var saldoInicial = 100m;
            var valorCredito = 50m;
            var requisicao = new CreateTransacaoRequest { Value = valorCredito, Description = "Recebimento" };
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = saldoInicial, Account = "12345-6" };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.AddAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);
            _mockRepositorioConta.Setup(r => r.UpdateAsync(It.IsAny<Conta>())).Returns(Task.CompletedTask);
            _mockRepositorioTransacao.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var resultado = await _servicoTransacao.CreateTransactionAsync(_id_pessoa, _id_conta, requisicao);

            Assert.NotNull(resultado);
            Assert.Equal(valorCredito, resultado.Value);
            Assert.Equal("credit", resultado.Type);
            Assert.Equal(saldoInicial + valorCredito, conta.Saldo);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.Is<Transacao>(t =>
                t.ContaId == _id_conta && t.Value == valorCredito && t.Description == "Recebimento" && t.Type == "credit")), Times.Once);
            _mockRepositorioConta.Verify(r => r.UpdateAsync(conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarTransacaoAsync_DeveCriarTransacaoDeDebitoComSucesso()
        {
            var saldoInicial = 100m;
            var valorDebito = -50m;
            var requisicao = new CreateTransacaoRequest { Value = valorDebito, Description = "Pagamento" };
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = saldoInicial, Account = "12345-6" };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.AddAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);
            _mockRepositorioConta.Setup(r => r.UpdateAsync(It.IsAny<Conta>())).Returns(Task.CompletedTask);
            _mockRepositorioTransacao.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var resultado = await _servicoTransacao.CreateTransactionAsync(_id_pessoa, _id_conta, requisicao);

            Assert.NotNull(resultado);
            Assert.Equal(valorDebito, resultado.Value);
            Assert.Equal("debit", resultado.Type);
            Assert.Equal(saldoInicial + valorDebito, conta.Saldo);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.Is<Transacao>(t =>
                t.ContaId == _id_conta && t.Value == valorDebito && t.Description == "Pagamento" && t.Type == "debit")), Times.Once);
            _mockRepositorioConta.Verify(r => r.UpdateAsync(conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarTransacaoAsync_DeveLancarUnauthorizedAccessException_QuandoContaNaoEncontrada()
        {
            var requisicao = new CreateTransacaoRequest { Value = 50m, Description = "Recebimento" };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync((Conta?)null);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _servicoTransacao.CreateTransactionAsync(_id_pessoa, _id_conta, requisicao));

            Assert.Equal("Conta não encontrada ou você não tem permissão para acessá-la.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
            _mockRepositorioConta.Verify(r => r.UpdateAsync(It.IsAny<Conta>()), Times.Never);
            _mockRepositorioTransacao.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarTransacaoAsync_DeveLancarUnauthorizedAccessException_QuandoContaPertenceAOutraPessoa()
        {
            var requisicao = new CreateTransacaoRequest { Value = 50m, Description = "Recebimento" };
            var conta = new Conta { Id = _id_conta, PessoaId = _id_outra_pessoa, Saldo = 100m };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _servicoTransacao.CreateTransactionAsync(_id_pessoa, _id_conta, requisicao));

            Assert.Equal("Conta não encontrada ou você não tem permissão para acessá-la.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
            _mockRepositorioConta.Verify(r => r.UpdateAsync(It.IsAny<Conta>()), Times.Never);
            _mockRepositorioTransacao.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarTransacaoAsync_DeveLancarInvalidOperationException_QuandoDebitoCausaSaldoInsuficiente()
        {
            var saldoInicial = 50m;
            var valorDebito = -100m;
            var requisicao = new CreateTransacaoRequest { Value = valorDebito, Description = "Pagamento grande" };
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = saldoInicial };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicoTransacao.CreateTransactionAsync(_id_pessoa, _id_conta, requisicao));

            Assert.Equal("Saldo insuficiente para realizar esta transação de débito.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
            _mockRepositorioConta.Verify(r => r.UpdateAsync(It.IsAny<Conta>()), Times.Never);
            _mockRepositorioTransacao.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarTransferenciaInternaAsync_DeveTransferirComSucesso()
        {
            var saldoInicialRemetente = 200m;
            var saldoInicialRecebedor = 50m;
            var valorTransferencia = 100m;
            var requisicao = new CreateTransferenciaRequest { ReceiverAccountId = _id_conta_recebedora, Value = valorTransferencia, Description = "Transferência interna" };

            var contaRemetente = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = saldoInicialRemetente, Account = "SenderAcc" };
            var contaRecebedora = new Conta { Id = _id_conta_recebedora, PessoaId = Guid.NewGuid(), Saldo = saldoInicialRecebedor, Account = "ReceiverAcc" };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(contaRemetente);
            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta_recebedora)).ReturnsAsync(contaRecebedora);
            _mockRepositorioTransacao.Setup(r => r.AddAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);
            _mockRepositorioConta.Setup(r => r.UpdateAsync(It.IsAny<Conta>())).Returns(Task.CompletedTask);
            _mockRepositorioTransacao.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var resultado = await _servicoTransacao.CreateInternalTransferAsync(_id_pessoa, _id_conta, requisicao);

            Assert.NotNull(resultado);
            Assert.Equal(-valorTransferencia, resultado.Value);
            Assert.Equal(saldoInicialRemetente - valorTransferencia, contaRemetente.Saldo);
            Assert.Equal(saldoInicialRecebedor + valorTransferencia, contaRecebedora.Saldo);

            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta_recebedora), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.Is<Transacao>(t =>
                t.ContaId == _id_conta && t.Value == -valorTransferencia && t.Type == "debit")), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.Is<Transacao>(t =>
                t.ContaId == _id_conta_recebedora && t.Value == valorTransferencia && t.Type == "credit")), Times.Once);
            _mockRepositorioConta.Verify(r => r.UpdateAsync(contaRemetente), Times.Once);
            _mockRepositorioConta.Verify(r => r.UpdateAsync(contaRecebedora), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarTransferenciaInternaAsync_DeveLancarUnauthorizedAccessException_QuandoContaRemetenteNaoEncontrada()
        {
            var requisicao = new CreateTransferenciaRequest { ReceiverAccountId = _id_conta_recebedora, Value = 100m, Description = "Transferência" };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync((Conta?)null);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _servicoTransacao.CreateInternalTransferAsync(_id_pessoa, _id_conta, requisicao));

            Assert.Equal("Conta de origem não encontrada ou você não tem permissão para acessá-la.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta_recebedora), Times.Never);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task CriarTransferenciaInternaAsync_DeveLancarUnauthorizedAccessException_QuandoContaRemetentePertenceAOutraPessoa()
        {
            var requisicao = new CreateTransferenciaRequest { ReceiverAccountId = _id_conta_recebedora, Value = 100m, Description = "Transferência" };
            var contaRemetente = new Conta { Id = _id_conta, PessoaId = _id_outra_pessoa, Saldo = 200m };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(contaRemetente);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _servicoTransacao.CreateInternalTransferAsync(_id_pessoa, _id_conta, requisicao));

            Assert.Equal("Conta de origem não encontrada ou você não tem permissão para acessá-la.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta_recebedora), Times.Never);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task CriarTransferenciaInternaAsync_DeveLancarInvalidOperationException_QuandoContaRecebedoraNaoEncontrada()
        {
            var saldoInicialRemetente = 200m;
            var requisicao = new CreateTransferenciaRequest { ReceiverAccountId = _id_conta_recebedora, Value = 100m, Description = "Transferência" };

            var contaRemetente = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = saldoInicialRemetente };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(contaRemetente);
            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta_recebedora)).ReturnsAsync((Conta?)null);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicoTransacao.CreateInternalTransferAsync(_id_pessoa, _id_conta, requisicao));

            Assert.Equal("Conta de destino não encontrada.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta_recebedora), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task CriarTransferenciaInternaAsync_DeveLancarInvalidOperationException_QuandoTransferirParaMesmaConta()
        {
            var saldoInicialRemetente = 200m;
            var requisicao = new CreateTransferenciaRequest { ReceiverAccountId = _id_conta, Value = 100m, Description = "Transferência" };

            var contaRemetente = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = saldoInicialRemetente };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(contaRemetente);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicoTransacao.CreateInternalTransferAsync(_id_pessoa, _id_conta, requisicao));

            Assert.Equal("Não é permitido transferir para a mesma conta.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Exactly(2));
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task CriarTransferenciaInternaAsync_DeveLancarInvalidOperationException_QuandoSaldoInsuficiente()
        {
            var saldoInicialRemetente = 50m;
            var valorTransferencia = 100m;
            var requisicao = new CreateTransferenciaRequest { ReceiverAccountId = _id_conta_recebedora, Value = valorTransferencia, Description = "Transferência" };

            var contaRemetente = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = saldoInicialRemetente };
            var contaRecebedora = new Conta { Id = _id_conta_recebedora, PessoaId = Guid.NewGuid(), Saldo = 0m };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(contaRemetente);
            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta_recebedora)).ReturnsAsync(contaRecebedora);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicoTransacao.CreateInternalTransferAsync(_id_pessoa, _id_conta, requisicao));

            Assert.Equal("Saldo insuficiente na conta de origem para realizar a transferência.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta_recebedora), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task ObterTransacoesPorContaIdAsync_DeveRetornarTransacoesComSucesso()
        {
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = 100m };
            var transacoes = new List<Transacao>
            {
                new Transacao { Id = Guid.NewGuid(), ContaId = _id_conta, Value = 10, Description = "Trans1", Type = "credit" },
                new Transacao { Id = Guid.NewGuid(), ContaId = _id_conta, Value = -5, Description = "Trans2", Type = "debit" }
            };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.CountTransactionsByContaIdAsync(_id_conta, null)).ReturnsAsync(2);
            _mockRepositorioTransacao.Setup(r => r.GetPagedTransactionsByContaIdAsync(_id_conta, 0, 10, null)).ReturnsAsync(transacoes);

            var resultado = await _servicoTransacao.GetTransactionsByContaIdAsync(_id_pessoa, _id_conta);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Transactions.Count());
            Assert.Equal(2, resultado.Pagination.TotalItems);
            Assert.Equal(1, resultado.Pagination.TotalPages);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.CountTransactionsByContaIdAsync(_id_conta, null), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetPagedTransactionsByContaIdAsync(_id_conta, 0, 10, null), Times.Once);
        }

        [Fact]
        public async Task ObterTransacoesPorContaIdAsync_DeveRetornarTransacoesComFiltroEPaginacao()
        {
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = 100m };
            var transacoesCredito = new List<Transacao>
            {
                new Transacao { Id = Guid.NewGuid(), ContaId = _id_conta, Value = 10, Description = "Trans1", Type = "credit" }
            };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.CountTransactionsByContaIdAsync(_id_conta, "credit")).ReturnsAsync(1);
            _mockRepositorioTransacao.Setup(r => r.GetPagedTransactionsByContaIdAsync(_id_conta, 0, 5, "credit")).ReturnsAsync(transacoesCredito);

            var resultado = await _servicoTransacao.GetTransactionsByContaIdAsync(_id_pessoa, _id_conta, 5, 1, "credit");

            Assert.NotNull(resultado);
            Assert.Single(resultado.Transactions);
            Assert.Equal("credit", resultado.Transactions.First().Type);
            Assert.Equal(1, resultado.Pagination.TotalItems);
            Assert.Equal(1, resultado.Pagination.TotalPages);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.CountTransactionsByContaIdAsync(_id_conta, "credit"), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetPagedTransactionsByContaIdAsync(_id_conta, 0, 5, "credit"), Times.Once);
        }

        [Fact]
        public async Task ObterTransacoesPorContaIdAsync_DeveLidarComParametrosDePaginacaoInvalidos()
        {
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = 100m };
            var transacoes = new List<Transacao>
            {
                new Transacao { Id = Guid.NewGuid(), ContaId = _id_conta, Value = 10, Description = "Trans1", Type = "credit" }
            };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.CountTransactionsByContaIdAsync(_id_conta, null)).ReturnsAsync(1);
            _mockRepositorioTransacao.Setup(r => r.GetPagedTransactionsByContaIdAsync(_id_conta, 0, 10, null)).ReturnsAsync(transacoes);

            var resultado = await _servicoTransacao.GetTransactionsByContaIdAsync(_id_pessoa, _id_conta, -5, 0);

            Assert.NotNull(resultado);
            Assert.Single(resultado.Transactions);
            Assert.Equal(10, resultado.Pagination.ItemsPerPage);
            Assert.Equal(1, resultado.Pagination.CurrentPage);
            _mockRepositorioTransacao.Verify(r => r.GetPagedTransactionsByContaIdAsync(_id_conta, 0, 10, null), Times.Once);
        }


        [Fact]
        public async Task ObterTransacoesPorContaIdAsync_DeveLancarUnauthorizedAccessException_QuandoContaNaoEncontrada()
        {
            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync((Conta?)null);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _servicoTransacao.GetTransactionsByContaIdAsync(_id_pessoa, _id_conta));

            Assert.Equal("Conta não encontrada ou você não tem permissão para acessá-la.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.CountTransactionsByContaIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
            _mockRepositorioTransacao.Verify(r => r.GetPagedTransactionsByContaIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task ObterTransacoesPorContaIdAsync_DeveLancarUnauthorizedAccessException_QuandoContaPertenceAOutraPessoa()
        {
            var conta = new Conta { Id = _id_conta, PessoaId = _id_outra_pessoa, Saldo = 100m };
            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _servicoTransacao.GetTransactionsByContaIdAsync(_id_pessoa, _id_conta));

            Assert.Equal("Conta não encontrada ou você não tem permissão para acessá-la.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.CountTransactionsByContaIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
            _mockRepositorioTransacao.Verify(r => r.GetPagedTransactionsByContaIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task ReverterTransacaoAsync_DeveLancarUnauthorizedAccessException_QuandoContaNaoEncontrada()
        {
            var idTransacao = Guid.NewGuid();
            var descricao = "Reversão";
            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync((Conta?)null);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _servicoTransacao.RevertTransactionAsync(_id_pessoa, _id_conta, idTransacao, descricao));

            Assert.Equal("Conta não encontrada ou você não tem permissão para acessá-la.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ReverterTransacaoAsync_DeveLancarUnauthorizedAccessException_QuandoContaPertenceAOutraPessoa()
        {
            var idTransacao = Guid.NewGuid();
            var descricao = "Reversão";
            var conta = new Conta { Id = _id_conta, PessoaId = _id_outra_pessoa, Saldo = 100m };
            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);

            var excecao = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _servicoTransacao.RevertTransactionAsync(_id_pessoa, _id_conta, idTransacao, descricao));

            Assert.Equal("Conta não encontrada ou você não tem permissão para acessá-la.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ReverterTransacaoAsync_DeveLancarInvalidOperationException_QuandoTransacaoNaoEncontrada()
        {
            var idTransacao = Guid.NewGuid();
            var descricao = "Reversão";
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = 100m };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.GetByIdAsync(idTransacao)).ReturnsAsync((Transacao?)null);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicoTransacao.RevertTransactionAsync(_id_pessoa, _id_conta, idTransacao, descricao));

            Assert.Equal("Transação não encontrada para a conta especificada.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetByIdAsync(idTransacao), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task ReverterTransacaoAsync_DeveLancarInvalidOperationException_QuandoTransacaoPertenceAOutraConta()
        {
            var idTransacao = Guid.NewGuid();
            var descricao = "Reversão";
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = 100m };
            var transacaoOriginal = new Transacao { Id = idTransacao, ContaId = Guid.NewGuid(), Value = 50m, Type = "credit", IsReverted = false };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.GetByIdAsync(idTransacao)).ReturnsAsync(transacaoOriginal);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicoTransacao.RevertTransactionAsync(_id_pessoa, _id_conta, idTransacao, descricao));

            Assert.Equal("Transação não encontrada para a conta especificada.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetByIdAsync(idTransacao), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task ReverterTransacaoAsync_DeveLancarInvalidOperationException_QuandoTransacaoJaRevertida()
        {
            var idTransacao = Guid.NewGuid();
            var descricao = "Reversão";
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = 100m };
            var transacaoOriginal = new Transacao { Id = idTransacao, ContaId = _id_conta, Value = 50m, Type = "credit", IsReverted = true };

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.GetByIdAsync(idTransacao)).ReturnsAsync(transacaoOriginal);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicoTransacao.RevertTransactionAsync(_id_pessoa, _id_conta, idTransacao, descricao));

            Assert.Equal("Esta transação já foi revertida.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetByIdAsync(idTransacao), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task ReverterTransacaoAsync_DeveLancarInvalidOperationException_QuandoNaoPodeExtrairIdDaContaParceira()
        {
            var idTransacao = Guid.NewGuid();
            var saldoInicial = 200m;
            var transacaoOriginal = new Transacao
            {
                Id = idTransacao,
                ContaId = _id_conta,
                Value = -100m,
                Description = "Transferência com descrição inválida",
                Type = "debit",
                IsReverted = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            };
            var conta = new Conta { Id = _id_conta, PessoaId = _id_pessoa, Saldo = saldoInicial };
            var descricao = "Reversão de transferência";

            _mockRepositorioConta.Setup(r => r.GetByIdAsync(_id_conta)).ReturnsAsync(conta);
            _mockRepositorioTransacao.Setup(r => r.GetByIdAsync(idTransacao)).ReturnsAsync(transacaoOriginal);

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicoTransacao.RevertTransactionAsync(_id_pessoa, _id_conta, idTransacao, descricao));

            Assert.Equal("Não foi possível extrair o ID da conta parceira da descrição da transação original para reversão. A reversão manual de ambas as partes pode ser necessária.", excecao.Message);
            _mockRepositorioConta.Verify(r => r.GetByIdAsync(_id_conta), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetByIdAsync(idTransacao), Times.Once);
            _mockRepositorioTransacao.Verify(r => r.GetQueryable(), Times.Never);
            _mockRepositorioTransacao.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }        
    }
}