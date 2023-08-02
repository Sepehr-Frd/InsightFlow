﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RedditMockup.Business.Base;
using RedditMockup.Common.Dtos;
using RedditMockup.DataAccess.Contracts;
using RedditMockup.DataAccess.Repositories;
using RedditMockup.Model.Entities;
using Sieve.Models;

namespace RedditMockup.Business.DomainEntityBusinesses;

public class UserBusiness : BaseBusiness<User, UserDto>
{
    #region [Redis Section]

    //private readonly IConnectionMultiplexer _connectionMultiplexer;

    //private readonly IDatabase _redisDb;


    /*
       public UserBusiness(IUnitOfWork unitOfWork, IMapper mapper, IConnectionMultiplexer connectionMultiplexer) :
            base(unitOfWork, unitOfWork.UserRepository!, mapper)
    {
        _userRepository = unitOfWork.UserRepository!;
        Mapper = mapper;
        _connectionMultiplexer = connectionMultiplexer;
        _redisDb = _connectionMultiplexer.GetDatabase();
    }

    */

    //var key = $"User {user.Id}";

    //var value = user.Username;

    //await _redisDb.StringSetAsync(key, value);

    #endregion

    #region [Fields]

    private readonly UserRepository _userRepository;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    #endregion

    #region [Constructor]

    public UserBusiness(IUnitOfWork unitOfWork, IMapper mapper) :
        base(unitOfWork, unitOfWork.UserRepository!, mapper)
    {
        _userRepository = unitOfWork.UserRepository!;

        _unitOfWork = unitOfWork;

        _mapper = mapper;
    }

    #endregion

    #region [Methods]

    public override async Task<User?> CreateAsync(UserDto userDto, CancellationToken cancellationToken = default)
    {
        var person = new Person
        {
            FirstName = userDto.FirstName,
            LastName = userDto.LastName
        };

        var createdPerson = await _unitOfWork.PersonRepository!.CreateAsync(person, cancellationToken);

        var user = _mapper.Map<User>(userDto);

        user.PersonId = createdPerson.Id;

        return await CreateAsync(user, cancellationToken);
    }

    public override async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _userRepository.GetByIdAsync(id,
            users => users.Include(user => user.Person)
                .Include(user => user.Profile),
            cancellationToken);

    public override async Task<User?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default) =>
        await _userRepository.GetByGuidAsync(guid,
            users => users.Include(user => user.Person)
                .Include(user => user.Profile),
            cancellationToken);

    public override async Task<List<User>?> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default) =>
        await _userRepository.GetAllAsync(sieveModel,
            users => users.Include(user => user.Person)
                .Include(user => user.Profile),
            cancellationToken);

    #endregion
}