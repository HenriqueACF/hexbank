using BankingBackend.Core.Common;
using BankingBackend.Core.Users;
using FluentValidation;
using MediatR;

namespace BankingBackend.Application.Users.Register;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork,
        IValidator<RegisterUserCommand> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var message = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<RegisterUserResponse>(new Error("Validation.Failed", message));
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(emailResult.Error);

        var cpfResult = Cpf.Create(request.Cpf);
        if (cpfResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(cpfResult.Error);

        if (await _userRepository.ExistsByEmailAsync(emailResult.Value, cancellationToken))
            return Result.Failure<RegisterUserResponse>(RegisterUserErrors.EmailAlreadyInUse);

        if (await _userRepository.ExistsByCpfAsync(cpfResult.Value, cancellationToken))
            return Result.Failure<RegisterUserResponse>(RegisterUserErrors.CpfAlreadyInUse);

        var passwordHash = _passwordHasher.Hash(request.Password);
        var userResult = User.Create(cpfResult.Value, emailResult.Value, passwordHash, UserRole.Customer);
        if (userResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(userResult.Error);

        var user = userResult.Value;
        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtProvider.Generate(user);

        return new RegisterUserResponse(user.Id, accessToken);
    }
}