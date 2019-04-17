using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JDSF.Demo.Server.Controllers
{
    public class DbController : Controller
    {
        [HttpGet]
        [Route("/db/gameinfo/getgameinfo")]
        public JsonResult Getgameinfo([FromQuery (Name="gameId")]string gameTestId)
        {
            List<string> gameList = new List<string>();
            gameList.Add("test-game-1");
            gameList.Add("test-game-2");
            gameList.Add("test-game-3");
            gameList.Add("test-game-4");
            gameList.Add("test-game-5"); 
            int gameIndex = Math.Abs( gameTestId.GetHashCode()) % gameList.Count;
            return Json(new {gameId = gameTestId, gameName = gameList[gameIndex] });
        }
    }
}
