using Dapper;

namespace AccessEngine.Services
{
    public interface IAccessService
    {
        Task<int> ExecuteAsync<U>(string storedProcedure, U parameters, CancellationToken cancellationToken = default);
        Task<string> ExecuteWithResultAsync<U>(string storedProcedure, U parameters, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> QueryAsync<T, U>(string storedProcedure, U parameters, CancellationToken cancellationToken = default);
        Task<T?> QueryFirstOrDefaultAsync<T, U>(string storedProcedure, U parameters, CancellationToken cancellationToken = default);
        Task<TResult> QueryMultipleAsync<U, TResult>(string storedProcedure, U parameters, Func<SqlMapper.GridReader, Task<TResult>> readFunc, CancellationToken cancellationToken = default);
        Task<T> QuerySingleAsync<T, U>(string storedProcedure, U parameters, CancellationToken cancellationToken = default);
        Task<T?> QuerySingleOrDefaultAsync<T, U>(string storedProcedure, U parameters, CancellationToken cancellationToken = default);
    }
}