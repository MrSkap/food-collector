using Common.Nats.Contracts;
using ProtoBuf;

namespace Common.Nats;

/// <summary>
///     Базовое сообщение для Nats.
/// </summary>
/// <remarks>
///     Для отправки protobuf сообщений через клиенты Nats нужно создать наслендника от этого класса и добавить его в
///     аттрибуты.
/// </remarks>
/// <example>[ProtoInclude(1, typeof(HelloMessage))]</example>
[ProtoContract]
[ProtoInclude(1, typeof(HelloMessage))]
[ProtoInclude(2, typeof(HelloMessageResponse))]
public class MessageBase
{
}