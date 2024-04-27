using ConnextX.API.Contracts;
using ConnextX.API.Data.Dtos;
using ConnextX.API.Data.Models;
using ConnextX.API.Data.Models.DbContext;
using ConnextX.API.Data.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnextX.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly Helper _helper;

        public AccountController(IAuthManager authManager
            , IConfiguration configuration,
            ApplicationDbContext dbContext,
            Helper helper)
        {
            _authManager = authManager;
            _configuration = configuration;
            _dbContext = dbContext;
            _helper = helper;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RegisterAsync([FromBody] ApiUserDto userDto)
        {
            var errors = await _authManager.Register(userDto);
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            return Ok();
        }


        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var authResponse = await _authManager.Login(loginDto);
            if (authResponse == null)
            {
                return Unauthorized();
            }
            return Ok(authResponse);
        }
        private string GetUserIdByUsername(string username)
        {
            return _dbContext.Users
                .Where(u => u.UserName == username)
                .Select(u => u.Id)
                .FirstOrDefault();
        }


        [HttpPost]
        [Route("subscribe")]
        public async Task<IActionResult> Subscribe(int userSelectionPlanId, [FromHeader] string Token)
        {
            var loggedInUser = _authManager.VerifyJwt(Token);
            var userId = GetUserIdByUsername(loggedInUser).ToString();
            if (userId == null)
            {
                return Unauthorized();
            }

            //// Retrieve plan details including price
            //var selectedPlan = await _dbContext.Plans.FindAsync(userSelectionPlanId);
            //if (selectedPlan == null)
            //{
            //    return NotFound("Plan not found.");
            //}

            //// Here, you would integrate the payment mechanism
            //var paymentResult = await PaymentService.ProcessPayment(userId, selectedPlan.Price);
            //if (!paymentResult.Success)
            //{
            //    return BadRequest("Payment failed. Please try again.");
            //}
            CurrentSubscription subscription = await GetCurrentSubscriptionOnlyWithuserId(userId);
            if (subscription != null)
            { 
                ArchivedSubscriptions archivedSubscription = new ArchivedSubscriptions
                {
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    PlanId = subscription.PlanId,
                    Quota = subscription.Quota,
                    UserId = subscription.UserId
                };
                await _dbContext.ArchivedSubscriptions.AddAsync(archivedSubscription);
                _dbContext.CurrentSubscriptions.Remove(subscription);
            }
            Plan plan = await _dbContext.Plans.FindAsync(userSelectionPlanId);
            var _subscription = new CurrentSubscription
            {
                StartDate = DateTime.Now,
                PlanId = userSelectionPlanId,
                EndDate = DateTime.Now.AddDays((double)_dbContext.Plans
                .Where(X => X.PlanId == userSelectionPlanId)
                .Select(X => X.Duration)
                .FirstOrDefault()),
                Quota = _dbContext.Plans
                .Where(X => X.PlanId == userSelectionPlanId).Select(X => X.Quota).FirstOrDefault(),
                UserId = userId,
            };
            await _dbContext.CurrentSubscriptions.AddAsync(_subscription);
            await _dbContext.SaveChangesAsync();
            var subdto = new subscriptionDto
            {
                StartDate = _subscription.StartDate,
                EndDate = _subscription.EndDate,
                userId = _subscription.UserId,
                Quota = _subscription.Quota,
                PlanId = _subscription.PlanId
            };
            return Ok(new {Subscribtion = subdto , Plan = plan  });
        }
        private async Task<CurrentSubscription> GetCurrentSubscriptionOnlyWithuserId(string userId)
        {
            var subscription = _dbContext.CurrentSubscriptions
                .Where(q => q.UserId == userId)
                .OrderBy(q => q.StartDate)
                .FirstOrDefault();
            return subscription;
        }


        [HttpGet]
        [Route("subcription")]
        public async Task<IActionResult> VerifyTokenGetUserReturnSubscriptionAndPlanByToken([FromHeader] string Token)
        {
            try
            {
                var loggedInUser = _authManager.VerifyJwt(Token);
                if (loggedInUser == null)
                {
                    return Unauthorized();
                }
                var userId = GetUserIdByUsername(loggedInUser).ToString();
                var subscriptionWithPlan = await GetCurrentSubscriptionWithPlanByUserId(userId);

                if (subscriptionWithPlan == null)
                {
                    return NotFound();
                }
                return Ok(subscriptionWithPlan);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message.ToString());
            }

        }
        private async Task<object> GetCurrentSubscriptionWithPlanByUserId(string userId)
        {
            // This will eagerly load the associated plan
            var subscriptionWithPlan = _dbContext.CurrentSubscriptions.Where(X => X.UserId == userId).Include(s => s.Plan).FirstOrDefaultAsync(); 
            return subscriptionWithPlan;
        }

        [HttpGet]
        [Route("currentandarchivedsubs")]
        public async Task<IActionResult> GetCurrentAndAcrhivedSubscriptions([FromHeader] string Token)
        {
            try
            {
                var loggedInUser = _authManager.VerifyJwt(Token);
                if (loggedInUser == null)
                {
                    return Unauthorized();
                }
                var userId = GetUserIdByUsername(loggedInUser).ToString();
                var currentsubscriptionWithPlan = await GetCurrentSubscriptionWithPlanByUserId(userId);
                var archivedsubscriptionWithPlan = await GetArchivedSubscriptionsWithPlanByUserId(userId);

                if (currentsubscriptionWithPlan == null && archivedsubscriptionWithPlan == null)
                {
                    return NotFound();
                }
                return Ok(new { CurrentSubscription = currentsubscriptionWithPlan, ArchivedSubscriptions = archivedsubscriptionWithPlan });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message.ToString());
            }
        }
        private async Task<object> GetArchivedSubscriptionsWithPlanByUserId(string userId)
        {
            var subscriptionWithPlan = _dbContext.ArchivedSubscriptions.Where(X => X.UserId == userId).Include(s => s.Plan).ToListAsync();
            return subscriptionWithPlan;
        }




        [HttpGet]
        [Route("validatesubscription")]
        public async Task<IActionResult> ValidateSubscription([FromHeader] string Token)
        {
            var subscription = await GetCurrentSubscriptionOnlyWithuserId(GetUserIdByUsername(_authManager.VerifyJwt(Token)));

            //User has never subscribed to any plan
            if (subscription == null)
            {
                return NotFound();
            }
            //User has subscribed to a plan but it has expired
            if (subscription.EndDate < DateTime.Now)
            {
                return BadRequest("Subscription has expired");
            }
            //User has subscribed to a plan and it is still active
            return Ok("paid user");
        }

        [HttpPost]
        [Route("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _dbContext.Plans.ToListAsync();
            return Ok(plans);   
        }

        [HttpPost]
        [Route("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbContext.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("activeusers")]
        public async Task<IActionResult> GetUsersWithActivePaidPlans()
        {
            var activeUsersWithPlans = await _dbContext.CurrentSubscriptions
                                                   .Where(x => x.EndDate > DateTime.Now && x.Quota != 0)
                                                   .Select(s => new
                                                   {
                                                       Username = s.User.UserName,
                                                       SubscriptionId = s.Id,
                                                       SubscriptionStartDate = s.StartDate,
                                                       SubscriptionEndDate = s.EndDate,
                                                       SubscriptionRemainingQuota = s.Quota,
                                                       PlanName = s.Plan.Name,
                                                   })
                                                   .ToListAsync();

            return Ok(activeUsersWithPlans);
        }

    }
}
