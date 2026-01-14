
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.DTOs;
using UsersMS.Domain.Interfaces;

namespace UsersMS.Application.Queries.GetUserHistory
{
    public class GetUserHistoryQueryHandler : IRequestHandler<GetUserHistoryQuery, List<UserHistoryDto>>
    {
        private readonly IUserRepository _repository;

        public GetUserHistoryQueryHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserHistoryDto>> Handle(GetUserHistoryQuery request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null) return new List<UserHistoryDto>();

            return user.History.Select(h => new UserHistoryDto(h.Action, h.Details, h.CreatedAt)).ToList();
        }
    }
}
