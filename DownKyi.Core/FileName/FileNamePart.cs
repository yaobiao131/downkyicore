namespace DownKyi.Core.FileName;

public enum FileNamePart
{
    // Video
    Order = 1,
    Section,
    MainTitle,
    PageTitle,
    VideoZone,
    AudioQuality,
    VideoQuality,
    VideoCodec,

    VideoPublishTime,

    Avid,
    Bvid,
    Cid,

    UpMid,
    UpName,

    // 斜杠
    Slash = 100,

    // HyphenSeparated
    Underscore = 101, // 下划线
    Hyphen, // 连字符
    Plus, // 加号
    Comma, // 逗号
    Period, // 句号
    And, // and
    Number, // #
    OpenParen, // 左圆括号
    CloseParen, // 右圆括号
    OpenBracket, // 左方括号
    CloseBracket, // 右方括号
    OpenBrace, // 左花括号
    CloseBrace, // 右花括号
    Blank, // 空白符
}