using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyTasks_API.Models;
using MyTasks_API.Repositories.Contracts;

namespace MyTasks_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        //Dependencias para serem injetadas
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUsuarioRepository _usuarioRepository;

        //Construtor recebendo a injeção de dependencia de IUsuarioRepository e SignInManager
        public UsuarioController(IUsuarioRepository usuarioRepository, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _usuarioRepository = usuarioRepository;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public ActionResult Login([FromBody] UsuarioDTO usuarioDTO)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("ConfirmacaoSenha");

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            // Pegando o email e a senha do usuário para verificar
            ApplicationUser usuario = _usuarioRepository.Sigin(usuarioDTO.Email, usuarioDTO.Senha);
            if (usuario != null)
            {
                // Fazendo o Login
                _signInManager.SignInAsync(usuario, false);
                return Ok();
            }

            return NotFound("Usuário não encontrado!");
        }

        public ActionResult Cadastrar([FromBody] UsuarioDTO usuarioDTO)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            ApplicationUser usuario = new ApplicationUser();
            
            usuario.FullName = usuarioDTO.Nome;
            usuario.Email = usuarioDTO.Email;
            // recebendo os dados para gerar o cadastro e criando o usuário.
            var resultado = _userManager.CreateAsync(usuario, usuarioDTO.Senha).Result;

            // Se o resultado/cadastro deu errado:
            if (!resultado.Succeeded)
            {
                List<string> erros = new List<string>();
                foreach (var erro in resultado.Errors)
                {
                    erros.Add(erro.Description);
                }

                return UnprocessableEntity(erros);
            }

            return Ok(usuario);
        }
    }
}