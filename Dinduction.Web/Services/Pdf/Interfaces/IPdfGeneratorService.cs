using Dinduction.Web.Models;

namespace Dinduction.Web.Services.Pdf.Interfaces
{
    public interface IPdfGeneratorService
    {
        Task<byte[]> GenerateTrainingResultPdfAsync(ViewRecordResultVM model);
    }
}