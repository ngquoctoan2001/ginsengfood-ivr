using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

internal static class JunitReport
{
    public static int Run(string[] arguments)
    {
        if (arguments.Length != 1 || !Directory.Exists(arguments[0]))
        {
            Console.Error.WriteLine("JUNIT_REPORT_FAIL expected an existing report directory.");
            return 1;
        }

        string[] reports = Directory.GetFiles(arguments[0], "*-test-result.xml", SearchOption.AllDirectories);
        if (reports.Length == 0)
        {
            Console.Error.WriteLine("JUNIT_REPORT_FAIL no test reports found.");
            return 1;
        }

        // Validate every file before writing any file. DTDs and external entities are never read.
        List<(string Path, XDocument Document)> documents = [];
        int cases = 0;
        int renamed = 0;
        try
        {
            foreach (string report in reports)
            {
                using XmlReader reader = XmlReader.Create(report, new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                });
                XDocument document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
                if (document.Root?.Name != "testsuites")
                {
                    throw new InvalidDataException("Expected a JUnit testsuites root.");
                }

                XElement[] testCases = document.Descendants("testcase").ToArray();
                if (testCases.Length == 0)
                {
                    throw new InvalidDataException("Expected at least one testcase.");
                }

                foreach (XElement testCase in testCases)
                {
                    string name = testCase.Attribute("name")?.Value
                        ?? throw new InvalidDataException("Expected testcase name.");
                    string className = testCase.Attribute("classname")?.Value
                        ?? throw new InvalidDataException("Expected testcase classname.");
                    int argumentsStart = name.IndexOf('(', StringComparison.Ordinal);
                    if (argumentsStart >= 0)
                    {
                        if (argumentsStart == 0 || !name.EndsWith(')'))
                        {
                            throw new InvalidDataException("Unexpected parameterized test name.");
                        }

                        // Keep distinct theories distinct in GitLab. Group the hexadecimal ID so
                        // a numeric run inside it cannot resemble a phone in the artifact scanner.
                        string digest = Convert.ToHexStringLower(SHA256.HashData(
                            Encoding.UTF8.GetBytes(className + "\n" + name)));
                        name = $"{name[..argumentsStart]}[case-{digest[..8]}-{digest[8..16]}]";
                        testCase.SetAttributeValue("name", name);
                        renamed++;
                    }

                    cases++;
                }

                // xUnit may truncate long arguments into identical display names. Keep every
                // reported occurrence; do not silently collapse those cases in GitLab's UI.
                foreach (IGrouping<string, XElement> group in testCases.GroupBy(
                    testCase => testCase.Attribute("classname")!.Value + "\n" + testCase.Attribute("name")!.Value,
                    StringComparer.Ordinal).Where(group => group.Count() > 1))
                {
                    int occurrence = 0;
                    foreach (XElement testCase in group)
                    {
                        testCase.SetAttributeValue("name", testCase.Attribute("name")!.Value
                            + FormattableString.Invariant($"[occurrence-{++occurrence:D4}]"));
                    }
                }

                documents.Add((report, document));
            }

            foreach ((string report, XDocument document) in documents)
            {
                // Counts, outcomes, TestId, failure bodies, stack traces and console output stay
                // intact. PII elsewhere is still rejected by scan-pii; this only labels theories.
                File.WriteAllText(report, document.ToString(SaveOptions.DisableFormatting), new UTF8Encoding(false));
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or XmlException or InvalidDataException)
        {
            // Never repeat an XML parser's input fragment or a testcase argument in the log.
            Console.Error.WriteLine($"JUNIT_REPORT_FAIL {exception.GetType().Name}");
            return 1;
        }

        Console.WriteLine($"JUNIT_REPORT_PASS files={reports.Length} cases={cases} renamed={renamed}");
        return 0;
    }
}
