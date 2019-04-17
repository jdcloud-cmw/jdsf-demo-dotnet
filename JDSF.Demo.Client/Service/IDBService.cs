using System;
using System.Threading.Tasks;
using JDSF.Demo.Client.Models;
using Refit;

namespace JDSF.Demo.Client.Service
{
    public interface IDBService
    {
        [Get("/db/gameinfo/getgameinfo")]
        Task<GameInfo> GetGameInfo([AliasAs("gameid")]string gameId);
    }
    
}
