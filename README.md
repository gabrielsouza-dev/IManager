# IManager

Sistema de RH para gestão de setores, cargos, folha de pagamento e holerites, com persistência de registros de ponto via integração externa com aplicação IMark.

## Tecnologias

- Aplicação híbrida (MVC + API) ASP.NET Core (.NET 10)
- ASP.NET Core Identity
- Entity Framework Core + Npgsql (PostgreSQL)
- FluentValidation
- Serilog
- MailKit
- AutoMapper
- Docker + Docker Compose

## Rodando com Docker

**Pré-requisitos:** Docker e Docker Compose instalados.

1. Clone o repositório

   ```bash
   git clone https://github.com/GabrielSouDev/IManager
   cd IManager
   ```

2. Renomeie o arquivo `.env.exemple` para `.env` e preencha as variáveis:

   ```env
   # ASP.NET
   ASPNETCORE_ENVIRONMENT=Production ou Development
   ```

## Postgres

- POSTGRES_NAME=your_database_name

- POSTGRES_USER=your_database_user

- POSTGRES_PASSWORD=your_database_password

- POSTGRES_HOST=localhost

- POSTGRES_PORT=5432

## Admin User

- ADMIN_EMAIL=admin@example.com

- ADMIN_PASSWORD=YourAdminPassword@123

- ADMIN_CONFIRM_PASSWORD=YourAdminPassword@123

## Email (SMTP)

- EMAIL_HOST=smtp.yourprovider.com

- EMAIL_PORT=587

- EMAIL_USERNAME=your_smtp_username

- EMAIL_PASSWORD=your_smtp_password

- EMAIL_FROM_NAME=YourAppName

- EMAIL_FROM_EMAIL=noreply@yourdomain.com

3. Suba os containers:

```bash
docker compose up -d
```

A aplicação estará disponível em `http://localhost:5001`.

## Rodando localmente

**Pré-requisitos:** .NET 10 SDK e PostgreSQL instalados.

1. Clone o repositório

   ```bash
   git clone https://github.com/GabrielSouDev/IManager
   cd IManager
   ```

2. Renomeie o arquivo `appsettings.exemple.json` para `appsettings.Development.json` e preencha com suas configurações:

   ```json
   {
     "Postgres": {
       "Name": "your_database_name",
       "User": "your_database_user",
       "Password": "your_database_password",
       "Host": "localhost",
       "Port": "5432"
     },
     "AdminUser": {
       "Email": "admin@example.com",
       "Password": "YourAdminPassword@123",
       "ConfirmPassword": "YourAdminPassword@123"
     },
     "Email": {
       "Host": "smtp.yourprovider.com",
       "Port": 587,
       "Username": "your_smtp_username",
       "Password": "your_smtp_password",
       "FromName": "YourAppName",
       "FromEmail": "noreply@yourdomain.com"
     }
   }
   ```

3. Rode a aplicação (migrations iniciará automaticamente):

   ```bash
   dotnet run --project IManager.Web
   ```

> 💡 **Dica:** Se preferir usar o Visual Studio com debug, suba apenas o PostgreSQL com o compose da pasta `Infra-Only-Docker`:
>
> ```bash
> cd Infra-Only-Docker
> docker compose up -d
> ```
>
> Depois rode a aplicação normalmente pelo Visual Studio. (importante possuir o appsettings.Development.json configurado)