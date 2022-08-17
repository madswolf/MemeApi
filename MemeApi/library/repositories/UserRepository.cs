using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories
{
    public class UserRepository
    {
        private readonly MemeContext _memeContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IFileSaver _fileSaver;

        public UserRepository(MemeContext memeContext, UserManager<User> userManager, SignInManager<User> signInManager, IFileSaver fileSaver)
        {
            _memeContext = memeContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _fileSaver = fileSaver;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _memeContext.Users.ToListAsync();
        }

        public async Task<User> GetUser(int id)
        {
            return await _memeContext.Users.FindAsync(id);
        }


        public async Task<(bool, Errors)> UpdateUser(int id, UserUpdateDTO updateDto)
        {
            var user = await GetUser(id);

            if (user == null)
                return (false,Errors.Not_Found);
            

            user.UserName = updateDto.NewUsername;
            user.Email = updateDto.NewEmail ?? user.Email;

            if (updateDto.NewProfilePic != null ||updateDto.NewEmail != null)
            {

                if (updateDto.CurrentPassword == null) return (false, Errors.Bad_Request);
                var passwordValidator = new PasswordValidator<User>();
                var result = await passwordValidator.ValidateAsync(_userManager, user, updateDto.CurrentPassword);

                if (!result.Succeeded) return (false, Errors.Incorrect_Login);

                if (updateDto.NewProfilePic != null)
                {
                    user.ProfilePicFile = updateDto.NewProfilePic.Name;
                    _fileSaver.SaveFile(updateDto.NewProfilePic, "profilePictures/");
                }

                if (updateDto.NewEmail != null) user.Email = updateDto.NewEmail;
            }

            if (updateDto.NewPassword != null)
            {
                var result =
                    await _userManager.ChangePasswordAsync(user, updateDto.CurrentPassword, updateDto.NewPassword);
                if (!result.Succeeded)
                {
                    return (false, Errors.Incorrect_Login);
                }
            }

            try
            {
                await _memeContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return (false, Errors.Failure);
                }
            }

            return (true,Errors.NoFailure);
        }

        public async Task<bool> DeleteUser(int id)
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

        private bool UserExists(int id)
        {
            return _memeContext.Users.Any(e => e.Id == id);
        }
    }
}
