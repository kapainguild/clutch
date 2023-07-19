using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Clutch.Configuration.Issues
{
    public class CallerInfo
    {
        public string FilePath { get; }

        public int LineNumber { get; }

        public int ColumnNumber { get; }

        public CallerInfo(string filePath, int lineNumber, int columnNumber)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public string GetTracePrefix()
        {
            return $"{Path.GetFileName(FilePath)}({LineNumber}, {ColumnNumber}): ";
        }

        public static CallerInfo GetCurrent()
        {
            // TODO: rewrite
#if DEBUG
            // no need this in debug at the moment. Not unit-tested, but consumes time while running UT
            return new CallerInfo(null, 0, 0);
#else
            var stack = new StackTrace(true);
            var frames = stack.GetFrames();
            var currentAssembly = Assembly.GetExecutingAssembly();
            var frame = frames.FirstOrDefault(s => s.GetMethod()?.DeclaringType.Assembly != currentAssembly);
            if (frame != null)
                return new CallerInfo(frame.GetFileName(), frame.GetFileLineNumber(), frame.GetFileColumnNumber());
            else
                return new CallerInfo(null, 0, 0);
#endif
        }
    }
}
