using EscapeFromTheWoods;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB {
	public class repo {
		private IMongoClient mongoClient;
		private IMongoDatabase mongoDatabase;
		private string _connectionString = @"";

		public repo(string connectionString) {
			this._connectionString = connectionString;
			mongoClient = new MongoClient(_connectionString);
			mongoDatabase = mongoClient.GetDatabase("apen");
		}

		public void WriteWoodRecords(List<DBWoodRecord> woodRecords) {
			var collection = mongoDatabase.GetCollection<DBWoodRecord>("WoodRecords");
			collection.InsertMany(woodRecords);
		}

		public void WriteMonkeyRecords(List<DBMonkeyRecord> monkeyRecords) {
			var collection = mongoDatabase.GetCollection<DBMonkeyRecord>("MonkeyRecords");
			collection.InsertMany(monkeyRecords);

			List<Log> logs = monkeyRecords.Select(monkey => new Log(monkey.woodID, monkey.monkeyID, $"{monkey.monkeyName} is now in tree {monkey.treeID} at location ({monkey.x}, {monkey.y})")).ToList();
			var collectionLogs = mongoDatabase.GetCollection<dynamic>("Logs");
			collectionLogs.InsertMany(logs);
		}
	}
}
