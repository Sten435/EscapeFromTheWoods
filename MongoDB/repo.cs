using EscapeFromTheWoods;
using EscapeFromTheWoods.Objects;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoDB
{
    public class Repo {
		private IMongoClient mongoClient;
		private IMongoDatabase mongoDatabase;
		private string _connectionString = @"";

		public Repo(string connectionString) {
			this._connectionString = connectionString;
			mongoClient = new MongoClient(_connectionString);
			mongoDatabase = mongoClient.GetDatabase("apen");
		}

		public async Task WriteWoodRecords(List<DBWoodRecord> woodRecords) {
			var collection = mongoDatabase.GetCollection<DBWoodRecord>("WoodRecords");
			await collection.InsertManyAsync(woodRecords);
		}

		public async Task WriteMonkeyRecords(List<DBMonkeyRecord> monkeyRecords) {
			var collection = mongoDatabase.GetCollection<DBMonkeyRecord>("MonkeyRecords");
			await collection.InsertManyAsync(monkeyRecords);

			List<Log> logs = monkeyRecords.Select(monkey => new Log(monkey.woodID, monkey.monkeyID, $"{monkey.monkeyName} is now in tree {monkey.treeID} at location ({monkey.x}, {monkey.y})")).ToList();
			var collectionLogs = mongoDatabase.GetCollection<dynamic>("Logs");
			await collectionLogs.InsertManyAsync(logs);
		}
	}
}
