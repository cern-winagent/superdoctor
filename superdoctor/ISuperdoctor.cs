using plugin;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace superdoctor
{
    [PluginAttribute(PluginName = "Superdoctor")]
    public class ISuperDoctor : IInputPlugin
    {
        public string Execute(JObject set)
        {
            var settings = set.ToObject<Settings.SuperDoctor>();

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.Path,
                    Arguments = "-e",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            int cnt = 0; // 0: beginning of program
                         // 1: Monitored Item
                         // 2: first line of '-----------'
                         // 3: second line of '------------' (date in the end)
            int itemPos = 0; int itemLength = 0;
            int highLimitPos = 0; int highLimitLength = 0;
            int lowLimitPos = 0; int lowLimitLength = 0;
            int valuePos = 0; int valueLength = 0;
            int ps = 0;
            var devices = new List<Models.Device>();
            var PSUs = new List<Models.PSU>();
            DateTime time = DateTime.Now;
            string output = "";
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                output += line;
                //Console.WriteLine($"{cnt}, {line}");
                if (line.Contains("Monitored Item"))
                {
                    cnt++;
                    string pattern = @"Monitored Item(\s+)";
                    Match match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        itemPos = match.Index;
                        itemLength = match.Length;
                    }
                    pattern = @"High Limit";
                    match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        highLimitPos = match.Index;
                        highLimitLength = match.Length;
                    }
                    pattern = @"Low Limit";
                    match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        lowLimitPos = match.Index;
                        lowLimitLength = match.Length;
                    }
                    pattern = @"(\s+)Reading";
                    match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        valuePos = match.Index;
                        valueLength = match.Length;
                    }
                }
                else if (line.Contains("-----------") && cnt > 2)
                {
                    string pattern = @"(-)+(\s+)";
                    Match match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        var date = line.Substring(match.Index + match.Length).Trim();
                        time = DateTime.ParseExact(date, "ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture);
                    }
                    cnt++;
                }
                else if (line.Contains("-------------") || (cnt > 2 && line.Trim() == ""))
                {
                    cnt++;
                    ps = 0;
                }

                if (cnt == 3)
                {
                    string valueWithUnit = line.Substring(valuePos, valueLength).Trim();
                    var valueAndUnit = valueWithUnit.Split(' ');
                    if (valueAndUnit.Length < 2) continue;
                    var highLimit = line.Substring(highLimitPos, highLimitLength).Trim() == "" ? (float?)null : float.Parse(line.Substring(highLimitPos, highLimitLength).Trim().Split(' ')[0]);
                    var lowLimit = line.Substring(lowLimitPos, lowLimitLength).Trim() == "" ? (float?)null : float.Parse(line.Substring(lowLimitPos, lowLimitLength).Trim().Split(' ')[0]);
                    //Console.WriteLine($"Device: {line.Substring(itemPos,itemLength)}\t {line.Substring(lowLimitPos, lowLimitLength)} --> {line.Substring(valuePos, valueLength)} <-- {line.Substring(highLimitPos, highLimitLength)}");
                    devices.Add(new Models.Device()
                    {
                        Name = line.Substring(itemPos, itemLength).Trim(),
                        HighLimit = highLimit,
                        LowLimit = lowLimit,
                        Value = float.Parse(valueAndUnit[0]),
                        Unit = valueAndUnit[1]
                    });
                }
                else if (cnt == 2 || (cnt >= 4 && cnt % 2 == 0))
                {
                    cnt++;
                }
                else if (cnt >= 7 && cnt % 2 == 1)
                {
                    if (ps == 0)
                    {
                        //Console.WriteLine($"PS Name: {line.Substring(itemPos, itemLength)}");
                        PSUs.Add(new Models.PSU()
                        {
                            Name = line.Substring(itemPos, itemLength).Trim(),
                            Devices = new List<Models.Device>()
                        });
                        ps++;
                    }
                    else
                    {
                        //Console.WriteLine($"\t{line.Substring(itemPos, itemLength)}: {line.Substring(valuePos, valueLength)}");
                        var lastPSUDevices = PSUs.Last().Devices;
                        string valueWithUnit = line.Substring(valuePos, valueLength).Trim();
                        var valueAndUnit = valueWithUnit.Split(' ');
                        if (valueAndUnit.Length < 2) continue;
                        lastPSUDevices.Add(new Models.Device()
                        {
                            Name = line.Substring(itemPos, itemLength).Trim(),
                            Value = float.Parse(valueAndUnit[0]),
                            Unit = valueAndUnit[1]
                        });
                    }
                }
            }
            var superdoctor = new Models.SuperdoctorData()
            {
                Devices = devices,
                PSUs = PSUs,
                Date = time.ToUniversalTime(),
                HostName = settings.HostName,
                UnprocessedOutput = output
            };
            return JsonConvert.SerializeObject(superdoctor);
        }
    }
}
