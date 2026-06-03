using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace MediaManager
{
    internal static class TestRunner
    {
        internal static bool Run()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var sb = new StringBuilder();

            void Out(string line)
            {
                Console.WriteLine(line);
                sb.AppendLine(line);
            }

            Out("\n###### MediaManager Tests ######\n");

            var testTypes = new[] { typeof(AgeCheckerTests), typeof(EpisodeFileTests), typeof(MovieFileTests), typeof(MediaFileTests), typeof(StatListTests), typeof(LibCheckerTests), typeof(ReflectorTests) };
            int passed = 0, failed = 0;

            foreach (var type in testTypes)
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    var capture = new StringWriter();
                    var originalOut = Console.Out;
                    Console.SetOut(capture);
                    try
                    {
                        method.Invoke(null, null);
                        Console.SetOut(originalOut);
                        Out($"[PASS] {method.Name}");
                        passed++;
                    }
                    catch (TargetInvocationException tie)
                    {
                        Console.SetOut(originalOut);
                        string innerOutput = capture.ToString().TrimEnd();
                        if (innerOutput.Length > 0)
                            Out(innerOutput);
                        Out($"[FAIL] {method.Name}: {tie.InnerException?.Message ?? tie.Message}");
                        failed++;
                    }
                    catch (Exception ex)
                    {
                        Console.SetOut(originalOut);
                        string innerOutput = capture.ToString().TrimEnd();
                        if (innerOutput.Length > 0)
                            Out(innerOutput);
                        Out($"[FAIL] {method.Name}: {ex.Message}");
                        failed++;
                    }
                }
            }

            Out($"\n-------------------------------");
            Out($"Results: {passed} passed, {failed} failed");
            Out($"-------------------------------\n");

            try
            {
                string repoRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Prog.ProjectPath, ".."));
                string logsPath = Path.Combine(repoRoot, "logs");
                if (!Directory.Exists(logsPath))
                    Directory.CreateDirectory(logsPath);
                string logPath = Path.Combine(logsPath, $"test-{timestamp}.log");
                File.WriteAllText(logPath, sb.ToString());
                Console.WriteLine($"  Log: {logPath}");
            }
            catch { /* log write failures must not break the test run */ }

            return failed == 0;
        }
    }
}
