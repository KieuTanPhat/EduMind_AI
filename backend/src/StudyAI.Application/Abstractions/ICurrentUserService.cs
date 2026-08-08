namespace StudyAI.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
