namespace deeplynx.interfaces;

public interface IBulkCopyUpsertExecutor
{
    Task<List<TOut>> CopyUpsertAsync<TIn, TOut>(
        Npgsql.NpgsqlConnection conn,
        Npgsql.NpgsqlTransaction tx,
        string createTempSql,
        string copyCommandText,
        IEnumerable<TIn> rows,
        Action<Npgsql.NpgsqlBinaryImporter, TIn> writeRow,
        string upsertSql,
        Func<Npgsql.NpgsqlDataReader, TOut> mapRow,
        CancellationToken ct = default);
}
