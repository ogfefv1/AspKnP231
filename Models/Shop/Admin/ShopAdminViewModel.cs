using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspKnP231.Models.Shop.Admin
{
    public class ShopAdminViewModel
    {
        public ShopSectionFormModel? ShopSectionFormModel { get; set; }

        public ModelStateDictionary? ShopSectionModelState { get; set; }
    }
}