using System;

namespace DownKyi.Utils.DataAnnotations;

/// <summary>
/// When the entity class property is <see cref="object"/>, map storage in JSON format. <br />
/// 当实体类属性为【对象】时，以 JSON 形式映射存储
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class JsonMapAttribute : Attribute { }