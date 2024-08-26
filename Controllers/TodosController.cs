using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoLibrary.DataAccess;
using TodoLibrary.Models;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodosController(ITodoData data, ILogger<TodosController> logger) : ControllerBase
{
    private readonly ITodoData _data = data;
    private readonly ILogger<TodosController> _logger = logger;

    private int GetUserId()
    {
        var nameIdentifier = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return (nameIdentifier is null) ? -1 : int.Parse(nameIdentifier);
    }

    // GET: api/Todos
    [HttpGet]
    public async Task<ActionResult<List<TodoModel>>> Get()
    {
        _logger.LogInformation("GET: api/Todos");

        try
        {
            var output = await _data.GetAllAssigned(GetUserId());

            return Ok(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The GET call to api/Todos failed.");
            return BadRequest();
        }
    }

    // GET api/Todos/5
    [HttpGet("{todoId}")]
    public async Task<ActionResult<TodoModel>> Get(int todoId)
    {
        _logger.LogInformation("GET: api/Todos/{TodoId}",todoId);

        try
        {
            var outpout = await _data.GetAssignedById(GetUserId(), todoId);

            return Ok(outpout);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, 
                "The GET call to {ApiPath} failed. The TodoId was {TodoId}",
                "api/Todos/Id",
                todoId);
            return BadRequest();
        }
    }

    // POST api/Todos
    [HttpPost]
    public async Task<ActionResult<TodoModel>> Post([FromBody] string task)
    {
        _logger.LogInformation("POST: api/Todos (Task: {Task})",task);

        try
        {
            var output = await _data.Create(GetUserId(), task);

            return Ok(output);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "The POST call to api/Todos failed. Task value was {Task}",task);
            return BadRequest();
        }
    }

    // PUT api/Todos/5
    [HttpPut("{todoId}")]
    public async Task<ActionResult> Put(int todoId, [FromBody] string task)
    {
        _logger.LogInformation("PUT: api/Todos/{TodoId} (Task: {Task})",todoId,task);

        try
        {
            await _data.UpdateTask(GetUserId(), todoId, task);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "The PUT call to {ApiPath} failed. The TodoId was {TodoId} (Task: {Task})",
                "api/Todos/Id",
                todoId,
                task);
            return BadRequest();
        }
    }

    // PUT api/Todos/5/Complete
    [HttpPut("{todoId}/Complete")]
    public async Task<ActionResult> Complete(int todoId)
    {
        _logger.LogInformation("PUT: api/Todos/{TodoID}/Complete",todoId);

        try
        {
            await _data.Complete(GetUserId(), todoId);

            return Ok();
        }
        catch (Exception ex)
        {

            _logger.LogError(ex,
                "The PUT call to {ApiPath} failed. The TodoId was {TodoId}",
                "api/Todos/Id/Complete",
                todoId);
            return BadRequest();
        }
    }

    // DELETE api/Todos/5
    [HttpDelete("{todoId}")]
    public async Task<ActionResult> Delete(int todoId)
    {
        _logger.LogInformation("DELETE: api/Todos/5");

        try
        {
            await _data.Delete(GetUserId(), todoId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "The DELETE call to {ApiPath} failed. The TodoId was {TodoId}",
                "api/Todos/Id",
                todoId);
            return BadRequest();
        }
    }
}
