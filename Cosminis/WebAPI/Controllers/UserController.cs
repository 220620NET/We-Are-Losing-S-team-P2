using DataAccess.Entities;
using CustomExceptions;
using Models;
using System.Data.SqlClient;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Services;

namespace Controllers;

public class UserController
{
	private readonly UserServices _userServices;

    public UserController(UserServices userServices)
    {
        _userServices = userServices;
    }

    public IResult SearchFriend(string username)
    {
    	User userInfo = _userServices.SearchFriend(username);
    	try
    	{
    		return Results.Ok(userInfo); 
    	}
    	catch(ResourceNotFound)
        {
            return Results.BadRequest("No user with that username was found."); 
        }	
    }
}