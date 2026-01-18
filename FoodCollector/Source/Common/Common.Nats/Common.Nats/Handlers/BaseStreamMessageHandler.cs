using Common.Nats.Handlers.Hello;
using MediatR;
using Serilog;

namespace Common.Nats.Handlers;

/// <summary>
///     Базовый обработчик стримов для получения сообщений Nats. Подпусывается на указанный субъект и передает сообщения на
///     конкретный обработчик используя MediatR.
/// </summary>
/// <remarks>Чтобы обрабатывать конкретные сообщения, необходимо создать соответствующий обработчик.</remarks>
/// <example><see cref="HelloMessageHandler" />.</example>
public class BaseStreamMessageHandler : IBaseStreamMessageHandler
{
    private static readonly ILogger Logger = Log.ForContext<BaseStreamMessageHandler>();
    private readonly IStreamNatsConsumer _consumer;
    private readonly IMediator _mediator;

    public BaseStreamMessageHandler(IStreamNatsConsumer consumer, IMediator mediator)
    {
        _consumer = consumer;
        _mediator = mediator;
    }

    public void StartMessageProcessing(
        string consumerName,
        string stream,
        string subject)
    {
        Logger.Information("Start nats message processing");
        _consumer.SubscribeOnStreamAsync(consumerName, stream, subject)
            .Subscribe(OnMessage);
    }

    private async void OnMessage(MessageBase messageBase)
    {
        Logger.Verbose("Get new message. Send it to concrete processor");
        await _mediator.Send(messageBase);
    }
}