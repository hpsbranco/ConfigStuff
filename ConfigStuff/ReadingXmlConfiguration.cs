using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions.Execution;
using Xunit;

namespace ConfigStuff
{
    public class ReadingXmlConfiguration
    {
        private readonly string configFile =
            Assembly.GetExecutingAssembly().Location + ".config";

        private const string expected =
            @"Server=(LocalDb)\MSSQLLocalDB;Database=SomeDb;Integrated Security=true";

        [Fact]
        public void UsingXmlDocument()
        {
            // using System.Xml;
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(this.configFile);
            var root = xmlDocument.DocumentElement;

            var query = "//connectionStrings/add[@name='default']/@connectionString";
            var connectionString = root.SelectSingleNode(query).Value;

            connectionString.Should().Be(expected);
        }

        [Fact]
        public void UsingLinqToXml()
        {
            // using System.Xml.Linq
            var document = XDocument.Load(this.configFile);

            var connectionString =
                (from cs in document.Descendants("connectionStrings")
                 from ds in cs.Descendants()
                 where (string) ds.Attribute("name") == "default"
                 select (string) ds.Attribute("connectionString")).First();

            connectionString.Should().Be(expected);
        }

        [Fact]
        public void UsingConfigurationBuilder()
        {
            // Install-Package Microsoft.Extensions.Configuration.Xml
            // using Microsoft.Extensions.Configuration
            var configuration = new ConfigurationBuilder()
                .AddXmlFile(this.configFile)
                .Build();

            var key = "add:default:connectionString";
            var connectionStrings = new HashSet<string>
            {
                configuration[$"connectionStrings:{key}"],
                configuration.GetSection("connectionStrings")[key],
                configuration.GetConnectionString(key)
            };

            connectionStrings.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void UsingConfigurationBuilderAndCustomProvider()
        {
            // using Microsoft.Extensions.Configuration
            var configuration = new ConfigurationBuilder()
                .Add(new CustomXmlConfigurationProvider())
                .Build();

            var connectionStrings = new HashSet<string>
            {
                configuration["connectionStrings:default"],
                configuration.GetSection("connectionStrings")["default"],
                configuration.GetConnectionString("default")
            };

            var appSetting = configuration["someSetting"];

            using (new AssertionScope())
            {
                connectionStrings.Should().BeEquivalentTo(expected);
                appSetting.Should().Be("someValue");
            }
        }
    }
}
