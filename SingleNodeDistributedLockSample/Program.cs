using SingleNodeDistributedLockSample;
using SingleNodeDistributedLockSample.Persistence;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Repository>();
builder.Services.AddSingleton<ICacheProvider, CacheProvider>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    var configOption = new ConfigurationOptions
    {
        EndPoints =
        {
             { builder.Configuration["redis:host"],Convert.ToInt32(builder.Configuration["redis:port"]) }
        },
        Password = builder.Configuration["redis:password"],
        DefaultDatabase = Convert.ToInt32(builder.Configuration["redis:defaultDb"])
    };
    builder.Services.AddStackExchangeRedisCache(config =>
    {
        config.Configuration = configOption.ToString();
    });
}
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
