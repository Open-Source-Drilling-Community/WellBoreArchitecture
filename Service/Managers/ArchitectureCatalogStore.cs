using Microsoft.Data.Sqlite;
using OSDC.DotnetLibraries.General.DataManagement;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Managers;

internal sealed class ArchitectureCatalogStore<T> where T : class
{
    private readonly SqlConnectionManager manager;
    private readonly string table;
    private readonly string documentColumn;
    private readonly Func<T, MetaInfo?> meta;
    private readonly Func<T, string?> name;
    private readonly Action<T, DateTimeOffset?> setCreated;
    private readonly Action<T, DateTimeOffset?> setModified;
    private readonly Func<T, bool>? exclusive;
    private readonly Func<T, bool>? validity;

    public ArchitectureCatalogStore(SqlConnectionManager manager, string table, string documentColumn,
        Func<T, MetaInfo?> meta, Func<T, string?> name, Action<T, DateTimeOffset?> setCreated,
        Action<T, DateTimeOffset?> setModified, Func<T, bool>? exclusive = null, Func<T, bool>? validity = null)
    {
        this.manager = manager; this.table = table; this.documentColumn = documentColumn;
        this.meta = meta; this.name = name; this.setCreated = setCreated; this.setModified = setModified;
        this.exclusive = exclusive; this.validity = validity;
    }

    public List<T> All()
    {
        using SqliteConnection connection = manager.GetConnection()!;
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT {documentColumn} FROM {table} ORDER BY Name, ID";
        using SqliteDataReader reader = command.ExecuteReader();
        List<T> values = [];
        while (reader.Read())
        {
            T? value = JsonSerializer.Deserialize<T>(reader.GetString(0), JsonSettings.Options);
            if (value != null) values.Add(value);
        }
        return values;
    }

    public T? ById(Guid id) => All().FirstOrDefault(value => meta(value)?.ID == id);

    public bool Add(T value)
    {
        MetaInfo? info = meta(value);
        if (info == null || info.ID == Guid.Empty || ById(info.ID) != null) return false;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        setCreated(value, now); setModified(value, now);
        return Write(value, false);
    }

    public bool Update(Guid id, T value)
    {
        if (id == Guid.Empty || meta(value)?.ID != id || ById(id) == null) return false;
        setModified(value, DateTimeOffset.UtcNow);
        return Write(value, true);
    }

    private bool Write(T value, bool update)
    {
        MetaInfo info = meta(value)!;
        string serialized = JsonSerializer.Serialize(value, JsonSettings.Options);
        string serializedMeta = JsonSerializer.Serialize(info, JsonSettings.Options);
        using SqliteConnection connection = manager.GetConnection()!;
        using SqliteTransaction transaction = connection.BeginTransaction();
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        if (exclusive == null)
            command.CommandText = update
                ? $"UPDATE {table} SET MetaInfo=$meta,Name=$name,CreationDate=$created,LastModificationDate=$modified,{documentColumn}=$document WHERE ID=$id"
                : $"INSERT INTO {table}(ID,MetaInfo,Name,CreationDate,LastModificationDate,{documentColumn}) VALUES($id,$meta,$name,$created,$modified,$document)";
        else
            command.CommandText = update
                ? $"UPDATE {table} SET MetaInfo=$meta,Name=$name,IsExclusive=$exclusive,HasValidityPeriod=$validity,CreationDate=$created,LastModificationDate=$modified,{documentColumn}=$document WHERE ID=$id"
                : $"INSERT INTO {table}(ID,MetaInfo,Name,IsExclusive,HasValidityPeriod,CreationDate,LastModificationDate,{documentColumn}) VALUES($id,$meta,$name,$exclusive,$validity,$created,$modified,$document)";
        command.Parameters.AddWithValue("$id", info.ID.ToString());
        command.Parameters.AddWithValue("$meta", serializedMeta);
        command.Parameters.AddWithValue("$name", (object?)name(value) ?? DBNull.Value);
        command.Parameters.AddWithValue("$created", ReadDate(value, "CreationDate") ?? DBNull.Value);
        command.Parameters.AddWithValue("$modified", ReadDate(value, "LastModificationDate") ?? DBNull.Value);
        command.Parameters.AddWithValue("$document", serialized);
        if (exclusive != null)
        {
            command.Parameters.AddWithValue("$exclusive", exclusive(value) ? 1 : 0);
            command.Parameters.AddWithValue("$validity", validity!(value) ? 1 : 0);
        }
        bool success = command.ExecuteNonQuery() == 1;
        if (success) transaction.Commit(); else transaction.Rollback();
        return success;
    }

    public bool Delete(Guid id)
    {
        using SqliteConnection connection = manager.GetConnection()!;
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {table} WHERE ID=$id";
        command.Parameters.AddWithValue("$id", id.ToString());
        return command.ExecuteNonQuery() == 1;
    }

    private static object? ReadDate(T value, string property) =>
        value.GetType().GetProperty(property)?.GetValue(value) is DateTimeOffset date
            ? date.ToString("O") : null;
}
