using System.IO;
using System.Text;
using System.Text.Json;

namespace FileWork
{
    public class FileManager
    {
        public static ConfigurationFile ReadConfigurationFile()
        {
            ConfigurationFile configurationFile = null;
            using (var file = new StreamReader(FileManagerSettings.ConfigFilePath))
            {
                string jsonString = file.ReadToEnd();
                configurationFile = JsonSerializer.Deserialize<ConfigurationFile>(jsonString);
            }
            return configurationFile;
        }

        public static void WriteConfigurationFile(ConfigurationFile configFile)
        {
            using (var file = new StreamWriter(FileManagerSettings.ConfigFilePath))
            {
                file.WriteLine(JsonSerializer.Serialize(configFile));
            }
        }

        public static void CreateConfigurationFile()
        {
            ConfigurationFile configurationFile = new ConfigurationFile();
            WriteConfigurationFile(configurationFile);
        }

    }
}
