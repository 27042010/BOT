namespace Domain.Interfaces.Service
{
    public interface IForexService
    {
        Task<double> GetForex(string from, string to);
    }
}
