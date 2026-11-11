# IManager

Sistema de RH para gestão de setores, cargos, folha de pagamento e holerites, com persistência de registros de ponto via integração externa com aplicação IMark.

## Tecnologias

- Aplicação híbrida (MVC + API) ASP.NET Core (.NET 10)
- ASP.NET Core Identity + JWT
- Entity Framework Core + Npgsql (PostgreSQL)
- FluentValidation
- Serilog
- MailKit
- AutoMapper
- QuestPDF
- Docker + Docker Compose

## Rodando com Docker

**Pré-requisitos:** Docker e Docker Compose instalados.

1. Clone o repositório

   ```bash
   git clone https://github.com/GabrielSouDev/IManager
   cd IManager
   ```

2. Copie o arquivo `.env.Example` para `.env` e preencha as variáveis (o `__` separa as seções da configuração):

   ```env
   # ASP.NET
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_PORT=5000

   # Postgres
   POSTGRES__NAME=imanager
   POSTGRES__USER=imanager-root
   POSTGRES__PASSWORD=sua_senha
   POSTGRES__HOST=localhost
   POSTGRES__PORT=5432

   # Empresa e usuário Staff criados no primeiro start
   DemoProfiles__Company__TradeName=Staff Company
   DemoProfiles__Company__DocumentNumber=29108745000150
   DemoProfiles__Users__Staff__Email=staff@imanager.com
   DemoProfiles__Users__Staff__Password=Staff@123
   DemoProfiles__Users__Staff__FullName=Demo Staff
   DemoProfiles__Users__Staff__DocumentNumber=64429898014

   # Email (SMTP)
   Email__Host=smtp.provider.com
   Email__Port=587
   Email__Username=services@seu-email.com
   Email__Password=sua_senha_smtp
   Email__FromName=IManager
   Email__FromEmail=noreply@seu-email.com

   # JWT (Key com no mínimo 32 caracteres)
   Jwt__Key=sua-chave-secreta-grande-aqui-minimo-32-chars
   Jwt__Issuer=imanager-api
   Jwt__Audience=imanager-mobile
   Jwt__ExpiresInMinutes=60
   ```

3. Suba os containers:

   ```bash
   docker compose up -d
   ```

A aplicação estará disponível em `http://localhost:5000` (porta de `ASPNETCORE_PORT`).

## Rodando localmente

**Pré-requisitos:** .NET 10 SDK e PostgreSQL instalados.

1. Clone o repositório

   ```bash
   git clone https://github.com/GabrielSouDev/IManager
   cd IManager
   ```

2. Copie o arquivo `IManager.Web/appsettings.Example.json` para `IManager.Web/appsettings.Development.json` e preencha com suas configurações (`Postgres`, `DemoProfiles`, `Email` e `Jwt`).

3. Rode a aplicação (migrations e seed de demonstração rodam automaticamente):

   ```bash
   dotnet run --project IManager.Web
   ```

   A aplicação estará disponível em `http://localhost:5214`.

> 💡 **Dica:** Se preferir usar o Visual Studio com debug, suba apenas o PostgreSQL com o compose da pasta `Infra-Only-Docker`.
> Ele lê um `.env` próprio dentro da pasta, com os mesmos valores do `appsettings.Development.json`:
>
> ```env
> Postgres__Name=your_database_name
> Postgres__User=your_database_user
> Postgres__Password=your_database_password
> Postgres__Port=5432
> ```
>
> ```bash
> cd Infra-Only-Docker
> docker compose up -d
> ```

## Logins de demonstração

No primeiro start (banco sem usuários), o seed cria o usuário Staff configurado acima e 3 empresas fictícias com administrador, funcionários e 2 anos de ponto e folhas (o último mês fica pendente de processamento).

Todos os logins do seed ficam listados em **`/demo-logins`** (também acessível pela tela de login).
