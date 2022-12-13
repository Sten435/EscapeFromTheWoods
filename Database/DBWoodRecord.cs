using System;
using System.Collections.Generic;
using System.Text;

namespace EscapeFromTheWoods
{
    public class DBWoodRecord
    {
        public DBWoodRecord(int woodID, int treeID, int x, int y)
        {
            this.woodID = woodID;
            this.treeID = treeID;
            this.x = x;
            this.y = y;
        }

        public int recordID { get; set; }
        public int woodID { get; set; }
        public int treeID { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }
}
