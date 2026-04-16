// Designed and Implemented by Amin Ghaderi

using FlashBid.Core;
using FlashBid.Infrastructure;
using FlashBid.Realtime;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<IRedisAuctionStateStore, RedisAuctionStateStore>();
builder.Services.AddSingleton<AuctionInitializer>();
builder.Services.AddSingleton<IBiddingEngine, BiddingEngine>();

builder.Services.AddControllers();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapHub<AuctionHub>("/auctionHub");

app.Run();
