using RedlockSample.Common;
using SingleNodeDistributedLockSample.Persistence;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var configOption = new ConfigurationOptions
{
    EndPoints =
        {
             { builder.Configuration["redis:host"],Convert.ToInt32(builder.Configuration["redis:port"]) }
        },
    Password = builder.Configuration["redis:password"],
    DefaultDatabase = Convert.ToInt32(builder.Configuration["redis:defaultDb"])
};
builder.Services.AddSingleton(typeof(Redlock.CSharp.Redlock), new Redlock.CSharp.Redlock(ConnectionMultiplexer.Connect(configOption.ToString())));
builder.Services.AddSingleton<IDistributedLockManager, RedlockDistributedLockManager>();
builder.Services.AddSingleton<Repository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
