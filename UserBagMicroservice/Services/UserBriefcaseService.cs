using AutoMapper;
using Grpc.Core;
using UserBagMicroservice.Data.Repository;
using UserBagMicroservice.Protos;

namespace UserBagMicroservice.Services
{
    public class UserBriefcaseService
    {
        private readonly IMongoRepository<Models.UserBag> _userBagRepository;
        private ILogger<UserBriefcaseService> _logger;
        private readonly IMapper _mapper;

        public UserBriefcaseService(IMongoRepository<Models.UserBag> userBagRepository, 
                              ILogger<UserBriefcaseService> logger, 
                              IMapper mapper)
        {
            _userBagRepository = userBagRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<GetUserProductsReply> GetUserProducts(GetUserProductsRequest request,
            ServerCallContext context)
        {
            List<Models.Product> productModels = _userBagRepository.FindById(request.UserId).Products;
            Models.Wrapper modelWrapper = new Models.Wrapper { Value = productModels };

            ToWrapper protoWrapper = _mapper.Map<ToWrapper>(modelWrapper);

            return await Task.FromResult(new GetUserProductsReply
            {
                UserId = request.UserId,
                Wrapper = protoWrapper
            });
        }

    }
}
