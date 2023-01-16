namespace DictionaryApplication.Web.Models;

public class UserRequestLog
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; }
    public string UserFirstName { get; set; }
    public string UserLastName { get; set; }
    public int IdQuestion { get; set; }
    public Question Question { get; set; }
}
