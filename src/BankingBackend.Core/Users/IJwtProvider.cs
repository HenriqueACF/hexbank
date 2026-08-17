namespace BankingBackend.Core.Users;

public interface IJwtProvider
{
    string Generate(User user);
}