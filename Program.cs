using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Masivian
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dynamoDBClient = services.GetRequiredService<IAmazonDynamoDB>();
                CreateTables(dynamoDBClient);
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        async public static void CreateTables(IAmazonDynamoDB dynamoDBClient)
        {
            var tables = await dynamoDBClient.ListTablesAsync(20);

            var requestTableRoulette = new CreateTableRequest
            {
                TableName = "Roulette",
                AttributeDefinitions = new List<AttributeDefinition>{
                    new AttributeDefinition{AttributeName = "Id",AttributeType = "S"}
                },
                KeySchema = new List<KeySchemaElement>
                  {new KeySchemaElement{AttributeName = "Id",KeyType = "HASH"}},
                ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 10, WriteCapacityUnits = 5 },
            };
            var requestTablePlayers = new CreateTableRequest
            {
                TableName = "Players",
                AttributeDefinitions = new List<AttributeDefinition>{
                    new AttributeDefinition{AttributeName = "Id", AttributeType = "S"},
                    new AttributeDefinition{AttributeName = "UserId", AttributeType = "S"},                    
                },
                KeySchema = new List<KeySchemaElement>{
                    new KeySchemaElement{AttributeName = "Id",KeyType = "HASH"},
                    new KeySchemaElement{AttributeName = "UserId",KeyType = "RANGE"}
                },
                ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 10, WriteCapacityUnits = 5 }
            };

            if (!tables.TableNames.Contains("Roulette"))
            {
                await dynamoDBClient.CreateTableAsync(requestTableRoulette);
            }

            if (!tables.TableNames.Contains("Players"))
            {
                await dynamoDBClient.CreateTableAsync(requestTablePlayers);
            }
        }
    }
}
