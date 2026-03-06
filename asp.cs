//using CART.Entity.CART;
using DocumentFormat.OpenXml.Spreadsheet;
using DX.Core.Interfaces;
using DX.Core.Interfaces.CART;
using DX.DataAccess.Entity.CC;
using DX.Models;
using DX.Models.API.Identity;
using DX.Models.Assets;
using DX.Models.Cart;
using DX.Models.CC;
using DX.Models.ControlDisplayConfig;
using DX.Models.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using static JsonSerializerHelper;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using APIResources = DX.Resources.API;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DX.Core.CART
{
    public class CARTService : CARTGenericRepository<DataAccess.Entity.CC.Cart>, ICart, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Lazy<ISPViewSPConfig> _spViewSPConfig;
        public CARTService(IServiceScopeFactory serviceScopeFactory, IOptions<AppSettings> appSettings, Lazy<ISPViewSPConfig> spViewSPConfig)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _spViewSPConfig = spViewSPConfig;
        }
        #region Public CRUD Methods

        /// <summary>
        /// add new Tags.
        /// </summary>
        /// <param name="articleModel"></param>
        /// <returns></returns>
        public async Task<CartModel> CreateCartAsync(CartModel cartModel)
        {
            int cartId = 0;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    if (null != cartModel)
                    {
                        bool IsAnonymous = false;
                        if (cartModel.IsAnonymous == true)
                        {
                            IsAnonymous = true;
                        }
                        else
                        {
                            cartModel.IsAnonymous = false;
                        }
                        var checkContactid = dbContext.Carts.Where(c => c.ContactId == cartModel.ContactID && c.IsDeleted == false).ToList();
                        if (checkContactid.Count() == 0)
                        {

                            DataAccess.Entity.CC.Cart cart = new DataAccess.Entity.CC.Cart
                            {
                                ContactId = cartModel.ContactID,
                                IsAnonymous = IsAnonymous
                            };

                            await Create(dbContext, cart);
                            cartId = cart.CartId;
                        }
                        else
                        {
                            cartId = checkContactid[0].CartId;
                        }
                        if (cartModel.AssetType == "Asset")
                        {
                            var feesid = dbContext.AssetFees.Where(a => a.AssetFeesId == cartModel.AssetFeeId && a.IsDeleted == false).ToList();
                            var itemid = dbContext.Catalogs.Where(a => a.FeesId == cartModel.AssetFeeId && a.Quantity > 0 && a.IsDeleted == false && a.LutCatalogCategoryId == 1).First();
                            if (itemid != null && feesid.Count() > 0)
                            {
                                
                                var checkitemCartId = dbContext.CartItems.Where(c => c.ItemId == itemid.ItemId && c.IsDeleted == false && (c.Ordered == true || c.CheckoutStarted == true)).ToList();
                                if (checkitemCartId.Count() == 0)
                                {
                                    var checkItemId = dbContext.CartItems.Where(c => c.ItemId == itemid.ItemId && c.IsDeleted == false && c.CartId == cartId).ToList();
                                    if (checkItemId.Count() == 0)
                                    {
                                        string engageRefId = string.Empty;
                                        var guid = $"select uuid_generate_v1()";
                                        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                                        {
                                            command.CommandText = guid;

                                            await dbContext.Database.OpenConnectionAsync();

                                            using var results = await command.ExecuteReaderAsync();
                                            while (await results.ReadAsync())
                                            {
                                                engageRefId = Convert.ToString(results[0]);
                                            }
                                            await dbContext.Database.CloseConnectionAsync();


                                            CartItem cartitem = new CartItem
                                            {
                                                CartId = cartId,
                                                ItemId = itemid.ItemId,
                                                TotalAmountDue = feesid[0].PaymentPending,
                                                TotalAmountPaid = feesid[0].PaymentPending,
                                                DueDate = feesid[0].DueDate,
                                                EngageOrderReferenceId = engageRefId,
                                            };
                                            dbContext.CartItems.Add(cartitem);
                                            dbContext.SaveChanges();
                                            cartModel.CartId = cartitem.CartItemId;
                                            cartModel.CartItemId = cartitem.CartItemId;
                                            cartModel.Message = "AddIteminCart";
                                        }
                                    }
                                    else
                                    {
                                        cartModel.CartId = checkItemId[0].CartId;
                                        cartModel.CartItemId = checkItemId[0].CartItemId;
                                        cartModel.Message = "alreadyincart";
                                    }
                                }
                                else
                                {
                                    cartModel.Message = "checkout";

                                }
                            }
                            else
                            {
                                cartModel.Message = "ErrorMessage";
                            }
                        }
                        else if (cartModel.AssetType == "UnPaidInVoice")
                        {
                            var itemid = dbContext.Catalogs.Where(a => a.FeesId == cartModel.FeeId && a.IsDeleted == false && a.LutCatalogCategoryId == 2).First();
                            if (itemid != null)
                            {
                              
                                var checkitemCartId = dbContext.CartItems.Where(c => c.ItemId == itemid.ItemId && (c.Ordered == true || c.CheckoutStarted == true)).ToList();
                                if (checkitemCartId.Count() == 0)
                                {
                                    var checkItemId = dbContext.CartItems.Where(c => c.ItemId == itemid.ItemId && c.IsDeleted == false && c.CartId == cartId).ToList();// && c.Ordered == false && c.CheckoutStarted == false).ToList();
                                    if (checkItemId.Count() == 0)
                                    {

                                        string engageRefId = string.Empty;
                                        var guid = $"select uuid_generate_v1()";
                                        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                                        {
                                            command.CommandText = guid;

                                            await dbContext.Database.OpenConnectionAsync();

                                            using var results = await command.ExecuteReaderAsync();
                                            while (await results.ReadAsync())
                                            {
                                                engageRefId = Convert.ToString(results[0]);
                                            }
                                            await dbContext.Database.CloseConnectionAsync();

                                            CartItem cartitem = new CartItem
                                            {
                                                CartId = cartId,
                                                ItemId = itemid.ItemId,
                                                TotalAmountDue = Convert.ToDecimal(cartModel.PayableAmt),
                                                TotalAmountPaid = Convert.ToDecimal(cartModel.PayableAmt),
                                                DueDate = cartModel.DueDate,
                                                EngageOrderReferenceId = engageRefId,
                                            };
                                            dbContext.CartItems.Add(cartitem);
                                            int affectedRows = dbContext.SaveChanges();
                                            cartModel.CartItemId = affectedRows;
                                            cartModel.CartId = cartitem.CartId;
                                        }
                                    }
                                    else
                                    {
                                        cartModel.CartId = 0;
                                        cartModel.CartItemId = checkItemId[0].CartItemId;
                                        cartModel.Message = "alreadyincart";
                                    }
                                }
                                else
                                {
                                    cartModel.Message = "checkout";

                                }
                            }
                        }
                        else if (cartModel.AssetType == "PermitInvoice")
                        {
                            var itemid = dbContext.Catalogs.Where(a => a.FeesId == cartModel.FeeId && a.IsDeleted == false && a.LutCatalogCategoryId == 3).First();
                            if (itemid != null)
                            {
                                //var checkItemId = dbContext.CartItems.Where(c => c.ItemId == itemid.ItemId && c.CartId == cartId && c.SaveforLater== false  && c.IsDeleted == false && c.Ordered == false && c.CheckoutStarted== false).ToList();
                                var checkitemCartId = dbContext.CartItems.Where(c => c.ItemId == itemid.ItemId && (c.Ordered == true || c.CheckoutStarted == true)).ToList();
                                if (checkitemCartId.Count() == 0)
                                {
                                    var checkItemId = dbContext.CartItems.Where(c => c.ItemId == itemid.ItemId && c.IsDeleted == false && c.CartId == cartId).ToList();// && c.Ordered == false && c.CheckoutStarted == false).ToList();
                                    if (checkItemId.Count() == 0)
                                    {

                                        string engageRefId = string.Empty;
                                        var guid = $"select uuid_generate_v1()";
                                        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                                        {
                                            command.CommandText = guid;

                                            await dbContext.Database.OpenConnectionAsync();

                                            using var results = await command.ExecuteReaderAsync();
                                            while (await results.ReadAsync())
                                            {
                                                engageRefId = Convert.ToString(results[0]);
                                            }
                                            await dbContext.Database.CloseConnectionAsync();

                                            CartItem cartitem = new CartItem
                                            {
                                                CartId = cartId,
                                                ItemId = itemid.ItemId,
                                                TotalAmountDue = Convert.ToDecimal(cartModel.PayableAmt),
                                                TotalAmountPaid = Convert.ToDecimal(cartModel.PayableAmt),
                                                DueDate = cartModel.DueDate,
                                                EngageOrderReferenceId = engageRefId,
                                            };
                                            dbContext.CartItems.Add(cartitem);
                                            dbContext.SaveChanges();
                                            cartModel.CartItemId = cartitem.CartItemId;
                                            cartModel.CartId = cartitem.CartId;
                                        }
                                    }
                                    else
                                    {
                                        cartModel.CartId = 0;
                                        cartModel.CartItemId = checkItemId[0].CartItemId;
                                        cartModel.Message = "alreadyincart";
                                    }
                                }
                                else
                                {
                                    cartModel.Message = "checkout";

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                cartModel.Message = "ErrorMessage";
                return cartModel;
            }
            return cartModel; // returns
        }
        #endregion

        public async Task<APIResponse> ProcessRequestCreateCartAsync(CartModel cartModel)
        {
            APIResponse response = new APIResponse();

            try
            {
                cartModel = await CreateCartAsync(cartModel);

                if (cartModel != null)
                {
                    response.Data = (cartModel);
                    response.Result.Code = APIReturnCodes.ServiceSuccessCode;
                    response.Result.Message = APIResources.Common.GenericSuccessMessage;
                }
                else
                {
                    response.Result.Code = APIReturnCodes.ServiceFailCode;
                    response.Result.Message = APIResources.Common.GenericFailMessage;
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);

                response.Result.Code = APIReturnCodes.ServiceFailCode;
                response.Result.Message = APIResources.Common.GenericFailMessage;
                response.Result.Cause = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }

            return response;
        }

        public async Task<APIResponse> ProcessRequestCreateCartAllPropertyAsync(CartAllPropertyModel cartallPropertyModel)
        {
            APIResponse response = new APIResponse();
            for (int i = 0; i < cartallPropertyModel.AssestIds.Count(); i++)
            {
                CartModel cartModel = new CartModel();
                cartModel.ContactID = cartallPropertyModel.ContactId;
                cartModel.AssestId = cartallPropertyModel.AssestIds[i];
                try
                {
                    cartModel = await CreateCartAsync(cartModel);

                    if (cartModel != null)
                    {
                        response.Data = (cartModel);
                        response.Result.Code = APIReturnCodes.ServiceSuccessCode;
                        response.Result.Message = APIResources.Common.GenericSuccessMessage;
                    }
                    else
                    {
                        response.Result.Code = APIReturnCodes.ServiceFailCode;
                        response.Result.Message = APIResources.Common.GenericFailMessage;
                    }
                }
                catch (Exception ex)
                {
                    SeriLogTools.LogErrorWithContext(ex);

                    response.Result.Code = APIReturnCodes.ServiceFailCode;
                    response.Result.Message = APIResources.Common.GenericFailMessage;
                    response.Result.Cause = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
            }
            return response;
        }

        public async Task<APIResponse> GetCartCountByContactIdAsync(int ContactId)
        {
            APIResponse response = new APIResponse();
            try
            {
                MyCartDisplayConfigModel reviewOrderDisplayConfigModel = new MyCartDisplayConfigModel();
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    var cartList = await (from cart in dbContext.Carts
                                          join cartItems in dbContext.CartItems
                                          on cart.CartId equals cartItems.CartId
                                          join catelog in dbContext.Catalogs
                                          on cartItems.ItemId equals catelog.ItemId
                                          where cart.ContactId == ContactId && cartItems.IsDeleted == false && cartItems.TotalAmountDue > 0 && cartItems.Ordered == false && cartItems.CheckoutStarted == false && cart.IsDeleted == false && catelog.IsDeleted == false && catelog.Quantity > 0
                                          select new
                                          {
                                              CartId = cart.CartId
                                              //cartItemId = cartItems.CartItemId,
                                              //EngageOrderReferenceId = cartItems.EngageOrderReferenceId,
                                              //Description = catelog.Attributes,
                                              //AmountDue = cartItems.TotalAmountDue,
                                              //ItemDescription = catelog.ItemName,
                                              //SaveforLater = cartItems.SaveforLater,
                                              //PartialPayment = catelog.AllowPartialPayment
                                          }).ToListAsync();


                    // Removed as only count of cart required
                    //if (cartList != null && cartList.Count() > 0)
                    //{
                    //    //IList<SubCartDetails> validProdcuts;
                    //    reviewOrderDisplayConfigModel.ListReviewOrder = new List<MyCartModel>();
                    //    int i = 0;
                    //    foreach (var odr in cartList)
                    //    {
                    //        try
                    //        {
                    //            MyCartModel orderModel = new MyCartModel();
                    //            orderModel.ItemDescription = odr.ItemDescription;
                    //            var result = Deserialize<CartSubdescDetails>(odr.Description);
                    //            if (result.ItemsDetails != null && result.ItemsDetails.Count() > 0)
                    //            {


                    //                    orderModel.ItemsDetails = result;
                    //                    orderModel.Assetnumber = result.Assetnumber;
                    //                    orderModel.ItemTotal = odr.AmountDue;
                    //                    orderModel.SaveforLater = odr.SaveforLater;
                    //                    orderModel.CartItemId = odr.cartItemId;
                    //                    orderModel.CartId = odr.CartId;
                    //                    orderModel.PartialPayment = odr.PartialPayment;
                    //                    reviewOrderDisplayConfigModel.ListReviewOrder.Add(orderModel);
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {

                    //        }

                    //    }
                    //}
                    //if (reviewOrderDisplayConfigModel != null)
                    //{
                    //    response.Data = reviewOrderDisplayConfigModel.ListReviewOrder;
                    //    response.Result.Code = APIReturnCodes.ServiceSuccessCode;
                    //    response.Result.Message = APIResources.Common.GenericSuccessMessage;
                    //}
                    //else

                    //{
                    //    response.Result.Code = APIReturnCodes.ServiceFailCode;
                    //    response.Result.Message = APIResources.Common.GenericFailMessage;
                    //}


                    if (cartList != null)
                    {
                        response.Data = (cartList.Count);
                        response.Result.Code = APIReturnCodes.ServiceSuccessCode;
                        response.Result.Message = APIResources.Common.GenericSuccessMessage;
                    }
                    else

                    {
                        response.Result.Code = APIReturnCodes.ServiceFailCode;
                        response.Result.Message = APIResources.Common.GenericFailMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);

                response.Result.Code = APIReturnCodes.ServiceFailCode;
                response.Result.Message = APIResources.Common.GenericFailMessage;
                response.Result.Cause = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }

            return response;
        }

        public async Task<MyCartDisplayConfigModel> GetAllCartSummary(Int32 ContactId)
        {
            MyCartDisplayConfigModel reviewOrderDisplayConfigModel = new MyCartDisplayConfigModel();
            reviewOrderDisplayConfigModel.ListReviewOrder = new List<MyCartModel>();
            decimal? total = 0;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();

                    var cartList = await (from cart in _dbContext.Carts
                                          join cartItems in _dbContext.CartItems
                                          on cart.CartId equals cartItems.CartId
                                          join catelog in _dbContext.Catalogs
                                          on cartItems.ItemId equals catelog.ItemId
                                          where
                                          cart.ContactId == ContactId &&
                                          cartItems.IsDeleted == false &&
                                          cartItems.TotalAmountDue > 0 &&
                                          cartItems.Ordered == false &&
                                          cartItems.CheckoutStarted == false &&
                                          cart.IsDeleted == false &&
                                          catelog.IsDeleted == false &&
                                          catelog.Quantity > 0
                                          orderby cartItems.CartItemId descending
                                          select new
                                          {
                                              CartId = cart.CartId,
                                              cartItemId = cartItems.CartItemId,
                                              EngageOrderReferenceId = cartItems.EngageOrderReferenceId,
                                              Description = catelog.Attributes,
                                              AmountDue = cartItems.TotalAmountDue,
                                              ItemDescription = catelog.ItemName,
                                              SaveforLater = cartItems.SaveforLater,
                                              PartialPayment = catelog.AllowPartialPayment,
                                              CatalogCategoryName = catelog.LutCatalogCategory.CatalogCategoryName,
                                              FeesId = catelog.FeesId,
                                          }).ToListAsync();

                    if (cartList != null && cartList.Count() > 0)
                    {
                        reviewOrderDisplayConfigModel.ListReviewOrder = new List<MyCartModel>();
                        foreach (var odr in cartList)
                        {
                            try
                            {
                                var assetFee = odr.CatalogCategoryName.ToLower().Contains("asset")
                                    ? _dbContext.AssetFees.Include(x => x.LutAssetFeeType).FirstOrDefault(x => x.AssetFeesId == odr.FeesId)
                                    : null;

                                var permitFee = odr.CatalogCategoryName.ToLower().Contains("permit")
                                    ? _dbContext.PermitFees.Include(x => x.LutPermitFeeType).FirstOrDefault(x => x.PermitFeeId == odr.FeesId)
                                    : null;

                                var inspectionFee = odr.CatalogCategoryName.ToLower().Contains("inspection")
                                    ? _dbContext.InspectionFees.Include(x => x.LutInspectionFeeType).FirstOrDefault(x => x.InspectionFeesId == odr.FeesId)
                                    : null;

                                var paymentGatewayConfig = assetFee?.LutAssetFeeType?.PaymentConfiguration
                                    ?? permitFee?.LutPermitFeeType?.PaymentConfiguration
                                    ?? inspectionFee?.LutInspectionFeeType?.PaymentConfiguration
                                    ?? null;

                                MyCartModel orderModel = new MyCartModel();
                                orderModel.ItemDescription = odr.ItemDescription;
                                var result = JsonConvert.DeserializeObject<CartSubdescDetails>(odr.Description);
                                if (result.ItemsDetails != null)
                                {
                                    var paymentGroupId = paymentGatewayConfig is null ? 0 : Convert.ToInt32(Convert.ToString(JObject.Parse(paymentGatewayConfig)["FeeInformation"].FirstOrDefault(item => item["Name"]?.ToString() == "PaymentGroup")?["Value"]));
                                    var paymentGroup = await _dbContext.PaymentGroup.FirstOrDefaultAsync(x => x.LutPaymentGroupId == paymentGroupId);
                                    orderModel.ItemsDetails = result;
                                    orderModel.Assetnumber = result.Assetnumber;
                                    orderModel.ItemTotal = odr.AmountDue;
                                    orderModel.SaveforLater = odr.SaveforLater;
                                    orderModel.CartItemId = odr.cartItemId;
                                    orderModel.CartId = odr.CartId;
                                    total += odr.AmountDue;
                                    orderModel.PartialPayment = odr.PartialPayment;
                                    orderModel.OtherDetails = result.OtherDetails;
                                    orderModel.PaymentGatewayConfig = paymentGatewayConfig;
                                    orderModel.PaymentGroupId = paymentGroupId;
                                    orderModel.PaymentGroupTitle = paymentGroup?.GroupName;
                                    reviewOrderDisplayConfigModel.ListReviewOrder.Add(orderModel);
                                }

                                reviewOrderDisplayConfigModel.TotalOrder = total;
                                reviewOrderDisplayConfigModel.CartId = cartList[0].CartId;
                                reviewOrderDisplayConfigModel.EngageOrderReferenceId = cartList[0].EngageOrderReferenceId != null ? cartList[0].EngageOrderReferenceId : null;
                            }
                            catch (Exception ex)
                            {
                                Serilog.Log.Warning("Check attrubutes in Catalog table " + odr.CartId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return reviewOrderDisplayConfigModel;
        }

        public async Task<bool> UpdateCartItemIdPayLater(Int32 CartItemId, bool SaveLater)
        {

            bool flag = false;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    var result = await dbContext.CartItems.SingleOrDefaultAsync(b => b.CartItemId == CartItemId);
                    if (result != null)
                    {
                        result.SaveforLater = SaveLater;
                        result.SaveForLaterDate = DateTime.Now;
                        dbContext.Entry(result).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                    flag = true;
                }

            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return flag;
        }

        public async Task<string> UpdateEngageRefIdbyCartId(Int32 CartId, string partialPayment)
        {
            string engageRefId = string.Empty;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    var result = dbContext.CartItems.Where(b => b.CartId == CartId && b.IsDeleted == false && b.CheckoutStarted == false && b.SaveforLater == false && b.Ordered == false).ToList();
                    if (result != null && result.Count() > 0)
                    {
                        var guid = $"select uuid_generate_v1()";
                        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = guid;

                            await dbContext.Database.OpenConnectionAsync();

                            using var results = await command.ExecuteReaderAsync();
                            while (await results.ReadAsync())
                            {
                                engageRefId = Convert.ToString(results[0]);
                            }
                            await dbContext.Database.CloseConnectionAsync();
                        }
                        if (partialPayment == "false")
                        {
                            var CartList = dbContext.CartItems.Where(b => b.CartId == CartId && b.CheckoutStarted == false && b.IsDeleted == false && b.Ordered == false && b.SaveforLater == false).ToList();
                            if (CartList != null && CartList.Count() > 0)
                            {
                                foreach (var item in CartList)
                                {
                                    item.EngageOrderReferenceId = engageRefId;
                                    item.TotalAmountPaid = item.TotalAmountDue;
                                }
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            var CartList = dbContext.CartItems.Where(b => b.CartId == CartId && b.CheckoutStarted == false && b.IsDeleted == false && b.Ordered == false && b.SaveforLater == false).ToList();
                            if (CartList != null && CartList.Count() > 0)
                            {
                                foreach (var item in CartList)
                                {
                                    item.EngageOrderReferenceId = engageRefId;
                                }
                                dbContext.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        result = dbContext.CartItems.Where(b => b.CartId == CartId).ToList();
                        if (result != null && result.Count() > 0)
                        {
                            var guid = $"select uuid_generate_v1()";
                            string EngageOrderReferenceIdGuID = string.Empty;

                            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                            {
                                command.CommandText = guid;

                                await dbContext.Database.OpenConnectionAsync();

                                using var results = await command.ExecuteReaderAsync();
                                while (await results.ReadAsync())
                                {
                                    EngageOrderReferenceIdGuID = Convert.ToString(result[0]);
                                }
                                await dbContext.Database.CloseConnectionAsync();
                            }
                            var CartList = dbContext.CartItems.Where(b => b.CartId == CartId && b.CheckoutStarted == false && b.IsDeleted == false && b.Ordered == false && b.SaveforLater == false).ToList();
                            if (CartList != null && CartList.Count() > 0)
                            {
                                CartList.ForEach(a => a.EngageOrderReferenceId = EngageOrderReferenceIdGuID);
                                dbContext.SaveChanges();

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return engageRefId;
        }

        public async Task<string> UpdateEngageRefIdbyCartId(Int32 CartId, string partialPayment, string[] cartItemIds)
        {
            string engageRefId = string.Empty;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    var parsedCartItemIds = cartItemIds.Select(x => int.Parse(x)).ToList();

                    var cartItems = _dbContext.CartItems.Where(b => b.CartId == CartId
                                                                    && b.IsDeleted == false
                                                                    && b.CheckoutStarted == false
                                                                    && b.SaveforLater == false
                                                                    && b.Ordered == false
                                                                    && parsedCartItemIds.Contains(b.CartItemId))
                                                        .ToList();

                    if (cartItems.Any())
                    {
                        engageRefId = await GenerateGuid();

                        foreach (var item in cartItems)
                        {
                            item.EngageOrderReferenceId = engageRefId;
                            if (partialPayment == "false")
                            {
                                item.TotalAmountPaid = item.TotalAmountDue;
                            }
                        }
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        var allCartItems = _dbContext.CartItems.Where(b => b.CartId == CartId).ToList();

                        if (allCartItems.Any())
                        {
                            engageRefId = await GenerateGuid();

                            foreach (var item in allCartItems.Where(b => parsedCartItemIds.Contains(b.CartItemId)))
                            {
                                item.EngageOrderReferenceId = engageRefId;
                            }
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw;
            }

            return engageRefId;
        }


        public async Task<bool> UpdateCartItemIdPayEnterAmt(string[] EnterAMT, string[] cartItemId, int contactId)
        {
            bool flag = false;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    if (EnterAMT.Length > 0)
                    {
                        for (int i = 0; i < EnterAMT.Length; i++)
                        {
                            var cartExitsinContcat = (from ep in dbContext.CartItems
                                                      join e in dbContext.Carts on ep.CartId equals e.CartId
                                                      where e.ContactId == contactId && ep.CartItemId == Convert.ToInt32(cartItemId[i])
                                                      select new
                                                      {
                                                          cartId = ep.CartId
                                                      }).ToListAsync();
                            if (cartExitsinContcat.Result.Count > 0)
                            {
                                var result = await dbContext.CartItems.Where(b => b.CartItemId == Convert.ToInt32(cartItemId[i]) && b.IsDeleted == false && b.CheckoutStarted == false && b.SaveforLater == false && b.Ordered == false).FirstOrDefaultAsync();
                                if (result != null)
                                {
                                    result.TotalAmountPaid = Convert.ToDecimal(EnterAMT[i]);
                                    dbContext.SaveChanges();
                                }
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }

                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return flag;
        }

        public async Task<string> UpdateEngageRefIdbyCartItemId(int[] CartItemId)
        {
            string engageRefId = string.Empty;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();

                    var result = _dbContext.CartItems.Where(b => CartItemId.Contains(b.CartItemId) && b.IsDeleted == false && b.CheckoutStarted == false && b.Ordered == false).ToList();
                    if (result != null && result.Count() > 0)
                    {
                        var guid = $"select uuid_generate_v1()";
                        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = guid;

                            await _dbContext.Database.OpenConnectionAsync();

                            using var results = await command.ExecuteReaderAsync();
                            while (await results.ReadAsync())
                            {
                                engageRefId = Convert.ToString(results[0]);
                            }
                            await _dbContext.Database.CloseConnectionAsync();
                        }

                        result.ForEach(a => a.EngageOrderReferenceId = engageRefId);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        result = _dbContext.CartItems.Where(b => CartItemId.Contains(b.CartItemId)).ToList();
                        if (result != null && result.Count() > 0)
                        {
                            var guid = $"select uuid_generate_v1()";

                            using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
                            {
                                command.CommandText = guid;

                                await _dbContext.Database.OpenConnectionAsync();

                                using var results = await command.ExecuteReaderAsync();
                                while (await results.ReadAsync())
                                {
                                    engageRefId = Convert.ToString(results[0]);
                                }
                                await _dbContext.Database.CloseConnectionAsync();
                            }
                            var CartList = _dbContext.CartItems.Where(b => CartItemId.Contains(b.CartItemId) && b.CheckoutStarted == false && b.IsDeleted == false && b.Ordered == false).ToList();
                            if (CartList != null && CartList.Count() > 0)
                            {
                                CartList.ForEach(a => a.EngageOrderReferenceId = engageRefId);
                                _dbContext.SaveChanges();

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return engageRefId;
        }


        public async Task<string> UpdateEngageRefIdbyCartItemId(Int32 CartItemId)
        {
            string engageRefId = string.Empty;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    var result = dbContext.CartItems.Where(b => b.CartItemId == CartItemId && b.IsDeleted == false && b.CheckoutStarted == false && b.Ordered == false).ToList();
                    if (result != null && result.Count() > 0)
                    {
                        var guid = $"select uuid_generate_v1()";
                        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = guid;

                            await dbContext.Database.OpenConnectionAsync();

                            using var results = await command.ExecuteReaderAsync();
                            while (await results.ReadAsync())
                            {
                                engageRefId = Convert.ToString(results[0]);
                            }
                            await dbContext.Database.CloseConnectionAsync();
                        }

                        result.ForEach(a => a.EngageOrderReferenceId = engageRefId);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        result = dbContext.CartItems.Where(b => b.CartItemId == CartItemId).ToList();
                        if (result != null && result.Count() > 0)
                        {
                            var guid = $"select uuid_generate_v1()";
                            //string EngageOrderReferenceIdGuID = string.Empty;

                            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                            {
                                command.CommandText = guid;

                                await dbContext.Database.OpenConnectionAsync();

                                using var results = await command.ExecuteReaderAsync();
                                while (await results.ReadAsync())
                                {
                                    engageRefId = Convert.ToString(results[0]);
                                }
                                await dbContext.Database.CloseConnectionAsync();
                            }
                            var CartList = dbContext.CartItems.Where(b => b.CartItemId == CartItemId && b.CheckoutStarted == false && b.IsDeleted == false && b.Ordered == false).ToList();
                            if (CartList != null && CartList.Count() > 0)
                            {
                                CartList.ForEach(a => a.EngageOrderReferenceId = engageRefId);
                                dbContext.SaveChanges();

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return engageRefId;
        }



        public async Task<bool> DeleteitemfromcartbyAssetId(int AssetId, Int32 ContactId, string UserName)
        {
            bool flag = false;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                string query = $"select * from  \"Asset\".\"removeParcelFromMyPropertyByAssetIdAndContactId\"(" + AssetId + "," + ContactId + ",'" + UserName + "')";
                var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    await dbContext.Database.OpenConnectionAsync();
                    using var results = await command.ExecuteReaderAsync();
                    while (await results.ReadAsync())
                    {
                        flag = Convert.ToBoolean(results[0]);
                    }
                    await dbContext.Database.CloseConnectionAsync();
                }
            }
            return flag;
        }

        public async Task<bool> CancelInvoicebyfeeId(int InspectionFeesId, int ContactId, string UserName, int LutCatalogCategoryId, string? comments)
        {
            bool flag = false;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                string query = $"select * from  \"INSP\".\"CancelInvoicebyFeeId\"(" + InspectionFeesId + "," + ContactId + ",'" + UserName + "'," + LutCatalogCategoryId + ",'" + comments + "')";
                var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    await dbContext.Database.OpenConnectionAsync();
                    using var results = await command.ExecuteReaderAsync();
                    while (await results.ReadAsync())
                    {
                        flag = Convert.ToBoolean(results[0]);
                    }
                    await dbContext.Database.CloseConnectionAsync();
                }
            }
            return flag;
        }
        public async Task<MyUtilityAPIResponse> InsertUpdateUtilityAssetandAssetFees(int ContactId, string AssetJson)
        {
            MyUtilityAPIResponse myUtilityAPIResponse = new MyUtilityAPIResponse();

            List<StoredProcedureParameterInfo> parameterInfos = new List<StoredProcedureParameterInfo>
                {
                    new StoredProcedureParameterInfo() { ParameterName = "InContactId", DataType = Convert.ToString(NpgsqlDbType.Integer), Value =  ContactId.ToString()},
                    new StoredProcedureParameterInfo() { ParameterName = "InAssetJson", DataType = Convert.ToString(NpgsqlDbType.Text), Value = AssetJson }
                };
            string response = await _spViewSPConfig.Value.ExecuteStoredProcedureToReturnString("CCConnectionString", "Asset.InsertUpdateUtilityAssetandAssetFees", parameterInfos);
            myUtilityAPIResponse = Deserialize<MyUtilityAPIResponse>(response);
            return myUtilityAPIResponse;
        }
        public async Task<MyUtilityAPIResponse> InsertUpdateUtilityFeeByAccountNumber(string AccountNumber, int ContactId)
        {
            MyUtilityAPIResponse myUtilityAPIResponse = new MyUtilityAPIResponse();

            string res = string.Empty;
            string generateQuery = $"select * from  \"Asset\".\"InsertUpdateUtilityFeeByAccountNumber\"('{AccountNumber}','{ContactId}');";
            var resultData = await _spViewSPConfig.Value.ExecuteQuery(generateQuery);

            return myUtilityAPIResponse;
        }

        public async Task<bool> checkAssetNumberExits(string AssetNumber)
        {
            bool flag = false;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                var checkAssetNumber = await dbContext.Assets.Where(a => a.AssetNumber == AssetNumber && a.IsDeleted == false).FirstOrDefaultAsync();
                if (checkAssetNumber == null)
                {
                    flag = true;
                }
            }
            return flag;

        }
        public async Task<bool> UpdateCartItemIdPayEnterAmtPico(string[] EnterAMT, string[] cartItemId)
        {
            bool flag = false;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    if (EnterAMT.Length > 0)
                    {
                        for (int i = 0; i < EnterAMT.Length; i++)
                        {
                            var cartExitsinContcat = (from ep in dbContext.CartItems
                                                      join e in dbContext.Carts on ep.CartId equals e.CartId
                                                      where ep.CartItemId == Convert.ToInt32(cartItemId[i])
                                                      select new
                                                      {
                                                          cartId = ep.CartId
                                                      }).ToListAsync();
                            if (cartExitsinContcat.Result.Count > 0)
                            {
                                var result = await dbContext.CartItems.Where(b => b.CartItemId == Convert.ToInt32(cartItemId[i]) && b.IsDeleted == false && b.CheckoutStarted == false && b.SaveforLater == false && b.Ordered == false).FirstOrDefaultAsync();
                                if (result != null)
                                {
                                    result.TotalAmountPaid = Convert.ToDecimal(EnterAMT[i]);
                                    dbContext.SaveChanges();
                                }
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }

                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return flag;
        }

        public async Task<int> FetchPaymentGroupIdByEngageOrderReferenceId(string engageOrderReferenceId)
        {
            int paymentGroupId = 0;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    var cartItem = await (from cart in _dbContext.Carts
                                      join cartItems in _dbContext.CartItems
                                      on cart.CartId equals cartItems.CartId
                                      join catelog in _dbContext.Catalogs
                                      on cartItems.ItemId equals catelog.ItemId
                                      where
                                      cartItems.EngageOrderReferenceId == engageOrderReferenceId &&
                                      cartItems.IsDeleted == false &&
                                      cartItems.TotalAmountDue > 0 &&
                                      cartItems.Ordered == false &&
                                      cartItems.CheckoutStarted == false &&
                                      cart.IsDeleted == false &&
                                      catelog.IsDeleted == false &&
                                      catelog.Quantity > 0
                                      select new
                                      {
                                          CartId = cart.CartId,
                                          cartItemId = cartItems.CartItemId,
                                          EngageOrderReferenceId = cartItems.EngageOrderReferenceId,
                                          Description = catelog.Attributes,
                                          AmountDue = cartItems.TotalAmountDue,
                                          ItemDescription = catelog.ItemName,
                                          SaveforLater = cartItems.SaveforLater,
                                          PartialPayment = catelog.AllowPartialPayment,
                                          CatalogCategoryName = catelog.LutCatalogCategory.CatalogCategoryName,
                                          FeesId = catelog.FeesId,
                                      }).FirstOrDefaultAsync();

                var paymentGatewayConfig =
                        (from assetFee in _dbContext.AssetFees
                         join lutAssetFeeType in _dbContext.LutAssetFeeTypes
                         on assetFee.LutAssetFeeTypeId equals lutAssetFeeType.LutAssetFeeTypeId
                         where cartItem.CatalogCategoryName.ToLower().Contains("asset") && assetFee.AssetFeesId == cartItem.FeesId
                         select lutAssetFeeType.PaymentConfiguration).FirstOrDefault()
                        ??
                        (from permitFee in _dbContext.PermitFees
                         join lutPermitFeeType in _dbContext.LutPermitFeeTypes
                         on permitFee.LutPermitFeeTypeId equals lutPermitFeeType.LutPermitFeeTypeId
                         where cartItem.CatalogCategoryName.ToLower().Contains("permit") && permitFee.PermitFeeId == cartItem.FeesId
                         select lutPermitFeeType.PaymentConfiguration).FirstOrDefault()
                        ??
                        (from inspectionFee in _dbContext.InspectionFees
                         join lutInspectionFeeType in _dbContext.LutInspectionFeeTypes
                         on inspectionFee.LutInspectionFeeTypeId equals lutInspectionFeeType.LutInspectionFeeTypeId
                         where cartItem.CatalogCategoryName.ToLower().Contains("inspection") && inspectionFee.InspectionFeesId == cartItem.FeesId
                         select lutInspectionFeeType.PaymentConfiguration).FirstOrDefault();

                paymentGroupId = paymentGatewayConfig is null ? 0 : Convert.ToInt32(Convert.ToString(JObject.Parse(paymentGatewayConfig)["FeeInformation"].FirstOrDefault(item => item["Name"]?.ToString() == "PaymentGroup")?["Value"]));
                }

            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return paymentGroupId;
        }

        private async Task<string> GenerateGuid()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
 
                string guid = string.Empty;
                var query = "SELECT uuid_generate_v1()";

                using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    await _dbContext.Database.OpenConnectionAsync();

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        if (await result.ReadAsync())
                        {
                            guid = Convert.ToString(result[0]);
                        }
                    }

                    await _dbContext.Database.CloseConnectionAsync();
                }
                return guid;
            }

        }
        public async Task<MultipleAssetFeeForAddtoCart> GetAssetFeeDetailByAssetId(int AssetId)
        {
            // Create an instance of MultipleAssetFeeForAddtoCart
            MultipleAssetFeeForAddtoCart multipleAssetFees = new MultipleAssetFeeForAddtoCart();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                // Fetch the data from the database
                var resultData = await _dbContext.AssetFees
                .Where(a => a.AssetId == AssetId && a.IsDeleted == false && a.PaymentPending > 0)
                .Select(a => new MultipleAssetFees
                {
                    AssetFeesId = a.AssetFeesId,
                    FeeAmount = a.FeeAmount,
                    FiscalYear = a.FiscalYear
                })
                .ToListAsync();

                // Populate the MultipleAssetFeeForAddtoCart object
                multipleAssetFees.AssetId = AssetId;
                multipleAssetFees.FeeDetails = resultData;
            }
            return multipleAssetFees;
        }
        public async Task<string> UpdateEngageRefIdbyMultipleCartItemId(Int32[] CartItemId)
        {
            string engageRefId = string.Empty;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CCContext>();
                    
                    var result = dbContext.CartItems.Where(b => CartItemId.Contains(b.CartItemId) && b.IsDeleted == false && b.CheckoutStarted == false && b.Ordered == false).ToList();
                    if (result != null && result.Count() > 0)
                    {
                        var guid = $"select uuid_generate_v1()";
                        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = guid;

                            await dbContext.Database.OpenConnectionAsync();

                            using var results = await command.ExecuteReaderAsync();
                            while (await results.ReadAsync())
                            {
                                engageRefId = Convert.ToString(results[0]);
                            }
                            await dbContext.Database.CloseConnectionAsync();
                        }

                        result.ForEach(a => a.EngageOrderReferenceId = engageRefId);
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                SeriLogTools.LogErrorWithContext(ex);
                throw ex;
            }
            return engageRefId;
        }
        public void Dispose()
        {
        }
    }
}
