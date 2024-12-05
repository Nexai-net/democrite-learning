namespace Silo.Models
{
    [GenerateSerializer]
    public sealed record class ChatMessage(Guid MessageId, string SenderUserName, string Message, DateTime UTCCreationTime);
}
