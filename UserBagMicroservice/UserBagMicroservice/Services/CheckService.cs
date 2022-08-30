using Briefcase;
using UserBagMicroservice.Models;

namespace UserBagMicroservice.Services
{
    public interface CheckService
    {
        public static void CheckUserBagOnNull(UserBag ?userBag)
        {
            if (userBag is null)
            {
                throw new ArgumentException(Error.UserNotFound.ToString());
            }
        }

        public static void CheckProductOnNull(Models.Product? product)
        {
            if (product is null)
            {
                throw new ArgumentException(Error.ProductNotFound.ToString());
            }
        }

        public static void CheckUserProductOnNull(UserBagProduct ?product)
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
            if (lastTransactionId.Equals(transactionId))
            {
                throw new ArgumentException("Duplicate transaction");
            }
        }

        public static Error GetError(string errorMessage)
        {

            foreach(Error error in Enum.GetValues(typeof(Error)))
            {
                if (errorMessage.Equals(error.ToString()))
                    return error;
            }

            return Error.UserNotFound;
        }
    }
}
