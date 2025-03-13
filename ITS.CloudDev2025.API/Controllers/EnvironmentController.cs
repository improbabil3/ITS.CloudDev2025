using ITS.CloudDev2025.API.DTOs;
using ITS.CloudDev2025.API.Input;
using Microsoft.AspNetCore.Mvc;

namespace ITS.CloudDev2025.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly ILogger _logger;

        public EnvironmentController(ILogger<EnvironmentController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieve the list of environment variables
        /// </summary>
        /// <returns>The list of environment variables as an object of type <see cref="EnvVariableDto"/>EnvVariableDto</see></returns>
        [HttpGet("all")]
        [ProducesResponseType<List<EnvVariableDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetEnvValues()
        {
            _logger.LogInformation($"Enter {nameof(GetEnvValues)}");

            List<EnvVariableDto> result = [];
            foreach (string key in Environment.GetEnvironmentVariables().Keys)
            {
                // No need to handle the casing (uppercase and lowercase) of the keys
                // because I read them directly from the environment variables
                result.Add(
                    new() { 
                        Key = key, 
                        Value = Environment.GetEnvironmentVariable(key) 
                    });
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieve the value of a specific environment variable.
        /// </summary>
        /// <param name="keyName">The identifier of the environment variable to be retrieved.</param>
        /// <returns>The value of the found variable.</returns>
        [HttpGet("search/{name}")]
        [ProducesResponseType<List<EnvVariableDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]        
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetEvnValue([FromRoute] string keyName)
        {
            _logger.LogInformation($"Enter {nameof(GetEvnValue)}");

            // Input validation
            if (string.IsNullOrWhiteSpace(keyName))
                return BadRequest("Key name is invalid.");

            // Check if key exists
            if (!Environment.GetEnvironmentVariables().Contains(keyName))
                return NotFound("Key does not exist.");

            return Ok(Environment.GetEnvironmentVariable(keyName));
        }

        /// <summary>
        /// Retrieve the value of a specific environment variable.
        /// </summary>
        /// <param name="keyName">The identifier of the environment variable to be retrieved.</param>
        /// <returns>The object of type <see cref="EnvVariableDto">EnvVariableDto</see> containing the searched variable with its respective value. </returns>
        [HttpGet("searchinsensitive/{name}")]
        [ProducesResponseType<List<EnvVariableDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetEvnValueCaseInsensitive([FromRoute] string keyName)
        {
            /* 
             * In the previous example, the handling of the fact that environment variables (in Windows) might exist 
             * but with different casing (uppercase/lowercase characters) is missing, leading to potential errors.
             * Below is a slightly more robust example with proper string handling.
             */
            _logger.LogInformation($"Enter {nameof(GetEvnValueCaseInsensitive)}");

            // Input validation
            if (string.IsNullOrWhiteSpace(keyName))
                return BadRequest("Key name is invalid.");

            // Check if key exists ignoring case
            if (!Environment.GetEnvironmentVariables().Keys
                            .OfType<string>()
                            .Any(x => x.Equals(keyName, StringComparison.InvariantCultureIgnoreCase)))
                return NotFound("Key does not exist.");
            
            // Get the actual key value stored in the environment variable dictionary
            string searchedKey = Environment.GetEnvironmentVariables().Keys
                                            .OfType<string>()
                                            .Where(x => x.Equals(keyName, StringComparison.InvariantCultureIgnoreCase))
                                            .Single();
            
            EnvVariableDto result = new() { Key = searchedKey, Value = Environment.GetEnvironmentVariable(searchedKey) };

            return Ok(result);
        }

        /// <summary>
        /// Add a new env variable to list
        /// </summary>
        /// <param name="envVariable">The env variable to add (key and value)</param>
        /// <returns>Variable successfully created</returns>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult AddNewEnvVariable([FromBody] EnvVariableInput envVariable)
        {
            _logger.LogInformation($"Enter {nameof(AddNewEnvVariable)}");

            // Input validation
            if (string.IsNullOrWhiteSpace(envVariable.Key))
                return BadRequest("Key name is invalid.");

            if (string.IsNullOrWhiteSpace(envVariable.Value))
                return BadRequest("Value is invalid.");

            // Check if key exists (remember the previous comment for manage casing - not managed in this case)
            if (Environment.GetEnvironmentVariables().Contains(envVariable.Key))
                return StatusCode(StatusCodes.Status409Conflict, "Key already exist.");

            Environment.SetEnvironmentVariable(envVariable.Key, envVariable.Value);

            // If the set create an unique id (for example, create a new recor on a DB), return http Status 200 - Ok and the id of the object created
            return Created();
        }
    }
}
