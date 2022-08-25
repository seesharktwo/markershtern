using AutoMapper;
using Grpc.Core;
using UserBagMicroservice.Data.Repository;
using UserBagMicroservice.Models;
using Briefcase;
using MongoDB.Bson;

namespace UserBagMicroservice.Services
{
    public class UserBagServiceGrpc : UserBriefcaseService.UserBriefcaseServiceBase
    {
        private readonly IMongoRepository<Models.UserBag> _userBagRepository;
        private readonly IMongoRepository<Models.Product> _productRepository;
        private readonly IMapper _mapper;

        public UserBagServiceGrpc(IMongoRepository<Models.UserBag> userBagRepository,
                              IMongoRepository<Models.Product> productRepository,
                              ILogger<UserBagServiceGrpc> logger,
                              IMapper mapper)
        {
            _userBagRepository = userBagRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public override async Task<GetUserProductsResponse> GetUserProducts(GetUserProductsRequest request, ServerCallContext context)
        {
            var response = new GetUserProductsResponse();
            var resultProductsList = new List<Protos.Product>();
            var productList = new Protos.ProductsList();
            var userBag = await _userBagRepository.FindOrCreateByIdAsync(new UserBag { Id = new ObjectId(request.UserId) });

            foreach (var userProduct in userBag.Products)
            {
                var product = await _productRepository.FindByIdAsync(userProduct.Id.ToString());
                resultProductsList.Add(new Protos.Product
                {
                    Id = product.Id,
                    AuthorId = product.AuthorId,
                    Name = product.Name,
                    Quantity = userProduct.Quantity
                });
            }

            productList.Value = resultProductsList;

            response.List = _mapper.Map<ProductsList>(productList);


            return response;
        }

        public override async Task<AddProductResponse> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var response = new AddProductResponse();
            var userBag = await _userBagRepository.FindOrCreateByIdAsync(new UserBag { Id = new ObjectId(request.UserId) });
            var product = await _productRepository.FindOrCreateOneAsync(p => p.Name == request.Name && p.AuthorId == request.UserId, new Models.Product { Name = request.Name, AuthorId = request.UserId });
            var userProduct = userBag.Products.FirstOrDefault(userProduct => userProduct.Id == product.Id);

            if (userProduct is null)
            {
                userBag.Products.Add(new UserBagProduct { Id = product.Id, Quantity = 0, TransactionId = "" });
                userProduct = userBag.Products.First(userProduct => userProduct.Id == product.Id);
            }

            userProduct.Quantity += request.Quantity;
            await _userBagRepository.ReplaceOneAsync(userBag);
            response.Success = new SuccessResponse();

            return response;
        }

        public override async Task<RemoveProductResponse> RemoveProduct(RemoveProductRequest request, ServerCallContext context)
        {
            var response = new RemoveProductResponse();
            try
            {
                var product = await _productRepository.FindByIdAsync(request.ProductId);
                CheckService.CheckProductOnNull(product);
                CheckService.CheckProductOwner(request.UserId, product.AuthorId);
                var userBag = await _userBagRepository.FindOrCreateByIdAsync(new UserBag { Id = new ObjectId(request.UserId)});
                var userProduct = userBag.Products.FirstOrDefault(product => product.Id.ToString() == request.ProductId);

                if (userProduct is not null)
                {
                    userBag.Products.Remove(userProduct);
                    await _userBagRepository.ReplaceOneAsync(userBag);
                }
                          
                response.Success = new SuccessResponse();
            }
            catch (ArgumentException e)
            {
                response.Error = CheckService.GetError(e.Message);
            }
            
            return response;
        }

        public override async Task<ValidateOrderResponse> ValidateOrder(ValidateOrderRequest request, ServerCallContext context)
        {
            var response = new ValidateOrderResponse();         
            try
            {
                var userBag = await _userBagRepository.FindByIdAsync(request.UserId);
                CheckService.CheckUserBagOnNull(userBag);
                var userProduct = userBag.Products.FirstOrDefault(product => product.Id.ToString() == request.ProductId);
                CheckService.CheckUserProductOnNull(userProduct);
                CheckService.CheckProductOnQuantity(userProduct.Quantity, request.Quantity);
                response.Success = new SuccessResponse();
            }
            catch (ArgumentException e)
            {
                response.Error = CheckService.GetError(e.Message);
            }

            return response;
        }

        
    }
}
