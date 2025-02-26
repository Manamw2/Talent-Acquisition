namespace HrBackOffice.Helper.FileProcessingService
{
    // Processing Status Model
    public class ProcessingStatus
    {
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int SuccessfulFiles { get; set; }
        public int FailedFiles { get; set; }
        public int NeedsActionFiles { get; set; }
        public List<string> ProcessingErrors { get; set; } = new();
    }
}
