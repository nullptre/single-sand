namespace SingleSand.Amq.AccessModel
{
    public interface IMessage
    {
        long? ConversationId { get; set; }

        string ResponseQueueName { get; set; }
         
    }
}