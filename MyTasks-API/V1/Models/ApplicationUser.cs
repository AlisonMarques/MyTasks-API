using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MyTasks_API.V1.Models
{
    //essa vai ser a classe que vai para o banco de dados
    public class ApplicationUser : IdentityUser
    {
        //Vamos usar as propridades que existem dentro do IdentityUSer e mais 2 que vamos criar
        
        public string FullName { get; set; }
        
        //Relacionamento
        [ForeignKey("UsuarioId")]
        public virtual ICollection<Tarefa> Tarefas { get; set; }
        
        [ForeignKey("UsuarioId")]
        public virtual ICollection<Token> Tokens { get; set; }
    }
}