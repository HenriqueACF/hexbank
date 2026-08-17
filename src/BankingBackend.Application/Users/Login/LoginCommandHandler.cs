using BankingBackend.Core.Common;
using BankingBackend.Core.Users;
using MediatR;

namespace BankingBackend.Application.Users.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);

        var passwordMatches = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordMatches)
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result.Failure<LoginResponse>(AuthErrors.InactiveAccount);

        var accessToken = _jwtProvider.Generate(user);
        return new LoginResponse(accessToken);
    }
}