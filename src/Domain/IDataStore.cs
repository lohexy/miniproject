namespace Domain;

public interface IDataStore<T>
{
    Task<List<T>> LoadAsync();
    Task SaveAsync(List<T> items);
}