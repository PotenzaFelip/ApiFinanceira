using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPessoaRepository _pessoaRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IPessoaRepository pessoaRepository, IConfiguration configuration)
        {
            _pessoaRepository = pessoaRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var cleanDocument = request.Document.Replace(".", "").Replace("-", "").Replace("/", "");

            var pessoa = await _pessoaRepository.GetByDocumentAsync(cleanDocument);

            if (pessoa == null || !VerifyPassword(request.Password, pessoa.SenhaHash))
            {
                return null;
            }

            var token = GenerateJwtToken(pessoa);

            return new LoginResponse { Token = token };
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return hashedPassword == Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
        }

        private string GenerateJwtToken(Pessoa pessoa)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada."));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, pessoa.Id.ToString()),
                new Claim(ClaimTypes.Name, pessoa.Nome),
                new Claim(ClaimTypes.PrimarySid, pessoa.Documento),

            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiresInHours"] ?? "1")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
