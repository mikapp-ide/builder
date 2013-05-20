using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Mikapp.BuilderService.Models;
using MongoDB.Driver;

namespace Mikapp.BuilderService {

	[ServiceContract]
	public interface IBuildService {
		[OperationContract]
		Stream Build(string projectId);
	}

	public class BuildService : IBuildService {
		private const string TemplateDir = @".\template";
		private const string TargetDir = @".\projects";

		private ProjectInfo Template { get; set; }

		public BuildService() {
			Template = new ProjectInfo {
				Guid = ConfigurationManager.AppSettings["TemplateProject.Guid"],
				Name = ConfigurationManager.AppSettings["TemplateProject.Name"],
				DisplayName = ConfigurationManager.AppSettings["TemplateProject.DisplayName"],
				Description = ConfigurationManager.AppSettings["TemplateProject.Description"]
			};
		}

		public Stream Build(string projectId) {
			var project = DownloadProjectInfo(projectId);

			var destinationFolder = CopyTemplateProject(project);

			var buildResult = BuildSolution(string.Format(@"{0}\{1}.sln", destinationFolder, project.Name));

			FileStream packageFile = File.OpenRead(
				string.Format(@"{0}\AppPackages\{1}_1.0.0.0_x86_Debug.appxupload", destinationFolder, project.Name)
			);

			return packageFile;
		}

		private string CopyTemplateProject(ProjectInfo project) {
			var destinationProjectDir = string.Format(@"{0}\{1}", TargetDir, project.Name);

			if (!Directory.Exists(TargetDir)) {
				Directory.CreateDirectory(TargetDir);
			}

			// To copy a folder's contents to a new location: 
			// Create a new target folder, if necessary. 
			if (!Directory.Exists(destinationProjectDir)) {
				Directory.CreateDirectory(destinationProjectDir);
			} else {
				var targetDirectory = new DirectoryInfo(destinationProjectDir);

				foreach (FileInfo file in targetDirectory.GetFiles("*", SearchOption.AllDirectories)) {
					file.Delete();
				}
				foreach (DirectoryInfo dir in targetDirectory.GetDirectories("*", SearchOption.AllDirectories)) {
					dir.Delete(true);
				}

				Directory.CreateDirectory(destinationProjectDir);
			}

			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(TemplateDir, "*", SearchOption.AllDirectories)) {
				Directory.CreateDirectory(dirPath.Replace(TemplateDir, destinationProjectDir));
			}

			//Copy all the files
			foreach (string newPath in Directory.GetFiles(TemplateDir, "*.*", SearchOption.AllDirectories)) {
				File.Copy(newPath, newPath.Replace(TemplateDir, destinationProjectDir).Replace(Template.Name, project.Name));
			}

			var projectFileContent = File.ReadAllText(
				string.Format(@"{0}\{1}.jsproj", destinationProjectDir, project.Name)
			).Replace(Template.Name, project.Name).Replace(Template.Guid, project.Guid);

			File.WriteAllText(string.Format(@"{0}\{1}.jsproj", destinationProjectDir, project.Name), projectFileContent);

			projectFileContent = File.ReadAllText(
				string.Format(@"{0}\{1}.sln", destinationProjectDir, project.Name)
			).Replace(Template.Name, project.Name).Replace(Template.Guid, project.Guid);

			File.WriteAllText(string.Format(@"{0}\{1}.sln", destinationProjectDir, project.Name), projectFileContent);

			projectFileContent = File.ReadAllText(
				string.Format(@"{0}\package.appxmanifest", destinationProjectDir)
			)
			.Replace(Template.DisplayName, project.DisplayName)
			.Replace(Template.Description, project.Description)
			.Replace(Template.Name, project.Name)
			.Replace(Template.Guid, project.Guid);

			File.WriteAllText(string.Format(@"{0}\package.appxmanifest", destinationProjectDir), projectFileContent);

			return destinationProjectDir;
		}

		private ProjectInfo DownloadProjectInfo(string projectId) {


			const string connectionString = "mongodb://MongoLab-sr:XbLlj0FWLBDGJ9teRwvYW135.NcUoxPJ2GqZxOjAiiU-@ds041157.mongolab.com:41157/MongoLab-sr";
			//System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_MONGOLAB_URI");

			var mongoClient = new MongoClient(connectionString);

			MongoServer server = mongoClient.GetServer();

			MongoDatabase database = server.GetDatabase("MongoLab-sr");

			return database.GetCollection<ProjectInfo>("projects").FindOneById(projectId);
		}

		private BuildResult BuildSolution(string pathToSolution) {
			var pc = new ProjectCollection();

			var globalProperty = new Dictionary<string, string> {
				{"Configuration", "Debug"},
				{"Platform", "x86"}
			};

			var buidlRequest = new BuildRequestData(pathToSolution, globalProperty, null, new[] { "Build" }, null);

			return BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), buidlRequest);
		}
	}
}
