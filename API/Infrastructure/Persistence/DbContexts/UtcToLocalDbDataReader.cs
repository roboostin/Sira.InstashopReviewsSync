using System.Collections;
using System.Data.Common;

namespace API.Infrastructure.Persistence.DbContexts;

public class UtcToLocalDbDataReader : DbDataReader
{
    private readonly DbDataReader _innerReader;
    private readonly TimeZoneInfo _timeZone;

    public UtcToLocalDbDataReader(DbDataReader innerReader, TimeZoneInfo timeZone)
    {
        _innerReader = innerReader;
        _timeZone = timeZone;
    }

    public override object GetValue(int ordinal)
    {
        var value = _innerReader.GetValue(ordinal);

        if (value is DateTime utcDateTime)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _timeZone);
        }

        return value;
    }

    public override T GetFieldValue<T>(int ordinal)
    {
        var value = _innerReader.GetFieldValue<T>(ordinal);

        if (typeof(T) == typeof(DateTime) && value is DateTime utcDateTime)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            var local = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _timeZone);
            return (T)(object)local;
        }

        return value;
    }

    // Delegate all other methods to the inner reader
    public override bool Read() => _innerReader.Read();
    public override int FieldCount => _innerReader.FieldCount;
    public override bool HasRows => _innerReader.HasRows;
    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(_innerReader.GetOrdinal(name));
    public override bool IsDBNull(int ordinal) => _innerReader.IsDBNull(ordinal);
    public override string GetName(int ordinal) => _innerReader.GetName(ordinal);
    public override int GetOrdinal(string name) => _innerReader.GetOrdinal(name);
    public override string GetDataTypeName(int ordinal) => _innerReader.GetDataTypeName(ordinal);
    public override Type GetFieldType(int ordinal) => _innerReader.GetFieldType(ordinal);
    public override int Depth => _innerReader.Depth;
    public override bool IsClosed => _innerReader.IsClosed;
    public override int RecordsAffected => _innerReader.RecordsAffected;
    public override bool NextResult() => _innerReader.NextResult();
    public override void Close() => _innerReader.Close();
    public override System.Data.DataTable GetSchemaTable() => _innerReader.GetSchemaTable();
    public override int GetValues(object[] values) => _innerReader.GetValues(values);
    public override bool GetBoolean(int ordinal) => _innerReader.GetBoolean(ordinal);
    public override byte GetByte(int ordinal) => _innerReader.GetByte(ordinal);
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
        _innerReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
    public override char GetChar(int ordinal) => _innerReader.GetChar(ordinal);
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
        _innerReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
    public override Guid GetGuid(int ordinal) => _innerReader.GetGuid(ordinal);
    public override short GetInt16(int ordinal) => _innerReader.GetInt16(ordinal);
    public override int GetInt32(int ordinal) => _innerReader.GetInt32(ordinal);
    public override long GetInt64(int ordinal) => _innerReader.GetInt64(ordinal);
    public override float GetFloat(int ordinal) => _innerReader.GetFloat(ordinal);
    public override double GetDouble(int ordinal) => _innerReader.GetDouble(ordinal);
    public override string GetString(int ordinal) => _innerReader.GetString(ordinal);
    public override decimal GetDecimal(int ordinal) => _innerReader.GetDecimal(ordinal);
    public override DateTime GetDateTime(int ordinal) => (DateTime)GetValue(ordinal);
    public override IEnumerator GetEnumerator() => ((IEnumerable)_innerReader).GetEnumerator();
}
