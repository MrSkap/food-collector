namespace Common.Nats.Nats;

public interface IPersistantNatsMessageClient
{
    Task<IObservable<MessageBase>> SubscribeOnPersistantSubject(string subject);
    Task PublishPersistantAsync(MessageBase message, string subject);
}