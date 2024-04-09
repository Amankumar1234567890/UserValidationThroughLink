
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserValidationThroughLink.Model;

namespace UserValidationThroughLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidationController : ControllerBase
    {   
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public ValidationController(IEmailService emailService,IMemoryCache memoryCache,IConfiguration confiq)
        { 
            _emailService = emailService;
            _memoryCache = memoryCache;
            _config = confiq;
        }

        [HttpPost]
        public async Task<IActionResult> SendMail(string email)
        {
            Mailrequest mailrequest = new Mailrequest();
            mailrequest.ToEmail = email;
            mailrequest.Subject = "Hello Aman";
            mailrequest.Body = HtmlContent(email);//"Confirmation";
            await _emailService.SendEmailAsync(mailrequest);
            return Ok();


        }
        private string HtmlContent(string email)
        {
            Random objrandom = new Random();
            int intvalue = objrandom.Next(10000, 99999);
           



            var token = GenerateToken(email);
            //string? validationUrl = $"https://localhost:7135/UserValidate/Index?email={email}&Value={intvalue}";
            string? validationUrl = $"https://localhost:7135/UserValidate/Index?token={token}&Value={intvalue}";


            StoreEmailInCache(intvalue,email);

            // Create the HTML content with the link
            string data = $"<h1>Thanks for Subscribing Us.</h1>" +
                          $"<p>Click <a href=\"{validationUrl}\">here</a> to validate your subscription.</p>";
            return data;
        }

        //[HttpPost]
        //[Route("ValidateSubscription")]
        //public IActionResult ValidateSubscription(Token token)
        //{
        //    //if (_memoryCache.TryGetValue(token.Email, out int cachedData))
        //    //{
        //    //    if (token.Value == cachedData)
        //    //    {
        //    //        _memoryCache.Remove(token.Email);
        //    //        return Ok();
        //    //    }
        //    //    // Cache hit: Data is available in the cache

        //    //}
        //    //return NotFound(); // Return an appropriate response
        //    return Ok();
        //}


        private void StoreEmailInCache(int value, string email)
        {
            _memoryCache.Set(value, email, TimeSpan.FromMinutes(2)); // Cache for 1 hour
        }

        private string GenerateToken(String email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                        new Claim("Email" , email),
                    

                

                       


                        //new Claim(ClaimTypes.Role,user.Role)
                    };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        [HttpPost("ValidateSubscription")]
        public IActionResult ValidateSubscription(Token token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token.token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == "Email").Value;
                if (_memoryCache.TryGetValue(token.Value, out string? cachedData))
                {
                    if (cachedData==email)
                    {
                        _memoryCache.Remove(token.Value);
                        return Ok("Subscription activated successfully.");
                    }
                }
                else
                {
                    return NotFound("Exceed time");
                }

                // Perform your subscription validation logic here
                // For example, activate the subscription for the user with the provided email

                
            }
            catch (Exception ex)
            {
                return BadRequest("Token validation failed.");
            }
            return Ok();
        }
    }
}
