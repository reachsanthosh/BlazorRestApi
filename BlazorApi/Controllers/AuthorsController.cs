using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using AutoMapper;
using BlazorApi.Contracts;
using BlazorApi.Data;
using BlazorApi.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApi.Controllers
{

    /// <summary>
    /// End point to interact with Authors
    /// </summary>
    [Route("api/[controller]")]
    [ApiController] 
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorRepository"></param>
        /// <param name="logger"></param>
        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted to get all authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<List<AuthorDTO>>(authors);
                _logger.LogInfo("Received all authors");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message}-{ex.InnerException}");
            }
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something is wrong, please contact Admin");
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo("Attempting to get a specific Author");
                
                var author = await _authorRepository.FindById(id);
                if (author == null)
                    return NotFound();

                var response = _mapper.Map<AuthorDTO>(author);

                _logger.LogInfo("successfully got a specific Author");
                return Ok(response);
            }
            catch (Exception ex)
            {
              return  InternalError($"{ex.Message}- {ex.InnerException}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo($"Author Delete start");
                if (id < 1)
                {
                    _logger.LogInfo($"Author delete failed with Bad request");
                    return BadRequest();
                }

                var isExists = await _authorRepository.IsExists(id);

                if (!isExists)
                {
                    _logger.LogInfo($"Author delete record not found");
                    return NotFound();
                }

                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"Delete failed");
                }

                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message}-{e.InnerException}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author update start");
                if (id < 1 || authorDTO == null || id!= authorDTO.Id)
                {
                    _logger.LogWarning($"Author update failed with Bad data");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Author update failed with Metadata data");
                    return BadRequest(ModelState);
                }

                var isExists = await _authorRepository.IsExists(id);

                if (!isExists)
                {
                    _logger.LogInfo($"Author update record not found");
                    return NotFound();
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);

                if (!isSuccess)
                {
                    _logger.LogWarning($"Author update failed while posting data");
                    return InternalError($"Updated failed");
                }

                _logger.LogInfo($"Author update completed");
                return NoContent();
            }
            catch (Exception a)
            {
                return InternalError($"{a.Message} - {a.InnerException}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author creation is submitted");
                if (authorDTO == null)
                {
                    _logger.LogWarning($"Request was empty");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Request was incomplete");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);

                if (!isSuccess)
                {
                   return InternalError($"Author creation failed");
                }
                _logger.LogInfo($"Author created");
                return Created("Create", new {author});
            }
            catch (Exception a)
            {
                return InternalError($"{a.Message} - {a.InnerException}");
            }
        }

    }
}
    