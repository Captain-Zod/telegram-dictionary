using DictionaryApplication.Web.Services;
using DictionaryApplication.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace DictionaryApplication.Web.Handlers;

public class Handler
{
    private readonly Cache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    public Handler(Cache cache, IServiceScopeFactory scopeFactory)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

    private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
    {
        if (message.Type != MessageType.Text)
            return;
        string answer;
        if (_cache.Questions.ContainsKey(message.Text))
        {
            answer = _cache.Questions[message.Text].Answer;
            using (var scope = _scopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IUserRequestLogService>();
                await service.CreateAsync(new Models.UserRequestLog
                {
                    IdQuestion = _cache.Questions[message.Text].Id,
                    UserName = message.From?.Username,
                    UserFirstName = message.From?.FirstName,
                    UserLastName = message.From?.LastName,
                    UserId = message.From?.Id ?? 0,
                });
            }
        }
        else
            answer = "Error message!";

        if(message.Text != "/start")
        {
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: answer);
            return;
        }

        ReplyKeyboardMarkup inlineKeyboard = new(
                    _cache.Questions.Keys.Except(_cache.DontSuggest).Select(x => new[] { new KeyboardButton(x) })
                );

        await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                    text: answer,
                                                    replyMarkup: inlineKeyboard);

        if (_cache.Questions.ContainsKey("/search")) //todo fix
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                text: _cache.Questions["/search"].Answer,
                                                replyMarkup: new InlineKeyboardMarkup(new[]
                                                        {
                                                                new[] {InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Search") }
                                                        }));
    }

    private async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
    {
        if (inlineQuery.Query.Length < 3)
            return;
        var list = _cache.Search(inlineQuery.Query);
        var results = list.Select(x =>
                new InlineQueryResultArticle(
                        id: x.Id.ToString(),
                        title: x.Text,
                        inputMessageContent: new InputTextMessageContent(
                            x.Text
                        )
                    )).ToArray();

        await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                               results: results,
                                               isPersonal: true,
                                               cacheTime: 0);
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    private Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
}
