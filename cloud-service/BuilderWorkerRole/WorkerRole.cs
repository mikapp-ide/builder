using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace BuilderWorkerRole {
	public class WorkerRole : RoleEntryPoint {
		public override void Run() {

			using (var host = new ServiceHost(typeof(BuildService))) {

				string ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["tcpinput"].IPEndpoint.Address.ToString();
				int tcpport = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["tcpinput"].IPEndpoint.Port;
				int mexport = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["mexinput"].IPEndpoint.Port;

				// Add a metadatabehavior for client proxy generation
				// The metadata is exposed via net.tcp
				var metadatabehavior = new ServiceMetadataBehavior();
				host.Description.Behaviors.Add(metadatabehavior);
				Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
				string mexlistenurl = string.Format("net.tcp://{0}:{1}/BuilderServiceMetaDataEndpoint", ip, mexport);
				string mexendpointurl = string.Format("net.tcp://{0}:{1}/BuilderServiceMetaDataEndpoint", RoleEnvironment.GetConfigurationSettingValue("Domain"), 8001);
				host.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, mexendpointurl, new Uri(mexlistenurl));

				// Add the endpoint for MyService
				string listenurl = string.Format("net.tcp://{0}:{1}/BuilderServiceEndpoint", ip, tcpport);
				string endpointurl = string.Format("net.tcp://{0}:{1}/BuilderServiceEndpoint", RoleEnvironment.GetConfigurationSettingValue("Domain"), 9001);
				host.AddServiceEndpoint(typeof(IBuildService), new NetTcpBinding(SecurityMode.None), endpointurl, new Uri(listenurl));
				host.Open();

				while (true) {
					Thread.Sleep(1800000);
					//Trace.WriteLine("Working", "Information");
				}
			}
		}

		public override bool OnStart() {
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			// For information on handling configuration changes
			// see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

			return base.OnStart();
		}
	}
}
