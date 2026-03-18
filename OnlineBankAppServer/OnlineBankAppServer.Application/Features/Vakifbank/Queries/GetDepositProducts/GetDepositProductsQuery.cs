using MediatR;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetDepositProducts;

public sealed record GetDepositProductsQuery() : IRequest<VakifbankDepositProductResponseDto?>;