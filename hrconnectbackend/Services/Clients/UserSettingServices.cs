using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Repository;

namespace hrconnectbackend.Services.Clients
{
    public class UserSettingServices(DataContext context)
        : GenericRepository<UserSettings>(context), IUserSettingsServices
    {
        // Method to create default user settings
        public async Task CreateDefaultSettings(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);

            if (employee == null)
            {
                throw new KeyNotFoundException($"No employee found with an id {employeeId}");
            }

            var newSetting = new UserSettings
            {
                UserId = employeeId,
                Theme = "Light",
                Language = "en",  // Default language
                NotificationsEnabled = true,  // Default to notifications enabled
                Timezone = "UTC",  // Default timezone
                DateFormat = "yyyy-MM-dd",  // Default date format
                TimeFormat = "24h",  // Default time format
                PrivacyLevel = "medium",  // Default privacy level
                CreatedAt = DateTime.UtcNow,  // Set the creation timestamp
                UpdatedAt = DateTime.UtcNow, // Set the update timestamp
                IsTwoFactorEnabled = true,
                TwoFactorMethod = "email",  // Default method for 2FA
                TwoFactorSecret = null,
            };

            await AddAsync(newSetting);

        }

        public async Task ResetSettings(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);

            if (employee == null)
            {
                throw new KeyNotFoundException($"No employee found with an id {employeeId}");
            }

            var setting = await GetByIdAsync(employeeId);

            if (setting == null)
            {
                throw new KeyNotFoundException($"No setting found for employee with id {employeeId}");
            }

            // Reset settings to defaults
            setting.Theme = "Light";
            setting.Language = "en";  // Default language
            setting.NotificationsEnabled = true;  // Default to notifications enabled
            setting.Timezone = "UTC";  // Default timezone
            setting.DateFormat = "yyyy-MM-dd";  // Default date format
            setting.TimeFormat = "12h";  // Default time format
            setting.PrivacyLevel = "medium";  // Default privacy level
            setting.UpdatedAt = DateTime.UtcNow; // Only update timestamp, don't change CreatedAt
            setting.IsTwoFactorEnabled = true;
            setting.TwoFactorMethod = "email";  // Default method for 2FA
            setting.TwoFactorSecret = null;

            await UpdateAsync(setting);


        }
    }
}
