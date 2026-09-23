using Core.Common;
using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using GenericPersistence.FiltroDinamico;
using MediatR;

namespace Core.Features.Pacientes.Queries;

public class GetPacientesPagedQueryHandler
    : IRequestHandler<GetPacientesPagedQuery, PagedDto<List<Paciente>>>
{
    private readonly IPacienteRepository _pacienteRepository;

    public GetPacientesPagedQueryHandler(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository;
    }

    public async Task<PagedDto<List<Paciente>>> Handle(
        GetPacientesPagedQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _pacienteRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            !string.IsNullOrEmpty(request.Filter) ? Filter.FromStringExpression<Paciente>(request.Filter) : null,
            request.OrderBy,
            cancellationToken: cancellationToken);

        return new PagedDto<List<Paciente>>()
        {
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize,
            TotalPage = result.TotalPage,
            TotalRecords = result.TotalRecords,
            Data = result.Data.ToList()
        };
    }
}
