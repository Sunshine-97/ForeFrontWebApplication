using ForeFrontWebApplication.Models.Order;
using MediatR;

namespace ForeFrontWebApplication.Queries;

public sealed record GetOrderByIdQuery(string OrderId) : IRequest<Orders?>;
