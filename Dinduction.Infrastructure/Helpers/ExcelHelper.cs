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
        public static async Task UploadUserAsync(Stream excelStream, IUserService userService)
        {
            if (excelStream == null) throw new ArgumentNullException(nameof(excelStream));
            if (userService == null) throw new ArgumentNullException(nameof(userService));

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return;

                var rowCount = worksheet.Dimension.End.Row;
                var colCount = worksheet.Dimension.End.Column;
                var users = new List<User>();

                // assume first row header -> start at row 2
                for (int r = 2; r <= rowCount; r++)
                {
                    var username = worksheet.Cells[r, 1].GetValue<string>()?.Trim();
                    var employeeName = worksheet.Cells[r, 2].GetValue<string>()?.Trim();

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(employeeName))
                        continue;

                    var department = colCount >= 3 ? worksheet.Cells[r, 3].GetValue<string>()?.Trim() : null;

                    var user = new User
                    {
                        UserName = username,
                        Password = username, // legacy behaviour
                        RoleId = 2,
                        EmployeeName = employeeName,
                        StartTraining = DateOnly.FromDateTime(DateTime.Today),
                        EndTraining = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                        Department = department
                    };

                    users.Add(user);
                }

                foreach (var user in users)
                {
                    // use the new IUserService methods
                    var userExist = await userService.GetByUserNameAsync(user.UserName);
                    if (userExist != null)
                    {
                        userExist.StartTraining = user.StartTraining;
                        userExist.EndTraining = user.EndTraining;
                        await userService.UpdateAsync(userExist);
                    }
                    else
                    {
                        await userService.InsertAsync(user);
                    }
                }
            }
        }

        // optional legacy wrapper (blocking) — use only if necessary
        public static void UploadUser(Stream excelStream, IUserService userService)
            => UploadUserAsync(excelStream, userService).GetAwaiter().GetResult();
    }
}