namespace SmartApi.Services
{
    public interface IAIService
    {
        Task<string> GenerateAnswer(string question, IEnumerable<object> data);
        Task<string> ExtractIntent(string question);
    }
}