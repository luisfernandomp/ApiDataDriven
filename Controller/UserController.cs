using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controller
{
    [Route("v1/users")]
    public class UserController : ControllerBase
    {
        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Post(
            [FromServices]DataContext context,
            [FromBody]User user
        ){
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                user.Role = "employee";

                context.Users.Add(user);
                await context.SaveChangesAsync();

                user.Password = "";

                return Ok(user);
            }
            catch (System.Exception)
            {
                return BadRequest(new {message = "Não foi possível criar um novo usuário"});
            }
        }

        [HttpPost]
        [Route("login")]

        // o dynamic faz com que o retorno possa ser qualquer coisa, não precisando seguir a estrutura do objeto
        public async Task<ActionResult<dynamic>> Authenticate(
            [FromServices]DataContext context,
            [FromBody]User user
        ){
            var result = await context.Users
                .AsNoTracking()
                .Where(x => x.Username == user.Username && x.Password == user.Password)
                .FirstOrDefaultAsync();

           if(result==null)
            return NotFound(new { message = "Usuário ou senha inválidos"});
        
            var token = TokenService.GenerateToken(user);
            
            user.Password = "";
            return new {
                user = user,
                token = token
            };
        }
    }
}