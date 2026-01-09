using System.Data;
using Dapper;

namespace NehaSurgicalAPI.Data;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        return DateOnly.FromDateTime((DateTime)value);
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}

public class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;
        
        return DateOnly.FromDateTime((DateTime)value);
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
    }
}

public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value)
    {
        if (value is TimeSpan ts)
            return TimeOnly.FromTimeSpan(ts);
        if (value is DateTime dt)
            return TimeOnly.FromDateTime(dt);
        
        return TimeOnly.Parse(value.ToString()!);
    }

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}

public class NullableTimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly?>
{
    public override TimeOnly? Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;
        
        if (value is TimeSpan ts)
            return TimeOnly.FromTimeSpan(ts);
        if (value is DateTime dt)
            return TimeOnly.FromDateTime(dt);
        
        return TimeOnly.Parse(value.ToString()!);
    }

    public override void SetValue(IDbDataParameter parameter, TimeOnly? value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.HasValue ? value.Value.ToTimeSpan() : DBNull.Value;
    }
}
