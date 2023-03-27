namespace MyNotes.Share;

/// <summary>
///     Параметры, необходимые для регистрации клиентского приложения
/// </summary>
public sealed class ClientOptions
{
    /// <summary>
    ///     Тип клиента
    /// </summary>
    public enum ClientType
    {
        /// <summary>
        ///     Application (client)
        /// </summary>
        Client,

        /// <summary>
        ///     Resource Server
        /// </summary>
        Resource
    }

    /// <summary>
    ///     Идентификатор клиента
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    ///     Пароль клиента (используется только для Type = Resource)
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    ///     Отображаемое имя клиента
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     Url, на который происходит редирект после авторизации
    /// </summary>
    public string RedirectUrl { get; set; }

    /// <summary>
    ///     Тип клиента
    /// </summary>
    public ClientType Type { get; set; }

    /// <summary>
    ///     Области доступа
    /// </summary>
    /// <remarks>
    ///     Если Type = Client, то добавляет скопы в Permissions.
    ///     Если Type = Resource, то создает описание скопа для клиента
    /// </remarks>
    public ICollection<string> Scopes { get; set; }
}