namespace HrBackOffice.Helper.FileProcessingService
{
    // Processing Status Model
    public class ProcessingStatus
    {
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int SuccessfulFiles { get; set; }
        public int FailedFiles { get; set; }
        public int CvExists { get; set; }
        public int EmailExists { get; set; }
        public int EmailNotValid { get; set; }
        public int ServerErrors { get; set; }
        public List<string> ProcessingErrors { get; set; } = new();
    }
}
