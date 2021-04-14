using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MyTasks_API.Database;
using MyTasks_API.Models;
using MyTasks_API.Repositories;
using MyTasks_API.Repositories.Contracts;
using Newtonsoft.Json;

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
                op.UseSqlite("Data Source=Database//MinhasTarefas.db");
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "MyTasks_API", Version = "v1"});
            });

            services.AddMvc();

            // Configurando a injeção de dependencias dos repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<ITarefaRepository, TarefaRepository>();

            // Configuração do identity para usar como serviço
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<MinhasTarefasContext>();
            
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

            services.AddMvc().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyTasks_API v1"));
            }

            app.UseAuthentication();
            app.UseStatusCodePages();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}