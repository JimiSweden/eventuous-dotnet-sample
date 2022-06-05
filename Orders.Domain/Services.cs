using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain
{
    
    public static class Services
    {

        /// <summary>
        /// Note: this delegate has an example of a dummy implementation in Registrations  (commented out)
        /// services.AddSingleton<Services.IsRoomAvailable>((id, period) => new ValueTask<bool>(true)); //a dummy implementation always returning true. 
        /// 
        /// and an example implementing the RoomCheckerService.IsRoomAvailable found here <see cref="RoomCheckerService"/>
        /// services.AddSingleton<Services.IsRoomAvailable>(RoomCheckerService.IsRoomAvailable); 
        /// 
        /// 
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public delegate ValueTask<bool> IsProductAvailable(ProductOrServiceId productOrServiceId, int amount);
        public delegate ValueTask<int> MaxProductsOrderable(ProductOrServiceId productOrServiceId);

        //see dummy implementation in service.. 
        //public delegate Money ConvertCurrency(Money from, string targetCurrency);
    }

    public class ProductAvailabilityService
    {
        static int AmountInWarehouse = 29;

        public static ValueTask<bool> IsProductAvailable(ProductOrServiceId productOrServiceId, int amount)
        {
            
            return AmountInWarehouse > amount ? new ValueTask<bool>(false) : new ValueTask<bool>(true);
        }

        public static ValueTask<int> MaxProductsOrderable(ProductOrServiceId productOrServiceId)
        {
            return new ValueTask<int>(AmountInWarehouse);
        }
    }
}
