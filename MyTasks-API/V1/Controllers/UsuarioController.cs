using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyTasks_API.V1.Models;
using MyTasks_API.V1.Repositories.Contracts;

namespace MyTasks_API.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsuarioController : ControllerBase
    {
        //Dependencias para serem injetadas
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        //Construtor recebendo a injeção de dependencia de IUsuarioRepository e SignInManager
        public UsuarioController(IUsuarioRepository usuarioRepository, SignInManager<ApplicationUser> signInManager,
            ITokenRepository tokenRepository, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _tokenRepository = tokenRepository;
        }

        [HttpPost("login")]
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

                //  _signInManager.SignInAsync(usuario, false); faz o login retornando cookies

                //utilizaremos o buildtoken para nao tem cookies e ter apenas o token jwt
                // Tornando a api em stateless
                return GerarToken(usuario);
            }

            return NotFound("Usuário não encontrado!");
        }
        
        [HttpPost("renovar")]
        public ActionResult Renovar([FromBody] TokenDto tokenDto)
        {
            var refreshTokenDB = _tokenRepository.Obter(tokenDto.RefreshToken);
            // se nao for encontrado no banco um token com o mesmo código, é pq ele foi deletado ou nunca existiu
            if (refreshTokenDB == null)
                return NotFound();
            
            //RefreshToken antigo - Atualizar = Desativar esse refreshtoken
            
            refreshTokenDB.Atualizado = DateTime.Now;
            refreshTokenDB.Utilizado = true;
            _tokenRepository.Atualizar(refreshTokenDB);
            
            //Gerar um novo Token com refresh token e salvar
            
            var usuario = _usuarioRepository.ObterId(refreshTokenDB.UsuarioId);

            return GerarToken(usuario);
        }
        

        [HttpPost("")]
        public ActionResult Cadastrar([FromBody] UsuarioDTO usuarioDTO)
        {
            // se o usuário  nao for válido
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            ApplicationUser usuario = new ApplicationUser();

            usuario.FullName = usuarioDTO.Nome;
            usuario.UserName = usuarioDTO.Email;
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

        //gerando token jwt
        private TokenDto BuildToken(ApplicationUser usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)
            };

            //recomendado colocar a key dentro de appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // Criando a assinatura
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //expiração do token
            var exp = DateTime.UtcNow.AddHours(1);

            // recebendo o token gerado com suas informações
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: exp,
                signingCredentials: sign
            );

            //gerando uma string com toda a criptografia passada pelo jwtSecuritytoken
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            // GERANDO O TOKEN REFRESH
            var refreshToken = Guid.NewGuid().ToString();
            
            var expRefreshToken = DateTime.UtcNow.AddHours(2);
            
            var tokenDTO = new TokenDto {Token = tokenString, Expiration = exp,
                RefreshToken = refreshToken, ExpirationRefreshToken = expRefreshToken
                
            };

            return tokenDTO;
        }
        private ActionResult GerarToken(ApplicationUser usuario)
        {
            var token = BuildToken(usuario);

            //Salvar o Token no banco
            var tokenModel = new Token()
            {
                RefreshToken = token.RefreshToken,
                ExpirationToken = token.Expiration,
                ExpirationRefreshToken = token.ExpirationRefreshToken,
                Usuario = usuario,
                Criado = DateTime.Now,
                Utilizado = false
            };
            _tokenRepository.Cadastrar(tokenModel);

            return Ok(token);
        }

    }
}