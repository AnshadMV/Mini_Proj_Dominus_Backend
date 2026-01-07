using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Entities.VideoServices
{
    public class VideoService
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string VideoUrl { get; set; } = null!;
        public string LearnMoreLink { get; set; } = null!;
        public string? Thumbnail { get; set; }
        public bool IsFeatured { get; set; }
    }
}
