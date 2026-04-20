// Dinduction.Web/Services/Pdf/Implementations/QuestPdfGeneratorService.cs

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Dinduction.Web.Services.Pdf.Interfaces;
using Dinduction.Web.Services.Pdf.Documents;
using Dinduction.Web.Models;

namespace Dinduction.Web.Services.Pdf.Implementations
{
    public class QuestPdfGeneratorService : IPdfGeneratorService
    {
        public QuestPdfGeneratorService()
        {
            // ✅ Set license (Community = free for most use cases)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<byte[]> GenerateTrainingResultPdfAsync(ViewRecordResultVM model)
        {
            return Task.Run(() =>
            {
                var document = new TrainingResultDocument(model);
                
                return document.GeneratePdf();
            });
        }
    }
}