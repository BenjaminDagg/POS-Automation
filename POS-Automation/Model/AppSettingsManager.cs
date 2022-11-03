using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Newtonsoft.Json;

namespace POS_Automation.Model
{
    public class AppSettingsManager
    {
        public static AppSettings Read()
        {
            string filePath = TestData.ConfigFilePath;
            string text = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<AppSettings>(text);
        }

        public static void Write(AppSettings _settings)
        {
            string filePath = TestData.ConfigFilePath;
            var json = JsonConvert.SerializeObject(_settings,Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
