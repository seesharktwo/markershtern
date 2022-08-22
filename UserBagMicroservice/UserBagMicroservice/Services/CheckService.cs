using Briefcase;
using UserBagMicroservice.Models;
using UserBagMicroservice.Protos;

namespace UserBagMicroservice.Services
{
    public interface CheckService
    {
        public static void CheckUserBagOnNull(UserBag userBag)
        {
            if (userBag is null)
            {
                throw new ArgumentException(Error.UserNotFound.ToString());
            }
        }

        public static void CheckProductOnNull(UserBagProduct product)
        {
            if (product is null)
            {
                throw new ArgumentException(Error.UserNotHaveProduct.ToString());
            }
        }

        public static void CheckProductOnQuantity(int productQquantity, int quantity)
        {
            if ((productQquantity - quantity) < 0)
            {
                throw new ArgumentException(Error.UserNotHaveQuantityProduct.ToString());
            }
        }

        public static void CheckProductOwner(string userId, string authorId)
        {
            if (!userId.Equals(authorId))
            {
                throw new ArgumentException(Error.UserIsNotOwner.ToString());
            }
        }

        public static void CheckDublicateTransaction(string lastTransactionId, string transactionId)
        {
            if (!lastTransactionId.Equals(transactionId))
            {
                throw new ArgumentException("Duplicate transaction");
            }
        }

        public static Error GerError(string errorMessage)
        {
            if (errorMessage == Error.UserNotHaveProduct.ToString())
            {
                return Error.UserNotHaveProduct;
            }
            else if (errorMessage == Error.UserNotHaveQuantityProduct.ToString())
            {
                return Error.UserNotHaveQuantityProduct;
            }
            else if (errorMessage == Error.UserIsNotOwner.ToString())
            {
                return Error.UserIsNotOwner;
            }

            return Error.UserNotFound;
        }
    }
}
