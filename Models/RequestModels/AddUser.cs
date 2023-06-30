namespace CrudeApi.Models.RequestModels
{
    public class AddUser
    {
        public string? UserName { get; set; }

        public string? EmailAddress { get; set; }

        public string? Password { get; set; }
    }
}
