using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace BuilderWorkerRole.Models {
	public class ProjectInfo {
		[BsonId(IdGenerator = typeof(CombGuidGenerator))]
		public string Guid { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("user_id")]
		public string UserId { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }

		[BsonElement("display_name")]
		public string DisplayName { get; set; }
	}
}
