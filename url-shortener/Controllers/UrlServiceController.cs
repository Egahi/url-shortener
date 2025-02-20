using System.Reflection;
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

        public UrlServiceController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(UrlRepsonseDTO), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery]string shortUrl = "")
        {
            var response = new UrlRepsonseDTO 
            { 
                Url = shortUrl
            };

            return Ok(new DataResponseDTO<UrlRepsonseDTO>(response));
        }

        [HttpPost]
        [Route("shorten")]
        [ProducesResponseType(typeof(UrlRepsonseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateErrorResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
        public IActionResult Post([FromBody]UrlRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ModelStateErrorResponseDTO(System.Net.HttpStatusCode.BadRequest, ModelState));
            }

            Url newUrl = _mapper.Map<Url>(model);
            var response = _mapper.Map<UrlRepsonseDTO>(newUrl);

            return Ok(new DataResponseDTO<UrlRepsonseDTO>(response));
        }
    }
}
