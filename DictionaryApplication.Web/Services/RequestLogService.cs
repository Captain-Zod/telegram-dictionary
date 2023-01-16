using DictionaryApplication.Web.Data;
using DictionaryApplication.Web.Models;

namespace DictionaryApplication.Web.Services;

public class UserRequestLogService : IUserRequestLogService
{
    private readonly AppDbContext _dbContext;
    public UserRequestLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task CreateAsync(UserRequestLog requestLog)
    {
        _dbContext.UserRequestLogs.Add(requestLog);
        await _dbContext.SaveChangesAsync();
    }
}
