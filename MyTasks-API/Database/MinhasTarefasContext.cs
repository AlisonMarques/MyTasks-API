using System;
using Microsoft.EntityFrameworkCore;
using MyTasks_API.Models;

namespace MyTasks_API.Database
{
    public class MinhasTarefasContext : DbContext
    {
        // Conexao básica
        public MinhasTarefasContext(DbContextOptions<MinhasTarefasContext> options) : base(options)
        {
            
        }

        public DbSet<Tarefa> Tarefas { get; set; }
    }
}