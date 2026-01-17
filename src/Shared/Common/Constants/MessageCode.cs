namespace Common.Constants;

public sealed class MessageCode
{
    // General validation constants
    public const string BadRequest = "BAD_REQUEST";

    // Product validation constants
    public const string ProductNameIsRequired = "PRODUCT_NAME_IS_REQUIRED";
    public const string ProductNameMaxLength = "PRODUCT_NAME_MAX_LENGTH";
    public const string ProductNameMinLength = "PRODUCT_NAME_MIN_LENGTH";

    public const string SkuIsRequired = "SKU_IS_REQUIRED";
    public const string SkuMaxLength = "SKU_MAX_LENGTH";
    public const string SkuMinLength = "SKU_MIN_LENGTH";
    public const string SkuAlreadyExists = "SKU_ALREADY_EXISTS";

    public const string ShortDescriptionIsRequired = "SHORT_DESCRIPTION_IS_REQUIRED";
    public const string ShortDescriptionMaxLength = "SHORT_DESCRIPTION_MAX_LENGTH";
    public const string ShortDescriptionMinLength = "SHORT_DESCRIPTION_MIN_LENGTH";

    public const string LongDescriptionIsRequired = "LONG_DESCRIPTION_IS_REQUIRED";
    public const string LongDescriptionMaxLength = "LONG_DESCRIPTION_MAX_LENGTH";
    public const string LongDescriptionMinLength = "LONG_DESCRIPTION_MIN_LENGTH";

    public const string PriceIsRequired = "PRICE_IS_REQUIRED";
    public const string PriceMustBeGreaterThanZero = "PRICE_MUST_BE_GREATER_THAN_ZERO";
    public const string PriceInvalidRange = "PRICE_INVALID_RANGE";

    public const string SalePriceMustBeGreaterThanZero = "SALE_PRICE_MUST_BE_GREATER_THAN_ZERO";
    public const string SalePriceInvalidRange = "SALE_PRICE_INVALID_RANGE";
    public const string SalePriceMustBeLessThanPrice = "SALE_PRICE_MUST_BE_LESS_THAN_PRICE";

    // Actor validation
    public const string ActorIsRequired = "ACTOR_IS_REQUIRED";
    public const string ActorInvalid = "ACTOR_INVALID";

    // Product not found
    public const string ProductIsNotFound = "PRODUCT_IS_NOT_FOUND";
    public const string ProductNotFoundById = "PRODUCT_NOT_FOUND_BY_ID";
    public const string ProductNotFoundBySlug = "PRODUCT_NOT_FOUND_BY_SLUG";
}
