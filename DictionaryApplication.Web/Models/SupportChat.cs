namespace DictionaryApplication.Web.Models;

public class SupportChat
{
    public int Id { get; set; }
    public bool Closed { get; set; }
    public long SupportChatId { get; set; }
    public long UserChatId { get; set; }
    public IList<SupportChatMessage> Messages { get; set; }
}
