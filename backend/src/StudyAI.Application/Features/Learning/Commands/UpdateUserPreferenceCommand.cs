using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Learning;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Learning.Commands;

public sealed record UpdateUserPreferenceCommand(Guid UserId, UpdateUserPreferenceRequest Request) : IRequest<UserPreferenceResponse>;

public sealed class UpdateUserPreferenceCommandValidator : AbstractValidator<UpdateUserPreferenceCommand>
{
    public UpdateUserPreferenceCommandValidator()
    {
        RuleFor(x => x.Request.LearningLevel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.LearningGoal).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.PreferredLanguage).NotEmpty().MaximumLength(20);
    }
}

public sealed class UpdateUserPreferenceCommandHandler : IRequestHandler<UpdateUserPreferenceCommand, UserPreferenceResponse>
{
    private readonly IApplicationDbContext _db;

    public UpdateUserPreferenceCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<UserPreferenceResponse> Handle(UpdateUserPreferenceCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.Users.Include(x => x.Preference).SingleOrDefaultAsync(x => x.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        var preference = user.Preference;
        if (preference is null)
        {
            preference = new UserPreference(user.Id);
            user.SetPreference(preference);
            _db.UserPreferences.Add(preference);
        }

        preference.Update(command.Request.LearningLevel.Trim(), command.Request.LearningGoal.Trim(), command.Request.PreferredLanguage.Trim());
        await _db.SaveChangesAsync(cancellationToken);
        return new UserPreferenceResponse(preference.LearningLevel, preference.LearningGoal, preference.PreferredLanguage);
    }
}
