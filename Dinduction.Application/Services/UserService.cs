// Dinduction.Application/Services/UserService.cs
using Dinduction.Application.Interfaces;

using System.Linq.Expressions;
using Dinduction.Domain.Entities;

namespace Dinduction.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _uow.Repository<User>().GetAllAsync(orderBy: u => u.UserName);
    }

    public async Task<List<User>> GetAdminTrainerAsync()
    {
        return await _uow.Repository<User>().GetAllAsync(
            predicate: u => u.RoleId != 2,
            orderBy: u => u.UserName);
    }

    public async Task<List<User>> GetWeeklyAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var maxDate = today.AddDays(7);

        Expression<Func<User, bool>> predicate = u =>
            u.StartTraining.HasValue && u.StartTraining >= today && u.StartTraining <= maxDate ||
            u.EndTraining.HasValue && u.EndTraining >= today && u.EndTraining <= maxDate;

        return await _uow.Repository<User>().GetAllAsync(
            predicate: predicate,
            orderBy: u => u.UserName);
    }
    public async Task<int> GetUserIdAsync(string username)
    {
        var user = await _uow.Repository<User>().GetAsync(u => u.UserName == username);
        return user?.Id ?? 0;
    }

    public async Task<string?> GetUserNameAsync(string username)
    {
        var user = await _uow.Repository<User>().GetAsync(u => u.UserName == username);
        return user?.EmployeeName;
    }

    public async Task<string?> GetBadgeNumberAsync(string username)
    {
        var user = await _uow.Repository<User>().GetAsync(u => u.UserName == username);
        return user?.UserName;
    }

    public async Task<User?> LoginAsync(string username)
    {
        return await _uow.Repository<User>().GetAsync(u => u.UserName == username);
    }

    public async Task UpdateAsync(User obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<User>().Update(obj);
        _uow.SaveChanges();
    }

    public async Task InsertAsync(User obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<User>().Add(obj);
        _uow.SaveChanges();
    }

    public async Task<bool> InsertUserAsync(User user)
    {
        var existing = await _uow.Repository<User>().GetAsync(u => u.UserName == user.UserName);
        if (existing != null) return false;

        _uow.Repository<User>().Add(user);
        _uow.SaveChanges();
        return true;
    }

    public async Task<List<User>> GetTrainerAsync()
    {
        return await _uow.Repository<User>().GetAllAsync(
            predicate: u => u.RoleId == 3,
            orderBy: u => u.UserName);
    }

    public async Task<List<User>> GetListParticipantAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);
        var dayAfterTomorrow = today.AddDays(2);

        Expression<Func<User, bool>> predicate = u =>
            u.StartTraining.HasValue && u.StartTraining <= dayAfterTomorrow &&
            u.EndTraining.HasValue && u.EndTraining >= yesterday;

        return await _uow.Repository<User>().GetAllAsync(
            predicate: predicate,
            orderBy: u => u.EmployeeName);
    }

    public async Task<string?> GetUserNameByIdAsync(int trainerId)
    {
        var user = await _uow.Repository<User>().GetByIdAsync(trainerId);
        return user?.EmployeeName;
    }

    public async Task<int> CountUserTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);
        var tomorrow = today.AddDays(1);

        Expression<Func<User, bool>> predicate = u =>
            (u.StartTraining == today || u.StartTraining == yesterday || u.StartTraining == tomorrow) ||
            u.EndTraining == today;

        return await _uow.Repository<User>().CountAsync(predicate);
    }

    // public async Task<List<User>> GetNotPresentAsync()
    // {
    //     var today = DateTime.Today;
    //     var allUser = await _uow.Repository<User>().GetAllAsync(u => u.StartTraining == today);
    //     var presenceIds = await _uow.Repository<ParticipantUser>()
    //         .GetAllAsync()
    //         .ContinueWith(t => t.Result.Select(p => p.UserId).ToList());

    //     return allUser.Where(u => !presenceIds.Contains(u.Id)).ToList();
    // }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _uow.Repository<User>().GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        if (id == 0) throw new ArgumentException("ID cannot be zero.", nameof(id));
        var user = await _uow.Repository<User>().GetByIdAsync(id);
        if (user != null)
        {
            _uow.Repository<User>().Delete(user);
            _uow.SaveChanges();
        }
    }

    public async Task<int> CountUserByDateAsync(DateOnly date)
    {
        var yesterday = date.AddDays(-1);
        var tomorrow = date.AddDays(1);

        Expression<Func<User, bool>> predicate = u =>
            (u.StartTraining == date || u.StartTraining == yesterday || u.StartTraining == tomorrow) ||
            u.EndTraining == date;

        return await _uow.Repository<User>().CountAsync(predicate);
    }

    public async Task<User?> GetPasswordAsync(string username)
    {
        return await _uow.Repository<User>().GetAsync(u => u.Password == username);
    }
    public async Task<User?> GetByUserNameAsync(string userName)
    {
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            return await _uow.Repository<User>().GetAsync(u => u.UserName == userName);
    }



}