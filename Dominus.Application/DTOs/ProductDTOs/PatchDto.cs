using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class PatchProductDto
{
    [DefaultValue("")]

    public string? Name { get; set; }
    [DefaultValue("")]

    public string? Description { get; set; }
    [DefaultValue("")]

    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }
    [DefaultValue("")]

    [Range(0, int.MaxValue)]
    public int? CurrentStock { get; set; }

    public bool? IsActive { get; set; }
    public bool? TopSelling { get; set; }
    public bool? Status { get; set; }

    public List<int>? ColorIds { get; set; }
    [DefaultValue("")]

    public string? Warranty { get; set; }
}
