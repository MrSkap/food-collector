using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Net;
using Serilog;

namespace Common.Nats.Nats;

public class FullNatsMessageClient: INatsMessageClient, IPersistantNatsMessageClient, IAsyncDisposable
{
    private static readonly ILogger Logger = Log.ForContext<FullNatsMessageClient>();
    private readonly NatsConfiguration _configuration;
    private readonly INatsClient _natsClient;
    private readonly ConcurrentDictionary<string, OpenedConnection> _openedConnections = new ConcurrentDictionary<string, OpenedConnection>();
    private INatsJSContext? _natsJsContext;

    public FullNatsMessageClient(NatsConfiguration natsConfiguration)
    {
        _configuration = natsConfiguration;
        _natsClient = new NatsClient(_configuration.ConnectionString, _configuration.ServiceName, _configuration.Credentials);
    }

    public async Task StartAsync()
    {
        Logger.Information("-> Initializing Nats Client");
        
        await _natsClient.ConnectAsync();
        await InitJetStream();
        
        Logger.Information("<- Initializing Nats Client");
    }
    
    public async Task PublishAsync(MessageBase message, string subject)
    {
        ThrowIfClientIsNotConnected();
        await _natsClient.PublishAsync(subject, message);
    }

    public Task<T> PublishWithResponseAsync<T>(MessageBase message, string subject)
    {
        ThrowIfClientIsNotConnected();
        throw new NotImplementedException();
    }

    public Task<IObservable<MessageBase>> SubscribeOnSubject(string subject)
    {
        ThrowIfClientIsNotConnected();
        throw new NotImplementedException();
    }

    public async Task PublishPersistantAsync(MessageBase message, string subject)
    {
        ThrowIfClientIsNotConnected();
        await _natsClient.PublishAsync(subject, message);
    }

    public async Task<IObservable<MessageBase>> SubscribeOnPersistantSubject(string stream)
    {
        ThrowIfJetStreamIsNotAvailable();

        if (_openedConnections.TryGetValue(stream, out var openedConnection))
        {
            return openedConnection.Observable;
        }
        
        var consumer = await _natsJsContext!.CreateOrUpdateConsumerAsync(stream: stream, new ConsumerConfig(_configuration.ServiceName));
        var clientToken = CancellationTokenSource.CreateLinkedTokenSource();

        var messageObservable = Observable.Create<MessageBase>(async (observer, ct) =>
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, clientToken.Token);
            
            try
            {
                await foreach (NatsJSMsg<MessageBase> msg in consumer
                                   .ConsumeAsync<MessageBase>()
                                   .WithCancellation(linkedCts.Token))
                {
                    try
                    {
                        if (msg.Data is not null)
                        {
                            observer.OnNext(msg.Data);
                        }
                        
                        // Подтверждаем обработку
                        await msg.AckAsync(cancellationToken: linkedCts.Token);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        
                        // Если наблюдатель отписался или ошибка
                        if (ct.IsCancellationRequested)
                        {
                            await msg.AckTerminateAsync(cancellationToken: linkedCts.Token); // Не пытаться доставлять снова
                            break;
                        }
                        
                        await msg.NakAsync(cancellationToken: linkedCts.Token); // Попробовать снова позже
                    }
                }
                
                observer.OnCompleted();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                observer.OnError(ex);
            }
            catch (OperationCanceledException)
            {
                observer.OnCompleted();
            }
        });
        
        _openedConnections.TryAdd(stream, new OpenedConnection(clientToken, messageObservable));
        return messageObservable;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var openedConnection in _openedConnections.Values)
        {
            await openedConnection.CancellationTokenSource.CancelAsync();
            openedConnection.CancellationTokenSource.Dispose();
        }
        await _natsClient.DisposeAsync();
    }

    private async Task InitJetStream()
    {
        if (_configuration.JetStreamConfigurations is null)
        {
            return;
        }

        ThrowIfClientIsNotConnected();

        _natsJsContext = _natsClient.CreateJetStreamContext();
        foreach (var streamConfiguration in _configuration.JetStreamConfigurations)
        {
            Logger.Information("Initializing stream {StreamName}", streamConfiguration.StreamName);
            var streamConfig = new StreamConfig(
                name: streamConfiguration.StreamName, 
                subjects: [streamConfiguration.SubjectWildcard]
            )
            {
                Storage = streamConfiguration.Storage,
                MaxAge = TimeSpan.FromMinutes(streamConfiguration.MaxAgeMin),
                MaxBytes = streamConfiguration.MaxBytes,
                Retention = streamConfiguration.Retention,
                Discard = streamConfiguration.DiscardPolicy,
                MaxMsgs =  streamConfiguration.MaxMessages,
            };
            await _natsJsContext.CreateOrUpdateStreamAsync(streamConfig);
        }
    }
    
    private void ThrowIfJetStreamIsNotAvailable()
    {
        ThrowIfClientIsNotConnected();
        if (_natsJsContext is null)
        {
            throw new NatsJSException("The NATS JetStream is not initialized");
        }
    }

    private void ThrowIfClientIsNotConnected()
    {
        if (_natsClient.Connection.ConnectionState != NatsConnectionState.Open)
        {
            throw new NatsException("The NATS client is not connected");
        }
    }
    
    private record OpenedConnection(CancellationTokenSource CancellationTokenSource, IObservable<MessageBase> Observable);
}