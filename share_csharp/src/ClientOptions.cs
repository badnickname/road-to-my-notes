namespace MyNotes.Share;

/// <summary>
///     Параметры, необходимые для регистрации клиентского приложения
/// </summary>
public sealed class ClientOptions
{
    public string ClientId { get; set; }
    
    public string ClientSecret { get; set; }

    public string DisplayName { get; set; }

    public string RedirectUrl { get; set; }
}