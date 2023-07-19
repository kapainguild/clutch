using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Validators;

namespace Clutch.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default.WithToolchain(CsProjCoreToolchain.NetCoreApp60));
            AddJob(Job.Default.WithToolchain(CsProjClassicNetToolchain.Net472)); 

            AddLogger(ConsoleLogger.Default);

            AddColumnProvider(DefaultColumnProviders.Instance); 
            AddExporter(CsvExporter.Default);
            AddAnalyser(EnvironmentAnalyser.Default, OutliersAnalyser.Default, 
                MinIterationTimeAnalyser.Default, MultimodalDistributionAnalyzer.Default, 
                RuntimeErrorAnalyser.Default, ZeroMeasurementAnalyser.Default);

            AddValidator(/*JitOptimizationsValidator.FailOnError, */BaselineValidator.FailOnError,
                SetupCleanupValidator.FailOnError, RunModeValidator.FailOnError,
                GenericBenchmarksValidator.DontFailOnError, DeferredExecutionValidator.FailOnError);
        }
    }


}
