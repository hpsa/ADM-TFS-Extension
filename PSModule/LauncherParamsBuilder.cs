using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
        private string secretkey = "EncriptionPass4Java";
        private readonly List<string> requiredParameters = new List<string> { "almRunHost", "almPassword" };
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
                encAlmPass = EncryptParameter(almPassword);
                SetParamValue("almPassword", encAlmPass);
            }
            catch
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
            string paramToSet = "-1";
            if (!string.IsNullOrEmpty(almTimeout))
            {
                paramToSet = almTimeout;
            }
            SetParamValue("almTimeout", paramToSet);
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


        public void SetFileSystemPassword(string oriPass)
        {
            string encPass;
            try
            {
                encPass = EncryptParameter(oriPass);
                SetParamValue("MobilePassword", encPass);
            }
            catch
            {

            }
        }

        public void SetPerScenarioTimeOut(string perScenarioTimeOut)
        {
            string paramToSet = "-1";
            if (!string.IsNullOrEmpty(perScenarioTimeOut))
            {
                paramToSet = perScenarioTimeOut;
            }
            SetParamValue("PerScenarioTimeOut", paramToSet);
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
            string proxyPass;
            try
            {
                proxyPass = EncryptParameter(proxyPassword);
                SetParamValue("MobileProxySetting_Password", proxyPass);
            }
            catch
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

        private string EncryptParameter(string parameter)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] pwdBytes = Encoding.UTF8.GetBytes(secretkey);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(parameter);
            return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }

    }
}
