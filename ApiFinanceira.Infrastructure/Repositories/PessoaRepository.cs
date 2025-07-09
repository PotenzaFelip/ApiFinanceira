using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Interfaces;
using ApiFinanceira.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Infrastructure.Repositories
{
    public class PessoaRepository : GenericRepository<Pessoa>, IPessoaRepository
    {
        public PessoaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Pessoa?> GetByDocumentAsync(string document)
        {
            var cleanDocument = document.Replace(".", "").Replace("-", "").Replace("/", "");
            return await _dbSet.FirstOrDefaultAsync(p => p.Documento == cleanDocument);
        }
    }
}
