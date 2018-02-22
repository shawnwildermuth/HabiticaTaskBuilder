using System;

namespace HabiticaTaskBuilder
{
  internal class HabiticaTask
  {
    public HabiticaTask()
    {
    }

    public string Text { get; set; }
    public string Type { get; set; }
    public string[] Tags { get; set; }
    public DateTime Date { get; set; }
    public string Priority { get; set; }
    public string[] Subtasks { get; set; }
  }

}