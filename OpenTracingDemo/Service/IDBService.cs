using System;
using System.Threading.Tasks;
using OpenTracingDemo.Models;
using Refit;

namespace OpenTracingDemo.Service
{
    public interface IDBService
    {
        [Get("/db/gameinfo/getgameinfo")]
        Task<GameInfo> GetGameInfo([AliasAs("gameid")]string gameId);
    }
    
}
