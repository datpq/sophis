using System;

using System.Xml.Linq;
using sophis.connector.configuration;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using sophis.services.xmlns_dataExchange_soa_sophis_net;
using sophis.services.xmlns_authentication;
using sophis.services.xmlns_soaDesigner_sophis_net;

namespace SophisETL.ISEngine
{
    public class IntegrationServiceEngine
    {
        #region Singleton
        // Singleton
        private static IntegrationServiceEngine _Instance;
        
        public static IntegrationServiceEngine Instance { get { return _Instance ?? (_Instance = new IntegrationServiceEngine()); } }
        private IntegrationServiceEngine()
        {
            InitializeDataExchangeConnexion();
            InitializeSOAMethodDesignerConnexion();
        }
        #endregion

        #region Parameters
        // Parameters
        private String __configurationFile;
        public String _configurationFile
        {
            get
            {
                if (__configurationFile == null)
                    throw new Exception("Integration Service Engine has not been set-up: are you missing the engine definition in the chain XML?");
                return __configurationFile;
            }
            set { __configurationFile = value; }
        }
        //private readonly string xmlConfig;
        #endregion
        
        public static sophis.services.DataExchangeServiceClient _ClientData = null;
        public static sophis.services.SoaMethodDesignerServiceClient _SOAClientData = null;

        public XDocument Import(string xmlMessage)
        {
            ProcessEntitiesRaw oneEntity = new ProcessEntitiesRaw()
            {
                message = xmlMessage,
                token = fTokenData
            };

            ProcessEntitiesRawResponse resp = _ClientData.processEntitiesRaw(oneEntity);
            XDocument resultXdoc = null;
            if (resp.response != null)
            {
                resultXdoc = XDocument.Parse(resp.response);
            }
            return resultXdoc;
        }

        public XDocument SOAImport(string xmlMessage)
        {
         var oneRaw = new ProcessRaw()
            {
                message = xmlMessage,
                token = fSOATokenData
            };

            ProcessRawResponse resp = _SOAClientData.processRaw(oneRaw);
            XDocument resultXdoc = null;
            if (resp != null)
            {
                resultXdoc = XDocument.Parse(resp.response);
            }
            return resultXdoc;
        }



        private Token fTokenData;
        private Token fSOATokenData;


        private BasicHttpBinding GetBasicBinding(System.ServiceModel.EndpointAddress adress, int timeOut)
        {
            BasicHttpBinding basicBinding = new BasicHttpBinding();
            basicBinding.MaxBufferSize = 65536000;
            basicBinding.MaxReceivedMessageSize = 65536000;
            basicBinding.MaxBufferPoolSize = 655360000;
            //This Setting need "System.Runtime.Serialization.dll"
            basicBinding.ReaderQuotas.MaxArrayLength = 65536000;
            basicBinding.ReaderQuotas.MaxNameTableCharCount = 65536000;
            basicBinding.ReaderQuotas.MaxStringContentLength = 655360000;

            basicBinding.SendTimeout = new TimeSpan(0, 0, timeOut, 0);
            basicBinding.ReceiveTimeout = new TimeSpan(0, 0, timeOut, 0);
            if (adress.Uri.AbsoluteUri.Contains("https"))
                basicBinding.Security.Mode = BasicHttpSecurityMode.Transport;//TransportWithMessageCredential;
            else
                basicBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;

            basicBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            basicBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.Basic;
            basicBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            return basicBinding;
        }

        private void InitializeDataExchangeConnexion()
        {
            if (_ClientData != null)
            {
                return;
            }

            System.Configuration.Configuration exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            UniversalAdapterGroup configurationSectionGroup = exeConfiguration.GetSectionGroup("UniversalAdapter") as UniversalAdapterGroup;
           
            string login = configurationSectionGroup.SoaConnection.SoaServers[0].ServerLogin;
            string password = configurationSectionGroup.SoaConnection.SoaServers[0].ServerPassword;
            string uri = configurationSectionGroup.SoaConnection.SoaServers[0].DataExchangeUri;
            int timeOut = configurationSectionGroup.SoaConnection.SoaServers[0].DataExchangeTimeout;
            // disable certificate checks
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                });

            System.ServiceModel.EndpointAddress adressDataExchange = new System.ServiceModel.EndpointAddress(uri);

            BasicHttpBinding binding = GetBasicBinding(adressDataExchange, timeOut);
            binding.AllowCookies = true;

            _ClientData = new sophis.services.DataExchangeServiceClient(binding, adressDataExchange);

            _ClientData.ClientCredentials.UserName.UserName = login;
            _ClientData.ClientCredentials.UserName.Password = password;

            var login1 = new sophis.services.xmlns_dataExchange_soa_sophis_net.Login()
            {
                user = login,
                password = password
            };
            var resp = _ClientData.login(login1);

            fTokenData = resp.token;
        }
        private void InitializeSOAMethodDesignerConnexion()
        {
            if (_SOAClientData != null)
            {
                return;
            }

            System.Configuration.Configuration exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            UniversalAdapterGroup configurationSectionGroup = exeConfiguration.GetSectionGroup("UniversalAdapter") as UniversalAdapterGroup;

            string login = configurationSectionGroup.SoaConnection.SoaServers[0].ServerLogin;
            string password = configurationSectionGroup.SoaConnection.SoaServers[0].ServerPassword;
            string uri = configurationSectionGroup.SoaConnection.SoaServers[0].MethodDesignerUri;
            int timeOut = configurationSectionGroup.SoaConnection.SoaServers[0].MethodDesignerTimeout;
            // disable certificate checks
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                });

            System.ServiceModel.EndpointAddress adressDataExchange = new System.ServiceModel.EndpointAddress(uri);

            BasicHttpBinding binding = GetBasicBinding(adressDataExchange, timeOut);
            binding.AllowCookies = true;

            _SOAClientData = new sophis.services.SoaMethodDesignerServiceClient(binding, adressDataExchange);

            _SOAClientData.ClientCredentials.UserName.UserName = login;
            _SOAClientData.ClientCredentials.UserName.Password = password;

            var login1 = new sophis.services.xmlns_dataExchange_soa_sophis_net.Login()
            {
                user = login,
                password = password
            };
            var resp = _ClientData.login(login1);

            fSOATokenData = resp.token;
        }
        
        
    }
}
