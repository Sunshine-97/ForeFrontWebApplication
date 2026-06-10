using ForeFrontWebApplication.Models.Warehouse;
using MediatR;

namespace ForeFrontWebApplication.Queries;

public sealed record GetTopProductsQuery : IRequest<IReadOnlyList<OrderVolumes>>;
