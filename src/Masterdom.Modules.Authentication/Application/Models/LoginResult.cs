namespace Masterdom.Modules.Authentication.Application.Models;

public sealed record LoginResult(string AccessToken, DateTime ExpiresAtUtc);
