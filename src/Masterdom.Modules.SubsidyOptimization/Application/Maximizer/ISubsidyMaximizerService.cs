namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public interface ISubsidyMaximizerService
{
    SubsidyMaximizerResult Execute(SubsidyMaximizerRequest request);
}
