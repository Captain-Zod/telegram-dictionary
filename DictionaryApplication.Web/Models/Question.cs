namespace DictionaryApplication.Web.Models;

public class Question
{
    public Question()
    {

    }
    public int Id { get; set; }
    public string Text { get; set; }
    public string Answer { get; set; }
    public List<UserRequestLog> UserRequestLogs { get; set; }
}
