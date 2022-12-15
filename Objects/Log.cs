namespace EscapeFromTheWoods.Objects
{
    public class Log
    {
        public int woodId { get; set; }
        public int monkeyId { get; set; }
        public string message { get; set; }

        public Log(int woodId, int monkeyId, string message)
        {
            this.woodId = woodId;
            this.monkeyId = monkeyId;
            this.message = message;
        }
    }
}