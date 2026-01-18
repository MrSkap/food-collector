using Common.Nats.Handlers.Hello;
using MediatR;
using Serilog;

namespace Common.Nats.Handlers;

/// <summary>
///     Базовый обработчик сообщений Nats. Подпусывается на указанный субъект и передает сообщения на конкретный обработчик
///     используя MediatR.
/// </summary>
/// <remarks>Чтобы обрабатывать конкретные сообщения, необходимо создать соответствующий обработчик.</remarks>
/// <example><see cref="HelloMessageHandler" />.</example>
public class BaseMessageHandler : IBaseMessageHandler
{
    private static readonly ILogger Logger = Log.ForContext<BaseMessageHandler>();
    private readonly INatsClientBase _clientBase;
    private readonly IMediator _mediator;

    public BaseMessageHandler(INatsClientBase clientBase, IMediator mediator)
    {
        _clientBase = clientBase;
        _mediator = mediator;
    }

    public void StartMessageProcessing(string subject)
    {
        if (string.IsNullOrEmpty(subject))
            throw new ArgumentException("Nats subject is empty");

        Logger.Information("Start nats message processing {Subject}", subject);
        _clientBase.SubscribeOnSubject(subject)
            .Subscribe(OnMessage);
    }

    private async void OnMessage(MessageBase messageBase)
    {
        Logger.Verbose("Get new message. Send it to concrete processor");
        await _mediator.Send(messageBase);
    }
}