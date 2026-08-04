using Masterdom.Modules.Payment.Application.Commands;
using Masterdom.Modules.Payment.Application.Queries;
using Masterdom.Modules.Payment.Application.Support;
using Masterdom.Modules.Payment.Domain.Entities.Payment;
using Masterdom.Modules.Payment.Domain.Repositories;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Modules.Payment.Application.Services;

public sealed class PaymentApplicationService : IPaymentApplicationService
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentUnitOfWork _unitOfWork;
    private readonly IPaymentPlatformOrchestrator _platformOrchestrator;

    public PaymentApplicationService(
        IPaymentRepository repository,
        IPaymentUnitOfWork unitOfWork,
        IPaymentPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public PaymentAggregate ReceivePayment(ReceivePaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = _repository.GetByReference(command.PaymentReference);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Payment '{command.PaymentReference.Value}' already exists.");
        }

        var payment = PaymentAggregate.Receive(
            PaymentId.New(),
            command.PaymentReference,
            command.PaymentAmount,
            command.PaymentDate,
            command.PaymentMethod,
            command.PaymentChannel,
            command.PaymentSource,
            command.ReceivedAtUtc);

        _unitOfWork.Execute(() => _repository.Add(payment));
        _platformOrchestrator.OnPaymentMutated(payment, "ReceivePayment");

        return payment;
    }

    public PaymentAggregate AllocatePayment(AllocatePaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = GetRequiredPayment(command.PaymentId);
        payment.Allocate(command.BillSettlements, command.AllocatedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(payment));
        _platformOrchestrator.OnPaymentMutated(payment, "AllocatePayment");

        return payment;
    }

    public PaymentAggregate ReversePayment(ReversePaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = GetRequiredPayment(command.PaymentId);
        payment.Reverse(command.Reason, command.ReversedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(payment));
        _platformOrchestrator.OnPaymentMutated(payment, "ReversePayment");

        return payment;
    }

    public PaymentAggregate VoidPayment(VoidPaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = GetRequiredPayment(command.PaymentId);
        payment.Void(command.Reason, command.VoidedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(payment));
        _platformOrchestrator.OnPaymentMutated(payment, "VoidPayment");

        return payment;
    }

    public PaymentAggregate? GetPayment(GetPaymentByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.PaymentId);
    }

    public PaymentAggregate? GetPayment(GetPaymentByReferenceQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetByReference(query.PaymentReference);
    }

    private PaymentAggregate GetRequiredPayment(PaymentId paymentId)
    {
        var payment = _repository.GetById(paymentId);
        if (payment is null)
        {
            throw new InvalidOperationException($"Payment '{paymentId}' was not found.");
        }

        return payment;
    }
}
