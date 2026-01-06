namespace Dominus.Application.DTOs.ProductDTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
        public bool InStock { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public int CurrentStock { get; set; }
        public bool TopSelling { get; set; } = true;
        public List<string> AvailableColors { get; set; } = new();
        public List<string> DeactivatedColors { get; set; } = new();
        public List<string> Images { get; set; } = new();
        public string Warranty { get; set; } = "";

    }
}
