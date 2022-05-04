namespace UserManagement.Models
{
    public class Response
    {
        public string Status { set; get; }
        public string Message { set; get; }

        public Users? User { set; get; }
    }
}
