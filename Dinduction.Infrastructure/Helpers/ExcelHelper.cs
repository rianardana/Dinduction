using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;
using OfficeOpenXml;

namespace Dinduction.Infrastructure.Helpers
{
    public static class ExcelHelper
    {
        public static async Task UploadUserAsync(Stream excelStream, IUserService userService, IPasswordService passwordService)
        {
            if (excelStream == null) throw new ArgumentNullException(nameof(excelStream));
            if (userService == null) throw new ArgumentNullException(nameof(userService));
            if (passwordService == null) throw new ArgumentNullException(nameof(passwordService));

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return;

                var rowCount = worksheet.Dimension.End.Row;
                var colCount = worksheet.Dimension.End.Column;
                var users = new List<User>();

                for (int r = 2; r <= rowCount; r++)
                {
                    var username = worksheet.Cells[r, 1].GetValue<string>()?.Trim();
                    var employeeName = worksheet.Cells[r, 2].GetValue<string>()?.Trim();

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(employeeName))
                        continue;

                    var department = colCount >= 3 ? worksheet.Cells[r, 3].GetValue<string>()?.Trim() : null;
                    
                    // ✅ Hash password SEKALI saja
                    var plainPassword = username;
                    var hashedPassword = passwordService.HashPassword(plainPassword);
                    
                    // Debug output (cek di Output window VS Code)
                    System.Diagnostics.Debug.WriteLine($"Username: {username}");
                    System.Diagnostics.Debug.WriteLine($"Plain: {plainPassword}");
                    System.Diagnostics.Debug.WriteLine($"Hashed: {hashedPassword}");
                    System.Diagnostics.Debug.WriteLine($"Hash Length: {hashedPassword?.Length}");
                    
                    var trainingType = "I"; // Default value
                    if (colCount >= 4)
                    {
                        var rawValue = worksheet.Cells[r, 4].GetValue<string>()?.Trim()?.ToUpper();
                        if (!string.IsNullOrEmpty(rawValue) && (rawValue == "I" || rawValue == "R"))
                        {
                            trainingType = rawValue;
                        }
                    }

                    var user = new User
                    {
                        UserName = username,
                        Password = hashedPassword, // ✅ Pakai variable yang sudah di-hash
                        RoleId = 2, // Default User role
                        EmployeeName = employeeName,
                        StartTraining = DateOnly.FromDateTime(DateTime.Today),
                        EndTraining = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                        Department = department,
                        TrainingType = trainingType
                    };

                    users.Add(user);
                } // ✅ Closing brace for 'for' loop

                foreach (var user in users)
                {
                    var userExist = await userService.GetByUserNameAsync(user.UserName);
                    if (userExist != null)
                    {
                        userExist.StartTraining = user.StartTraining;
                        userExist.EndTraining = user.EndTraining;
                        userExist.Department = user.Department;
                        userExist.TrainingType = user.TrainingType;
                        userExist.Password = user.Password; // ✅ UPDATE PASSWORD untuk user existing!
                        await userService.UpdateAsync(userExist);
                    }
                    else
                    {
                        await userService.InsertAsync(user);
                    }
                } // ✅ Closing brace for 'foreach' loop

            } // ✅ Closing brace for 'using' block
        } // ✅ Closing brace for UploadUserAsync method

        // Optional legacy wrapper (blocking)
        public static void UploadUser(Stream excelStream, IUserService userService, IPasswordService passwordService)
            => UploadUserAsync(excelStream, userService, passwordService).GetAwaiter().GetResult();
        
    } // ✅ Closing brace for ExcelHelper class
} // ✅ Closing brace for namespace