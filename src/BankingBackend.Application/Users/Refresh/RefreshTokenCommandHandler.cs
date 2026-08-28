using BankingBackend.Application.Users.Login;
using BankingBackend.Core.Common;
using BankingBackend.Core.Users;
using FluentValidation;
using MediatR;

namespace BankingBackend.Application.Users.Refresh;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RefreshTokenCommand> _validator;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IRefreshTokenProvider refreshTokenProvider,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork,
        IValidator<RefreshTokenCommand> validator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _refreshTokenProvider = refreshTokenProvider;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var message = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<LoginResponse>(new Error("Validation.Failed", message));
        }

        var stored = await _refreshTokenRepository.GetByValueAsync(
            request.RefreshTokenValue, cancellationToken);
        
        if (stored is null || stored.IsRevoked || stored.IsExpired(DateTime.UtcNow))
            return Result.Failure<LoginResponse>(RefreshTokenErrors.Invalid);

        var user = await _userRepository.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<LoginResponse>(RefreshTokenErrors.Invalid);

        var accessToken = _jwtProvider.Generate(user);
        
        var revokeResult = stored.Revoke();
        if (revokeResult.IsFailure)
            return Result.Failure<LoginResponse>(revokeResult.Error);

        var newRefreshToken = _refreshTokenProvider.Create(user.Id);
        _refreshTokenRepository.Add(newRefreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, newRefreshToken.Value.ToString());
    }
}