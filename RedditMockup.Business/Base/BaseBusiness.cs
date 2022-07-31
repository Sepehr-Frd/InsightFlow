﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using RedditMockup.Business.Contracts;
using RedditMockup.Common.Dtos;
using RedditMockup.DataAccess.Contracts;
using RedditMockup.Model.Entities;
using Sieve.Models;

namespace RedditMockup.Business.Base;


public abstract class BaseBusiness<T, DTO> : IBaseBusiness<T, DTO>
    where T : BaseEntity
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IBaseRepository<T> _repository;

    private readonly IMapper _mapper;

    protected BaseBusiness(IUnitOfWork unitOfWork, IBaseRepository<T> repository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SamanSalamatResponse?> CreateAsync(T t, CancellationToken cancellationToken = new())
    {

        var entity = await _repository.CreateAsync(t, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        var response = _mapper.Map<DTO>(entity);

        return new SamanSalamatResponse
        {
            Data = response,
            IsSuccess = true,
            Message = "Entity Saved"
        };

    }

    public async Task<SamanSalamatResponse?> UpdateAsync(T t, CancellationToken cancellationToken = new())
    {

        await _repository.UpdateAsync(t, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return new SamanSalamatResponse
        {
            IsSuccess = true,
            Message = "Entity Updated"
        };
    }

    public async Task<SamanSalamatResponse?> DeleteAsync(T t, CancellationToken cancellationToken = new())
    {
        await _repository.DeleteAsync(t, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return new SamanSalamatResponse
        {
            IsSuccess = true,
            Message = "Entity Deleted"
        };
    }

    public async Task<SamanSalamatResponse<IEnumerable<DTO>>?> LoadAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = new())
    {
        var data = await _repository.LoadAllAsync(sieveModel, null, cancellationToken);

        var result = _mapper.Map<IEnumerable<DTO>>(data);

        return new SamanSalamatResponse<IEnumerable<DTO>>
        {
            Data = result,
            Message = "Data Loaded",
            IsSuccess = true
        };
    }


    /*-------------------------------------------------------------------*/

    public abstract Task<SamanSalamatResponse?> CreateAsync(DTO dto, HttpContext httpContext, CancellationToken 
    cancellationToken = new());

    public abstract Task<SamanSalamatResponse?> LoadByIdAsync(int id, CancellationToken cancellationToken = new());

    public abstract Task<SamanSalamatResponse?> UpdateAsync(int id, DTO dto, CancellationToken cancellationToken = new());

    public abstract Task<SamanSalamatResponse?> DeleteAsync(int id, CancellationToken cancellationToken = new());

}