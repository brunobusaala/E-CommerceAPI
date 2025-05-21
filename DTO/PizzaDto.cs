namespace CrudeApi.DTO
{
    public class PizzaDto
    {
        public Guid Id { get; set; }

        public string? SizeID { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageName { get; set; }

    }
}
