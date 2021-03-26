using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Masivian.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masivian
{
    public class DynamoUtils
    {
        private const string ROULETTE = "Roulette";
        private const string PLAYERS = "Players";
        private const string OPEN = "Open";
        private const string CLOSED = "Closed";
        private readonly IAmazonDynamoDB _dynamoDBClient;
        public DynamoUtils(IAmazonDynamoDB dynamoDBClient)
        {
            _dynamoDBClient = dynamoDBClient;
        }
        async public Task<string> CreateRoulette()
        {
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();
            var fields = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue(myuuidAsString) },
                { "State", new AttributeValue("created") },
            };

            var rouleteRequest = new PutItemRequest(tableName: ROULETTE, fields);
            await _dynamoDBClient.PutItemAsync(rouleteRequest);

            return myuuidAsString;
        }
        async public Task<bool> OpenRoulette(Roulette roulette)
        {
            await SaveRouletteState(roulette: roulette, state: OPEN);
            return true;
        }
        async public Task<Roulette> GetRoulette(string rouletteId)
        {
            var key = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue(rouletteId) }
            };
            var rouleteItem = await _dynamoDBClient
                .GetItemAsync(new GetItemRequest(
                    tableName: ROULETTE,
                    key: key));
            if (rouleteItem.Item.Count == 0)
            {
                return new Roulette();
            }

            return new Roulette
            {
                Id = rouletteId,
                State = rouleteItem.Item["State"].S
            };
        }
        async public Task<bool> SaveBetGame(Player player)
        {
            await _dynamoDBClient.PutItemAsync(new PutItemRequest(tableName: PLAYERS,
                new Dictionary<string, AttributeValue> {
                    { "Id", new AttributeValue(player.RouletteId) },
                    { "UserId", new AttributeValue(player.UserId) },
                    { "IsColor", new AttributeValue { BOOL = player.IsColor } },
                    { "Number", new AttributeValue { N = player.Number.ToString()} },
                    { "Ammount", new AttributeValue { N = player.Ammount.ToString() } },
                    { "Price", new AttributeValue { N = player.Price.ToString() } },
                    { "Winner", new AttributeValue { BOOL = player.Winner} }
                }));
            return true;
        }
        async public Task<bool> CloseRoulette(Roulette roulette)
        {
            await SaveRouletteState(roulette: roulette, state: CLOSED);
            return true;
        }
        async public Task<bool> SaveRouletteState(Roulette roulette, string state)
        {
            var key = GetKey(roulette);
            var stateClosed = GetState(state);
            await _dynamoDBClient.UpdateItemAsync(
                 new UpdateItemRequest(tableName: ROULETTE,
                 key: key,
                 attributeUpdates: stateClosed
                 ));
            return true;
        }
        async public Task<IEnumerable<Roulette>> ListRoulette()
        {
            var rouleteItems = await _dynamoDBClient.ScanAsync(new ScanRequest(ROULETTE));
            return rouleteItems.Items
                .Select(item => new Roulette
                {
                    Id = item["Id"].S,
                    State = item["State"].S
                });
        }
        async public Task<IEnumerable<Player>> GetPlayersByRouletteId(string rouletteId)
        {
            QueryRequest qRequest = new QueryRequest
            {
                TableName = PLAYERS,
                ExpressionAttributeNames = new Dictionary<string, string>
                {{ "#Id", "Id" },{ "#Num", "Number" }},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                  { ":qId",   new AttributeValue { S = rouletteId} },
                },
                KeyConditionExpression = "#Id = :qId",
                ProjectionExpression = "#Id, UserId, IsColor, Price, Winner, Ammount, #Num"
            };

            var rouleteItems = await _dynamoDBClient.QueryAsync(qRequest);

            return rouleteItems.Items
                .Select(item => new Player
                {
                    UserId = item["UserId"].S,
                    Ammount = float.Parse(item["Ammount"].N),
                    IsColor = item["IsColor"].BOOL,
                    Number = int.Parse(item["Number"].N),
                    Price = float.Parse(item["Price"].N),
                    RouletteId = item["Id"].S,
                    Winner = item["Winner"].BOOL,
                });
        }
        private Dictionary<string, AttributeValueUpdate> GetState(string state)
        {
            return new Dictionary<string, AttributeValueUpdate>
            {
                { "State", new AttributeValueUpdate(
                    value:new AttributeValue(state),
                    action:new AttributeAction("PUT"))
                }
            };
        }
        private Dictionary<string, AttributeValue> GetKey(Roulette roulette)
        {
            return new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue(roulette.Id) }
            };
        }
    }
}
