using System;
using System.Collections.Generic;
using System.Text;

namespace EscapeFromTheWoods
{
    public class DBMonkeyRecord
    {
        public DBMonkeyRecord(int monkeyID, string monkeyName, int woodID, int seqNr, int treeID, int x, int y)
        {
            //this.recordID = recordID;
            this.monkeyID = monkeyID;
            this.monkeyName = monkeyName;
            this.woodID = woodID;
            this.seqNr = seqNr;
            this.treeID = treeID;
            this.x = x;
            this.y = y;
        }

        public int recordID { get; set; }
        public int monkeyID { get; set; }
        public string monkeyName { get; set; }
        public int woodID { get; set; }
        public int seqNr { get; set; }
        public int treeID { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }
}
