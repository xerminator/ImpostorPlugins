using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MatchLog
{
    public class Match
    {

        public int MatchID { get; set; }
        public string gameStarted { get; set; }
        public string players { get; set; }
        public string impostors { get; set; }
        public string eventsLogFile { get; set; }
        public string result { get; set; }
        public string reason { get; set; }

        public void Process()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string directoryPath = Path.Combine(workingDirectory, "plugins", "Matchlog");
            //string pattern = "*.json";
            string match_pattern = "*_match.json";

            //string[] all_files = Directory.GetFiles(directoryPath, pattern).Union(Directory.GetFiles(directoryPath, match_pattern)).ToArray();
            string[] match_files = Directory.GetFiles(directoryPath, match_pattern);

            string output_file = "matches.txt";
            string error_file = "errors.txt";
            StringBuilder output = new StringBuilder();
            try
            {
                foreach (string file in match_files)
                {

                    string jsonString = File.ReadAllText(file);
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
                    string minified = MinifyJson(jsonDocument.RootElement);

                    output.AppendLine(minified);

                }

                File.WriteAllText(output_file, output.ToString());

                PythonScriptExecutor pythonExecutor = new PythonScriptExecutor();
                string pythonScriptPath = Path.Combine(Environment.CurrentDirectory + "elo.py");
                string arguments = "";
                pythonExecutor.ExecutePythonScript(pythonScriptPath, arguments);

            }
            catch (Exception ex)
            {
                File.WriteAllText(error_file, ex.Message);
            }






        }

        public string MinifyJson(JsonElement jsonElement)
        {
            StringBuilder builder = new StringBuilder();
            WriteJsonElement(builder, jsonElement);
            return builder.ToString();
        }

        public void WriteJsonElement(StringBuilder builder, JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                    builder.Append('{');
                    bool first = true;
                    foreach (var property in jsonElement.EnumerateObject())
                    {
                        if (!first)
                            builder.Append(',');
                        builder.Append('"').Append(property.Name).Append('"').Append(':');
                        WriteJsonElement(builder, property.Value);
                        first = false;
                    }
                    builder.Append('}');
                    break;
                case JsonValueKind.Array:
                    builder.Append('[');
                    first = true;
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        if (!first)
                            builder.Append(',');
                        WriteJsonElement(builder, item);
                        first = false;
                    }
                    builder.Append(']');
                    break;
                case JsonValueKind.String:
                    builder.Append('"').Append(jsonElement.GetString()).Append('"');
                    break;
                case JsonValueKind.Number:
                    builder.Append(jsonElement.ToString());
                    break;
                case JsonValueKind.True:
                    builder.Append("true");
                    break;
                case JsonValueKind.False:
                    builder.Append("false");
                    break;
                case JsonValueKind.Null:
                    builder.Append("null");
                    break;
                default:
                    throw new InvalidOperationException("Invalid JSON value kind.");
            }
        }

    }
}