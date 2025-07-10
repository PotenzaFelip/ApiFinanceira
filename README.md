# ApiFinanceira
Desafios Técnicos de uma ApiFinanceira

---

## 📖 Sumário
* [🚀 Funcionalidades Principais](#-funcionalidades-principais)
* [🛠️ Tecnologias Utilizadas](#️-tecnologias-utilizadas)
* [⚙️ Configuração do Projeto](#️-configuração-do-projeto)
  * [1. Clonar o Repositório](#1-clonar-o-repositório)
  * [2. Configuração do Banco de Dados PostgreSQL](#2-configuração-do-banco-de-dados-postgresql)
* [▶️ Executando o Projeto](#️-executando-o-projeto)
* [🧪 Rodando os Testes](#-rodando-os-testes)

---

## 🚀 Funcionalidades Principais

* **Autenticação JWT:** Sistema de login com geração de JSON Web Tokens para acesso seguro.
* **Gestão de Pessoas:** Criação e consulta de registros de pessoas.
* **Gestão de Contas:** Criação e consulta de contas bancárias associadas a pessoas, com controle de saldo e limite.
* **Gestão de Cartões:** Criação e associação de cartões de crédito a contas.
* **Transações:**
    * Débitos e Créditos.
    * Transferências Internas entre contas.
    * Reversão de Transações (com validação de saldo e prevenção de reversões duplicadas).
* **Consulta de Saldo:** Endpoint para verificar o saldo atual de uma conta.
* **Paginação:** Consulta de histórico de transações com paginação.

---

## 🛠️ Tecnologias Utilizadas

* **Backend:** ASP.NET Core (.NET 8.0)
* **Linguagem:** C#
* **ORM:** Entity Framework Core
* **Banco de Dados:** PostgreSQL
* **Autenticação:** JWT (JSON Web Tokens)
* **Documentação API:** Swagger/OpenAPI
* **Testes:** XUnit, Moq

---

## ⚙️ Configuração do Projeto

Siga os passos abaixo para configurar e rodar o projeto em seu ambiente de desenvolvimento.

### 1. Clonar o Repositório

Abra seu terminal (Git Bash, PowerShell, CMD) e clone o repositório:

```bash
git clone [URL_DO_SEU_REPOSITORIO]
cd [NomeDaPastaDoSeuRepositorio]
```
### 2. Configuração do Banco de Dados PostgreSQL
## a. Crie um Banco de Dados:
Abra seu cliente PostgreSQL (pgAdmin, DBeaver, ou via psql) e crie um novo banco de dados.
Nome Sugerido: ApiFinanceiraDb

## b. Atualize a String de Conexão:
Navegue até o projeto ApiFinanceira.Api e abra o arquivo appsettings.json.
Atualize a string de conexão DefaultConnection com as credenciais do seu banco de dados PostgreSQL.

```JSON

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ApiFinanceiraDb;Username=seu_usuario;Password=sua_senha"
  },
}
```
Importante: Substitua seu_usuario e sua_senha pelas suas credenciais do PostgreSQL.

## c. Aplicar Migrações do Entity Framework Core:
Abra o Package Manager Console no Visual Studio (View > Other Windows > Package Manager Console).

Defina o Default project para ApiFinanceira.Infrastructure.

Adicione a primeira migração (se for a primeira vez ou se a pasta Migrations estiver vazia/corrompida):

```PowerShell

Add-Migration InitialCreate
```
Revise o arquivo de migração gerado na pasta ApiFinanceira.Infrastructure/Data/Migrations/ para garantir que ele esteja criando as tabelas esperadas.

Aplique as migrações ao banco de dados:

```PowerShell

Update-Database
```
Este comando criará todas as tabelas e o histórico de migrações no seu banco de dados.

Se você tiver problemas com migrações (por exemplo, ele tenta recriar o banco):
Isso geralmente acontece se o DbContextModelSnapshot.cs está desatualizado ou você modificou o banco de dados manualmente. Uma solução comum em desenvolvimento é:

Exclua a pasta Migrations inteira do projeto ApiFinanceira.Infrastructure.

Exclua (drop) o seu banco de dados ApiFinanceiraDb no PostgreSQL.

Repita os passos Add-Migration InitialCreate e Update-Database.

### ▶️ Executando o Projeto
Siga estas etapas para iniciar a API no Visual Studio:

1.  **Abrir a Solução no Visual Studio:**
    * Após clonar o repositório, navegue até a pasta `ApiFinanceira/ApiFinanceira` dentro do diretório raiz do projeto.
    * Duplo clique no arquivo de solução `ApiFinanceira.sln` para abri-lo no Visual Studio.

2.  **Definir Projeto de Inicialização:**
    * No **Solution Explorer** (Gerenciador de Soluções) do Visual Studio, localize o projeto `ApiFinanceira.Api`.
    * Clique com o botão direito sobre ele e selecione "Set as Startup Project" (Definir como Projeto de Inicialização).

3.  **Executar a Aplicação:**
    * Pressione `F5` no teclado (ou vá em `Debug` > `Start Debugging` no menu superior do Visual Studio).

O projeto será compilado e iniciado. Uma nova aba no seu navegador padrão deverá abrir automaticamente, direcionando para a interface do Swagger/OpenAPI (geralmente em https://localhost:7161/swagger ou uma porta similar). Esta interface interativa permite que você visualize e teste todos os endpoints da API. Caso a página do Swagger não abra automaticamente, verifique a saída do console no Visual Studio para encontrar a URL exata.

### 🧪 Rodando os Testes
Para garantir a qualidade e o correto funcionamento da aplicação, você pode executar os testes unitários e de integração.

1.  **Usando o Visual Studio**
    * Abra o Gerenciador de Testes:    
    * No Visual Studio, vá em Test > Test Explorer (ou Gerenciador de Testes). Se não estiver visível, pode ser necessário instalá-lo ou ativá-lo nas extensões.

2.  **Construir a Solução:**
    * Certifique-se de que a solução esteja construída para que os testes sejam descobertos. Vá em Build > Build Solution (ou Construir Solução).

3.  **Executar os Testes:**
    * No Test Explorer, você verá uma lista de todos os testes disponíveis.
    * Você pode clicar em "Run All Tests" (Executar Todos os Testes) para rodar todos os testes do projeto.
