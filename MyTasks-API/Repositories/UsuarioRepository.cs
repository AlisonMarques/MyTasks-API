using System;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MyTasks_API.Models;
using MyTasks_API.Repositories.Contracts;

namespace MyTasks_API.Repositories
{

    public class UsuarioRepository : IUsuarioRepository
    {
        // injeção de dependencias
        private readonly UserManager<ApplicationUser> _userManager;

        //aplicando a injençao de dependencias através do construtor
        public UsuarioRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        
        public ApplicationUser Sigin(string email, string senha)
        {
            // buscando um usuário com o mesmo email
            //.RESULT transforma a variável em sincrona
            var usuario = _userManager.FindByEmailAsync(email).Result;
            
            // verificando se o usuário bate com a senha
            if (_userManager.CheckPasswordAsync(usuario, senha).Result)
            {
                return usuario;
            }
            else
            {
                //utilziar domain notification e nao exception
                throw new Exception("E-mail ou senha inválidos!");
            }
        }

        public void Cadastrar(ApplicationUser usuario, string senha)
        {
            //gerando o usuário e o hash da senha
            var result = _userManager.CreateAsync(usuario, senha).Result;

            // verificando se o usuário não foi cadastrado e retornando o erro
            if (!result.Succeeded)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var erro in result.Errors)
                {
                    sb.Append(erro.Description);
                }
                //utilziar domain notification e nao exception
                throw new Exception($"Usuário não cadastrado! {sb.ToString()}");
            }
        }


    }
}