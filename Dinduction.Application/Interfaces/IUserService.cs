
using Dinduction.Domain.Entities;

namespace Dinduction.Application.Interfaces;

public interface IUserService
{
    Task<User?> LoginAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetAdminTrainerAsync();
    Task<List<User>> GetWeeklyAsync();
    Task<List<User>> GetTrainerAsync();
    Task<List<User>> GetListParticipantAsync();
    //Task<List<User>> GetNotPresentAsync();

    Task<int> GetUserIdAsync(string username);
    Task<string?> GetUserNameAsync(string username);
    Task<string?> GetBadgeNumberAsync(string username);
    Task<string?> GetUserNameByIdAsync(int trainerId);

    Task<bool> InsertUserAsync(User user);
    Task InsertAsync(User obj);
    Task UpdateAsync(User obj);
    Task DeleteAsync(int id);

    Task<int> CountUserTodayAsync();
    Task<int> CountUserByDateAsync(DateOnly date); 
    Task<User?> GetPasswordAsync(string username);

    Task<User?> GetByUserNameAsync(string userName);
    Task<string> GetBadgeNumberByIdAsync(int userId);
    

}