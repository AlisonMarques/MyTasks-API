using System.Linq;
using MyTasks_API.Database;
using MyTasks_API.Models;
using MyTasks_API.Repositories.Contracts;

namespace MyTasks_API.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        // dependecia padrao
        private readonly MinhasTarefasContext _banco;
        //injeção de dependencia
        public TokenRepository(MinhasTarefasContext banco)
        {
            _banco = banco;
        }
        
        
        public Token Obter(string refreshToken)
        {
            // retornando um token igual que nao foi utilizado
           return _banco.Token.FirstOrDefault(a => a.RefreshToken == refreshToken && a.Utilizado == false);
        }
        
        public void Cadastrar(Token token)
        {
            _banco.Token.Add(token);
            _banco.SaveChanges();
        }

        public void Atualizar(Token token)
        {
            _banco.Token.Update(token);
            _banco.SaveChanges();
        }
    }
}