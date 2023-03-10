using System.Reflection;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run();
// var benchmark = new SendNotificationBenchmark();
// benchmark.Setup();
// await benchmark.Old();
// await benchmark.New();
