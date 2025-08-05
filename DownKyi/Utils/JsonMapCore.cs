using System.Text.Json;
using DownKyi.Utils.DataAnnotations;
using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace DownKyi.Utils;

public static class SystemTextJsonHelper
{
    private static readonly MethodInfo DeserializeMethodInfo =
        typeof(JsonSerializer).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Deserialize"
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 2
                        && m.GetParameters()[0].ParameterType == typeof(string));

    public static object? Deserialize(string json, Type type, JsonSerializerOptions options)
    {
        // 创建泛型方法
        var genericMethod = DeserializeMethodInfo.MakeGenericMethod(type);
        return genericMethod.Invoke(null, new object[] { json, options });
    }

    public static string Serialize(object value, JsonSerializerOptions options)
    {
        return JsonSerializer.Serialize(value, options);
    }
}

public static class FreeSqlJsonMapCoreExtensions
{
    private static int _isAoped;
    private static readonly ConcurrentDictionary<Type, bool> DicTypes = new();

    private static readonly MethodInfo? MethodJsonConvertDeserializeObject =
        typeof(SystemTextJsonHelper).GetMethod("Deserialize", new[] { typeof(string), typeof(Type), typeof(JsonSerializerOptions) });

    private static readonly MethodInfo? MethodJsonConvertSerializeObject =
        typeof(SystemTextJsonHelper).GetMethod("Serialize", new[] { typeof(object), typeof(JsonSerializerOptions) });

    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>> DicJsonMapFluentApi = new();

    private static readonly object ConcurrentObj = new object();

    public static ColumnFluent JsonMap(this ColumnFluent col)
    {
        DicJsonMapFluentApi.GetOrAdd(col._entityType, et => new ConcurrentDictionary<string, bool>())
            .GetOrAdd(col._property.Name, pn => true);
        return col;
    }

    /// <summary>
    /// When the entity class property is <see cref="object"/> and the attribute is marked as <see cref="JsonMapAttribute"/>, map storage in JSON format. <br />
    /// 当实体类属性为【对象】时，并且标记特性 [JsonMap] 时，该属性将以JSON形式映射存储
    /// </summary>
    /// <returns></returns>
    public static void UseJsonMap(this IFreeSql fsql)
    {
        UseJsonMap(fsql, JsonSerializerOptions.Default);
    }

    public static void UseJsonMap(this IFreeSql fsql, JsonSerializerOptions options)
    {
        if (Interlocked.CompareExchange(ref _isAoped, 1, 0) == 0)
        {
            FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
            {
                if (DicTypes.ContainsKey(type))
                    return Expression.IfThenElse(
                        Expression.TypeIs(valueExp, type),
                        Expression.Return(returnTarget, valueExp),
                        Expression.Return(returnTarget,
                            Expression.TypeAs(
                                Expression.Call(MethodJsonConvertDeserializeObject, Expression.Convert(valueExp, typeof(string)), Expression.Constant(type),
                                    Expression.Constant(options)), type))
                    );
                return null;
            });
        }

        fsql.Aop.ConfigEntityProperty += (s, e) =>
        {
            var isJsonMap = e.Property.GetCustomAttributes(typeof(JsonMapAttribute), false).Any() ||
                            DicJsonMapFluentApi.TryGetValue(e.EntityType, out var tryjmfu) && tryjmfu.ContainsKey(e.Property.Name);
            if (isJsonMap)
            {
                if (DicTypes.ContainsKey(e.Property.PropertyType) == false &&
                    FreeSql.Internal.Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(e.Property.PropertyType))
                    return; //基础类型使用 JsonMap 无效

                if (e.ModifyResult.MapType == null)
                {
                    e.ModifyResult.MapType = typeof(string);
                    e.ModifyResult.StringLength = -2;
                }

                if (DicTypes.TryAdd(e.Property.PropertyType, true))
                {
                    lock (ConcurrentObj)
                    {
                        FreeSql.Internal.Utils.dicExecuteArrayRowReadClassOrTuple[e.Property.PropertyType] = true;
                        FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionObjectToStringIfThenElse.Add((LabelTarget returnTarget, Expression valueExp, Expression elseExp,
                            Type type) =>
                        {
                            return Expression.IfThenElse(
                                Expression.TypeIs(valueExp, e.Property.PropertyType),
                                Expression.Return(returnTarget,
                                    Expression.Call(MethodJsonConvertSerializeObject, Expression.Convert(valueExp, typeof(object)), Expression.Constant(options)), typeof(object)),
                                elseExp);
                        });
                    }
                }
            }
        };
        switch (fsql.Ado.DataType)
        {
            case DataType.Sqlite:
            case DataType.MySql:
            case DataType.OdbcMySql:
            case DataType.CustomMySql:
            case DataType.SqlServer:
            case DataType.OdbcSqlServer:
            case DataType.CustomSqlServer:
            case DataType.Oracle:
            case DataType.OdbcOracle:
            case DataType.CustomOracle:
            case DataType.Dameng:
            case DataType.DuckDB:
                fsql.Aop.ParseExpression += (_, e) =>
                {
                    //if (e.Expression is MethodCallExpression callExp)
                    //{
                    //    var objExp = callExp.Object;
                    //    var objType = objExp?.Type;
                    //    if (objType?.FullName == "System.Byte[]") return;

                    //    if (objType == null && callExp.Method.DeclaringType == typeof(Enumerable))
                    //    {
                    //        objExp = callExp.Arguments.FirstOrDefault();
                    //        objType = objExp?.Type;
                    //    }
                    //    if (objType == null) objType = callExp.Method.DeclaringType;
                    //    if (objType != null || objType.IsArrayOrList())
                    //    {
                    //        string left = null;
                    //        switch (callExp.Method.Name)
                    //        {
                    //            case "Any":
                    //                left = objExp == null ? null : getExp(objExp);
                    //                if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
                    //                return $"(case when {left} is null then 0 else array_length({left},1) end > 0)";
                    //            case "Contains":
                    //        }
                    //    }
                    //}
                    //解析 POCO Json   a.Customer.Name
                    if (e.Expression is MemberExpression memExp)
                    {
                        if (e.Expression.IsParameter() == false) return;
                        var parentMemExps = new Stack<MemberExpression>();
                        parentMemExps.Push(memExp);
                        while (true)
                        {
                            switch (memExp.Expression?.NodeType)
                            {
                                case ExpressionType.MemberAccess:
                                case ExpressionType.Parameter: break;
                                default: return;
                            }

                            switch (memExp.Expression.NodeType)
                            {
                                case ExpressionType.MemberAccess:
                                    memExp = memExp.Expression as MemberExpression;
                                    if (memExp == null) return;
                                    parentMemExps.Push(memExp);
                                    break;
                                case ExpressionType.Parameter:
                                    var tb = fsql.CodeFirst.GetTableByEntity(memExp.Expression.Type);
                                    if (tb == null) return;
                                    if (tb.ColumnsByCs.TryGetValue(parentMemExps.Pop().Member.Name, out var trycol) == false) return;
                                    if (DicTypes.ContainsKey(trycol.CsType) == false) return;
                                    var result = e.FreeParse(Expression.MakeMemberAccess(memExp.Expression, tb.Properties[trycol.CsName]));
                                    if (parentMemExps.Any() == false)
                                    {
                                        e.Result = result;
                                        return;
                                    }

                                    var jsonPath = "";
                                    switch (fsql.Ado.DataType)
                                    {
                                        case DataType.Sqlite:
                                        case DataType.MySql:
                                        case DataType.OdbcMySql:
                                        case DataType.CustomMySql:
                                            StyleJsonExtract();
                                            return;
                                        case DataType.SqlServer:
                                        case DataType.OdbcSqlServer:
                                        case DataType.CustomSqlServer:
                                        case DataType.Oracle:
                                        case DataType.OdbcOracle:
                                        case DataType.CustomOracle:
                                        case DataType.Dameng:
                                            StyleJsonValue();
                                            return;
                                        case DataType.DuckDB:
                                            StyleDotAccess();
                                            return;
                                    }

                                    StylePgJson();
                                    return;

                                    void StyleJsonExtract()
                                    {
                                        while (parentMemExps.Any())
                                        {
                                            memExp = parentMemExps.Pop();
                                            jsonPath = $"{jsonPath}.{memExp.Member.Name}";
                                        }

                                        e.Result = $"json_extract({result},'${jsonPath}')";
                                    }

                                    void StyleJsonValue()
                                    {
                                        while (parentMemExps.Any())
                                        {
                                            memExp = parentMemExps.Pop();
                                            jsonPath = $"{jsonPath}.{memExp.Member.Name}";
                                        }

                                        e.Result = $"json_value({result},'${jsonPath}')";
                                    }

                                    void StyleDotAccess()
                                    {
                                        while (parentMemExps.Any())
                                        {
                                            memExp = parentMemExps.Pop();
                                            result = $"{result}['{memExp.Member.Name}']";
                                        }

                                        e.Result = result;
                                    }

                                    void StylePgJson()
                                    {
                                        while (parentMemExps.Any())
                                        {
                                            memExp = parentMemExps.Pop();
                                            var opt = parentMemExps.Any() ? "->" : $"->>{(memExp.Type.IsArrayOrList() ? "/*json array*/" : "")}";
                                            result = $"{result}{opt}'{memExp.Member.Name}'";
                                        }

                                        e.Result = result;
                                    }
                            }
                        }
                    }
                };
                break;
        }
    }
}