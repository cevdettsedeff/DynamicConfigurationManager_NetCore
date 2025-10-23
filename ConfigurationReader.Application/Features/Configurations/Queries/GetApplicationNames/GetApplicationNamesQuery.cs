using ConfigurationReader.Application.Common;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Queries.GetApplicationNames;

public class GetApplicationNamesQuery : IRequest<Result<List<string>>>
{
}