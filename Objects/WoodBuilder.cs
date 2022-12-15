using MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EscapeFromTheWoods
{
    public static class WoodBuilder
    {        
        public static Wood GetWood(int aantalTrees, Map map,string path, Repo db)
        {
			Random r = new Random(100);
			Dictionary<int, Tree> trees = new Dictionary<int, Tree>();
			int count = 0;

			while (count < aantalTrees) {
				Tree tree = new Tree(count, r.Next(map.xmin, map.xmax), r.Next(map.ymin, map.ymax));
				if (!trees.ContainsKey(tree.treeID)) { trees.Add(tree.treeID, tree); count++; }
			}

			Wood w = new Wood(IDgenerator.GetWoodID(), trees.Values.ToList(), map, path, db);
			return w;
		}
    }
}
