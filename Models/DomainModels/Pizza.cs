namespace CrudeApi.Models.DomainModels
{
    public class Pizza
    {
        public Guid Id { get; set; }
        public string? SizeID { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageName { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}
