using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using url_shortener.Models;
using url_shortener.ModelsDTO;

namespace url_shortener.Controllers
{
    [ApiController]
    [Route("[controller]/v1")]
    public class UrlServiceController : Controller
    {
        private IMapper _mapper { get; }
        private static readonly Dictionary<string, Url> _urlDictionary = new Dictionary<string, Url>();
        private static readonly Dictionary<string, string> _reverseDictionary = new Dictionary<string, string>();
        private static readonly string _baseChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random random = new Random();
        private static readonly int _minShortUrlLength = 6;
        private static readonly int _maxShortUrlLength = 8;
        private static readonly string _baseUrl = "short.url/";

        public UrlServiceController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(LongUrlRepsonseDTO), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery]string shortUrl = "")
        {
            var url = _urlDictionary.ContainsKey(shortUrl) ? _urlDictionary[shortUrl] : new Url();

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
                return BadRequest(new ModelStateErrorResponseDTO(System.Net.HttpStatusCode.BadRequest, ModelState));
            }

            if (!IsValidUrl(model.Url))
            {
                return BadRequest(new ErrorResponseDTO(System.Net.HttpStatusCode.BadRequest, new string[] { "Invalid url" }));
            }

            Url newUrl = _mapper.Map<Url>(model);
            GenerateShortUrl(newUrl);
            var response = _mapper.Map<ShortUrlResponseDTO>(newUrl);

            return Ok(new DataResponseDTO<ShortUrlResponseDTO>(response));
        }

        private static void GenerateShortUrl(Url url) 
        {
            if (_reverseDictionary.ContainsKey(url.LongUrl))
            {
                url.ShortUrl = _reverseDictionary[url.LongUrl];
                return;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(url.LongUrl));
                int urlLength = random.Next(_minShortUrlLength, _maxShortUrlLength + 1);
                StringBuilder shortUrlBuilder;
                string shortUrl;

                do
                {
                    shortUrlBuilder = new StringBuilder();

                    for (int i = 0; i < urlLength; i++)
                    {
                        // Combine hash byte and randomness to reduce collisions
                        shortUrlBuilder.Append(_baseChars[(hashBytes[i] + random.Next(_baseChars.Length)) % _baseChars.Length]);
                    }

                    shortUrl = _baseUrl + shortUrlBuilder.ToString();
                } while (_urlDictionary.ContainsKey(shortUrl));


                url.ShortUrl = shortUrl;
                _urlDictionary[url.ShortUrl] = url;
                _reverseDictionary[url.LongUrl] = url.ShortUrl;
            }
        }

        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && 
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
