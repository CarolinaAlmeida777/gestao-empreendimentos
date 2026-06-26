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
- Senha do MySQL :58931561
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


---

## Como executar o backend

```bash
cd backend

# ou navegar por
cd C:\Users\Carolina\Downloads\empreendimentos-app\empreendimentos-app\backend 

# Executar a API
dotnet run --project src/Empreendimentos.Api
```

A API estará disponível em `http://localhost:5000`, com o **Swagger UI na raiz** (`http://localhost:5000/`).


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

### Clean Architecture no backend
O backend foi dividido em 4 projetos com dependências unidirecionais (`Api → Infrastructure → Application → Domain`):

- **Domain**: contém a entidade `Empreendimento` com **todas as invariantes de negócio encapsuladas** (nome mínimo, formato de CNPJ, regra de "todo empreendimento nasce Ativo", bloqueio de edição quando inativo). Não depende de nenhum pacote externo — é C# puro. Isso garante que essas regras não possam ser "puladas" por nenhuma camada superior.
- **Application**: orquestra casos de uso (`EmpreendimentoService`) e define a *interface* `IEmpreendimentoRepository` (porta de saída). A regra de unicidade de CNPJ vive aqui, pois depende de consultar o banco — não é uma invariante que a entidade consegue verificar sozinha.
- **Infrastructure**: implementa o repositório com EF Core + MySQL, incluindo uma constraint **única no banco** para o CNPJ (defesa em profundidade contra condições de corrida, além da verificação feita na Application).
- **Api**: expõe os endpoints REST, configura Swagger, CORS e o middleware global de tratamento de exceções.

### Inativação em vez de exclusão
Conforme exigido, não há endpoint de exclusão. O endpoint `PATCH /api/empreendimentos/{id}/inativar` apenas muda o status — o histórico do registro é preservado.

### Tratamento de erros centralizado
Em vez de `try/catch` repetido em cada controller, um `ExceptionHandlingMiddleware` captura exceções de domínio (`DomainException`, `NotFoundException`, `ConflictException`) e as traduz para `400`, `404` e `409` respectivamente, com um corpo de resposta padronizado (`status`, `titulo`, `mensagem`, `timestamp`). O frontend usa esse formato para popular as mensagens de erro no toast.

### Normalização do CNPJ
O CNPJ é armazenado no banco apenas com dígitos (sem máscara), garantindo que a verificação de unicidade funcione independente de como o usuário formatou o campo. A formatação (`XX.XXX.XXX/XXXX-XX`) é responsabilidade da camada de apresentação (DTO de saída no backend e máscara no input do frontend).

### Validação de CNPJ duplicada (frontend + backend)
O algoritmo de validação do dígito verificador do CNPJ está implementado tanto no frontend (feedback imediato ao digitar) quanto no backend (fonte de verdade, já que o frontend nunca deve ser a única camada de validação).

### Frontend com Standalone Components e Signals
O projeto não usa NgModules — todos os componentes são `standalone`, seguindo o padrão recomendado a partir do Angular 17+. Estado local simples (filtros, loading, toasts) é gerenciado com `signal()` em vez de `BehaviorSubject`, o que simplifica a leitura reativa nos templates (`@if`, `@for`).

### Interceptor HTTP único para loading e erros
Um único interceptor funcional (`apiInterceptor`) cuida de duas responsabilidades transversais a toda chamada HTTP:
1. Ativa/desativa uma barra de carregamento global (suporta chamadas concorrentes via contador).
2. Captura erros HTTP e os transforma em notificações toast, usando a mensagem vinda do backend quando disponível.

### Paginação, filtro e ordenação no backend
Para evitar carregar todos os registros no frontend, a listagem é paginada, filtrada e ordenada inteiramente no backend (via LINQ + EF Core), com os parâmetros (`nome`, `status`, `ordenarPor`, `decrescente`, `pagina`, `tamanhoPagina`) recebidos via query string.

### Debounce na busca por nome
O campo de busca por nome usa debounce de 350ms (RxJS) para evitar disparar uma requisição a cada tecla digitada.

---


