﻿using MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EscapeFromTheWoods {
	public static class WoodBuilder {
		public static Wood GetWood(int aantalTrees, Map map, string path, Repo db) {
			bool maxAantalBomen = map.xmax * map.ymax < aantalTrees;
			if (maxAantalBomen) {
				Console.WriteLine($"Je kan maximum: {map.xmax * map.ymax} bomen hebben");
			}

			Random r = new Random(100);
			Dictionary<(int, int), Tree> trees = new Dictionary<(int, int), Tree>();
			int count = 0;

			while (count < aantalTrees) {
				Tree tree = new Tree(count, r.Next(map.xmin, map.xmax), r.Next(map.ymin, map.ymax));
				if (!trees.ContainsKey((tree.x, tree.y))) { trees.Add((tree.x, tree.y), tree); count++; }
			}

			Wood w = new Wood(IDgenerator.GetWoodID(), trees.Values.ToList(), map, path, db);
			return w;
		}
	}
}
