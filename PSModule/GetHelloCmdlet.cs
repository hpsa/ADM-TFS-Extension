using System.Management.Automation;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace PSModule
{
    [Cmdlet(VerbsCommon.Invoke, "FSTask")]
    public class GetHelloCmdlet : AbstractLauncherTask
    {
        //protected override void BeginProcessing()
        //{
        //    base.BeginProcessing();
        //    Console.WriteLine("BeginProcessing");
        //}
        [Parameter(Position = 0, Mandatory = true)]
        public string TestsPath;

        [Parameter(Position = 1, Mandatory = false)]
        public string Timeout;

        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new LauncherParamsBuilder();

            builder.SetRunType(RunType.FileSystem);
            builder.SetPerScenarioTimeOut(Timeout);
            /*
                        boolean useMC = BooleanUtils.toBoolean(map.get(RunFromFileSystemTaskConfigurator.USE_MC_SETTINGS));
                        if (useMC)
                        {
                            String mcServerUrl = map.get(RunFromFileSystemTaskConfigurator.MCSERVERURL);
                            String mcUserName = map.get(RunFromFileSystemTaskConfigurator.MCUSERNAME);
                            String mcPassword = map.get(RunFromFileSystemTaskConfigurator.MCPASSWORD);
                            String proxyAddress = null;
                            String proxyUserName = null;
                            String proxyPassword = null;

                            boolean useSSL = BooleanUtils.toBoolean(map.get(RunFromFileSystemTaskConfigurator.USE_SSL));
                            builder.setMobileUseSSL(useSSL ? 1 : 0);

                            if (useSSL)
                            {
                                buildLogger.addBuildLogEntry("********** Use SSL ********** ");
                            }

                            boolean useProxy = BooleanUtils.toBoolean(map.get(RunFromFileSystemTaskConfigurator.USE_PROXY));

                            builder.setMobileUseProxy(useProxy ? 1 : 0);

                            if (useProxy)
                            {

                                buildLogger.addBuildLogEntry("********** Use Proxy ********** ");

                                builder.setMobileProxyType(2);

                                proxyAddress = map.get(RunFromFileSystemTaskConfigurator.PROXY_ADDRESS);

                                //proxy info
                                if (proxyAddress != null)
                                {

                                    builder.setMobileProxySetting_Address(proxyAddress);

                                }

                                Boolean specifyAuthentication = BooleanUtils.toBoolean(RunFromFileSystemTaskConfigurator.SPECIFY_AUTHENTICATION);

                                builder.setMobileProxySetting_Authentication(specifyAuthentication ? 1 : 0);

                                if (specifyAuthentication)
                                {
                                    proxyUserName = map.get(RunFromFileSystemTaskConfigurator.PROXY_USERNAME);
                                    proxyPassword = map.get(RunFromFileSystemTaskConfigurator.PROXY_PASSWORD);

                                    if (proxyUserName != null && proxyPassword != null)
                                    {
                                        builder.setMobileProxySetting_UserName(proxyUserName);
                                        builder.setMobileProxySetting_Password(proxyPassword);
                                    }

                                }
                            }
                            else
                            {
                                builder.setMobileProxyType(0);
                            }


                            if (!mcInfoCheck(mcServerUrl, mcUserName, mcPassword))
                            {
                                //url name password
                                builder.setServerUrl(mcServerUrl);
                                builder.setUserName(mcUserName);
                                builder.setFileSystemPassword(mcPassword);

                                String jobUUID = map.get(RunFromFileSystemTaskConfigurator.JOB_UUID);

                                //write the specified job info(json type) to properties
                                JobOperation operation = new JobOperation(mcServerUrl, mcUserName, mcPassword, proxyAddress, proxyUserName, proxyPassword);

                                String mobileInfo = null;
                                JSONObject jobJSON = null;
                                JSONObject dataJSON = null;
                                JSONArray extArr = null;
                                JSONObject applicationJSONObject = null;

                                if (jobUUID != null)
                                {

                                    try
                                    {
                                        jobJSON = operation.getJobById(jobUUID);
                                    }
                                    catch (HttpConnectionException e)
                                    {
                                        buildLogger.addErrorLogEntry("********** Fail to connect mobile center, please check URL, UserName, Password, and Proxy Configuration ********** ");
                                    }

                                    if (jobJSON != null)
                                    {
                                        dataJSON = (JSONObject)jobJSON.get("data");
                                        if (dataJSON != null)
                                        {

                                            applicationJSONObject = (JSONObject)dataJSON.get("application");
                                            if (applicationJSONObject != null)
                                            {
                                                applicationJSONObject.remove(ICON);
                                            }

                                            extArr = (JSONArray)dataJSON.get("extraApps");
                                            if (extArr != null)
                                            {
                                                Iterator<Object> iterator = extArr.iterator();

                                                while (iterator.hasNext())
                                                {
                                                    JSONObject extAppJSONObject = (JSONObject)iterator.next();
                                                    extAppJSONObject.remove(ICON);
                                                }

                                            }
                                        }

                                        mobileInfo = dataJSON.toJSONString();
                                        builder.setMobileInfo(mobileInfo);
                                    }
                                }

                            }


                        }
                        */

           var tests =  TestsPath.Split("\n".ToArray());

            for (int i = 0; i < tests.Length; i++)
            {
                string pathToTest = tests[i].Replace("\\", "\\\\");
                builder.SetTest(i + 1, pathToTest);
            }

            return builder.GetProperties();
        }

        //protected override void ProcessRecord()
        //{
        //    // System.Diagnostics.Debugger.Launch();
        //    // System.Diagnostics.Debugger.Break();
        //    string[] names = { "Mary", "Ferra", "el", "Lussy" };

        //    if (string.IsNullOrEmpty(this.Prefix))
        //    {
        //        WriteObject(names, true);
        //    }
        //    else
        //    {
        //        var prefixed = names.Select(name => this.Prefix + name);
        //        WriteObject(prefixed, true);
        //    }
        //}

        //protected override void StopProcessing()
        //{
        //    base.StopProcessing();
        //    WriteObject("StopProcessing");
        //}

        //protected override void EndProcessing()
        //{
        //    base.EndProcessing();
        //    WriteWarning("EndProcessing");
        //}
    }
}
