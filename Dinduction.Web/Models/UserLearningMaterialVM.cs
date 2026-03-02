// File: Dinduction.Web/Models/UserLearningMaterialVM.cs
namespace Dinduction.Web.Models;

public class UserLearningMaterialVM
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? FilePath { get; set; }
    
    // ✅ Computed: file extension
    public string? FileType => GetFileType();
    
    // ✅ Progress
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    // ✅ Helper: icon based on file type
    public string DisplayIcon => GetIconClass();
    
    private string? GetFileType()
    {
        if (string.IsNullOrEmpty(FilePath)) return null;
        return Path.GetExtension(FilePath)?.ToLower().TrimStart('.');
    }
    
    private string GetIconClass()
    {
        return FileType switch
        {
            "pdf" => "📄",
            "mp4" or "webm" or "mov" => "🎬",
            "jpg" or "jpeg" or "png" => "🖼️",
            _ => "📎"
        };
    }
}