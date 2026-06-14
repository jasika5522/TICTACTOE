using Dapper;
using System.Data;

namespace TICTACTOEAPI.DAL.TypeHandlers
{
    public class CharTypeHandler : SqlMapper.TypeHandler<char>
    {
        public override void SetValue(IDbDataParameter parameter, char value)
        {
            parameter.Value = value.ToString();
            parameter.DbType = DbType.String;
        }

        public override char Parse(object value) =>
            value is string s && s.Length > 0 ? s[0] : '\0';
    }

    public class NullableCharTypeHandler : SqlMapper.TypeHandler<char?>
    {
        public override void SetValue(IDbDataParameter parameter, char? value)
        {
            parameter.Value = value.HasValue ? (object)value.Value.ToString() : DBNull.Value;
            parameter.DbType = DbType.String;
        }

        public override char? Parse(object value) =>
            value is DBNull or null ? null : ((string)value).Length > 0 ? ((string)value)[0] : null;
    }
}
