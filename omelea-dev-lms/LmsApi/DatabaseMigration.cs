using Npgsql;

namespace LmsApi;

public static class DatabaseMigration
{
    public static async Task Apply(NpgsqlDataSource source, string directory)
    {
        await using var connection = await source.OpenConnectionAsync();
        await using var migrationLock = new NpgsqlCommand("SELECT pg_advisory_lock(2026100801)", connection);
        await migrationLock.ExecuteNonQueryAsync();
        try
        {
            await using var init = new NpgsqlCommand("""
                CREATE TABLE IF NOT EXISTS public.lms_schema_migrations (
                    name text PRIMARY KEY, applied_at timestamptz NOT NULL DEFAULT now()
                )
                """, connection);
            await init.ExecuteNonQueryAsync();
            foreach (var file in Directory.GetFiles(directory, "*.sql").Order())
            {
                var name = Path.GetFileName(file);
                await using var exists = new NpgsqlCommand("SELECT count(*) FROM public.lms_schema_migrations WHERE name=@name", connection);
                exists.Parameters.AddWithValue("name", name);
                if ((long)(await exists.ExecuteScalarAsync())! > 0) continue;
                await using var transaction = await connection.BeginTransactionAsync();
                await using var script = new NpgsqlCommand(await File.ReadAllTextAsync(file), connection, transaction);
                await script.ExecuteNonQueryAsync();
                await using var record = new NpgsqlCommand("INSERT INTO public.lms_schema_migrations(name) VALUES (@name)", connection, transaction);
                record.Parameters.AddWithValue("name", name);
                await record.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
        }
        finally
        {
            await using var unlock = new NpgsqlCommand("SELECT pg_advisory_unlock(2026100801)", connection);
            await unlock.ExecuteNonQueryAsync();
        }
    }
}
