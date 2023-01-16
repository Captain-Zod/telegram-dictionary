using DictionaryApplication.Web.Data;
using DictionaryApplication.Web.Handlers;
using DictionaryApplication.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DictionaryApplication.Web.Controllers;
public class StatsController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly Cache _dictionary;

    public StatsController(AppDbContext dbContext, Cache dictionary)
    {
        _dbContext = dbContext;
        _dictionary = dictionary;
    }
    public async Task<IActionResult> Index()
    {
        var all = await _dbContext.Questions
                            .Include(x => x.UserRequestLogs)
                            .Select(x => new StatsVM
                            {
                                Question = x.Text,
                                Count = x.UserRequestLogs.Count()
                            })
                            .OrderByDescending(x => x.Count)
                            .ToListAsync();
        return View(all);
    }
}
