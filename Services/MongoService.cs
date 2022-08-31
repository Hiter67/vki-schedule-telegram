using MongoDB.Bson;
using MongoDB.Driver;
using vki_schedule_telegram.Configurations;
using vki_schedule_telegram.Models;

namespace vki_schedule_telegram.Services;

public sealed class MongoService
{
    private readonly ILogger<MongoService> _logger;
    private readonly DataBaseConfiguration _dataBaseConfiguration;
    
    IMongoDatabase Database;
    IMongoCollection<User> Users;
    IMongoCollection<Parsed> Parseds;
    public MongoService(ILogger<MongoService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _dataBaseConfiguration = configuration
            .GetSection("DataBaseConfiguration").Get<DataBaseConfiguration>();
        var connection = new MongoUrlBuilder(_dataBaseConfiguration.ConnectionString);
        var client = new MongoClient(_dataBaseConfiguration.ConnectionString);
        
        Database = client.GetDatabase("vki-bot");
        
        Users = Database.GetCollection<User>("Users");
        Parseds = Database.GetCollection<Parsed>("Parseds");
    }
    
    public async Task<List<Parsed>> GetParseds(string name)
    {
        // строитель фильтров
        var builder = new FilterDefinitionBuilder<Parsed>();
        var filter = builder.Empty; // фильтр для выборки всех документов
        // фильтр по имени
        if (!String.IsNullOrWhiteSpace(name))
        {
            filter = filter & builder.Regex("Name", new BsonRegularExpression(name));
        }
        return await Parseds.Find(filter).ToListAsync();
    }
    public async Task<Parsed> GetParsed(long id)
    {
        return await Parseds.Find(new BsonDocument("_id", id)).FirstOrDefaultAsync();
    }
    public async Task<Parsed> GetParsed(string name)
    {
        return await Parseds.Find(new BsonDocument("Name", name)).FirstOrDefaultAsync();
    }
    public async Task AddParsed(Parsed parsed)
    {
        await Parseds.InsertOneAsync(parsed);
    }

    public async Task UpdateParser(Parsed parsed)
    {
        await RemoveParser(parsed.Name);
        await AddParsed(parsed);
    }
    public async Task RemoveParser(string name)
    {
        await Parseds.DeleteOneAsync(new BsonDocument("Name", name));
    }
    
    public async Task<List<User>> GetUsers(string? group, string? name)
    {
        // строитель фильтров
        var builder = new FilterDefinitionBuilder<User>();
        var filter = builder.Empty; // фильтр для выборки всех документов
        // фильтр по имени
        if (!String.IsNullOrEmpty(name))
        {
            filter = filter & builder.Regex("Name", new BsonRegularExpression(name));
        }
        // фильтр по группе
        if (!String.IsNullOrEmpty(group))
        {
            filter = filter & builder.Regex("Group", new BsonRegularExpression(group));
        }
        return await Users.Find(filter).ToListAsync();
    }
    public async Task<User> GetUser(long id)
    {
        return await Users.Find(new BsonDocument("_id", id)).FirstOrDefaultAsync();
    }
    public async Task AddUser(User user)
    {
        await Users.InsertOneAsync(user);
    }
    public async Task UpdateUser(User user)
    {
        await Users.ReplaceOneAsync(new BsonDocument("_id", user.Id), user);
    }
    public async Task RemoveUser(long tgId)
    {
        await Users.DeleteOneAsync(new BsonDocument("TgId", tgId));
    }
}