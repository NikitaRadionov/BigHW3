using Microsoft.AspNetCore.Mvc;
using PaymentsService.DTOs;
using PaymentsService.UseCases.CreateAccount;
using PaymentsService.UseCases.Deposit;
using PaymentsService.UseCases.GetBalance;

namespace PaymentsService.Controllers
{
    [ApiController]
    [Route("accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly ICreateAccountService _createAccount;
        private readonly IDepositService _deposit;
        private readonly IGetBalanceService _getBalance;

        public AccountsController(
            ICreateAccountService createAccount,
            IDepositService deposit,
            IGetBalanceService getBalance)
        {
            _createAccount = createAccount;
            _deposit = deposit;
            _getBalance = getBalance;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount()
        {
            var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
            if (!Guid.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid or missing X-User-Id header.");

            var response = await _createAccount.CreateAccountAsync(
                new CreateAccountRequest(userId),
                HttpContext.RequestAborted);

            return response != null
                ? Ok(response)
                : BadRequest("Account already exists.");
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto dto)
        {
            var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
            if (!Guid.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid or missing X-User-Id header.");

            var response = await _deposit.DepositAsync(new DepositRequest(userId, dto.Amount), HttpContext.RequestAborted);

            return response != null
                ? Ok(response)
                : NotFound("Account not found.");
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
            if (!Guid.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid or missing X-User-Id header.");

            var response = await _getBalance.GetBalanceAsync(new GetBalanceRequest(userId), HttpContext.RequestAborted);

            return response != null
                ? Ok(response)
                : NotFound("Account not found.");
        }
    }
}