namespace LibraryManagement.Services.Notifications
{
    public static class PhoneNumbers
    {
        // "9876543210" -> "919876543210" (digits only, with country code). Numbers that already have one are kept.
        public static string WithCountryCode(string mobile, string defaultCountryCode)
        {
            var digits = new string(mobile.Where(char.IsDigit).ToArray());
            if (digits.Length == 0)
            {
                throw new NotificationSendException($"'{mobile}' is not a phone number.", permanent: true);
            }

            if (digits.Length == 10)
            {
                return defaultCountryCode + digits;
            }
            if (digits.Length == 11 && digits.StartsWith('0'))
            {
                return defaultCountryCode + digits[1..];
            }
            return digits;
        }

        // "+919876543210"
        public static string ToE164(string mobile, string defaultCountryCode) => "+" + WithCountryCode(mobile, defaultCountryCode);
    }
}
