namespace BreganUtils
{
    public class TimezoneHelper
    {
        public static DateTime ConvertDateTimeToLocalTime(string timezoneId, DateTime dt)
        {
            var sourceTZ = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            var destinazionTZ = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(dt, sourceTZ, destinazionTZ), DateTimeKind.Local);
        }
    }
}