using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using MongoDB;

namespace EscapeFromTheWoods {
	public class Wood {
		private const int drawingFactor = 8;
		private string path;
		private repo db;
		private Random r = new Random(1);
		public int woodID { get; set; }
		public Dictionary<int, Tree> trees { get; set; }
		public List<Monkey> monkeys { get; private set; }
		private Map map;

		public Wood(int woodID, Dictionary<int, Tree> trees, Map map, string path, repo db) {
			this.woodID = woodID;
			this.trees = trees;
			this.monkeys = new List<Monkey>();
			this.map = map;
			this.path = path;
			this.db = db;
		}
		public void PlaceMonkey(string monkeyName, int monkeyID) {
			int treeNr;
			do {
				treeNr = r.Next(0, trees.Count - 1);
			}
			while (trees[treeNr].hasMonkey);
			Monkey m = new Monkey(monkeyID, monkeyName, trees[treeNr]);
			monkeys.Add(m);
			trees[treeNr].hasMonkey = true;
		}
		public void Escape() {
			List<List<Tree>> routes = new List<List<Tree>>();
			foreach (Monkey monkey in monkeys) {
				routes.Add(EscapeMonkey(monkey));
			}
			WriteEscaperoutesToBitmap(routes);
		}
		private void WriteRouteToDB(Monkey monkey, List<Tree> route) {
			Console.WriteLine($"{woodID}:write db routes {woodID},{monkey.name} start");
			List<DBMonkeyRecord> records = new List<DBMonkeyRecord>();
			for (int j = 0; j < route.Count; j++) {
				records.Add(new DBMonkeyRecord(monkey.monkeyID, monkey.name, woodID, j, route[j].treeID, route[j].x, route[j].y));
			}
			db.WriteMonkeyRecords(records);
			Console.WriteLine($"{woodID}:write db routes {woodID},{monkey.name} end");
		}
		public void WriteEscaperoutesToBitmap(List<List<Tree>> routes) {
			List<Task> tasks = new List<Task>();

			Console.WriteLine($"{woodID}:write bitmap routes {woodID} start");

			Color[] cvalues = new Color[] { Color.DarkRed, Color.Black, Color.DarkBlue, Color.DarkMagenta, Color.DarkViolet };

			var width = (map.xmax - map.xmin) * drawingFactor;
			var height = (map.ymax - map.ymin) * drawingFactor;

			Bitmap bm = new Bitmap(width, height);
			Graphics g = Graphics.FromImage(bm);

			int delta = drawingFactor / 2;

			Brush b = Brushes.DarkGreen;
			Pen p = Pens.DarkGreen;
			g.FillRectangle(Brushes.White, 0, 0, width, height);

			tasks.Add(Task.Run(() => {
				var g2 = Graphics.FromImage(bm);
				foreach (Tree t in trees.Values) {
					g2.DrawEllipse(p, t.x * drawingFactor, t.y * drawingFactor, drawingFactor, drawingFactor);
				}
			}));

			int colorN = 0;
			tasks.Add(Task.Run(() => {
				foreach (List<Tree> route in routes) {

					int p1x = route[0].x * drawingFactor + delta;
					int p1y = route[0].y * drawingFactor + delta;

					Color color = cvalues[colorN % cvalues.Length];
					Pen pen = new Pen(color, 1);

					g.DrawEllipse(pen, p1x - delta, p1y - delta, drawingFactor, drawingFactor);
					g.FillEllipse(new SolidBrush(color), p1x - delta, p1y - delta, drawingFactor, drawingFactor);

					for (int i = 1; i < route.Count; i++) {
						g.DrawLine(pen, p1x, p1y, route[i].x * drawingFactor + delta, route[i].y * drawingFactor + delta);

						p1x = route[i].x * drawingFactor + delta;
						p1y = route[i].y * drawingFactor + delta;
					}
					colorN++;
				}
			}));

			Task.WaitAll(tasks.ToArray());

			var selectedE = ImageFormat.Png;
			bm.Save(Path.Combine(path, woodID.ToString() + "_escapeRoutes." + selectedE.ToString().ToLower()), selectedE);

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"{woodID}:write bitmap routes {woodID} end");
		}
		public IEnumerable<IEnumerable<T>> ChunkBy<T>(IEnumerable<T> source, int chunkSize) {
			while (source.Any()) {
				var chunk = source.Take(chunkSize);
				yield return chunk;
				source = source.Skip(chunkSize);
			}
		}
		public void WriteWoodToDB() {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{woodID}:write db wood {woodID} start");
			List<DBWoodRecord> records = new List<DBWoodRecord>();
			foreach (var tree in trees) {
				records.Add(new DBWoodRecord(woodID, tree.Key, tree.Value.x, tree.Value.y));
			}
			db.WriteWoodRecords(records);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{woodID}:write db wood {woodID} end");
		}
		public List<Tree> EscapeMonkey(Monkey monkey) {
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"{woodID}:start {woodID},{monkey.name}");
			Dictionary<int, bool> visited = new Dictionary<int, bool>();
			List<Tree> route = new List<Tree>() { monkey.tree };

			foreach (var tree in trees) {
				visited.Add(tree.Value.treeID, false);
			}

			int chunckSize = 150;

			if (trees.Count <= 150) {
				while (trees.Count <= chunckSize) {
					chunckSize -= 10;
				}
			}

			var dictionary = ChunkBy(trees.Values.ToList().OrderBy(tree => tree.x).ThenBy(tree => tree.y).ToList(), chunckSize)
				.ToDictionary(tree => (tree.Last().x, tree.Last().y
				));

			do {
				visited[monkey.tree.treeID] = true;
				SortedList<double, List<Tree>> distanceToMonkey = new SortedList<double, List<Tree>>();

				int monkeyX = monkey.tree.x;
				int monkeyY = monkey.tree.y;

				double closestDistance = double.PositiveInfinity;
				List<Tree> chunck = null;

				foreach (var entry in dictionary) {
					double distance = Math.Sqrt(Math.Pow(monkeyX - entry.Key.Item1, 2) + Math.Pow(monkeyY - entry.Key.Item2, 2));
					if (distance < closestDistance) {
						closestDistance = distance;
						chunck = entry.Value.ToList();
					}
				}

				//zoek dichtste boom die nog niet is bezocht
				foreach (Tree tree in chunck) {
					if ((!visited[tree.treeID]) && (!tree.hasMonkey)) {
						double distance = Math.Sqrt(Math.Pow(tree.x - monkey.tree.x, 2) + Math.Pow(tree.y - monkey.tree.y, 2));
						if (distanceToMonkey.ContainsKey(distance)) {
							distanceToMonkey[distance].Add(tree);
						} else {
							distanceToMonkey.Add(distance, new List<Tree>() { tree });
						}
					}
				}


				//distance to border            
				//noord oost zuid west
				double distanceToBorder = (new List<double>(){ map.ymax - monkey.tree.y,
				map.xmax - monkey.tree.x,monkey.tree.y-map.ymin,monkey.tree.x-map.xmin }).Min();
				if (distanceToMonkey.Count == 0) {
					WriteRouteToDB(monkey, route);
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
					return route;
				}
				if (distanceToBorder < distanceToMonkey.First().Key) {
					WriteRouteToDB(monkey, route);
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
					return route;
				}

				route.Add(distanceToMonkey.First().Value.First());
				monkey.tree = distanceToMonkey.First().Value.First();
			}
			while (true);
		}
	}
}
