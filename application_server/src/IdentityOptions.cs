using MyNotes.Share;

namespace MyNotes.Application;

/// <summary>
///     Опции для регистрации клиентского и серверных приложений
/// </summary>
public sealed class IdentityOptions
{
    public ClientOptions ApplicationClient { get; set; }

    public ClientOptions ApplicationServer { get; set; }
}