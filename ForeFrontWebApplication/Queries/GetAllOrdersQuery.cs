using ForeFrontWebApplication.Models.Order;
using MediatR;

namespace ForeFrontWebApplication.Queries;

public sealed record GetAllOrdersQuery : IRequest<IReadOnlyList<Orders>>;
