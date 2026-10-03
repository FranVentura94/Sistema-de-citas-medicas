namespace Core.Services;

public interface ITokenProvider
{
    Task<string> GetTokenAsync();
}