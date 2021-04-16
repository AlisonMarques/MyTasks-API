using System.ComponentModel.DataAnnotations;

namespace MyTasks_API.V1.Models
{
    // Essa classe nao vai pro banco de dados, apenas vai receber as informações
    public class UsuarioDTO
    {
        [Required]
        public string Nome { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Senha { get; set; }
        
        [Required]
        [Compare("Senha")]
        public string ConfirmacaoSenha { get; set; }
    }
}