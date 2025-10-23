using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Queries.GetConfigurationById;

public class GetConfigurationByIdQuery : IRequest<Result<ConfigurationItemDto>>
{
    public int Id { get; set; }

    public GetConfigurationByIdQuery(int id)
    {
        Id = id;
    }
}