using System;
using System.Collections.Generic;
using MyTasks_API.V1.Models;

namespace MyTasks_API.V1.Repositories.Contracts
{
    public interface ITarefaRepository
    {
        //cadastra tarefa e modifica
        List<Tarefa> Sincronizacao(List<Tarefa> tarefas);
        
        //recupera todas as tarefas
        List<Tarefa> Restauracao(ApplicationUser usuario, DateTime? dataUltimaSincronizacao);
    }
}