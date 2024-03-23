namespace Consumer.DTOs;
public class OutputVar()
{
  public Dictionary<int, string> DatabaseToFile { get; set; } = new();
  public Dictionary<int, string> FileToDatabase { get; set; } = new();
  public Dictionary<int, string> DifferentStatus { get; set; } = new();
}