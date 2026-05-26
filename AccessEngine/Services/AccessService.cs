using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessEngine.Services
{
    public class AccessService : IAccessService
    {
        private readonly string _connectionString;
        private const int DefaultCommandTimeout = 120;

        public AccessService(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private static CommandDefinition CreateStoredProcedureCommand(
            string storedProcedure,
            object? parameters,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storedProcedure))
                throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedure));

            return new CommandDefinition(
                commandText: storedProcedure,
                parameters: parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: DefaultCommandTimeout,
                cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<T>> QueryAsync<T, U>(
            string storedProcedure,
            U parameters,
            CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();

            var command = CreateStoredProcedureCommand(
                storedProcedure,
                parameters,
                cancellationToken);

            return await connection.QueryAsync<T>(command);
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T, U>(
            string storedProcedure,
            U parameters,
            CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();

            var command = CreateStoredProcedureCommand(
                storedProcedure,
                parameters,
                cancellationToken);

            return await connection.QueryFirstOrDefaultAsync<T>(command);
        }

        public async Task<T> QuerySingleAsync<T, U>(
            string storedProcedure,
            U parameters,
            CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();

            var command = CreateStoredProcedureCommand(
                storedProcedure,
                parameters,
                cancellationToken);

            return await connection.QuerySingleAsync<T>(command);
        }

        public async Task<T?> QuerySingleOrDefaultAsync<T, U>(
            string storedProcedure,
            U parameters,
            CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();

            var command = CreateStoredProcedureCommand(
                storedProcedure,
                parameters,
                cancellationToken);

            return await connection.QuerySingleOrDefaultAsync<T>(command);
        }

        public async Task<int> ExecuteAsync<U>(
            string storedProcedure,
            U parameters,
            CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();

            var command = CreateStoredProcedureCommand(
                storedProcedure,
                parameters,
                cancellationToken);

            return await connection.ExecuteAsync(command);
        }

        public async Task<string> ExecuteWithResultAsync<U>(
            string storedProcedure,
            U parameters,
            CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();

            var dynamicParameters = new DynamicParameters(parameters);

            dynamicParameters.Add(
                name: "Result",
                dbType: DbType.String,
                direction: ParameterDirection.Output,
                size: 200);

            var command = CreateStoredProcedureCommand(
                storedProcedure,
                dynamicParameters,
                cancellationToken);

            await connection.ExecuteAsync(command);

            return dynamicParameters.Get<string>("Result") ?? string.Empty;
        }

        public async Task<TResult> QueryMultipleAsync<U, TResult>(
            string storedProcedure,
            U parameters,
            Func<SqlMapper.GridReader, Task<TResult>> readFunc,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(readFunc);

            await using var connection = CreateConnection();

            var command = CreateStoredProcedureCommand(
                storedProcedure,
                parameters,
                cancellationToken);

            using var gridReader = await connection.QueryMultipleAsync(command);

            return await readFunc(gridReader);
        }
    }
}
