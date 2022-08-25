using vki_schedule_telegram.Configurations;

namespace vki_schedule_telegram.Services;

public class DataBaseService
{
    private readonly ILogger<DataBaseService> _logger;
    private readonly DataBaseConfiguration _dataBaseConfiguration;
    public DataBaseService(ILogger<DataBaseService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _dataBaseConfiguration = configuration
            .GetSection("DataBaseConfiguration").Get<DataBaseConfiguration>();
    }
}