namespace RpsLs.ApplicationService.Services;

public interface IRandomService
{
    Task<int> GetRandomNumberAsync();
}
