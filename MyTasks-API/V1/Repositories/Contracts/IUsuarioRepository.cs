using MyTasks_API.V1.Models;

namespace MyTasks_API.V1.Repositories.Contracts
{
    public interface IUsuarioRepository
    {
        // métodos do usuário
        void Cadastrar(ApplicationUser usuario, string senha);

        ApplicationUser ObterId(string id);

        ApplicationUser Sigin(string email, string senha);
    }
}