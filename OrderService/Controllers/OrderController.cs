using Microsoft.AspNetCore.Mvc;
using OrderService.UseCases.CreateOrder;
using OrderService.UseCases.GetUserOrders;
using OrderService.UseCases.GetOrderById;
using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;

namespace OrdersService.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly ICreateOrderService _createOrderService;
    private readonly IGetUserOrdersService _getUserOrdersService;
    private readonly IGetOrderByIdService _getOrderByIdService;

    public OrdersController(
        ICreateOrderService createOrderService,
        IGetUserOrdersService getUserOrdersService,
        IGetOrderByIdService getOrderByIdService)
    {
        _createOrderService = createOrderService;
        _getUserOrdersService = getUserOrdersService;
        _getOrderByIdService = getOrderByIdService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TryGetUserId(out var userId))
            return BadRequest("Missing or invalid X-User-Id header");

        try
        {
            var response = await _createOrderService.CreateOrderAsync(new CreateOrderRequest(userId, dto.Amount, dto.Description), cancellationToken);
            return Ok(response);
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "Database update failed");
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return BadRequest("Missing or invalid X-User-Id header");

        var response = await _getUserOrdersService.GetOrdersAsync(new GetUserOrdersRequest(userId), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return BadRequest("Missing or invalid X-User-Id header");

        var response = await _getOrderByIdService.GetOrderByIdAsync(new GetOrderByIdRequest(id, userId), cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var header = Request.Headers["X-User-Id"].FirstOrDefault();
        return Guid.TryParse(header, out userId);
    }
}