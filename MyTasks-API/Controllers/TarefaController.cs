using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyTasks_API.Models;
using MyTasks_API.Repositories.Contracts;

namespace MyTasks_API.Controllers
{
    [Route("api/[controller")]
    [ApiController]
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
        
        public ActionResult Sincronizar([FromBody]List<Tarefa> tarefas)
        {
            return Ok(_tarefaRepository.Sincronizacao(tarefas));
        }

        public ActionResult Restaurar(DateTime data)
        {
            // Pegando qual usuário está logado
            var usuario = _userManager.GetUserAsync(HttpContext.User).Result;
            
            return Ok(_tarefaRepository.Restauracao(usuario, data));
        }
    }
}