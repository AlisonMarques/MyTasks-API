using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTasks_API.V1.Models
{
    public class Tarefa
    {
        [Key]
        //Id da tarefa no banco da Aplicação
        public int IdTarefaApi { get; set; }

        //id da tarefa no banco do Aplicativo mobile ou web
        public int IdTarefaApp { get; set; }
        
        public string Titulo { get; set; }
        public DateTime DataHora { get; set; }
        public string Local { get; set; }
        public string Descricao { get; set; }
        public string Tipo { get; set; }
        public bool Concluido { get; set; }
        
        //Vai ser para exclusão lógica
        public bool Excluido { get; set; }
        
        public DateTime Criado { get; set; }
        public DateTime Atualizado { get; set; }

        #region RELACIONAMENTO
        // O UsuarioID não vai ser int porque o ApplicationUser não trabalha com int
        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; }

        public virtual ApplicationUser Usuario { get; set; }

        #endregion
        
    }
}