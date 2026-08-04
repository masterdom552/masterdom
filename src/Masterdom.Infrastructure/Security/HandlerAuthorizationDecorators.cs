using System.Reflection;

namespace Masterdom.Infrastructure.Security;

internal static class AuthorizationDecoratorSupport
{
    public static TResult Execute<TRequest, TResult>(
        TRequest request,
        Func<TRequest, TResult> next,
        IRequestAuthorizationService authorizationService)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(authorizationService);

        var result = authorizationService.Authorize(request!);
        if (result.IsAllowed)
        {
            return next(request);
        }

        return CreateFailure<TResult>(result);
    }

    private static TResult CreateFailure<TResult>(Masterdom.Core.Security.AuthorizationResult authorizationResult)
    {
        var failureMethod = typeof(TResult).GetMethod(
            "Failure",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(string), typeof(string)],
            modifiers: null);

        if (failureMethod is null)
        {
            throw new InvalidOperationException($"Type '{typeof(TResult).FullName}' does not expose a compatible Failure factory.");
        }

        return (TResult)failureMethod.Invoke(
            obj: null,
            parameters:
            [
                authorizationResult.ErrorCode,
                authorizationResult.ErrorMessage ?? "The request is not authorized."
            ])!;
    }
}

internal sealed class PropertyCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.Properties.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.Properties.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public PropertyCommandAuthorizationDecorator(
        Masterdom.Modules.Properties.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class PropertyQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Properties.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Properties.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public PropertyQueryAuthorizationDecorator(
        Masterdom.Modules.Properties.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class PeopleCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.People.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.People.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public PeopleCommandAuthorizationDecorator(
        Masterdom.Modules.People.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class PeopleQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.People.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.People.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public PeopleQueryAuthorizationDecorator(
        Masterdom.Modules.People.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class LeaseCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.Lease.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.Lease.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public LeaseCommandAuthorizationDecorator(
        Masterdom.Modules.Lease.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class LeaseQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Lease.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Lease.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public LeaseQueryAuthorizationDecorator(
        Masterdom.Modules.Lease.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class TenancyCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.Tenancy.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.Tenancy.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public TenancyCommandAuthorizationDecorator(
        Masterdom.Modules.Tenancy.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class TenancyQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Tenancy.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Tenancy.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public TenancyQueryAuthorizationDecorator(
        Masterdom.Modules.Tenancy.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class MeteringCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.Metering.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.Metering.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public MeteringCommandAuthorizationDecorator(
        Masterdom.Modules.Metering.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class MeteringQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Metering.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Metering.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public MeteringQueryAuthorizationDecorator(
        Masterdom.Modules.Metering.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class BillingCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.Billing.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.Billing.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public BillingCommandAuthorizationDecorator(
        Masterdom.Modules.Billing.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class BillingQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Billing.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Billing.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public BillingQueryAuthorizationDecorator(
        Masterdom.Modules.Billing.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class FinancialLedgerCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.FinancialLedger.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.FinancialLedger.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public FinancialLedgerCommandAuthorizationDecorator(
        Masterdom.Modules.FinancialLedger.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class FinancialLedgerQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.FinancialLedger.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.FinancialLedger.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public FinancialLedgerQueryAuthorizationDecorator(
        Masterdom.Modules.FinancialLedger.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class PaymentCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.Payment.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.Payment.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public PaymentCommandAuthorizationDecorator(
        Masterdom.Modules.Payment.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class PaymentQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Payment.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Payment.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public PaymentQueryAuthorizationDecorator(
        Masterdom.Modules.Payment.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class ReportingQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Reporting.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Reporting.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public ReportingQueryAuthorizationDecorator(
        Masterdom.Modules.Reporting.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class NotificationsCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.Notifications.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.Notifications.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public NotificationsCommandAuthorizationDecorator(
        Masterdom.Modules.Notifications.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class NotificationsQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Notifications.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Notifications.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public NotificationsQueryAuthorizationDecorator(
        Masterdom.Modules.Notifications.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}

internal sealed class DocumentsCommandAuthorizationDecorator<TCommand, TResult>
    : Masterdom.Modules.Documents.Application.Support.ICommandHandler<TCommand, TResult>
{
    private readonly Masterdom.Modules.Documents.Application.Support.ICommandHandler<TCommand, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public DocumentsCommandAuthorizationDecorator(
        Masterdom.Modules.Documents.Application.Support.ICommandHandler<TCommand, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TCommand command) =>
        AuthorizationDecoratorSupport.Execute(command, _inner.Handle, _authorizationService);
}

internal sealed class DocumentsQueryAuthorizationDecorator<TQuery, TResult>
    : Masterdom.Modules.Documents.Application.Support.IQueryHandler<TQuery, TResult>
{
    private readonly Masterdom.Modules.Documents.Application.Support.IQueryHandler<TQuery, TResult> _inner;
    private readonly IRequestAuthorizationService _authorizationService;

    public DocumentsQueryAuthorizationDecorator(
        Masterdom.Modules.Documents.Application.Support.IQueryHandler<TQuery, TResult> inner,
        IRequestAuthorizationService authorizationService)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public TResult Handle(TQuery query) =>
        AuthorizationDecoratorSupport.Execute(query, _inner.Handle, _authorizationService);
}
