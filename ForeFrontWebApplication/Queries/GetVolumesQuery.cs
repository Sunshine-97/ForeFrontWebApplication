using ForeFrontWebApplication.Models.Warehouse;
using MediatR;

namespace ForeFrontWebApplication.Queries;

public sealed record GetVolumesQuery(DateTime? From, DateTime? To)
    : IRequest<IReadOnlyList<OrderVolumes>>;
