using MyTasks_API.V1.Models;

namespace MyTasks_API.V1.Repositories.Contracts
{
    public interface ITokenRepository
    {
        // banco recomendado para salvar cache, token e coisas similares é o (Key-value)
        //Alguns exemplos são: Oracle NoSQL, Riak, Azure Table Storage, BerkeleyDB e Redis.

        // C - R - U 
        void Cadastrar(Token token);

        Token Obter(string refreshToken);

        void Atualizar(Token token);
    }
}