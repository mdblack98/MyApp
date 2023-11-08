using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace HamlibGUI
{
#pragma warning disable IDE1006 // Naming Styles
    public class DataParser
    {
        public static RootObject? ParseMulticastDataPacket(string jsonData)
        {
            try
            {
                var myJson = JsonSerializer.Deserialize<RootObject>(jsonData);
                return myJson;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"An error occurred while parsing the JSON data: {ex.Message}");
                return null;
            }
        }
    }

    public class Id
    {
        public string? model { get; set; }
        public string? endpoint { get; set; }
        public string? process { get; set; }
        public string? deviceId { get; set; }
    }
    public class Rig
    {
        public string? status { get; set; }
        public string? errorMsg { get; set; }
        public string? name { get; set; }
        public bool split { get; set; }
        public string? splitVfo { get; set; }
        public bool SatMode { get; set; }
        public Id? id { get; set; }
        public string[]? modes { get; set; }
    }


    public class VFO
    {
        public string? name { get; set; }
        public double freq { get; set; }
        public string? mode { get; set; }
        public int width { get; set; }
        public bool ptt { get; set; }
        public bool rx { get; set; }
        public bool tx { get; set; }
    }

    public class RootObject
    {
        public string? app { get; set; }
        public string? version { get; set; }
        public int seq { get; set; }
        public int crc { get; set; }
        public Rig? rig { get; set; }
        public List<VFO>? vfos { get; set; }
        public required string time { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
