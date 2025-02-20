﻿using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        private IUrlRepository _urlRepository { get; }
        private IMemoryCache _cache { get; } 
        private IMapper _mapper { get; }
        private static readonly Random random = new Random();

        public UrlServiceController(IUrlRepository urlRepository, 
            IMapper mapper, IMemoryCache cache)
        {
            _urlRepository = urlRepository;
            _cache = cache;
            _mapper = mapper;
        }

        [HttpGet("{shortUrl}")]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute]string shortUrl = "")
        {
            var shortUrlDecoded = HttpUtility.UrlDecode(shortUrl);

            var url = await GetUrl(shortUrlDecoded);

            if (url == null)
            {
                return NotFound();
            }

            url.AccessCount++;
            _urlRepository.Update(x => string.Equals(x.ShortUrl, url.ShortUrl), url); ;
            
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _cache.Set(shortUrl, url, cacheEntryOptions);

            var response = _mapper.Map<LongUrlRepsonseDTO>(url);

            return Redirect(url.LongUrl);
        }

        [HttpGet("stats/{shortUrl}")]
        [ProducesResponseType(typeof(URLStatsResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStats([FromRoute] string shortUrl = "")
        {
            var shortUrlDecoded = HttpUtility.UrlDecode(shortUrl);

            var url = await GetUrl(shortUrlDecoded);

            if (url == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<URLStatsResponseDTO>(url);

            return Ok(new DataResponseDTO<URLStatsResponseDTO>(response));
        }

        [HttpPost]
        [Route("shorten")]
        [ProducesResponseType(typeof(ShortUrlResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateErrorResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody]UrlRequestDTO model)
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
            var isSuccess = await GenerateShortUrl(newUrl);

            if (!isSuccess)
            {
                return BadRequest(new ErrorResponseDTO(HttpStatusCode.BadRequest,
                    new string[] { AppConstants.DEFAULT_ERROR_MESSAGE }));
            }

            var response = _mapper.Map<ShortUrlResponseDTO>(newUrl);

            return Ok(new DataResponseDTO<ShortUrlResponseDTO>(response));
        }

        private async Task<bool> GenerateShortUrl(Url url) 
        {
            var existingUrl = _urlRepository.GetOne(x => string.Equals(x.LongUrl, url.LongUrl));
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
                } while (_urlRepository.Get(x => string.Equals(x.ShortUrl, shortUrl)).Any());

                url.ShortUrl = shortUrl;

                try
                {
                    _urlRepository.Insert(url);
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

        private async Task<Url> GetUrl(string shortUrl)
        {
            if (!_cache.TryGetValue(shortUrl, out Url url))
            {
                // Key not in cache, so get data from database
                url = _urlRepository.GetOne(x => string.Equals(x.ShortUrl, shortUrl));
            }

            return url;
        }
    }
}
