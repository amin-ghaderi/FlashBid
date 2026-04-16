// Designed and Implemented by Amin Ghaderi

using FlashBid.Core;
using FlashBid.Infrastructure;
using FlashBid.Realtime;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

builder.Services.AddSingleton<IRedisAuctionStateStore, RedisAuctionStateStore>();
builder.Services.AddSingleton<AuctionInitializer>();
builder.Services.AddSingleton<IBiddingEngine, BiddingEngine>();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapHub<AuctionHub>("/auctionHub");

app.Run();
