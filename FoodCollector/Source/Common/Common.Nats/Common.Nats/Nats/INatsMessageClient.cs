namespace Common.Nats.Nats;

public interface INatsMessageClient
{
    Task PublishAsync(MessageBase message, string subject);
    Task<T> PublishWithResponseAsync<T>(MessageBase message, string subject);
    Task<IObservable<MessageBase>> SubscribeOnSubject(string subject);
}