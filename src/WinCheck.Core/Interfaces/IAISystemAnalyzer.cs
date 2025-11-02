using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface IAISystemAnalyzer
{
    /// <summary>
    /// Run comprehensive AI-powered system analysis
    /// </summary>
    Task<SystemAnalysisReport> AnalyzeSystemAsync();

    /// <summary>
    /// Get AI-generated optimization plan
    /// </summary>
    Task<OptimizationPlan> GenerateOptimizationPlanAsync();

    /// <summary>
    /// Execute optimization plan with AI monitoring
    /// </summary>
    Task<OptimizationResult> ExecuteOptimizationPlanAsync(OptimizationPlan plan);

    /// <summary>
    /// Get natural language explanation of system status
    /// </summary>
    Task<string> ExplainSystemStatusAsync();

    /// <summary>
    /// Answer user questions about the system
    /// </summary>
    Task<string> AskQuestionAsync(string question);
}
