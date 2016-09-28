using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSModule
{
    /*
 * 	        runType=<Alm/FileSystem/LoadRunner>
	        almServerUrl=http://<server>:<port>/qcbin
	        almUserName=<user>
	        almPassword=<password>
	        almDomain=<domain>
	        almProject=<project>
	        almRunMode=<RUN_LOCAL/RUN_REMOTE/RUN_PLANNED_HOST>
	        almTimeout=<-1>/<numberOfSeconds>
	        almRunHost=<hostname>
	        TestSet<number starting at 1>=<testSet>/<AlmFolder>
	        Test<number starting at 1>=<testFolderPath>/<a Path ContainingTestFolders>/<mtbFilePath>

 */

    public class LauncherParamsBuilder
    {
        private readonly List<string> requiredParameters = new List<string> {"almRunHost" };
        private Dictionary<string, string> properties = new Dictionary<string, string>();

        public Dictionary<string, string> GetProperties()
        {
            return properties;
        }

        public void SetRunType(RunType runType)
        {
            SetParamValue("runType", runType.ToString());
        }

        public void SetAlmServerUrl(string almServerUrl)
        {
            SetParamValue("almServerUrl", almServerUrl);
        }

        public void SetAlmUserName(string almUserName)
        {
            SetParamValue("almUserName", almUserName);
        }

        public void SetAlmPassword(string almPassword)
        {
            string encAlmPass;
            try
            {

                //encAlmPass = EncryptionUtils.Encrypt(
                //                almPassword,
                //                EncryptionUtils.getSecretKey());

                //properties.Add("almPassword", encAlmPass);

            }
            catch (Exception e)
            {

            }
        }

        public void SetAlmDomain(string almDomain)
        {
            SetParamValue("almDomain", almDomain);
        }

        public void SetAlmProject(string almProject)
        {
            SetParamValue("almProject", almProject);
        }

        public void SetAlmRunMode(AlmRunMode almRunMode)
        {
            properties.Add("almRunMode", almRunMode != AlmRunMode.RUN_NONE ? almRunMode.ToString() : "");
        }

        public void SetAlmTimeout(string almTimeout)
        {
            SetParamValue("almTimeout", almTimeout);
        }


        public void SetTestSet(int index, string testSet)
        {
            SetParamValue("TestSet" + index, testSet);
        }

        public void SetAlmTestSet(string testSets)
        {
            SetParamValue("almTestSets", testSets);
        }

        public void SetAlmRunHost(string host)
        {
            SetParamValue("almRunHost", host);
        }

        public void SetTest(int index, string test)
        {
            SetParamValue("Test" + index, test);
        }


        public void SetFileSystemPassword(String oriPass)
        {
            String encPass;
            try
            {

                //encPass =
                //        EncryptionUtils.Encrypt(
                //                oriPass,
                //                EncryptionUtils.getSecretKey());

                //properties.Add("MobilePassword", encPass);

            }
            catch (Exception e)
            {

            }
        }

        public void SetPerScenarioTimeOut(string perScenarioTimeOut)
        {
            SetParamValue("PerScenarioTimeOut", perScenarioTimeOut);
        }

        public void setServerUrl(String serverUrl)
        {
            SetParamValue("MobileHostAddress", serverUrl);
        }

        public void setUserName(String username)
        {
            SetParamValue("MobileUserName", username);
        }

        public void setProxyHost(String proxyHost)
        {
            SetParamValue("proxyHost", proxyHost);
        }

        public void setProxyPort(String proxyPort)
        {
            SetParamValue("proxyPort", proxyPort);
        }

        #region mobile
        public void SetMobileInfo(String mobileInfo)
        {
            SetParamValue("mobileinfo", mobileInfo);
        }

        public void setMobileUseSSL(int type)
        {
            SetParamValue("MobileUseSSL", type.ToString());
        }

        public void setMobileUseProxy(int proxy)
        {
            SetParamValue("MobileUseProxy", proxy.ToString());
        }

        public void setMobileProxyType(int type)
        {
            SetParamValue("MobileProxyType", type.ToString());
        }

        public void setMobileProxySetting_Address(String proxyAddress)
        {
            SetParamValue("MobileProxySetting_Address", proxyAddress);
        }

        public void SetMobileProxySetting_Authentication(int authentication)
        {
            SetParamValue("MobileProxySetting_Authentication", authentication.ToString());
        }

        public void SetMobileProxySetting_UserName(string proxyUserName)
        {
            SetParamValue("MobileProxySetting_UserName", proxyUserName);
        }

        public void SetMobileProxySetting_Password(string proxyPassword)
        {
            String proxyPass;
            try
            {

                //proxyPass =
                //        EncryptionUtils.Encrypt(
                //                proxyPassword,
                //                EncryptionUtils.getSecretKey());

                //properties.Add("MobileProxySetting_Password", proxyPass);

            }
            catch (Exception e)
            {

            }
        }
        #endregion

        private void SetParamValue(string paramName, string paramValue)
        {

            if (string.IsNullOrEmpty(paramValue))
            {
                if (!requiredParameters.Contains(paramName))
                    properties.Remove(paramName);
                else
                    properties.Add(paramName, "");
            }
            else
            {
                properties.Add(paramName, paramValue);
            }
        }

    }
}
