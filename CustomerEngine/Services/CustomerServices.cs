
    using AccessEngine.Services;
using CustomerEngine.Models;

namespace CustomerEngine.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly IAccessService _accessService;

    public CustomerService(IAccessService accessService) => _accessService = accessService;

    public async Task<CustomerPagedResultModel> GetPagedAsync(CustomerPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);

        var rows = (await _accessService.QueryAsync<CustomerPagedRow, object>("dbo.spCustomerGetPaged", new
        {
            request.PageNumber,
            request.PageSize,
            SearchText = CustomerValidation.TrimOrNull(request.SearchText),
            MemberType = CustomerValidation.TrimOrNull(request.MemberType),
            request.MemberLevelId,
            request.IsActive,
            CreditStatus = CustomerValidation.TrimOrNull(request.CreditStatus)
        }, cancellationToken)).ToArray();

        return new CustomerPagedResultModel
        {
            Customers = rows,
            TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public Task<CustomerModel?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return _accessService.QuerySingleOrDefaultAsync<CustomerModel, object>("dbo.spCustomerGetById", new { CustomerId = customerId }, cancellationToken);
    }

    public Task<CustomerModel?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequireText(phoneNumber, nameof(phoneNumber));
        return _accessService.QuerySingleOrDefaultAsync<CustomerModel, object>("dbo.spCustomerGetByPhoneNumber", new { PhoneNumber = phoneNumber.Trim() }, cancellationToken);
    }

    public Task<int> CreateAsync(CustomerCreateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequireText(model.CustomerName, nameof(model.CustomerName));
        CustomerValidation.RequireText(model.PhoneNumber, nameof(model.PhoneNumber));
        CustomerValidation.RequirePositive(model.CreatedByUserId, nameof(model.CreatedByUserId));

        return _accessService.QuerySingleAsync<int, object>("dbo.spCustomerCreate", new
        {
            CustomerCode = CustomerValidation.TrimOrNull(model.CustomerCode),
            CustomerName = model.CustomerName.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            Email = CustomerValidation.TrimOrNull(model.Email),
            MemberType = ValidateMemberType(model.MemberType),
            model.MemberLevelId,
            model.DateOfBirth,
            Gender = CustomerValidation.TrimOrNull(model.Gender),
            Address = CustomerValidation.TrimOrNull(model.Address),
            model.CreatedByUserId
        }, cancellationToken);
    }

    public Task UpdateAsync(CustomerUpdateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.CustomerId, nameof(model.CustomerId));
        CustomerValidation.RequireText(model.CustomerName, nameof(model.CustomerName));
        CustomerValidation.RequireText(model.PhoneNumber, nameof(model.PhoneNumber));
        CustomerValidation.RequirePositive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));

        return _accessService.ExecuteAsync("dbo.spCustomerUpdate", new
        {
            model.CustomerId,
            CustomerName = model.CustomerName.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            Email = CustomerValidation.TrimOrNull(model.Email),
            MemberType = ValidateMemberType(model.MemberType),
            model.MemberLevelId,
            model.DateOfBirth,
            Gender = CustomerValidation.TrimOrNull(model.Gender),
            Address = CustomerValidation.TrimOrNull(model.Address),
            model.ApplyMemberLevelCreditDefault,
            model.UpdatedByUserId
        }, cancellationToken);
    }

    public Task ToggleActiveAsync(int customerId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequirePositive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerToggleActive", new { CustomerId = customerId, IsActive = isActive, UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    public Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, int? excludeCustomerId = null, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequireText(phoneNumber, nameof(phoneNumber));
        return _accessService.QuerySingleAsync<bool, object>("dbo.spCustomerIsPhoneNumberExists", new { PhoneNumber = phoneNumber.Trim(), ExcludeCustomerId = excludeCustomerId }, cancellationToken);
    }

    public Task<bool> IsEmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequireText(email, nameof(email));
        return _accessService.QuerySingleAsync<bool, object>("dbo.spCustomerIsEmailExists", new { Email = email.Trim(), ExcludeCustomerId = excludeCustomerId }, cancellationToken);
    }

    public Task UpdatePurchaseSummaryAsync(int customerId, decimal saleAmount, DateTime purchaseDate, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequireNonNegative(saleAmount, nameof(saleAmount));
        return _accessService.ExecuteAsync("dbo.spCustomerUpdatePurchaseSummary", new { CustomerId = customerId, SaleAmount = saleAmount, PurchaseDate = purchaseDate }, cancellationToken);
    }

    private static string ValidateMemberType(string? memberType)
    {
        var value = string.IsNullOrWhiteSpace(memberType) ? "Retail" : memberType.Trim();
        if (value is not ("Retail" or "Wholesale")) throw new ArgumentException("Member type must be Retail or Wholesale.", nameof(memberType));
        return value;
    }
}

public sealed class CustomerMemberTypeService : ICustomerMemberTypeService
{
    private readonly IAccessService _accessService;

    public CustomerMemberTypeService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<MemberTypeModel>> GetAllActiveTypesAsync(CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<MemberTypeModel, object>("dbo.spCustomerMemberTypeGetAllActive", new { }, cancellationToken)).ToArray();

    public async Task<IReadOnlyCollection<CustomerMembershipModel>> GetActiveMembershipsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return (await _accessService.QueryAsync<CustomerMembershipModel, object>("dbo.spCustomerMemberTypeGetByCustomerId", new { CustomerId = customerId }, cancellationToken)).ToArray();
    }

    public Task<bool> HasActiveMembershipAsync(int customerId, string memberTypeCode, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequireText(memberTypeCode, nameof(memberTypeCode));
        return _accessService.QuerySingleAsync<bool, object>("dbo.spCustomerMemberTypeCheckActiveMembership", new { CustomerId = customerId, MemberTypeCode = CustomerMembershipText.NormalizeCode(memberTypeCode) }, cancellationToken);
    }

    public Task AssignAsync(int customerId, string memberTypeCode, int createdByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequireText(memberTypeCode, nameof(memberTypeCode));
        CustomerValidation.RequirePositive(createdByUserId, nameof(createdByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerMemberTypeAssign", new { CustomerId = customerId, MemberTypeCode = CustomerMembershipText.NormalizeCode(memberTypeCode), CreatedByUserId = createdByUserId }, cancellationToken);
    }

    public Task DeactivateAsync(int customerId, string memberTypeCode, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequireText(memberTypeCode, nameof(memberTypeCode));
        CustomerValidation.RequirePositive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerMemberTypeDeactivate", new { CustomerId = customerId, MemberTypeCode = CustomerMembershipText.NormalizeCode(memberTypeCode), UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    public Task SyncAsync(int customerId, IReadOnlyCollection<string> memberTypeCodes, WholesaleMemberProfileSaveModel? wholesaleProfile, RubberSupplierMemberProfileSaveModel? rubberSupplierProfile, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequirePositive(updatedByUserId, nameof(updatedByUserId));
        var codes = NormalizeMemberCodes(memberTypeCodes);
        return _accessService.ExecuteAsync("dbo.spCustomerMemberTypeSync", new
        {
            CustomerId = customerId,
            MemberTypeCodesCsv = string.Join(",", codes),
            WholesaleBusinessName = CustomerValidation.TrimOrNull(wholesaleProfile?.BusinessName),
            wholesaleProfile?.WholesaleLevelId,
            WholesaleIsApproved = wholesaleProfile?.IsApproved ?? false,
            WholesalePaymentTermDays = wholesaleProfile?.PaymentTermDays ?? 0,
            WholesaleApprovedByUserId = wholesaleProfile?.ApprovedByUserId,
            SupplierCode = CustomerValidation.TrimOrNull(rubberSupplierProfile?.SupplierCode),
            rubberSupplierProfile?.DefaultBusinessLocationId,
            SupplierRemark = CustomerValidation.TrimOrNull(rubberSupplierProfile?.Remark),
            UpdatedByUserId = updatedByUserId
        }, cancellationToken);
    }

    internal static IReadOnlyCollection<string> NormalizeMemberCodes(IReadOnlyCollection<string>? memberTypeCodes)
    {
        var codes = (memberTypeCodes ?? Array.Empty<string>())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(CustomerMembershipText.NormalizeCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return codes.Length == 0 ? new[] { MemberTypeCodes.Retail } : codes;
    }
}

public sealed class CustomerRegistrationService : ICustomerRegistrationService
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerMemberTypeService _memberTypeService;

    public CustomerRegistrationService(ICustomerService customerService, ICustomerMemberTypeService memberTypeService)
    {
        _customerService = customerService;
        _memberTypeService = memberTypeService;
    }

    public async Task<int> RegisterAsync(CustomerCreateModel model, CancellationToken cancellationToken = default)
    {
        var codes = CustomerMemberTypeService.NormalizeMemberCodes(model.MemberTypeCodes);
        var customerId = await _customerService.CreateAsync(new CustomerCreateModel
        {
            CustomerCode = model.CustomerCode,
            CustomerName = model.CustomerName,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            MemberType = LegacyMemberType(codes),
            MemberTypeCodes = codes,
            WholesaleProfile = model.WholesaleProfile,
            RubberSupplierProfile = model.RubberSupplierProfile,
            MemberLevelId = model.MemberLevelId,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Address = model.Address,
            CreatedByUserId = model.CreatedByUserId
        }, cancellationToken);
        await _memberTypeService.SyncAsync(customerId, codes, model.WholesaleProfile, model.RubberSupplierProfile, model.CreatedByUserId, cancellationToken);
        return customerId;
    }

    public async Task UpdateMembershipsAsync(CustomerUpdateModel model, CancellationToken cancellationToken = default)
    {
        var codes = CustomerMemberTypeService.NormalizeMemberCodes(model.MemberTypeCodes);
        await _customerService.UpdateAsync(new CustomerUpdateModel
        {
            CustomerId = model.CustomerId,
            CustomerName = model.CustomerName,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            MemberType = LegacyMemberType(codes),
            MemberTypeCodes = codes,
            WholesaleProfile = model.WholesaleProfile,
            RubberSupplierProfile = model.RubberSupplierProfile,
            MemberLevelId = model.MemberLevelId,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Address = model.Address,
            ApplyMemberLevelCreditDefault = model.ApplyMemberLevelCreditDefault,
            UpdatedByUserId = model.UpdatedByUserId
        }, cancellationToken);
        await _memberTypeService.SyncAsync(model.CustomerId, codes, model.WholesaleProfile, model.RubberSupplierProfile, model.UpdatedByUserId, cancellationToken);
    }

    private static string LegacyMemberType(IReadOnlyCollection<string> codes) =>
        codes.Contains(MemberTypeCodes.Wholesale, StringComparer.OrdinalIgnoreCase) ? "Wholesale" : "Retail";
}

public sealed class MemberLoyaltyService : IMemberLoyaltyService
{
    private readonly IAccessService _accessService;

    public MemberLoyaltyService(IAccessService accessService) => _accessService = accessService;

    public Task<MemberLoyaltyAccountModel?> GetAccountAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return _accessService.QuerySingleOrDefaultAsync<MemberLoyaltyAccountModel, object>("dbo.spMemberLoyaltyAccountGetByCustomerId", new { CustomerId = customerId }, cancellationToken);
    }

    public Task<LoyaltyPointCalculationResultModel> CalculateFromRubberWeightAsync(int customerId, decimal rubberWeightKg, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        if (rubberWeightKg <= 0) throw new ArgumentException("Rubber weight must be greater than 0.", nameof(rubberWeightKg));
        return _accessService.QuerySingleAsync<LoyaltyPointCalculationResultModel, object>("dbo.spMemberLoyaltyCalculateFromRubberWeight", new { CustomerId = customerId, RubberWeightKg = rubberWeightKg }, cancellationToken);
    }

    public Task<MemberLoyaltyTransactionModel> AddPointsFromRubberPurchaseAsync(int customerId, long rubberPurchaseHeaderId, decimal confirmedWeightKg, int employeeId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequirePositive(rubberPurchaseHeaderId, nameof(rubberPurchaseHeaderId));
        if (confirmedWeightKg <= 0) throw new ArgumentException("Confirmed weight must be greater than 0.", nameof(confirmedWeightKg));
        CustomerValidation.RequirePositive(employeeId, nameof(employeeId));
        return _accessService.QuerySingleAsync<MemberLoyaltyTransactionModel, object>("dbo.spMemberLoyaltyAddFromRubberPurchase", new { CustomerId = customerId, RubberPurchaseHeaderId = rubberPurchaseHeaderId, ConfirmedWeightKg = confirmedWeightKg, CreatedByUserId = employeeId }, cancellationToken);
    }

    public Task RedeemPointsAsync(int customerId, int points, string? remark, int employeeId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequirePositive(points, nameof(points));
        CustomerValidation.RequirePositive(employeeId, nameof(employeeId));
        return _accessService.ExecuteAsync("dbo.spMemberLoyaltyRedeem", new { CustomerId = customerId, Points = points, Remark = CustomerValidation.TrimOrNull(remark), CreatedByUserId = employeeId }, cancellationToken);
    }

    public Task ReversePointsFromCancelledRubberPurchaseAsync(long rubberPurchaseHeaderId, int employeeId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(rubberPurchaseHeaderId, nameof(rubberPurchaseHeaderId));
        CustomerValidation.RequirePositive(employeeId, nameof(employeeId));
        return _accessService.ExecuteAsync("dbo.spMemberLoyaltyReverseRubberPurchase", new { RubberPurchaseHeaderId = rubberPurchaseHeaderId, CreatedByUserId = employeeId }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MemberLoyaltyTransactionModel>> GetTransactionsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return (await _accessService.QueryAsync<MemberLoyaltyTransactionModel, object>("dbo.spMemberLoyaltyTransactionGetByCustomerId", new { CustomerId = customerId }, cancellationToken)).ToArray();
    }
}

public sealed class MemberSalesCreditService : IMemberSalesCreditService
{
    private readonly ICustomerCreditService _creditService;

    public MemberSalesCreditService(ICustomerCreditService creditService) => _creditService = creditService;

    public Task<CustomerCreditModel?> GetAccountAsync(int customerId, CancellationToken cancellationToken = default) =>
        _creditService.GetByCustomerIdAsync(customerId, cancellationToken);

    public async Task<decimal> GetAvailableCreditAsync(int customerId, CancellationToken cancellationToken = default) =>
        (await _creditService.GetByCustomerIdAsync(customerId, cancellationToken))?.AvailableCredit ?? 0m;

    public async Task<bool> HasEnoughCreditAsync(int customerId, decimal requestedAmount, CancellationToken cancellationToken = default) =>
        (await _creditService.CheckEligibilityAsync(customerId, requestedAmount, cancellationToken)).IsAllowed;

    public Task CreateOrUpdateCreditApprovalAsync(CustomerCreditUpdateModel model, CancellationToken cancellationToken = default) =>
        _creditService.SetCreditAsync(model, cancellationToken);

    public Task<long> UseCreditForSaleAsync(int customerId, long saleId, decimal amount, int employeeId, CancellationToken cancellationToken = default) =>
        _creditService.CreateCreditSaleAsync(customerId, saleId, amount, null, false, "Sales credit used for POS sale.", employeeId, cancellationToken);

    public Task RecordRepaymentAsync(int customerId, decimal amount, string? paymentMethod, int employeeId, CancellationToken cancellationToken = default) =>
        _creditService.ReceivePaymentAsync(new CustomerCreditPaymentModel { CustomerId = customerId, Amount = amount, ReferenceNo = paymentMethod, CreatedByUserId = employeeId }, cancellationToken);

    public async Task AdjustCreditLimitAsync(int customerId, decimal newLimit, string reason, int approvedByEmployeeId, CancellationToken cancellationToken = default)
    {
        var account = await _creditService.GetByCustomerIdAsync(customerId, cancellationToken);
        await _creditService.SetCreditAsync(new CustomerCreditUpdateModel
        {
            CustomerId = customerId,
            AllowCredit = true,
            CreditLimit = newLimit,
            CreditTermDays = account?.CreditTermDays ?? 0,
            CreditStatus = account?.CreditStatus ?? "Good",
            RequireManagerApproval = account?.RequireManagerApproval ?? false,
            ApprovedByUserId = approvedByEmployeeId,
            Remark = reason,
            UpdatedByUserId = approvedByEmployeeId
        }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerCreditTransactionModel>> GetTransactionsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await _creditService.GetTransactionsPagedAsync(new CustomerCreditTransactionPagedRequestModel { CustomerId = customerId, PageNumber = 1, PageSize = 500 }, cancellationToken);
        return result.Transactions;
    }
}

public sealed class RubberPurchaseService : IRubberPurchaseService
{
    private readonly IAccessService _accessService;

    public RubberPurchaseService(IAccessService accessService) => _accessService = accessService;

    public Task<long> CreateAsync(RubberPurchaseHeaderCreateModel model, CancellationToken cancellationToken = default)
    {
        if (model.CustomerId.GetValueOrDefault() <= 0 && string.IsNullOrWhiteSpace(model.NonMemberFarmerName))
        {
            throw new ArgumentException("A member supplier or non-member farmer name is required.", nameof(model.CustomerId));
        }

        if (model.CustomerId.GetValueOrDefault() > 0 && !string.IsNullOrWhiteSpace(model.NonMemberFarmerName))
        {
            throw new ArgumentException("Use either a member supplier or non-member farmer details, not both.", nameof(model.CustomerId));
        }

        CustomerValidation.RequirePositive(model.BusinessLocationId, nameof(model.BusinessLocationId));
        CustomerValidation.RequirePositive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        if (model.WeightKg <= 0) throw new ArgumentException("Rubber weight must be greater than 0.", nameof(model.WeightKg));
        if (model.TotalAmount is < 0) throw new ArgumentException("Total amount cannot be negative.", nameof(model.TotalAmount));

        return _accessService.QuerySingleAsync<long, object>("dbo.spRubberPurchaseHeaderCreate", new
        {
            model.CustomerId,
            NonMemberFarmerName = CustomerValidation.TrimOrNull(model.NonMemberFarmerName),
            NonMemberFarmerPhone = CustomerValidation.TrimOrNull(model.NonMemberFarmerPhone),
            model.BusinessLocationId,
            model.RubberAuctionLocationId,
            model.TransactionDate,
            model.WeightKg,
            model.RubberPriceId,
            model.MarketingPriceId,
            model.PricePerKgSnapshot,
            model.PercentageSnapshot,
            model.TotalAmount,
            PaymentStatus = CustomerValidation.TrimOrNull(model.PaymentStatus) ?? "Pending",
            model.CreatedByUserId
        }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<long>> CreateBatchAsync(RubberPurchaseBatchCreateModel model, CancellationToken cancellationToken = default)
    {
        if (model.Purchases.Count == 0)
        {
            throw new ArgumentException("At least one rubber purchase row is required.", nameof(model.Purchases));
        }

        var ids = new List<long>(model.Purchases.Count);
        foreach (var purchase in model.Purchases)
        {
            ids.Add(await CreateAsync(purchase, cancellationToken));
        }

        return ids;
    }

    public async Task<RubberPurchaseHeaderPagedModel> GetPagedAsync(RubberPurchaseHeaderPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<RubberPurchaseHeaderPagedRow, object>("dbo.spRubberPurchaseHeaderGetPaged", new
        {
            request.PageNumber,
            request.PageSize,
            request.CustomerId,
            request.BusinessLocationId,
            request.RubberAuctionLocationId,
            PaymentStatus = CustomerValidation.TrimOrNull(request.PaymentStatus),
            request.DateFrom,
            request.DateTo,
            SearchText = CustomerValidation.TrimOrNull(request.SearchText)
        }, cancellationToken)).ToArray();

        return new RubberPurchaseHeaderPagedModel
        {
            Items = rows,
            TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public Task<RubberPurchaseHeaderModel?> GetByIdAsync(long rubberPurchaseHeaderId, CancellationToken cancellationToken = default)
    {
        if (rubberPurchaseHeaderId <= 0) throw new ArgumentException("Rubber purchase id must be greater than 0.", nameof(rubberPurchaseHeaderId));
        return _accessService.QuerySingleOrDefaultAsync<RubberPurchaseHeaderModel, object>("dbo.spRubberPurchaseHeaderGetById", new { RubberPurchaseHeaderId = rubberPurchaseHeaderId }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RubberPurchaseHeaderModel>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return (await _accessService.QueryAsync<RubberPurchaseHeaderModel, object>("dbo.spRubberPurchaseHeaderGetByCustomerId", new { CustomerId = customerId }, cancellationToken)).ToArray();
    }

    public Task<RubberPurchaseHeaderModel> PayBillAsync(RubberPurchasePayBillModel model, CancellationToken cancellationToken = default)
    {
        if (model.RubberPurchaseHeaderId <= 0) throw new ArgumentException("Rubber purchase id must be greater than 0.", nameof(model.RubberPurchaseHeaderId));
        CustomerValidation.RequireNonNegative(model.PaidAmount, nameof(model.PaidAmount));
        CustomerValidation.RequireNonNegative(model.CreditDeductedAmount, nameof(model.CreditDeductedAmount));
        if (model.PaidAmount + model.CreditDeductedAmount <= 0) throw new ArgumentException("Payment settlement amount must be greater than 0.", nameof(model.PaidAmount));
        CustomerValidation.RequirePositive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.QuerySingleAsync<RubberPurchaseHeaderModel, object>("dbo.spRubberPurchaseHeaderPayBill", new
        {
            model.RubberPurchaseHeaderId,
            model.PaidAmount,
            model.CreditDeductedAmount,
            PaymentMethod = CustomerValidation.TrimOrNull(model.PaymentMethod) ?? "Cash",
            PaymentRemark = CustomerValidation.TrimOrNull(model.PaymentRemark),
            model.UpdatedByUserId
        }, cancellationToken);
    }
}

public sealed class RubberPriceService : IRubberPriceService
{
    private readonly IAccessService _accessService;

    public RubberPriceService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<RubberPriceModel>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<RubberPriceModel, object>("dbo.spRubberPriceGetAll", new { }, cancellationToken)).ToArray();

    public async Task<IReadOnlyCollection<RubberPriceModel>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<RubberPriceModel, object>("dbo.spRubberPriceGetActive", new { }, cancellationToken)).ToArray();

    public Task<RubberPriceModel?> GetByIdAsync(int rubberPriceId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(rubberPriceId, nameof(rubberPriceId));
        return _accessService.QuerySingleOrDefaultAsync<RubberPriceModel, object>("dbo.spRubberPriceGetById", new { RubberPriceId = rubberPriceId }, cancellationToken);
    }

    public Task<int> CreateAsync(RubberPriceSaveModel model, CancellationToken cancellationToken = default)
    {
        ValidateRubberPrice(model.PricePerKg, model.PercentageOfService);
        return _accessService.QuerySingleAsync<int, object>("dbo.spRubberPriceCreate", new
        {
            model.RubberAuctionLocationId,
            model.PricePerKg,
            model.PercentageOfService,
            model.IsActive
        }, cancellationToken);
    }

    public Task UpdateAsync(RubberPriceUpdateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.RubberPriceId, nameof(model.RubberPriceId));
        ValidateRubberPrice(model.PricePerKg, model.PercentageOfService);
        return _accessService.ExecuteAsync("dbo.spRubberPriceUpdate", new
        {
            model.RubberPriceId,
            model.RubberAuctionLocationId,
            model.PricePerKg,
            model.PercentageOfService,
            model.IsActive
        }, cancellationToken);
    }

    public Task ToggleActiveAsync(int rubberPriceId, bool isActive, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(rubberPriceId, nameof(rubberPriceId));
        return _accessService.ExecuteAsync("dbo.spRubberPriceToggleActive", new { RubberPriceId = rubberPriceId, IsActive = isActive }, cancellationToken);
    }

    public Task HardDeleteAsync(int rubberPriceId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(rubberPriceId, nameof(rubberPriceId));
        return _accessService.ExecuteAsync("dbo.spRubberPriceHardDelete", new { RubberPriceId = rubberPriceId }, cancellationToken);
    }

    private static void ValidateRubberPrice(decimal pricePerKg, decimal percentageOfService)
    {
        if (pricePerKg < 0) throw new ArgumentException("Rubber price cannot be negative.", nameof(pricePerKg));
        if (percentageOfService < 0 || percentageOfService > 100) throw new ArgumentException("Service percentage must be between 0 and 100.", nameof(percentageOfService));
    }
}

public sealed class RubberAuctionLocationService : IRubberAuctionLocationService
{
    private readonly IAccessService _accessService;

    public RubberAuctionLocationService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<RubberAuctionLocationModel>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<RubberAuctionLocationModel, object>("dbo.spRubberAuctionLocationGetAll", new { }, cancellationToken)).ToArray();

    public async Task<IReadOnlyCollection<RubberAuctionLocationModel>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<RubberAuctionLocationModel, object>("dbo.spRubberAuctionLocationGetActive", new { }, cancellationToken)).ToArray();
}

public sealed class MemberLevelService : IMemberLevelService
{
    private readonly IAccessService _accessService;

    public MemberLevelService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<MemberLevelModel>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<MemberLevelModel, object>("dbo.spMemberLevelGetAll", new { }, cancellationToken)).ToArray();

    public async Task<IReadOnlyCollection<MemberLevelModel>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<MemberLevelModel, object>("dbo.spMemberLevelGetAllActive", new { }, cancellationToken)).ToArray();

    public Task<MemberLevelModel?> GetByIdAsync(int memberLevelId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(memberLevelId, nameof(memberLevelId));
        return _accessService.QuerySingleOrDefaultAsync<MemberLevelModel, object>("dbo.spMemberLevelGetById", new { MemberLevelId = memberLevelId }, cancellationToken);
    }

    public Task<int> CreateAsync(MemberLevelCreateModel model, CancellationToken cancellationToken = default)
    {
        ValidateMemberLevel(model.LevelCode, model.LevelName, model.DiscountPercent, model.MinSpendingAmount, model.PointEarnAmount, model.PointEarnPoint, model.PointMultiplier, model.AllowCredit, model.DefaultCreditLimit, model.DefaultCreditTermDays, model.MaxOverdueDaysAllowed, model.CreatedByUserId);
        return _accessService.QuerySingleAsync<int, object>("dbo.spMemberLevelCreate", MemberLevelParameters(model), cancellationToken);
    }

    public Task UpdateAsync(MemberLevelUpdateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.MemberLevelId, nameof(model.MemberLevelId));
        ValidateMemberLevel(model.LevelCode, model.LevelName, model.DiscountPercent, model.MinSpendingAmount, model.PointEarnAmount, model.PointEarnPoint, model.PointMultiplier, model.AllowCredit, model.DefaultCreditLimit, model.DefaultCreditTermDays, model.MaxOverdueDaysAllowed, model.UpdatedByUserId);
        return _accessService.ExecuteAsync("dbo.spMemberLevelUpdate", new
        {
            model.MemberLevelId,
            model.LevelCode,
            model.LevelName,
            model.Description,
            model.MinSpendingAmount,
            model.DiscountPercent,
            model.PointEarnAmount,
            model.PointEarnPoint,
            model.PointMultiplier,
            model.AllowCredit,
            model.DefaultCreditLimit,
            model.DefaultCreditTermDays,
            model.RequireManagerApprovalForCredit,
            model.MaxOverdueDaysAllowed,
            model.DisplayOrder,
            model.IsActive,
            model.UpdatedByUserId
        }, cancellationToken);
    }

    public Task ToggleActiveAsync(int memberLevelId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(memberLevelId, nameof(memberLevelId));
        CustomerValidation.RequirePositive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spMemberLevelToggleActive", new { MemberLevelId = memberLevelId, IsActive = isActive, UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    public Task<bool> IsLevelCodeExistsAsync(string levelCode, int? excludeMemberLevelId = null, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequireText(levelCode, nameof(levelCode));
        return _accessService.QuerySingleAsync<bool, object>("dbo.spMemberLevelIsCodeExists", new { LevelCode = levelCode.Trim(), ExcludeMemberLevelId = excludeMemberLevelId }, cancellationToken);
    }

    private static object MemberLevelParameters(MemberLevelCreateModel model) => new
    {
        LevelCode = model.LevelCode.Trim(),
        LevelName = model.LevelName.Trim(),
        Description = CustomerValidation.TrimOrNull(model.Description),
        model.MinSpendingAmount,
        model.DiscountPercent,
        model.PointEarnAmount,
        model.PointEarnPoint,
        model.PointMultiplier,
        model.AllowCredit,
        model.DefaultCreditLimit,
        model.DefaultCreditTermDays,
        model.RequireManagerApprovalForCredit,
        model.MaxOverdueDaysAllowed,
        model.DisplayOrder,
        model.CreatedByUserId
    };

    private static void ValidateMemberLevel(string levelCode, string levelName, decimal discountPercent, decimal minSpendingAmount, decimal pointEarnAmount, decimal pointEarnPoint, decimal pointMultiplier, bool allowCredit, decimal creditLimit, int creditTermDays, int maxOverdueDaysAllowed, int userId)
    {
        CustomerValidation.RequireText(levelCode, nameof(levelCode));
        CustomerValidation.RequireText(levelName, nameof(levelName));
        CustomerValidation.RequirePositive(userId, nameof(userId));
        if (discountPercent is < 0 or > 100) throw new ArgumentException("Discount percent must be between 0 and 100.", nameof(discountPercent));
        CustomerValidation.RequireNonNegative(minSpendingAmount, nameof(minSpendingAmount));
        if (pointEarnAmount <= 0) throw new ArgumentException("Point earn amount must be greater than 0.", nameof(pointEarnAmount));
        CustomerValidation.RequireNonNegative(pointEarnPoint, nameof(pointEarnPoint));
        if (pointMultiplier <= 0) throw new ArgumentException("Point multiplier must be greater than 0.", nameof(pointMultiplier));
        CustomerValidation.RequireNonNegative(creditLimit, nameof(creditLimit));
        if (creditTermDays < 0) throw new ArgumentException("Credit term days cannot be negative.", nameof(creditTermDays));
        if (maxOverdueDaysAllowed < 0) throw new ArgumentException("Max overdue days allowed cannot be negative.", nameof(maxOverdueDaysAllowed));
        if (!allowCredit && (creditLimit != 0 || creditTermDays != 0)) throw new ArgumentException("Credit limit and term must be zero when credit is not allowed.");
    }
}

public sealed class MemberLevelUpgradeRuleService : IMemberLevelUpgradeRuleService
{
    private readonly IAccessService _accessService;

    public MemberLevelUpgradeRuleService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<MemberLevelUpgradeRuleModel>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<MemberLevelUpgradeRuleModel, object>("dbo.spMemberLevelUpgradeRuleGetAll", new { }, cancellationToken)).ToArray();

    public Task<MemberLevelUpgradeRuleModel?> GetByIdAsync(int memberLevelUpgradeRuleId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(memberLevelUpgradeRuleId, nameof(memberLevelUpgradeRuleId));
        return _accessService.QuerySingleOrDefaultAsync<MemberLevelUpgradeRuleModel, object>("dbo.spMemberLevelUpgradeRuleGetById", new { MemberLevelUpgradeRuleId = memberLevelUpgradeRuleId }, cancellationToken);
    }

    public Task<MemberLevelUpgradeRuleModel?> GetByFromLevelIdAsync(int fromMemberLevelId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(fromMemberLevelId, nameof(fromMemberLevelId));
        return _accessService.QuerySingleOrDefaultAsync<MemberLevelUpgradeRuleModel, object>("dbo.spMemberLevelUpgradeRuleGetByFromLevelId", new { FromMemberLevelId = fromMemberLevelId }, cancellationToken);
    }

    public Task<int> CreateAsync(MemberLevelUpgradeRuleCreateModel model, CancellationToken cancellationToken = default)
    {
        ValidateRule(model.FromMemberLevelId, model.ToMemberLevelId, model.RequiredTotalSpending, model.RequiredPurchaseCount, model.RequiredMembershipDays, model.CreatedByUserId);
        return _accessService.QuerySingleAsync<int, object>("dbo.spMemberLevelUpgradeRuleCreate", model, cancellationToken);
    }

    public Task UpdateAsync(MemberLevelUpgradeRuleUpdateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.MemberLevelUpgradeRuleId, nameof(model.MemberLevelUpgradeRuleId));
        ValidateRule(model.FromMemberLevelId, model.ToMemberLevelId, model.RequiredTotalSpending, model.RequiredPurchaseCount, model.RequiredMembershipDays, model.UpdatedByUserId);
        return _accessService.ExecuteAsync("dbo.spMemberLevelUpgradeRuleUpdate", model, cancellationToken);
    }

    public Task ToggleActiveAsync(int memberLevelUpgradeRuleId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(memberLevelUpgradeRuleId, nameof(memberLevelUpgradeRuleId));
        CustomerValidation.RequirePositive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spMemberLevelUpgradeRuleToggleActive", new { MemberLevelUpgradeRuleId = memberLevelUpgradeRuleId, IsActive = isActive, UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    public Task<CustomerLevelEligibilityResultModel> CheckCustomerLevelEligibilityAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return _accessService.QuerySingleAsync<CustomerLevelEligibilityResultModel, object>("dbo.spCustomerCheckLevelEligibility", new { CustomerId = customerId }, cancellationToken);
    }

    public Task UpgradeCustomerLevelAsync(int customerId, int changedByUserId, bool applyMemberLevelCreditDefault = true, bool managerApproved = false, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequirePositive(changedByUserId, nameof(changedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerUpgradeLevel", new { CustomerId = customerId, ChangedByUserId = changedByUserId, ApplyMemberLevelCreditDefault = applyMemberLevelCreditDefault, ManagerApproved = managerApproved }, cancellationToken);
    }

    private static void ValidateRule(int fromLevelId, int toLevelId, decimal spending, int count, int days, int userId)
    {
        CustomerValidation.RequirePositive(fromLevelId, nameof(fromLevelId));
        CustomerValidation.RequirePositive(toLevelId, nameof(toLevelId));
        if (fromLevelId == toLevelId) throw new ArgumentException("From and to member levels cannot be the same.");
        CustomerValidation.RequireNonNegative(spending, nameof(spending));
        if (count < 0) throw new ArgumentException("Required purchase count cannot be negative.", nameof(count));
        if (days < 0) throw new ArgumentException("Required membership days cannot be negative.", nameof(days));
        CustomerValidation.RequirePositive(userId, nameof(userId));
    }
}

public sealed class LoyaltyPointService : ILoyaltyPointService
{
    private readonly IAccessService _accessService;

    public LoyaltyPointService(IAccessService accessService) => _accessService = accessService;

    public Task<CustomerPointBalanceModel?> GetBalanceAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return _accessService.QuerySingleOrDefaultAsync<CustomerPointBalanceModel, object>("dbo.spCustomerPointGetBalance", new { CustomerId = customerId }, cancellationToken);
    }

    public async Task<LoyaltyPointPagedResultModel> GetMovementsPagedAsync(LoyaltyPointPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(request.CustomerId, nameof(request.CustomerId));
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<CustomerPointMovementPagedRow, object>("dbo.spCustomerPointGetMovementsPaged", new
        {
            request.CustomerId,
            request.PageNumber,
            request.PageSize,
            request.DateFrom,
            request.DateTo,
            MovementType = CustomerValidation.TrimOrNull(request.MovementType)
        }, cancellationToken)).ToArray();

        return new LoyaltyPointPagedResultModel { Movements = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public Task<decimal> EarnPointsAsync(int customerId, decimal saleAmount, string? referenceType, long? referenceId, string? referenceNo, DateTime? expiryDate, int createdByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        if (saleAmount <= 0) throw new ArgumentException("Sale amount must be greater than 0.", nameof(saleAmount));
        CustomerValidation.RequirePositive(createdByUserId, nameof(createdByUserId));
        return _accessService.QuerySingleAsync<decimal, object>("dbo.spCustomerPointEarn", new { CustomerId = customerId, SaleAmount = saleAmount, ReferenceType = CustomerValidation.TrimOrNull(referenceType), ReferenceId = referenceId, ReferenceNo = CustomerValidation.TrimOrNull(referenceNo), ExpiryDate = expiryDate?.Date, CreatedByUserId = createdByUserId }, cancellationToken);
    }

    public Task RedeemPointsAsync(int customerId, decimal points, string? referenceType, long? referenceId, string? referenceNo, string? remark, int createdByUserId, CancellationToken cancellationToken = default)
    {
        ValidatePointChange(customerId, points, createdByUserId);
        return _accessService.ExecuteAsync("dbo.spCustomerPointRedeem", new { CustomerId = customerId, Points = points, ReferenceType = CustomerValidation.TrimOrNull(referenceType), ReferenceId = referenceId, ReferenceNo = CustomerValidation.TrimOrNull(referenceNo), Remark = CustomerValidation.TrimOrNull(remark), CreatedByUserId = createdByUserId }, cancellationToken);
    }

    public Task AdjustPointsAsync(LoyaltyPointAdjustModel model, CancellationToken cancellationToken = default)
    {
        ValidatePointChange(model.CustomerId, model.Points, model.CreatedByUserId);
        if (model.AdjustmentType is not ("AdjustIn" or "AdjustOut")) throw new ArgumentException("Adjustment type must be AdjustIn or AdjustOut.", nameof(model.AdjustmentType));
        return _accessService.ExecuteAsync("dbo.spCustomerPointAdjust", new { model.CustomerId, model.AdjustmentType, model.Points, ReferenceType = CustomerValidation.TrimOrNull(model.ReferenceType), model.ReferenceId, ReferenceNo = CustomerValidation.TrimOrNull(model.ReferenceNo), Remark = CustomerValidation.TrimOrNull(model.Remark), model.CreatedByUserId }, cancellationToken);
    }

    public Task ReverseByReferenceAsync(string referenceType, long referenceId, string? remark, int createdByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequireText(referenceType, nameof(referenceType));
        CustomerValidation.RequirePositive(referenceId, nameof(referenceId));
        CustomerValidation.RequirePositive(createdByUserId, nameof(createdByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerPointReverseByReference", new { ReferenceType = referenceType.Trim(), ReferenceId = referenceId, Remark = CustomerValidation.TrimOrNull(remark), CreatedByUserId = createdByUserId }, cancellationToken);
    }

    public Task<int> ExpirePointsAsync(DateTime expiryDate, int createdByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(createdByUserId, nameof(createdByUserId));
        return _accessService.QuerySingleAsync<int, object>("dbo.spCustomerPointExpire", new { ExpiryDate = expiryDate.Date, CreatedByUserId = createdByUserId }, cancellationToken);
    }

    private static void ValidatePointChange(int customerId, decimal points, int userId)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        if (points <= 0) throw new ArgumentException("Points must be greater than 0.", nameof(points));
        CustomerValidation.RequirePositive(userId, nameof(userId));
    }
}

public sealed class CustomerCreditService : ICustomerCreditService
{
    private readonly IAccessService _accessService;

    public CustomerCreditService(IAccessService accessService) => _accessService = accessService;

    public Task<CustomerCreditModel?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return _accessService.QuerySingleOrDefaultAsync<CustomerCreditModel, object>("dbo.spCustomerCreditGetByCustomerId", new { CustomerId = customerId }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerCreditPosModel>> SearchForPOSAsync(string? searchText, int top = 20, CancellationToken cancellationToken = default)
    {
        if (top <= 0 || top > 100) throw new ArgumentException("Top must be between 1 and 100.", nameof(top));
        var rows = await _accessService.QueryAsync<CustomerCreditPosModel, object>("dbo.spCustomerSearchForPOS", new { SearchText = CustomerValidation.TrimOrNull(searchText), Top = top }, cancellationToken);
        return rows.ToArray();
    }

    public Task<CustomerCreditPosModel?> GetCreditInfoAsync(int customerId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return _accessService.QuerySingleOrDefaultAsync<CustomerCreditPosModel, object>("dbo.spCustomerGetCreditInfo", new { CustomerId = customerId }, cancellationToken);
    }

    public Task SetCreditAsync(CustomerCreditUpdateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.CustomerId, nameof(model.CustomerId));
        CustomerValidation.RequireNonNegative(model.CreditLimit, nameof(model.CreditLimit));
        if (model.CreditTermDays < 0) throw new ArgumentException("Credit term days cannot be negative.", nameof(model.CreditTermDays));
        CustomerValidation.RequireText(model.CreditStatus, nameof(model.CreditStatus));
        CustomerValidation.RequirePositive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerCreditSet", new { model.CustomerId, model.AllowCredit, model.CreditLimit, model.CreditTermDays, model.CreditStatus, model.RequireManagerApproval, model.ApprovedByUserId, Remark = CustomerValidation.TrimOrNull(model.Remark), model.UpdatedByUserId }, cancellationToken);
    }

    public Task<CustomerCreditCheckResultModel> CheckEligibilityAsync(int customerId, decimal saleAmount, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        if (saleAmount <= 0) throw new ArgumentException("Sale amount must be greater than 0.", nameof(saleAmount));
        return _accessService.QuerySingleAsync<CustomerCreditCheckResultModel, object>("dbo.spCustomerCreditCheckEligibility", new { CustomerId = customerId, SaleAmount = saleAmount }, cancellationToken);
    }

    public Task<long> CreateCreditSaleAsync(int customerId, long saleId, decimal amount, string? referenceNo, bool managerApproved, string? remark, int createdByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        CustomerValidation.RequirePositive(saleId, nameof(saleId));
        if (amount <= 0) throw new ArgumentException("Amount must be greater than 0.", nameof(amount));
        CustomerValidation.RequirePositive(createdByUserId, nameof(createdByUserId));
        return _accessService.QuerySingleAsync<long, object>("dbo.spCustomerCreditCreateSale", new { CustomerId = customerId, SaleId = saleId, Amount = amount, ReferenceNo = CustomerValidation.TrimOrNull(referenceNo), ManagerApproved = managerApproved, Remark = CustomerValidation.TrimOrNull(remark), CreatedByUserId = createdByUserId }, cancellationToken);
    }

    public Task ReceivePaymentAsync(CustomerCreditPaymentModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.CustomerId, nameof(model.CustomerId));
        if (model.Amount <= 0) throw new ArgumentException("Amount must be greater than 0.", nameof(model.Amount));
        CustomerValidation.RequirePositive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerCreditReceivePayment", new { model.CustomerId, model.Amount, model.PaidDate, ReferenceNo = CustomerValidation.TrimOrNull(model.ReferenceNo), Remark = CustomerValidation.TrimOrNull(model.Remark), model.CreatedByUserId }, cancellationToken);
    }

    public Task AdjustCreditAsync(CustomerCreditAdjustmentModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.CustomerId, nameof(model.CustomerId));
        if (model.AdjustmentType is not ("AdjustmentIn" or "AdjustmentOut")) throw new ArgumentException("Adjustment type must be AdjustmentIn or AdjustmentOut.", nameof(model.AdjustmentType));
        if (model.Amount <= 0) throw new ArgumentException("Amount must be greater than 0.", nameof(model.Amount));
        CustomerValidation.RequirePositive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerCreditAdjust", new { model.CustomerId, model.AdjustmentType, model.Amount, ReferenceNo = CustomerValidation.TrimOrNull(model.ReferenceNo), Remark = CustomerValidation.TrimOrNull(model.Remark), model.CreatedByUserId }, cancellationToken);
    }

    public async Task<CustomerCreditTransactionPagedResultModel> GetTransactionsPagedAsync(CustomerCreditTransactionPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(request.CustomerId, nameof(request.CustomerId));
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<CustomerCreditTransactionPagedRow, object>("dbo.spCustomerCreditGetTransactionsPaged", new { request.CustomerId, request.PageNumber, request.PageSize, request.DateFrom, request.DateTo, TransactionType = CustomerValidation.TrimOrNull(request.TransactionType), Status = CustomerValidation.TrimOrNull(request.Status) }, cancellationToken)).ToArray();
        return new CustomerCreditTransactionPagedResultModel { Transactions = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public Task<CustomerCreditRepaymentResultModel> CreateRepaymentAsync(CustomerCreditRepaymentCreateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.CustomerId, nameof(model.CustomerId));
        CustomerValidation.RequirePositive(model.PaymentMethodId, nameof(model.PaymentMethodId));
        if (model.PaymentAmount <= 0) throw new ArgumentException("Payment amount must be greater than 0.", nameof(model.PaymentAmount));
        CustomerValidation.RequirePositive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.QuerySingleAsync<CustomerCreditRepaymentResultModel, object>("dbo.spCustomerCreditRepaymentCreate", new { model.CustomerId, model.PaymentMethodId, model.PaymentAmount, ReferenceNo = CustomerValidation.TrimOrNull(model.ReferenceNo), Remark = CustomerValidation.TrimOrNull(model.Remark), model.CreatedByUserId }, cancellationToken);
    }

    public async Task<CustomerCreditRepaymentPagedResultModel> GetRepaymentsPagedAsync(CustomerCreditRepaymentPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<CustomerCreditRepaymentPagedRow, object>("dbo.spCustomerCreditRepaymentGetPaged", new { request.PageNumber, request.PageSize, request.CustomerId, Status = CustomerValidation.TrimOrNull(request.Status), request.DateFrom, request.DateTo }, cancellationToken)).ToArray();
        return new CustomerCreditRepaymentPagedResultModel { Repayments = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public Task<CustomerCreditRepaymentModel?> GetRepaymentByIdAsync(long customerCreditRepaymentId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerCreditRepaymentId, nameof(customerCreditRepaymentId));
        return _accessService.QuerySingleOrDefaultAsync<CustomerCreditRepaymentModel, object>("dbo.spCustomerCreditRepaymentGetById", new { CustomerCreditRepaymentId = customerCreditRepaymentId }, cancellationToken);
    }

    public Task VoidRepaymentAsync(long customerCreditRepaymentId, string reason, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerCreditRepaymentId, nameof(customerCreditRepaymentId));
        CustomerValidation.RequireText(reason, nameof(reason));
        CustomerValidation.RequirePositive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerCreditRepaymentVoid", new { CustomerCreditRepaymentId = customerCreditRepaymentId, Reason = reason.Trim(), UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    public Task<int> UpdateOverdueStatusAsync(CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<int, object>("dbo.spCustomerCreditUpdateOverdueStatus", new { }, cancellationToken);
}

public sealed class CustomerHistoryService : ICustomerHistoryService
{
    private readonly IAccessService _accessService;

    public CustomerHistoryService(IAccessService accessService) => _accessService = accessService;

    public Task<CustomerHistorySummaryModel?> GetSummaryAsync(int customerId, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerId, nameof(customerId));
        return _accessService.QuerySingleOrDefaultAsync<CustomerHistorySummaryModel, object>("dbo.spCustomerHistoryGetSummary", new { CustomerId = customerId, DateFrom = dateFrom, DateTo = dateTo }, cancellationToken);
    }

    public Task<PagedResultModel<CustomerPurchaseHistoryModel>> GetPurchaseHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default) =>
        QueryHistoryAsync<CustomerPurchaseHistoryModel, CustomerPurchaseHistoryPagedRow>("dbo.spCustomerHistoryGetPurchaseHistory", request, cancellationToken);

    public Task<PagedResultModel<CustomerPaymentHistoryModel>> GetPaymentHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default) =>
        QueryHistoryAsync<CustomerPaymentHistoryModel, CustomerPaymentHistoryPagedRow>("dbo.spCustomerHistoryGetPaymentHistory", request, cancellationToken);

    public Task<PagedResultModel<CustomerCreditHistoryModel>> GetCreditHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default) =>
        QueryHistoryAsync<CustomerCreditHistoryModel, CustomerCreditHistoryPagedRow>("dbo.spCustomerHistoryGetCreditHistory", request, cancellationToken);

    public Task<PagedResultModel<CustomerPointHistoryModel>> GetPointHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default) =>
        QueryHistoryAsync<CustomerPointHistoryModel, CustomerPointHistoryPagedRow>("dbo.spCustomerHistoryGetPointHistory", request, cancellationToken);

    public Task<PagedResultModel<CustomerLevelHistoryModel>> GetLevelHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default) =>
        QueryHistoryAsync<CustomerLevelHistoryModel, CustomerLevelHistoryPagedRow>("dbo.spCustomerHistoryGetLevelHistory", request, cancellationToken);

    public Task<PagedResultModel<CustomerRefundHistoryModel>> GetRefundHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default) =>
        QueryHistoryAsync<CustomerRefundHistoryModel, CustomerRefundHistoryPagedRow>("dbo.spCustomerHistoryGetRefundHistory", request, cancellationToken);

    public async Task<PagedResultModel<CustomerTimelineModel>> GetTimelineAsync(CustomerTimelinePagedRequestModel request, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(request.CustomerId, nameof(request.CustomerId));
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<CustomerTimelinePagedRow, object>("dbo.spCustomerHistoryGetTimeline", new { request.CustomerId, request.DateFrom, request.DateTo, HistoryType = CustomerValidation.TrimOrNull(request.HistoryType), request.PageNumber, request.PageSize }, cancellationToken)).ToArray();
        return new PagedResultModel<CustomerTimelineModel> { Items = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public async Task<CustomerNotePagedResultModel> GetNotesPagedAsync(CustomerNotePagedRequestModel request, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(request.CustomerId, nameof(request.CustomerId));
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<CustomerNotePagedRow, object>("dbo.spCustomerNoteGetPaged", new { request.CustomerId, request.PageNumber, request.PageSize, NoteType = CustomerValidation.TrimOrNull(request.NoteType), request.IsActive }, cancellationToken)).ToArray();
        return new CustomerNotePagedResultModel { Notes = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public Task<CustomerNoteModel?> GetNoteByIdAsync(long customerNoteId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerNoteId, nameof(customerNoteId));
        return _accessService.QuerySingleOrDefaultAsync<CustomerNoteModel, object>("dbo.spCustomerNoteGetById", new { CustomerNoteId = customerNoteId }, cancellationToken);
    }

    public Task<long> AddNoteAsync(CustomerNoteCreateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.CustomerId, nameof(model.CustomerId));
        CustomerValidation.RequireText(model.NoteType, nameof(model.NoteType));
        CustomerValidation.RequireText(model.NoteText, nameof(model.NoteText));
        CustomerValidation.RequirePositive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.QuerySingleAsync<long, object>("dbo.spCustomerNoteCreate", new { model.CustomerId, NoteType = model.NoteType.Trim(), NoteText = model.NoteText.Trim(), model.IsImportant, model.CreatedByUserId }, cancellationToken);
    }

    public Task UpdateNoteAsync(CustomerNoteUpdateModel model, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(model.CustomerNoteId, nameof(model.CustomerNoteId));
        CustomerValidation.RequireText(model.NoteType, nameof(model.NoteType));
        CustomerValidation.RequireText(model.NoteText, nameof(model.NoteText));
        CustomerValidation.RequirePositive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerNoteUpdate", new { model.CustomerNoteId, NoteType = model.NoteType.Trim(), NoteText = model.NoteText.Trim(), model.IsImportant, model.IsActive, model.UpdatedByUserId }, cancellationToken);
    }

    public Task ToggleNoteActiveAsync(long customerNoteId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        CustomerValidation.RequirePositive(customerNoteId, nameof(customerNoteId));
        CustomerValidation.RequirePositive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spCustomerNoteToggleActive", new { CustomerNoteId = customerNoteId, IsActive = isActive, UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    private async Task<PagedResultModel<TModel>> QueryHistoryAsync<TModel, TRow>(string storedProcedure, CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken)
        where TRow : TModel, ITotalCountRow
    {
        CustomerValidation.RequirePositive(request.CustomerId, nameof(request.CustomerId));
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<TRow, object>(storedProcedure, new { request.CustomerId, request.DateFrom, request.DateTo, request.PageNumber, request.PageSize }, cancellationToken)).ToArray();
        return new PagedResultModel<TModel> { Items = rows.Cast<TModel>().ToArray(), TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }
}

public sealed class CustomerReportService : ICustomerReportService
{
    private readonly IAccessService _accessService;

    public CustomerReportService(IAccessService accessService) => _accessService = accessService;

    public Task<CustomerReportSummaryModel> GetSummaryAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<CustomerReportSummaryModel, object>("dbo.spCustomerReportGetSummary", new { request.DateFrom, request.DateTo, MemberType = CustomerValidation.TrimOrNull(request.MemberType), request.MemberLevelId, request.IsActive, request.Top, request.NoPurchaseAfterDate }, cancellationToken);

    public async Task<IReadOnlyCollection<TopCustomerModel>> GetTopCustomersBySpendingAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<TopCustomerModel, object>("dbo.spCustomerReportTopCustomersBySpending", ReportParameters(request), cancellationToken)).ToArray();

    public async Task<IReadOnlyCollection<TopCustomerModel>> GetTopCustomersByVisitAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<TopCustomerModel, object>("dbo.spCustomerReportTopCustomersByVisit", ReportParameters(request), cancellationToken)).ToArray();

    public async Task<IReadOnlyCollection<MemberLevelSummaryModel>> GetMemberLevelSummaryAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<MemberLevelSummaryModel, object>("dbo.spCustomerReportMemberLevelSummary", ReportParameters(request), cancellationToken)).ToArray();

    public Task<LoyaltyPointSummaryModel> GetLoyaltyPointSummaryAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<LoyaltyPointSummaryModel, object>("dbo.spCustomerReportLoyaltyPointSummary", ReportParameters(request), cancellationToken);

    public Task<CustomerCreditSummaryModel> GetCreditSummaryAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<CustomerCreditSummaryModel, object>("dbo.spCustomerReportCreditSummary", ReportParameters(request), cancellationToken);

    public async Task<IReadOnlyCollection<InactiveCustomerModel>> GetInactiveCustomersAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default) =>
        (await _accessService.QueryAsync<InactiveCustomerModel, object>("dbo.spCustomerReportInactiveCustomers", ReportParameters(request), cancellationToken)).ToArray();

    private static object ReportParameters(CustomerReportRequestModel request)
    {
        if (request.Top <= 0) throw new ArgumentException("Top must be greater than 0.", nameof(request.Top));
        return new { request.DateFrom, request.DateTo, request.MemberLevelId, request.IsActive, request.Top, request.NoPurchaseAfterDate };
    }
}

public sealed class CustomerAuditService : ICustomerAuditService
{
    private readonly IAccessService _accessService;

    public CustomerAuditService(IAccessService accessService) => _accessService = accessService;

    public async Task<CustomerAuditLogPagedResultModel> GetPagedAsync(CustomerAuditLogPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        CustomerValidation.ValidatePage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<CustomerAuditLogPagedRow, object>("dbo.spCustomerAuditLogGetPaged", new
        {
            request.CustomerId,
            request.PageNumber,
            request.PageSize,
            ActionType = CustomerValidation.TrimOrNull(request.ActionType),
            request.DateFrom,
            request.DateTo
        }, cancellationToken)).ToArray();

        return new CustomerAuditLogPagedResultModel { Logs = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }
}

internal interface ITotalCountRow { int TotalCount { get; } }

internal sealed class CustomerPagedRow : CustomerSummaryModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerPointMovementPagedRow : CustomerPointMovementModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerCreditTransactionPagedRow : CustomerCreditTransactionModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerCreditRepaymentPagedRow : CustomerCreditRepaymentModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerPurchaseHistoryPagedRow : CustomerPurchaseHistoryModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerPaymentHistoryPagedRow : CustomerPaymentHistoryModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerCreditHistoryPagedRow : CustomerCreditHistoryModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerPointHistoryPagedRow : CustomerPointHistoryModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerLevelHistoryPagedRow : CustomerLevelHistoryModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerRefundHistoryPagedRow : CustomerRefundHistoryModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerTimelinePagedRow : CustomerTimelineModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerNotePagedRow : CustomerNoteModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class CustomerAuditLogPagedRow : CustomerAuditLogModel, ITotalCountRow { public int TotalCount { get; init; } }
internal sealed class RubberPurchaseHeaderPagedRow : RubberPurchaseHeaderModel, ITotalCountRow { public int TotalCount { get; init; } }

internal static class CustomerValidation
{
    public static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void RequireText(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{paramName} is required.", paramName);
    }

    public static void RequirePositive(int value, string paramName)
    {
        if (value <= 0) throw new ArgumentException($"{paramName} must be greater than 0.", paramName);
    }

    public static void RequirePositive(long value, string paramName)
    {
        if (value <= 0) throw new ArgumentException($"{paramName} must be greater than 0.", paramName);
    }

    public static void RequireNonNegative(decimal value, string paramName)
    {
        if (value < 0) throw new ArgumentException($"{paramName} cannot be negative.", paramName);
    }

    public static void ValidatePage(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
        if (pageSize <= 0 || pageSize > 500) throw new ArgumentException("Page size must be between 1 and 500.", nameof(pageSize));
    }
}
