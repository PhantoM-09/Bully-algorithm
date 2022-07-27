using System;
using System.Collections.Generic;

namespace FileWork
{
    [Serializable]
    public class ConfigurationFile
    {
        public string CoordinatorIp { get; set; }
        public List<string> IPAllServers { get; set; }

        public ConfigurationFile()
        {
            IPAllServers = new List<string>();
        }
    }
}
