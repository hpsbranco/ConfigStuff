using System.Configuration;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ConfigStuff
{
    // Install-Package System.Configuration.ConfigurationManager
    // using System.Configuration
    public class CustomXmlConfigurationProvider
        : ConfigurationProvider, IConfigurationSource
    {
        public override void Load()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var config = ConfigurationManager.OpenExeConfiguration(exePath);

            foreach (ConnectionStringSettings connString in
                config.ConnectionStrings.ConnectionStrings)
            {
                this.Data.Add($"ConnectionStrings:{connString.Name}",
                    connString.ConnectionString);
            }

            foreach (var key in config.AppSettings.Settings.AllKeys)
            {
                this.Data.Add(key, config.AppSettings.Settings[key].Value);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
