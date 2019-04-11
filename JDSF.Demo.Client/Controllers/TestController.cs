using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenTracingDemo.Models;
using OpenTracingDemo.Service;

namespace OpenTracingDemo.Controllers
{
    public class TestController: Controller
    {
        private IHttpClientFactory _HttpClientFactory;

        private IDBService _DBService;
        public TestController(IHttpClientFactory httpClientFactory,IDBService dBService)
        {
            _HttpClientFactory = httpClientFactory;
            _DBService = dBService;
        }

        [Route("/api/gameInfo")]
        [HttpGet]
        public JsonResult GetGameInfo([FromQuery]string gameId)
        {
            return Json(new { GameName = "test",GameId = gameId });
        }

        [Route("/api/health/check")]
        [HttpGet]
        public JsonResult HealthCheck()
        {
            return Json(new { status = "UP" });
        }
        [Route("/api/refit/gameInfo")]
        [HttpGet]
        public async Task<GameInfo> GetRefitGameInfo([FromQuery]string gameId)
        {
            return await _DBService.GetGameInfo(gameId);
        }

        [Route("/api/db/gameInfo")]
        [HttpGet]
        public async Task<JsonResult>   GetDBGameInfo([FromQuery]string gameId)
        {
            var client = _HttpClientFactory.CreateClient("LoadBalance");
            var resp =  await client.GetAsync($"http://db-service/db/gameinfo/getgameinfo?gameid={gameId}") ;

            if(resp.IsSuccessStatusCode)
            {
                return   Json(resp.Content.ReadAsAsync<GameInfo>().Result); 
            }
            else
            {

                return Json(new {ErrorCode=400,Message = "get request error" });
            } 
        } 
    }
}
