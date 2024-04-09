using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using UserValidationUsingMVC.Models;

namespace UserValidationUsingMVC.Controllers
{
    public class UserValidateController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7166/api");
        private readonly HttpClient _client;

        public UserValidateController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }

   
        public  IActionResult Index(string token,int Value)
        {
            Token data = new Token();
            data.token = token;
            data.Value = Value;
            var data1=JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(data1, Encoding.UTF8, "application/Json");
            var t =   _client.PostAsync(_client.BaseAddress + "/Validation/ValidateSubscription", content).Result;
            //HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/Yoga/UserCartData", content).Result;

            if (t.IsSuccessStatusCode)
            {
                return View();
            }
            throw new Exception("Validation Failed");
           
            
        }
      
    }
}
