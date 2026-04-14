using System;
using System.IO;
using System.Text;

public class ApiTraceLogger
{
    private readonly string _file;

    public ApiTraceLogger(string apiName)
    {
        string dir = @"C:\UIDAI_DEBUG";
        Directory.CreateDirectory(dir);

        string correlationId = Guid.NewGuid().ToString();

        _file = Path.Combine(dir, $"{apiName}_{correlationId}.txt");

        Write("==================================================");
        Write($"{apiName} EXECUTION TRACE");
        Write($"CorrelationId : {correlationId}");
        Write($"Timestamp     : {DateTime.UtcNow:O}");
        Write("==================================================");
    }

    public void Step(string title)
    {
        Write("");
        Write("--------------------------------------------------");
        Write(title);
        Write("--------------------------------------------------");
    }

    public void Data(string name, string value)
    {
        Write($"{name} :");
        Write(value);
        Write("");
    }

    public void Write(string text)
    {
        File.AppendAllText(_file, text + Environment.NewLine);
    }

    public void End()
    {
        Write("==================================================");
        Write("END OF TRACE");
        Write("==================================================");
    }
}