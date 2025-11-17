namespace SEP490_BE.DAL.DTOs.Common
{
    public class BulkImportResultDto
    {
        public int Total { get; set; }
        public int Success { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
