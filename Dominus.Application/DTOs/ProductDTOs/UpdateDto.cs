using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class UpdateProductDto
{
    [DefaultValue("")]
    [Required]
    public int Id { get; set; }

    [DefaultValue("")]
    [Required]
    public string Name { get; set; } = null!;

    [DefaultValue("")]
    [Required]
    public string Description { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    [DefaultValue("")]
    [Range(0, int.MaxValue)]
    public int CurrentStock { get; set; }

    [MinLength(1, ErrorMessage = "At least one color is required")]
    public List<int> ColorIds { get; set; } = new();

    public bool IsActive { get; set; }
    public bool TopSelling { get; set; }
    public bool Status { get; set; }
    [DefaultValue("")]
    public string? Warranty { get; set; }
}
