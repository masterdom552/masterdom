using Masterdom.Modules.Notifications.Application.Commands;
using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Modules.Notifications.Application.Services;
using Masterdom.Modules.Notifications.Application.Support;

namespace Masterdom.Modules.Notifications.Application.Handlers.Commands;

public sealed class GenerateNotificationCommandHandler
    : ICommandHandler<GenerateNotificationCommand, ExecutionResult<GeneratedNotification>>
{
    private readonly INotificationApplicationService _applicationService;

    public GenerateNotificationCommandHandler(INotificationApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<GeneratedNotification> Handle(GenerateNotificationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var generated = _applicationService.Generate(
                command.EventCode,
                command.RecipientId,
                command.RequestedAtUtc,
                command.Parameters,
                command.RequestedDeliveryAtUtc);

            return ExecutionResult<GeneratedNotification>.Success(generated);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<GeneratedNotification>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<GeneratedNotification>.Failure("not_allowed", ex.Message);
        }
    }
}
