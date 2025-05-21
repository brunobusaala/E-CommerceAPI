namespace CrudeApi.Models.RequestModels
{
    public class EditPizza
    {
        public string? SizeID { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageName { get; set; }
    }
}
