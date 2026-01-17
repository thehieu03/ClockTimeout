namespace Common.Constants;

public sealed class MessageCode
{
    public const string BadRequest = "BAD_REQUEST";
    public const string ProductNameIsRequired = "PRODUCT_NAME_IS_REQUIRED";
    public const string ProductNameMaxLength = "PRODUCT_NAME_MAX_LENGTH";
    public const string SkuIsRequired = "SKU_IS_REQUIRED";
    public const string SkuMaxLength = "SKU_MAX_LENGTH";
    public const string ShortDescriptionIsRequired = "SHORT_DESCRIPTION_IS_REQUIRED";
    public const string ShortDescriptionMaxLength = "SHORT_DESCRIPTION_MAX_LENGTH";
    public const string LongDescriptionIsRequired = "LONG_DESCRIPTION_IS_REQUIRED";
    public const string LongDescriptionMaxLength = "LONG_DESCRIPTION_MAX_LENGTH";
    public const string PriceIsRequired = "PRICE_IS_REQUIRED";
    public const string PriceMustBeGreaterThanZero = "PRICE_MUST_BE_GREATER_THAN_ZERO";
    public const string SalePriceMustBeGreaterThanZero = "SALE_PRICE_MUST_BE_GREATER_THAN_ZERO";
    public const string ActorIsRequired = "ACTOR_IS_REQUIRED";

    public const string DecisionFlowIllegal = "DECISION_FLOW_ILLEGAL";
}
