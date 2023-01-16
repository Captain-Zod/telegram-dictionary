using DictionaryApplication.Web.Models;

namespace DictionaryApplication.Web.Services;

public interface IUserRequestLogService
{
    Task CreateAsync(UserRequestLog requestLog);
}
