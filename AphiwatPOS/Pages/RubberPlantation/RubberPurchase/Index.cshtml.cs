using System.ComponentModel.DataAnnotations;
using AphiwatPOS.Pages.Customer;
using CustomerEngine.Models;
using CustomerEngine.Services;
using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.RubberPlantation.RubberPurchase;

public sealed class IndexModel : PageModel
{
    private readonly IRubberPurchaseService _rubberPurchaseService;
    private readonly IRubberPriceService _rubberPriceService;
    private readonly IRubberAuctionLocationService _auctionLocationService;
    private readonly ICustomerService _customerService;
    private readonly IInventoryLocationService _locationService;

    public IndexModel(
        IRubberPurchaseService rubberPurchaseService,
        IRubberPriceService rubberPriceService,
        IRubberAuctionLocationService auctionLocationService,
        ICustomerService customerService,
        IInventoryLocationService locationService)
    {
        _rubberPurchaseService = rubberPurchaseService;
        _rubberPriceService = rubberPriceService;
        _auctionLocationService = auctionLocationService;
        _customerService = customerService;
        _locationService = locationService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? RubberAuctionLocationId { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public RubberPurchaseInput Input { get; set; } = new();
    [BindProperty] public RubberPurchaseBatchInput BatchInput { get; set; } = new();

    public RubberPurchaseHeaderPagedModel Purchases { get; private set; } = new();
    public IReadOnlyCollection<CustomerSummaryModel> RubberSuppliers { get; private set; } = Array.Empty<CustomerSummaryModel>();
    public IReadOnlyCollection<InventoryLocationModel> Locations { get; private set; } = Array.Empty<InventoryLocationModel>();
    public IReadOnlyCollection<RubberAuctionLocationModel> AuctionLocations { get; private set; } = Array.Empty<RubberAuctionLocationModel>();
    public IReadOnlyCollection<RubberPriceModel> RubberPrices { get; private set; } = Array.Empty<RubberPriceModel>();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool OpenBatchModal { get; private set; }

    public decimal TotalWeightKg => Purchases.Items.Sum(x => x.WeightKg);
    public decimal TotalAmount => Purchases.Items.Sum(x => x.TotalAmount ?? 0);
    public int TotalPointsEarned => Purchases.Items.Sum(x => x.PointsEarned);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        RemoveModelStatePrefix(nameof(BatchInput));
        Input.PaymentStatus = "Pending";
        Input.SellerType = Input.SellerType == "NonMember" ? "NonMember" : "Member";

        if (Input.SellerType == "NonMember")
        {
            ModelState.Remove("Input.CustomerId");
            Input.CustomerId = null;
            if (string.IsNullOrWhiteSpace(Input.NonMemberFarmerName))
            {
                ModelState.AddModelError("Input.NonMemberFarmerName", "Farmer name is required.");
            }
        }
        else
        {
            ModelState.Remove("Input.NonMemberFarmerName");
            ModelState.Remove("Input.NonMemberFarmerPhone");
            Input.NonMemberFarmerName = null;
            Input.NonMemberFarmerPhone = null;
            if (Input.CustomerId.GetValueOrDefault() <= 0)
            {
                ModelState.AddModelError("Input.CustomerId", "Please select a rubber supplier member.");
            }
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please review the rubber purchase form.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        var selectedPrice = await _rubberPriceService.GetByIdAsync(Input.RubberPriceId, cancellationToken);
        if (selectedPrice is null || !selectedPrice.IsActive)
        {
            ModelState.AddModelError("Input.RubberPriceId", "Please select an active rubber price.");
            ErrorMessage = "Please select an active rubber price.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        if (!PriceMatchesAuctionLocation(selectedPrice, Input.RubberAuctionLocationId))
        {
            ModelState.AddModelError("Input.RubberPriceId", "Please select a market price for the selected auction location.");
            ErrorMessage = "Please select a market price for the selected auction location.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        var grossAmount = Input.WeightKg * selectedPrice.PricePerKg;
        var deductionAmount = grossAmount * (selectedPrice.PercentageOfService / 100m);
        var totalAmount = Math.Max(0, grossAmount - deductionAmount);

        await _rubberPurchaseService.CreateAsync(new RubberPurchaseHeaderCreateModel
        {
            CustomerId = Input.CustomerId,
            NonMemberFarmerName = Input.NonMemberFarmerName,
            NonMemberFarmerPhone = Input.NonMemberFarmerPhone,
            BusinessLocationId = Input.BusinessLocationId,
            RubberAuctionLocationId = Input.RubberAuctionLocationId,
            TransactionDate = Input.TransactionDate,
            WeightKg = Input.WeightKg,
            RubberPriceId = selectedPrice.RubberPriceId,
            PricePerKgSnapshot = selectedPrice.PricePerKg,
            PercentageSnapshot = selectedPrice.PercentageOfService,
            TotalAmount = totalAmount,
            PaymentStatus = Input.PaymentStatus,
            CreatedByUserId = CustomerPageHelpers.CurrentUserId(User)
        }, cancellationToken);

        TempData["StatusMessage"] = "Rubber purchase recorded successfully.";
        return RedirectToPage(new { SearchText, RubberAuctionLocationId, DateFrom, DateTo, PageNumber });
    }

    public async Task<IActionResult> OnPostCreateBatchAsync(CancellationToken cancellationToken)
    {
        BatchInput.PaymentStatus = "Pending";
        BatchInput.Rows ??= new List<RubberPurchaseBatchRowInput>();
        ModelState.Clear();

        var activeRows = BatchInput.Rows
            .Select((row, index) => new { Row = row, Index = index })
            .Where(x => x.Row.WeightKg > 0 || x.Row.CustomerId.GetValueOrDefault() > 0 || !string.IsNullOrWhiteSpace(x.Row.NonMemberFarmerName))
            .ToArray();

        if (BatchInput.BusinessLocationId <= 0)
        {
            ModelState.AddModelError("BatchInput.BusinessLocationId", "Business location is required.");
        }

        if (BatchInput.RubberAuctionLocationId <= 0)
        {
            ModelState.AddModelError("BatchInput.RubberAuctionLocationId", "Auction location is required.");
        }

        if (BatchInput.RubberPriceId <= 0)
        {
            ModelState.AddModelError("BatchInput.RubberPriceId", "Market price is required.");
        }

        if (!activeRows.Any())
        {
            ModelState.AddModelError("BatchInput.Rows", "Add at least one purchase row.");
        }

        foreach (var item in activeRows)
        {
            item.Row.SellerType = item.Row.SellerType == "NonMember" ? "NonMember" : "Member";
            if (item.Row.WeightKg <= 0)
            {
                ModelState.AddModelError($"BatchInput.Rows[{item.Index}].WeightKg", "Weight is required.");
            }

            if (item.Row.SellerType == "NonMember")
            {
                item.Row.CustomerId = null;
                if (string.IsNullOrWhiteSpace(item.Row.NonMemberFarmerName))
                {
                    ModelState.AddModelError($"BatchInput.Rows[{item.Index}].NonMemberFarmerName", "Farmer name is required.");
                }
            }
            else
            {
                item.Row.NonMemberFarmerName = null;
                item.Row.NonMemberFarmerPhone = null;
                if (item.Row.CustomerId.GetValueOrDefault() <= 0)
                {
                    ModelState.AddModelError($"BatchInput.Rows[{item.Index}].CustomerId", "Supplier member is required.");
                }
            }
        }

        var selectedPrice = BatchInput.RubberPriceId > 0
            ? await _rubberPriceService.GetByIdAsync(BatchInput.RubberPriceId, cancellationToken)
            : null;
        if (selectedPrice is null || !selectedPrice.IsActive)
        {
            ModelState.AddModelError("BatchInput.RubberPriceId", "Please select an active rubber price.");
        }
        else if (!PriceMatchesAuctionLocation(selectedPrice, BatchInput.RubberAuctionLocationId))
        {
            ModelState.AddModelError("BatchInput.RubberPriceId", "Please select a market price for the selected auction location.");
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please review the batch purchase rows.";
            OpenBatchModal = true;
            EnsureBatchRows();
            await LoadAsync(cancellationToken);
            return Page();
        }

        var createdByUserId = CustomerPageHelpers.CurrentUserId(User);
        var purchases = activeRows.Select(item =>
        {
            var grossAmount = item.Row.WeightKg * selectedPrice!.PricePerKg;
            var deductionAmount = grossAmount * (selectedPrice.PercentageOfService / 100m);
            return new RubberPurchaseHeaderCreateModel
            {
                CustomerId = item.Row.CustomerId,
                NonMemberFarmerName = item.Row.NonMemberFarmerName,
                NonMemberFarmerPhone = item.Row.NonMemberFarmerPhone,
                BusinessLocationId = BatchInput.BusinessLocationId,
                RubberAuctionLocationId = BatchInput.RubberAuctionLocationId,
                TransactionDate = BatchInput.TransactionDate,
                WeightKg = item.Row.WeightKg,
                RubberPriceId = selectedPrice.RubberPriceId,
                PricePerKgSnapshot = selectedPrice.PricePerKg,
                PercentageSnapshot = selectedPrice.PercentageOfService,
                TotalAmount = Math.Max(0, grossAmount - deductionAmount),
                PaymentStatus = BatchInput.PaymentStatus,
                CreatedByUserId = createdByUserId
            };
        }).ToArray();

        await _rubberPurchaseService.CreateBatchAsync(new RubberPurchaseBatchCreateModel { Purchases = purchases }, cancellationToken);

        TempData["StatusMessage"] = $"{purchases.Length} rubber purchases recorded successfully.";
        return RedirectToPage(new { SearchText, RubberAuctionLocationId, DateFrom, DateTo, PageNumber });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Locations = await _locationService.GetAllActiveAsync(cancellationToken);
        AuctionLocations = await _auctionLocationService.GetActiveAsync(cancellationToken);
        RubberPrices = await _rubberPriceService.GetActiveAsync(cancellationToken);
        if (Input.BusinessLocationId <= 0)
        {
            Input.BusinessLocationId = Locations.FirstOrDefault()?.LocationId ?? 0;
        }
        if (BatchInput.BusinessLocationId <= 0)
        {
            BatchInput.BusinessLocationId = Input.BusinessLocationId;
        }
        EnsureBatchRows();

        RubberSuppliers = (await _customerService.GetPagedAsync(new CustomerPagedRequestModel
        {
            PageNumber = 1,
            PageSize = 200,
            MemberType = MemberTypeCodes.RubberSupplier,
            IsActive = true
        }, cancellationToken)).Customers;

        Purchases = await _rubberPurchaseService.GetPagedAsync(new RubberPurchaseHeaderPagedRequestModel
        {
            PageNumber = PageNumber,
            PageSize = 20,
            RubberAuctionLocationId = RubberAuctionLocationId,
            DateFrom = DateFrom,
            DateTo = DateTo,
            SearchText = SearchText
        }, cancellationToken);
    }

    public sealed class RubberPurchaseInput
    {
        public string SellerType { get; set; } = "Member";
        [Range(1, int.MaxValue)] public int? CustomerId { get; set; }
        [StringLength(255)] public string? NonMemberFarmerName { get; set; }
        [StringLength(50)] public string? NonMemberFarmerPhone { get; set; }
        [Range(1, int.MaxValue)] public int BusinessLocationId { get; set; }
        [Range(1, int.MaxValue)] public int RubberAuctionLocationId { get; set; }
        [Range(1, int.MaxValue)] public int RubberPriceId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Today;
        [Range(0.01, 99999999)] public decimal WeightKg { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
    }

    public sealed class RubberPurchaseBatchInput
    {
        public int BusinessLocationId { get; set; }
        public int RubberAuctionLocationId { get; set; }
        public int RubberPriceId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Today;
        public string PaymentStatus { get; set; } = "Pending";
        public List<RubberPurchaseBatchRowInput> Rows { get; set; } = new();
    }

    public sealed class RubberPurchaseBatchRowInput
    {
        public string SellerType { get; set; } = "Member";
        public int? CustomerId { get; set; }
        public string? NonMemberFarmerName { get; set; }
        public string? NonMemberFarmerPhone { get; set; }
        public decimal WeightKg { get; set; }
    }

    private void EnsureBatchRows()
    {
        while (BatchInput.Rows.Count < 5)
        {
            BatchInput.Rows.Add(new RubberPurchaseBatchRowInput());
        }
    }

    private static bool PriceMatchesAuctionLocation(RubberPriceModel selectedPrice, int rubberAuctionLocationId)
    {
        return selectedPrice.RubberAuctionLocationId is null
            || selectedPrice.RubberAuctionLocationId == rubberAuctionLocationId;
    }

    private void RemoveModelStatePrefix(string prefix)
    {
        foreach (var key in ModelState.Keys.Where(key => key == prefix || key.StartsWith(prefix + ".", StringComparison.Ordinal)).ToArray())
        {
            ModelState.Remove(key);
        }
    }
}
