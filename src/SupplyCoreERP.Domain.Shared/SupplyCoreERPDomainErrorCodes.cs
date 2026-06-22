namespace SupplyCoreERP;

public static class SupplyCoreERPDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    #region Inventory - Zone
    public const string InvalidZoneForTicketType = "SupplyCoreERP:InvalidZoneForTicketType";
    public const string ZoneNotAllowedForInventory = "SupplyCoreERP:ZoneNotAllowedForInventory";
    public const string InvalidZoneTransferDirection = "SupplyCoreERP:InvalidZoneTransferDirection";
    public const string InsufficientAvailableQuantityForTransfer = "SupplyCoreERP:InsufficientAvailableQuantityForTransfer";
    #endregion
}
