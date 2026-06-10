using Microsoft.EntityFrameworkCore;

// Inicializa o construtor da aplicação web (configura rotas, variáveis e serviços)
var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAÇÃO DO BANCO DE DADOS (MySQL) ---
// Busca a string de conexão configurada no arquivo 'appsettings.json'
var connectionString = builder.Configuration.GetConnectionString("SalesWebMvcContext")
    ?? throw new InvalidOperationException("Connection string 'SalesWebMvcContext' not found.");

// Adiciona o contexto do Entity Framework (SalesWebMvcContext) ao sistema do app
builder.Services.AddDbContext<SalesWebMvcContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString), // Detecta automaticamente a versão do MySQL instalada
        b => b.MigrationsAssembly("SalesWebMvc")  // Define onde as migrações do banco serão salvas
    )
);

// --- REGISTRO DE SERVIÇOS NO CONTAINER DE DEPENDÊNCIAS ---
// Adiciona o suporte para o padrão MVC (Controllers e Views) e ativa a compilação em tempo real do Razor (HTML)
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// REGISTRO DO SEEDINGSERVICE (Injeção de Dependência)
// Informa ao app que o SeedingService existe e pode ser usado para injetar dados no banco
builder.Services.AddScoped<SalesWebMvc.Data.SeedingService>();


builder.Services.AddScoped<SalesWebMvc.Services.SellerService>();


// Compila todas as configurações acima e constrói oficialmente a aplicação (app)
var app = builder.Build();


// --- CONFIGURAÇÃO DO FLUXO DE REQUISIÇÕES (HTTP PIPELINE) ---

// Verifica se o aplicativo NÃO está rodando em ambiente de desenvolvimento (ou seja, está em Produção)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Redireciona o usuário para uma página amigável de erro se algo quebrar
    app.UseHsts();                          // Ativa segurança estrita de HTTPS (HTTP Strict Transport Security)
}
else
{
    // --- EXECUÇÃO DO SEEDING (Apenas em ambiente de testes/desenvolvimento) ---
    // Cria um escopo temporário na memória para conseguir acessar os serviços registrados
    using (var scope = app.Services.CreateScope())
    {
        // Pede ao sistema para buscar a instância criada do SeedingService
        var seedingService = scope.ServiceProvider.GetRequiredService<SalesWebMvc.Data.SeedingService>();

        // Executa o método responsável por popular as tabelas do banco com dados iniciais
        seedingService.Seed();
       

    }
}

// Força a aplicação a redirecionar qualquer acesso HTTP comum para HTTPS (seguro)
app.UseHttpsRedirection();

// Ativa o suporte a arquivos estáticos (permite que o app leia arquivos como imagens, CSS e JavaScript da pasta wwwroot)
app.UseStaticFiles();

// Ativa o sistema de rotas do .NET para identificar qual URL aponta para qual página
app.UseRouting();

// Ativa o sistema de autorização (regras de quem pode ou não acessar certas páginas)
app.UseAuthorization();

// Define a rota padrão do site (Se nenhuma página for digitada, abre a Home/Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicia oficialmente o servidor e coloca o site no ar
app.Run();
