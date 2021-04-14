using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyTasks_API.Models;
using MyTasks_API.Repositories.Contracts;

namespace MyTasks_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TarefaController : ControllerBase
    {
        //Dependencias para serem injetadas
        private readonly ITarefaRepository _tarefaRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        //Construtor recebendo a injeção de dependencia 
        public TarefaController(ITarefaRepository tarefaRepository, UserManager<ApplicationUser> userManager)
        {
            _tarefaRepository = tarefaRepository;
            _userManager = userManager;
        }
        
        //Está dizendo que pra ter acesso a essa funçã, o usuário deve estar logado
        [Authorize]
        [HttpPost("sincronizar")]
        public ActionResult Sincronizar([FromBody]List<Tarefa> tarefas)
        {
            return Ok(_tarefaRepository.Sincronizacao(tarefas));
        }
        
        //Está dizendo que pra ter acesso a essa funçã, o usuário deve estar logado
        [Authorize]
        [HttpGet("restaurar")]
        public ActionResult Restaurar(DateTime data)
        {
            // Pegando qual usuário está logado
            var usuario = _userManager.GetUserAsync(HttpContext.User).Result;
            
            return Ok(_tarefaRepository.Restauracao(usuario, data));
        }
    }
}