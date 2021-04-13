using MyTasks_API.Models;

namespace MyTasks_API.Repositories.Contracts
{
    public interface IUsuarioRepository
    {
        // métodos do usuário
        void Cadastrar(ApplicationUser usuario, string senha);

        ApplicationUser Sigin(string email, string senha);
    }
}