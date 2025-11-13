using deeplynx.interfaces;
using Npgsql;

namespace deeplynx.helpers.BigData;

public sealed class BulkCopyUpsertExecutor : IBulkCopyUpsertExecutor
{
    /// <summary>
    ///     Bulk upsert to a PostgreSQL Database using binary copy and single instance upserting
    /// </summary>
    /// <param name="conn">NPGSQL PostgreSQL Connection</param>
    /// <param name="tx">NPGSQL PostgreSQL Transaction for rollback</param>
    /// <param name="createTempSql">DDL statement to define temp table schema</param>
    /// <param name="copyCommandText">SQL statement to copy data into temp table</param>
    /// <param name="rows">Input enumerable data of generic type to be inserted</param>
    /// <param name="writeRow">Delegate map to handle per row binary writes of 'rows'</param>
    /// <param name="upsertSql">SQL Statement to handle data specific PG update and conflict resolution</param>
    /// <param name="mapRow">Map generic return list fields from upserted results</param>
    /// <param name="ct">Optional cancellation token to end long requests</param>
    /// <returns>Generic type list of ORM rows created</returns>
    public async Task<List<TOut>> CopyUpsertAsync<TIn, TOut>(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        string createTempSql,
        string copyCommandText,
        IEnumerable<TIn> rows,
        Action<NpgsqlBinaryImporter, TIn> writeRow,
        string upsertSql,
        Func<NpgsqlDataReader, TOut> mapRow,
        CancellationToken ct = default)
    {
        await using (var cmd = new NpgsqlCommand(createTempSql, conn, tx))
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await using (var writer = conn.BeginBinaryImport(copyCommandText))
        {
            foreach (var row in rows)
            {
                await writer.StartRowAsync(ct);
                writeRow(writer, row);
            }

            await writer.CompleteAsync(ct);
        }

        var result = new List<TOut>();
        await using (var upsert = new NpgsqlCommand(upsertSql, conn, tx))
        await using (var reader = await upsert.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
                result.Add(mapRow(reader));
        }

        return result;
    }

    /// <summary>
    ///     Bulk insert to a PostgreSQL Database using binary copy and single instance upserting
    /// </summary>
    /// <param name="conn">NPGSQL PostgreSQL Connection</param>
    /// <param name="tx">NPGSQL PostgreSQL Transaction for rollback</param>
    /// <param name="createTempSql">DDL statement to define temp table schema</param>
    /// <param name="copyCommandText">SQL statement to copy data into temp table</param>
    /// <param name="rows">Input enumerable data of generic type to be inserted</param>
    /// <param name="writeRow">Delegate map to handle per row binary writes of 'rows'</param>
    /// <param name="insertSql">SQL Statement to handle data specific inserts</param>
    /// <param name="ct">Optional cancellation token to end long requests</param>
    /// <returns>Generic type list of ORM rows created</returns>
    public async Task<int> CopyInsertAsync<TIn>(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string createTempSql, string copyCommandText,
        IEnumerable<TIn> rows,
        Action<NpgsqlBinaryImporter, TIn> writeRow,
        string insertSql,
        CancellationToken ct = default)
    {
        if (conn is null) throw new ArgumentNullException(nameof(conn));
        if (createTempSql is null) throw new ArgumentNullException(nameof(createTempSql));
        if (copyCommandText is null) throw new ArgumentNullException(nameof(copyCommandText));
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        if (writeRow is null) throw new ArgumentNullException(nameof(writeRow));
        if (insertSql is null) throw new ArgumentNullException(nameof(insertSql));

        await using (var cmd = new NpgsqlCommand(createTempSql, conn, tx))
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await using (var writer = conn.BeginBinaryImport(copyCommandText))
        {
            foreach (var row in rows)
            {
                if (row is null) throw new ArgumentException("One or more rows were null");
                await writer.StartRowAsync(ct);
                writeRow(writer, row);
            }

            await writer.CompleteAsync(ct);
        }

        await using var insert = new NpgsqlCommand(insertSql, conn, tx);
        return await insert.ExecuteNonQueryAsync(ct);
    }
}