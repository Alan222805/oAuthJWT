using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using oAuthJWT.Models;

namespace oAuthJWT.Services
{
    public class UserService : IUserService
    {
        private readonly oAuthJTWContext _dbContext;

        public UserService(oAuthJTWContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public IEnumerable<User> GetUsers()
        {
            return _dbContext.Users;
        }

        public async Task PostUser(User user)
        {
            var newUser = new User{
                Id = Guid.NewGuid(),
                GoogleId = user.GoogleId,
                Name = user.Name,
                Email = user.Email
            };

            _dbContext.Add(newUser);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteUser(Guid userId)
        {
            var userexist = _dbContext.Users.Find(userId);

            if(userexist != null)
            {
                _dbContext.Users.Remove(userexist);
                await _dbContext.SaveChangesAsync();
            }
        }

        //Eliminar el token del usuario
        public async Task Patch(string googleId)
        {
            var user = await _dbContext.Users.FindAsync(googleId);

            if(user == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            user.CurrentToken = null;
            user.TokenExpiration = null;

            await _dbContext.SaveChangesAsync();
        }
    }


    public interface IUserService
    {
        IEnumerable<User> GetUsers();
        Task PostUser(User user);
        Task Patch(string googleId);
        Task DeleteUser(Guid userId);
    }
}