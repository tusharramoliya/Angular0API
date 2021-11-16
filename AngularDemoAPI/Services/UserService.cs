using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.DBContext;
using WebApi.DBContext.DBModel;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IUserService
    {
        UserViewModel Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        Task<CreateUserResponse> CreateUpdateUser(CreateUserRequest model);
        DeleteUserResponse DeleteUser(long id);
    }

    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private readonly db_Context _context;
        private readonly IRazorViewToStringRenderer _toStringRenderer;

        public UserService(IOptions<AppSettings> appSettings,
            db_Context context,
            IRazorViewToStringRenderer toStringRenderer)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _toStringRenderer = toStringRenderer;
        }

        public UserViewModel Authenticate(string username, string password)
        {
            var userrec = _context.Users.SingleOrDefault(x => x.Email.ToLower() == username.ToLower() && x.Password == password);

            // return null if user not found
            if (userrec == null)
                return null;

            UserViewModel user = new UserViewModel()
            {
                firstname = userrec.FirstName,
                lastname = userrec.LastName,
                id = userrec.Id,
                username = userrec.Email,
                role = userrec.Role,
                message = userrec.Message
            };

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.id.ToString()),
                    new Claim(ClaimTypes.Role, user.role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.token = tokenHandler.WriteToken(token);

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.WithoutPasswords();
        }

        public User GetById(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            return user.WithoutPassword();
        }

        public async Task<CreateUserResponse> CreateUpdateUser(CreateUserRequest model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (model.Id <= 0)
                    {
                        var checkuser = _context.Users.FirstOrDefault(f => f.Email.ToLower() == model.email.ToLower());

                        if (checkuser != null)
                            return new CreateUserResponse()
                            {
                                issuccess = false,
                                message = "Entered email already exist."
                            };

                        var user = new User()
                        {
                            Email = model.email,
                            Password = model.password,
                            FirstName = model.firstname,
                            LastName = model.lastname,
                            Role = Role.User,
                            Message = model.message
                        };

                        _context.Users.Add(user);
                        _context.SaveChanges();

                        await SendEmail(user);
                    }
                    else
                    {
                        var checksameemailuser = _context.Users.FirstOrDefault(f => f.Email.ToLower() == model.email.ToLower()
                                                                            && f.Id != model.Id);

                        if (checksameemailuser != null)
                            return new CreateUserResponse()
                            {
                                message = "entered email with user already exist."
                            };

                        var getuser = _context.Users.FirstOrDefault(f => f.Id == model.Id);

                        if (getuser == null)
                            return new CreateUserResponse()
                            {
                                message = "user doesn't exist."
                            };

                        getuser.FirstName = model.firstname;
                        getuser.LastName = model.lastname;
                        getuser.Message = model.message;

                        _context.Users.Update(getuser);
                        _context.SaveChanges();
                    }

                    transaction.Commit();

                    return new CreateUserResponse()
                    {
                        issuccess = true,
                        message = "User save successfully."
                    };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new CreateUserResponse()
                    {
                        message = ex.Message
                    };
                }
            }
        }

        public DeleteUserResponse DeleteUser(long id)
        {
            try
            {
                var getuser = _context.Users.FirstOrDefault(f => f.Id == id);

                if (getuser == null)
                    return new DeleteUserResponse()
                    {
                        message = "User doesn't exist."
                    };

                _context.Users.Remove(getuser);
                _context.SaveChanges();

                return new DeleteUserResponse()
                {
                    issuccess = true,
                    message = "User delete successfully."
                };

            }
            catch (Exception ex)
            {
                return new DeleteUserResponse()
                {
                    message = ex.Message
                };
            }
        }

        private async Task<bool> SendEmail(User usermodel)
        {
            //var message = new MailMessage();
            //message.To.Add(new MailAddress(string.Format("{0} {1}", usermodel.FirstName, usermodel.LastName) + " <" + usermodel.Email + ">"));
            //message.From = new MailAddress("Test Demo <" + _appSettings.mailusername + ">");
            //message.Subject = "Test Demo";

            //message.Body = await _toStringRenderer.RenderViewToStringAsync("~/Views/UserEmailTemplate.cshtml", usermodel);

            //message.IsBodyHtml = true;
            //using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            //{
            //    try
            //    {

            //        var fromemail = _appSettings.SMTPUserName;
            //        var frompassword = _appSettings.SMTPPassword;

            //        smtp.Credentials = new System.Net.NetworkCredential(fromemail, frompassword);
            //        smtp.EnableSsl = true;

            //        await smtp.SendMailAsync(message);
            //        await Task.FromResult(0);
            //    }
            //    catch (Exception ex)
            //    {

            //    }

            //}

            return true;

        }

    }
}
