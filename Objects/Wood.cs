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
		private Repo db;
		private Random r = new Random(1);
		private Map map;
		private Dictionary<int, HashSet<int>> apenPaden = new();

		public int woodID { get; set; }
		public List<Tree> trees { get; set; }
		public List<Monkey> monkeys { get; private set; }

		public Wood(int woodID, List<Tree> trees, Map map, string path, Repo db) {
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
				treeNr = r.Next(0, trees.Count);
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
		private async void WriteRouteToDB(Monkey monkey, List<Tree> route) {
			Console.WriteLine($"{woodID}:write db routes {woodID},{monkey.name} start");
			List<DBMonkeyRecord> records = new List<DBMonkeyRecord>();
			for (int j = 0; j < route.Count; j++) {
				records.Add(new DBMonkeyRecord(monkey.monkeyID, monkey.name, woodID, j, route[j].treeID, route[j].x, route[j].y));
			}
			await db.WriteMonkeyRecords(records);
			Console.WriteLine($"{woodID}:write db routes {woodID},{monkey.name} end");
		}
		public void WriteEscaperoutesToBitmap(List<List<Tree>> routes) {
			List<Task> tasks = new List<Task>();

			Console.WriteLine($"{woodID}:write bitmap routes {woodID} start");

			Color[] cvalues = new Color[] { Color.DarkRed, Color.Black, Color.DarkBlue, Color.Cyan, Color.DarkViolet };

			var width = (map.xmax - map.xmin) * drawingFactor;
			var height = (map.ymax - map.ymin) * drawingFactor;

			Bitmap bm = new Bitmap(width, height);
			Graphics g = Graphics.FromImage(bm);
			Graphics g2 = Graphics.FromImage(bm);

			int delta = drawingFactor / 2;

			Brush b = Brushes.DarkGreen;
			Pen p = Pens.DarkGreen;
			g.FillRectangle(Brushes.WhiteSmoke, 0, 0, width, height);

			tasks.Add(Task.Run(() => {
				foreach (Tree t in trees) {
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
		public async void WriteWoodToDB() {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{woodID}:write db wood {woodID} start");
			List<DBWoodRecord> records = new List<DBWoodRecord>();
			foreach (var tree in trees) {
				records.Add(new DBWoodRecord(woodID, tree.treeID, tree.x, tree.y));
			}
			await db.WriteWoodRecords(records);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{woodID}:write db wood {woodID} end");
		}
		public List<Tree> EscapeMonkey(Monkey monkey) {
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"{woodID}:start {woodID},{monkey.name}");
			Dictionary<int, bool> visited = new Dictionary<int, bool>();
			List<Tree> route = new List<Tree>() { monkey.tree };

			for (int i = 0; i < trees.Count; i++) {
				visited.Add(trees[i].treeID, false);
			}

			do {
				if (!apenPaden.ContainsKey(route.Count)) {
					apenPaden.Add(route.Count, new HashSet<int>());
				}
				apenPaden[route.Count].Add(monkey.tree.treeID);

				visited[monkey.tree.treeID] = true;
				SortedList<double, List<Tree>> distanceToMonkey = new SortedList<double, List<Tree>>();

				int radius = map.xmax * map.ymax / trees.Count;
				int multiplier = 1;

				List<Tree> filteredTrees;

				do {
					//zoek dichtste boom die nog niet is bezocht
					int searchRect = radius * multiplier;
					filteredTrees = trees.Where((tree) => {
						if (visited[tree.treeID] || tree.hasMonkey || apenPaden[route.Count].Contains(tree.treeID)) return false;

						int lBound = monkey.tree.x - searchRect;
						int rBound = monkey.tree.x + searchRect;

						int bBound = monkey.tree.y - searchRect;
						int oBound = monkey.tree.y + searchRect;

						if (tree.x >= lBound && tree.x < rBound) {
							if (tree.y >= bBound && tree.y < oBound)
								return true;
						}
						return false;
					}).ToList();

					if (filteredTrees.Count == 0 && map.xmax - map.xmin > searchRect && map.ymax - map.ymin > searchRect) {
						multiplier++;
						continue;
					}
					break;
				} while (true);


				foreach (Tree t in filteredTrees) {
					double distances = Math.Sqrt(Math.Pow(t.x - monkey.tree.x, 2) + Math.Pow(t.y - monkey.tree.y, 2));
					if (distanceToMonkey.ContainsKey(distances)) {
						distanceToMonkey[distances].Add(t);
					} else {
						distanceToMonkey.Add(distances, new List<Tree>() { t });
					}
				}


				//distance to border            
				//noord oost zuid west
				double distanceToBorder = (new List<double>() { map.ymax - monkey.tree.y, map.xmax - monkey.tree.x, monkey.tree.y - map.ymin, monkey.tree.x - map.xmin }).Min();
				if (distanceToMonkey.Count == 0) {
					WriteRouteToDB(monkey, route);
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
					return route;
				}
				if (distanceToBorder <= distanceToMonkey.First().Key) {
					WriteRouteToDB(monkey, route);
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
					return route;
				}

				Tree dichtesteTree = null;
				int distance = int.MaxValue;

				int maxBorderX = map.xmax;
				int minBorderX = map.xmin;

				int maxBorderY = map.ymax;
				int minBorderY = map.ymin;
				
				foreach (Tree tree in distanceToMonkey.First().Value) {

					int distanceToRightBorder = maxBorderX - tree.x;
					int distanceToLeftBorder = tree.x - minBorderX;
					int distanceToTopBorder = maxBorderY - tree.y;
					int distanceToBottomBorder = tree.y - minBorderY;

					int minDistance = (new List<int>() { distanceToRightBorder, distanceToLeftBorder, distanceToTopBorder, distanceToBottomBorder }).Min();

					if (dichtesteTree == null || minDistance <= distance) {
						dichtesteTree = tree;
						distance = minDistance;
					}
				}

				route.Add(dichtesteTree);
				monkey.tree = dichtesteTree;
			}
			while (true);
		}
	}
}
