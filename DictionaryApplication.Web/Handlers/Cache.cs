using DictionaryApplication.Web.Data;
using DictionaryApplication.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace DictionaryApplication.Web.Handlers;

public class Cache
{
    private readonly IServiceScopeFactory _scopeFactory;

    public Cache(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        RefreshAsync();
    }

    public async void RefreshAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var _appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entities = await _appDbContext.Questions
                                        .ToDictionaryAsync(a => a.Text);

            Questions = entities;
        }
    }

    private Dictionary<string, Question> GetDefault()
    {
        return new Dictionary<string, Question>();
    }

    private Dictionary<string, Question> _questions;
    public Dictionary<string, Question> Questions
    {
        get
        {
            if (_questions == null)
                _questions = GetDefault();
            return _questions;
        }
        set
        {
            _questions = value;
        }
    }
    public string[] DontSuggest { get; set; } = new[]
    {
        "/start",
        "/search"
    };

    public Question[] Search(string text)
    {
        return Questions.Where(x => x.Key.Contains(text, StringComparison.OrdinalIgnoreCase))
                        .Select(x => x.Value)
                        .ToArray();
    }
}
