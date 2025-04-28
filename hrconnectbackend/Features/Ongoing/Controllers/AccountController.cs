// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Mvc;
//
// namespace hrconnectbackend.Features.Ongoing.Controllers;
//
// public class AccountController : Controller
// {
//     private readonly AuthenticationService _authenticationService;
//
//     public AccountController(AuthenticationService authenticationService)
//     {
//         _authenticationService = authenticationService;
//     }
//
//     [HttpPost]
//     public async Task<IActionResult> Login(string username, string password)
//     {
//         try
//         {
//             bool isAuthenticated = await _authenticationService.AuthenticateUserAsync(username, password);
//             if (isAuthenticated)
//             {
//                 // Successfully logged in
//                 return RedirectToAction("Dashboard");
//             }
//             else
//             {
//                 // Invalid login attempt
//                 ModelState.AddModelError("", "Invalid username or password.");
//                 return View();
//             }
//         }
//         catch (InvalidOperationException ex)
//         {
//             // Account is locked
//             ModelState.AddModelError("", ex.Message);
//             return View();
//         }
//     }
// }
