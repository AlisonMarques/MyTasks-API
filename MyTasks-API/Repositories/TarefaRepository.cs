using System;
using System.Collections.Generic;
using System.Linq;
using MyTasks_API.Database;
using MyTasks_API.Models;
using MyTasks_API.Repositories.Contracts;

namespace MyTasks_API.Repositories
{
    public class TarefaRepository : ITarefaRepository
    {
        // injeção de dependencias
        private readonly MinhasTarefasContext _banco;
        
        //aplicando a injençao de dependencias atraveś do construtor
        public TarefaRepository(MinhasTarefasContext banco)
        {
            _banco = banco;
        }
        
        public List<Tarefa> Restauracao(ApplicationUser usuario, DateTime? dataUltimaSincronizacao)
        {
            var query = _banco.Tarefas.Where(a => a.UsuarioId == usuario.Id).AsQueryable();
            
            //Verificando se a data da ultima atualizacao existe
            if (dataUltimaSincronizacao != null)
            {
                query.Where(a => a.Criado >= dataUltimaSincronizacao || a.Atualizado >= dataUltimaSincronizacao);
            }

            return query.ToList<Tarefa>();
        }
        
        public List<Tarefa> Sincronizacao(List<Tarefa> tarefas)
        {
            #region Cadastrar novos registros

            var tarefasNovas = tarefas.Where(a => a.IdTarefaApi == 0).ToList();
            var tarefasExcluidasAtualizadas = tarefas.Where(a => a.IdTarefaApi != 0).ToList();
            
            if (tarefasNovas.Count() > 0)
            {
                //Enquanto tarefasNovas for maior que 0, o foreach vai percorrer tarefasNovas
                // e adicionar cada nova tarefa no banco de dados
                foreach (var tarefa in tarefasNovas)
                {
                    _banco.Tarefas.Add(tarefa);
                }
            }

            #endregion Cadastrar novos registros
            

            #region Atualização de Registro (Excluído)

            if (tarefasExcluidasAtualizadas.Count() > 0)
            {
                //Enquanto tarefasExcluidasAtualizadas for maior que 0, o foreach vai percorrer tarefasExcluidasAtualizadas
                // e fazer a atualização de cada tarefa no banco de dados
                foreach (var tarefa in tarefasExcluidasAtualizadas)
                {
                    _banco.Tarefas.Update(tarefa);
                }
            }
            #endregion Atualização de registro (Excluído)
            
            _banco.SaveChanges();

            return tarefasNovas.ToList();
        }


    }
}