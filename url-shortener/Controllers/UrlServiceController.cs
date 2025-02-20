using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using url_shortener.Models;
using url_shortener.ModelsDTO;
using url_shortener.Repositories;
using url_shortener.Utilities;

namespace url_shortener.Controllers
{
    [ApiController]
    [Route("[controller]/v1")]
    public class UrlServiceController : Controller
    {
        public IUrlRepository UrlRepository { get; }

        private IMapper _mapper { get; }
        private static readonly Random random = new Random();

        public UrlServiceController(IUrlRepository urlRepository, IMapper mapper)
        {
            UrlRepository = urlRepository;
            _mapper = mapper;
        }

        [HttpGet("{shortUrl}")]
        [ProducesResponseType(typeof(LongUrlRepsonseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public IActionResult Get([FromRoute]string shortUrl = "")
        {
            var shortUrlDecoded = HttpUtility.UrlDecode(shortUrl);
            var url = UrlRepository.GetOne(x => string.Equals(x.ShortUrl, shortUrlDecoded));

            if (url == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<LongUrlRepsonseDTO>(url);

            return Ok(new DataResponseDTO<LongUrlRepsonseDTO>(response));
        }

        [HttpPost]
        [Route("shorten")]
        [ProducesResponseType(typeof(ShortUrlResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateErrorResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
        public IActionResult Post([FromBody]UrlRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDTO(System.Net.HttpStatusCode.BadRequest,
                    new string[] { AppConstants.INVALID_URL_ERROR_MESSAGE }));
            }

            if (!IsValidUrl(model.Url))
            {
                return BadRequest(new ErrorResponseDTO(System.Net.HttpStatusCode.BadRequest, 
                    new string[] { AppConstants.INVALID_URL_ERROR_MESSAGE }));
            }

            var newUrl = _mapper.Map<Url>(model);
            var isSuccess = GenerateShortUrl(newUrl);

            if (!isSuccess)
            {
                return BadRequest(new ErrorResponseDTO(HttpStatusCode.BadRequest,
                    new string[] { AppConstants.DEFAULT_ERROR_MESSAGE }));
            }

            var response = _mapper.Map<ShortUrlResponseDTO>(newUrl);

            return Ok(new DataResponseDTO<ShortUrlResponseDTO>(response));
        }

        private bool GenerateShortUrl(Url url) 
        {
            var existingUrl = UrlRepository.GetOne(x => string.Equals(x.LongUrl, url.LongUrl));
            if (existingUrl != null)
            {
                url.ShortUrl = existingUrl.ShortUrl;
                return true;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(url.LongUrl));
                int urlLength = random.Next(AppConstants.MIN_SHORTURL_LENGTH, AppConstants.MAX_SHORTURL_LENGTH + 1);
                StringBuilder shortUrlBuilder;
                string shortUrl;

                do
                {
                    shortUrlBuilder = new StringBuilder();

                    for (int i = 0; i < urlLength; i++)
                    {
                        // Combine hash byte and randomness to reduce collisions
                        shortUrlBuilder.Append(AppConstants.BASECHARS[(hashBytes[i] + random.Next(AppConstants.BASECHARS.Length)) % AppConstants.BASECHARS.Length]);
                    }

                    // If shortUrl should be formatted as a url
                    //shortUrl = AppConstants.BASE_URL + shortUrlBuilder.ToString();

                    shortUrl = shortUrlBuilder.ToString();
                } while (UrlRepository.Get(x => string.Equals(x.ShortUrl, shortUrl)).Any());

                url.ShortUrl = shortUrl;

                try
                {
                    UrlRepository.Insert(url);
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            }
        }

        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && 
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
