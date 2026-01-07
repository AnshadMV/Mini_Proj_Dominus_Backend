namespace Dominus.Application.DTOs.VideoServices
{
    public class CreateVideoServiceDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string VideoUrl { get; set; } = null!;
        public string LearnMoreLink { get; set; } = null!;
        public string? Thumbnail { get; set; }
        public bool IsFeatured { get; set; }
    }
}
