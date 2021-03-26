using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Masivian.Controllers;
using Masivian.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masivian
{
    public class Business
    {
        private readonly ILogger<RouletteController> _logger;
        private readonly DynamoUtils dynamouUtils;
        private const string OK = "OK";
        private const string ERROR = "ERROR";
        public Business(ILogger<RouletteController> logger, IAmazonDynamoDB dynamoDBClient)
        {
            _logger = logger;
            dynamouUtils = new DynamoUtils(dynamoDBClient);
        }
        async public Task<string> CreateRoulette()
        {
            return await dynamouUtils.CreateRoulette();
        }
        async public Task<string> OpenRoulette(string rouletteId)
        {
            var roulette = await GetRoulette(rouletteId: rouletteId);
            if (roulette.ExistAndIsClosed())
            {
                return ERROR;
            }

            return OK;
        }
        async public Task<string> ToBetRoulette(Player player)
        {
            var roulette = await GetRoulette(rouletteId: player.RouletteId);
            if (roulette.DontExistAndIsClosed())
            {
                return ERROR;
            }
            var isGameSaved = await dynamouUtils.SaveBetGame(player: player);
            if (isGameSaved)
            {
                return OK;
            }

            return ERROR;
        }
        async public Task<IEnumerable<Player>> CloseRoulette(string rouletteId)
        {
            var roulette = await GetRoulette(rouletteId: rouletteId);
            if (!roulette.Exists())
            {
                return null;
            }
            if (roulette.IsClosed())
            {
                return await GetPlayersByRouletteId(rouletteId: rouletteId);
            }
            await dynamouUtils.CloseRoulette(roulette);
            var playerList = (await GetPlayersByRouletteId(rouletteId: rouletteId)).ToList();
            SelectWinner(players: playerList);
            SetPrice(players: playerList);
            SaveGame(players: playerList);

            return playerList;
        }
        async public Task<IEnumerable<Roulette>> ListRoulette()
        {
            return await dynamouUtils.ListRoulette();
        }
        async public Task<IEnumerable<Player>> GetPlayersByRouletteId(string rouletteId)
        {
            return await dynamouUtils.GetPlayersByRouletteId(rouletteId: rouletteId);
        }
        async private Task<Roulette> GetRoulette(string rouletteId)
        {
            return await dynamouUtils.GetRoulette(rouletteId: rouletteId);
        }
        private void SelectWinner(IEnumerable<Player> players)
        {
            var numberWinner = new Random().Next(0, 36);
            var isBlakColor = numberWinner % 2 == 0;

            foreach (var player in players)
            {
                if (player.Number == numberWinner)
                {
                    player.Winner = true;
                    continue;
                }

                if (player.IsColor)
                {
                    var isBlackColorPlayer = player.Number % 2 == 0;
                    if (isBlakColor == isBlackColorPlayer)
                    {
                        player.Winner = true;
                    }
                }
            }
        }
        private void SetPrice(IEnumerable<Player> players)
        {
            foreach (var player in players)
            {
                player.Price = player.Ammount * 5;
                if (player.IsColor)
                {
                    player.Price = player.Ammount * 1.8;
                }
            }
        }
        async private void SaveGame(IEnumerable<Player> players)
        {
            foreach (var player in players)
            {
                await dynamouUtils.SaveBetGame(player: player);
            }
        }
    }
}
