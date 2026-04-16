using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Logging;

namespace NFramework.Mediator.Tests.Logging;

public class LoggingMaskingTests
{
    internal sealed class TestLoggingBehavior(ILogger logger) : LoggingBehaviorBase<object, object>(logger)
    {
        public static string TestMaskValue(string value, in LogExcludeParameter param)
        {
            return MaskValue(value, param);
        }
    }

    [Theory]
    // Email Masking (value.Contains('@'))
    // Long email
    [InlineData("test@example.com", 2, 4, '*', "te**********.com")]
    // Email < 9 chars -> no masking
    [InlineData("a@b.com", 1, 1, '*', "a@b.com")]
    // Email with 0 start/end
    [InlineData("test@example.com", 0, 0, '*', "****************")]
    // Email where maskLength <= 0
    [InlineData("test@example.com", 8, 8, '*', "test@example.com")]
    // Numeric masking (value.All(char.IsDigit))
    // CC length -> keepStart = param=0 ? 2 : param, keepEnd = param=0 ? 2 : param
    [InlineData("1234567812345678", 0, 0, '*', "12************78")] // Defaults to 2, 2 when 0
    [InlineData("1234567812345678", 4, 4, '*', "1234********5678")]
    // Short numeric (<= 4 chars) -> no masking
    [InlineData("1234", 0, 0, '*', "1234")]
    // Default fixed length masking (param.KeepEndChars == 0)
    // password, keep 0 -> length string is 0 + 3 (fixed 3). But value.length is 8, which is > keepStart(0).
    // Result is an array of size 3. Filled with '*'. So "***"
    [InlineData("password", 0, 0, '*', "***")]
    // password, keep 2 -> array of size 5. my***
    [InlineData("mypassword", 2, 0, '*', "my***")]
    // short value (length <= keepStart)
    [InlineData("ab", 4, 0, '*', "ab")]
    // Default masking with EndChars > 0
    // length == start + end + 1
    [InlineData("abc", 1, 1, '*', "a*c")]
    // length <= start + end
    [InlineData("ab", 1, 1, '*', "ab")]
    // length < start + end + 5
    [InlineData("abcdef", 2, 2, '*', "ab**ef")]
    // length >= start + end + 5 -> max 5 mask chars inserted
    [InlineData("verylongpassword", 2, 2, '*', "ve*****rd")]
    // Empty value
    [InlineData("", 2, 2, '*', "")]
    public void MaskValue_AppliesCorrectAlgorithm(
        string value,
        int keepStart,
        int keepEnd,
        char maskChar,
        string expected
    )
    {
        LogExcludeParameter param = new LogExcludeParameter
        {
            Name = "Test",
            Mask = true,
            KeepStartChars = keepStart,
            KeepEndChars = keepEnd,
            MaskChar = maskChar,
        };

        string result = TestLoggingBehavior.TestMaskValue(value, param);

        result.ShouldBe(expected);
    }
}
