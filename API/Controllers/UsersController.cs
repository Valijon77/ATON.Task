using API.Data;
using API.DTOs;
using API.Enums;
using API.Extensions;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly DataContext _dataContext;
    private readonly ITokenService _tokenService;

    public UsersController(DataContext dataContext, ITokenService tokenService)
    {
        _dataContext = dataContext;
        _tokenService = tokenService;
    }

    // Create_1
    [HttpPost("register"), AllowAnonymous]
    public async Task<ActionResult> Register(RegisterDto registerDto)
    {
        bool isDifferentUser = false;

        var creatorLogin = User.GetLogin();

        if (creatorLogin is not null)
        {
            var userCreator = await _dataContext.Users.FirstOrDefaultAsync(
                u => u.Login == creatorLogin.ToLower()
            );

            if (userCreator is not null)
            {
                isDifferentUser = true;
                if (userCreator.Admin is false && registerDto.Admin is true)
                    return BadRequest("Только администратор может создать другого администратора.");
            }
        }
        else
        {
            if (registerDto.Admin is true)
            {
                return BadRequest("Только администратор может создать другого администратора.");
            }
        }

        var userToBeCreated = await _dataContext.Users.FirstOrDefaultAsync(
            u => u.Login == registerDto.Login!.ToLower()
        );

        if (userToBeCreated is not null)
            return BadRequest("Пользователь с указанным именем уже существует");


        User appUser =
            new()
            {
                Guid = Guid.NewGuid(),
                Login = registerDto.Login.ToLower(),
                Password = registerDto.Password,
                Name = registerDto.Name,
                Gender = (Gender)registerDto.Gender,
                Birthday = registerDto.Birthday,
                Admin = registerDto.Admin,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = isDifferentUser ? creatorLogin : registerDto.Login.ToLower(),
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = isDifferentUser ? creatorLogin : registerDto.Login.ToLower(),
                RevokedOn = null,
                RevokedBy = null,
            };

        _dataContext.Users.Add(appUser);

        if (await _dataContext.SaveChangesAsync() > 0)
        {
            return Ok(
                new UserDto()
                {
                    Guid = appUser.Guid,
                    Login = appUser.Login,
                    Name = appUser.Name,
                    Gender = nameof(appUser.Gender),
                    Birthday = appUser.Birthday,
                    Admin = appUser.Admin,
                    Token = _tokenService.CreateToken(appUser)
                }
            );
        }

        return BadRequest("Проблема при сохранении изменений в базе данных.");
    }

    // Read_7
    [HttpPost("login"), AllowAnonymous]
    public async Task<ActionResult> Login(LoginDto loginDto)
    {
        var appUser = await _dataContext.Users.SingleOrDefaultAsync(u => u.Login == loginDto.Login);

        if (appUser is null)
            return Unauthorized("Пользователь не существует");

        if (appUser.RevokedOn is not null)
            return BadRequest("Пользователь был удален");

        if (loginDto.Password != appUser.Password)
            return Unauthorized("Неверный пароль");

        return Ok(
            new UserDto()
            {
                Guid = appUser.Guid,
                Login = appUser.Login,
                Name = appUser.Name,
                Gender = appUser.Gender.ToString(),
                Birthday = appUser.Birthday,
                Admin = appUser.Admin,
                Token = _tokenService.CreateToken(appUser)
            }
        );
    }

    // Read_8
    [HttpGet]
    public async Task<ActionResult> GetUsersOlderThan([FromQuery] int age)
    {
        bool isAdmin =
            (await _dataContext.Users.SingleOrDefaultAsync(x => x.Login == User.GetLogin()))?.Admin
            ?? false;

        if (!isAdmin)
            return Unauthorized(
                "Для получения этой информации вам необходимы права администратора."
            );

        DateTime today = DateTime.UtcNow;

        var listOfUsers = _dataContext.Users.Where(
            user =>
                user.Birthday.HasValue
                && today.Year
                    - ((DateTime)user.Birthday).Year
                    - (((DateTime)user.Birthday).Month > today.Month ? 1 : 0)
                    > age
        );

        return Ok(await listOfUsers.ToListAsync());
    }

    // Read_6
    [HttpGet("{login}")]
    public async Task<ActionResult> GetUsersByLogin(string login)
    {
        bool isAdmin =
            (await _dataContext.Users.SingleOrDefaultAsync(x => x.Login == User.GetLogin()))?.Admin
            ?? false;

        if (!isAdmin)
            return Unauthorized(
                "Для получения этой информации вам необходимы права администратора."
            );

        var appUser = await _dataContext.Users.SingleOrDefaultAsync(u => u.Login == login);

        if (appUser is null)
            return BadRequest("Неверное имя пользователя");

        return Ok(
            new RequestByLoginDto()
            {
                Name = appUser.Name,
                Gender = appUser.Gender.ToString(),
                Birthday = appUser.Birthday,
                Active = appUser.RevokedOn is null
            }
        );
    }

    // Read_5
    [HttpGet("activeUsers")]
    public async Task<ActionResult> GetActiveUsers()
    {
        bool isAdmin =
            (await _dataContext.Users.SingleOrDefaultAsync(x => x.Login == User.GetLogin()))?.Admin
            ?? false;

        if (!isAdmin)
            return Unauthorized(
                "Для получения этой информации вам необходимы права администратора."
            );

        var activeUsers = await _dataContext.Users
            .Where(x => x.RevokedOn == null)
            .OrderBy(x => x.CreatedOn)
            .ToListAsync();

        return Ok(activeUsers);
    }

    [HttpDelete("{login}")]
    public async Task<ActionResult> DeleteUser(string login, [FromQuery] bool softDelete)
    {
        bool isAdmin =
            (await _dataContext.Users.SingleOrDefaultAsync(x => x.Login == User.GetLogin()))?.Admin
            ?? false;

        if (!isAdmin)
            return Unauthorized(
                "Для получения этой информации вам необходимы права администратора."
            );

        var appUser = await _dataContext.Users.SingleOrDefaultAsync(x => x.Login == login);

        if (appUser is null)
            return BadRequest("Пользователь не существует");

        if (appUser.RevokedOn != null && softDelete)
            return BadRequest("Пользователь уже был мягко удален");

        if (softDelete)
        {
            appUser.RevokedOn = DateTime.UtcNow;
            appUser.RevokedBy = User.GetLogin();
        }
        else
        {
            _dataContext.Users.Remove(appUser);
        }

        if (await _dataContext.SaveChangesAsync() > 0)
        {
            return Ok();
        }

        return BadRequest("Произошла ошибка при удалении пользователя");
    }

    // Update-2_10
    [HttpPut("unblock/{login}")]
    public async Task<ActionResult> UpdateUnblockUser(string login)
    {
        bool isAdmin =
            (await _dataContext.Users.SingleOrDefaultAsync(x => x.Login == User.GetLogin()))?.Admin
            ?? false;

        if (!isAdmin)
            return Unauthorized(
                "Для получения этой информации вам необходимы права администратора."
            );

        var appUser = await _dataContext.Users.SingleOrDefaultAsync(x => x.Login == login);

        if (appUser is null)
            return BadRequest("Пользователь не существует");

        if (appUser.RevokedOn == null || appUser.RevokedBy == null)
            return BadRequest("Невозможно разблокировать пользователя, пользовател уже активен");

        appUser.RevokedOn = null;
        appUser.RevokedBy = null;

        if (await _dataContext.SaveChangesAsync() > 0)
        {
            return NoContent();
        }

        return BadRequest("проблема при обновлении активности пользователя");
    }

    // Update-1_2
    [HttpPut("update")]
    public async Task<ActionResult> UpdatUserFields(UpdateUserDto updateDto)
    {
        var userMakingRequest = await _dataContext.Users.SingleOrDefaultAsync(
            x => x.Login == User.GetLogin()
        );

        if (userMakingRequest is null)
            return Unauthorized("Пожалуйста, войдите или зарегистрируйтесь");

        if (userMakingRequest!.Admin is false)
        {
            if (userMakingRequest!.Login == updateDto.Login && userMakingRequest.RevokedOn != null)
            {
                return Unauthorized("Ваш аккаунт заблокирован");
            }
            else if (userMakingRequest!.Login != updateDto.Login)
            {
                return Unauthorized("У вас недостаточно прав для внесения изменений");
            }
        }

        var appUser = await _dataContext.Users.SingleOrDefaultAsync(
            x => x.Login == updateDto.Login
        );

        if (appUser is null)
            return BadRequest("Пользователь не существует");

        appUser.Name = updateDto.Name;
        appUser.Birthday = updateDto.Birthday;
        appUser.Gender = (Gender)updateDto.Gender;
        appUser.ModifiedBy = User.GetLogin();
        appUser.ModifiedOn = DateTime.UtcNow;

        if (await _dataContext.SaveChangesAsync() > 0)
        {
            return NoContent();
        }

        return BadRequest("Ошибка при обновлении профиля пользователя");
    }

    [HttpPut("updatePassword")]
    public async Task<ActionResult> UpdatePassword(UpdatePasswordDto updateDto)
    {
        var userMakingRequest = await _dataContext.Users.SingleOrDefaultAsync(
            x => x.Login == User.GetLogin()
        );

        if (userMakingRequest is null)
            return Unauthorized("Пожалуйста, войдите или зарегистрируйтесь");

        if (!userMakingRequest!.Admin)
        {
            if (userMakingRequest!.Login == updateDto.Login && userMakingRequest.RevokedOn != null)
            {
                return Unauthorized("Ваш аккаунт заблокирован");
            }
            else if (userMakingRequest!.Login != updateDto.Login)
            {
                return Unauthorized("У вас недостаточно прав для внесения изменений");
            }
        }

        var appUser = await _dataContext.Users.SingleOrDefaultAsync(
            x => x.Login == updateDto.Login
        );

        if (appUser is null)
            return BadRequest("Пользователь не существует");

        appUser.Password = updateDto.Password;
        appUser.ModifiedBy = User.GetLogin();
        appUser.ModifiedOn = DateTime.UtcNow;

        if (await _dataContext.SaveChangesAsync() > 0)
        {
            return NoContent();
        }

        return BadRequest("Ошибка при обновлении пароля пользователя");
    }

    [HttpPut("updateLogin")]
    public async Task<ActionResult> UpdateLogin(UpdateLoginDto updateDto)
    {
        var userMakingRequest = await _dataContext.Users.SingleOrDefaultAsync(
            x => x.Login == User.GetLogin()
        );

        if (userMakingRequest is null)
            return Unauthorized("Пожалуйста, войдите или зарегистрируйтесь");

        if (!userMakingRequest!.Admin)
        {
            if (userMakingRequest!.Login == updateDto.Login && userMakingRequest.RevokedOn != null)
            {
                return Unauthorized("Ваш аккаунт заблокирован");
            }
            else if (userMakingRequest!.Login != updateDto.Login)
            {
                return Unauthorized("У вас недостаточно прав для внесения изменений");
            }
        }

        var appUser = await _dataContext.Users.SingleOrDefaultAsync(
            x => x.Login == updateDto.Login
        );

        if (appUser is null)
            return BadRequest("Пользователь не существует");

        bool loginAlreadyTaken = await _dataContext.Users.AnyAsync(
            x => x.Login == updateDto.NewLogin
        );
        if (loginAlreadyTaken)
            return BadRequest("Это имя пользователя уже занято. Попробуйте другой логин");

        appUser.Login = updateDto.NewLogin;
        appUser.ModifiedBy = updateDto.NewLogin;
        appUser.ModifiedOn = DateTime.UtcNow;

        if (await _dataContext.SaveChangesAsync() > 0)
        {
            return NoContent();
        }

        return BadRequest("Ошибка при обновлении имени пользователя");
    }
}
