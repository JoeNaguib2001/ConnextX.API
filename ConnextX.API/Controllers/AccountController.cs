using ConnextX.API.Contracts;
using ConnextX.API.Data.Dtos;
using ConnextX.API.Data.Models;
using ConnextX.API.Data.Models.DbContext;
using ConnextX.API.Data.Models.Users;
using Microsoft.AspNetCore.Authorization;
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


        [HttpPost]
        [Route("subscribe")]
        public async Task<IActionResult> Subscribe(int userSelectionPlanId)
        {
            var loggedInUserId = _authManager.VerifyJwt(GetCurrentSignedInUserToken());
            if (loggedInUserId == null)
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
            CurrentSubscription subscription = await GetCurrentSubscriptionOnlyWithuserId(loggedInUserId);
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
                UserId = loggedInUserId,
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
        public async Task<IActionResult> VerifyTokenGetUserReturnSubscriptionAndPlanByToken()
        {
            try
            {
                var loggedInUserId = _authManager.VerifyJwt(GetCurrentSignedInUserToken());
                if (loggedInUserId == null)
                {
                    return Unauthorized();
                }
                var subscriptionWithPlan =  GetCurrentSubscriptionWithPlanByUserId(loggedInUserId);

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
        private CurrentSubscription GetCurrentSubscriptionWithPlanByUserId(string userId)
        {
            // This will eagerly load the associated plan
            CurrentSubscription subscriptionWithPlan =   _dbContext.CurrentSubscriptions.Where(X => X.UserId == userId).Include(s => s.Plan).FirstOrDefault(); 
            return subscriptionWithPlan;
        }

        [HttpGet]
        [Route("currentandarchivedsubs")]
        public async Task<IActionResult> GetCurrentAndAcrhivedSubscriptions()
        {
            try
            {
                var loggedInUserId = _authManager.VerifyJwt(GetCurrentSignedInUserToken());
                if (loggedInUserId == null)
                {
                    return Unauthorized();
                }
                var currentsubscriptionWithPlan =  GetCurrentSubscriptionWithPlanByUserId(loggedInUserId);
                var archivedsubscriptionWithPlan =  GetArchivedSubscriptionsWithPlanByUserId(loggedInUserId);

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
        private List<ArchivedSubscriptions> GetArchivedSubscriptionsWithPlanByUserId(string userId)
        {
            var subscriptionWithPlan = _dbContext.ArchivedSubscriptions.Where(X => X.UserId == userId).Include(s => s.Plan).ToList();
            return subscriptionWithPlan;
        }




        [HttpGet]
        [Route("validatesubscription")]
        public async Task<IActionResult> ValidateSubscription()
        {
            var subscription = await GetCurrentSubscriptionOnlyWithuserId((_authManager.VerifyJwt(GetCurrentSignedInUserToken())));

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


        private string GetCurrentSignedInUserToken()
        {
            var authorizationHeader = HttpContext.Request.Headers["Authorization"];
            string accessToken = string.Empty;
            if (authorizationHeader.ToString().StartsWith("Bearer"))
            {
                accessToken = authorizationHeader.ToString().Substring("Bearer ".Length).Trim();
            }
            return accessToken;
        }
        [Authorize]
        [HttpPost]
        [Route("user")]
        public async Task<IActionResult> GetUser()
        {
            var accessToken = GetCurrentSignedInUserToken();
            var userId = _authManager.VerifyJwt(accessToken);
            var _user = await _dbContext.Users.Where(X => X.Id == userId).FirstOrDefaultAsync();
            return Ok(new
            {
                FirstName = _user.FirstName,
                LastName = _user.LastName,
                UserName = _user.UserName,
                Email = _user.Email,
            });
        }

        [HttpPost]
        [Route("users")]
        public async Task<IActionResult> GetUsers()
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
