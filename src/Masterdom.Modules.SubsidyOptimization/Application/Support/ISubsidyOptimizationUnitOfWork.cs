namespace Masterdom.Modules.SubsidyOptimization.Application.Support;

public interface ISubsidyOptimizationUnitOfWork
{
    void Execute(Action operation);
}
