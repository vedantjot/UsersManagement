using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserManagement.Models;
using UserManagement.Models.Data;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManagementContext _context;
        private readonly IConfiguration _config;
        private readonly String connectionString;

        public UsersController(UserManagementContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

            connectionString = _config.GetValue<string>("ConnectionStrings:Database");
        }


    

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            
            using (IDbConnection con = new SqlConnection(connectionString))
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();

               var users = con.Query<Users>("getAllUsers").ToList();
                return users;

            }

           

           
        }


        // GET: api/Users/login
        [HttpPost("login")]
        public Response login(Users user)
        {
            Users temp;
            using (IDbConnection con = new SqlConnection(connectionString))
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();

                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@email", user.Email);
               


                temp = con.Query<Users>("getUserByEmail", parameter, commandType: CommandType.StoredProcedure).FirstOrDefault();
             

            }


            if (temp == null)
            {
                return new Response { Message = "User Not Found", Status = "Invalid", User = null };
            }

            user.Password=user.Password.Trim();
            temp.Password=temp.Password.Trim();

            if (BCrypt.Net.BCrypt.Verify(user.Password, temp.Password)){

                //CookieOptions cookieRemember = new CookieOptions();
                //cookieRemember.Expires = DateTime.Now.AddSeconds(604800);
                //Response.Cookies.Append("userId", Convert.ToString(temp.Id), cookieRemember);

                //HttpContext.Session.SetInt32("userId", temp.Id);

                return new Response { Message = "Login Sucessful", Status = "Valid", User = temp };
            }

            return new Response { Message = "Invalid credentials", Status = "Invalid", User = null };
        }





        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUsers(int id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return users;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(int id, Users users)
        {
            if (id != users.Id)
            {
                return BadRequest();
            }

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users/signup
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("singup")]
        public Response PostUsers(Users users)
        {
            users.Password=users.Password.Trim();
            users.Password= BCrypt.Net.BCrypt.HashPassword(users.Password);


           

            using (IDbConnection con = new SqlConnection(connectionString))
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();

                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@email", users.Email);
                parameter.Add("@password", users.Password);
                parameter.Add("@fname", users.FirstName);
                parameter.Add("@lname", users.LastName);

                var result = con.Execute("addNewUser",parameter, commandType: CommandType.StoredProcedure);

                if(result>0)
                {
                    return new Response { Message = "Signup sucessfull", Status = "Valid", User = users };
                }

                return new Response { Message = "Signup Failed", Status = "InValid", User = null };

                //  return (Response)result;

            }


            //return CreatedAtAction("GetUsers", new { id = users.Id }, users);

           // return new Response { Message = "Signup sucessfull", Status = "Valid", User = users };
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Users>> DeleteUsers(int id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return users;
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
