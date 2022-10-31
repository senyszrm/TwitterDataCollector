using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SampledStreamServer.Models
{
    public class TwitterConfigXml : ITwitterConfigFile
    {
        // Contains the URL to the sample stream endpoint obtained from the config file
        public string sampleStreamURL { get; private set; } = "";
        // Contains the user's bearer token obtained from the config file
        public string userBearerToken { get; private set; } = "";

        // Reference to the default location of the configuration XML file
        private static string DEFAULT_XML_PATH = String.Format("{0}TwitterConnectionInfo.xml", AppContext.BaseDirectory);

        // Expected XML Elements for the configuration file
        private const string EXPECTED_STREAM_ELEMENT = "sample_stream_url";
        private const string EXPECTED_BEARER_TOKEN_ELEMENT = "bearer_token";

        /// <summary>
        ///  Constructor which pulls required information from the provided configuration file
        /// </summary>
        /// <param name="configXmlPath">Path to the configuration XML file to be used</param>
        public TwitterConfigXml(string configXmlPath = "")
        {
            XmlTextReader twitterConfigXmlReader = new XmlTextReader(configXmlPath == "" ? DEFAULT_XML_PATH : configXmlPath);

            // Read the configuration XML file and look for the elements necessary to establish a Twitter connection
            while (twitterConfigXmlReader.Read())
            {
                twitterConfigXmlReader.MoveToElement();
                if (twitterConfigXmlReader.NodeType == XmlNodeType.Element && twitterConfigXmlReader.Name == EXPECTED_STREAM_ELEMENT)
                {
                    sampleStreamURL = twitterConfigXmlReader.ReadElementContentAsString().Trim();
                }
                else if (twitterConfigXmlReader.NodeType == XmlNodeType.Element && twitterConfigXmlReader.Name == EXPECTED_BEARER_TOKEN_ELEMENT)
                {
                    userBearerToken = twitterConfigXmlReader.ReadElementContentAsString().Trim();
                }

            }
            // If the URL or Bearer Token weren't set, throw an error
            if (sampleStreamURL == "")
            {
                throw new InvalidOperationException("Configuration file did not contain a valid Stream URL.");
            }
            if (userBearerToken == "")
            {
                throw new InvalidOperationException("Configuration file did not contain a valid Bearer Token.");
            }
        }
    }
}
