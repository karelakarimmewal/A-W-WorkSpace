using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using TranQuik.Model;

namespace TranQuik.Controller
{
    public class OrderTransaction
    {
        public int TransactionID { get; set; }
        public int ComputerID { get; set; }
        public string TransactionUUID { get; set; }
        public string TranKey { get; set; }
        public DateTime? ReserveTime { get; set; }
        public int ReserveStaffID { get; set; }
        public DateTime? OpenTime { get; set; }
        public int OpenStaffID { get; set; }
        public string OpenStaff { get; set; }
        public DateTime? PaidTime { get; set; }
        public int PaidStaffID { get; set; }
        public string PaidStaff { get; set; }
        public int PaidComputerID { get; set; }
        public int VerifyPaidStaffID { get; set; }
        public DateTime? VerifyPaidDateTime { get; set; }
        public DateTime? BuffetStartTime { get; set; }
        public DateTime? BuffetEndTime { get; set; }
        public decimal BuffetTime { get; set; }
        public int BuffetType { get; set; }
        public DateTime? CloseTime { get; set; }
        public int CommStaffID { get; set; }
        public decimal DiscountItem { get; set; }
        public decimal DiscountBill { get; set; }
        public decimal DiscountOther { get; set; }
        public decimal TotalDiscount { get; set; }
        public short TransactionStatusID { get; set; }
        public int SaleMode { get; set; }
        public string TransactionName { get; set; }
        public string QueueName { get; set; }
        public short NoCustomer { get; set; }
        public int NoCustomerWhenOpen { get; set; }
        public int DocType { get; set; }
        public int ReceiptYear { get; set; }
        public int ReceiptMonth { get; set; }
        public int ReceiptDay { get; set; }
        public int ReceiptID { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public int ShopID { get; set; }
        public decimal TransactionVAT { get; set; }
        public decimal TransactionVATable { get; set; }
        public decimal TranBeforeVAT { get; set; }
        public string VATCode { get; set; }
        public decimal VATPercent { get; set; }
        public decimal ProductVAT { get; set; }
        public decimal ServiceChargePercent { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal ServiceChargeVAT { get; set; }
        public decimal SCBeforeVAT { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal OtherIncomeVAT { get; set; }
        public decimal OtherIncomeBeforeVAT { get; set; }
        public decimal ReceiptTotalQty { get; set; }
        public decimal ReceiptRetailPrice { get; set; }
        public decimal ReceiptDiscount { get; set; }
        public decimal ReceiptSalePrice { get; set; }
        public decimal ReceiptNetSale { get; set; }
        public decimal ReceiptPayPrice { get; set; }
        public decimal ReceiptRoundingBill { get; set; }
        public int SessionID { get; set; }
        public int CloseComputerID { get; set; }
        public int VoidStaffID { get; set; }
        public string VoidStaff { get; set; }
        public string VoidReason { get; set; }
        public DateTime? VoidTime { get; set; }
        public int IsCloneBill { get; set; }
        public int VoidTranID { get; set; }
        public int VoidComID { get; set; }
        public decimal DiffCloneBill { get; set; }
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public int HasOrder { get; set; }
        public int NoPrintBillDetail { get; set; }
        public short NoReprint { get; set; }
        public decimal LastPayCheckBill { get; set; }
        public decimal DiffPayCheckBill { get; set; }
        public int BillDetailReferenceNo { get; set; }
        public int CallForCheckBill { get; set; }
        public string TransactionNote { get; set; }
        public int CurrentAccessComputer { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? PrintWarningTime { get; set; }
        public int PrintBeginTime { get; set; }
        public int AlreadyCalculateStock { get; set; }
        public int AlreadyExportToHQ { get; set; }
        public int HasFullTax { get; set; }
        public int TableID { get; set; }
        public string TableName { get; set; }
        public int IsSplitTransaction { get; set; }
        public int IsFromOtherTransaction { get; set; }
        public string ReferenceNo { get; set; }
        public int FromDepositTransactionID { get; set; }
        public int FromDepositComputerID { get; set; }
        public string WifiUserName { get; set; }
        public string WifiPassword { get; set; }
        public DateTime? WifiExpire { get; set; }
        public string LogoImage { get; set; }
        public int Deleted { get; set; }

        public async Task<int> GenerateTransactionID()
        {
            LocalDbConnector dbConnector = new LocalDbConnector();

            try
            {
                string getLastSessionIdQuery = "SELECT MAX(TransactionID) FROM ordertransaction";

                int newTransactionID;

                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand getLastSessionIdCommand = new MySqlCommand(getLastSessionIdQuery, connection))
                    {
                        object result = await getLastSessionIdCommand.ExecuteScalarAsync();

                        newTransactionID = result != DBNull.Value ? Convert.ToInt32(result) + 1 : 1;

                        TransactionID = newTransactionID;
                    }
                }

                return newTransactionID; // Return the generated transaction ID
            }
            catch (MySqlException ex)
            {
                // Handle MySQL exceptions
                Console.WriteLine("MySQL Error: " + ex.Message);
                throw; // Rethrow the exception for higher-level handling
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Console.WriteLine("Error: " + ex.Message);
                throw; // Rethrow the exception for higher-level handling
            }
        }

        public string GenerateUUID()
        {
            // Create a new UUID (GUID)
            Guid uuid = Guid.NewGuid();

            // Convert the UUID to a string
            string uuidString = uuid.ToString().ToUpper();

            // Return the UUID string
            return uuidString;
        }

        public class NumberGenerator
        {
            public static string GenerateNumber(string prefix, int TransactionID)
            {
                return $"{prefix}{TransactionID:D7}";
            }
        }
    }

    public class OrderPayDetail
    {
        public int PayDetailID
        {
            get; set;
        }
        public int TransactionID
        {
            get; set;
        }
        public int ComputerID
        {
            get; set;
        }
        public string TranKey
        {
            get; set;
        }
        public int PayTypeID
        {
            get; set;
        }
        public decimal PayAmount
        {
            get; set;
        }
        public decimal CashChange
        {
            get; set;
        }
        public decimal CashChangeMainCurrency
        {
            get; set;
        }
        public string CashChangeMainCurrencyCode
        {
            get; set;
        }
        public decimal CashChangeCurrencyAmount
        {
            get; set;
        }
        public string CashChangeCurrencyCode
        {
            get; set;
        }
        public string CashChangeCurrencyName
        {
            get; set;
        }
        public decimal CashChangeCurrencyRatio
        {
            get; set;
        }
        public decimal CashChangeExchangeRate
        {
            get; set;
        }
        public string CreditCardNo
        {
            get; set;
        }
        public string CreditCardHolderName
        {
            get; set;
        }
        public string CCApproveCode
        {
            get; set;
        }
        public byte ExpireMonth
        {
            get; set;
        }
        public short ExpireYear
        {
            get; set;
        }
        public string ChequeNumber
        {
            get; set;
        }
        public DateTime? ChequeDate
        {
            get; set;
        }
        public int BankNameID
        {
            get; set;
        }
        public int CreditCardType
        {
            get; set;
        }
        public string PaidByName
        {
            get; set;
        }
        public string PayRemark
        {
            get; set;
        }
        public decimal Paid
        {
            get; set;
        }
        public int CardID
        {
            get; set;
        }
        public string CardNo
        {
            get; set;
        }
        public decimal PrepaidDiscountPercent
        {
            get; set;
        }
        public decimal RevenueRatio
        {
            get; set;
        }
        public bool IsFromEDC
        {
            get; set;
        }
        public string CurrencyCode
        {
            get; set;
        }
        public string CurrencyName
        {
            get; set;
        }
        public decimal CurrencyRatio
        {
            get; set;
        }
        public decimal ExchangeRate
        {
            get; set;
        }
        public decimal CurrencyAmount
        {
            get; set;
        }
        public int ShopID
        {
            get; set;
        }
        public DateTime? SaleDate
        {
            get; set;
        }
        public int OrgPayTypeID
        {
            get; set;
        }
        public decimal VoucherSellValue
        {
            get; set;
        }
        public decimal VoucherCostValue
        {
            get; set;
        }
        public int VoucherID
        {
            get; set;
        }
        public int VShopID
        {
            get; set;
        }
        public string VoucherNo
        {
            get; set;
        }
        public string VoucherSN
        {
            get; set;
        }
        public decimal RedeemSettingPoint
        {
            get; set;
        }
        public decimal RedeemPerPayAmount
        {
            get; set;
        }
        public decimal RedeemPoint
        {
            get; set;
        }
    }

    public class TransactionService
    {
        private LocalDbConnector localDbConnector = new LocalDbConnector();
        public async Task<bool> InsertTransaction(OrderTransaction transaction)
        {
            try
            {
                // Define the SQL query for inserting a new transaction
                string query = @"
                INSERT INTO ordertransaction (
                    TransactionID, ComputerID, TransactionUUID, TranKey, ReserveTime, ReserveStaffID,
                    OpenTime, OpenStaffID, OpenStaff, PaidTime, PaidStaffID, PaidStaff, PaidComputerID,
                    VerifyPaidStaffID, VerifyPaidDateTime, BuffetStartTime, BuffetEndTime, BuffetTime,
                    BuffetType, CloseTime, CommStaffID, DiscountItem, DiscountBill, DiscountOther,
                    TotalDiscount, TransactionStatusID, SaleMode, TransactionName, QueueName, NoCustomer,
                    NoCustomerWhenOpen, DocType, ReceiptYear, ReceiptMonth, ReceiptDay, ReceiptID,
                    ReceiptNumber, SaleDate, ShopID, TransactionVAT, TransactionVATable, TranBeforeVAT,
                    VATCode, VATPercent, ProductVAT, ServiceChargePercent, ServiceCharge, ServiceChargeVAT,
                    SCBeforeVAT, OtherIncome, OtherIncomeVAT, OtherIncomeBeforeVAT, ReceiptTotalQty,
                    ReceiptRetailPrice, ReceiptDiscount, ReceiptSalePrice, ReceiptNetSale, ReceiptPayPrice,
                    ReceiptRoundingBill, SessionID, CloseComputerID, VoidStaffID, VoidStaff, VoidReason,
                    VoidTime, IsCloneBill, VoidTranID, VoidComID, DiffCloneBill, MemberID, MemberName,
                    HasOrder, NoPrintBillDetail, NoReprint, LastPayCheckBill, DiffPayCheckBill,
                    BillDetailReferenceNo, CallForCheckBill, TransactionNote, CurrentAccessComputer,
                    UpdateDate, BeginTime, EndTime, PrintWarningTime, PrintBeginTime, AlreadyCalculateStock,
                    AlreadyExportToHQ, HasFullTax, TableID, TableName, IsSplitTransaction, IsFromOtherTransaction,
                    ReferenceNo, FromDepositTransactionID, FromDepositComputerID, WifiUserName, WifiPassword,
                    WifiExpire, LogoImage, Deleted
                ) VALUES (
                    @TransactionID, @ComputerID, @TransactionUUID, @TranKey, @ReserveTime, @ReserveStaffID,
                    @OpenTime, @OpenStaffID, @OpenStaff, @PaidTime, @PaidStaffID, @PaidStaff, @PaidComputerID,
                    @VerifyPaidStaffID, @VerifyPaidDateTime, @BuffetStartTime, @BuffetEndTime, @BuffetTime,
                    @BuffetType, @CloseTime, @CommStaffID, @DiscountItem, @DiscountBill, @DiscountOther,
                    @TotalDiscount, @TransactionStatusID, @SaleMode, @TransactionName, @QueueName, @NoCustomer,
                    @NoCustomerWhenOpen, @DocType, @ReceiptYear, @ReceiptMonth, @ReceiptDay, @ReceiptID,
                    @ReceiptNumber, @SaleDate, @ShopID, @TransactionVAT, @TransactionVATable, @TranBeforeVAT,
                    @VATCode, @VATPercent, @ProductVAT, @ServiceChargePercent, @ServiceCharge, @ServiceChargeVAT,
                    @SCBeforeVAT, @OtherIncome, @OtherIncomeVAT, @OtherIncomeBeforeVAT, @ReceiptTotalQty,
                    @ReceiptRetailPrice, @ReceiptDiscount, @ReceiptSalePrice, @ReceiptNetSale, @ReceiptPayPrice,
                    @ReceiptRoundingBill, @SessionID, @CloseComputerID, @VoidStaffID, @VoidStaff, @VoidReason,
                    @VoidTime, @IsCloneBill, @VoidTranID, @VoidComID, @DiffCloneBill, @MemberID, @MemberName,
                    @HasOrder, @NoPrintBillDetail, @NoReprint, @LastPayCheckBill, @DiffPayCheckBill,
                    @BillDetailReferenceNo, @CallForCheckBill, @TransactionNote, @CurrentAccessComputer,
                    @UpdateDate, @BeginTime, @EndTime, @PrintWarningTime, @PrintBeginTime, @AlreadyCalculateStock,
                    @AlreadyExportToHQ, @HasFullTax, @TableID, @TableName, @IsSplitTransaction, @IsFromOtherTransaction,
                    @ReferenceNo, @FromDepositTransactionID, @FromDepositComputerID, @WifiUserName, @WifiPassword,
                    @WifiExpire, @LogoImage, @Deleted
                )";

                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@TransactionID", transaction.TransactionID);
                        command.Parameters.AddWithValue("@ComputerID", transaction.ComputerID);
                        command.Parameters.AddWithValue("@TransactionUUID", transaction.TransactionUUID);
                        command.Parameters.AddWithValue("@TranKey", transaction.TranKey);
                        command.Parameters.AddWithValue("@ReserveTime", transaction.ReserveTime);
                        command.Parameters.AddWithValue("@ReserveStaffID", transaction.ReserveStaffID);
                        command.Parameters.AddWithValue("@OpenTime", transaction.OpenTime);
                        command.Parameters.AddWithValue("@OpenStaffID", transaction.OpenStaffID);
                        command.Parameters.AddWithValue("@OpenStaff", transaction.OpenStaff);
                        command.Parameters.AddWithValue("@PaidTime", transaction.PaidTime);
                        command.Parameters.AddWithValue("@PaidStaffID", transaction.PaidStaffID);
                        command.Parameters.AddWithValue("@PaidStaff", transaction.PaidStaff);
                        command.Parameters.AddWithValue("@PaidComputerID", transaction.PaidComputerID);
                        command.Parameters.AddWithValue("@VerifyPaidStaffID", transaction.VerifyPaidStaffID);
                        command.Parameters.AddWithValue("@VerifyPaidDateTime", transaction.VerifyPaidDateTime);
                        command.Parameters.AddWithValue("@BuffetStartTime", transaction.BuffetStartTime);
                        command.Parameters.AddWithValue("@BuffetEndTime", transaction.BuffetEndTime);
                        command.Parameters.AddWithValue("@BuffetTime", transaction.BuffetTime);
                        command.Parameters.AddWithValue("@BuffetType", transaction.BuffetType);
                        command.Parameters.AddWithValue("@CloseTime", transaction.CloseTime);
                        command.Parameters.AddWithValue("@CommStaffID", transaction.CommStaffID);
                        command.Parameters.AddWithValue("@DiscountItem", transaction.DiscountItem);
                        command.Parameters.AddWithValue("@DiscountBill", transaction.DiscountBill);
                        command.Parameters.AddWithValue("@DiscountOther", transaction.DiscountOther);
                        command.Parameters.AddWithValue("@TotalDiscount", transaction.TotalDiscount);
                        command.Parameters.AddWithValue("@TransactionStatusID", transaction.TransactionStatusID);
                        command.Parameters.AddWithValue("@SaleMode", transaction.SaleMode);
                        command.Parameters.AddWithValue("@TransactionName", transaction.TransactionName);
                        command.Parameters.AddWithValue("@QueueName", transaction.QueueName);
                        command.Parameters.AddWithValue("@NoCustomer", transaction.NoCustomer);
                        command.Parameters.AddWithValue("@NoCustomerWhenOpen", transaction.NoCustomerWhenOpen);
                        command.Parameters.AddWithValue("@DocType", transaction.DocType);
                        command.Parameters.AddWithValue("@ReceiptYear", transaction.ReceiptYear);
                        command.Parameters.AddWithValue("@ReceiptMonth", transaction.ReceiptMonth);
                        command.Parameters.AddWithValue("@ReceiptDay", transaction.ReceiptDay);
                        command.Parameters.AddWithValue("@ReceiptID", transaction.ReceiptID);
                        command.Parameters.AddWithValue("@ReceiptNumber", transaction.ReceiptNumber);
                        command.Parameters.AddWithValue("@SaleDate", transaction.SaleDate);
                        command.Parameters.AddWithValue("@ShopID", transaction.ShopID);
                        command.Parameters.AddWithValue("@TransactionVAT", transaction.TransactionVAT);
                        command.Parameters.AddWithValue("@TransactionVATable", transaction.TransactionVATable);
                        command.Parameters.AddWithValue("@TranBeforeVAT", transaction.TranBeforeVAT);
                        command.Parameters.AddWithValue("@VATCode", transaction.VATCode);
                        command.Parameters.AddWithValue("@VATPercent", transaction.VATPercent);
                        command.Parameters.AddWithValue("@ProductVAT", transaction.ProductVAT);
                        command.Parameters.AddWithValue("@ServiceChargePercent", transaction.ServiceChargePercent);
                        command.Parameters.AddWithValue("@ServiceCharge", transaction.ServiceCharge);
                        command.Parameters.AddWithValue("@ServiceChargeVAT", transaction.ServiceChargeVAT);
                        command.Parameters.AddWithValue("@SCBeforeVAT", transaction.SCBeforeVAT);
                        command.Parameters.AddWithValue("@OtherIncome", transaction.OtherIncome);
                        command.Parameters.AddWithValue("@OtherIncomeVAT", transaction.OtherIncomeVAT);
                        command.Parameters.AddWithValue("@OtherIncomeBeforeVAT", transaction.OtherIncomeBeforeVAT);
                        command.Parameters.AddWithValue("@ReceiptTotalQty", transaction.ReceiptTotalQty);
                        command.Parameters.AddWithValue("@ReceiptRetailPrice", transaction.ReceiptRetailPrice);
                        command.Parameters.AddWithValue("@ReceiptDiscount", transaction.ReceiptDiscount);
                        command.Parameters.AddWithValue("@ReceiptSalePrice", transaction.ReceiptSalePrice);
                        command.Parameters.AddWithValue("@ReceiptNetSale", transaction.ReceiptNetSale);
                        command.Parameters.AddWithValue("@ReceiptPayPrice", transaction.ReceiptPayPrice);
                        command.Parameters.AddWithValue("@ReceiptRoundingBill", transaction.ReceiptRoundingBill);
                        command.Parameters.AddWithValue("@SessionID", transaction.SessionID);
                        command.Parameters.AddWithValue("@CloseComputerID", transaction.CloseComputerID);
                        command.Parameters.AddWithValue("@VoidStaffID", transaction.VoidStaffID);
                        command.Parameters.AddWithValue("@VoidStaff", transaction.VoidStaff);
                        command.Parameters.AddWithValue("@VoidReason", transaction.VoidReason);
                        command.Parameters.AddWithValue("@VoidTime", transaction.VoidTime);
                        command.Parameters.AddWithValue("@IsCloneBill", transaction.IsCloneBill);
                        command.Parameters.AddWithValue("@VoidTranID", transaction.VoidTranID);
                        command.Parameters.AddWithValue("@VoidComID", transaction.VoidComID);
                        command.Parameters.AddWithValue("@DiffCloneBill", transaction.DiffCloneBill);
                        command.Parameters.AddWithValue("@MemberID", transaction.MemberID);
                        command.Parameters.AddWithValue("@MemberName", transaction.MemberName);
                        command.Parameters.AddWithValue("@HasOrder", transaction.HasOrder);
                        command.Parameters.AddWithValue("@NoPrintBillDetail", transaction.NoPrintBillDetail);
                        command.Parameters.AddWithValue("@NoReprint", transaction.NoReprint);
                        command.Parameters.AddWithValue("@LastPayCheckBill", transaction.LastPayCheckBill);
                        command.Parameters.AddWithValue("@DiffPayCheckBill", transaction.DiffPayCheckBill);
                        command.Parameters.AddWithValue("@BillDetailReferenceNo", transaction.BillDetailReferenceNo);
                        command.Parameters.AddWithValue("@CallForCheckBill", transaction.CallForCheckBill);
                        command.Parameters.AddWithValue("@TransactionNote", transaction.TransactionNote);
                        command.Parameters.AddWithValue("@CurrentAccessComputer", transaction.CurrentAccessComputer);
                        command.Parameters.AddWithValue("@UpdateDate", transaction.UpdateDate);
                        command.Parameters.AddWithValue("@BeginTime", transaction.BeginTime);
                        command.Parameters.AddWithValue("@EndTime", transaction.EndTime);
                        command.Parameters.AddWithValue("@PrintWarningTime", transaction.PrintWarningTime);
                        command.Parameters.AddWithValue("@PrintBeginTime", transaction.PrintBeginTime);
                        command.Parameters.AddWithValue("@AlreadyCalculateStock", transaction.AlreadyCalculateStock);
                        command.Parameters.AddWithValue("@AlreadyExportToHQ", transaction.AlreadyExportToHQ);
                        command.Parameters.AddWithValue("@HasFullTax", transaction.HasFullTax);
                        command.Parameters.AddWithValue("@TableID", transaction.TableID);
                        command.Parameters.AddWithValue("@TableName", transaction.TableName);
                        command.Parameters.AddWithValue("@IsSplitTransaction", transaction.IsSplitTransaction);
                        command.Parameters.AddWithValue("@IsFromOtherTransaction", transaction.IsFromOtherTransaction);
                        command.Parameters.AddWithValue("@ReferenceNo", transaction.ReferenceNo);
                        command.Parameters.AddWithValue("@FromDepositTransactionID", transaction.FromDepositTransactionID);
                        command.Parameters.AddWithValue("@FromDepositComputerID", transaction.FromDepositComputerID);
                        command.Parameters.AddWithValue("@WifiUserName", transaction.WifiUserName);
                        command.Parameters.AddWithValue("@WifiPassword", transaction.WifiPassword);
                        command.Parameters.AddWithValue("@WifiExpire", transaction.WifiExpire);
                        command.Parameters.AddWithValue("@LogoImage", transaction.LogoImage);
                        command.Parameters.AddWithValue("@Deleted", transaction.Deleted);

                        // Execute the command
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }
    }

    public class TransactionServicePayDetail
    {
        private LocalDbConnector localDbConnector = new LocalDbConnector();
        public async Task<bool> InsertTransaction(OrderPayDetail orderPayDetail)
        {
            try
            {
                // Define the SQL query for inserting a new transaction
                string query = @"
            INSERT INTO orderpaydetail (
                PayDetailID, TransactionID, ComputerID, TranKey, PayTypeID, PayAmount, CashChange,
                CashChangeMainCurrency, CashChangeMainCurrencyCode, CashChangeCurrencyAmount,
                CashChangeCurrencyCode, CashChangeCurrencyName, CashChangeCurrencyRatio,
                CashChangeExchangeRate, CreditCardNo, CreditCardHolderName, CCApproveCode,
                ExpireMonth, ExpireYear, ChequeNumber, ChequeDate, BankNameID, CreditCardType,
                PaidByName, PayRemark, Paid, CardID, CardNo, PrepaidDiscountPercent, RevenueRatio,
                IsFromEDC, CurrencyCode, CurrencyName, CurrencyRatio, ExchangeRate, CurrencyAmount,
                ShopID, SaleDate, OrgPayTypeID, VoucherSellValue, VoucherCostValue, VoucherID,
                VShopID, VoucherNo, VoucherSN, RedeemSettingPoint, RedeemPerPayAmount, RedeemPoint
            ) VALUES (
                @PayDetailID, @TransactionID, @ComputerID, @TranKey, @PayTypeID, @PayAmount, @CashChange,
                @CashChangeMainCurrency, @CashChangeMainCurrencyCode, @CashChangeCurrencyAmount,
                @CashChangeCurrencyCode, @CashChangeCurrencyName, @CashChangeCurrencyRatio,
                @CashChangeExchangeRate, @CreditCardNo, @CreditCardHolderName, @CCApproveCode,
                @ExpireMonth, @ExpireYear, @ChequeNumber, @ChequeDate, @BankNameID, @CreditCardType,
                @PaidByName, @PayRemark, @Paid, @CardID, @CardNo, @PrepaidDiscountPercent, @RevenueRatio,
                @IsFromEDC, @CurrencyCode, @CurrencyName, @CurrencyRatio, @ExchangeRate, @CurrencyAmount,
                @ShopID, @SaleDate, @OrgPayTypeID, @VoucherSellValue, @VoucherCostValue, @VoucherID,
                @VShopID, @VoucherNo, @VoucherSN, @RedeemSettingPoint, @RedeemPerPayAmount, @RedeemPoint
            )";

                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@PayDetailID", orderPayDetail.PayDetailID);
                        command.Parameters.AddWithValue("@TransactionID", orderPayDetail.TransactionID);
                        command.Parameters.AddWithValue("@ComputerID", orderPayDetail.ComputerID);
                        command.Parameters.AddWithValue("@TranKey", orderPayDetail.TranKey);
                        command.Parameters.AddWithValue("@PayTypeID", orderPayDetail.PayTypeID);
                        command.Parameters.AddWithValue("@PayAmount", orderPayDetail.PayAmount);
                        command.Parameters.AddWithValue("@CashChange", orderPayDetail.CashChange);
                        command.Parameters.AddWithValue("@CashChangeMainCurrency", orderPayDetail.CashChangeMainCurrency);
                        command.Parameters.AddWithValue("@CashChangeMainCurrencyCode", orderPayDetail.CashChangeMainCurrencyCode);
                        command.Parameters.AddWithValue("@CashChangeCurrencyAmount", orderPayDetail.CashChangeCurrencyAmount);
                        command.Parameters.AddWithValue("@CashChangeCurrencyCode", orderPayDetail.CashChangeCurrencyCode);
                        command.Parameters.AddWithValue("@CashChangeCurrencyName", orderPayDetail.CashChangeCurrencyName);
                        command.Parameters.AddWithValue("@CashChangeCurrencyRatio", orderPayDetail.CashChangeCurrencyRatio);
                        command.Parameters.AddWithValue("@CashChangeExchangeRate", orderPayDetail.CashChangeExchangeRate);
                        command.Parameters.AddWithValue("@CreditCardNo", orderPayDetail.CreditCardNo);
                        command.Parameters.AddWithValue("@CreditCardHolderName", orderPayDetail.CreditCardHolderName);
                        command.Parameters.AddWithValue("@CCApproveCode", orderPayDetail.CCApproveCode);
                        command.Parameters.AddWithValue("@ExpireMonth", orderPayDetail.ExpireMonth);
                        command.Parameters.AddWithValue("@ExpireYear", orderPayDetail.ExpireYear);
                        command.Parameters.AddWithValue("@ChequeNumber", orderPayDetail.ChequeNumber);
                        command.Parameters.AddWithValue("@ChequeDate", orderPayDetail.ChequeDate);
                        command.Parameters.AddWithValue("@BankNameID", orderPayDetail.BankNameID);
                        command.Parameters.AddWithValue("@CreditCardType", orderPayDetail.CreditCardType);
                        command.Parameters.AddWithValue("@PaidByName", orderPayDetail.PaidByName);
                        command.Parameters.AddWithValue("@PayRemark", orderPayDetail.PayRemark);
                        command.Parameters.AddWithValue("@Paid", orderPayDetail.Paid);
                        command.Parameters.AddWithValue("@CardID", orderPayDetail.CardID);
                        command.Parameters.AddWithValue("@CardNo", orderPayDetail.CardNo);
                        command.Parameters.AddWithValue("@PrepaidDiscountPercent", orderPayDetail.PrepaidDiscountPercent);
                        command.Parameters.AddWithValue("@RevenueRatio", orderPayDetail.RevenueRatio);
                        command.Parameters.AddWithValue("@IsFromEDC", orderPayDetail.IsFromEDC);
                        command.Parameters.AddWithValue("@CurrencyCode", orderPayDetail.CurrencyCode);
                        command.Parameters.AddWithValue("@CurrencyName", orderPayDetail.CurrencyName);
                        command.Parameters.AddWithValue("@CurrencyRatio", orderPayDetail.CurrencyRatio);
                        command.Parameters.AddWithValue("@ExchangeRate", orderPayDetail.ExchangeRate);
                        command.Parameters.AddWithValue("@CurrencyAmount", orderPayDetail.CurrencyAmount);
                        command.Parameters.AddWithValue("@ShopID", orderPayDetail.ShopID);
                        command.Parameters.AddWithValue("@SaleDate", orderPayDetail.SaleDate);
                        command.Parameters.AddWithValue("@OrgPayTypeID", orderPayDetail.OrgPayTypeID);
                        command.Parameters.AddWithValue("@VoucherSellValue", orderPayDetail.VoucherSellValue);
                        command.Parameters.AddWithValue("@VoucherCostValue", orderPayDetail.VoucherCostValue);
                        command.Parameters.AddWithValue("@VoucherID", orderPayDetail.VoucherID);
                        command.Parameters.AddWithValue("@VShopID", orderPayDetail.VShopID);
                        command.Parameters.AddWithValue("@VoucherNo", orderPayDetail.VoucherNo);
                        command.Parameters.AddWithValue("@VoucherSN", orderPayDetail.VoucherSN);
                        command.Parameters.AddWithValue("@RedeemSettingPoint", orderPayDetail.RedeemSettingPoint);
                        command.Parameters.AddWithValue("@RedeemPerPayAmount", orderPayDetail.RedeemPerPayAmount);
                        command.Parameters.AddWithValue("@RedeemPoint", orderPayDetail.RedeemPoint);

                        // Execute the command
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }
    }

}
