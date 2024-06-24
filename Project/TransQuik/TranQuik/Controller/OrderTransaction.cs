using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

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

    public class OrderDetail
    {
        public int OrderDetailID
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
        public int OrgOrderDetailID
        {
            get; set;
        }
        public int OrgTransactionID
        {
            get; set;
        }
        public int OrgComputerID
        {
            get; set;
        }
        public string OrgTranKey
        {
            get; set;
        }
        public byte ComponentLevel
        {
            get; set;
        }
        public int OrderDetailLinkID
        {
            get; set;
        }
        public short InsertOrderNo
        {
            get; set;
        }
        public byte IndentLevel
        {
            get; set;
        }
        public short DisplayOrdering
        {
            get; set;
        }
        public DateTime SaleDate
        {
            get; set;
        }
        public int ShopID
        {
            get; set;
        }
        public int ProductID
        {
            get; set;
        }
        public int ProductSetType
        {
            get; set;
        }
        public short OrderStatusID
        {
            get; set;
        }
        public short OrderEditStatus
        {
            get; set;
        }
        public int SaleMode
        {
            get; set;
        }
        public short SaleType
        {
            get; set;
        }
        public decimal TotalQty
        {
            get; set;
        }
        public decimal PricePerUnit
        {
            get; set;
        }
        public decimal TotalRetailPrice
        {
            get; set;
        }
        public decimal OrgPricePerUnit
        {
            get; set;
        }
        public decimal OrgTotalRetailPrice
        {
            get; set;
        }
        public decimal DiscPricePercent
        {
            get; set;
        }
        public decimal DiscPrice
        {
            get; set;
        }
        public decimal DiscPercent
        {
            get; set;
        }
        public decimal DiscAmount
        {
            get; set;
        }
        public decimal DiscOtherPercent
        {
            get; set;
        }
        public decimal DiscOther
        {
            get; set;
        }
        public decimal TotalItemDisc
        {
            get; set;
        }
        public decimal SalePrice
        {
            get; set;
        }
        public decimal DiscBill
        {
            get; set;
        }
        public decimal TotalDiscount
        {
            get; set;
        }
        public decimal NetSale
        {
            get; set;
        }
        public decimal AdjFromSaleType
        {
            get; set;
        }
        public decimal Vatable
        {
            get; set;
        }
        public string ProductVATCode
        {
            get; set;
        }
        public string VATDisplay
        {
            get; set;
        }
        public decimal ProductVATPercent
        {
            get; set;
        }
        public decimal ProductVAT
        {
            get; set;
        }
        public decimal ProductBeforeVAT
        {
            get; set;
        }
        public decimal TotalRetailVAT
        {
            get; set;
        }
        public decimal DiscVAT
        {
            get; set;
        }
        public byte IsSCBeforeDisc
        {
            get; set;
        }
        public byte HasServiceCharge
        {
            get; set;
        }
        public decimal SCPercent
        {
            get; set;
        }
        public decimal SCAmount
        {
            get; set;
        }
        public decimal SCVAT
        {
            get; set;
        }
        public decimal SCBeforeVAT
        {
            get; set;
        }
        public decimal WVatable
        {
            get; set;
        }
        public decimal SCWAmount
        {
            get; set;
        }
        public decimal SCWVAT
        {
            get; set;
        }
        public decimal SCWBeforeVAT
        {
            get; set;
        }
        public decimal WeightPrice
        {
            get; set;
        }
        public decimal WeightPriceVAT
        {
            get; set;
        }
        public decimal WeightBeforeVAT
        {
            get; set;
        }
        public decimal PaymentVAT
        {
            get; set;
        }
        public string OtherFoodName
        {
            get; set;
        }
        public int OtherProductGroupID
        {
            get; set;
        }
        public byte DiscountAllow
        {
            get; set;
        }
        public byte ItemDiscAllow
        {
            get; set;
        }
        public short AlreadyDiscQty
        {
            get; set;
        }
        public int? LastTransactionID
        {
            get; set;
        }
        public int? LastComputerID
        {
            get; set;
        }
        public string PrinterID
        {
            get; set;
        }
        public int InventoryID
        {
            get; set;
        }
        public int OrderStaffID
        {
            get; set;
        }
        public string OrderStaff
        {
            get; set;
        }
        public int OrderComputerID
        {
            get; set;
        }
        public string OrderComputer
        {
            get; set;
        }
        public int OrderTableID
        {
            get; set;
        }
        public string OrderTable
        {
            get; set;
        }
        public byte VoidTypeID
        {
            get; set;
        }
        public int VoidStaffID
        {
            get; set;
        }
        public string VoidStaff
        {
            get; set;
        }
        public DateTime? VoidDateTime
        {
            get; set;
        }
        public string VoidManualText
        {
            get; set;
        }
        public string VoidReasonText
        {
            get; set;
        }
        public byte VATType
        {
            get; set;
        }
        public byte PrintGroup
        {
            get; set;
        }
        public int NoPrintBill
        {
            get; set;
        }
        public byte NoRePrintOrder
        {
            get; set;
        }
        public DateTime? StartTime
        {
            get; set;
        }
        public DateTime? FinishTime
        {
            get; set;
        }
        public byte PrintStatus
        {
            get; set;
        }
        public DateTime? PrintOrderDateTime
        {
            get; set;
        }
        public DateTime? LastPrintOrderDateTime
        {
            get; set;
        }
        public string PrintErrorMsg
        {
            get; set;
        }
        public int CancelPrintStaffID
        {
            get; set;
        }
        public DateTime? CancelPrintDateTime
        {
            get; set;
        }
        public string CancelPrintReason
        {
            get; set;
        }
        public int ProcessID
        {
            get; set;
        }
        public DateTime? InsertOrderDateTime
        {
            get; set;
        }
        public DateTime? SubmitOrderDateTime
        {
            get; set;
        }
        public DateTime? ModifyOrderDateTime
        {
            get; set;
        }
        public int ModifyStaffID
        {
            get; set;
        }
        public string Comment
        {
            get; set;
        }
        public byte IsComment
        {
            get; set;
        }
        public short BillCheckID
        {
            get; set;
        }
        public int PGroupID
        {
            get; set;
        }
        public short SetGroupNo
        {
            get; set;
        }
        public decimal QtyRatio
        {
            get; set;
        }
        public byte FreeItem
        {
            get; set;
        }
        public int SummaryID
        {
            get; set;
        }
        public byte Deleted
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

    public class TransactionServiceOrderDetail
    {
        private LocalDbConnector localDbConnector = new LocalDbConnector();

        public async Task<bool> InsertTransaction(OrderDetail orderDetail)
        {
            try
            {
                // Define the SQL query for inserting a new transaction
                string query = @"
        INSERT INTO orderdetail (
            OrderDetailID, TransactionID, ComputerID, TranKey, OrgOrderDetailID, OrgTransactionID, OrgComputerID,
            OrgTranKey, ComponentLevel, OrderDetailLinkID, InsertOrderNo, IndentLevel, DisplayOrdering, SaleDate,
            ShopID, ProductID, ProductSetType, OrderStatusID, OrderEditStatus, SaleMode, SaleType, TotalQty,
            PricePerUnit, TotalRetailPrice, OrgPricePerUnit, OrgTotalRetailPrice, DiscPricePercent, DiscPrice,
            DiscPercent, DiscAmount, DiscOtherPercent, DiscOther, TotalItemDisc, SalePrice, DiscBill, TotalDiscount,
            NetSale, AdjFromSaleType, Vatable, ProductVATCode, VATDisplay, ProductVATPercent, ProductVAT,
            ProductBeforeVAT, TotalRetailVAT, DiscVAT, IsSCBeforeDisc, HasServiceCharge, SCPercent, SCAmount,
            SCVAT, SCBeforeVAT, WVatable, SCWAmount, SCWVAT, SCWBeforeVAT, WeightPrice, WeightPriceVAT,
            WeightBeforeVAT, PaymentVAT, OtherFoodName, OtherProductGroupID, DiscountAllow, ItemDiscAllow,
            AlreadyDiscQty, LastTransactionID, LastComputerID, PrinterID, InventoryID, OrderStaffID, OrderStaff,
            OrderComputerID, OrderComputer, OrderTableID, OrderTable, VoidTypeID, VoidStaffID, VoidStaff,
            VoidDateTime, VoidManualText, VoidReasonText, VATType, PrintGroup, NoPrintBill, NoRePrintOrder,
            StartTime, FinishTime, PrintStatus, PrintOrderDateTime, LastPrintOrderDateTime, PrintErrorMsg,
            CancelPrintStaffID, CancelPrintDateTime, CancelPrintReason, ProcessID, InsertOrderDateTime,
            SubmitOrderDateTime, ModifyOrderDateTime, ModifyStaffID, Comment, IsComment, BillCheckID, PGroupID,
            SetGroupNo, QtyRatio, FreeItem, SummaryID, Deleted
        ) VALUES (
            @OrderDetailID, @TransactionID, @ComputerID, @TranKey, @OrgOrderDetailID, @OrgTransactionID, @OrgComputerID,
            @OrgTranKey, @ComponentLevel, @OrderDetailLinkID, @InsertOrderNo, @IndentLevel, @DisplayOrdering, @SaleDate,
            @ShopID, @ProductID, @ProductSetType, @OrderStatusID, @OrderEditStatus, @SaleMode, @SaleType, @TotalQty,
            @PricePerUnit, @TotalRetailPrice, @OrgPricePerUnit, @OrgTotalRetailPrice, @DiscPricePercent, @DiscPrice,
            @DiscPercent, @DiscAmount, @DiscOtherPercent, @DiscOther, @TotalItemDisc, @SalePrice, @DiscBill, @TotalDiscount,
            @NetSale, @AdjFromSaleType, @Vatable, @ProductVATCode, @VATDisplay, @ProductVATPercent, @ProductVAT,
            @ProductBeforeVAT, @TotalRetailVAT, @DiscVAT, @IsSCBeforeDisc, @HasServiceCharge, @SCPercent, @SCAmount,
            @SCVAT, @SCBeforeVAT, @WVatable, @SCWAmount, @SCWVAT, @SCWBeforeVAT, @WeightPrice, @WeightPriceVAT,
            @WeightBeforeVAT, @PaymentVAT, @OtherFoodName, @OtherProductGroupID, @DiscountAllow, @ItemDiscAllow,
            @AlreadyDiscQty, @LastTransactionID, @LastComputerID, @PrinterID, @InventoryID, @OrderStaffID, @OrderStaff,
            @OrderComputerID, @OrderComputer, @OrderTableID, @OrderTable, @VoidTypeID, @VoidStaffID, @VoidStaff,
            @VoidDateTime, @VoidManualText, @VoidReasonText, @VATType, @PrintGroup, @NoPrintBill, @NoRePrintOrder,
            @StartTime, @FinishTime, @PrintStatus, @PrintOrderDateTime, @LastPrintOrderDateTime, @PrintErrorMsg,
            @CancelPrintStaffID, @CancelPrintDateTime, @CancelPrintReason, @ProcessID, @InsertOrderDateTime,
            @SubmitOrderDateTime, @ModifyOrderDateTime, @ModifyStaffID, @Comment, @IsComment, @BillCheckID, @PGroupID,
            @SetGroupNo, @QtyRatio, @FreeItem, @SummaryID, @Deleted
        )";

                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@OrderDetailID", orderDetail.OrderDetailID);
                        command.Parameters.AddWithValue("@TransactionID", orderDetail.TransactionID);
                        command.Parameters.AddWithValue("@ComputerID", orderDetail.ComputerID);
                        command.Parameters.AddWithValue("@TranKey", orderDetail.TranKey);
                        command.Parameters.AddWithValue("@OrgOrderDetailID", orderDetail.OrgOrderDetailID);
                        command.Parameters.AddWithValue("@OrgTransactionID", orderDetail.OrgTransactionID);
                        command.Parameters.AddWithValue("@OrgComputerID", orderDetail.OrgComputerID);
                        command.Parameters.AddWithValue("@OrgTranKey", orderDetail.OrgTranKey);
                        command.Parameters.AddWithValue("@ComponentLevel", orderDetail.ComponentLevel);
                        command.Parameters.AddWithValue("@OrderDetailLinkID", orderDetail.OrderDetailLinkID);
                        command.Parameters.AddWithValue("@InsertOrderNo", orderDetail.InsertOrderNo);
                        command.Parameters.AddWithValue("@IndentLevel", orderDetail.IndentLevel);
                        command.Parameters.AddWithValue("@DisplayOrdering", orderDetail.DisplayOrdering);
                        command.Parameters.AddWithValue("@SaleDate", orderDetail.SaleDate);
                        command.Parameters.AddWithValue("@ShopID", orderDetail.ShopID);
                        command.Parameters.AddWithValue("@ProductID", orderDetail.ProductID);
                        command.Parameters.AddWithValue("@ProductSetType", orderDetail.ProductSetType);
                        command.Parameters.AddWithValue("@OrderStatusID", orderDetail.OrderStatusID);
                        command.Parameters.AddWithValue("@OrderEditStatus", orderDetail.OrderEditStatus);
                        command.Parameters.AddWithValue("@SaleMode", orderDetail.SaleMode);
                        command.Parameters.AddWithValue("@SaleType", orderDetail.SaleType);
                        command.Parameters.AddWithValue("@TotalQty", orderDetail.TotalQty);
                        command.Parameters.AddWithValue("@PricePerUnit", orderDetail.PricePerUnit);
                        command.Parameters.AddWithValue("@TotalRetailPrice", orderDetail.TotalRetailPrice);
                        command.Parameters.AddWithValue("@OrgPricePerUnit", orderDetail.OrgPricePerUnit);
                        command.Parameters.AddWithValue("@OrgTotalRetailPrice", orderDetail.OrgTotalRetailPrice);
                        command.Parameters.AddWithValue("@DiscPricePercent", orderDetail.DiscPricePercent);
                        command.Parameters.AddWithValue("@DiscPrice", orderDetail.DiscPrice);
                        command.Parameters.AddWithValue("@DiscPercent", orderDetail.DiscPercent);
                        command.Parameters.AddWithValue("@DiscAmount", orderDetail.DiscAmount);
                        command.Parameters.AddWithValue("@DiscOtherPercent", orderDetail.DiscOtherPercent);
                        command.Parameters.AddWithValue("@DiscOther", orderDetail.DiscOther);
                        command.Parameters.AddWithValue("@TotalItemDisc", orderDetail.TotalItemDisc);
                        command.Parameters.AddWithValue("@SalePrice", orderDetail.SalePrice);
                        command.Parameters.AddWithValue("@DiscBill", orderDetail.DiscBill);
                        command.Parameters.AddWithValue("@TotalDiscount", orderDetail.TotalDiscount);
                        command.Parameters.AddWithValue("@NetSale", orderDetail.NetSale);
                        command.Parameters.AddWithValue("@AdjFromSaleType", orderDetail.AdjFromSaleType);
                        command.Parameters.AddWithValue("@Vatable", orderDetail.Vatable);
                        command.Parameters.AddWithValue("@ProductVATCode", orderDetail.ProductVATCode);
                        command.Parameters.AddWithValue("@VATDisplay", orderDetail.VATDisplay);
                        command.Parameters.AddWithValue("@ProductVATPercent", orderDetail.ProductVATPercent);
                        command.Parameters.AddWithValue("@ProductVAT", orderDetail.ProductVAT);
                        command.Parameters.AddWithValue("@ProductBeforeVAT", orderDetail.ProductBeforeVAT);
                        command.Parameters.AddWithValue("@TotalRetailVAT", orderDetail.TotalRetailVAT);
                        command.Parameters.AddWithValue("@DiscVAT", orderDetail.DiscVAT);
                        command.Parameters.AddWithValue("@IsSCBeforeDisc", orderDetail.IsSCBeforeDisc);
                        command.Parameters.AddWithValue("@HasServiceCharge", orderDetail.HasServiceCharge);
                        command.Parameters.AddWithValue("@SCPercent", orderDetail.SCPercent);
                        command.Parameters.AddWithValue("@SCAmount", orderDetail.SCAmount);
                        command.Parameters.AddWithValue("@SCVAT", orderDetail.SCVAT);
                        command.Parameters.AddWithValue("@SCBeforeVAT", orderDetail.SCBeforeVAT);
                        command.Parameters.AddWithValue("@WVatable", orderDetail.WVatable);
                        command.Parameters.AddWithValue("@SCWAmount", orderDetail.SCWAmount);
                        command.Parameters.AddWithValue("@SCWVAT", orderDetail.SCWVAT);
                        command.Parameters.AddWithValue("@SCWBeforeVAT", orderDetail.SCWBeforeVAT);
                        command.Parameters.AddWithValue("@WeightPrice", orderDetail.WeightPrice);
                        command.Parameters.AddWithValue("@WeightPriceVAT", orderDetail.WeightPriceVAT);
                        command.Parameters.AddWithValue("@WeightBeforeVAT", orderDetail.WeightBeforeVAT);
                        command.Parameters.AddWithValue("@PaymentVAT", orderDetail.PaymentVAT);
                        command.Parameters.AddWithValue("@OtherFoodName", orderDetail.OtherFoodName);
                        command.Parameters.AddWithValue("@OtherProductGroupID", orderDetail.OtherProductGroupID);
                        command.Parameters.AddWithValue("@DiscountAllow", orderDetail.DiscountAllow);
                        command.Parameters.AddWithValue("@ItemDiscAllow", orderDetail.ItemDiscAllow);
                        command.Parameters.AddWithValue("@AlreadyDiscQty", orderDetail.AlreadyDiscQty);
                        command.Parameters.AddWithValue("@LastTransactionID", orderDetail.LastTransactionID);
                        command.Parameters.AddWithValue("@LastComputerID", orderDetail.LastComputerID);
                        command.Parameters.AddWithValue("@PrinterID", orderDetail.PrinterID);
                        command.Parameters.AddWithValue("@InventoryID", orderDetail.InventoryID);
                        command.Parameters.AddWithValue("@OrderStaffID", orderDetail.OrderStaffID);
                        command.Parameters.AddWithValue("@OrderStaff", orderDetail.OrderStaff);
                        command.Parameters.AddWithValue("@OrderComputerID", orderDetail.OrderComputerID);
                        command.Parameters.AddWithValue("@OrderComputer", orderDetail.OrderComputer);
                        command.Parameters.AddWithValue("@OrderTableID", orderDetail.OrderTableID);
                        command.Parameters.AddWithValue("@OrderTable", orderDetail.OrderTable);
                        command.Parameters.AddWithValue("@VoidTypeID", orderDetail.VoidTypeID);
                        command.Parameters.AddWithValue("@VoidStaffID", orderDetail.VoidStaffID);
                        command.Parameters.AddWithValue("@VoidStaff", orderDetail.VoidStaff);
                        command.Parameters.AddWithValue("@VoidDateTime", orderDetail.VoidDateTime);
                        command.Parameters.AddWithValue("@VoidManualText", orderDetail.VoidManualText);
                        command.Parameters.AddWithValue("@VoidReasonText", orderDetail.VoidReasonText);
                        command.Parameters.AddWithValue("@VATType", orderDetail.VATType);
                        command.Parameters.AddWithValue("@PrintGroup", orderDetail.PrintGroup);
                        command.Parameters.AddWithValue("@NoPrintBill", orderDetail.NoPrintBill);
                        command.Parameters.AddWithValue("@NoRePrintOrder", orderDetail.NoRePrintOrder);
                        command.Parameters.AddWithValue("@StartTime", orderDetail.StartTime);
                        command.Parameters.AddWithValue("@FinishTime", orderDetail.FinishTime);
                        command.Parameters.AddWithValue("@PrintStatus", orderDetail.PrintStatus);
                        command.Parameters.AddWithValue("@PrintOrderDateTime", orderDetail.PrintOrderDateTime);
                        command.Parameters.AddWithValue("@LastPrintOrderDateTime", orderDetail.LastPrintOrderDateTime);
                        command.Parameters.AddWithValue("@PrintErrorMsg", orderDetail.PrintErrorMsg);
                        command.Parameters.AddWithValue("@CancelPrintStaffID", orderDetail.CancelPrintStaffID);
                        command.Parameters.AddWithValue("@CancelPrintDateTime", orderDetail.CancelPrintDateTime);
                        command.Parameters.AddWithValue("@CancelPrintReason", orderDetail.CancelPrintReason);
                        command.Parameters.AddWithValue("@ProcessID", orderDetail.ProcessID);
                        command.Parameters.AddWithValue("@InsertOrderDateTime", orderDetail.InsertOrderDateTime);
                        command.Parameters.AddWithValue("@SubmitOrderDateTime", orderDetail.SubmitOrderDateTime);
                        command.Parameters.AddWithValue("@ModifyOrderDateTime", orderDetail.ModifyOrderDateTime);
                        command.Parameters.AddWithValue("@ModifyStaffID", orderDetail.ModifyStaffID);
                        command.Parameters.AddWithValue("@Comment", orderDetail.Comment);
                        command.Parameters.AddWithValue("@IsComment", orderDetail.IsComment);
                        command.Parameters.AddWithValue("@BillCheckID", orderDetail.BillCheckID);
                        command.Parameters.AddWithValue("@PGroupID", orderDetail.PGroupID);
                        command.Parameters.AddWithValue("@SetGroupNo", orderDetail.SetGroupNo);
                        command.Parameters.AddWithValue("@QtyRatio", orderDetail.QtyRatio);
                        command.Parameters.AddWithValue("@FreeItem", orderDetail.FreeItem);
                        command.Parameters.AddWithValue("@SummaryID", orderDetail.SummaryID);
                        command.Parameters.AddWithValue("@Deleted", orderDetail.Deleted);

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
