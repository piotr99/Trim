namespace Trim.Models.DTOs
{
    public class CaseDTO
    {
        public string MessageContent { get; set; }
        public DateTime SentAt { get; set; }
        public string Direction { get; set; }
        public string SenderName { get; set; }
        public bool IsPrivate { get; set; }
    }
    public class ChangeSalesmanDto
    {
        public int SalespersonId { get; set; }
    }

    public class UpdateDescriptionDto
    {
        public string Description { get; set; }
    }

    public class SaveCommentDTO
    {
        public int CaseId { get; set; }
        public string MessageContent { get; set; }
        public bool IsPrivateMessage { get; set; }
    }
}
