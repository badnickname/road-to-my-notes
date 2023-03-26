namespace MyNotes.Share;

/// <summary>
///     Параметры, необходимые для авторизации в IdentityService в качестве администратора
/// </summary>
public sealed class IdentityServiceOptions
{
    public string Url { get; set; }

    public string Login { get; set; }

    public string Password { get; set; }
}