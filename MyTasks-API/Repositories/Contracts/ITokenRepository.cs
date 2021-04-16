using MyTasks_API.Models;

namespace MyTasks_API.Repositories.Contracts
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