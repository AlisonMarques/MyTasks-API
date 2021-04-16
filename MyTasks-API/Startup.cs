using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyTasks_API.Database;
using MyTasks_API.V1.Models;
using MyTasks_API.V1.Repositories;
using MyTasks_API.V1.Repositories.Contracts;
using MyTasks_API.V1.Helpers.Swagger;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyTasks_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //removendo a validação do modelState que ocorre automáticamente
            services.Configure<ApiBehaviorOptions>(op => { op.SuppressModelStateInvalidFilter = true; });

            // configurando o serviço de banco de dados
            services.AddDbContext<MinhasTarefasContext>(op =>
            {
                op.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });
            
            services.AddControllers();

            // Configurando a injeção de dependencias dos repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<ITarefaRepository, TarefaRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            // Para o controller reconhecer
            services.AddSingleton<IConfiguration>(Configuration);

            // Configuração do identity para usar como serviço
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<MinhasTarefasContext>()
                //habilitando o uso de tokens no identity
                .AddDefaultTokenProviders();
            
            
            // config forma de autenticação para trabalhar com o JWT ao inves dos cookies
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });

            // configurando o Authorization para receber as configurações do Jwt feito acima
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build()
                );
            });

            // Configurando para API retornar ao usuário a mensagem 401 que é quando o usuário não está logado
            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    //enviando a mensagem para o usuário
                    context.Response.StatusCode = 401;
                    // Retornando para api que a tarefa foi completada
                    return Task.CompletedTask;

                };
            });
            
            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddMvc(config =>
            {
                // qnd colocar um formato nao suportado, vai retornar um erro 406
                config.ReturnHttpNotAcceptable = true;
                // adicionando suporte ao xml na entrada e na saida
                config.InputFormatters.Add(new XmlSerializerInputFormatter(config));
                config.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            });

            services.AddMvc().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            
                        
            //controle de versionamento
            services.AddApiVersioning(cfg =>
            {
                // essa config gera o headers indicando quais versões a api suporta
                cfg.ReportApiVersions = true;

                //cfg.ApiVersionReader = new HeaderApiVersionReader("api-version");
                
                cfg.AssumeDefaultVersionWhenUnspecified = true;
                cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1,0);
            });
            services.AddSwaggerGen(cfg =>
            {
                cfg.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Adicione o Json Web Token(JWT) para autenticar",
                    Scheme = "Bearer"
                });

                cfg.AddSecurityRequirement(new OpenApiSecurityRequirement{ 
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Bearer", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }
                });
                //resolvendo o problema de conflitos com as rotas
                cfg.ResolveConflictingActions(apiDescription => apiDescription.First());
               
                // definindo no swagger quantas versões a API possui
                cfg.SwaggerDoc("v1.0", new OpenApiInfo()
                {
                    Title = "MyTasks API - V1.0",
                    Version = "V1.0"
                });
                
                // Configuração para subir os comentários do controller para o Swagger
                var caminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
                var nomeProjeto = $"{PlatformServices.Default.Application.ApplicationName}.xml";
                var caminhoArquivoXmlComentario = Path.Combine(caminhoProjeto, nomeProjeto);
                cfg.IncludeXmlComments(caminhoArquivoXmlComentario);

                cfg.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var versions = methodInfo.DeclaringType
                        .GetCustomAttributes(true)
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v.ToString()}" == docName);

                });
                services.AddMvc(cfg => cfg.Conventions.Add
                    (new ApiExplorerGroupPerVersionConvention()));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "MyTasks_API v1"));
            }
            app.UseAuthentication();
            app.UseStatusCodePages();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            //app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            app.UseMvc();
            app.UseSwagger();

            // Ativando middlewares para o uso do Swagger
            app.UseSwaggerUI(cfg =>
            {
                cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "MyTasks API - V1.0");
                cfg.RoutePrefix = String.Empty;
            });
        }
    }
}