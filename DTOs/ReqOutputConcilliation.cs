namespace Consumer.DTOs;
public class ReqOutpuConcilliation(OutputVar outputVar)
{
    public List<BaseJson> DatabaseToFile = outputVar.DatabaseToFile.Select(pair => new BaseJson(pair.Key, pair.Value)).ToList();
    public List<BaseJson> FileToDatabase = outputVar.FileToDatabase.Select(pair => new BaseJson(pair.Key, pair.Value)).ToList();
    public List<BaseJson> DifferentStatus = outputVar.DifferentStatus.Select(pair => new BaseJson(pair.Key, pair.Value)).ToList();
}

public class BaseJson(int id, string status)
{
    public int Id { get; set; } = id;
    public string Status { get; set; } = status;
}