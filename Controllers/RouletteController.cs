using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Masivian.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masivian.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouletteController : ControllerBase
    {
        private readonly ILogger<RouletteController> _logger;
        private readonly Business business;
        public const string USERID = "UserID";
        public const string ERROR = "error";
        public RouletteController(ILogger<RouletteController> logger, IAmazonDynamoDB dynamoDBClient)
        {
            _logger = logger;
            business = new Business(dynamoDBClient: dynamoDBClient, logger: logger);
        }

        [HttpPost("create")]
        async public Task<string> Create()
        {
            _logger.LogDebug("Creating roulette");
            return await business.CreateRoulette();
        }

        [HttpPost("open")]
        async public Task<string> OpenRoulette(string rouletteId)
        {
            _logger.LogDebug($"Opening roulette: {rouletteId}");
            return await business.OpenRoulette(rouletteId: rouletteId);
        }

        [HttpPost("tobet")]
        async public Task<string> ToBet([FromBody]Player player)
        {            
            var isUserAdded = AddUserId(player);
            _logger.LogDebug($"To bet Player: {player.UserId}");
            if (isUserAdded)
            {
                return await business.ToBetRoulette(player: player);
            }

            return ERROR;
        }

        [HttpPost("close")]
        async public Task<IEnumerable<Player>> Close(string rouletteId)
        {
            _logger.LogDebug($"Closing roulette: {rouletteId}");
            return await business.CloseRoulette(rouletteId: rouletteId);
        }

        [HttpGet("list")]
        async public Task<IEnumerable<Roulette>> List()
        {
            _logger.LogDebug($"Listing roulettes");
            return await business.ListRoulette();
        }
        private bool AddUserId(Player player)
        {            
            var hasUserId = Request.Headers.ContainsKey(USERID);
            if (!hasUserId)
            {
                return false;
            }
            var userId = Request.Headers[USERID];
            player.UserId = userId;

            return true;
        }
    }
}
