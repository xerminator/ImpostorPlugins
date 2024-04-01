using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchLog
{
    public class PythonScriptExecutor
    {
        public void ExecutePythonScript(string pythonScriptPath, string arguments)
        {
            string pythonInterpreter = "python3"; // Path to your Python interpreter

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pythonInterpreter,
                Arguments = $"{pythonScriptPath} {arguments}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
            }
        }
    }
}
