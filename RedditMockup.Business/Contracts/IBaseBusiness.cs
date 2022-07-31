﻿using Microsoft.AspNetCore.Http;
using RedditMockup.Common.Dtos;
using Sieve.Models;

namespace RedditMockup.Business.Contracts;

public interface IBaseBusiness<T, DTO>
{
    Task<SamanSalamatResponse?> CreateAsync(T t, CancellationToken cancellationToken);

    Task<SamanSalamatResponse<IEnumerable<DTO>>?> LoadAllAsync(SieveModel sieveModel, CancellationToken cancellationToken);

    Task<SamanSalamatResponse?> UpdateAsync(T t, CancellationToken cancellationToken);

    Task<SamanSalamatResponse?> DeleteAsync(T t, CancellationToken cancellationToken);

    Task<SamanSalamatResponse?> CreateAsync(DTO dto, HttpContext httpContext, CancellationToken cancellationToken);

    Task<SamanSalamatResponse?> LoadByIdAsync(int id, CancellationToken cancellationToken);

    Task<SamanSalamatResponse?> UpdateAsync(int id, DTO dto, CancellationToken cancellationToken);

    Task<SamanSalamatResponse?> DeleteAsync(int id, CancellationToken cancellationToken);

}