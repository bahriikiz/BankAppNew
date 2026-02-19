using MediatR;
using OnlineBankAppServer.Domain.Entities;

namespace OnlineBankAppServer.Application.Features.Beneficiaries.Queries.GetAllBeneficiaries;

public sealed record GetAllBeneficiariesQuery() : IRequest<List<Beneficiary>>;