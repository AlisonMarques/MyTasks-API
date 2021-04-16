using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyTasks_API.V1.Models;

namespace MyTasks_API.Database
{
    public class MinhasTarefasContext : IdentityDbContext<ApplicationUser>
    {
        // Conexao básica
        public MinhasTarefasContext(DbContextOptions<MinhasTarefasContext> options) : base(options)
        {
            
        }

        public DbSet<Tarefa> Tarefas { get; set; }
        public DbSet<Token> Token { get; set; }

    }
}