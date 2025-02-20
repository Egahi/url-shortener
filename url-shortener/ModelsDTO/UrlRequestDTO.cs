using System.ComponentModel.DataAnnotations;

namespace url_shortener.ModelsDTO
{
    public class UrlRequestDTO
    {
        [Required]
        public string Url { get; set; }
    }
}
