using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetBooks()
        {
            var location = GetControllerNames();

            try
            {
                
                _logger.LogInfo($"{location} - get all Books");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location} - received all Books");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message}-{ex.InnerException}");
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetBook(int Id)
        {
            var location = GetControllerNames();

            try
            {
                _logger.LogInfo($"{location} - Attempted to call Book");
                var book = await _bookRepository.FindById(Id);

                if (book == null)
                {
                    _logger.LogWarning($"{location}: Failed to retrieve the Book {Id}");
                }

                var response = _mapper.Map<BookDTO>(book);

                
                _logger.LogInfo($"{location} - Successfully  received  Book {Id}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message}-{ex.InnerException}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO bookDTO)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location}: Book update start");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _logger.LogWarning($"{location} : Author update failed with Bad data");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"{location}: Book update failed with Metadata data");
                    return BadRequest(ModelState);
                }

                var isExists = await _bookRepository.IsExists(id);

                if (!isExists)
                {
                    _logger.LogInfo($"{location} : Book update record not found");
                    return NotFound();
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);

                if (!isSuccess)
                {
                    _logger.LogWarning($"{location}: Book update failed while posting data");
                    return InternalError($"Updated failed");
                }

                _logger.LogInfo($"{location} : Book update completed");
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
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerNames();

            try
            {
                _logger.LogInfo($"{location}: Book creation is submitted");
                if (bookDTO == null)
                {
                    _logger.LogWarning($"{location}:Request was empty");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"{location} : Request was incomplete");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);

                if (!isSuccess)
                {
                    return InternalError($"{location} :Book creation failed");
                }
                _logger.LogInfo($"{location} :Book created");
                return Created("Create", new { book });
            }
            catch (Exception a)
            {
                return InternalError($"{location} : {a.Message} - {a.InnerException}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Book Delete start");
                if (id < 1)
                {
                    _logger.LogInfo($"{location} : Book delete failed with Bad request");
                    return BadRequest();
                }

                var isExists = await _bookRepository.IsExists(id);

                if (!isExists)
                {
                    _logger.LogInfo($"Author delete record not found");
                    return NotFound();
                }

                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                {
                    return InternalError($"Delete failed");
                }

                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location} :{e.Message}-{e.InnerException}");
            }
        }

        private string GetControllerNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something is wrong, please contact Admin");
        }
    }
}
