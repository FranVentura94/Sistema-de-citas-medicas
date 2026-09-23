using Core.Common;
using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Queries;

public class GetPacientesPagedQuery : RequestParametersGets, IRequest<PagedDto<List<Paciente>>>
{
}
