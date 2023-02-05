﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RedditMockup.Business.Base;
using RedditMockup.Common.Dtos;
using RedditMockup.Common.Helpers;
using RedditMockup.DataAccess.Contracts;
using RedditMockup.DataAccess.Repositories;
using RedditMockup.Model.Entities;
using Sieve.Models;
//using StackExchange.Redis;

namespace RedditMockup.Business.Businesses;

public class UserBusiness : BaseBusiness<User, UserDto>
{
    private readonly UserRepository _userRepository;

    private readonly IMapper _mapper;

    //private readonly IConnectionMultiplexer _connectionMultiplexer;

    //private readonly IDatabase _redisDb;

    /*
       public UserBusiness(IUnitOfWork unitOfWork, IMapper mapper, IConnectionMultiplexer connectionMultiplexer) :
            base(unitOfWork, unitOfWork.UserRepository!, mapper)
    {
        _userRepository = unitOfWork.UserRepository!;
        _mapper = mapper;
        _connectionMultiplexer = connectionMultiplexer;
        _redisDb = _connectionMultiplexer.GetDatabase();
    }
    */

    public UserBusiness(IUnitOfWork unitOfWork, IMapper mapper) :
            base(unitOfWork, unitOfWork.UserRepository!, mapper)
    {
        _userRepository = unitOfWork.UserRepository!;
        _mapper = mapper;
    }

    public override async Task<CustomResponse?> CreateAsync(UserDto dto, HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var isUsernameValid = !await UsernameExistsAsync(dto.Username!, cancellationToken);

        if (!isUsernameValid)
        {
            return new CustomResponse
            {
                IsSuccess = false,
                Message = $"{dto.Username} is already used, try another one"
            };
        }

        dto.Password = await dto.Password!.GetHashStringAsync();

        var user = _mapper.Map<User>(dto);

        user.Profile = new()
        {
            UserId = user.Id
        };

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = 2
        };

        user.UserRoles!.Add(userRole);

        return await CreateAsync(user, cancellationToken);
    }


    public async Task<User?> LoadModelByIdAsync(int id, CancellationToken cancellationToken = new())
    {
        SieveModel sieveModel = new()
        {
            Filters = $"Id=={id}"
        };

        var users = await _userRepository.LoadAllAsync(sieveModel,
            include => include
                            .Include(x => x.Person)
                            .Include(x => x.Profile)
                            .Include(x => x.Questions)
                            .Include(x => x.Answers)
                            .Include(x => x.UserRoles), cancellationToken);

        if (users.Count == 0)
        {
            return null;
        }

        var user = users.Single();

        #region [redis Section]

        //var key = $"User {user.Id}";

        //var value = user.Username;

        //await _redisDb.StringSetAsync(key, value);

        #endregion

        return user;
    }

    public override async Task<CustomResponse?> LoadByIdAsync(int id, CancellationToken cancellationToken = new())
    {
        var user = await LoadModelByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return new CustomResponse
            {
                IsSuccess = false,
                Message = $"No user exists with the ID of {id}"
            };
        }

        var response = _mapper.Map<UserDto>(user);

        return new CustomResponse
        {
            Data = response,
            IsSuccess = true
        };
    }

    private async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = new())
    {
        SieveModel sieveModel = new()
        {
            Filters = $"Username=={username}"
        };

        var users = await _userRepository.LoadAllAsync(sieveModel, null, cancellationToken);

        return users.Count > 0;
    }

    public override async Task<CustomResponse?> UpdateAsync(int id, UserDto dto,
        CancellationToken cancellationToken = new())
    {
        var user = await LoadModelByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return new CustomResponse
            {
                IsSuccess = false,
                Message = $"No user found with ID of {id}"
            };
        }

        _mapper.Map(dto, user);

        return await UpdateAsync(user, cancellationToken);
    }

    public override async Task<CustomResponse?> DeleteAsync(int id, CancellationToken cancellationToken = new())
    {
        var user = await LoadModelByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return new CustomResponse
            {
                IsSuccess = false,
                Message = $"No user found with ID of {id}"
            };
        }

        return await DeleteAsync(user, cancellationToken);
    }
}