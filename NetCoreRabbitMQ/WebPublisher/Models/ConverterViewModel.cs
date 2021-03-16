using Microsoft.AspNetCore.Http;

namespace WebPublisher.Models
{
    public class ConverterViewModel
    {
        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Word file
        /// </summary>
        public IFormFile File { get; set; }
    }
}
