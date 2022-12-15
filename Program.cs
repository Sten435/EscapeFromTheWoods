using MongoDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EscapeFromTheWoods {
	class Program {
		static void Main(string[] args) {

			List<Task> tasks = new List<Task>();

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=monkeys;Integrated Security=True";
			connectionString = "mongodb://localhost:27017";
			//DBwriter db = new DBwriter(connectionString)
			repo db = new repo(connectionString);

			string path = @"C:\Users\stanp\Desktop\monkeys";
			Map m1 = new Map(0, 500, 0, 500);
			Wood w1 = WoodBuilder.GetWood(500, m1, path, db);
			w1.PlaceMonkey("Alice", IDgenerator.GetMonkeyID());
			w1.PlaceMonkey("Janice", IDgenerator.GetMonkeyID());
			w1.PlaceMonkey("Toby", IDgenerator.GetMonkeyID());
			w1.PlaceMonkey("Mindy", IDgenerator.GetMonkeyID());
			w1.PlaceMonkey("Jos", IDgenerator.GetMonkeyID());

			Map m2 = new Map(0, 200, 0, 400);
			Wood w2 = WoodBuilder.GetWood(2500, m2, path, db);
			w2.PlaceMonkey("Tom", IDgenerator.GetMonkeyID());
			w2.PlaceMonkey("Jerry", IDgenerator.GetMonkeyID());
			w2.PlaceMonkey("Tiffany", IDgenerator.GetMonkeyID());
			w2.PlaceMonkey("Mozes", IDgenerator.GetMonkeyID());
			w2.PlaceMonkey("Jebus", IDgenerator.GetMonkeyID());

			Map m3 = new Map(0, 400, 0, 400);
			Wood w3 = WoodBuilder.GetWood(2000, m3, path, db);
			w3.PlaceMonkey("Kelly", IDgenerator.GetMonkeyID());
			w3.PlaceMonkey("Kenji", IDgenerator.GetMonkeyID());
			w3.PlaceMonkey("Kobe", IDgenerator.GetMonkeyID());
			w3.PlaceMonkey("Kendra", IDgenerator.GetMonkeyID());

			tasks.Add(Task.Run(w1.WriteWoodToDB));
			tasks.Add(Task.Run(w2.WriteWoodToDB));
			tasks.Add(Task.Run(w3.WriteWoodToDB));
			tasks.Add(Task.Run(w1.Escape));
			tasks.Add(Task.Run(w2.Escape));
			tasks.Add(Task.Run(w3.Escape));

			Task.WaitAll(tasks.ToArray());

			stopwatch.Stop();
			// Write result.
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
			Console.WriteLine("end");
		}
	}
}
