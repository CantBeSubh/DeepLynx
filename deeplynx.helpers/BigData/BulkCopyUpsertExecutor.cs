using deeplynx.interfaces;

namespace deeplynx.helpers.BigData;

public sealed class BulkCopyUpsertExecutor : IBulkCopyUpsertExecutor
{
    public async Task<List<TOut>> CopyUpsertAsync<TIn, TOut>(
        Npgsql.NpgsqlConnection conn,
        Npgsql.NpgsqlTransaction tx,
        string createTempSql,
        string copyCommandText,
        IEnumerable<TIn> rows,
        Action<Npgsql.NpgsqlBinaryImporter, TIn> writeRow,
        string upsertSql,
        Func<Npgsql.NpgsqlDataReader, TOut> mapRow,
        CancellationToken ct = default)
    {
        await using (var cmd = new Npgsql.NpgsqlCommand(createTempSql, conn, tx))
            await cmd.ExecuteNonQueryAsync(ct);

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
        await using (var upsert = new Npgsql.NpgsqlCommand(upsertSql, conn, tx))
        await using (var reader = await upsert.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
                result.Add(mapRow(reader));
        }
        return result;
    }
}
