using BasicWebServer.Server.Controllers;
using BasicWebServer.Server.HTTP;

namespace BasicWebServer.Demo.Controllers
{
    public class UserController : Controller
    {
        private const string LoginForm = @"<form action='/Login' method='POST'>
   Username: <input type='text' name='Username'/>
   Password: <input type='text' name='Password'/>
   <input type='submit' value ='Log In' /> 
</form>";

        private const string Username = "user";
        private const string Password = "user123";

        public UserController(Request request) 
            : base(request)
        {
        }

        public Response Login() => Html(UserController.LoginForm);

        public Response LogInUser()
        {
            this.Request.Session.Clear();

            var usernameMatches =
                this.Request.Form["Username"] == UserController.Username;
            var passwordMatches =
                this.Request.Form["Password"] == UserController.Password;

            if (usernameMatches && passwordMatches)
            {
                if (!this.Request.Session.ContainsKey(Session.SessionUserKey))
                {
                    this.Request.Session[Session.SessionUserKey] = "MyUserId";

                    var cookies = new CookieCollection();
                    cookies.Add(Session.SessionCookieName,
                        this.Request.Session.Id);

                    return Html("<h3>Logged succesfully!</h3>", cookies);
                }

                return Html("<h3>Logged succesfully!</h3>");
            }

            return Redirect("/Login");
        }

        public Response Logout()
        {
            this.Request.Session.Clear();

            return Html("<h3>Logged out succesfully!</h3>");
        }

        public Response GetUserData()
        {
            if (this.Request.Session.ContainsKey(Session.SessionUserKey))
            {
                return Html($"<h3>Currently logged-in user is " +
                    $"with username '{UserController.Username}'</h3>");
            }

            return Redirect("/Login");
        }
    }
}
