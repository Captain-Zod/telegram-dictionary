using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using DictionaryApplication.Web.Services;
using DictionaryApplication.Web.Handlers;
using DictionaryApplication.Web;
using DictionaryApplication.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("FAQBotDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    { options.UseSqlServer(builder.Configuration.GetConnectionString("Default")); });
}
builder.Services.AddScoped<ApplicationDbContextInitialiser>();

builder.Services.AddScoped<IUserRequestLogService, UserRequestLogService>();
builder.Services.AddSingleton<Cache>();
builder.Services.AddSingleton<Handler>();
Configuration.BotToken = builder.Configuration.GetSection("BotConfiguration").GetSection("Token").Value;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        if (context.Database.IsSqlServer())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        //var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        //logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Initialise and seed database
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var Bot = new TelegramBotClient(Configuration.BotToken);

User me = await Bot.GetMeAsync();
ReceiverOptions receiverOptions = new() { AllowedUpdates = new[] { UpdateType.InlineQuery, UpdateType.Message } };
var handler = app.Services.GetService<Handler>();
Bot.StartReceiving(handler.HandleUpdateAsync,
                   handler.HandleErrorAsync,
                   receiverOptions);

app.Run();
