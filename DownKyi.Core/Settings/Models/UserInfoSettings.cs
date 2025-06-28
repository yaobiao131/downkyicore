namespace DownKyi.Core.Settings.Models;

public class UserInfoSettings : IEquatable<UserInfoSettings>
{
    public long Mid { get; set; }
    public string Name { get; set; }
    public bool IsLogin { get; set; } // 是否登录
    public bool IsVip { get; set; } // 是否为大会员，未登录时为false

    public string ImgKey { get; set; }
    public string SubKey { get; set; }
    
    public override int GetHashCode()
    {
        return GetHashCode(this);
    }

    public int GetHashCode(UserInfoSettings obj)
    {
        return HashCode.Combine(obj.Mid, obj.Name, obj.IsLogin, obj.IsVip, obj.ImgKey, obj.SubKey);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj);
    }

    public bool Equals(UserInfoSettings? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Mid == other.Mid && Name == other.Name && IsLogin == other.IsLogin &&
               IsVip == other.IsVip && ImgKey == other.ImgKey && SubKey == other.SubKey;
    }
}