using MediatR;
using OnlineBankAppServer.Application.DTOs;

namespace OnlineBankAppServer.Application.Features.Admin.Queries.GetDashboard;

public sealed record GetAdminDashboardQuery() : IRequest<AdminDashboardDto>;