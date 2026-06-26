# Gestão de Empreendimentos

Aplicação para cadastro e gestão de empreendimentos, desenvolvida como case técnico. Permite cadastrar, listar (com filtros, ordenação e paginação), editar e inativar empreendimentos, respeitando as regras de negócio descritas no desafio.

## Tecnologias utilizadas

**Backend**
- C# / .NET 8
- Entity Framework Core 8
- MySQL (via [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql))
- Swagger / OpenAPI (Swashbuckle)
- xUnit + Moq (testes unitários)
- Clean Architecture (Domain, Application, Infrastructure, Api)

**Frontend**
- Angular 18 (standalone components, sem NgModules)
- Formulários reativos (`ReactiveFormsModule`)
- RxJS
- SCSS puro (sem framework de UI)

**Banco de dados**
- MySQL 8

---

## Pré-requisitos

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) e npm
- [Angular CLI 18](https://angular.dev/tools/cli) (`npm install -g @angular/cli`)
- MySQL 8 rodando localmente (ou via Docker — veja abaixo)

### Subindo o MySQL rapidamente (opcional, via Docker)

Se preferir não instalar o MySQL diretamente, suba um container temporário:

```bash
docker run --name mysql-empreendimentos \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=empreendimentos_db \
  -p 3306:3306 \
  -d mysql:8
```

---

## Configuração do banco de dados

1. Garanta que o MySQL está rodando e acessível.
2. O `appsettings.json` versionado no repositório contém apenas um **placeholder** de connection string (sem credenciais reais):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=empreendimentos_db;User=root;Password=root;TreatTinyAsBoolean=true;"
}
```

3. Para rodar localmente **sem expor sua senha real no Git**, configure-a via `dotnet user-secrets` (o projeto já está com `UserSecretsId` configurado no `.csproj`):

```bash
cd backend/src/Empreendimentos.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=empreendimentos_db;User=root;Password=SUA_SENHA_AQUI;TreatTinyAsBoolean=true;"
```

O .NET carrega automaticamente o User Secret em ambiente de desenvolvimento, sobrescrevendo o valor do `appsettings.json` — e esse arquivo de secrets **nunca é versionado** (fica fora da pasta do projeto, em `%APPDATA%\Microsoft\UserSecrets` no Windows ou `~/.microsoft/usersecrets` no Linux/Mac).

Alternativa via variável de ambiente (útil em CI/CD ou Docker):

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=empreendimentos_db;User=root;Password=SUA_SENHA_AQUI;TreatTinyAsBoolean=true;"
```

4. As migrations do EF Core criam a tabela `empreendimentos` automaticamente (passo abaixo).

---

## Como executar o backend

```bash
cd backend

# Restaurar dependências
dotnet restore

# Instalar a ferramenta de migrations do EF Core (se ainda não tiver)
dotnet tool install --global dotnet-ef

# Criar a migration inicial (gera o histórico de schema)
dotnet ef migrations add InitialCreate \
  --project src/Empreendimentos.Infrastructure \
  --startup-project src/Empreendimentos.Api

# Aplicar a migration no banco (cria a tabela empreendimentos)
dotnet ef database update \
  --project src/Empreendimentos.Infrastructure \
  --startup-project src/Empreendimentos.Api

# Executar a API
dotnet run --project src/Empreendimentos.Api
```

A API estará disponível em `http://localhost:5000`, com o **Swagger UI na raiz** (`http://localhost:5000/`).

### Executando os testes unitários

```bash
cd backend
dotnet test
```

Os testes cobrem:
- Regras de negócio da entidade `Empreendimento` (nome mínimo, validação de CNPJ, transições de status, bloqueio de edição quando inativo).
- Orquestração do `EmpreendimentoService` (conflito de CNPJ duplicado, não encontrado, fluxo de inativação), usando mocks do repositório.

---

## Como executar o frontend

```bash
cd frontend

# Instalar dependências
npm install

# Executar em modo desenvolvimento
npm start
# ou: ng serve
```

A aplicação estará disponível em `http://localhost:4200`.

> A URL da API é configurada em `src/environments/environment.ts` (`apiUrl: 'http://localhost:5000/api'`). Ajuste essa constante caso o backend esteja em outra porta/host.

---

## Principais decisões técnicas

### Por que separar o backend em camadas?
Organizei o código em quatro partes (Domain, Application, Infrastructure e Api) para manter cada coisa no seu lugar. As regras do negócio — como "o nome precisa ter pelo menos 3 letras" ou "um empreendimento inativo não pode ser editado" — ficam isoladas numa camada que não depende de banco de dados nem de nenhuma biblioteca externa. Isso garante que essas regras sempre vão ser respeitadas, não importa de onde a informação chegue. As outras camadas só cuidam de "encanamento": falar com o banco, expor a API, etc.

### Por que inativar em vez de excluir?
Como pedido no desafio, nenhum registro é apagado do banco. Ao "remover" um empreendimento pela tela, ele só muda de status para Inativo — o histórico continua existindo, só que marcado como inativo.

### Como os erros são tratados
Em vez de espalhar tratamento de erro por todo canto do código, criei um ponto único que intercepta qualquer problema (CNPJ duplicado, nome inválido, registro não encontrado) e devolve uma mensagem clara e padronizada. É essa mensagem que aparece nos avisos (toasts) que você vê na tela.

### Cuidado com o CNPJ
O CNPJ é guardado no banco só com os números, sem pontos ou barra — assim o sistema reconhece que "11.222.333/0001-81" e "11222333000181" são o mesmo CNPJ, evitando duplicidade por causa de formatação diferente. A máscara visual (com pontos e barra) é aplicada só na hora de mostrar na tela. A validação do CNPJ (aquele cálculo de dígito verificador) acontece tanto no formulário, para avisar o usuário na hora, quanto no backend, que é quem realmente garante que nada inválido entre no sistema.

### Tecnologia mais atual no frontend
O Angular foi feito com os recursos mais recentes (components standalone e signals), o que deixa o código mais simples e direto, sem a burocracia de versões mais antigas do framework.

### Carregamento e erros de forma automática
Toda vez que a tela precisa falar com o backend, um mecanismo único já cuida de mostrar a barra de carregamento e, se algo der errado, exibir o aviso de erro — sem precisar repetir essa lógica em cada tela.

### Busca, filtro e paginação ficam a cargo do backend
Quando você filtra por nome, status ou muda a ordenação, é o backend que faz esse trabalho e devolve só os resultados da página atual. Isso evita carregar a lista inteira de uma vez, o que manteria o sistema rápido mesmo com muitos empreendimentos cadastrados.

### Busca com uma pequena pausa
Ao digitar no campo de busca por nome, o sistema espera você parar de digitar por um instante antes de buscar — assim ele não dispara uma busca a cada letra digitada, economizando chamadas desnecessárias.

---