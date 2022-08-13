using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories
{
    public class UserRepository
    {
        private readonly MemeContext _memeContext;

        public UserRepository(MemeContext memeContext)
        {
            _memeContext = memeContext;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _memeContext.Users.ToListAsync();
        }

        public async Task<User> GetUser(long id)
        {
            return await _memeContext.Users.FindAsync(id);
        }


        public async Task<bool> UpdateUser(long id, UserUpdateDTO updateDto)
        {
            var user = await GetUser(id);

            if (user == null)
                return false;
            

            user.UserName = updateDto.NewUsername;
            user.Email = updateDto.NewEmail ?? user.Email;

            try
            {
                await _memeContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> DeleteUser(long id)
        {
            var user = await GetUser(id);
            if (user == null)
            {
                return true;
            }

            _memeContext.Users.Remove(user);
            await _memeContext.SaveChangesAsync();
            return false;
        }

        private bool UserExists(long id)
        {
            return _memeContext.Users.Any(e => e.Id == id);
        }
    }
}
