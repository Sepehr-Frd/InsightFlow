﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedditMockup.Business.Base;
using RedditMockup.Common.Dtos;
using RedditMockup.DataAccess.Contracts;
using RedditMockup.DataAccess.Repositories;
using RedditMockup.Model.Entities;
using Sieve.Models;
using System.Net;

namespace RedditMockup.Business.DomainEntityBusinesses;

public class AnswerBusiness : BaseBusiness<Answer, AnswerDto>
{
    #region [Fields]

    private readonly AnswerRepository _answerRepository;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    #endregion

    #region [Constructor]

    public AnswerBusiness(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, unitOfWork.AnswerRepository!, mapper)
    {
        _answerRepository = unitOfWork.AnswerRepository!;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    #endregion

    #region [Methods]

    public override async Task<Answer?> CreateAsync(AnswerDto questionDto, CancellationToken cancellationToken = default)
    {
        var answer = _mapper.Map<Answer>(questionDto);

        var userId = await _unitOfWork.UserRepository!.GetIdByGuidAsync(questionDto.UserGuid, cancellationToken);

        if (userId is null)
        {
            return null;
        }

        var questionId = await _unitOfWork.QuestionRepository!.GetIdByGuidAsync(questionDto.QuestionGuid, cancellationToken);

        if (questionId is null)
        {
            return null;
        }

        answer.UserId = (int)userId;

        answer.QuestionId = (int)questionId;

        return await CreateAsync(answer, cancellationToken);
    }

    public override async Task<Answer?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _answerRepository.GetByIdAsync(id,
            answers => answers.Include(answer => answer.User)
                .Include(answer => answer.Question),
            cancellationToken);

    public override async Task<Answer?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default) =>
        await _answerRepository.GetByGuidAsync(guid,
            answers => answers.Include(answer => answer.User)
                .Include(answer => answer.Question),
            cancellationToken);

    public override async Task<List<Answer>?> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default) =>
        await _answerRepository.GetAllAsync(sieveModel,
            answers => answers.Include(answer => answer.User)
                .Include(answer => answer.Question),
            cancellationToken);

    public async Task<CustomResponse<List<Answer>>> GetAnswersByQuestionGuidAsync(Guid questionGuid, CancellationToken cancellationToken = default)
    {

        var question = await _unitOfWork.QuestionRepository!.GetByGuidAsync(questionGuid, null, cancellationToken);

        if (question is null)
        {
            return CustomResponse<List<Answer>>.CreateUnsuccessfulResponse(HttpStatusCode.NotFound);
        }

        SieveModel sieveModel = new()
        {
            Filters = $"QuestionId=={question.Id}"
        };

        var answers = await _answerRepository.GetAllAsync(sieveModel, null, cancellationToken);

        if (answers.IsNullOrEmpty())
        {
            return CustomResponse<List<Answer>>.CreateUnsuccessfulResponse(HttpStatusCode.NotFound, $"No answer found with question guid of {questionGuid}");
        }

        return CustomResponse<List<Answer>>.CreateSuccessfulResponse(answers);
    }

    public async Task<CustomResponse> SubmitVoteAsync(Guid answerGuid, bool kind, CancellationToken cancellationToken = default)
    {
        var answer = await _answerRepository.GetByGuidAsync(answerGuid,
            answers => answers.Include(answer => answer.User)
                .Include(answer => answer.Votes),
            cancellationToken);

        if (answer is null)
        {
            return CustomResponse.CreateUnsuccessfulResponse(HttpStatusCode.NotFound, $"No answer found with guid of {answerGuid}");
        }

        if (answer.User is null)
        {
            return CustomResponse.CreateUnsuccessfulResponse(HttpStatusCode.InternalServerError);
        }

        if (kind)
        {
            answer.User.Score += 1;
        }

        var vote = new AnswerVote
        {
            Kind = kind,
            AnswerId = answer.Id
        };

        await _answerRepository.SubmitVoteAsync(vote, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return CustomResponse.CreateSuccessfulResponse($"{(kind ? "Up" : "Down")}vote submitted");
    }

    public async Task<CustomResponse<List<AnswerVote>>> GetVotesByAnswerGuidAsync(Guid answerGuid, CancellationToken cancellationToken = default)
    {
        var answer = await _answerRepository.GetByGuidAsync(answerGuid,
            answers => answers.Include(answer => answer.Votes), cancellationToken);

        if (answer is null)
        {
            return CustomResponse<List<AnswerVote>>.CreateUnsuccessfulResponse(HttpStatusCode.NotFound);
        }

        var votes = answer.Votes!.ToList();

        return CustomResponse<List<AnswerVote>>.CreateSuccessfulResponse(votes);

    }

    #endregion
}