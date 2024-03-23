using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Consumer.Data;
using Consumer.DTOs;
using Consumer.Exceptions;
using Consumer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

var dbOptions = new DbContextOptionsBuilder<ApixDbContext>()
    .UseNpgsql(@"Host=localhost;Port=5433;Database=APIx_DB;UserName=postgres;Password=postgres;
        Enlist=False;No Reset On Close=True;") //TODO: mudar para endereço do container
    .Options;

var cacheOpts = new ConfigurationOptions
{
    EndPoints = { "localhost:6379" },
    AbortOnConnectFail = false
};
IDistributedCache cache = new RedisCache(new RedisCacheOptions
{
    Configuration = "localhost:6379"
});

var httpClient = new HttpClient();

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/"
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "concilliations",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) => handleReceivedMessage(model, ea);


channel.BasicConsume(queue: "concilliations",
 autoAck: false,
 consumer: consumer
 );

Console.Read();

void handleReceivedMessage(object? model, BasicDeliverEventArgs ea)
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var concilliationId = int.Parse(message);
    using var db = new ApixDbContext(dbOptions);
    Concilliation? concilliation = null;
    OutputVar outputVar = new OutputVar();
    var start = DateTime.Now;

    try
    {
        concilliation = retrieveConcilliationById(concilliationId, db);
        HttpResponseMessage getConcilliationFileResponse = getConcilliationFile(concilliation);
        if (!getConcilliationFileResponse.IsSuccessStatusCode) throw new InvalidInputFileException("Failed to retrieve concilliation file");

        PopulateOutputVar(getConcilliationFileResponse, outputVar, concilliation, db);
        updateConcilliationStatus(concilliation, "SUCCESS", db);
        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        sendRequestToPSP(concilliation, outputVar);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);

        if (concilliation == null)
        {
            return;
        }

        updateConcilliationStatus(concilliation, "FAILED", db);
    }

    Console.WriteLine($"Concilliation {concilliationId} processed in {(DateTime.Now - start).TotalSeconds}s");
    Console.WriteLine($"DatabaseToFile: {outputVar.DatabaseToFile.Count}");
    Console.WriteLine($"FileToDatabase: {outputVar.FileToDatabase.Count}");
    Console.WriteLine($"DifferentStatus: {outputVar.DifferentStatus.Count}");
    Console.WriteLine("--------------------------------------------------");
};

Concilliation retrieveConcilliationById(int concilliationId, ApixDbContext db)
{
    Concilliation? concilliation = null;
    string cacheKey = $"concilliation-{concilliationId}";

    string? cachedConcilliation = cache.GetString(cacheKey);

    if (cachedConcilliation != null)
    {
        removeFromCache(concilliationId);
        concilliation = JsonConvert.DeserializeObject<Concilliation>(cachedConcilliation);
    }
    else
    {
        concilliation = db.Concilliations
            .FirstOrDefault(p => p.Id == concilliationId);
    }

    return concilliation ?? throw new ConcilliationNotFoundException("Payment not found");
}

void removeFromCache(int concilliationId)
{
    string cacheKey = $"concilliation-{concilliationId}";
    cache.Remove(cacheKey);
}

void updateConcilliationStatus(Concilliation concilliation, string status, ApixDbContext db)
{
    string query = @"
        UPDATE ""Concilliation"" 
        SET ""Status"" = {0}, ""UpdatedAt"" = {1}
        WHERE ""Id"" = {2};";
    db.Database.ExecuteSqlRaw(query, [status, DateTime.UtcNow, concilliation.Id]);
    db.SaveChanges();
}

HttpResponseMessage getConcilliationFile(Concilliation concilliation)
{
    var taskResponseFromDestiny = httpClient.GetAsync(concilliation.FileUrl).Result;

    return taskResponseFromDestiny;
}

void PopulateOutputVar(HttpResponseMessage response, OutputVar outputVar, Concilliation concilliation, ApixDbContext db)
{
    int GMT = -3;
    DateTime startDate = concilliation.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime().AddHours(GMT);
    DateTime endDate = concilliation.Date.ToDateTime(TimeOnly.MaxValue).ToUniversalTime().AddHours(GMT);
    string content = response.Content.ReadAsStringAsync().Result;
    string[] lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    Count count = db.Database
        .SqlQueryRaw<Count>(@"
            SELECT 
                COUNT(p.""Id"")
            FROM ""Payment"" AS p
            INNER JOIN ""PaymentProviderAccount"" AS a ON a.""Id"" = p.""PaymentProviderAccountId"" 
            WHERE 
                a.""PaymentProviderId"" = {0}
                AND p.""CreatedAt"" > {1}
                AND p.""CreatedAt"" < {2}
            ", [concilliation.PaymentProviderId, startDate, endDate])
        .First();

    Console.WriteLine("Count " + count.count);
    int pageSize = 10000;
    int pages = (int)Math.Ceiling((double)count.count / pageSize);
    Console.WriteLine("Pages " + pages);
    List<BaseJson> paymentsDB = new();

    for (int i = 1; i <= pages; i++)
    {
        int pageNumber = i;
        int offset = (pageNumber - 1) * pageSize; 
        List<BaseJson> part = db.Database
            .SqlQueryRaw<BaseJson>(@"
            SELECT 
                p.""Id"", p.""Status""
            FROM ""Payment"" AS p
            INNER JOIN ""PaymentProviderAccount"" AS a ON a.""Id"" = p.""PaymentProviderAccountId"" 
            WHERE 
                a.""PaymentProviderId"" = {0}
                AND p.""CreatedAt"" > {1}
                AND p.""CreatedAt"" < {2}
            ORDER BY p.""Id""
            LIMIT {3} OFFSET {4}
        ", [concilliation.PaymentProviderId, startDate, endDate, pageSize, offset])
            .ToList();

        paymentsDB.AddRange(part);
    }

    Dictionary<int, string> databasePayments = paymentsDB.ToDictionary(p => p.Id, p => p.Status);
    Dictionary<int, string> receivedPayments = new();

    foreach (string line in lines)
    {
        BaseJson baseJson = JsonConvert.DeserializeObject<BaseJson>(line) ??
            throw new InvalidInputFileException("Invalid input file");

        receivedPayments.Add(baseJson.Id, baseJson.Status);

        if (!databasePayments.TryGetValue(baseJson.Id, out string? value))
        {
            outputVar.FileToDatabase.Add(baseJson.Id, baseJson.Status);
        }
        else if (value != baseJson.Status)
        {
            outputVar.DifferentStatus.Add(baseJson.Id, baseJson.Status);
        }
    };

    foreach (KeyValuePair<int, string> payment in databasePayments)
    {
        if (!receivedPayments.TryGetValue(payment.Key, out string? value))
        {
            outputVar.DatabaseToFile.Add(payment.Key, payment.Value);
        }
    };
}

void sendRequestToPSP(Concilliation concilliation, OutputVar outputVar)
{
    string PSPUrl = concilliation.Postback;
    ReqOutpuConcilliation reqOutpuConcilliation = new ReqOutpuConcilliation(outputVar);
    string serialized = JsonConvert.SerializeObject(reqOutpuConcilliation);
    HttpContent content = new StringContent(serialized, Encoding.UTF8, "application/json");
    _ = httpClient.PostAsync(PSPUrl, content);
}

public class Count
{
    public int count { get; set; }
}