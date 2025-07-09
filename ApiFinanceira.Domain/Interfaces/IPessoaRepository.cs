using ApiFinanceira.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Interfaces
{
    public interface IPessoaRepository : IGenericRepository<Pessoa>
    {
        Task<Pessoa?> GetByDocumentAsync(string document);

    }
}
