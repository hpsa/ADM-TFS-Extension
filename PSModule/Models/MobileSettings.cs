using System;

namespace PSModule.Models
{
    public class MobileSettings
    {
        public bool useMC;
        public String mcServerUrl;// = map.get(RunFromFileSystemTaskConfigurator.MCSERVERURL);
        public String mcUserName;// = map.get(RunFromFileSystemTaskConfigurator.MCUSERNAME);
        public String mcPassword;// = map.get(RunFromFileSystemTaskConfigurator.MCPASSWORD);
        public String proxyAddress;// = null;
        public String proxyUserName;// = null;
        public String proxyPassword;// = null;

        public bool useSSL;
        public bool useProxy;

        public bool specifyAuthentication;
    }
}
